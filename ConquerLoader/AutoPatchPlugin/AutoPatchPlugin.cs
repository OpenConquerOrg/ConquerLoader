using CLCore;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;

namespace AutoPatchPlugin
{
    public class AutoPatchPlugin : IPlugin, IPreLaunchPlugin
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public string Name
        {
            get { return "AutoPatch Plugin"; }
        }

        public string Explanation
        {
            get { return "Applies archive-based autopatches (.zip/.rar) to the client folder before launch."; }
        }

        public PluginType PluginType
        {
            get { return PluginType.FREE; }
        }

        public void Init()
        {
        }

        public void Configure()
        {
            AutoPatchSettings settings = AutoPatchSettingsStore.Load();
            AutoPatchConfigurationWindow window = new AutoPatchConfigurationWindow(settings);

            Window owner = Application.Current != null
                ? Application.Current.Windows.OfType<Window>().FirstOrDefault(currentWindow => currentWindow.IsActive)
                : null;

            if (owner != null && owner != window)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            bool? saved = window.ShowDialog();
            if (saved == true && window.UpdatedSettings != null)
            {
                AutoPatchSettingsStore.Save(window.UpdatedSettings);
                MessageBox.Show("AutoPatch settings saved.", Name, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public PluginPreLaunchResult BeforeLaunch(PluginPreLaunchContext context)
        {
            AutoPatchSettings settings = AutoPatchSettingsStore.Load();
            if (context == null || context.Server == null || !settings.Enabled)
            {
                return PluginPreLaunchResult.Success();
            }

            if (string.IsNullOrWhiteSpace(settings.ManifestLocation))
            {
                return settings.FailLaunchOnError
                    ? PluginPreLaunchResult.Fail("AutoPatch is enabled but no manifest location is configured.")
                    : PluginPreLaunchResult.Success();
            }

            try
            {
                AutoPatchManifest manifest = LoadManifest(settings.ManifestLocation);
                if (manifest == null)
                {
                    throw new InvalidOperationException("The autopatch manifest is empty or invalid.");
                }

                if (manifest.UsesLegacyFileEntries && manifest.GetPackages().Count == 0)
                {
                    throw new InvalidOperationException(
                        "This AutoPatch manifest still uses legacy file entries. Use the 'packages' or 'archives' list with .zip/.rar patches.");
                }

                AutoPatchManifestPackage[] patchPackages = manifest.GetPackages()
                    .Where(package => package != null && package.Enabled && !string.IsNullOrWhiteSpace(GetPackageSource(package)))
                    .ToArray();

                if (patchPackages.Length == 0)
                {
                    context.Log?.Invoke("AutoPatch manifest does not contain patch archives.");
                    return PluginPreLaunchResult.Success();
                }

                string targetDirectory = ResolveTargetDirectory(context.WorkingDirectory, settings.RelativeTargetFolder);
                Directory.CreateDirectory(targetDirectory);

                AutoPatchState state = AutoPatchStateStore.Load();
                bool stateChanged = false;

                for (int index = 0; index < patchPackages.Length; index++)
                {
                    AutoPatchManifestPackage package = patchPackages[index];
                    stateChanged |= ApplyPatchPackage(package, manifest, settings.ManifestLocation, targetDirectory, state, context);

                    if (context.ReportProgress != null)
                    {
                        int progress = 1 + (int)Math.Round(((index + 1d) / Math.Max(1, patchPackages.Length)) * 7d);
                        context.ReportProgress(progress);
                    }
                }

                if (stateChanged)
                {
                    AutoPatchStateStore.Save(state);
                }

                context.Log?.Invoke("AutoPatch completed successfully for " + context.Server.ServerName + ".");
                return PluginPreLaunchResult.Success();
            }
            catch (Exception ex)
            {
                context.Log?.Invoke("AutoPatch failed: " + ex);
                return settings.FailLaunchOnError
                    ? PluginPreLaunchResult.Fail("AutoPatch failed: " + ex.Message)
                    : PluginPreLaunchResult.Success();
            }
        }

        private static AutoPatchManifest LoadManifest(string manifestLocation)
        {
            string resolvedManifestLocation = ResolveSourcePath(manifestLocation, null);
            string json = ReadText(resolvedManifestLocation);
            return JsonConvert.DeserializeObject<AutoPatchManifest>(json);
        }

        private static bool ApplyPatchPackage(
            AutoPatchManifestPackage package,
            AutoPatchManifest manifest,
            string manifestLocation,
            string targetDirectory,
            AutoPatchState state,
            PluginPreLaunchContext context)
        {
            string stateKey = BuildPackageStateKey(targetDirectory, package);
            string desiredFingerprint = BuildPackageFingerprint(manifest, package);
            string currentFingerprint;

            if (state.AppliedPackages.TryGetValue(stateKey, out currentFingerprint) &&
                string.Equals(currentFingerprint, desiredFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                context.Log?.Invoke("AutoPatch skipped " + GetPackageDisplayName(package) + " (already applied).");
                return false;
            }

            string sourceBase = string.IsNullOrWhiteSpace(manifest.BaseUrl)
                ? manifestLocation
                : ResolveSourcePath(manifest.BaseUrl, manifestLocation);
            string source = ResolveSourcePath(GetPackageSource(package), sourceBase);
            string archiveFormat = ResolveArchiveFormat(package, source);
            string archiveExtension = ResolveArchiveExtension(archiveFormat);
            string tempArchivePath = MaterializeSourceToTempFile(source, archiveExtension);

            try
            {
                FileInfo archiveFile = new FileInfo(tempArchivePath);
                if (package.Size.HasValue && archiveFile.Length != package.Size.Value)
                {
                    throw new InvalidOperationException("Size mismatch for patch package " + GetPackageDisplayName(package) + ".");
                }

                if (!string.IsNullOrWhiteSpace(package.Sha256))
                {
                    string archiveHash = ComputeSha256(tempArchivePath);
                    if (!HashesMatch(archiveHash, package.Sha256))
                    {
                        throw new InvalidOperationException("Hash mismatch for patch package " + GetPackageDisplayName(package) + ".");
                    }
                }

                string extractDirectory = ResolveDestinationPath(targetDirectory, package.ExtractTo ?? string.Empty);
                Directory.CreateDirectory(extractDirectory);

                context.Log?.Invoke("AutoPatch applying " + GetPackageDisplayName(package) + " from " + source + ".");
                ExtractArchive(tempArchivePath, archiveFormat, extractDirectory);

                state.AppliedPackages[stateKey] = desiredFingerprint;
                context.Log?.Invoke("AutoPatch applied " + GetPackageDisplayName(package) + ".");
                return true;
            }
            finally
            {
                DeleteFileSafe(tempArchivePath);
            }
        }

        private static string GetPackageSource(AutoPatchManifestPackage package)
        {
            return !string.IsNullOrWhiteSpace(package.Url)
                ? package.Url
                : package.Archive;
        }

        private static string GetPackageDisplayName(AutoPatchManifestPackage package)
        {
            if (!string.IsNullOrWhiteSpace(package.Id))
            {
                return package.Id;
            }

            if (!string.IsNullOrWhiteSpace(package.Archive))
            {
                return package.Archive;
            }

            if (!string.IsNullOrWhiteSpace(package.Url))
            {
                return package.Url;
            }

            return "patch package";
        }

        private static string BuildPackageStateKey(string targetDirectory, AutoPatchManifestPackage package)
        {
            string normalizedTarget = Path.GetFullPath(targetDirectory).TrimEnd(Path.DirectorySeparatorChar);
            string packageKey = GetPackageDisplayName(package);
            return normalizedTarget + "|" + packageKey;
        }

        private static string BuildPackageFingerprint(AutoPatchManifest manifest, AutoPatchManifestPackage package)
        {
            return string.Join("|", new[]
            {
                manifest.Version ?? string.Empty,
                package.Id ?? string.Empty,
                package.Archive ?? string.Empty,
                package.Url ?? string.Empty,
                package.Format ?? string.Empty,
                package.ExtractTo ?? string.Empty,
                package.Size.HasValue ? package.Size.Value.ToString() : string.Empty,
                package.Sha256 ?? string.Empty
            });
        }

        private static string ResolveArchiveFormat(AutoPatchManifestPackage package, string source)
        {
            if (!string.IsNullOrWhiteSpace(package.Format))
            {
                string normalizedFormat = package.Format.Trim().ToLowerInvariant();
                if (normalizedFormat == "zip" || normalizedFormat == "rar")
                {
                    return normalizedFormat;
                }

                throw new InvalidOperationException("Unsupported patch format '" + package.Format + "'. Use zip or rar.");
            }

            string extension = GetSourceExtension(source);
            if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                return "zip";
            }

            if (string.Equals(extension, ".rar", StringComparison.OrdinalIgnoreCase))
            {
                return "rar";
            }

            throw new InvalidOperationException(
                "Unable to infer the archive format for " + GetPackageDisplayName(package) + ". Set the 'format' field to zip or rar.");
        }

        private static string ResolveArchiveExtension(string archiveFormat)
        {
            return archiveFormat == "rar" ? ".rar" : ".zip";
        }

        private static string GetSourceExtension(string source)
        {
            Uri uri;
            if (Uri.TryCreate(source, UriKind.Absolute, out uri))
            {
                return Path.GetExtension(uri.IsFile ? uri.LocalPath : uri.AbsolutePath);
            }

            return Path.GetExtension(source);
        }

        private static string MaterializeSourceToTempFile(string source, string extension)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "ConquerLoader", "AutoPatch");
            Directory.CreateDirectory(tempDirectory);

            string tempFilePath = Path.Combine(tempDirectory, Guid.NewGuid().ToString("N") + extension);
            if (IsHttpSource(source))
            {
                using (HttpResponseMessage response = HttpClient.GetAsync(source).Result)
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream input = response.Content.ReadAsStreamAsync().Result)
                    using (FileStream output = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        input.CopyTo(output);
                    }
                }
            }
            else
            {
                File.Copy(source, tempFilePath, true);
            }

            return tempFilePath;
        }

        private static void ExtractArchive(string archivePath, string archiveFormat, string extractDirectory)
        {
            switch (archiveFormat)
            {
                case "zip":
                    ExtractZipArchive(archivePath, extractDirectory);
                    break;
                case "rar":
                    ExtractRarArchive(archivePath, extractDirectory);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported patch format '" + archiveFormat + "'.");
            }
        }

        private static void ExtractZipArchive(string archivePath, string extractDirectory)
        {
            using (FileStream archiveStream = File.OpenRead(archivePath))
            using (ZipArchive zipArchive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string normalizedEntryPath = (entry.FullName ?? string.Empty).Replace('/', Path.DirectorySeparatorChar);
                    if (string.IsNullOrWhiteSpace(normalizedEntryPath))
                    {
                        continue;
                    }

                    bool isDirectory = normalizedEntryPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
                    string destinationPath = ResolveDestinationPath(extractDirectory, normalizedEntryPath);

                    if (isDirectory)
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    string destinationFolder = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrWhiteSpace(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    using (Stream entryStream = entry.Open())
                    using (FileStream outputStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        entryStream.CopyTo(outputStream);
                    }

                    if (entry.LastWriteTime != default(DateTimeOffset))
                    {
                        File.SetLastWriteTime(destinationPath, entry.LastWriteTime.LocalDateTime);
                    }
                }
            }
        }

        private static void ExtractRarArchive(string archivePath, string extractDirectory)
        {
            string extractorPath = FindRarExtractor();
            if (string.IsNullOrWhiteSpace(extractorPath))
            {
                throw new InvalidOperationException(
                    "RAR extraction requires UnRAR.exe or WinRAR.exe to be installed on this machine.");
            }

            string destinationPath = extractDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = extractorPath,
                Arguments = string.Format("x -o+ -y \"{0}\" \"{1}\"", archivePath, destinationPath),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = extractDirectory
            };

            using (Process process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Unable to start the RAR extractor process.");
                }

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException("RAR extraction failed with exit code " + process.ExitCode + ".");
                }
            }
        }

        private static string FindRarExtractor()
        {
            string[] candidates =
            {
                "unrar.exe",
                "rar.exe",
                @"C:\Program Files\WinRAR\UnRAR.exe",
                @"C:\Program Files\WinRAR\WinRAR.exe",
                @"C:\Program Files (x86)\WinRAR\UnRAR.exe",
                @"C:\Program Files (x86)\WinRAR\WinRAR.exe"
            };

            foreach (string candidate in candidates)
            {
                if (Path.IsPathRooted(candidate))
                {
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }

                    continue;
                }

                string resolvedPath = FindExecutableInPath(candidate);
                if (!string.IsNullOrWhiteSpace(resolvedPath))
                {
                    return resolvedPath;
                }
            }

            return null;
        }

        private static string FindExecutableInPath(string executableName)
        {
            string pathEnvironment = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] directories = pathEnvironment.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string directory in directories)
            {
                try
                {
                    string fullPath = Path.Combine(directory.Trim(), executableName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static string ResolveTargetDirectory(string workingDirectory, string relativeTargetFolder)
        {
            string baseDirectory = string.IsNullOrWhiteSpace(workingDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : workingDirectory;

            if (string.IsNullOrWhiteSpace(relativeTargetFolder))
            {
                return Path.GetFullPath(baseDirectory);
            }

            return Path.GetFullPath(Path.Combine(baseDirectory, relativeTargetFolder));
        }

        private static string ResolveDestinationPath(string rootDirectory, string relativePath)
        {
            string normalizedRoot = Path.GetFullPath(rootDirectory);
            string safeRelativePath = (relativePath ?? string.Empty)
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            string fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, safeRelativePath));
            string expectedPrefix = normalizedRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(fullPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("AutoPatch blocked an unsafe target path: " + relativePath);
            }

            return fullPath;
        }

        private static string ResolveSourcePath(string source, string baseSource)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            Uri absoluteUri;
            if (Uri.TryCreate(source, UriKind.Absolute, out absoluteUri))
            {
                if (absoluteUri.IsFile)
                {
                    return absoluteUri.LocalPath;
                }

                if (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps)
                {
                    return absoluteUri.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(baseSource))
            {
                return Path.IsPathRooted(source)
                    ? Path.GetFullPath(source)
                    : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, source));
            }

            Uri baseUri;
            if (Uri.TryCreate(baseSource, UriKind.Absolute, out baseUri) &&
                (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps))
            {
                return new Uri(baseUri, source).ToString();
            }

            string basePath = baseSource;
            if (File.Exists(baseSource))
            {
                basePath = Path.GetDirectoryName(baseSource);
            }

            return Path.IsPathRooted(source)
                ? Path.GetFullPath(source)
                : Path.GetFullPath(Path.Combine(basePath, source));
        }

        private static string ReadText(string source)
        {
            if (IsHttpSource(source))
            {
                return HttpClient.GetStringAsync(source).Result;
            }

            return File.ReadAllText(source);
        }

        private static bool IsHttpSource(string source)
        {
            Uri uri;
            return Uri.TryCreate(source, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static string ComputeSha256(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
            {
                return ConvertHashToHex(sha256.ComputeHash(stream));
            }
        }

        private static string ConvertHashToHex(byte[] hashBytes)
        {
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        private static bool HashesMatch(string computedHexHash, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash))
            {
                return true;
            }

            string normalizedExpected = expectedHash.Trim();
            if (string.Equals(computedHexHash, normalizedExpected, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                byte[] expectedBytes = Convert.FromBase64String(normalizedExpected);
                string expectedHexHash = ConvertHashToHex(expectedBytes);
                return string.Equals(computedHexHash, expectedHexHash, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static void DeleteFileSafe(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
            }
        }
    }
}

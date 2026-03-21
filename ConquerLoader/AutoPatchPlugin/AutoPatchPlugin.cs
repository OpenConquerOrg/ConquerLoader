using CLCore;
using Newtonsoft.Json;
using System;
using System.IO;
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
            get { return "Applies a manifest-driven autopatch to the client folder before launch."; }
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
                if (manifest == null || manifest.Files == null || manifest.Files.Count == 0)
                {
                    context.Log?.Invoke("AutoPatch manifest does not contain patch files.");
                    return PluginPreLaunchResult.Success();
                }

                string targetDirectory = ResolveTargetDirectory(context.WorkingDirectory, settings.RelativeTargetFolder);
                Directory.CreateDirectory(targetDirectory);

                AutoPatchManifestFile[] patchFiles = manifest.Files
                    .Where(file => !string.IsNullOrWhiteSpace(file.Path))
                    .ToArray();

                for (int index = 0; index < patchFiles.Length; index++)
                {
                    AutoPatchManifestFile file = patchFiles[index];
                    ApplyPatchFile(file, manifest, settings.ManifestLocation, targetDirectory, context);

                    if (context.ReportProgress != null)
                    {
                        int progress = 1 + (int)Math.Round(((index + 1d) / Math.Max(1, patchFiles.Length)) * 7d);
                        context.ReportProgress(progress);
                    }
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

        private static void ApplyPatchFile(AutoPatchManifestFile file, AutoPatchManifest manifest, string manifestLocation, string targetDirectory, PluginPreLaunchContext context)
        {
            string destinationPath = ResolveDestinationPath(targetDirectory, file.Path);
            bool requiresUpdate = !File.Exists(destinationPath);

            if (!requiresUpdate && file.Size.HasValue)
            {
                FileInfo localFile = new FileInfo(destinationPath);
                requiresUpdate = localFile.Length != file.Size.Value;
            }

            if (!requiresUpdate && !string.IsNullOrWhiteSpace(file.Sha256))
            {
                requiresUpdate = !HashesMatch(ComputeSha256(destinationPath), file.Sha256);
            }

            if (!requiresUpdate)
            {
                context.Log?.Invoke("AutoPatch skipped " + file.Path + " (already up to date).");
                return;
            }

            string sourceBase = string.IsNullOrWhiteSpace(manifest.BaseUrl)
                ? manifestLocation
                : ResolveSourcePath(manifest.BaseUrl, manifestLocation);
            string source = ResolveSourcePath(string.IsNullOrWhiteSpace(file.Url) ? file.Path : file.Url, sourceBase);

            context.Log?.Invoke("AutoPatch downloading " + file.Path + " from " + source + ".");
            byte[] content = ReadBytes(source);

            if (!string.IsNullOrWhiteSpace(file.Sha256))
            {
                string contentHash = ComputeSha256(content);
                if (!HashesMatch(contentHash, file.Sha256))
                {
                    throw new InvalidOperationException("Hash mismatch for patched file " + file.Path + ".");
                }
            }

            string destinationFolder = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            File.WriteAllBytes(destinationPath, content);
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

        private static byte[] ReadBytes(string source)
        {
            if (IsHttpSource(source))
            {
                return HttpClient.GetByteArrayAsync(source).Result;
            }

            return File.ReadAllBytes(source);
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

        private static string ComputeSha256(byte[] content)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return ConvertHashToHex(sha256.ComputeHash(content));
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
    }
}

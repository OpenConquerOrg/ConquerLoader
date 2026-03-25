using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CLCore
{
    public sealed class PluginSettingsFile
    {
        public List<PluginSettingsEntry> Plugins { get; set; }

        public PluginSettingsFile()
        {
            Plugins = new List<PluginSettingsEntry>();
        }
    }

    public sealed class PluginSettingsEntry
    {
        public string Name { get; set; }
        public PluginSource Source { get; set; }
        public string AssemblyPath { get; set; }
        public string RemoteName { get; set; }
        public bool Enabled { get; set; }
    }

    public static class PluginSettingsStore
    {
        private static string SettingsFilePath
        {
            get { return Path.Combine(Constants.PluginsFolderName, "plugin-state.json"); }
        }

        public static PluginSettingsFile Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new PluginSettingsFile();
                }

                PluginSettingsFile settings = JsonConvert.DeserializeObject<PluginSettingsFile>(File.ReadAllText(SettingsFilePath));
                return settings ?? new PluginSettingsFile();
            }
            catch
            {
                return new PluginSettingsFile();
            }
        }

        public static void Save(PluginSettingsFile settings)
        {
            if (settings == null)
            {
                settings = new PluginSettingsFile();
            }

            settings.Plugins = settings.Plugins
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Name))
                .GroupBy(entry => BuildIdentity(entry.Name, entry.Source, entry.AssemblyPath, entry.RemoteName), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            Directory.CreateDirectory(Constants.PluginsFolderName);
            File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static string NormalizeRelativeAssemblyPath(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                return null;
            }

            string fullPluginsDirectory = Path.GetFullPath(Constants.PluginsFolderName).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullAssemblyPath = Path.GetFullPath(assemblyPath);
            if (fullAssemblyPath.StartsWith(fullPluginsDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return fullAssemblyPath.Substring(fullPluginsDirectory.Length);
            }

            return fullAssemblyPath;
        }

        public static PluginSettingsEntry FindEntry(PluginSettingsFile settings, string pluginName, PluginSource source, string assemblyPath, string remoteName)
        {
            if (settings == null)
            {
                return null;
            }

            string normalizedAssemblyPath = NormalizeRelativeAssemblyPath(assemblyPath);
            return settings.Plugins.FirstOrDefault(entry =>
                entry != null &&
                string.Equals(entry.Name, pluginName, StringComparison.OrdinalIgnoreCase) &&
                entry.Source == source &&
                (
                    (!string.IsNullOrWhiteSpace(normalizedAssemblyPath) &&
                     string.Equals(entry.AssemblyPath, normalizedAssemblyPath, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(remoteName) &&
                     string.Equals(entry.RemoteName, remoteName, StringComparison.OrdinalIgnoreCase)) ||
                    (string.IsNullOrWhiteSpace(entry.AssemblyPath) && string.IsNullOrWhiteSpace(entry.RemoteName))
                ));
        }

        public static PluginSettingsEntry EnsureEntry(PluginSettingsFile settings, string pluginName, PluginSource source, string assemblyPath, string remoteName, bool enabled)
        {
            if (settings == null)
            {
                settings = new PluginSettingsFile();
            }

            PluginSettingsEntry entry = FindEntry(settings, pluginName, source, assemblyPath, remoteName);
            if (entry == null)
            {
                entry = new PluginSettingsEntry
                {
                    Name = pluginName,
                    Source = source,
                    AssemblyPath = NormalizeRelativeAssemblyPath(assemblyPath),
                    RemoteName = remoteName,
                    Enabled = enabled
                };
                settings.Plugins.Add(entry);
            }
            else
            {
                entry.AssemblyPath = NormalizeRelativeAssemblyPath(assemblyPath) ?? entry.AssemblyPath;
                entry.RemoteName = remoteName ?? entry.RemoteName;
            }

            return entry;
        }

        public static void RemoveEntry(PluginSettingsFile settings, string pluginName, PluginSource source, string assemblyPath, string remoteName)
        {
            PluginSettingsEntry entry = FindEntry(settings, pluginName, source, assemblyPath, remoteName);
            if (entry != null)
            {
                settings.Plugins.Remove(entry);
            }
        }

        public static void CleanupMissingEntries(PluginSettingsFile settings, IEnumerable<PluginCatalogItem> installedPlugins)
        {
            if (settings == null)
            {
                return;
            }

            HashSet<string> validEntries = new HashSet<string>(
                installedPlugins
                    .Where(item => item != null && item.IsInstalled)
                    .Select(item => BuildIdentity(item.Name, item.Source, item.AssemblyPath, item.RemoteName)),
                StringComparer.OrdinalIgnoreCase);

            settings.Plugins = settings.Plugins
                .Where(entry => entry != null && validEntries.Contains(BuildIdentity(entry.Name, entry.Source, entry.AssemblyPath, entry.RemoteName)))
                .ToList();
        }

        private static string BuildIdentity(string pluginName, PluginSource source, string assemblyPath, string remoteName)
        {
            return string.Join("|", new[]
            {
                pluginName ?? string.Empty,
                source.ToString(),
                NormalizeRelativeAssemblyPath(assemblyPath) ?? string.Empty,
                remoteName ?? string.Empty
            });
        }
    }
}

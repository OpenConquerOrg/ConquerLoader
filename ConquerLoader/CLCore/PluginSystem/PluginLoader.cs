using CLCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CLCore
{
    public class PluginLoader
    {
        public static List<IPlugin> Plugins { get; set; } = new List<IPlugin>();
        public static List<PluginCatalogItem> PluginCatalog { get; private set; } = new List<PluginCatalogItem>();

        public void LoadPlugins()
        {
            PluginSettingsFile settings = PluginSettingsStore.Load();
            Plugins = new List<IPlugin>();
            PluginCatalog = new List<PluginCatalogItem>();

            if (Directory.Exists(Constants.PluginsFolderName))
            {
                string[] files = Directory.GetFiles(Constants.PluginsFolderName, "*.dll", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    LoadInstalledPluginAssembly(file, settings);
                }
            }

            PluginCatalog = PluginCatalog
                .OrderBy(item => item.Source == PluginSource.Local ? 0 : 1)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            PluginSettingsStore.CleanupMissingEntries(settings, PluginCatalog.Where(item => item.IsInstalled));
            PluginSettingsStore.Save(settings);
        }

        public async Task<int> LoadPluginsFromAPI(LoaderConfig loaderConfig)
        {
            return await Task.FromResult(LoadRemoteCatalog(loaderConfig).Count);
        }

        public List<PluginCatalogItem> GetPluginCatalogSnapshot(LoaderConfig loaderConfig)
        {
            if (PluginCatalog == null)
            {
                PluginCatalog = new List<PluginCatalogItem>();
            }

            List<PluginCatalogItem> catalog = PluginCatalog
                .Select(item => item.Clone())
                .ToList();

            List<PluginCatalogItem> remoteCatalog = LoadRemoteCatalog(loaderConfig);
            foreach (PluginCatalogItem remoteItem in remoteCatalog)
            {
                bool alreadyInstalled = catalog.Any(item =>
                    item.IsInstalled &&
                    item.Source == PluginSource.Remote &&
                    (
                        (!string.IsNullOrWhiteSpace(item.RemoteName) &&
                         string.Equals(item.RemoteName, remoteItem.RemoteName, StringComparison.OrdinalIgnoreCase)) ||
                        string.Equals(item.Name, remoteItem.Name, StringComparison.OrdinalIgnoreCase)
                    ));

                if (!alreadyInstalled)
                {
                    catalog.Add(remoteItem);
                }
            }

            return catalog
                .OrderBy(item => item.IsInstalled ? 0 : 1)
                .ThenBy(item => item.Source == PluginSource.Local ? 0 : 1)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public PluginCatalogItem InstallRemotePlugin(LoaderConfig loaderConfig, string remotePluginName)
        {
            if (loaderConfig == null || string.IsNullOrWhiteSpace(loaderConfig.LicenseKey))
            {
                throw new InvalidOperationException("A license key is required to install remote plugins.");
            }

            if (string.IsNullOrWhiteSpace(remotePluginName))
            {
                throw new InvalidOperationException("The remote plugin name cannot be empty.");
            }

            string targetDirectory = Path.Combine(Constants.PluginsFolderName, "Remote");
            Directory.CreateDirectory(targetDirectory);

            string targetFileName = BuildSafeDllFileName(remotePluginName);
            string targetPath = Path.Combine(targetDirectory, targetFileName);

            byte[] pluginBytes;
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync($"{CLServerConfig.APIBaseUri}/Plugin/DownloadPlugin/{loaderConfig.LicenseKey}/{remotePluginName}").Result)
            {
                response.EnsureSuccessStatusCode();
                pluginBytes = response.Content.ReadAsByteArrayAsync().Result;
            }

            File.WriteAllBytes(targetPath, pluginBytes);

            try
            {
                PluginSettingsFile settings = PluginSettingsStore.Load();
                Assembly assembly = Assembly.Load(pluginBytes);
                List<PluginCatalogItem> installedItems = CreateCatalogItemsFromAssembly(assembly, settings, targetPath, PluginSource.Remote, remotePluginName, false);
                if (installedItems.Count == 0)
                {
                    throw new InvalidOperationException("The downloaded remote plugin does not contain any valid plugin entry points.");
                }

                foreach (PluginCatalogItem installedItem in installedItems)
                {
                    RemoveMatchingCatalogEntries(installedItem);
                    PluginCatalog.Add(installedItem);
                }

                PluginCatalog = PluginCatalog
                    .OrderBy(item => item.IsInstalled ? 0 : 1)
                    .ThenBy(item => item.Source == PluginSource.Local ? 0 : 1)
                    .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                PluginSettingsStore.CleanupMissingEntries(settings, PluginCatalog.Where(item => item.IsInstalled));
                PluginSettingsStore.Save(settings);
                return installedItems.First();
            }
            catch
            {
                TryDeleteFile(targetPath);
                throw;
            }
        }

        public bool SetPluginEnabled(PluginCatalogItem item, bool enabled)
        {
            if (item == null || !item.IsInstalled)
            {
                return false;
            }

            PluginSettingsFile settings = PluginSettingsStore.Load();
            PluginSettingsEntry entry = PluginSettingsStore.EnsureEntry(settings, item.Name, item.Source, item.AssemblyPath, item.RemoteName, enabled);
            entry.Enabled = enabled;
            PluginSettingsStore.Save(settings);

            PluginCatalogItem installedItem = FindInstalledCatalogItem(item);
            if (installedItem != null)
            {
                installedItem.IsEnabled = enabled;
            }

            return true;
        }

        public bool UninstallRemotePlugin(PluginCatalogItem item)
        {
            if (item == null || !item.IsInstalled || item.Source != PluginSource.Remote)
            {
                return false;
            }

            string fullAssemblyPath = ResolveAssemblyPath(item.AssemblyPath);
            TryDeleteFile(fullAssemblyPath);

            PluginSettingsFile settings = PluginSettingsStore.Load();
            List<PluginCatalogItem> removedItems = PluginCatalog
                .Where(existing => existing != null &&
                                   existing.IsInstalled &&
                                   existing.Source == PluginSource.Remote &&
                                   (
                                       string.Equals(existing.AssemblyPath, item.AssemblyPath, StringComparison.OrdinalIgnoreCase) ||
                                       (!string.IsNullOrWhiteSpace(existing.RemoteName) &&
                                        string.Equals(existing.RemoteName, item.RemoteName, StringComparison.OrdinalIgnoreCase))
                                   ))
                .ToList();

            foreach (PluginCatalogItem removedItem in removedItems)
            {
                PluginSettingsStore.RemoveEntry(settings, removedItem.Name, removedItem.Source, removedItem.AssemblyPath, removedItem.RemoteName);
                PluginCatalog.Remove(removedItem);
            }

            PluginSettingsStore.CleanupMissingEntries(settings, PluginCatalog.Where(existing => existing.IsInstalled));
            PluginSettingsStore.Save(settings);
            return removedItems.Count > 0;
        }

        private static void LoadInstalledPluginAssembly(string assemblyPath, PluginSettingsFile settings)
        {
            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));
            List<PluginCatalogItem> items = CreateCatalogItemsFromAssembly(
                assembly,
                settings,
                assemblyPath,
                IsRemoteAssemblyPath(assemblyPath) ? PluginSource.Remote : PluginSource.Local,
                null,
                true);

            foreach (PluginCatalogItem item in items)
            {
                PluginCatalog.Add(item);
                if (item.IsEnabled && item.Instance != null)
                {
                    Plugins.Add(item.Instance);
                }
            }
        }

        private static List<PluginCatalogItem> CreateCatalogItemsFromAssembly(
            Assembly assembly,
            PluginSettingsFile settings,
            string assemblyPath,
            PluginSource source,
            string remoteName,
            bool isLoadedInCurrentSession)
        {
            List<PluginCatalogItem> items = new List<PluginCatalogItem>();
            foreach (Type type in GetPluginTypes(assembly))
            {
                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                PluginSettingsEntry entry = PluginSettingsStore.EnsureEntry(settings, plugin.Name, source, assemblyPath, remoteName, true);
                PluginCatalogItem item = new PluginCatalogItem
                {
                    Name = plugin.Name,
                    Explanation = string.IsNullOrWhiteSpace(plugin.Explanation) ? "No description provided." : plugin.Explanation,
                    PluginType = plugin.PluginType,
                    Source = entry.Source,
                    IsInstalled = true,
                    IsEnabled = entry.Enabled,
                    IsLoadedInCurrentSession = isLoadedInCurrentSession && entry.Enabled,
                    AssemblyPath = entry.AssemblyPath,
                    RemoteName = entry.RemoteName,
                    Instance = plugin
                };
                items.Add(item);
            }

            return items;
        }

        private static IEnumerable<Type> GetPluginTypes(Assembly assembly)
        {
            Type interfaceType = typeof(IPlugin);
            try
            {
                return assembly
                    .GetTypes()
                    .Where(type => interfaceType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    .ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types
                    .Where(type => type != null && interfaceType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    .ToArray();
            }
        }

        private static List<PluginCatalogItem> LoadRemoteCatalog(LoaderConfig loaderConfig)
        {
            List<PluginCatalogItem> items = new List<PluginCatalogItem>();
            if (loaderConfig == null || string.IsNullOrWhiteSpace(loaderConfig.LicenseKey))
            {
                return items;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = client.GetAsync($"{CLServerConfig.APIBaseUri}/Plugin/MyPlugins/{loaderConfig.LicenseKey}").Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return items;
                    }

                    string result = response.Content.ReadAsStringAsync().Result;
                    List<APIRemoteModule> apiRemoteModules = JsonConvert.DeserializeObject<List<APIRemoteModule>>(result) ?? new List<APIRemoteModule>();
                    foreach (APIRemoteModule module in apiRemoteModules.Where(module => module != null && !string.IsNullOrWhiteSpace(module.Name)))
                    {
                        items.Add(new PluginCatalogItem
                        {
                            Name = string.IsNullOrWhiteSpace(module.DisplayName) ? module.Name : module.DisplayName,
                            Explanation = string.IsNullOrWhiteSpace(module.Explanation)
                                ? "This plugin is available from your remote license catalog and can be installed on demand."
                                : module.Explanation,
                            PluginType = ParsePluginType(module.PluginType),
                            Source = PluginSource.Remote,
                            IsInstalled = false,
                            IsEnabled = false,
                            IsLoadedInCurrentSession = false,
                            AssemblyPath = null,
                            RemoteName = module.Name,
                            Instance = null
                        });
                    }
                }
            }
            catch
            {
                return new List<PluginCatalogItem>();
            }

            return items
                .GroupBy(item => item.RemoteName ?? item.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static PluginType ParsePluginType(string pluginType)
        {
            PluginType parsedType;
            if (!string.IsNullOrWhiteSpace(pluginType) && Enum.TryParse(pluginType, true, out parsedType))
            {
                return parsedType;
            }

            return PluginType.PREMIUM;
        }

        private static void RemoveMatchingCatalogEntries(PluginCatalogItem installedItem)
        {
            List<PluginCatalogItem> duplicates = PluginCatalog
                .Where(existing => existing != null &&
                                   existing.IsInstalled &&
                                   existing.Source == installedItem.Source &&
                                   (
                                       string.Equals(existing.AssemblyPath, installedItem.AssemblyPath, StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(existing.Name, installedItem.Name, StringComparison.OrdinalIgnoreCase)
                                   ))
                .ToList();

            foreach (PluginCatalogItem duplicate in duplicates)
            {
                PluginCatalog.Remove(duplicate);
            }
        }

        private static PluginCatalogItem FindInstalledCatalogItem(PluginCatalogItem item)
        {
            return PluginCatalog.FirstOrDefault(existing =>
                existing != null &&
                existing.IsInstalled &&
                existing.Source == item.Source &&
                string.Equals(existing.Name, item.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.AssemblyPath, item.AssemblyPath, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsRemoteAssemblyPath(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                return false;
            }

            string remoteDirectory = Path.GetFullPath(Path.Combine(Constants.PluginsFolderName, "Remote")).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullAssemblyPath = Path.GetFullPath(assemblyPath);
            return fullAssemblyPath.StartsWith(remoteDirectory, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveAssemblyPath(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                return null;
            }

            if (Path.IsPathRooted(assemblyPath))
            {
                return assemblyPath;
            }

            return Path.Combine(Constants.PluginsFolderName, assemblyPath);
        }

        private static string BuildSafeDllFileName(string remotePluginName)
        {
            string fileName = remotePluginName;
            if (fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidCharacter, '_');
            }

            return fileName + ".dll";
        }

        private static void TryDeleteFile(string filePath)
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

        protected class APIRemoteModule
        {
            public uint Id { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Explanation { get; set; }
            public string PluginType { get; set; }
            public uint LicenseId { get; set; }
            public byte[] Content { get; set; }
        }
    }
}

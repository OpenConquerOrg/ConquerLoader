using Newtonsoft.Json;
using System;
using System.IO;

namespace CLAutoPatchPlugin
{
    internal static class AutoPatchSettingsStore
    {
        private static string LegacySettingsPath
        {
            get
            {
                return Path.Combine(PluginsDirectory, "AutoPatchPlugin.settings.json");
            }
        }

        internal static string PluginsDirectory
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CLCore.Constants.PluginsFolderName);
            }
        }

        internal static string SettingsPath
        {
            get
            {
                return Path.Combine(PluginsDirectory, "CLAutoPatchPlugin.settings.json");
            }
        }

        internal static AutoPatchSettings Load()
        {
            try
            {
                string settingsPath = File.Exists(SettingsPath) ? SettingsPath : LegacySettingsPath;
                if (!File.Exists(settingsPath))
                {
                    return new AutoPatchSettings();
                }

                AutoPatchSettings settings = JsonConvert.DeserializeObject<AutoPatchSettings>(File.ReadAllText(settingsPath));
                return settings ?? new AutoPatchSettings();
            }
            catch
            {
                return new AutoPatchSettings();
            }
        }

        internal static void Save(AutoPatchSettings settings)
        {
            Directory.CreateDirectory(PluginsDirectory);
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}

using Newtonsoft.Json;
using System;
using System.IO;

namespace AutoPatchPlugin
{
    internal static class AutoPatchSettingsStore
    {
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
                return Path.Combine(PluginsDirectory, "AutoPatchPlugin.settings.json");
            }
        }

        internal static AutoPatchSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return new AutoPatchSettings();
                }

                AutoPatchSettings settings = JsonConvert.DeserializeObject<AutoPatchSettings>(File.ReadAllText(SettingsPath));
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

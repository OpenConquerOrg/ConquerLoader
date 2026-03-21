using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoPatchPlugin
{
    internal class AutoPatchState
    {
        public Dictionary<string, string> AppliedPackages { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    internal static class AutoPatchStateStore
    {
        internal static string StatePath
        {
            get
            {
                return Path.Combine(AutoPatchSettingsStore.PluginsDirectory, "AutoPatchPlugin.state.json");
            }
        }

        internal static AutoPatchState Load()
        {
            try
            {
                if (!File.Exists(StatePath))
                {
                    return new AutoPatchState();
                }

                AutoPatchState state = JsonConvert.DeserializeObject<AutoPatchState>(File.ReadAllText(StatePath));
                if (state == null)
                {
                    return new AutoPatchState();
                }

                if (state.AppliedPackages == null)
                {
                    state.AppliedPackages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return state;
            }
            catch
            {
                return new AutoPatchState();
            }
        }

        internal static void Save(AutoPatchState state)
        {
            Directory.CreateDirectory(AutoPatchSettingsStore.PluginsDirectory);
            File.WriteAllText(StatePath, JsonConvert.SerializeObject(state ?? new AutoPatchState(), Formatting.Indented));
        }
    }
}

using CLCore.Models;
using System;
using System.Collections.Generic;

namespace CLCore
{
    public class PluginPreLaunchContext
    {
        public LoaderConfig LoaderConfig { get; set; }
        public ServerConfiguration Server { get; set; }
        public string ExecutablePath { get; set; }
        public string WorkingDirectory { get; set; }
        public string StartupPath { get; set; }
        public bool UseDecryptedServerDat { get; set; }
        public bool UseDirectX9Environment { get; set; }
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Action<int> ReportProgress { get; set; }
        public Action<string> Log { get; set; }
    }
}

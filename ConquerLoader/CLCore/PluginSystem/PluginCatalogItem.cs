namespace CLCore
{
    public enum PluginSource
    {
        Local = 0,
        Remote = 1
    }

    public sealed class PluginCatalogItem
    {
        public string Name { get; set; }
        public string Explanation { get; set; }
        public PluginType PluginType { get; set; }
        public PluginSource Source { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsLoadedInCurrentSession { get; set; }
        public string AssemblyPath { get; set; }
        public string RemoteName { get; set; }
        public IPlugin Instance { get; set; }

        public PluginCatalogItem Clone()
        {
            return new PluginCatalogItem
            {
                Name = Name,
                Explanation = Explanation,
                PluginType = PluginType,
                Source = Source,
                IsInstalled = IsInstalled,
                IsEnabled = IsEnabled,
                IsLoadedInCurrentSession = IsLoadedInCurrentSession,
                AssemblyPath = AssemblyPath,
                RemoteName = RemoteName,
                Instance = Instance
            };
        }
    }
}

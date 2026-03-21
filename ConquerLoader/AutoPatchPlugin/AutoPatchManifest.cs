using System.Collections.Generic;

namespace AutoPatchPlugin
{
    internal class AutoPatchManifest
    {
        public string Version { get; set; }
        public string BaseUrl { get; set; }
        public List<AutoPatchManifestFile> Files { get; set; } = new List<AutoPatchManifestFile>();
    }

    internal class AutoPatchManifestFile
    {
        public string Path { get; set; }
        public string Url { get; set; }
        public long? Size { get; set; }
        public string Sha256 { get; set; }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace CLAutoPatchPlugin
{
    internal class AutoPatchManifest
    {
        public string Version { get; set; }
        public string BaseUrl { get; set; }
        public List<AutoPatchManifestPackage> Packages { get; set; } = new List<AutoPatchManifestPackage>();
        public List<AutoPatchManifestPackage> Archives { get; set; } = new List<AutoPatchManifestPackage>();
        public List<AutoPatchManifestLegacyFile> Files { get; set; } = new List<AutoPatchManifestLegacyFile>();

        public List<AutoPatchManifestPackage> GetPackages()
        {
            return Packages
                .Concat(Archives)
                .Where(package => package != null)
                .ToList();
        }

        public bool UsesLegacyFileEntries
        {
            get
            {
                return Files != null && Files.Count > 0;
            }
        }
    }

    internal class AutoPatchManifestPackage
    {
        public string Id { get; set; }
        public string Archive { get; set; }
        public string Url { get; set; }
        public string Format { get; set; }
        public string ExtractTo { get; set; }
        public long? Size { get; set; }
        public string Sha256 { get; set; }
        public bool Enabled { get; set; } = true;
    }

    internal class AutoPatchManifestLegacyFile
    {
        public string Path { get; set; }
        public string Url { get; set; }
        public long? Size { get; set; }
        public string Sha256 { get; set; }
    }
}

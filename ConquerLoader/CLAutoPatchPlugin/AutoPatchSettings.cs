namespace CLAutoPatchPlugin
{
    public class AutoPatchSettings
    {
        public bool Enabled { get; set; }
        public string ManifestLocation { get; set; }
        public bool FailLaunchOnError { get; set; } = true;
        public string RelativeTargetFolder { get; set; }
    }
}

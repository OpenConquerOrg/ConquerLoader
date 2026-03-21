namespace CLCore
{
    public class PluginPreLaunchResult
    {
        public bool ContinueLaunch { get; set; } = true;
        public string Message { get; set; }

        public static PluginPreLaunchResult Success()
        {
            return new PluginPreLaunchResult { ContinueLaunch = true };
        }

        public static PluginPreLaunchResult Fail(string message)
        {
            return new PluginPreLaunchResult
            {
                ContinueLaunch = false,
                Message = message
            };
        }
    }
}

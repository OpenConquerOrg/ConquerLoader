namespace CLCore
{
    public interface IPreLaunchPlugin
    {
        PluginPreLaunchResult BeforeLaunch(PluginPreLaunchContext context);
    }
}

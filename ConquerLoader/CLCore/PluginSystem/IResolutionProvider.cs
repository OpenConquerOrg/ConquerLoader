using System.Collections.Generic;

namespace CLCore
{
    /// <summary>
    /// Interface for plugins that want to provide custom screen resolutions
    /// that will appear in the ConquerLoader resolution selector
    /// </summary>
    public interface IResolutionProvider
    {
        /// <summary>
        /// Gets a list of custom resolutions provided by this plugin
        /// Format: "WIDTHxHEIGHT" (e.g., "2560x1440")
        /// </summary>
        /// <returns>List of resolution strings</returns>
        List<string> GetCustomResolutions();

        /// <summary>
        /// Gets the display name for a custom resolution
        /// </summary>
        /// <param name="resolution">The resolution string (e.g., "2560x1440")</param>
        /// <returns>The display name (e.g., "2K (2560x1440)")</returns>
        string GetResolutionDisplayName(string resolution);

        /// <summary>
        /// Called when the user selects a custom resolution from the dropdown
        /// </summary>
        /// <param name="resolution">The selected resolution string</param>
        void OnResolutionSelected(string resolution);

        /// <summary>
        /// Called before launching the game to apply the resolution settings
        /// Returns true if the provider handled the resolution
        /// </summary>
        /// <param name="resolution">The resolution to apply</param>
        /// <param name="workingDirectory">The game working directory</param>
        /// <param name="log">Optional logging action</param>
        /// <returns>True if handled, false to let other providers handle it</returns>
        bool ApplyResolution(string resolution, string workingDirectory, System.Action<string> log);
    }
}

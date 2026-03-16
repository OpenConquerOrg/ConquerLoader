namespace CLCore
{
    public interface IPlugin
	{
		/// <summary>
		/// The name of the plugin.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// The Explanation of the plugin.
		/// </summary>
		string Explanation { get; }
		/// <summary>
		/// The type of Plugin. FREE or PREMIUM are the posible values.
		/// </summary>
		PluginType PluginType { get; }
		/// <summary>
		/// The method is called when you have loaded the plugins in ConquerLoader.
		/// </summary>
		void Init();
		/// <summary>
		/// The method is called when go to configuration button of plugin in ConquerLoader.
		/// </summary>
		void Configure();
        //bool RemotePlugin { get; set; }
    }
}

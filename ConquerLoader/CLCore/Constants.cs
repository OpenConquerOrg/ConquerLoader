using System.Windows.Forms;

namespace CLCore
{
	public static class Constants
	{
		//The folder name which contains the plugin DLLs
		public const string PluginsFolderName = "Plugins";
		public static string LicenseKey = null;
		public static string ClientPath = null;
		public static System.ComponentModel.BackgroundWorker MainWorker = null;
		public static bool CloseOnFinish = false;
		public static bool HideInTrayOnFinish = false;
		public static int MinVersionCreateFlashFix = 5717;
        public static int MinVersionUseRAWServerDat = 5095;
        public static int MaxVersionUseRAWServerDat = 6736;
		public static string LockConfigurationKey = "CONQUERLOADERDFX";
		public static int MinVersionUseDX8DX9Folders = 6371;
		public static bool EnableCLServerConnections = true; // manual de momento
        public static bool ForceServerDat = false;
    }
	public static class CLTheme
	{
        public static Control.ControlCollection MainControls = null;
        public static object MainForm = null;
    }
	public enum PluginType
	{
		FREE,
		PREMIUM
	}
}


using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CLCore.Models
{
    public class LoaderConfig
    {
        public BindingList<ServerConfiguration> Servers { get; set; }
        public ServerConfiguration DefaultServer { get; set; }
        public bool DebugMode { get; set; }
        public bool CloseOnFinish { get; set; }
        public bool HighResolution { get; set; }
        public bool FullScreen { get; set; }
        public bool ServernameChange { get; set; }
        public bool DisableAutoFixFlash { get; set; }
        public string Title { get; set; }
        public string Lang { get; set; }
        public string LicenseKey { get; set; }
        public bool DisableScreenChanges { get; set; }
        public bool UseCustomDLLs { get; set; }
        public bool FPSUnlock { get; set; }

        public LoaderConfig()
        {
            if (Servers == null)
            {
                Servers = new BindingList<ServerConfiguration>();
            }
        }

        public List<ServerDatGroup> GetGroups()
        {
            List<ServerDatGroup> Groups = new List<ServerDatGroup>();
            foreach (ServerConfiguration Server in Servers)
            {
                ServerDatGroup g = Groups.Where(x => x.GroupName == Server.Group.GroupName && x.GroupIcon == Server.Group.GroupIcon).FirstOrDefault();
                if (g != null)
                {
                    g.Servers.Add(Server);
                } else
                {
                    if (Server.Group.Servers == null)
                    {
                        Server.Group.Servers = new List<ServerConfiguration>();
                    }
                    Server.Group.Servers.Add(Server);
                    Groups.Add(Server.Group);
                }
            }
            return Groups;
        }
    }
}

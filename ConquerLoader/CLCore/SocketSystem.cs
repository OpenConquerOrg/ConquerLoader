using CLCore.Models.DTOs;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace CLCore
{
    public static class CLServerConfig
    {
        public const string DefaultAPIBaseUri = "https://api.conquerloader.com/api";
        public static string APIBaseUri = ResolveApiBaseUri();
        public static uint ServerPort = 8000;

        public static string NormalizeApiBaseUri(string apiBaseUri)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUri))
            {
                return DefaultAPIBaseUri;
            }

            return apiBaseUri.Trim().TrimEnd('/');
        }

        private static string ResolveApiBaseUri()
        {
            try
            {
                return NormalizeApiBaseUri(ConfigurationManager.AppSettings["ApiBaseUri"]);
            }
            catch
            {
                return DefaultAPIBaseUri;
            }
        }
    }
    public class ConnectionEvent
    {
        public string IPAddress { get; set; }
        public System.Timers.ElapsedEventHandler Event { get; set; }
    }
    public class CLServer
    {
        public SimpleTcpServer TcpServer { get; set; }
        public List<ConnectionEvent> Connections { get; set; }
        public delegate void NotAllowedIP(string IPAddress);
        public event NotAllowedIP DetectedNotAllowedIP;
        private System.Timers.Timer WatcherTimer { get; set; }
        private Models.LogWritter LogWritter { get; set; }
        private string API_GET_CONNECTIONS = "";
        private string API_SET_CONNECTIONS = "";
        private string API_HAVE_CONNECTIONS = "";
        private string API_LICENSE_KEY = "";
        /// <summary>
        /// Launch CLServer
        /// </summary>
        /// <param name="LicenseKey"></param>
        public CLServer(string LicenseKey, bool ServerMode = false)
        {
            API_LICENSE_KEY = LicenseKey;
            Init();
            if (ServerMode)
            {
                API_LICENSE_KEY = LicenseKey;
                Init();
                bool SetToApi = SetConnectionsToAPI(true);
                if (SetToApi)
                {
                    TcpServer = new SimpleTcpServer().Start((int)CLServerConfig.ServerPort);
                    DetectedNotAllowedIP += CLServer_DetectedNotAllowedIP;

                    // Setup a socket server
                    TcpServer.ClientConnected += Server_ClientConnected;
                    TcpServer.ClientDisconnected += Server_ClientDisconnected;
                    TcpServer.Delimiter = 0x13;
                    //Server.TcpServer.DelimiterDataReceived += (sendr, msg) =>
                    //{
                    //    if (msg.MessageString.StartsWith("/"))
                    //        {
                    //            string[] parameters = msg.MessageString.TrimStart('/').Split(' ');
                    //            switch (parameters[0])
                    //            {
                    //                case "autoclick_detected":
                    //                    break;
                    //                default:
                    //                    break;
                    //            }
                    //        }
                    //        else
                    //        {
                    //        }
                    //};
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Cannot connect to ConquerLoader API. Contact with darkfoxdeveloper@gmail.com for any question or problem.", "CLCore", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    Environment.Exit(255);
                }
            } else
            {
                if (HaveConnectionsFromAPI())
                {
                    GetConnectionsFromAPI();
                }
            }
        }
        private void Init()
        {
            Constants.LicenseKey = API_LICENSE_KEY;
            API_HAVE_CONNECTIONS = $"{CLServerConfig.APIBaseUri}/Connection/HasAny/{Constants.LicenseKey}";
            API_GET_CONNECTIONS = $"{CLServerConfig.APIBaseUri}/Connection/List/{Constants.LicenseKey}";
            API_SET_CONNECTIONS = $"{CLServerConfig.APIBaseUri}/Connection/Set/{Constants.LicenseKey}";
            Connections = new List<ConnectionEvent>();
            LogWritter = new Models.LogWritter(Path.Combine(Directory.GetCurrentDirectory(), "CLServer.log"));
        }

        private bool HaveConnectionsFromAPI()
        {
            return HttpRequests.GET(API_HAVE_CONNECTIONS).IsSuccessStatusCode;
        }

        private List<ConnectionEvent> GetConnectionsFromAPI()
        {
            string result = HttpRequests.GET(API_GET_CONNECTIONS).Content.ReadAsStringAsync().Result;
            List<string> IPList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);
            Connections.Clear(); // Clear previous
            foreach (string IP in IPList)
            {
                Connections.Add(new ConnectionEvent() { IPAddress = IP });
            }
            return Connections;
        }

        private bool SetConnectionsToAPI(bool Clear = false)
        {
            List<ConnectionDTO> IPs = new List<ConnectionDTO>();
            if (!Clear)
            {
                foreach (ConnectionEvent ce in Connections)
                {
                    IPs.Add(new ConnectionDTO() { IPAddress = ce.IPAddress });
                }
            }
            LogWritter.Write(string.Format("IPs for API: {0}", IPs.Count));
            bool Success = HttpRequests.JsonPOST(API_SET_CONNECTIONS, IPs).IsSuccessStatusCode;
            return Success;
        }

        private void CLServer_DetectedNotAllowedIP(string IPAddress)
        {
            LogWritter.Write(string.Format("Detected a IP {0} not connected to CLServer. Warning!", IPAddress));
        }

        private void Server_ClientConnected(object sender, TcpClient e)
        {
            string ClientAddressIP = ((IPEndPoint)e.Client.RemoteEndPoint).Address.ToString();
            if (Connections.Where(x => x.IPAddress == ClientAddressIP).Count() <= 0)
            {
                Connections.Add(new ConnectionEvent() { IPAddress = ClientAddressIP });
            }
            SetConnectionsToAPI();
            LogWritter.Write(string.Format("{0} Connections sended to API", Connections.Count()));
        }
        private void Server_ClientDisconnected(object sender, TcpClient e)
        {
            string ClientAddressIP = ((IPEndPoint)e.Client.RemoteEndPoint).Address.ToString();
            ConnectionEvent cEv = Connections.Where(x => x.IPAddress.Equals(ClientAddressIP)).FirstOrDefault();
            if (cEv != null)
            {
                Connections.Remove(cEv);
            }
            SetConnectionsToAPI();
            LogWritter.Write(string.Format("{0} Connections sended to API", Connections.Count()));
        }
        public bool CheckConnectionByIP(string IPAddress)
        {
            if (HaveConnectionsFromAPI())
            {
                GetConnectionsFromAPI();
            }
            LogWritter.Write(string.Format("[DEBUG] Total connections: {0}", Connections.Count()));
            LogWritter.Write(string.Format("[DEBUG] Checking if valid IP: {0}", IPAddress));
            //if (IsLocalIpAddress(IPAddress))
            //{
            //    return true;
            //}
            return Connections.Where(x => x.IPAddress == IPAddress).Count() > 0;
        }
        public bool IsLocalIpAddress(string host)
        {
            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
        public ConnectionEvent GetConnectionEventByIP(string IPAddress)
        {
            return Connections.Where(x => x.IPAddress == IPAddress).FirstOrDefault();
        }
        public void WatchClientIP(string IPAddress)
        {
            if (WatcherTimer == null)
            {
                WatcherTimer = new System.Timers.Timer
                {
                    Interval = 10000,
                    Enabled = true
                };
            }
            ConnectionEvent cEv = GetConnectionEventByIP(IPAddress);
            System.Timers.ElapsedEventHandler eventForHandle = new System.Timers.ElapsedEventHandler((s, e) => WatchClientIP_Watcher(IPAddress));
            if (eventForHandle != null && cEv != null)
            {
                cEv.Event = eventForHandle;
                WatcherTimer.Elapsed += eventForHandle;
            }
        }
        private void UnwatchClientIP(string IPAddress)
        {
            if (WatcherTimer != null)
            {
                ConnectionEvent cEv = GetConnectionEventByIP(IPAddress);
                Connections.Remove(cEv);
                WatcherTimer.Elapsed -= cEv.Event;
            }
        }
        private void WatchClientIP_Watcher(string IPAddress)
        {
            bool Connected = CheckConnectionByIP(IPAddress);
            LogWritter.Write(string.Format("Valid Connection: {0}", Connected));
            if (!Connected)
            {
                DetectedNotAllowedIP(IPAddress);
                UnwatchClientIP(IPAddress);
            }
        }
        private void SyncroConnections()
        {
            GetConnectionsFromAPI();
        }
    }
    public class CLClient
    {
        public SimpleTcpClient Client = null;
        public CLClient(string IP, uint Port)
        {
            Client = new SimpleTcpClient();
            Client.Connect(IP, (int)Port);
        }

        public void Send(string content, uint seconds = 3)
        {
            Client.WriteLineAndGetReply(content, TimeSpan.FromSeconds(seconds));
        }

        public Message SendAndGetMessage(string content, uint seconds = 3)
        {
            return Client.WriteLineAndGetReply(content, TimeSpan.FromSeconds(seconds));
        }

        public void Disconnect()
        {
            Client.Disconnect();
            Client.Dispose();
        }
    }

    public static class SocketSystem
    {
        public static CLClient CurrentSocketClient = null;
    }
}

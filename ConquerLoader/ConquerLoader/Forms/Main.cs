using CLCore;
using CLCore.Models;
using ConquerLoader.Models;
using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ConquerLoader.Forms
{
    public partial class Main : MetroFramework.Forms.MetroForm
    {
        public LoaderConfig LoaderConfig = null;
        public ServerConfiguration SelectedServer = null;
        public Process CurrentConquerProcess = null;
        public string HookINI = "CLHook.ini";
        public string HookDLL = "CLHook.dll";
        public bool AllStarted = false;
        public bool DX9Allowed = false;
        public bool CustomDLLs = false;
        public Main()
        {
            InitializeComponent();
            CLTheme.MainForm = this;
            CLTheme.MainControls = this.Controls;
            this.Resizable = false;
            this.Theme = MetroFramework.MetroThemeStyle.Light;
            LoaderConfig = Core.GetLoaderConfig();
            if (Core.UseEncryptedConfig)
            {
                btnSettings.Enabled = false;
            }
            Core.LoadControlTranslations(Controls);
            LoaderEvents.LauncherLoaded += LoaderEvents_LauncherLoaded;
            LoaderEvents.ConquerLaunched += LoaderEvents_ConquerLaunched;
            LoaderEvents.LauncherExit += LoaderEvents_LauncherExit;
            Constants.ClientPath = Directory.GetCurrentDirectory();
            if (LoaderConfig != null) {
                Constants.CloseOnFinish = LoaderConfig.CloseOnFinish;
                CustomDLLs = LoaderConfig.UseCustomDLLs;
                if (LoaderConfig.HighResolution)
                {
                    cbxResolutions.Text = "1024x768";
                }
                else if (LoaderConfig.FHDResolution)
                {
                    cbxResolutions.Text = "1920x1080";
                }
                else
                {
                    cbxResolutions.Text = "800x600";
                }
            }
            Constants.MainWorker = worker;
            Core.LoadAvailablePlugins();
            Core.LoadRemotePlugins(); // Experimental
            Core.InitPlugins();
            DX9Allowed = Core.DirectXVersion() >= 9;
        }
        private void Main_Load(object sender, EventArgs e)
        {
            if (LoaderConfig == null)
            {
                Settings s = new Settings();
                s.ShowDialog(this);
            }
            else
            {
                LoadConfigInForm();
            }
            AllStarted = true;
            RefreshServerList(true, true);

            LoaderEvents.LauncherLoadedStartEvent();

            bool InstaledCPlusRedistributableX64 = Core.IsVCRuntimeInstalled("x64");
            bool InstaledCPlusRedistributableX86 = Core.IsVCRuntimeInstalled("x86");

            // Install the C++ redistributable 2015-2019 if need
            if (!InstaledCPlusRedistributableX64 && Environment.Is64BitOperatingSystem)
            {
                File.WriteAllBytes("cpp2015-2019x64.exe", Properties.Resources.VC_redist_x64);
                Process.Start("cpp2015-2019x64.exe").WaitForExit();
                File.Delete("cpp2015-2019x64.exe");
            }
            if (!InstaledCPlusRedistributableX86)
            {
                File.WriteAllBytes("cpp2015-2019x86.exe", Properties.Resources.VC_redist_x86);
                Process.Start("cpp2015-2019x86.exe").WaitForExit();
                File.Delete("cpp2015-2019x86.exe");
            }
        }

        private void GenerateRequiredDLL()
        {
            Core.LogWritter.Write("Generating Required DLLs...");
            string PathToConquerExe = Path.Combine(Application.StartupPath, SelectedServer.ExecutableName);
            string WorkingDir = Path.GetDirectoryName(PathToConquerExe);
            // Generate required dlls if are necessary
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "CLHook.dll")))
            {
                Core.LogWritter.Write("Generated CLHook.dll");
                SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, "CLHook.dll"), Properties.Resources.CLHook_Legacy, ex => Core.LogWritter.Write(ex.ToString()));
            } else
            {
                Core.LogWritter.Write("Using existing CLHook.dll");
            }
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ConquerCipherHook.dll")))
            {
                Core.LogWritter.Write("Generated ConquerCipherHook.dll");
                SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, "ConquerCipherHook.dll"), Properties.Resources.ConquerCipherHook, ex => Core.LogWritter.Write(ex.ToString()));
            } else
            {
                Core.LogWritter.Write("Using existing ConquerCipherHook.dll");
            }
        }

        //  The NotifyIcon object
        private void TrayMinimizerForm_Resize(object sender, EventArgs e)
        {
            if (Constants.HideInTrayOnFinish)
            {
                noty.Text = ProductName;
                noty.Icon = Properties.Resources.ConquerLoaderLogo;

                if (FormWindowState.Minimized == this.WindowState)
                {
                    noty.Visible = true;
                    Hide();
                    noty.BalloonTipTitle = $"{ProductName} {ProductVersion}";
                    noty.BalloonTipText = "The extensible loader for ConquerOnline";
                    noty.ShowBalloonTip(1000);
                }
                else if (FormWindowState.Normal == this.WindowState)
                {
                    noty.Visible = false;
                }
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            bool Exists = CurrentConquerProcess != null;
            if (Exists)
            {
                try
                {
                    CurrentConquerProcess.Kill();
                }
                catch (Exception)
                {
                } finally
                {
                    if (Process.GetProcessesByName("Conquer.exe").Count() <= 0)
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void LoaderEvents_LauncherLoaded()
        {
            Core.LogWritter.Write("Event LauncherLoaded Fired!");
        }

        private void LoaderEvents_ConquerLaunched(List<Parameter> parameters)
        {
            string parametersTxt = "";
            foreach (Parameter p in parameters)
            {
                parametersTxt += $"Id={p.Id };Value={p.Value}";
            }
            Core.LogWritter.Write($"Event ConquerLaunched Fired! Parameters: {parametersTxt}");
        }

        private void LoaderEvents_LauncherExit(List<Parameter> parameters)
        {
            try { Core.LogWritter.Write("Event LauncherExit Fired!"); } catch(Exception) {}
        }

        private void LoadConfigInForm()
        {
            cbxServers.Items.Clear();
            if (LoaderConfig.Title != null && LoaderConfig.Title.Length > 0)
            {
                Text = LoaderConfig.Title;
            }
            Core.LogWritter.Write("Loaded config.json");
            btnLogModules.Enabled = LoaderConfig.DebugMode;
            btnLogModules.Visible = LoaderConfig.DebugMode;
            btnCloseCO.Enabled = LoaderConfig.DebugMode;
            btnCloseCO.Visible = LoaderConfig.DebugMode;
            tglFPSUnlock.Checked = LoaderConfig.FPSUnlock;
            Constants.LicenseKey = LoaderConfig.LicenseKey;
            foreach (ServerConfiguration server in LoaderConfig.Servers)
            {
                cbxServers.Items.Add(server.ServerName);
            }
            RefreshServerList(true, true);
        }

        private void RefreshServerList(bool CheckServerStatus = true, bool SelectDefault = false)
        {
            if (LoaderConfig.Servers.Count > 0)
            {
                foreach (object s in cbxServers.Items)
                {
                    if (LoaderConfig.DefaultServer != null && s.Equals(LoaderConfig.DefaultServer.ServerName))
                    {
                        if (LoaderConfig.DefaultServer.ServerVersion == 5187)
                        {
                            if (!cbxResolutions.Items.Contains("1920x1080")) cbxResolutions.Items.Add("1920x1080");
                        }
                        else
                        {
                            if (cbxResolutions.Items.Contains("1920x1080")) cbxResolutions.Items.Remove("1920x1080");
                        }
                        if (SelectDefault)
                        {
                            cbxServers.SelectedItem = s;
                        }
                        if (CheckServerStatus)
                        {
                            SetServerStatus();
                        }
                    }
                }
            }
        }

        private void SetServerStatus()
        {
            if (AllStarted)
            {
                bool Online = Core.ServerAvailable(LoaderConfig.DefaultServer.LoginHost, LoaderConfig.DefaultServer.GamePort);
                serverStatus.Text = Online ? "ONLINE" : "OFFLINE";
                if (Online)
                {
                    serverStatus.ForeColor = Color.DarkGreen;
                }
                else
                {
                    serverStatus.ForeColor = Color.DarkRed;
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (LoaderConfig == null) return;
                if (cbxServers.SelectedItem == null) return;
                SelectedServer = LoaderConfig.Servers.Where(x => x.ServerName == cbxServers.SelectedItem.ToString()).FirstOrDefault();
                if (SelectedServer != null)
                {
                    // Save Last Selected
                    SelectedServer = LoaderConfig.Servers.Where(x => x.ServerName == cbxServers.SelectedItem.ToString()).FirstOrDefault();
                    LoaderConfig.DefaultServer = SelectedServer;
                    Core.SaveLoaderConfig(LoaderConfig);
                    if (File.Exists(HookINI))
                    {
                        File.Delete(HookINI);
                    }
                    if (LoaderConfig.ServernameChange) // That is deprecated, in future are removed from code
                    {
                        if (SelectedServer.ServerNameMemoryAddress != null && SelectedServer.ServerNameMemoryAddress.Length >= 8) // Detect valid memory address
                        {
                            // Nothing to do. Using the specified custom memory address 
                        }
                        else
                        {
                            // Set default recommended for each range of versions
                            if (SelectedServer.ServerVersion < 5600)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x005726DC";
                            }
                            if (SelectedServer.ServerVersion >= 5600)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x0097FAB8";
                            }
                            if (SelectedServer.ServerVersion >= 5700)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x009BEA00";
                            }
                            if (SelectedServer.ServerVersion >= 6000)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x00A56348";
                            }
                            if (SelectedServer.ServerVersion >= 6609)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x00CE8180";
                            }
                            if (SelectedServer.ServerVersion >= 6617)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x00CD7240";
                            }
                            // Fix for this range of versions 5180 - 5200
                            if (SelectedServer.ServerVersion >= 5180 && SelectedServer.ServerVersion <= 5200)
                            {
                                SelectedServer.ServerNameMemoryAddress = "0x0071E688";
                            }
                            Core.LogWritter.Write("Using Servername Change with Address: " + SelectedServer.ServerNameMemoryAddress);
                        }
                    }
                    else
                    {
                        SelectedServer.ServerNameMemoryAddress = "0";
                    }
                    // Create first the config used by DLL
                    File.WriteAllText(HookINI, "[CLHook]"
                        + Environment.NewLine + "HOST=" + SelectedServer.LoginHost
                        + Environment.NewLine + "GAMEHOST=" + SelectedServer.GameHost
                        + Environment.NewLine + "PORT=" + SelectedServer.LoginPort
                        + Environment.NewLine + "GAMEPORT=" + SelectedServer.GamePort
                        + Environment.NewLine + "SERVERNAME=" + SelectedServer.ServerName
                        + Environment.NewLine + "ENABLE_HOSTNAME=" + (SelectedServer.EnableHostName ? "1" : "0")
                        + Environment.NewLine + "HOSTNAME=" + SelectedServer.Hostname
                        + Environment.NewLine + "SERVER_VERSION=" + SelectedServer.ServerVersion
                        + Environment.NewLine + "SERVERNAME_MEMORYADDRESS=" + SelectedServer.ServerNameMemoryAddress
                        + Environment.NewLine + "DISABLE_AUTOFIX_FLASH=" + (LoaderConfig.DisableAutoFixFlash ? "1" : "0")
                        );
                    Core.LogWritter.Write("Created the Hook Configuration");
                    // Modify Setup of client
                    if (!LoaderConfig.DisableScreenChanges)
                    {
                        Core.LogWritter.Write("Changing Screen Options...");
                        string SetupIniPath = Path.Combine(Directory.GetCurrentDirectory(), "ini", "GameSetup.ini");
                        IniManager parser = new IniManager(SetupIniPath, "ScreenMode");
                        /*
                         *  ScreenModeRecord
                         *  0 = 800x600, windowed
                         *  1 = 800x600, full-screen
                         *  2 = 1024x768, windowed
                         *  3 = 1024x768, full-screen
                         *  FullScrType
                         *  1 = FullScreen
                         *  0 = Windowed
                        */
                        parser.Write("ScreenMode", "FullScrType", LoaderConfig.FullScreen ? "1" : "0");
                        parser.Write("ScreenMode", "ScrWidth", LoaderConfig.FHDResolution ? "1920" : LoaderConfig.HighResolution ? "1024" : "800");
                        parser.Write("ScreenMode", "ScrHeight", LoaderConfig.FHDResolution ? "1080" : LoaderConfig.HighResolution ? "768" : "600");
                        if (LoaderConfig.HighResolution || LoaderConfig.FHDResolution)
                        {
                            parser.Write("ScreenMode", "ScreenModeRecord", LoaderConfig.FullScreen ? "3" : "2");
                            Core.LogWritter.Write($"[+] Changing ScreenModeRecord to {(LoaderConfig.FullScreen ? "2" : "3")}");
                        }
                        else
                        {
                            parser.Write("ScreenMode", "ScreenModeRecord", LoaderConfig.FullScreen ? "1" : "0");
                            Core.LogWritter.Write($"[+] Changing ScreenModeRecord to {(LoaderConfig.FullScreen ? "1" : "0")}");
                        }
                        Core.LogWritter.Write($"[+] Changing FullScrType to {(LoaderConfig.FullScreen ? "0" : "1")}");
                        Core.LogWritter.Write($"[+] Changing ScrWidth to {(LoaderConfig.HighResolution ? "1024" : "800")}");
                        Core.LogWritter.Write($"[+] Changing ScrHeight to {(LoaderConfig.HighResolution ? "768" : "600")}");
                    }
                    worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                Core.LogWritter.Write("Error found: " + ex);
            }
        }

        private void RebuildServerDat()
        {
            new ServersDatGenerator(LoaderConfig.Servers);
        }

        private void Worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (CustomDLLs)
            {
                HookDLL = "COHook.dll";
            }
            // Do start conquer and inject
            Core.LogWritter.Write("Launching " + SelectedServer.ExecutableName + "...");
            string PathToConquerExe = Path.Combine(Application.StartupPath, SelectedServer.ExecutableName);
            string WorkingDir = Path.GetDirectoryName(PathToConquerExe);
            bool NoUseDX8_DX9 = true;
            bool UseDecryptedServerDat = false;
            bool AlreadyUsingLoader = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length >= 1;
            if (AlreadyUsingLoader)
            {
                Core.LogWritter.Write("Detected already using ConnquerLoader.");
            }
            GenerateRequiredDLL();
            if (SelectedServer.ServerVersion >= Constants.MinVersionUseRAWServerDat && SelectedServer.ServerVersion <= Constants.MaxVersionUseRAWServerDat)
            {
                HookDLL = "COHook.dll";
                Core.LogWritter.Write("Using Custom Server.dat (New Raw Mode, Better performance)...");
                RebuildServerDat();
                CustomDLLs = true; // Force Launch first the dll COHook
            }
            // If using 6000 Version or more the HookDLL Used is COHook.dll
            if ((SelectedServer.ServerVersion >= Constants.MinVersionUseServerDat && SelectedServer.ServerVersion <= Constants.MaxVersionUseServerDat) || Constants.ForceServerDat)
            {
                HookDLL = "COHook.dll";
                Core.LogWritter.Write("Using Custom Server.dat...");
                UseDecryptedServerDat = true;
            }
            if (File.Exists(PathToConquerExe))
            {
                string CheckPathEnvDX8 = Path.Combine(Application.StartupPath, "Env_DX8", SelectedServer.ExecutableName);
                string CheckPathEnvDX9 = Path.Combine(Application.StartupPath, "Env_DX9", SelectedServer.ExecutableName);
                Core.LogWritter.Write("Checking if existing path: " + CheckPathEnvDX8 + "...");
                if (File.Exists(CheckPathEnvDX8))
                {
                    Core.LogWritter.Write("Found Env_DX8 Folder. Setting the folder for run executable...");
                    PathToConquerExe = CheckPathEnvDX8;
                    WorkingDir = Path.GetDirectoryName(PathToConquerExe);
                    if (CurrentConquerProcess == null)
                    {
                        string OutputCopyDll = Path.Combine(Application.StartupPath, "Env_DX8", HookINI);
                        if (File.Exists(OutputCopyDll))
                        {
                            File.Delete(OutputCopyDll);
                        }
                        if (!AlreadyUsingLoader)
                        {
                            File.Copy(Path.Combine(Application.StartupPath, HookINI), OutputCopyDll);
                        }
                        if (SelectedServer.ServerVersion >= 6371 && SelectedServer.ServerVersion <= Constants.MaxVersionUseServerDat)
                        {
                            RebuildServerDat();
                            SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, "TQAnp.dll"), Properties.Resources.TQAnp, ex => Core.LogWritter.Write(ex.ToString()));
                            if (!CustomDLLs)
                            {
                                Core.LogWritter.Write("Generating required files for use Custom Server.dat... (Using DX8)");
                                SafeIO.TryWriteAllBytes(Path.Combine(Application.StartupPath, HookDLL), Properties.Resources.COHook6371, ex => Core.LogWritter.Write(ex.ToString()));
                            }
                        }
                    }
                    NoUseDX8_DX9 = false;
                }
                if (SelectedServer.UseDirectX9 && DX9Allowed)
                {
                    if (File.Exists(CheckPathEnvDX9))
                    {
                        Core.LogWritter.Write("Found Env_DX9 Folder. Setting the folder for run executable...");
                        PathToConquerExe = CheckPathEnvDX9;
                        WorkingDir = Path.GetDirectoryName(PathToConquerExe);
                        if (CurrentConquerProcess == null)
                        {
                            string OutputCopyDll = Path.Combine(Application.StartupPath, "Env_DX9", HookINI);
                            if (File.Exists(OutputCopyDll))
                            {
                                File.Delete(OutputCopyDll);
                            }
                            File.Copy(Path.Combine(Application.StartupPath, HookINI), OutputCopyDll);
                            if (SelectedServer.ServerVersion >= 6600)
                            {
                                RebuildServerDat();
                                SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, "TQAnp.dll"), Properties.Resources.TQAnp, ex => Core.LogWritter.Write(ex.ToString()));
                                if (!CustomDLLs)
                                {
                                    Core.LogWritter.Write("Generating required files for use Custom Server.dat... (Using DX9)");
                                    SafeIO.TryWriteAllBytes(Path.Combine(Application.StartupPath, HookDLL), Properties.Resources.COHook6371, ex => Core.LogWritter.Write(ex.ToString()));
                                }
                            }
                        }
                        NoUseDX8_DX9 = false;
                    }
                }
                if (NoUseDX8_DX9 && UseDecryptedServerDat  && SelectedServer.ServerVersion >= Constants.MinVersionUseServerDat)
                {
                    if (!CustomDLLs)
                    {
                        SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, "COFlashFixer.dll"), Properties.Resources.COFlashFixer_DLL); // Fix for flash
                        if (SelectedServer.ServerVersion >= 6176 && SelectedServer.ServerVersion <= 6370)
                        {
                            SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, HookDLL), Properties.Resources.COHook6176); // 6176 TO 6370 Hook
                        }
                        else
                        {
                            SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, HookDLL), Properties.Resources.COHook6022); // V5717 TO V6175 Hook
                        }
                        Core.LogWritter.Write("Generating required files for use Custom Server.dat...");
                    }
                    RebuildServerDat();
                }
                if (SelectedServer.ServerVersion >= Constants.MinVersionUseRAWServerDat && SelectedServer.ServerVersion <= Constants.MaxVersionUseRAWServerDat && !CustomDLLs)
                {
                    bool HookCreated = SafeIO.TryWriteAllBytes(Path.Combine(WorkingDir, HookDLL), Properties.Resources.COServerDat); // 5095 - 5716
                    Core.LogWritter.Write($"Generating required files for use Custom Server.dat... Hook Created: {HookCreated}");
                }
                Process conquerProc = Process.Start(new ProcessStartInfo() { FileName = PathToConquerExe, WorkingDirectory = WorkingDir, Arguments = "blacknull" });
                if (conquerProc != null)
                {
                    Core.LogWritter.Write("Process launched!");
                    if (CustomDLLs)
                    {
                        if (!File.Exists(Application.StartupPath + @"\" + HookDLL))
                        {
                            Core.LogWritter.Write($"Hook file not exists {HookDLL}");
                        }
                        if (!Injector.StartInjection(Application.StartupPath + @"\" + HookDLL, (uint)conquerProc.Id, worker).Injected)
                        {
                            Core.LogWritter.Write($"Injection of Custom DLL failed! [{HookDLL}] Reason: {Injector.LastStatus.ResultMessage}");
                            MetroFramework.MetroMessageBox.Show(this, $"[{SelectedServer.ServerName}] Cannot inject {HookDLL}", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            Core.LogWritter.Write($"Custom Injection of {HookDLL} successfully!");
                        }
                    }
                    worker.ReportProgress(10);
                    CurrentConquerProcess = conquerProc;
                    CurrentConquerProcess.EnableRaisingEvents = true;
                    CurrentConquerProcess.Exited += new EventHandler(ConquerProc_Exited);
                    int ConquerOpened = Process.GetProcessesByName(CurrentConquerProcess.ProcessName).Count();
                    if (Constants.EnableCLServerConnections)
                    {
                        Core.LogWritter.Write($"CLServer Enabled. Processes of Conquer opened: {ConquerOpened} (Only connect if have less or equal to 1)");
                    }
                    if (Constants.EnableCLServerConnections && ConquerOpened <= 1) // Only if not have other Conquer.exe opened
                    {
                        Core.LogWritter.Write("Connecting to CLServer");
                        // Try connect to CLServer
                        try
                        {
                            SocketSystem.CurrentSocketClient = new CLClient(SelectedServer.LoginHost, CLServerConfig.ServerPort);
                            Core.LogWritter.Write(string.Format("CLClient connected at CLServer with port {0}.", CLServerConfig.ServerPort));
                        }
                        catch (Exception ex) // Prevent break process of loader
                        {
                            Console.WriteLine("Cannot connect to CLServer: {0}", ex);
                        }
                    }
                    LoaderEvents.ConquerLaunchedStartEvent(new List<Parameter>
                    {
                        new Parameter() { Id = "ConquerProcessId", Value = CurrentConquerProcess.Id.ToString() },
                        new Parameter() { Id = "GameServerIP", Value = SelectedServer.GameHost }
                    });
                    worker.ReportProgress(20);
                    if (UseDecryptedServerDat)
                    {
                        if (SelectedServer.ServerVersion <= 6186)
                        {
                            if (!Injector.StartInjection(Application.StartupPath + @"\COFlashFixer.dll", (uint)conquerProc.Id, worker).Injected)
                            {
                                Core.LogWritter.Write($"Injection COFlashFixer failed! Reason: {Injector.LastStatus.ResultMessage}");
                                MetroFramework.MetroMessageBox.Show(this, $"[{SelectedServer.ServerName}] Cannot inject COFlashFixer.dll", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                Core.LogWritter.Write("Injected COFlashFixer successfully!");
                            }
                        }
                        if (SelectedServer.ServerVersion >= Constants.MinVersionUseServerDat)
                        {
                            if (!Injector.StartInjection(Application.StartupPath + @"\" + "ConquerCipherHook.dll", (uint)conquerProc.Id, worker).Injected)
                            {
                                Core.LogWritter.Write($"Injection ConquerCipherHook failed! Reason: {Injector.LastStatus.ResultMessage}");
                                MetroFramework.MetroMessageBox.Show(this, $"[{SelectedServer.ServerName}] Cannot inject ConquerCipherHook.dll", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                Core.LogWritter.Write("Injected ConquerCipherHook successfully!");
                            }
                        }
                        if (CustomDLLs) return;
                        if (!Injector.StartInjection(Application.StartupPath + @"\" + HookDLL, (uint)conquerProc.Id, worker).Injected)
                        {
                            Core.LogWritter.Write($"Injection {HookDLL} failed! Reason: {Injector.LastStatus.ResultMessage}");
                            MetroFramework.MetroMessageBox.Show(this, $"[{SelectedServer.ServerName}] Cannot inject {HookDLL}", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            Core.LogWritter.Write($"Injected {HookDLL} successfully!");
                        }
                    } else
                    {
                        if (CustomDLLs) return;
                        if (!Injector.StartInjection(Application.StartupPath + @"\" + HookDLL, (uint)conquerProc.Id, worker).Injected)
                        {
                            Core.LogWritter.Write($"Injection {HookDLL} failed! Reason: {Injector.LastStatus.ResultMessage}");
                            MetroFramework.MetroMessageBox.Show(this, $"[{SelectedServer.ServerName}] Cannot inject " + HookDLL, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            Core.LogWritter.Write($"Injected {HookDLL} successfully!");
                        }
                    }
                    worker.ReportProgress(100);
                }
                else
                {
                    Core.LogWritter.Write("Cannot launch " + SelectedServer.ExecutableName + "");
                    MetroFramework.MetroMessageBox.Show(this, $"[{SelectedServer.ServerName}] Cannot start {SelectedServer.ExecutableName}", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            } else
            {
                Core.LogWritter.Write("Path for Executable not found: " + PathToConquerExe);
            }
        }

        private void ConquerProc_Exited(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName(SelectedServer.ExecutableName.Replace(".exe", "")).Count() <= 0)
            {
                Environment.Exit(0);
            }
        }

        private void Worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.pBar.Value = e.ProgressPercentage;
            if (this.pBar.Value >= 100)
            {
                if (Constants.CloseOnFinish)
                {
                    LoaderEvents.LauncherExitStartEvent(new List<Parameter>() { new Parameter() { Id = "CLOSE_MESSAGE", Value = "Finished" } });
                    Environment.Exit(0);
                }
                if (Constants.HideInTrayOnFinish)
                {
                    if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1)
                    {
                        LoaderEvents.LauncherExitStartEvent(new List<Parameter>() { new Parameter() { Id = "CLOSE_MESSAGE", Value = "Finished" } });
                        Environment.Exit(0);
                    } else
                    {
                        WindowState = FormWindowState.Minimized;
                    }
                }
            }
        }

        private void BtnLogModules_Click(object sender, EventArgs e)
        {
            if (CurrentConquerProcess == null) return;
            string t = "Modules on process: " + CurrentConquerProcess.ProcessName;
            foreach (ProcessModule m in CurrentConquerProcess.Modules)
            {
                t += "ModuleName:" + m.ModuleName + Environment.NewLine + "FileName:" + m.FileName + Environment.NewLine;
            }
            Core.LogWritter.Write(t);
        }

        private void CbxServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AllStarted)
            {
                foreach (object s in cbxServers.Items)
                {
                    MetroComboBox currentSender = sender as MetroComboBox;
                    if (s.Equals(currentSender.SelectedItem.ToString()))
                    {
                        cbxServers.SelectedItem = currentSender.SelectedItem.ToString();
                        LoaderConfig.DefaultServer = LoaderConfig.Servers.Where(x => x.ServerName == currentSender.SelectedItem.ToString()).FirstOrDefault();
                        if (LoaderConfig.DefaultServer.ServerVersion == 5187)
                        {
                            if (!cbxResolutions.Items.Contains("1920x1080")) cbxResolutions.Items.Add("1920x1080");
                        } else
                        {
                            if (cbxResolutions.Items.Contains("1920x1080")) cbxResolutions.Items.Remove("1920x1080");
                        }
                        SetServerStatus();
                        Core.SaveLoaderConfig(LoaderConfig);
                    }
                }
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            Settings s = new Settings();
            s.Tag = this;
            s.ShowDialog(this);
            LoaderConfig = s.CurrentLoaderConfig;
            LoadConfigInForm();
        }

        private void BtnCloseCO_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentConquerProcess.Kill();
            } catch(Exception)
            {
            }
        }

        private void LblAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        private void CbxResolutions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResolutionChanges();
        }

        private void ResolutionChanges()
        {
            if (cbxResolutions.SelectedItem != null)
            {
                string SelectedRes = cbxResolutions.SelectedItem.ToString();
                if (SelectedRes == "800x600")
                {
                    LoaderConfig.HighResolution = false;
                }
                if (SelectedRes == "1024x768" || SelectedRes == "1920x1080")
                {
                    LoaderConfig.HighResolution = SelectedRes == "1024x768";
                    LoaderConfig.FHDResolution = SelectedRes == "1920x1080";
                }
                Core.SaveLoaderConfig(LoaderConfig);
            }
        }

        private void TglFPSUnlock_CheckedChanged(object sender, EventArgs e)
        {
            LoaderConfig.FPSUnlock = (sender as MetroToggle).Checked;
            Core.SaveLoaderConfig(LoaderConfig);
        }
    }
}

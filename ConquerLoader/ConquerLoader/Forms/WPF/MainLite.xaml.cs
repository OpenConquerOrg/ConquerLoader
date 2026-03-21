using CLCore;
using CLCore.Models;
using ConquerLoader.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;

namespace ConquerLoader.Forms.WPF
{
    public partial class MainLite : Window
    {
        public LoaderConfig LoaderConfig = null;
        public ServerConfiguration SelectedServer = null;
        public Process CurrentConquerProcess = null;
        public string LegacyHookINI = "CLHook.ini";
        public string LegacyHookDLL = "CLHook.dll";
        public bool LegacyHookEnabled = false;
        public string HookDLL = "COHook.dll";
        public bool AllStarted = false;
        public bool DX9Allowed = false;
        public bool CustomDLLs = false;
        public bool FirstRunModeEnabled = false;

        private readonly BackgroundWorker worker;
        private readonly WinForms.NotifyIcon noty;

        private string ProductNameText { get { return WinForms.Application.ProductName; } }
        private string ProductVersionText { get { return WinForms.Application.ProductVersion; } }

        public MainLite()
        {
            worker = new BackgroundWorker();
            noty = new WinForms.NotifyIcon();

            InitializeComponent();
            ApplyStaticTexts();
            noty.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;

            Loaded += MainLite_Load;
            StateChanged += TrayMinimizerForm_Resize;
            StateChanged += MainLite_StateChanged;

            LoaderConfig = Core.GetLoaderConfig();
            if (LoaderConfig == null)
            {
                LoaderConfig lc = new LoaderConfig();
                Core.SaveLoaderConfig(lc);
                LoaderConfig = Core.GetLoaderConfig();
            }

            if (Core.UseEncryptedConfig)
            {
                btnSettings.IsEnabled = false;
            }

            if (LoaderConfig != null)
            {
                Constants.CloseOnFinish = LoaderConfig.CloseOnFinish;
                CustomDLLs = LoaderConfig.UseCustomDLLs;
                SetResolutionSelectionFromConfig();
            }

            LoaderEvents.LauncherLoaded += LoaderEvents_LauncherLoaded;
            LoaderEvents.ConquerLaunched += LoaderEvents_ConquerLaunched;
            LoaderEvents.LauncherExit += LoaderEvents_LauncherExit;

            Constants.ClientPath = Directory.GetCurrentDirectory();
            Constants.MainWorker = worker;

            Core.LoadAvailablePlugins();
            Core.LoadRemotePlugins();
            Core.InitPlugins();
            DX9Allowed = Core.DirectXVersion() >= 9;
            UpdateChromeButtons();
        }

        protected override void OnClosed(EventArgs e)
        {
            noty.Visible = false;
            noty.Dispose();
            base.OnClosed(e);
        }


        private void MainLite_Load(object sender, RoutedEventArgs e)
        {
            if (LoaderConfig == null)
            {
                SettingsWindow s = new SettingsWindow();
                s.Owner = this;
                s.ShowDialog();
                LoaderConfig = Core.GetLoaderConfig();
            }

            if (LoaderConfig != null)
            {
                LoadConfigInForm();
            }

            AllStarted = true;
            RefreshServerList(true, true);
            LoaderEvents.LauncherLoadedStartEvent();

            bool installedCPlusRedistributableX64 = Core.IsVCRuntimeInstalled("x64");
            bool installedCPlusRedistributableX86 = Core.IsVCRuntimeInstalled("x86");

            if (!installedCPlusRedistributableX64 && Environment.Is64BitOperatingSystem)
            {
                File.WriteAllBytes("cpp2015-2019x64.exe", Properties.Resources.VC_redist_x64);
                Process.Start("cpp2015-2019x64.exe").WaitForExit();
                File.Delete("cpp2015-2019x64.exe");
            }
            if (!installedCPlusRedistributableX86)
            {
                File.WriteAllBytes("cpp2015-2019x86.exe", Properties.Resources.VC_redist_x86);
                Process.Start("cpp2015-2019x86.exe").WaitForExit();
                File.Delete("cpp2015-2019x86.exe");
            }

            if (LoaderConfig != null && LoaderConfig.Servers.Count <= 0)
            {
                EnableFirstRunMode();
            }

            UpdateChromeButtons();
        }

        private void MainLite_StateChanged(object sender, EventArgs e)
        {
            UpdateChromeButtons();
        }

        private void GenerateRequiredDLL()
        {
            Core.LogWritter.Write("Generating Required DLLs...");
            string pathToConquerExe = Path.Combine(WinForms.Application.StartupPath, SelectedServer.ExecutableName);
            string workingDir = Path.GetDirectoryName(pathToConquerExe);

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "CLHook.dll")))
            {
                Core.LogWritter.Write("Generated CLHook.dll");
                SafeIO.TryWriteAllBytes(Path.Combine(workingDir, "CLHook.dll"), Properties.Resources.CLHook_Legacy, ex => Core.LogWritter.Write(ex.ToString()));
            }
            else if (LegacyHookEnabled)
            {
                Core.LogWritter.Write("Using existing CLHook.dll");
            }

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ConquerCipherHook.dll")))
            {
                Core.LogWritter.Write("Generated ConquerCipherHook.dll");
                SafeIO.TryWriteAllBytes(Path.Combine(workingDir, "ConquerCipherHook.dll"), Properties.Resources.ConquerCipherHook, ex => Core.LogWritter.Write(ex.ToString()));
            }
            else
            {
                Core.LogWritter.Write("Using existing ConquerCipherHook.dll");
            }
        }

        private void TrayMinimizerForm_Resize(object sender, EventArgs e)
        {
            if (Constants.HideInTrayOnFinish)
            {
                noty.Text = ProductNameText;
                noty.Icon = Properties.Resources.ConquerLoaderLogo;

                if (WindowState == WindowState.Minimized)
                {
                    noty.Visible = true;
                    Hide();
                    noty.BalloonTipTitle = ProductNameText + " " + ProductVersionText;
                    noty.BalloonTipText = T("mainTrayBalloon", "The extensible loader for ConquerOnline");
                    noty.ShowBalloonTip(1000);
                }
                else if (WindowState == WindowState.Normal)
                {
                    noty.Visible = false;
                }
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, WinForms.MouseEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            noty.Visible = false;
            UpdateChromeButtons();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject source = e.OriginalSource as DependencyObject;
            while (source != null)
            {
                if (source is Button)
                {
                    return;
                }
                source = VisualTreeHelper.GetParent(source);
            }

            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void BtnMinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            UpdateChromeButtons();
        }

        private void BtnMaxRestoreWindow_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        private void BtnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            UpdateChromeButtons();
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
                parametersTxt += "Id=" + p.Id + ";Value=" + p.Value;
            }
            Core.LogWritter.Write("Event ConquerLaunched Fired! Parameters: " + parametersTxt);
        }

        private void LoaderEvents_LauncherExit(List<Parameter> parameters)
        {
            try
            {
                Core.LogWritter.Write("Event LauncherExit Fired!");
            }
            catch (Exception)
            {
            }
        }

        private void LoadConfigInForm()
        {
            lstServers.Items.Clear();

            if (!string.IsNullOrEmpty(LoaderConfig.Title))
            {
                Title = LoaderConfig.Title;
                txtWindowTitle.Text = LoaderConfig.Title;
                txtTopBarTitle.Text = LoaderConfig.Title;
            }
            else
            {
                Title = "ConquerLoader";
                txtWindowTitle.Text = "ConquerLoader";
                txtTopBarTitle.Text = "ConquerLoader";
            }

            ApplyStaticTexts();
            ApplyWindowTitle();
            Core.LogWritter.Write("Loaded config.json");
            btnStart.IsEnabled = LoaderConfig.Servers.Count > 0;
            tglFPSUnlock.IsChecked = LoaderConfig.FPSUnlock;
            Constants.LicenseKey = LoaderConfig.LicenseKey;
            UpdateDebugToolsVisibility();
            UpdateLauncherHints();

            foreach (ServerConfiguration server in LoaderConfig.Servers)
            {
                lstServers.Items.Add(server.ServerName);
            }

            SetResolutionSelectionFromConfig();
            RefreshServerList(true, true);
        }

        private void RefreshServerList(bool checkServerStatus, bool selectDefault)
        {
            if (LoaderConfig.Servers.Count <= 0)
            {
                EnableFirstRunMode();
                txtSelectedServer.Text = T("mainNoServerSelected", "No server selected");
                txtServerStatusHint.Text = T("mainFirstServerHint", "Open Settings to create or import your first server configuration.");
                txtHeaderDescription.Text = T("mainSetupFirstServerDescription", "Set up your first server, then choose it here to launch the client with the correct options.");
                txtActionsDescription.Text = T("mainOptionsDescription", "Choose the resolution and FPS behavior before launching.");
                btnStart.IsEnabled = false;
                UpdateStatusBadgeNeutral();
                UpdateLauncherHints();
                return;
            }

            DisableFirstRunMode();
            ApplyHeaderDescription();
            txtActionsDescription.Text = T("mainOptionsDescription", "Choose the resolution and FPS behavior before launching.");
            UpdateLauncherHints();

            if (LoaderConfig.DefaultServer == null)
            {
                LoaderConfig.DefaultServer = LoaderConfig.Servers.FirstOrDefault();
            }

            if (LoaderConfig.Servers.Count > 0)
            {
                foreach (object s in lstServers.Items)
                {
                    if (LoaderConfig.DefaultServer != null && s.Equals(LoaderConfig.DefaultServer.ServerName))
                    {
                        UpdateResolutionOptionsForServer(LoaderConfig.DefaultServer);
                        if (selectDefault)
                        {
                            lstServers.SelectedItem = s;
                            txtSelectedServer.Text = s.ToString();
                        }
                        if (checkServerStatus)
                        {
                            SetServerStatus();
                        }
                    }
                }
            }
        }

        private void SetServerStatus()
        {
            if (AllStarted && LoaderConfig != null && LoaderConfig.DefaultServer != null)
            {
                bool online = Core.ServerAvailable(LoaderConfig.DefaultServer.LoginHost, LoaderConfig.DefaultServer.GamePort);
                serverStatus.Text = online
                    ? T("mainServerStatusOnline", "ONLINE")
                    : T("mainServerStatusOffline", "OFFLINE");
                serverStatusBadge.Background = online ? CreateBrush("#15803D") : CreateBrush("#B91C1C");
                txtServerStatusHint.Text = online
                    ? T("mainSelectedServerStatusOnline", "The selected server responded to the reachability check.")
                    : T("mainSelectedServerStatusOffline", "The selected server did not respond. You can still review the configuration in Settings.");
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoaderConfig == null || lstServers.SelectedItem == null)
                {
                    return;
                }

                SelectedServer = LoaderConfig.Servers.Where(x => x.ServerName == lstServers.SelectedItem.ToString()).FirstOrDefault();
                if (SelectedServer != null)
                {
                    LoaderConfig.DefaultServer = SelectedServer;
                    Core.SaveLoaderConfig(LoaderConfig);

                    if (File.Exists(LegacyHookINI))
                    {
                        File.Delete(LegacyHookINI);
                    }

                    if (LoaderConfig.ServernameChange)
                    {
                        if (SelectedServer.ServerNameMemoryAddress == null || SelectedServer.ServerNameMemoryAddress.Length < 8)
                        {
                            if (SelectedServer.ServerVersion < 5600) SelectedServer.ServerNameMemoryAddress = "0x005726DC";
                            if (SelectedServer.ServerVersion >= 5600) SelectedServer.ServerNameMemoryAddress = "0x0097FAB8";
                            if (SelectedServer.ServerVersion >= 5700) SelectedServer.ServerNameMemoryAddress = "0x009BEA00";
                            if (SelectedServer.ServerVersion >= 6000) SelectedServer.ServerNameMemoryAddress = "0x00A56348";
                            if (SelectedServer.ServerVersion >= 6609) SelectedServer.ServerNameMemoryAddress = "0x00CE8180";
                            if (SelectedServer.ServerVersion >= 6617) SelectedServer.ServerNameMemoryAddress = "0x00CD7240";
                            if (SelectedServer.ServerVersion >= 5180 && SelectedServer.ServerVersion <= 5200) SelectedServer.ServerNameMemoryAddress = "0x0071E688";
                            Core.LogWritter.Write("Using Servername Change with Address: " + SelectedServer.ServerNameMemoryAddress);
                        }
                    }
                    else
                    {
                        SelectedServer.ServerNameMemoryAddress = "0";
                    }

                    File.WriteAllText(LegacyHookINI, "[CLHook]"
                        + Environment.NewLine + "HOST=" + SelectedServer.LoginHost
                        + Environment.NewLine + "GAMEHOST=" + SelectedServer.GameHost
                        + Environment.NewLine + "PORT=" + SelectedServer.LoginPort
                        + Environment.NewLine + "GAMEPORT=" + SelectedServer.GamePort
                        + Environment.NewLine + "SERVERNAME=" + SelectedServer.ServerName
                        + Environment.NewLine + "ENABLE_HOSTNAME=" + (SelectedServer.EnableHostName ? "1" : "0")
                        + Environment.NewLine + "HOSTNAME=" + SelectedServer.Hostname
                        + Environment.NewLine + "SERVER_VERSION=" + SelectedServer.ServerVersion
                        + Environment.NewLine + "SERVERNAME_MEMORYADDRESS=" + SelectedServer.ServerNameMemoryAddress
                        + Environment.NewLine + "DISABLE_AUTOFIX_FLASH=" + (LoaderConfig.DisableAutoFixFlash ? "1" : "0"));

                    Core.LogWritter.Write("Created the Hook Configuration");

                    if (!LoaderConfig.DisableScreenChanges)
                    {
                        Core.LogWritter.Write("Changing Screen Options...");
                        string setupIniPath = Path.Combine(Directory.GetCurrentDirectory(), "ini", "GameSetup.ini");
                        IniManager parser = new IniManager(setupIniPath, "ScreenMode");

                        parser.Write("ScreenMode", "FullScrType", LoaderConfig.FullScreen ? "0" : "1");
                        Core.LogWritter.Write("[+] Changing FullScrType to " + (LoaderConfig.FullScreen ? "0" : "1"));
                        parser.Write("ScreenMode", "ScrWidth", LoaderConfig.FHDResolution ? "1920" : LoaderConfig.HighResolution ? "1024" : "800");
                        parser.Write("ScreenMode", "ScrHeight", LoaderConfig.FHDResolution ? "1080" : LoaderConfig.HighResolution ? "768" : "600");

                        bool isCustomResolution = LoaderConfig.FHDResolution && SelectedServer.ServerVersion >= 5600;
                        if (LoaderConfig.HighResolution || LoaderConfig.FHDResolution)
                        {
                            parser.Write("ScreenMode", "ScreenModeRecord", LoaderConfig.FullScreen ? "3" : isCustomResolution ? "4" : "2");
                            Core.LogWritter.Write("[+] Changing ScreenModeRecord to " + (LoaderConfig.FullScreen ? "3" : isCustomResolution ? "4" : "2"));
                        }
                        else
                        {
                            parser.Write("ScreenMode", "ScreenModeRecord", LoaderConfig.FullScreen ? "1" : "0");
                            Core.LogWritter.Write("[+] Changing ScreenModeRecord to " + (LoaderConfig.FullScreen ? "1" : "0"));
                        }

                        Core.LogWritter.Write("[+] Changing ScrWidth to " + (LoaderConfig.FHDResolution ? "1920" : LoaderConfig.HighResolution ? "1024" : "800"));
                        Core.LogWritter.Write("[+] Changing ScrHeight to " + (LoaderConfig.FHDResolution ? "1080" : LoaderConfig.HighResolution ? "768" : "600"));
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

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Core.LogWritter.Write("Launching " + SelectedServer.ExecutableName + "...");
            string pathToConquerExe = Path.Combine(WinForms.Application.StartupPath, SelectedServer.ExecutableName);
            string workingDir = Path.GetDirectoryName(pathToConquerExe);
            bool noUseDX8DX9 = true;
            bool useDecryptedServerDat = true;
            bool alreadyUsingLoader = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1;
            if (alreadyUsingLoader) Core.LogWritter.Write("Detected already using ConquerLoader.");

            GenerateRequiredDLL();

            if (SelectedServer.ServerVersion >= Constants.MinVersionUseRAWServerDat && SelectedServer.ServerVersion <= Constants.MaxVersionUseRAWServerDat)
            {
                Core.LogWritter.Write("Using Custom Server.dat (New Raw Mode, Better performance)...");
                RebuildServerDat();
            }

            if (!File.Exists(pathToConquerExe))
            {
                Core.LogWritter.Write("Path for Executable not found: " + pathToConquerExe);
                return;
            }

            if (SelectedServer.ServerVersion >= Constants.MinVersionUseRAWServerDat && SelectedServer.ServerVersion <= Constants.MaxVersionUseRAWServerDat && !CustomDLLs)
            {
                bool hookCreated = SafeIO.TryWriteAllBytes(Path.Combine(workingDir, HookDLL), Properties.Resources.COServerDat);
                Core.LogWritter.Write("Generating required files for use Custom Server.dat... Hook Created: " + hookCreated + " [" + Path.Combine(workingDir, HookDLL) + "]");
            }

            string checkPathEnvDX8 = Path.Combine(WinForms.Application.StartupPath, "Env_DX8", SelectedServer.ExecutableName);
            string checkPathEnvDX9 = Path.Combine(WinForms.Application.StartupPath, "Env_DX9", SelectedServer.ExecutableName);
            Core.LogWritter.Write("Checking if existing path: " + checkPathEnvDX8 + "...");

            if (File.Exists(checkPathEnvDX8))
            {
                Core.LogWritter.Write("Found Env_DX8 Folder. Setting the folder for run executable...");
                pathToConquerExe = checkPathEnvDX8;
                workingDir = Path.GetDirectoryName(pathToConquerExe);
                if (CurrentConquerProcess == null)
                {
                    string outputCopyDll = Path.Combine(WinForms.Application.StartupPath, "Env_DX8", LegacyHookINI);
                    if (File.Exists(outputCopyDll)) File.Delete(outputCopyDll);
                    if (!alreadyUsingLoader) File.Copy(Path.Combine(WinForms.Application.StartupPath, LegacyHookINI), outputCopyDll);
                    if (SelectedServer.ServerVersion >= 6371 && SelectedServer.ServerVersion <= Constants.MaxVersionUseRAWServerDat)
                    {
                        RebuildServerDat();
                        SafeIO.TryWriteAllBytes(Path.Combine(workingDir, "TQAnp.dll"), Properties.Resources.TQAnp, ex => Core.LogWritter.Write(ex.ToString()));
                    }
                }
                noUseDX8DX9 = false;
            }

            if (SelectedServer.UseDirectX9 && DX9Allowed && File.Exists(checkPathEnvDX9))
            {
                Core.LogWritter.Write("Found Env_DX9 Folder. Setting the folder for run executable...");
                pathToConquerExe = checkPathEnvDX9;
                workingDir = Path.GetDirectoryName(pathToConquerExe);
                if (CurrentConquerProcess == null)
                {
                    string outputCopyDll = Path.Combine(WinForms.Application.StartupPath, "Env_DX9", LegacyHookINI);
                    if (File.Exists(outputCopyDll)) File.Delete(outputCopyDll);
                    File.Copy(Path.Combine(WinForms.Application.StartupPath, LegacyHookINI), outputCopyDll);
                    if (SelectedServer.ServerVersion >= 6600)
                    {
                        RebuildServerDat();
                        SafeIO.TryWriteAllBytes(Path.Combine(workingDir, "TQAnp.dll"), Properties.Resources.TQAnp, ex => Core.LogWritter.Write(ex.ToString()));
                    }
                }
                noUseDX8DX9 = false;
            }

            if (noUseDX8DX9 && useDecryptedServerDat && SelectedServer.ServerVersion >= Constants.MinVersionCreateFlashFix)
            {
                Core.LogWritter.Write("Generating COFlashFixer.dll...");
                bool createdFlashFix = SafeIO.TryWriteAllBytes(Path.Combine(workingDir, "COFlashFixer.dll"), Properties.Resources.COFlashFixer_DLL);
                Core.LogWritter.Write("Generating COFlashFixer.dll... [" + (createdFlashFix ? "Created" : "Failed") + "]");
                RebuildServerDat();
            }

            Process conquerProc = Process.Start(new ProcessStartInfo
            {
                FileName = pathToConquerExe,
                WorkingDirectory = workingDir,
                Arguments = "blacknull"
            });

            if (conquerProc == null)
            {
                Core.LogWritter.Write("Cannot launch " + SelectedServer.ExecutableName);
                ShowWarning("[" + SelectedServer.ServerName + "] Cannot start " + SelectedServer.ExecutableName);
                return;
            }

            Core.LogWritter.Write("Process launched!");

            if (CustomDLLs)
            {
                if (!File.Exists(WinForms.Application.StartupPath + @"\" + HookDLL))
                {
                    Core.LogWritter.Write("Hook file not exists " + HookDLL);
                }
                if (!Injector.StartInjection(WinForms.Application.StartupPath + @"\" + HookDLL, (uint)conquerProc.Id, worker).Injected)
                {
                    Core.LogWritter.Write("Injection of Custom DLL failed! [" + HookDLL + "] Reason: " + Injector.LastStatus.ResultMessage);
                    ShowWarning("[" + SelectedServer.ServerName + "] Cannot inject " + HookDLL);
                }
                else
                {
                    Core.LogWritter.Write("Custom Injection of " + HookDLL + " successfully!");
                }
            }

            worker.ReportProgress(10);
            CurrentConquerProcess = conquerProc;
            CurrentConquerProcess.EnableRaisingEvents = true;
            CurrentConquerProcess.Exited += ConquerProc_Exited;
            int conquerOpened = Process.GetProcessesByName(CurrentConquerProcess.ProcessName).Count();

            if (Constants.EnableCLServerConnections)
            {
                Core.LogWritter.Write("CLServer Enabled. Processes of Conquer opened: " + conquerOpened + " (Only connect if have less or equal to 1)");
            }

            if (Constants.EnableCLServerConnections && conquerOpened <= 1)
            {
                Core.LogWritter.Write("Connecting to CLServer");
                try
                {
                    SocketSystem.CurrentSocketClient = new CLClient(SelectedServer.LoginHost, CLServerConfig.ServerPort);
                    Core.LogWritter.Write(string.Format("CLClient connected at CLServer with port {0}.", CLServerConfig.ServerPort));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot connect to CLServer: {0}", ex);
                }
            }

            LoaderEvents.ConquerLaunchedStartEvent(new List<Parameter>
            {
                new Parameter { Id = "ConquerProcessId", Value = CurrentConquerProcess.Id.ToString() },
                new Parameter { Id = "GameServerIP", Value = SelectedServer.GameHost }
            });

            worker.ReportProgress(20);

            if (useDecryptedServerDat)
            {
                if (SelectedServer.ServerVersion <= 6186)
                {
                    if (!Injector.StartInjection(WinForms.Application.StartupPath + @"\COFlashFixer.dll", (uint)conquerProc.Id, worker).Injected)
                    {
                        Core.LogWritter.Write("Injection COFlashFixer failed! Reason: " + Injector.LastStatus.ResultMessage);
                        ShowWarning("[" + SelectedServer.ServerName + "] Cannot inject COFlashFixer.dll");
                    }
                    else
                    {
                        Core.LogWritter.Write("Injected COFlashFixer successfully!");
                    }
                }

                if (SelectedServer.ServerVersion >= Constants.MinVersionUseRAWServerDat)
                {
                    if (!Injector.StartInjection(WinForms.Application.StartupPath + @"\ConquerCipherHook.dll", (uint)conquerProc.Id, worker).Injected)
                    {
                        Core.LogWritter.Write("Injection ConquerCipherHook failed! Reason: " + Injector.LastStatus.ResultMessage);
                        ShowWarning("[" + SelectedServer.ServerName + "] Cannot inject ConquerCipherHook.dll");
                    }
                    else
                    {
                        Core.LogWritter.Write("Injected ConquerCipherHook successfully!");
                    }
                }

                if (CustomDLLs) return;

                if (!Injector.StartInjection(WinForms.Application.StartupPath + @"\" + HookDLL, (uint)conquerProc.Id, worker).Injected)
                {
                    Core.LogWritter.Write("Injection " + HookDLL + " failed! Reason: " + Injector.LastStatus.ResultMessage);
                    ShowWarning("[" + SelectedServer.ServerName + "] Cannot inject " + HookDLL);
                }
                else
                {
                    Core.LogWritter.Write("Injected " + HookDLL + " successfully!");
                }

                if (LegacyHookEnabled)
                {
                    if (!Injector.StartInjection(WinForms.Application.StartupPath + @"\" + LegacyHookDLL, (uint)conquerProc.Id, worker).Injected)
                    {
                        Core.LogWritter.Write("Injection " + LegacyHookDLL + " failed! Reason: " + Injector.LastStatus.ResultMessage);
                        ShowWarning("[" + SelectedServer.ServerName + "] Cannot inject " + LegacyHookDLL);
                    }
                    else
                    {
                        Core.LogWritter.Write("Injected " + LegacyHookDLL + " successfully!");
                    }
                }
            }
            else
            {
                if (CustomDLLs) return;

                if (!Injector.StartInjection(WinForms.Application.StartupPath + @"\" + HookDLL, (uint)conquerProc.Id, worker).Injected)
                {
                    Core.LogWritter.Write("Injection " + HookDLL + " failed! Reason: " + Injector.LastStatus.ResultMessage);
                    ShowWarning("[" + SelectedServer.ServerName + "] Cannot inject " + HookDLL);
                }
                else
                {
                    Core.LogWritter.Write("Injected " + HookDLL + " successfully!");
                }
            }

            worker.ReportProgress(100);
        }

        private void ConquerProc_Exited(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName(SelectedServer.ExecutableName.Replace(".exe", "")).Count() <= 0)
            {
                Environment.Exit(0);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pBar.Value = e.ProgressPercentage;
            txtProgressValue.Text = e.ProgressPercentage + "%";
            if (pBar.Value >= 100)
            {
                if (Constants.CloseOnFinish)
                {
                    LoaderEvents.LauncherExitStartEvent(new List<Parameter> { new Parameter { Id = "CLOSE_MESSAGE", Value = "Finished" } });
                    Environment.Exit(0);
                }
                if (Constants.HideInTrayOnFinish)
                {
                    if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1)
                    {
                        LoaderEvents.LauncherExitStartEvent(new List<Parameter> { new Parameter { Id = "CLOSE_MESSAGE", Value = "Finished" } });
                        Environment.Exit(0);
                    }
                    else
                    {
                        WindowState = WindowState.Minimized;
                    }
                }
            }
        }

        private void BtnLogModules_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentConquerProcess == null) return;

            string text = "Modules on process: " + CurrentConquerProcess.ProcessName;
            foreach (ProcessModule m in CurrentConquerProcess.Modules)
            {
                text += "ModuleName:" + m.ModuleName + Environment.NewLine + "FileName:" + m.FileName + Environment.NewLine;
            }
            Core.LogWritter.Write(text);
        }

        private void CbxServers_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllStarted && lstServers.SelectedItem != null)
            {
                txtSelectedServer.Text = lstServers.SelectedItem.ToString();
                foreach (object s in lstServers.Items)
                {
                    if (s.Equals(lstServers.SelectedItem.ToString()))
                    {
                        LoaderConfig.DefaultServer = LoaderConfig.Servers.Where(x => x.ServerName == lstServers.SelectedItem.ToString()).FirstOrDefault();
                        UpdateResolutionOptionsForServer(LoaderConfig.DefaultServer);
                        SetServerStatus();
                        Core.SaveLoaderConfig(LoaderConfig);
                    }
                }
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow s = new SettingsWindow();
            s.Owner = this;
            s.ShowDialog();
            LoaderConfig = s.CurrentLoaderConfig;
            if (LoaderConfig == null) LoaderConfig = Core.GetLoaderConfig();
            if (LoaderConfig != null) LoadConfigInForm();
        }

        public void ReloadLocalizedUi()
        {
            LoaderConfig = Core.GetLoaderConfig() ?? LoaderConfig;
            if (LoaderConfig != null)
            {
                LoadConfigInForm();
            }
            else
            {
                ApplyStaticTexts();
            }
        }

        private void BtnCreateServer_Click(object sender, RoutedEventArgs e)
        {
            WizardWindow wizard = new WizardWindow();
            wizard.Owner = this;
            wizard.ShowDialog();

            LoaderConfig = Core.GetLoaderConfig();
            if (LoaderConfig != null)
            {
                LoadConfigInForm();
            }
        }

        private void BtnStartFirstRun_Click(object sender, RoutedEventArgs e)
        {
            BtnCreateServer_Click(sender, e);
        }

        private void BtnCloseCO_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentConquerProcess != null) CurrentConquerProcess.Kill();
            }
            catch (Exception)
            {
            }
        }

        private void LblAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow ab = new AboutWindow();
            ab.Owner = this;
            ab.ShowDialog();
        }

        private void CbxResolutions_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            ResolutionChanges();
        }

        private void ResolutionChanges()
        {
            if (cbxResolutions.SelectedItem != null && LoaderConfig != null)
            {
                string selectedRes = cbxResolutions.SelectedItem.ToString();
                if (selectedRes == "800x600")
                {
                    LoaderConfig.HighResolution = false;
                    LoaderConfig.FHDResolution = false;
                }
                if (selectedRes == "1024x768" || selectedRes == "1920x1080")
                {
                    LoaderConfig.HighResolution = selectedRes == "1024x768";
                    LoaderConfig.FHDResolution = selectedRes == "1920x1080";
                }
                Core.SaveLoaderConfig(LoaderConfig);
                txtLaunchTip.Text = TF("mainResolutionSelected", "Selected resolution: {0}. Review fullscreen and FPS options before launching.", selectedRes);
            }
        }

        private void TglFPSUnlock_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (LoaderConfig != null)
            {
                LoaderConfig.FPSUnlock = tglFPSUnlock.IsChecked == true;
                Core.SaveLoaderConfig(LoaderConfig);
            }
        }

        private void SetResolutionSelectionFromConfig()
        {
            if (LoaderConfig == null) return;

            if (LoaderConfig.FHDResolution && !cbxResolutions.Items.Contains("1920x1080"))
            {
                cbxResolutions.Items.Add("1920x1080");
            }

            if (LoaderConfig.HighResolution)
            {
                cbxResolutions.SelectedItem = "1024x768";
            }
            else if (LoaderConfig.FHDResolution)
            {
                cbxResolutions.SelectedItem = "1920x1080";
            }
            else
            {
                cbxResolutions.SelectedItem = "800x600";
            }
        }

        private void UpdateResolutionOptionsForServer(ServerConfiguration server)
        {
            if (server == null) return;

            bool supportsFhd = server.ServerVersion == 5187 || server.ServerVersion >= 5600;
            bool containsFhd = cbxResolutions.Items.Contains("1920x1080");

            if (supportsFhd && !containsFhd)
            {
                cbxResolutions.Items.Add("1920x1080");
            }
            else if (!supportsFhd && containsFhd)
            {
                if (Equals(cbxResolutions.SelectedItem, "1920x1080"))
                {
                    cbxResolutions.SelectedItem = "1024x768";
                }
                cbxResolutions.Items.Remove("1920x1080");
            }
        }

        private void ApplyStaticTexts()
        {
            txtServerSelectionTitle.Text = T("mainServerSelection", "Server Selection");
            txtAvailableServers.Text = T("mainAvailableServers", "Available servers");
            txtLauncherNoteTitle.Text = T("mainLauncherNote", "Launcher note");
            txtEnterLabel.Text = T("mainEnterLabel", "ENTER THE REALM");
            txtOptionsTitle.Text = T("mainOptionsTitle", "Options");
            txtResolutionLabel.Text = T("mainResolution", "Resolution");
            txtFirstRunBadge.Text = T("mainFirstTimeSetup", "First-time setup");
            txtHelper.Text = T("mainHelperDefault", "Pick the server you want to launch. Your last realm stays selected for the next session.");
            txtLaunchTip.Text = T("mainLaunchTipDefault", "Choose a server first so the launch status and resolution stay in sync.");
            ApplyHeaderDescription();
            txtActionsDescription.Text = T("mainOptionsDescription", "Choose the resolution and FPS behavior before launching.");
            txtFpsUnlockHint.Text = T("mainFpsUnlockHint", "Use this only when your client and server support it.");
            ApplyTranslation("btnStartFirstRun", btnStartFirstRun, "Create My First Server");
            ApplyTranslation("btnSettings", btnOpenSettingsFromOverlay, "Open Settings");
            ApplyTranslation("lblAbout", btnAboutFromOverlay, "About");
            txtFirstRunTitle.Text = T("mainFirstRunTitle", "Let's create your first server");
            txtFirstRunDescription.Text = T("mainFirstRunDescription", "ConquerLoader needs at least one server before it can launch the game. The guided wizard will walk you through the setup step by step.");
            txtFirstRunStep1.Text = T("mainFirstRunStep1", "1. Choose a name for the server so you can recognize it later.");
            txtFirstRunStep2.Text = T("mainFirstRunStep2", "2. Enter the server IP, client version, and executable in the wizard.");
            txtFirstRunStep3.Text = T("mainFirstRunStep3", "3. Save the server and come back here to launch the game.");
            ApplyTranslation("btnStart", btnStart, "ENTER");
            ApplyTranslation("btnSettings", btnSettings, "Settings");
            ApplyTranslation("btnCloseCO", btnCloseCO, "Close Game Process");
            ApplyTranslation("btnLogModules", btnLogModules, "Log Modules");
            ApplyTranslation("lblFPSUnlock", lblFPSUnlock, "Unlock FPS");
            ApplyTranslation("lblAbout", lblAbout, "About");
            ApplyTranslation("commonEnabled", tglFPSUnlock, "Enabled");
            ApplyTranslation("mainDiagnosticsTitle", txtDiagnosticsTitle, "Diagnostics");
            ApplyTranslation("mainDiagnosticsDescription", txtDiagnosticsDescription, "These debug actions are only shown when Debug Mode is enabled in Settings.");
            noty.Visible = true;
            txtProgressValue.Text = "0%";
            UpdateStatusBadgeNeutral();
            txtServerStatusHint.Text = T("mainSelectedServerHint", "The selected server will be checked before launch.");
            txtSelectedServer.Text = LoaderConfig != null && LoaderConfig.DefaultServer != null
                ? LoaderConfig.DefaultServer.ServerName
                : T("mainNoServerSelected", "No server selected");
            UpdateDebugToolsVisibility();
            UpdateLauncherHints();
        }

        private void ApplyWindowTitle()
        {
            if (LoaderConfig != null && !string.IsNullOrEmpty(LoaderConfig.Title))
            {
                Title = LoaderConfig.Title;
                txtWindowTitle.Text = LoaderConfig.Title;
                txtTopBarTitle.Text = LoaderConfig.Title;
            }
            else
            {
                Title = "ConquerLoader";
                txtWindowTitle.Text = "ConquerLoader";
                txtTopBarTitle.Text = "ConquerLoader";
            }
        }

        private void ApplyHeaderDescription()
        {
            string fallback = T("mainHeaderDescription", "Choose your realm, set the launch options, and enter from a single launcher screen.");

            if (LoaderConfig != null && !string.IsNullOrWhiteSpace(LoaderConfig.Description))
            {
                txtHeaderDescription.Text = LoaderConfig.Description.Trim();
            }
            else
            {
                txtHeaderDescription.Text = fallback;
            }
        }

        private void ApplyTranslation(string key, ContentControl control, string fallback)
        {
            control.Content = Core.TranslateText(key, fallback);
        }

        private void ApplyTranslation(string key, TextBlock control, string fallback)
        {
            control.Text = Core.TranslateText(key, fallback);
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }

        private string TF(string key, string fallback, params object[] args)
        {
            return string.Format(T(key, fallback), args);
        }

        private void UpdateStatusBadgeNeutral()
        {
            serverStatus.Text = T("mainServerStatusNeutral", "-");
            serverStatusBadge.Background = CreateBrush("#94A3B8");
        }

        private void UpdateChromeButtons()
        {
            if (btnMaxRestoreWindow == null)
            {
                return;
            }

            btnMinimizeWindow.Content = "\uE921";
            btnMinimizeWindow.ToolTip = T("chromeMinimize", "Minimize");
            btnCloseWindow.Content = "\uE8BB";
            btnCloseWindow.Tag = "Close";
            btnCloseWindow.ToolTip = T("chromeClose", "Close");
            btnMaxRestoreWindow.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
            btnMaxRestoreWindow.ToolTip = WindowState == WindowState.Maximized
                ? T("chromeRestore", "Restore")
                : T("chromeMaximize", "Maximize");
        }

        private void UpdateDebugToolsVisibility()
        {
            if (diagnosticsCard == null)
            {
                return;
            }

            bool showDiagnostics = LoaderConfig != null && LoaderConfig.DebugMode;
            diagnosticsCard.Visibility = showDiagnostics ? Visibility.Visible : Visibility.Collapsed;
            btnLogModules.IsEnabled = showDiagnostics;
            btnLogModules.Visibility = showDiagnostics ? Visibility.Visible : Visibility.Collapsed;
            btnCloseCO.IsEnabled = showDiagnostics;
            btnCloseCO.Visibility = showDiagnostics ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateLauncherHints()
        {
            if (LoaderConfig != null && LoaderConfig.Servers.Count <= 0)
            {
                txtHelper.Text = T("mainHelperFirstRun", "Create your first server, then come back here to launch it.");
                txtLaunchTip.Text = T("mainLaunchTipFirstRun", "The guided wizard will help you configure the server step by step.");
            }
            else
            {
                txtHelper.Text = T("mainHelperDefault", "Pick the server you want to launch. Your last realm stays selected for the next session.");
                txtLaunchTip.Text = T("mainLaunchTipDefault", "Choose a server first so the launch status and resolution stay in sync.");
            }
        }

        private void EnableFirstRunMode()
        {
            FirstRunModeEnabled = true;
            if (firstRunOverlay != null)
            {
                firstRunOverlay.Visibility = Visibility.Visible;
            }
        }

        private void DisableFirstRunMode()
        {
            FirstRunModeEnabled = false;
            if (firstRunOverlay != null)
            {
                firstRunOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowWarning(string text)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(text, ProductNameText, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        private SolidColorBrush CreateBrush(string color)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
    }
}


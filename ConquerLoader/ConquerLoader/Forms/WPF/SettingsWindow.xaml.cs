using CLCore.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConquerLoader.Forms.WPF
{
    public partial class SettingsWindow : Window
    {
        public LoaderConfig CurrentLoaderConfig = null;

        public SettingsWindow()
        {
            InitializeComponent();
            Loaded += SettingsWindow_Loaded;
            StateChanged += SettingsWindow_StateChanged;
            UpdateChromeButtons();
            if (Debugger.IsAttached)
            {
                btnSetupLicense.Visibility = Visibility.Visible;
            }
        }

        public void SetDefaultUI()
        {
            if (CurrentLoaderConfig == null) return;

            tglDebugMode.IsChecked = CurrentLoaderConfig.DebugMode;
            tglCloseOnFinish.IsChecked = CurrentLoaderConfig.CloseOnFinish;
            tglHighResolution.IsChecked = CurrentLoaderConfig.HighResolution;
            tglFullscreen.IsChecked = CurrentLoaderConfig.FullScreen;
            tglDisableAutoFixFlash.IsChecked = CurrentLoaderConfig.DisableAutoFixFlash;
            tglDisableScreenChanges.IsChecked = CurrentLoaderConfig.DisableScreenChanges;
            tglUseCustomDLLs.IsChecked = CurrentLoaderConfig.UseCustomDLLs;
            tglFPSUnlock.IsChecked = CurrentLoaderConfig.FPSUnlock;
            tbxTitle.Text = CurrentLoaderConfig.Title ?? string.Empty;
            tbxDescription.Text = CurrentLoaderConfig.Description ?? string.Empty;
            RefreshServersGrid();

            if (CurrentLoaderConfig.FHDResolution)
            {
                tglHighResolution.IsChecked = false;
            }

            UpdateImportantInfo();
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                tbxTitle.ToolTip = Owner.Title;
            }

            langSelector.Items.Add("English");
            langSelector.Items.Add("Espanol");
            langSelector.Items.Add("Portugues");

            CurrentLoaderConfig = Core.GetLoaderConfig();

            if (!File.Exists(Core.ConfigJsonPath) && !File.Exists(Core.ConfigJsonPath + ".lock"))
            {
                LoaderConfig lc = new LoaderConfig();
                Core.SaveLoaderConfig(lc);
                MessageBox.Show(T("settingsCreateConfigWarning", "Cannot load config.json. Creating one... Restart App!"), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }

            if (CurrentLoaderConfig != null)
            {
                string findStr = "English";
                if (CurrentLoaderConfig.Lang == "es")
                {
                    findStr = "Espanol";
                }
                if (CurrentLoaderConfig.Lang == "pt")
                {
                    findStr = "Portugues";
                }
                langSelector.SelectedIndex = langSelector.Items.IndexOf(findStr);
                SetDefaultUI();
                ApplyTranslations();
            }
        }

        private void RefreshServersGrid()
        {
            gridViewSettings.ItemsSource = null;
            gridViewSettings.ItemsSource = CurrentLoaderConfig != null ? CurrentLoaderConfig.Servers : null;
            RefreshServerColumnHeaders();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Core.SaveLoaderConfig(CurrentLoaderConfig);
            DialogResult = true;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsWindow_StateChanged(object sender, EventArgs e)
        {
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

        private void TglDebugMode_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig != null) CurrentLoaderConfig.DebugMode = tglDebugMode.IsChecked == true;
        }

        private void TglCloseOnFinish_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig != null) CurrentLoaderConfig.CloseOnFinish = tglCloseOnFinish.IsChecked == true;
        }

        private void TbxTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentLoaderConfig != null)
            {
                CurrentLoaderConfig.Title = tbxTitle.Text;
            }
        }

        private void TbxDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentLoaderConfig != null)
            {
                CurrentLoaderConfig.Description = tbxDescription.Text;
            }
        }

        private void TglHighResolution_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig != null) CurrentLoaderConfig.HighResolution = tglHighResolution.IsChecked == true;
        }

        private void TglFullscreen_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig != null) CurrentLoaderConfig.FullScreen = tglFullscreen.IsChecked == true;
        }

        private void TglDisableAutoFixFlash_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig != null) CurrentLoaderConfig.DisableAutoFixFlash = tglDisableAutoFixFlash.IsChecked == true;
        }

        private void TglDisableScreenChanges_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig == null) return;

            CurrentLoaderConfig.DisableScreenChanges = tglDisableScreenChanges.IsChecked == true;
            if (CurrentLoaderConfig.DisableScreenChanges)
            {
                tglFullscreen.IsChecked = false;
                tglFullscreen.IsEnabled = false;
                tglHighResolution.IsChecked = false;
                tglHighResolution.IsEnabled = false;
            }
            else
            {
                tglFullscreen.IsEnabled = true;
                tglHighResolution.IsEnabled = true;
            }
        }

        private void TglUseCustomDLLs_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig == null) return;

            CurrentLoaderConfig.UseCustomDLLs = tglUseCustomDLLs.IsChecked == true;
            if (CurrentLoaderConfig.UseCustomDLLs)
            {
                MessageBox.Show(T("settingsCustomDllWarning", "To use a custom DLL, place a file called COHook.dll next to the loader so it can be injected automatically."), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TglFPSUnlock_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentLoaderConfig != null)
            {
                CurrentLoaderConfig.FPSUnlock = tglFPSUnlock.IsChecked == true;
                UpdateImportantInfo();
            }
        }

        private void UpdateImportantInfo()
        {
            bool visible = CurrentLoaderConfig != null && CurrentLoaderConfig.FPSUnlock;
            importantInfoCard.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (visible)
            {
                lblImportantInfo.Text = T("settingsFpsWarning", "FPS Unlock may cause instability in some systems, for example with old graphic cards. Use at your own risk.");
            }
        }

        private void BtnWizard_Click(object sender, RoutedEventArgs e)
        {
            WizardWindow wizardForm = new WizardWindow();
            wizardForm.Owner = this;
            wizardForm.ShowDialog();
            CurrentLoaderConfig = Core.GetLoaderConfig();
            RefreshServersGrid();
        }

        private void BtnPlugins_Click(object sender, RoutedEventArgs e)
        {
            PluginsWindow pluginsForm = new PluginsWindow();
            pluginsForm.Owner = this;
            pluginsForm.ShowDialog();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (gridViewSettings.SelectedIndex < 0) return;

            if (Core.GetLoaderConfig().Servers.Count > gridViewSettings.SelectedIndex)
            {
                WizardWindow wizardEditForm = new WizardWindow(gridViewSettings.SelectedIndex);
                wizardEditForm.Owner = this;
                wizardEditForm.ShowDialog();
                CurrentLoaderConfig = Core.GetLoaderConfig();
                RefreshServersGrid();
            }
        }

        private void BtnServerDat_Click(object sender, RoutedEventArgs e)
        {
            ServerDatManagerWindow serverDatForm = new ServerDatManagerWindow();
            serverDatForm.Owner = this;
            serverDatForm.ShowDialog();
        }

        private void LangSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentLoaderConfig == null || langSelector.SelectedIndex < 0) return;

            switch (langSelector.SelectedIndex)
            {
                case 0:
                    CurrentLoaderConfig.Lang = "en";
                    break;
                case 1:
                    CurrentLoaderConfig.Lang = "es";
                    break;
                case 2:
                    CurrentLoaderConfig.Lang = "pt";
                    break;
                default:
                    CurrentLoaderConfig.Lang = "en";
                    break;
            }

            Core.DetectLang(CurrentLoaderConfig);
            ApplyTranslations();
            if (Owner is MainLite mainLite)
            {
                mainLite.ReloadLocalizedUi();
            }
            Core.SaveLoaderConfig(CurrentLoaderConfig);
        }

        private void ApplyTranslations()
        {
            if (btnSave == null)
            {
                return;
            }

            btnClose.Content = Core.TranslateText("btnClose", "Close");
            btnSave.Content = Core.TranslateText("btnSave", "Save");
            btnLockConfig.Content = Core.TranslateText("btnLockConfig", "Lock Config");
            btnSetupLicense.Content = Core.TranslateText("btnSetupLicense", "Setup License");
            btnServerDat.Content = Core.TranslateText("btnServerDat", "Server.dat");
            btnPlugins.Content = Core.TranslateText("btnPlugins", "Plugins");
            btnWizard.Content = Core.TranslateText("btnWizard", "New +");
            btnEdit.Content = Core.TranslateText("btnEdit", "Edit");

            tglDebugMode.Content = Core.TranslateText("lblDebugMode", "Debug Mode");
            tglCloseOnFinish.Content = Core.TranslateText("lblCloseOnFinish", "Close On Finish");
            tglHighResolution.Content = Core.TranslateText("lblHighResolution", "High Resolution Mode");
            tglFullscreen.Content = Core.TranslateText("lblFullscreen", "Fullscreen");
            tglDisableAutoFixFlash.Content = Core.TranslateText("lblDisableAutoFixFlash", "Disable AutoFix Flash");
            tglDisableScreenChanges.Content = Core.TranslateText("lblDisableScreenChanges", "Disable Screen Changes");
            tglUseCustomDLLs.Content = Core.TranslateText("lblUseCustomDLLs", "Use Custom DLLs");
            tglFPSUnlock.Content = Core.TranslateText("lblFPSUnlock", "FPS Unlock");
            Title = T("settingsWindowTitle", "Settings");
            txtWindowTitleBar.Text = T("settingsWindowTitle", "Settings");
            txtWindowHeading.Text = T("settingsWindowTitle", "Settings");
            txtWindowDescription.Text = T("settingsWindowDescription", "Configure launcher behavior, language, and server management from one place.");
            txtLauncherHeading.Text = T("settingsLauncherTitle", "Launcher");
            txtLauncherDescription.Text = T("settingsLauncherDescription", "Core behavior and UI preferences for the loader.");
            txtLoaderTitleLabel.Text = T("lblTitle", "Title in Loader");
            txtLoaderDescriptionLabel.Text = T("settingsLoaderDescriptionLabel", "Description in Loader");
            txtAdvancedHeading.Text = T("settingsAdvancedTitle", "Advanced");
            txtAdvancedDescription.Text = T("settingsAdvancedDescription", "These actions affect persistence and debugging tools.");
            txtServersHeading.Text = T("settingsServersTitle", "Servers");
            txtServersDescription.Text = T("settingsServersDescription", "Create, edit, and review the servers available in the launcher.");
            txtWizardHint.Text = T("settingsWizardHint", "Use New + to open the guided server setup.");
            UpdateImportantInfo();
            RefreshServerColumnHeaders();
        }

        private void GridViewSettings_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = TranslateServerColumnHeader(e.PropertyName);
        }

        private void RefreshServerColumnHeaders()
        {
            if (gridViewSettings == null)
            {
                return;
            }

            foreach (DataGridColumn column in gridViewSettings.Columns)
            {
                string propertyName = column.SortMemberPath;
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    propertyName = column.Header != null ? column.Header.ToString() : string.Empty;
                }

                column.Header = TranslateServerColumnHeader(propertyName);
            }
        }

        private string TranslateServerColumnHeader(string propertyName)
        {
            switch (propertyName)
            {
                case "ServerName":
                    return T("settingsColumnServerName", "Server name");
                case "ServerVersion":
                    return T("settingsColumnServerVersion", "Client version");
                case "LoginHost":
                    return T("settingsColumnLoginHost", "Login host");
                case "GameHost":
                    return T("settingsColumnGameHost", "Game host");
                case "LoginPort":
                    return T("settingsColumnLoginPort", "Login port");
                case "GamePort":
                    return T("settingsColumnGamePort", "Game port");
                case "ExecutableName":
                    return T("settingsColumnExecutableName", "Executable");
                case "EnableHostName":
                    return T("settingsColumnEnableHostName", "Use hostname");
                case "UseDirectX9":
                    return T("settingsColumnUseDirectX9", "DirectX9");
                case "Hostname":
                    return T("settingsColumnHostname", "Hostname");
                case "ServerNameMemoryAddress":
                    return T("settingsColumnServerNameMemoryAddress", "Server name address");
                case "ServerIcon":
                    return T("settingsColumnServerIcon", "Server icon");
                case "Group":
                    return T("settingsColumnGroup", "Group");
                default:
                    return propertyName;
            }
        }

        private void BtnLockConfig_Click(object sender, RoutedEventArgs e)
        {
            Core.UseEncryptedConfig = true;
            Core.SaveLoaderConfig(CurrentLoaderConfig);
            File.Delete(Core.ConfigJsonPath);
            Application.Current.Shutdown();
        }

        private void BtnSetupLicense_Click(object sender, RoutedEventArgs e)
        {
            SetupLicenseWindow setupLic = new SetupLicenseWindow();
            setupLic.Owner = this;
            setupLic.ShowDialog();
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }
    }
}

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

        private WizardWindow WizardForm = null;
        private WizardWindow WizardEditForm = null;
        private ServerDatManagerWindow ServerDatForm = null;
        private PluginsWindow PluginsForm = null;

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
                MessageBox.Show("Cannot load config.json. Creating one... Restart App!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
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
            }
        }

        private void RefreshServersGrid()
        {
            gridViewSettings.ItemsSource = null;
            gridViewSettings.ItemsSource = CurrentLoaderConfig != null ? CurrentLoaderConfig.Servers : null;
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
            btnMinimizeWindow.ToolTip = "Minimize";
            btnCloseWindow.Content = "\uE8BB";
            btnCloseWindow.Tag = "Close";
            btnCloseWindow.ToolTip = "Close";
            btnMaxRestoreWindow.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
            btnMaxRestoreWindow.ToolTip = WindowState == WindowState.Maximized ? "Restore" : "Maximize";
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
                MessageBox.Show("For use Custom DLL can put a DLL called COHook.dll for be auto-injected like native lib of loader.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                lblImportantInfo.Text = "FPS Unlock may cause instability in some systems, for example with old graphic cards. Use at your own risk.";
            }
        }

        private void BtnWizard_Click(object sender, RoutedEventArgs e)
        {
            if (WizardForm == null)
            {
                WizardForm = new WizardWindow();
            }
            WizardForm.Owner = this;
            WizardForm.ShowDialog();
            CurrentLoaderConfig = Core.GetLoaderConfig();
            RefreshServersGrid();
        }

        private void BtnPlugins_Click(object sender, RoutedEventArgs e)
        {
            if (PluginsForm == null)
            {
                PluginsForm = new PluginsWindow();
            }
            PluginsForm.Owner = this;
            PluginsForm.ShowDialog();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (gridViewSettings.SelectedIndex < 0) return;

            if (WizardEditForm != null)
            {
                WizardEditForm.Close();
            }

            if (Core.GetLoaderConfig().Servers.Count > gridViewSettings.SelectedIndex)
            {
                WizardEditForm = new WizardWindow(gridViewSettings.SelectedIndex);
                WizardEditForm.Owner = this;
                WizardEditForm.ShowDialog();
                CurrentLoaderConfig = Core.GetLoaderConfig();
                RefreshServersGrid();
            }
        }

        private void BtnServerDat_Click(object sender, RoutedEventArgs e)
        {
            if (ServerDatForm != null)
            {
                ServerDatForm.Close();
            }
            ServerDatForm = new ServerDatManagerWindow();
            ServerDatForm.Owner = this;
            ServerDatForm.ShowDialog();
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
            Core.SaveLoaderConfig(CurrentLoaderConfig);
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
    }
}

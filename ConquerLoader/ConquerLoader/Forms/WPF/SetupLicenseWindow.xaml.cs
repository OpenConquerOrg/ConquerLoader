using CLCore.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConquerLoader.Forms.WPF
{
    public partial class SetupLicenseWindow : Window
    {
        public LoaderConfig Config = Core.GetLoaderConfig();

        public SetupLicenseWindow()
        {
            InitializeComponent();
            tbxLicenseKey.Text = Config != null ? Config.LicenseKey : string.Empty;
            StateChanged += SetupLicenseWindow_StateChanged;
            UpdateChromeButtons();
            ApplyTranslations();
        }

        private void BtnSetup_Click(object sender, RoutedEventArgs e)
        {
            if (Config == null)
            {
                Config = Core.GetLoaderConfig() ?? new LoaderConfig();
            }

            Config.LicenseKey = tbxLicenseKey.Text;
            Core.SaveLoaderConfig(Config);
            MessageBox.Show(T("setupLicenseSavedMessage", "License saved. Restart the loader to refresh available plugins."), Title, MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetupLicenseWindow_StateChanged(object sender, System.EventArgs e)
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

        private void ApplyTranslations()
        {
            Title = T("setupLicenseWindowTitle", "Setup License");
            txtWindowTitleBar.Text = T("setupLicenseWindowTitle", "Setup License");
            txtLicenseHeading.Text = T("setupLicenseHeading", "License setup");
            txtLicenseDescription.Text = T("setupLicenseDescription", "Paste your license key below to unlock premium plugins and remote modules.");
            txtLicenseKeyLabel.Text = T("setupLicenseKeyLabel", "License key");
            txtLicenseExample.Text = T("setupLicenseExample", "Example: 2d32a164-fcd5-796b-b43f-005a78274cad");
            btnClose.Content = Core.TranslateText("btnClose", "Close");
            btnSetup.Content = T("btnSetupLicenseSave", "Save License");
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }
    }
}

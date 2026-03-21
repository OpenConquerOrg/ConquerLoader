using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ConquerLoader.Forms.WPF
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Loaded += AboutWindow_Loaded;
            StateChanged += AboutWindow_StateChanged;
            UpdateChromeButtons();
        }

        private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            lblAboutVersionN.Text = "v" + fvi.ProductVersion;
            ApplyTranslations();
        }

        private void BtnChangelog_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://conquerloader.com/changelog/");
        }

        private void BtnWebsite_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://conquerloader.com/");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void BrandCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://conquerloader.com/");
        }

        private static void OpenUrl(string url)
        {
            Process.Start(url);
        }

        private void AboutWindow_StateChanged(object sender, System.EventArgs e)
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
            Title = T("aboutWindowTitle", "About");
            txtWindowTitleBar.Text = T("aboutWindowTitle", "About");
            txtAboutHeading.Text = T("aboutHeading", "About ConquerLoader");
            txtAboutDescription.Text = T("aboutDescription", "A lightweight launcher with plugin support and server management for Conquer Online.");
            txtAboutCreatedBy.Text = T("aboutCreatedBy", "Created by Cristian Ocana Soler (DaRkFoxDeveloper)");
            txtAboutPortuguese.Text = T("aboutPortuguese", "Portuguese translation by Louan Fontenele");
            txtAboutTesters.Text = T("aboutTesters", "Testers: Robert Frias, Pezzi Tomas");
            txtAboutVersionLabel.Text = T("aboutVersionLabel", "Version:");
            txtAboutLinks.Text = T("aboutLinks", "Links");
            runWebsiteLabel.Text = T("aboutWebsiteLabel", "Website: ");
            runChangelogLabel.Text = T("aboutChangelogLabel", "Changelog: ");
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }
    }
}

using CLCore.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConquerLoader.Forms.WPF
{
    public partial class ServerDatManagerWindow : Window
    {
        private LoaderConfig loaderConfig = null;

        public ServerDatManagerWindow()
        {
            InitializeComponent();
            Loaded += ServerDatManagerWindow_Loaded;
            StateChanged += ServerDatManagerWindow_StateChanged;
            UpdateChromeButtons();
        }

        private void ServerDatManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loaderConfig = Core.GetLoaderConfig();
            lstServers.Items.Clear();
            lstGroups.Items.Clear();
            ApplyTranslations();

            if (loaderConfig == null) return;

            foreach (ServerConfiguration server in loaderConfig.Servers)
            {
                lstServers.Items.Add(server);
            }
        }

        private void LstServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstGroups.Items.Clear();
            if (loaderConfig == null || lstServers.SelectedIndex < 0) return;

            ServerDatGroup group = loaderConfig.Servers[lstServers.SelectedIndex].Group;
            if (group != null)
            {
                lstGroups.Items.Add(group);
                txtGroupSummary.Text = string.Format(T("serverDatGroupSummary", "Group: {0} | Icon: {1}"), group.GroupName, group.GroupIcon);
            }
            else
            {
                txtGroupSummary.Text = T("serverDatNoGroup", "This server does not currently have a server.dat group assigned.");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ServerDatManagerWindow_StateChanged(object sender, EventArgs e)
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
            Title = T("serverDatWindowTitle", "Server.dat Viewer");
            txtWindowTitleBar.Text = T("serverDatWindowTitle", "Server.dat Viewer");
            txtServerDatHeading.Text = T("serverDatHeading", "Server.dat viewer");
            txtServerDatDescription.Text = T("serverDatDescription", "Inspect which server groups and icons are associated with each configured server.");
            txtServersTitle.Text = T("serverDatServersTitle", "Servers");
            txtGroupDetailsTitle.Text = T("serverDatGroupDetailsTitle", "Group details");
            txtGroupSummary.Text = T("serverDatNoSelection", "Select a server to inspect its assigned login screen group.");
            btnClose.Content = T("btnClose", "Close");
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }
    }
}

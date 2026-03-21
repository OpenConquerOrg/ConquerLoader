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
                txtGroupSummary.Text = "Group: " + group.GroupName + " | Icon: " + group.GroupIcon;
            }
            else
            {
                txtGroupSummary.Text = "This server does not currently have a server.dat group assigned.";
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
            btnMinimizeWindow.ToolTip = "Minimize";
            btnCloseWindow.Content = "\uE8BB";
            btnCloseWindow.Tag = "Close";
            btnCloseWindow.ToolTip = "Close";
            btnMaxRestoreWindow.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
            btnMaxRestoreWindow.ToolTip = WindowState == WindowState.Maximized ? "Restore" : "Maximize";
        }
    }
}

using CLCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConquerLoader.Forms.WPF
{
    public partial class PluginsWindow : Window
    {
        public PluginsWindow()
        {
            InitializeComponent();
            Loaded += PluginsWindow_Loaded;
            StateChanged += PluginsWindow_StateChanged;
            UpdateChromeButtons();
        }

        private void PluginsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            pluginsItems.ItemsSource = PluginLoader.Plugins != null
                ? PluginLoader.Plugins.ToList()
                : Enumerable.Empty<IPlugin>();
        }

        private void BtnConfigure_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            IPlugin plugin = source != null ? source.Tag as IPlugin : null;
            if (plugin != null)
            {
                plugin.Configure();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PluginsWindow_StateChanged(object sender, System.EventArgs e)
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

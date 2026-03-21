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
            ApplyTranslations();
        }

        private void ConfigureButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Content = T("btnConfigurePlugin", "Configure");
            }
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
            Title = T("pluginsWindowTitle", "Plugins");
            txtWindowTitleBar.Text = T("pluginsWindowTitle", "Plugins");
            txtPluginsHeading.Text = T("pluginsWindowTitle", "Plugins");
            txtPluginsDescription.Text = T("pluginsWindowDescription", "Open a plugin configuration screen from a clearer list of installed modules.");
            btnClose.Content = T("btnClose", "Close");
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }
    }
}

using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CLAutoPatchPlugin
{
    public partial class AutoPatchConfigurationWindow : Window
    {
        public AutoPatchSettings UpdatedSettings { get; private set; }

        public AutoPatchConfigurationWindow(AutoPatchSettings settings)
        {
            InitializeComponent();
            Loaded += AutoPatchConfigurationWindow_Loaded;
            StateChanged += AutoPatchConfigurationWindow_StateChanged;
            LoadSettings(settings ?? new AutoPatchSettings());
            UpdateChromeButtons();
        }

        private void AutoPatchConfigurationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtSchemaHelp.Text =
                "{\r\n" +
                "  \"version\": \"1.0.0\",\r\n" +
                "  \"baseUrl\": \"https://cdn.example.com/patch/\",\r\n" +
                "  \"packages\": [\r\n" +
                "    {\r\n" +
                "      \"id\": \"base-client\",\r\n" +
                "      \"archive\": \"base-client-100.zip\",\r\n" +
                "      \"format\": \"zip\",\r\n" +
                "      \"extractTo\": \".\",\r\n" +
                "      \"sha256\": \"HEX_OR_BASE64_SHA256\"\r\n" +
                "    },\r\n" +
                "    {\r\n" +
                "      \"id\": \"hd-textures\",\r\n" +
                "      \"url\": \"patches/hd-textures.rar\",\r\n" +
                "      \"format\": \"rar\",\r\n" +
                "      \"extractTo\": \"data\"\r\n" +
                "    }\r\n" +
                "  ]\r\n" +
                "}";
        }

        private void LoadSettings(AutoPatchSettings settings)
        {
            chkEnabled.IsChecked = settings.Enabled;
            txtManifestLocation.Text = settings.ManifestLocation ?? string.Empty;
            txtRelativeTargetFolder.Text = settings.RelativeTargetFolder ?? string.Empty;
            chkFailLaunchOnError.IsChecked = settings.FailLaunchOnError;
        }

        private void BtnBrowseManifest_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select patch manifest file",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog(this) == true)
            {
                txtManifestLocation.Text = dialog.FileName;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            UpdatedSettings = new AutoPatchSettings
            {
                Enabled = chkEnabled.IsChecked == true,
                ManifestLocation = (txtManifestLocation.Text ?? string.Empty).Trim(),
                RelativeTargetFolder = (txtRelativeTargetFolder.Text ?? string.Empty).Trim(),
                FailLaunchOnError = chkFailLaunchOnError.IsChecked == true
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
            DialogResult = false;
            Close();
        }

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            UpdateChromeButtons();
        }

        private void AutoPatchConfigurationWindow_StateChanged(object sender, EventArgs e)
        {
            UpdateChromeButtons();
        }

        private void UpdateChromeButtons()
        {
            if (btnMaxRestoreWindow == null)
            {
                return;
            }

            btnMinimizeWindow.Content = "\uE921";
            btnCloseWindow.Content = "\uE8BB";
            btnCloseWindow.Tag = "Close";
            btnMaxRestoreWindow.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
        }
    }
}

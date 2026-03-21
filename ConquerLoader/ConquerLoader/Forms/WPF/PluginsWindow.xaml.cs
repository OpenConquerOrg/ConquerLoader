using CLCore;
using CLCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConquerLoader.Forms.WPF
{
    public partial class PluginsWindow : Window
    {
        private readonly PluginLoader pluginLoader = new PluginLoader();
        private LoaderConfig loaderConfig;

        public PluginsWindow()
        {
            InitializeComponent();
            Loaded += PluginsWindow_Loaded;
            StateChanged += PluginsWindow_StateChanged;
            UpdateChromeButtons();
        }

        private void PluginsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loaderConfig = Core.GetLoaderConfig();
            ApplyTranslations();
            ReloadPluginCatalog();
        }

        private void ReloadPluginCatalog()
        {
            List<PluginCardItem> cards = pluginLoader
                .GetPluginCatalogSnapshot(loaderConfig)
                .Select(CreateCardItem)
                .ToList();

            pluginsItems.ItemsSource = cards;
            emptyStateCard.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private PluginCardItem CreateCardItem(PluginCatalogItem item)
        {
            bool isRemoteAvailable = item.Source == PluginSource.Remote && !item.IsInstalled;
            bool canConfigure = item.IsInstalled && item.Instance != null;

            return new PluginCardItem
            {
                CatalogItem = item,
                Name = item.Name,
                Explanation = item.Explanation,
                SourceText = item.Source == PluginSource.Remote
                    ? T("pluginsSourceRemote", "Remote")
                    : T("pluginsSourceLocal", "Local"),
                TierText = item.PluginType == PluginType.PREMIUM
                    ? T("pluginsTierPremium", "Premium")
                    : T("pluginsTierFree", "Free"),
                StatusText = !item.IsInstalled
                    ? T("pluginsStatusAvailable", "Available")
                    : item.IsEnabled
                        ? T("pluginsStatusEnabled", "Enabled")
                        : T("pluginsStatusDisabled", "Disabled"),
                HintText = BuildHintText(item),
                InstallButtonText = T("btnInstallPlugin", "Install"),
                ToggleButtonText = item.IsEnabled
                    ? T("btnDisablePlugin", "Disable")
                    : T("btnEnablePlugin", "Enable"),
                ConfigureButtonText = T("btnConfigurePlugin", "Configure"),
                RemoveButtonText = T("btnRemovePlugin", "Remove"),
                InstallVisibility = isRemoteAvailable ? Visibility.Visible : Visibility.Collapsed,
                ToggleVisibility = item.IsInstalled ? Visibility.Visible : Visibility.Collapsed,
                ConfigureVisibility = canConfigure ? Visibility.Visible : Visibility.Collapsed,
                RemoveVisibility = item.IsInstalled && item.Source == PluginSource.Remote ? Visibility.Visible : Visibility.Collapsed
            };
        }

        private string BuildHintText(PluginCatalogItem item)
        {
            if (item.Source == PluginSource.Remote && !item.IsInstalled)
            {
                return T("pluginsRemoteAvailableHint", "Available from your license catalog. Install it to download it into your local plugins folder.");
            }

            if (item.Source == PluginSource.Remote)
            {
                return T("pluginsRemoteInstalledHint", "This plugin was installed from the remote catalog. Restart the loader after install, remove, or enable changes.");
            }

            return T("pluginsLocalHint", "This plugin comes from the local Plugins folder.");
        }

        private PluginCardItem GetCardItem(object sender)
        {
            FrameworkElement source = sender as FrameworkElement;
            return source != null ? source.Tag as PluginCardItem : null;
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            PluginCardItem card = GetCardItem(sender);
            if (card == null || card.CatalogItem == null)
            {
                return;
            }

            try
            {
                loaderConfig = Core.GetLoaderConfig();
                pluginLoader.InstallRemotePlugin(loaderConfig, card.CatalogItem.RemoteName ?? card.CatalogItem.Name);
                MessageBox.Show(
                    T("pluginsInstallSuccess", "Plugin installed. Restart the loader to activate it."),
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                ReloadPluginCatalog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("pluginsActionError", "The plugin action failed: {0}"), ex.Message),
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void BtnToggleEnabled_Click(object sender, RoutedEventArgs e)
        {
            PluginCardItem card = GetCardItem(sender);
            if (card == null || card.CatalogItem == null)
            {
                return;
            }

            try
            {
                pluginLoader.SetPluginEnabled(card.CatalogItem, !card.CatalogItem.IsEnabled);
                MessageBox.Show(
                    T("pluginsStateSaved", "Plugin state saved. Restart the loader to apply the change."),
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                ReloadPluginCatalog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("pluginsActionError", "The plugin action failed: {0}"), ex.Message),
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void BtnConfigure_Click(object sender, RoutedEventArgs e)
        {
            PluginCardItem card = GetCardItem(sender);
            if (card == null || card.CatalogItem == null || card.CatalogItem.Instance == null)
            {
                return;
            }

            card.CatalogItem.Instance.Configure();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            PluginCardItem card = GetCardItem(sender);
            if (card == null || card.CatalogItem == null)
            {
                return;
            }

            MessageBoxResult confirmResult = MessageBox.Show(
                string.Format(T("pluginsRemoveConfirm", "Remove plugin \"{0}\" from the local installation?"), card.CatalogItem.Name),
                Title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                if (pluginLoader.UninstallRemotePlugin(card.CatalogItem))
                {
                    MessageBox.Show(
                        T("pluginsRemoveSuccess", "Plugin removed. Restart the loader if it was active in the current session."),
                        Title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                ReloadPluginCatalog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(T("pluginsActionError", "The plugin action failed: {0}"), ex.Message),
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            loaderConfig = Core.GetLoaderConfig();
            ApplyTranslations();
            ReloadPluginCatalog();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PluginsWindow_StateChanged(object sender, EventArgs e)
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
            bool hasLicense = loaderConfig != null && !string.IsNullOrWhiteSpace(loaderConfig.LicenseKey);

            Title = T("pluginsWindowTitle", "Plugins");
            txtWindowTitleBar.Text = T("pluginsWindowTitle", "Plugins");
            txtPluginsHeading.Text = T("pluginsWindowTitle", "Plugins");
            txtPluginsDescription.Text = T("pluginsWindowDescription", "Review local plugins, browse remote modules from your license, and decide which ones should be installed.");
            txtPluginsHint.Text = hasLicense
                ? T("pluginsHintLicensed", "Local plugins come from the Plugins folder. Remote plugins come from your license catalog and are installed on demand.")
                : T("pluginsHintNoLicense", "Local plugins work without a license. Add a license key if you want to browse and install remote plugins.");
            txtEmptyState.Text = T("pluginsEmptyState", "No plugins are available right now. Add local DLLs to the Plugins folder or configure a license to browse remote modules.");
            btnRefresh.Content = T("btnRefreshPlugins", "Refresh");
            btnClose.Content = T("btnClose", "Close");
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }

        public sealed class PluginCardItem
        {
            public PluginCatalogItem CatalogItem { get; set; }
            public string Name { get; set; }
            public string Explanation { get; set; }
            public string SourceText { get; set; }
            public string TierText { get; set; }
            public string StatusText { get; set; }
            public string HintText { get; set; }
            public string InstallButtonText { get; set; }
            public string ToggleButtonText { get; set; }
            public string ConfigureButtonText { get; set; }
            public string RemoveButtonText { get; set; }
            public Visibility InstallVisibility { get; set; }
            public Visibility ToggleVisibility { get; set; }
            public Visibility ConfigureVisibility { get; set; }
            public Visibility RemoveVisibility { get; set; }
        }
    }
}

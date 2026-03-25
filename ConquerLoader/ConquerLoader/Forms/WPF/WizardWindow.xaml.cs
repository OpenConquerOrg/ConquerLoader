using CLCore;
using CLCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConquerLoader.Forms.WPF
{
    public partial class WizardWindow : Window
    {
        private readonly int indexSelectedServer = -1;
        private ServerConfiguration selectedServer = null;
        private LoaderConfig loaderConfig = null;
        private int currentStep = 1;

        public WizardWindow()
        {
            InitializeComponent();
            loaderConfig = Core.GetLoaderConfig();
            Loaded += WizardWindow_Loaded;
            StateChanged += WizardWindow_StateChanged;
            UpdateChromeButtons();
        }

        public WizardWindow(int indexSelectedServer)
        {
            InitializeComponent();
            this.indexSelectedServer = indexSelectedServer;
            loaderConfig = Core.GetLoaderConfig();
            Loaded += WizardWindow_Loaded;
            StateChanged += WizardWindow_StateChanged;
            UpdateChromeButtons();
        }

        private void WizardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaderConfig == null)
            {
                loaderConfig = new LoaderConfig();
            }

            LoadServerDatGroups();

            if (indexSelectedServer >= 0 && loaderConfig.Servers.Count > indexSelectedServer)
            {
                selectedServer = loaderConfig.Servers[indexSelectedServer];
                if (selectedServer.Group == null)
                {
                    selectedServer.Group = new ServerDatGroup();
                }

                tbxServerName.Text = selectedServer.ServerName;
                tbxIP.Text = selectedServer.LoginHost;
                tbxConquerExe.Text = selectedServer.ExecutableName;
                tbxLoginPort.Text = selectedServer.LoginPort.ToString();
                tbxGamePort.Text = selectedServer.GamePort.ToString();
                tbxVersion.Text = selectedServer.ServerVersion.ToString();
                tglUseDX9.IsChecked = selectedServer.UseDirectX9;
            }
            else
            {
                string versionDatFilename = Path.Combine(Directory.GetCurrentDirectory(), "Version.dat");
                string[] versionDatLines = File.Exists(versionDatFilename) ? File.ReadAllLines(versionDatFilename) : new string[] { };
                int realVersion = 0;

                if (versionDatLines.Length == 1)
                {
                    File.WriteAllText(versionDatFilename, "99999" + System.Environment.NewLine + "#" + versionDatLines[0]);
                    versionDatLines = File.ReadAllLines(versionDatFilename);
                }

                foreach (string str in versionDatLines)
                {
                    if (str.StartsWith("#"))
                    {
                        string fixedStr = str.Replace("#", "");
                        int parsedVersion;
                        if (int.TryParse(fixedStr, out parsedVersion) && parsedVersion > 4000)
                        {
                            realVersion = parsedVersion;
                        }
                    }
                }

                tbxVersion.Text = realVersion.ToString();
                tbxConquerExe.Text = "Conquer.exe";
                tbxLoginPort.Text = "9958";
                tbxGamePort.Text = "5816";
            }

            TbxVersion_TextChanged(null, null);
            RestoreSavedGroupSelection();
            UpdateWizardStepUI();
            ApplyTranslations();
        }

        private void LoadServerDatGroups()
        {
            tbxGroup.Items.Clear();
            string p = Path.Combine(Constants.ClientPath, "data", "main", "flash");
            if (Directory.Exists(p))
            {
                foreach (string s in Directory.GetDirectories(p, "Group*"))
                {
                    string dir = Path.GetFileName(s);
                    tbxGroup.Items.Add(dir);
                }
            }
        }

        private static void EnsureComboContains(ComboBox comboBox, string value)
        {
            if (comboBox == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!comboBox.Items.Contains(value))
            {
                comboBox.Items.Add(value);
            }
        }

        private string FirstAvailableGroupName()
        {
            return tbxGroup != null && tbxGroup.Items.Count > 0
                ? tbxGroup.Items[0].ToString()
                : string.Empty;
        }

        private bool RequiresValidatedServerIcon(int version)
        {
            return version >= 6000;
        }

        private string ResolveGroupName(int version, bool fallbackToFirstAvailable)
        {
            string selectedGroup = SelectedGroupName();
            if (!string.IsNullOrWhiteSpace(selectedGroup))
            {
                return selectedGroup;
            }

            if (selectedServer != null && selectedServer.Group != null && !string.IsNullOrWhiteSpace(selectedServer.Group.GroupName))
            {
                return selectedServer.Group.GroupName;
            }

            if (fallbackToFirstAvailable && version >= Constants.MinVersionUseRAWServerDat)
            {
                return FirstAvailableGroupName();
            }

            return string.Empty;
        }

        private void RestoreSavedGroupSelection()
        {
            if (selectedServer == null)
            {
                return;
            }

            string savedGroupName = selectedServer.Group != null ? selectedServer.Group.GroupName : string.Empty;
            string savedServerIcon = selectedServer.ServerIcon;

            if (!string.IsNullOrWhiteSpace(savedGroupName))
            {
                EnsureComboContains(tbxGroup, savedGroupName);
                tbxGroup.SelectedItem = savedGroupName;
                TbxGroup_SelectionChanged(null, null);
            }

            if (!string.IsNullOrWhiteSpace(savedServerIcon))
            {
                EnsureComboContains(tbxGroupIcon, savedServerIcon);
                tbxGroupIcon.SelectedItem = savedServerIcon;
            }
        }

        private string SelectedGroupName()
        {
            if (tbxGroup == null)
            {
                return string.Empty;
            }

            if (tbxGroup.SelectedItem != null)
            {
                return tbxGroup.SelectedItem.ToString();
            }

            return string.IsNullOrWhiteSpace(tbxGroup.Text) ? string.Empty : tbxGroup.Text.Trim();
        }

        private string SelectedGroupIcon()
        {
            if (tbxGroupIcon == null)
            {
                return string.Empty;
            }

            if (tbxGroupIcon.SelectedItem != null)
            {
                return tbxGroupIcon.SelectedItem.ToString();
            }

            return string.IsNullOrWhiteSpace(tbxGroupIcon.Text) ? string.Empty : tbxGroupIcon.Text.Trim();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCurrentStep(4))
            {
                return;
            }

            int version;
            if (string.IsNullOrWhiteSpace(tbxServerName.Text))
            {
                MessageBox.Show(T("wizardMsgWriteServerName", "Write a server name before saving."), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IPAddress.TryParse(tbxIP.Text, out _))
            {
                MessageBox.Show(T("wizardMsgWriteValidIp", "Write a valid IP address before saving."), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbxVersion.Text, out version))
            {
                MessageBox.Show(T("wizardMsgWriteValidVersion", "Write a valid client version before saving."), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string groupName = ResolveGroupName(version, false);
            string selectedIcon = SelectedGroupIcon();
            List<string> valids = ServerDatGroupIcons(groupName);
            if (RequiresValidatedServerIcon(version) && visualGroupCard.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    MessageBox.Show(T("wizardMsgSelectValidGroup", "Select a valid group for this client version."), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!valids.Contains(selectedIcon))
                {
                    MessageBox.Show(T("wizardMsgInvalidGroupIcon", "This group icon is invalid for this client version."), Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            uint loginPort = ParsePort(tbxLoginPort.Text, 9958);
            uint gamePort = ParsePort(tbxGamePort.Text, 5816);

            if (indexSelectedServer >= 0 && selectedServer != null)
            {
                selectedServer.ServerName = tbxServerName.Text;
                selectedServer.LoginHost = tbxIP.Text;
                selectedServer.GameHost = tbxIP.Text;
                selectedServer.ServerVersion = (uint)version;
                selectedServer.ExecutableName = tbxConquerExe.Text;
                selectedServer.LoginPort = loginPort;
                selectedServer.GamePort = gamePort;
                selectedServer.UseDirectX9 = tglUseDX9.IsChecked == true;
                selectedServer.ServerIcon = selectedIcon;
                selectedServer.Group = new ServerDatGroup
                {
                    GroupIcon = string.IsNullOrWhiteSpace(groupName) ? null : groupName + ".swf",
                    GroupName = groupName
                };
            }
            else
            {
                loaderConfig.Servers.Add(new ServerConfiguration
                {
                    GameHost = tbxIP.Text,
                    LoginHost = tbxIP.Text,
                    ExecutableName = tbxConquerExe.Text,
                    ServerName = tbxServerName.Text,
                    UseDirectX9 = tglUseDX9.IsChecked == true,
                    ServerVersion = (uint)version,
                    LoginPort = loginPort,
                    GamePort = gamePort,
                    Group = new ServerDatGroup { GroupIcon = string.IsNullOrWhiteSpace(groupName) ? null : groupName + ".swf", GroupName = groupName },
                    ServerIcon = selectedIcon
                });
            }

            Core.SaveLoaderConfig(loaderConfig);
            DialogResult = true;
            Close();
        }

        private static uint ParsePort(string text, uint fallback)
        {
            uint value;
            return uint.TryParse(text, out value) ? value : fallback;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentStep > 1)
            {
                currentStep--;
                UpdateWizardStepUI();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCurrentStep(currentStep))
            {
                return;
            }

            if (currentStep < 4)
            {
                currentStep++;
                UpdateWizardStepUI();
            }
        }

        private void TbxVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            int version;
            if (!int.TryParse(tbxVersion.Text, out version))
            {
                return;
            }

            bool usesRawServerDat = version >= Constants.MinVersionUseRAWServerDat;
            visualGroupCard.Visibility = usesRawServerDat ? Visibility.Visible : Visibility.Collapsed;
            if (usesRawServerDat)
            {
                if (!string.IsNullOrWhiteSpace(SelectedGroupName()))
                {
                    TbxGroup_SelectionChanged(null, null);
                }
                else
                {
                    tbxGroupIcon.Items.Clear();
                    tbxGroupIcon.SelectedIndex = -1;
                }
            }

            bool supportsDx9 = version >= Constants.MinVersionUseDX8DX9Folders;
            dx9Card.Visibility = supportsDx9 ? Visibility.Visible : Visibility.Collapsed;
            if (!supportsDx9)
            {
                tglUseDX9.IsChecked = false;
            }

            UpdateWizardStepUI();
        }

        private void ValidateNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tbx = sender as TextBox;
            int n;
            if (tbx == null) return;

            if (!int.TryParse(tbx.Text, out n))
            {
                tbx.Text = "0";
                if (tbx.Name == "tbxLoginPort")
                {
                    tbx.Text = "9958";
                }
                if (tbx.Name == "tbxGamePort")
                {
                    tbx.Text = "5816";
                }
            }

        }

        private void TbxIP_LostFocus(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(tbxIP.Text, out ip))
            {
                tbxIP.Text = "";
                lblHelpIP.Text = T("wizardIpInvalid", "Please write a valid IP address.");
            }
            else
            {
                lblHelpIP.Text = T("wizardIpHelp", "Write the IP address of your login server.");
            }
            ValidateCurrentStep(currentStep, false);
        }

        private void InputValueChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCurrentStep(currentStep, false);
        }

        private void InputSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateCurrentStep(currentStep, false);
        }

        private void Dx9ValueChanged(object sender, RoutedEventArgs e)
        {
            ValidateCurrentStep(currentStep, false);
        }

        private List<string> ServerDatGroupIcons(string groupName = null)
        {
            List<string> groupIcons = new List<string>();
            string p = Path.Combine(Constants.ClientPath, "data", "main", "flash");
            string selectedGroup = string.IsNullOrWhiteSpace(groupName) ? SelectedGroupName() : groupName;
            if (Directory.Exists(p) && !string.IsNullOrWhiteSpace(selectedGroup))
            {
                foreach (string s in Directory.GetDirectories(p, selectedGroup))
                {
                    string dir = Path.GetFileName(s);
                    foreach (string f in Directory.GetFiles(s))
                    {
                        groupIcons.Add(dir + "/" + Path.GetFileName(f));
                    }
                }
            }
            return groupIcons;
        }

        private void TbxGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string previousSelection = SelectedGroupIcon();
            tbxGroupIcon.Items.Clear();
            foreach (string icon in ServerDatGroupIcons())
            {
                if (!tbxGroupIcon.Items.Contains(icon))
                {
                    tbxGroupIcon.Items.Add(icon);
                }
            }

            if (!string.IsNullOrWhiteSpace(previousSelection) && tbxGroupIcon.Items.Contains(previousSelection))
            {
                tbxGroupIcon.SelectedItem = previousSelection;
            }
            else
            {
                tbxGroupIcon.SelectedIndex = -1;
            }

            UpdateWizardStepUI();
        }

        private void UpdateWizardStepUI()
        {
            stepPanel1.Visibility = currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            stepPanel2.Visibility = currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            stepPanel3.Visibility = currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
            stepPanel4.Visibility = currentStep == 4 ? Visibility.Visible : Visibility.Collapsed;

            btnPrevious.IsEnabled = currentStep > 1;
            btnNext.Visibility = currentStep < 4 ? Visibility.Visible : Visibility.Collapsed;
            btnSave.Visibility = currentStep == 4 ? Visibility.Visible : Visibility.Collapsed;

            UpdateStepBadge(stepBadge1, stepBadgeText1, currentStep, 1);
            UpdateStepBadge(stepBadge2, stepBadgeText2, currentStep, 2);
            UpdateStepBadge(stepBadge3, stepBadgeText3, currentStep, 3);
            UpdateStepBadge(stepBadge4, stepBadgeText4, currentStep, 4);

            switch (currentStep)
            {
                case 1:
                    txtStepHeading.Text = T("wizardStep1Heading", "Step 1. Basic identity");
                    txtStepDescription.Text = T("wizardStep1Description", "Start with the two pieces of information every setup needs: the server name and the login server IP.");
                    txtStepHint.Text = T("wizardStep1Hint", "Use a friendly name and a valid IP address. Once both are correct, you can continue to the client details.");
                    break;
                case 2:
                    txtStepHeading.Text = T("wizardStep2Heading", "Step 2. Client details");
                    txtStepDescription.Text = T("wizardStep2Description", "Now tell the loader which client build you use and which executable should be launched.");
                    txtStepHint.Text = T("wizardStep2Hint", "The version number is important because it decides compatibility rules, available assets, and whether DirectX9 can be used.");
                    break;
                case 3:
                    txtStepHeading.Text = T("wizardStep3Heading", "Step 3. Compatibility options");
                    txtStepDescription.Text = T("wizardStep3Description", "Review ports, graphics compatibility, and any login screen assets required by newer clients.");
                    txtStepHint.Text = T("wizardStep3Hint", "Most servers keep the default ports. Choose the login screen group first, then pick one of the server icons available inside that group.");
                    break;
                default:
                    txtStepHeading.Text = T("wizardStep4Heading", "Step 4. Review and save");
                    txtStepDescription.Text = T("wizardStep4Description", "Everything is ready. Review the values from the previous steps and save the server when they look correct.");
                    txtStepHint.Text = T("wizardStep4Hint", "If something looks wrong, go back to the previous step and adjust it before saving.");
                    break;
            }

            ValidateCurrentStep(currentStep, false);
        }

        private void UpdateStepBadge(Border badge, TextBlock text, int activeStep, int step)
        {
            if (step < activeStep)
            {
                badge.Background = BrushFromHex("#137333");
                text.Text = "✓";
            }
            else if (step == activeStep)
            {
                badge.Background = BrushFromHex("#C53D2B");
                text.Text = step.ToString();
            }
            else
            {
                badge.Background = BrushFromHex("#808997");
                text.Text = step.ToString();
            }
        }

        private bool ValidateCurrentStep(int step, bool showMessage = true)
        {
            string message = null;
            int version = 0;
            int.TryParse(tbxVersion.Text, out version);

            switch (step)
            {
                case 1:
                    if (string.IsNullOrWhiteSpace(tbxServerName.Text))
                    {
                        message = T("wizardValidationStep1Name", "Write a server name to continue.");
                    }
                    else if (!IPAddress.TryParse(tbxIP.Text, out _))
                    {
                        message = T("wizardValidationStep1Ip", "Write a valid server IP to continue.");
                    }
                    break;
                case 2:
                    if (version <= 0)
                    {
                        message = T("wizardValidationStep2Version", "Write a valid client version to continue.");
                    }
                    else if (string.IsNullOrWhiteSpace(tbxConquerExe.Text))
                    {
                        message = T("wizardValidationStep2Exe", "Write the executable name to continue.");
                    }
                    break;
                case 3:
                    if (!uint.TryParse(tbxLoginPort.Text, out _) || !uint.TryParse(tbxGamePort.Text, out _))
                    {
                        message = T("wizardValidationStep3Ports", "Login port and game port must be valid numbers.");
                    }
                    else if (visualGroupCard.Visibility == Visibility.Visible && RequiresValidatedServerIcon(version))
                    {
                        string selectedGroup = ResolveGroupName(version, false);
                        List<string> valids = ServerDatGroupIcons(selectedGroup);
                        string selectedIcon = SelectedGroupIcon();
                        if (string.IsNullOrWhiteSpace(selectedGroup))
                        {
                            message = T("wizardValidationStep3Group", "Choose a visual group for this client version.");
                        }
                        else if (!valids.Contains(selectedIcon))
                        {
                            message = T("wizardValidationStep3Icon", "Choose a valid server icon for the selected group.");
                        }
                    }
                    break;
                case 4:
                    if (!ValidateCurrentStep(1, false) || !ValidateCurrentStep(2, false) || !ValidateCurrentStep(3, false))
                    {
                        message = T("wizardValidationStep4Previous", "Finish the previous steps before saving.");
                    }
                    break;
            }

            if (string.IsNullOrEmpty(message))
            {
                txtValidation.Foreground = BrushFromHex("#137333");
                txtValidation.Text = step == 4
                    ? T("wizardValidationReady", "Everything required is complete. You can save this server now.")
                    : T("wizardValidationComplete", "This step is complete. You can continue.");
                return true;
            }

            txtValidation.Foreground = BrushFromHex("#B06000");
            txtValidation.Text = message;

            if (showMessage)
            {
                MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return false;
        }

        private void WizardWindow_StateChanged(object sender, EventArgs e)
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
            if (btnPrevious == null)
            {
                return;
            }

            Title = indexSelectedServer >= 0
                ? T("wizardEditWindowTitle", "Edit Server Wizard")
                : T("wizardWindowTitle", "Server Setup Wizard");
            txtWindowTitleBar.Text = Title;
            txtWizardTitle.Text = Title;
            txtWizardIntro.Text = T("wizardIntro", "This assistant guides you step by step so you can configure a server without guessing what each field means.");
            btnCancel.Content = T("btnClose", "Close");
            btnPrevious.Content = Core.TranslateText("btnPrevious", "Previous");
            btnNext.Content = Core.TranslateText("btnNext", "Next");
            btnSave.Content = Core.TranslateText("btnSave", "Save Server");
            tglUseDX9.Content = Core.TranslateText("tglUseDX9", "Use DirectX9 environment");
            stepTitle1.Text = T("wizardBadge1Title", "Basics");
            stepTitle2.Text = T("wizardBadge2Title", "Client");
            stepTitle3.Text = T("wizardBadge3Title", "Options");
            stepTitle4.Text = T("wizardBadge4Title", "Review");
            stepSubtitle1.Text = T("wizardBadge1Subtitle", "Name and address");
            stepSubtitle2.Text = T("wizardBadge2Subtitle", "Version and executable");
            stepSubtitle3.Text = T("wizardBadge3Subtitle", "Ports and graphics");
            stepSubtitle4.Text = T("wizardBadge4Subtitle", "Confirm and save");
            txtServerNameLabel.Text = T("wizardFieldServerName", "Server name");
            txtServerNameHelp.Text = T("wizardHelpServerName", "Use the public name players should recognize inside the loader.");
            txtServerIpLabel.Text = T("wizardFieldServerIp", "Server IP");
            lblHelpIP.Text = T("wizardIpHelp", "Write the IP address of your login server.");
            txtClientVersionLabel.Text = T("wizardFieldClientVersion", "Client version");
            txtClientVersionHelp.Text = T("wizardHelpClientVersion", "This value controls compatibility, available assets, and DirectX9 support.");
            txtExecutableLabel.Text = T("wizardFieldExecutable", "Executable name");
            txtExecutableHelp.Text = T("wizardHelpExecutable", "Usually this is Conquer.exe unless your client uses another file name.");
            txtLoginPortLabel.Text = T("wizardFieldLoginPort", "Login port");
            txtGamePortLabel.Text = T("wizardFieldGamePort", "Game port");
            txtAssetsTitle.Text = T("wizardAssetsTitle", "Login screen assets");
            txtAssetsDescription.Text = T("wizardAssetsDescription", "Only newer clients need these values. Choose the visual group first, then the icon.");
            txtGroupLabel.Text = T("wizardFieldGroup", "Group");
            txtServerIconLabel.Text = T("wizardFieldServerIcon", "Server icon");
            txtServerIconHint.Text = T("wizardServerIconHint", "This icon belongs to the selected group and identifies the server entry inside the login screen.");
            txtGraphicsTitle.Text = T("wizardGraphicsTitle", "Graphics compatibility");
            lblUseDX9.Text = T("wizardDx9Help", "Enable this only if your selected client version supports DirectX9 folders.");
            txtReadyTitle.Text = T("wizardReadyTitle", "Ready to save");
            txtReadyDescription.Text = T("wizardReadyDescription", "Review the values from the previous steps. If everything looks correct, save the server and it will appear in the launcher immediately.");
            txtReadyWarning.Text = T("wizardReadyWarning", "If you are not sure about group or icon values, leave them only when the selected client version requires them.");
            txtRequirementsTitle.Text = T("wizardRequirementsTitle", "What this step needs");
            txtValidationTitle.Text = T("wizardValidationTitle", "Validation");
            if (string.IsNullOrWhiteSpace(txtValidation.Text) || txtValidation.Text == "Complete the required fields to continue.")
            {
                txtValidation.Text = T("wizardValidationPending", "Complete the required fields to continue.");
            }
            UpdateWizardStepUI();
        }

        private SolidColorBrush BrushFromHex(string color)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        private string T(string key, string fallback)
        {
            return Core.TranslateText(key, fallback);
        }
    }
}

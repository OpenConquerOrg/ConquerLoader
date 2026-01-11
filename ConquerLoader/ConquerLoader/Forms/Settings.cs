using CLCore.Models;
using ConquerLoader.Forms;
using MetroFramework.Controls;
using System;
using System.IO;
using System.Windows.Forms;

namespace ConquerLoader
{
    public partial class Settings : MetroFramework.Forms.MetroForm
    {
        public LoaderConfig CurrentLoaderConfig = null;
        private Wizard WizardForm = null;
        private Wizard WizardEditForm = null;
        private ServerDatManager ServerDatForm = null;
        private Plugins PluginsForm = null;
        public Settings()
        {
            InitializeComponent();
            Core.LoadControlTranslations(Controls);
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            if ((MetroFramework.Forms.MetroForm)Tag != null)
            {
                tbxTitle.WaterMark = ((MetroFramework.Forms.MetroForm)Tag).Text;
            }
            langSelector.Items.Add("English");
            langSelector.Items.Add("Español");
            langSelector.Items.Add("Português");
            pbFlag.Visible = true;
            langSelector.Visible = true;
            lblServerNameChange.Visible = false; // No need now
            tglServerNameChange.Visible = false; // No need now
            CurrentLoaderConfig = Core.GetLoaderConfig();
            if (CurrentLoaderConfig != null)
            {
                string findStr = "English";
                if (CurrentLoaderConfig.Lang == "es")
                {
                    findStr = "Español";
                }
                if (CurrentLoaderConfig.Lang == "pt")
                {
                    findStr = "Português";
                }
                langSelector.SelectedIndex = langSelector.FindString(findStr);
            }
            if (!File.Exists(Core.ConfigJsonPath) && !File.Exists(Core.ConfigJsonPath+".lock"))
            {
                LoaderConfig lc = new LoaderConfig();
                Core.SaveLoaderConfig(lc);
                MetroFramework.MetroMessageBox.Show(this, "Cannot load config.json. Creating one... Restart App!", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            } else
            {
                tglDebugMode.Checked = CurrentLoaderConfig.DebugMode;
                tglCloseOnFinish.Checked = CurrentLoaderConfig.CloseOnFinish;
                tglHighResolution.Checked = CurrentLoaderConfig.HighResolution;
                tglFullscreen.Checked = CurrentLoaderConfig.FullScreen;
                tglServerNameChange.Checked = CurrentLoaderConfig.ServernameChange;
                tglDisableAutoFixFlash.Checked = CurrentLoaderConfig.DisableAutoFixFlash;
                tglDisableScreenChanges.Checked = CurrentLoaderConfig.DisableScreenChanges;
                tglUseCustomDLLs.Checked = CurrentLoaderConfig.UseCustomDLLs;
                tglFPSUnlock.Checked = CurrentLoaderConfig.FPSUnlock;
                tbxTitle.Text = CurrentLoaderConfig.Title;
                gridViewSettings.DataSource = CurrentLoaderConfig.Servers;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Core.SaveLoaderConfig(CurrentLoaderConfig);
            this.Close();
        }

        private void TglDebugMode_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.DebugMode = (sender as MetroToggle).Checked;
        }

        private void TglCloseOnFinish_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.CloseOnFinish = (sender as MetroToggle).Checked;
        }

        private void TbxTitle_TextChanged(object sender, EventArgs e)
        {
            MetroTextBox mtbx = (sender as MetroTextBox);
            if (CurrentLoaderConfig != null)
            {
                CurrentLoaderConfig.Title = mtbx.Text;
            }
        }

        private void TglHighResolution_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.HighResolution = (sender as MetroToggle).Checked;
        }

        private void TglFullscreen_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.FullScreen = (sender as MetroToggle).Checked;
        }

        private void TglServerNameChange_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.ServernameChange = (sender as MetroToggle).Checked;
        }

        private void TglDisableAutoFixFlash_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.DisableAutoFixFlash = (sender as MetroToggle).Checked;
        }

        private void TglDisableScreenChanges_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.DisableScreenChanges = (sender as MetroToggle).Checked;
            if (CurrentLoaderConfig.DisableScreenChanges)
            {
                tglFullscreen.Checked = false;
                tglFullscreen.Enabled = false;
                tglHighResolution.Checked = false;
                tglHighResolution.Enabled = false;
            } else
            {
                tglFullscreen.Enabled = true;
                tglHighResolution.Enabled = true;
            }
        }

        private void TglUseCustomDLLs_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.UseCustomDLLs = (sender as MetroToggle).Checked;
            if (CurrentLoaderConfig.UseCustomDLLs)
            {
                MetroFramework.MetroMessageBox.Show(this, "For use Custom DLL can put a DLL called COHook.dll for be auto-injected like native lib of loader.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TglFPSUnlock_CheckedChanged(object sender, EventArgs e)
        {
            CurrentLoaderConfig.FPSUnlock = (sender as MetroToggle).Checked;
            if (CurrentLoaderConfig.FPSUnlock)
            {
                lblImportantInfo.Text = "FPS Unlock may cause instability in some systems, for example with Old Graphic Cards. Use at your own risk.";
                lblImportantInfo.Visible = true;
            }
        }

        private void BtnWizard_Click(object sender, EventArgs e)
        {
            if (WizardForm == null)
            {
                WizardForm = new Wizard();
            }
            WizardForm.ShowDialog();
            CurrentLoaderConfig = Core.GetLoaderConfig();
            gridViewSettings.DataSource = CurrentLoaderConfig.Servers;
        }

        private void BtnPlugins_Click(object sender, EventArgs e)
        {
            if (PluginsForm == null)
            {
                PluginsForm = new Plugins();
            }
            PluginsForm.ShowDialog();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (gridViewSettings.CurrentRow != null)
            {
                if (WizardEditForm != null)
                {
                    WizardEditForm.Dispose();
                }
                if (Core.GetLoaderConfig().Servers.Count > gridViewSettings.CurrentRow.Index)
                {
                    WizardEditForm = new Wizard(gridViewSettings.CurrentRow.Index);
                    WizardEditForm.ShowDialog();
                    CurrentLoaderConfig = Core.GetLoaderConfig();
                    gridViewSettings.DataSource = CurrentLoaderConfig.Servers;
                }
            }
        }

        private void BtnServerDat_Click(object sender, EventArgs e)
        {
            if (ServerDatForm != null)
            {
                ServerDatForm.Dispose();
            }
            ServerDatForm = new ServerDatManager();
            ServerDatForm.ShowDialog();
        }

        private void LangSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            MetroComboBox mcb = sender as MetroComboBox;
            switch(mcb.SelectedIndex)
            {
                case 0:
                    pbFlag.Image = Properties.Resources.en;
                    CurrentLoaderConfig.Lang = "en";
                    break;
                case 1:
                    pbFlag.Image = Properties.Resources.es;
                    CurrentLoaderConfig.Lang = "es";
                    break;
                case 2:
                    pbFlag.Image = Properties.Resources.pt;
                    CurrentLoaderConfig.Lang = "pt";
                    break;
                default:
                    pbFlag.Image = Properties.Resources.en;
                    CurrentLoaderConfig.Lang = "en";
                    break;
            }
            Core.DetectLang(CurrentLoaderConfig);
            Core.LoadControlTranslations(Controls);
            if (Owner != null) Core.LoadControlTranslations(Owner.Controls);
            Core.SaveLoaderConfig(CurrentLoaderConfig);
        }

        private void BtnLockConfig_Click(object sender, EventArgs e)
        {
            Core.UseEncryptedConfig = true;
            Core.SaveLoaderConfig(CurrentLoaderConfig);
            File.Delete(Core.ConfigJsonPath);
            Application.Exit();
        }

        private void lblFPSUnlock_Click(object sender, EventArgs e)
        {

        }
    }
}

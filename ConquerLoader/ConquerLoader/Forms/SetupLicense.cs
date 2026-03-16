using CLCore.Models;
using System.Windows.Forms;

namespace ConquerLoader.Forms
{
    public partial class SetupLicense : MetroFramework.Forms.MetroForm
    {
        public LoaderConfig config = Core.GetLoaderConfig();
        public SetupLicense()
        {
            InitializeComponent();
            tbxLicenseKey.Text = config.LicenseKey;
        }

        private void BtnSetup_Click(object sender, System.EventArgs e)
        {
            config.LicenseKey = tbxLicenseKey.Text;
            Core.SaveLoaderConfig(config);
            DialogResult = DialogResult.OK;
            MetroFramework.MetroMessageBox.Show(this, "License Saved! Restart the Loader for see all available plugins!", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

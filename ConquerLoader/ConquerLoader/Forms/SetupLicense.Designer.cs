namespace ConquerLoader.Forms
{
    partial class SetupLicense
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupLicense));
            this.lblLicenseInfo = new MetroFramework.Controls.MetroLabel();
            this.btnSetup = new MetroFramework.Controls.MetroButton();
            this.tbxLicenseKey = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // lblLicenseInfo
            // 
            this.lblLicenseInfo.AutoSize = true;
            this.lblLicenseInfo.Location = new System.Drawing.Point(32, 78);
            this.lblLicenseInfo.Name = "lblLicenseInfo";
            this.lblLicenseInfo.Size = new System.Drawing.Size(368, 19);
            this.lblLicenseInfo.TabIndex = 1;
            this.lblLicenseInfo.Text = "Setup your license key and enjoy of plugins created by Owner";
            // 
            // btnSetup
            // 
            this.btnSetup.Location = new System.Drawing.Point(32, 210);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(472, 50);
            this.btnSetup.TabIndex = 2;
            this.btnSetup.Text = "Setup";
            this.btnSetup.UseSelectable = true;
            this.btnSetup.Click += new System.EventHandler(this.BtnSetup_Click);
            // 
            // tbxLicenseKey
            // 
            // 
            // 
            // 
            this.tbxLicenseKey.CustomButton.Image = null;
            this.tbxLicenseKey.CustomButton.Location = new System.Drawing.Point(402, 1);
            this.tbxLicenseKey.CustomButton.Name = "";
            this.tbxLicenseKey.CustomButton.Size = new System.Drawing.Size(69, 69);
            this.tbxLicenseKey.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbxLicenseKey.CustomButton.TabIndex = 1;
            this.tbxLicenseKey.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbxLicenseKey.CustomButton.UseSelectable = true;
            this.tbxLicenseKey.CustomButton.Visible = false;
            this.tbxLicenseKey.DisplayIcon = true;
            this.tbxLicenseKey.Icon = global::ConquerLoader.Properties.Resources.ConquerLoaderICON;
            this.tbxLicenseKey.Lines = new string[] {
        "Your license key (Example: 2d32a164-fcd5-796b-b43f-005a78274cad)"};
            this.tbxLicenseKey.Location = new System.Drawing.Point(32, 124);
            this.tbxLicenseKey.MaxLength = 32767;
            this.tbxLicenseKey.Multiline = true;
            this.tbxLicenseKey.Name = "tbxLicenseKey";
            this.tbxLicenseKey.PasswordChar = '\0';
            this.tbxLicenseKey.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbxLicenseKey.SelectedText = "";
            this.tbxLicenseKey.SelectionLength = 0;
            this.tbxLicenseKey.SelectionStart = 0;
            this.tbxLicenseKey.ShortcutsEnabled = true;
            this.tbxLicenseKey.Size = new System.Drawing.Size(472, 71);
            this.tbxLicenseKey.TabIndex = 3;
            this.tbxLicenseKey.Text = "Your license key (Example: 2d32a164-fcd5-796b-b43f-005a78274cad)";
            this.tbxLicenseKey.UseSelectable = true;
            this.tbxLicenseKey.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbxLicenseKey.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // SetupLicense
            // 
            this.AcceptButton = this.btnSetup;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 301);
            this.Controls.Add(this.tbxLicenseKey);
            this.Controls.Add(this.btnSetup);
            this.Controls.Add(this.lblLicenseInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SetupLicense";
            this.Resizable = false;
            this.Text = "Setup your License";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Controls.MetroLabel lblLicenseInfo;
        private MetroFramework.Controls.MetroButton btnSetup;
        private MetroFramework.Controls.MetroTextBox tbxLicenseKey;
    }
}
namespace ConquerLoader
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.tglDebugMode = new MetroFramework.Controls.MetroToggle();
            this.tglCloseOnFinish = new MetroFramework.Controls.MetroToggle();
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.lblFPSUnlock = new MetroFramework.Controls.MetroLabel();
            this.tglFPSUnlock = new MetroFramework.Controls.MetroToggle();
            this.lblUseCustomDLLs = new MetroFramework.Controls.MetroLabel();
            this.tglUseCustomDLLs = new MetroFramework.Controls.MetroToggle();
            this.lblDisableScreenChanges = new MetroFramework.Controls.MetroLabel();
            this.tglDisableScreenChanges = new MetroFramework.Controls.MetroToggle();
            this.btnLockConfig = new MetroFramework.Controls.MetroButton();
            this.btnServerDat = new MetroFramework.Controls.MetroButton();
            this.lblDisableAutoFixFlash = new MetroFramework.Controls.MetroLabel();
            this.tglDisableAutoFixFlash = new MetroFramework.Controls.MetroToggle();
            this.btnPlugins = new MetroFramework.Controls.MetroButton();
            this.btnWizard = new MetroFramework.Controls.MetroButton();
            this.btnEdit = new MetroFramework.Controls.MetroButton();
            this.lblFullscreen = new MetroFramework.Controls.MetroLabel();
            this.tglFullscreen = new MetroFramework.Controls.MetroToggle();
            this.lblHighResolution = new MetroFramework.Controls.MetroLabel();
            this.tglHighResolution = new MetroFramework.Controls.MetroToggle();
            this.gridViewSettings = new System.Windows.Forms.DataGridView();
            this.btnSave = new MetroFramework.Controls.MetroButton();
            this.lblTitle = new MetroFramework.Controls.MetroLabel();
            this.tbxTitle = new MetroFramework.Controls.MetroTextBox();
            this.lblCloseOnFinish = new MetroFramework.Controls.MetroLabel();
            this.lblDebugMode = new MetroFramework.Controls.MetroLabel();
            this.lblServerNameChange = new MetroFramework.Controls.MetroLabel();
            this.tglServerNameChange = new MetroFramework.Controls.MetroToggle();
            this.langSelector = new MetroFramework.Controls.MetroComboBox();
            this.pbFlag = new System.Windows.Forms.PictureBox();
            this.lblImportantInfo = new MetroFramework.Controls.MetroLabel();
            this.btnSetupLicense = new MetroFramework.Controls.MetroButton();
            this.metroPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewSettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFlag)).BeginInit();
            this.SuspendLayout();
            // 
            // tglDebugMode
            // 
            this.tglDebugMode.AutoSize = true;
            this.tglDebugMode.Location = new System.Drawing.Point(32, 63);
            this.tglDebugMode.Name = "tglDebugMode";
            this.tglDebugMode.Size = new System.Drawing.Size(80, 24);
            this.tglDebugMode.TabIndex = 0;
            this.tglDebugMode.Text = "Off";
            this.tglDebugMode.UseSelectable = true;
            this.tglDebugMode.CheckedChanged += new System.EventHandler(this.TglDebugMode_CheckedChanged);
            // 
            // tglCloseOnFinish
            // 
            this.tglCloseOnFinish.AutoSize = true;
            this.tglCloseOnFinish.Location = new System.Drawing.Point(32, 140);
            this.tglCloseOnFinish.Name = "tglCloseOnFinish";
            this.tglCloseOnFinish.Size = new System.Drawing.Size(80, 24);
            this.tglCloseOnFinish.TabIndex = 1;
            this.tglCloseOnFinish.Text = "Off";
            this.tglCloseOnFinish.UseSelectable = true;
            this.tglCloseOnFinish.CheckedChanged += new System.EventHandler(this.TglCloseOnFinish_CheckedChanged);
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.lblFPSUnlock);
            this.metroPanel1.Controls.Add(this.tglFPSUnlock);
            this.metroPanel1.Controls.Add(this.lblUseCustomDLLs);
            this.metroPanel1.Controls.Add(this.tglUseCustomDLLs);
            this.metroPanel1.Controls.Add(this.lblDisableScreenChanges);
            this.metroPanel1.Controls.Add(this.tglDisableScreenChanges);
            this.metroPanel1.Controls.Add(this.btnLockConfig);
            this.metroPanel1.Controls.Add(this.btnServerDat);
            this.metroPanel1.Controls.Add(this.lblDisableAutoFixFlash);
            this.metroPanel1.Controls.Add(this.tglDisableAutoFixFlash);
            this.metroPanel1.Controls.Add(this.btnPlugins);
            this.metroPanel1.Controls.Add(this.btnWizard);
            this.metroPanel1.Controls.Add(this.btnEdit);
            this.metroPanel1.Controls.Add(this.lblFullscreen);
            this.metroPanel1.Controls.Add(this.tglFullscreen);
            this.metroPanel1.Controls.Add(this.lblHighResolution);
            this.metroPanel1.Controls.Add(this.tglHighResolution);
            this.metroPanel1.Controls.Add(this.gridViewSettings);
            this.metroPanel1.Controls.Add(this.btnSave);
            this.metroPanel1.Controls.Add(this.lblTitle);
            this.metroPanel1.Controls.Add(this.tbxTitle);
            this.metroPanel1.Controls.Add(this.lblCloseOnFinish);
            this.metroPanel1.Controls.Add(this.lblDebugMode);
            this.metroPanel1.Controls.Add(this.tglCloseOnFinish);
            this.metroPanel1.Controls.Add(this.tglDebugMode);
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 15;
            this.metroPanel1.Location = new System.Drawing.Point(24, 106);
            this.metroPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(1124, 695);
            this.metroPanel1.TabIndex = 0;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 15;
            // 
            // lblFPSUnlock
            // 
            this.lblFPSUnlock.AutoSize = true;
            this.lblFPSUnlock.Location = new System.Drawing.Point(510, 29);
            this.lblFPSUnlock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFPSUnlock.Name = "lblFPSUnlock";
            this.lblFPSUnlock.Size = new System.Drawing.Size(72, 19);
            this.lblFPSUnlock.TabIndex = 31;
            this.lblFPSUnlock.Text = "FPs Unlock";
            // 
            // tglFPSUnlock
            // 
            this.tglFPSUnlock.AutoSize = true;
            this.tglFPSUnlock.Location = new System.Drawing.Point(510, 63);
            this.tglFPSUnlock.Name = "tglFPSUnlock";
            this.tglFPSUnlock.Size = new System.Drawing.Size(80, 24);
            this.tglFPSUnlock.TabIndex = 30;
            this.tglFPSUnlock.Text = "Off";
            this.tglFPSUnlock.UseSelectable = true;
            this.tglFPSUnlock.CheckedChanged += new System.EventHandler(this.TglFPSUnlock_CheckedChanged);
            // 
            // lblUseCustomDLLs
            // 
            this.lblUseCustomDLLs.AutoSize = true;
            this.lblUseCustomDLLs.Location = new System.Drawing.Point(822, 108);
            this.lblUseCustomDLLs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUseCustomDLLs.Name = "lblUseCustomDLLs";
            this.lblUseCustomDLLs.Size = new System.Drawing.Size(104, 19);
            this.lblUseCustomDLLs.TabIndex = 29;
            this.lblUseCustomDLLs.Text = "Use Custom DLL";
            // 
            // tglUseCustomDLLs
            // 
            this.tglUseCustomDLLs.AutoSize = true;
            this.tglUseCustomDLLs.Location = new System.Drawing.Point(822, 140);
            this.tglUseCustomDLLs.Name = "tglUseCustomDLLs";
            this.tglUseCustomDLLs.Size = new System.Drawing.Size(80, 24);
            this.tglUseCustomDLLs.TabIndex = 28;
            this.tglUseCustomDLLs.Text = "Off";
            this.tglUseCustomDLLs.UseSelectable = true;
            this.tglUseCustomDLLs.CheckedChanged += new System.EventHandler(this.TglUseCustomDLLs_CheckedChanged);
            // 
            // lblDisableScreenChanges
            // 
            this.lblDisableScreenChanges.AutoSize = true;
            this.lblDisableScreenChanges.Location = new System.Drawing.Point(822, 29);
            this.lblDisableScreenChanges.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDisableScreenChanges.Name = "lblDisableScreenChanges";
            this.lblDisableScreenChanges.Size = new System.Drawing.Size(148, 19);
            this.lblDisableScreenChanges.TabIndex = 27;
            this.lblDisableScreenChanges.Text = "Disable Screen Changes";
            // 
            // tglDisableScreenChanges
            // 
            this.tglDisableScreenChanges.AutoSize = true;
            this.tglDisableScreenChanges.Location = new System.Drawing.Point(822, 62);
            this.tglDisableScreenChanges.Name = "tglDisableScreenChanges";
            this.tglDisableScreenChanges.Size = new System.Drawing.Size(80, 24);
            this.tglDisableScreenChanges.TabIndex = 26;
            this.tglDisableScreenChanges.Text = "Off";
            this.tglDisableScreenChanges.UseSelectable = true;
            this.tglDisableScreenChanges.CheckedChanged += new System.EventHandler(this.TglDisableScreenChanges_CheckedChanged);
            // 
            // btnLockConfig
            // 
            this.btnLockConfig.Location = new System.Drawing.Point(628, 263);
            this.btnLockConfig.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLockConfig.Name = "btnLockConfig";
            this.btnLockConfig.Size = new System.Drawing.Size(166, 34);
            this.btnLockConfig.TabIndex = 25;
            this.btnLockConfig.Text = "Lock Config";
            this.btnLockConfig.UseSelectable = true;
            this.btnLockConfig.Click += new System.EventHandler(this.BtnLockConfig_Click);
            // 
            // btnServerDat
            // 
            this.btnServerDat.Location = new System.Drawing.Point(802, 220);
            this.btnServerDat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnServerDat.Name = "btnServerDat";
            this.btnServerDat.Size = new System.Drawing.Size(100, 34);
            this.btnServerDat.TabIndex = 22;
            this.btnServerDat.Text = "Server.dat";
            this.btnServerDat.UseSelectable = true;
            this.btnServerDat.Click += new System.EventHandler(this.BtnServerDat_Click);
            // 
            // lblDisableAutoFixFlash
            // 
            this.lblDisableAutoFixFlash.AutoSize = true;
            this.lblDisableAutoFixFlash.Location = new System.Drawing.Point(510, 108);
            this.lblDisableAutoFixFlash.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDisableAutoFixFlash.Name = "lblDisableAutoFixFlash";
            this.lblDisableAutoFixFlash.Size = new System.Drawing.Size(132, 19);
            this.lblDisableAutoFixFlash.TabIndex = 21;
            this.lblDisableAutoFixFlash.Text = "Disable AutoFix Flash";
            // 
            // tglDisableAutoFixFlash
            // 
            this.tglDisableAutoFixFlash.AutoSize = true;
            this.tglDisableAutoFixFlash.Location = new System.Drawing.Point(510, 142);
            this.tglDisableAutoFixFlash.Name = "tglDisableAutoFixFlash";
            this.tglDisableAutoFixFlash.Size = new System.Drawing.Size(80, 24);
            this.tglDisableAutoFixFlash.TabIndex = 20;
            this.tglDisableAutoFixFlash.Text = "Off";
            this.tglDisableAutoFixFlash.UseSelectable = true;
            this.tglDisableAutoFixFlash.CheckedChanged += new System.EventHandler(this.TglDisableAutoFixFlash_CheckedChanged);
            // 
            // btnPlugins
            // 
            this.btnPlugins.Location = new System.Drawing.Point(802, 263);
            this.btnPlugins.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnPlugins.Name = "btnPlugins";
            this.btnPlugins.Size = new System.Drawing.Size(100, 34);
            this.btnPlugins.TabIndex = 19;
            this.btnPlugins.Text = "Plugins";
            this.btnPlugins.UseSelectable = true;
            this.btnPlugins.Click += new System.EventHandler(this.BtnPlugins_Click);
            // 
            // btnWizard
            // 
            this.btnWizard.Location = new System.Drawing.Point(914, 220);
            this.btnWizard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnWizard.Name = "btnWizard";
            this.btnWizard.Size = new System.Drawing.Size(86, 34);
            this.btnWizard.TabIndex = 16;
            this.btnWizard.Text = "New +";
            this.btnWizard.UseSelectable = true;
            this.btnWizard.Click += new System.EventHandler(this.BtnWizard_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(914, 263);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(86, 34);
            this.btnEdit.TabIndex = 15;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseSelectable = true;
            this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // lblFullscreen
            // 
            this.lblFullscreen.AutoSize = true;
            this.lblFullscreen.Location = new System.Drawing.Point(249, 108);
            this.lblFullscreen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFullscreen.Name = "lblFullscreen";
            this.lblFullscreen.Size = new System.Drawing.Size(68, 19);
            this.lblFullscreen.TabIndex = 12;
            this.lblFullscreen.Text = "FullScreen";
            // 
            // tglFullscreen
            // 
            this.tglFullscreen.AutoSize = true;
            this.tglFullscreen.Location = new System.Drawing.Point(249, 140);
            this.tglFullscreen.Name = "tglFullscreen";
            this.tglFullscreen.Size = new System.Drawing.Size(80, 24);
            this.tglFullscreen.TabIndex = 11;
            this.tglFullscreen.Text = "Off";
            this.tglFullscreen.UseSelectable = true;
            this.tglFullscreen.CheckedChanged += new System.EventHandler(this.TglFullscreen_CheckedChanged);
            // 
            // lblHighResolution
            // 
            this.lblHighResolution.AutoSize = true;
            this.lblHighResolution.Location = new System.Drawing.Point(249, 29);
            this.lblHighResolution.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHighResolution.Name = "lblHighResolution";
            this.lblHighResolution.Size = new System.Drawing.Size(139, 19);
            this.lblHighResolution.TabIndex = 10;
            this.lblHighResolution.Text = "High Resolution Mode";
            // 
            // tglHighResolution
            // 
            this.tglHighResolution.AutoSize = true;
            this.tglHighResolution.Location = new System.Drawing.Point(249, 63);
            this.tglHighResolution.Name = "tglHighResolution";
            this.tglHighResolution.Size = new System.Drawing.Size(80, 24);
            this.tglHighResolution.TabIndex = 9;
            this.tglHighResolution.Text = "Off";
            this.tglHighResolution.UseSelectable = true;
            this.tglHighResolution.CheckedChanged += new System.EventHandler(this.TglHighResolution_CheckedChanged);
            // 
            // gridViewSettings
            // 
            this.gridViewSettings.AllowUserToOrderColumns = true;
            this.gridViewSettings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridViewSettings.Location = new System.Drawing.Point(30, 306);
            this.gridViewSettings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gridViewSettings.Name = "gridViewSettings";
            this.gridViewSettings.RowHeadersWidth = 62;
            this.gridViewSettings.Size = new System.Drawing.Size(1082, 375);
            this.gridViewSettings.TabIndex = 8;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(1006, 220);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(105, 77);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseSelectable = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(30, 229);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(92, 19);
            this.lblTitle.TabIndex = 6;
            this.lblTitle.Text = "Title in Loader";
            // 
            // tbxTitle
            // 
            // 
            // 
            // 
            this.tbxTitle.CustomButton.Image = null;
            this.tbxTitle.CustomButton.Location = new System.Drawing.Point(252, 1);
            this.tbxTitle.CustomButton.Name = "";
            this.tbxTitle.CustomButton.Size = new System.Drawing.Size(33, 33);
            this.tbxTitle.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.tbxTitle.CustomButton.TabIndex = 1;
            this.tbxTitle.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.tbxTitle.CustomButton.UseSelectable = true;
            this.tbxTitle.CustomButton.Visible = false;
            this.tbxTitle.Lines = new string[0];
            this.tbxTitle.Location = new System.Drawing.Point(32, 262);
            this.tbxTitle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbxTitle.MaxLength = 32767;
            this.tbxTitle.Name = "tbxTitle";
            this.tbxTitle.PasswordChar = '\0';
            this.tbxTitle.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.tbxTitle.SelectedText = "";
            this.tbxTitle.SelectionLength = 0;
            this.tbxTitle.SelectionStart = 0;
            this.tbxTitle.ShortcutsEnabled = true;
            this.tbxTitle.Size = new System.Drawing.Size(286, 35);
            this.tbxTitle.TabIndex = 5;
            this.tbxTitle.UseSelectable = true;
            this.tbxTitle.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.tbxTitle.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.tbxTitle.TextChanged += new System.EventHandler(this.TbxTitle_TextChanged);
            // 
            // lblCloseOnFinish
            // 
            this.lblCloseOnFinish.AutoSize = true;
            this.lblCloseOnFinish.Location = new System.Drawing.Point(32, 108);
            this.lblCloseOnFinish.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCloseOnFinish.Name = "lblCloseOnFinish";
            this.lblCloseOnFinish.Size = new System.Drawing.Size(96, 19);
            this.lblCloseOnFinish.TabIndex = 4;
            this.lblCloseOnFinish.Text = "Close on Finish";
            // 
            // lblDebugMode
            // 
            this.lblDebugMode.AutoSize = true;
            this.lblDebugMode.Location = new System.Drawing.Point(32, 29);
            this.lblDebugMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDebugMode.Name = "lblDebugMode";
            this.lblDebugMode.Size = new System.Drawing.Size(87, 19);
            this.lblDebugMode.TabIndex = 3;
            this.lblDebugMode.Text = "Debug Mode";
            // 
            // lblServerNameChange
            // 
            this.lblServerNameChange.AutoSize = true;
            this.lblServerNameChange.Location = new System.Drawing.Point(207, 25);
            this.lblServerNameChange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerNameChange.Name = "lblServerNameChange";
            this.lblServerNameChange.Size = new System.Drawing.Size(240, 19);
            this.lblServerNameChange.TabIndex = 14;
            this.lblServerNameChange.Text = "ServerName Change (Deprecated now)";
            // 
            // tglServerNameChange
            // 
            this.tglServerNameChange.AutoSize = true;
            this.tglServerNameChange.Location = new System.Drawing.Point(207, 58);
            this.tglServerNameChange.Name = "tglServerNameChange";
            this.tglServerNameChange.Size = new System.Drawing.Size(80, 24);
            this.tglServerNameChange.TabIndex = 13;
            this.tglServerNameChange.Text = "Off";
            this.tglServerNameChange.UseSelectable = true;
            this.tglServerNameChange.CheckedChanged += new System.EventHandler(this.TglServerNameChange_CheckedChanged);
            // 
            // langSelector
            // 
            this.langSelector.FormattingEnabled = true;
            this.langSelector.ItemHeight = 23;
            this.langSelector.Location = new System.Drawing.Point(1042, 58);
            this.langSelector.Name = "langSelector";
            this.langSelector.Size = new System.Drawing.Size(104, 29);
            this.langSelector.TabIndex = 23;
            this.langSelector.UseSelectable = true;
            this.langSelector.SelectedIndexChanged += new System.EventHandler(this.LangSelector_SelectedIndexChanged);
            // 
            // pbFlag
            // 
            this.pbFlag.Image = global::ConquerLoader.Properties.Resources.en;
            this.pbFlag.Location = new System.Drawing.Point(994, 58);
            this.pbFlag.Name = "pbFlag";
            this.pbFlag.Size = new System.Drawing.Size(39, 29);
            this.pbFlag.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbFlag.TabIndex = 24;
            this.pbFlag.TabStop = false;
            // 
            // lblImportantInfo
            // 
            this.lblImportantInfo.AutoSize = true;
            this.lblImportantInfo.Location = new System.Drawing.Point(24, 822);
            this.lblImportantInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblImportantInfo.Name = "lblImportantInfo";
            this.lblImportantInfo.Size = new System.Drawing.Size(107, 38);
            this.lblImportantInfo.TabIndex = 25;
            this.lblImportantInfo.Text = "Important info: -\r\n";
            this.lblImportantInfo.Visible = false;
            // 
            // btnSetupLicense
            // 
            this.btnSetupLicense.Location = new System.Drawing.Point(1034, 822);
            this.btnSetupLicense.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSetupLicense.Name = "btnSetupLicense";
            this.btnSetupLicense.Size = new System.Drawing.Size(102, 38);
            this.btnSetupLicense.TabIndex = 26;
            this.btnSetupLicense.Text = "Setup License";
            this.btnSetupLicense.UseSelectable = true;
            this.btnSetupLicense.Visible = false;
            this.btnSetupLicense.Click += new System.EventHandler(this.BtnSetupLicense_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1156, 866);
            this.Controls.Add(this.btnSetupLicense);
            this.Controls.Add(this.lblImportantInfo);
            this.Controls.Add(this.metroPanel1);
            this.Controls.Add(this.langSelector);
            this.Controls.Add(this.pbFlag);
            this.Controls.Add(this.tglServerNameChange);
            this.Controls.Add(this.lblServerNameChange);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.Padding = new System.Windows.Forms.Padding(20, 92, 20, 20);
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.metroPanel1.ResumeLayout(false);
            this.metroPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewSettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFlag)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroToggle tglDebugMode;
        private MetroFramework.Controls.MetroToggle tglCloseOnFinish;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroLabel lblDebugMode;
        private MetroFramework.Controls.MetroLabel lblCloseOnFinish;
        private MetroFramework.Controls.MetroLabel lblTitle;
        private MetroFramework.Controls.MetroTextBox tbxTitle;
        private MetroFramework.Controls.MetroButton btnSave;
        private System.Windows.Forms.DataGridView gridViewSettings;
        private MetroFramework.Controls.MetroLabel lblHighResolution;
        private MetroFramework.Controls.MetroToggle tglHighResolution;
        private MetroFramework.Controls.MetroLabel lblFullscreen;
        private MetroFramework.Controls.MetroToggle tglFullscreen;
        private MetroFramework.Controls.MetroLabel lblServerNameChange;
        private MetroFramework.Controls.MetroToggle tglServerNameChange;
        private MetroFramework.Controls.MetroButton btnEdit;
        private MetroFramework.Controls.MetroButton btnWizard;
        private MetroFramework.Controls.MetroButton btnPlugins;
        private MetroFramework.Controls.MetroLabel lblDisableAutoFixFlash;
        private MetroFramework.Controls.MetroToggle tglDisableAutoFixFlash;
        private MetroFramework.Controls.MetroButton btnServerDat;
        private MetroFramework.Controls.MetroComboBox langSelector;
        private System.Windows.Forms.PictureBox pbFlag;
        private MetroFramework.Controls.MetroButton btnLockConfig;
        private MetroFramework.Controls.MetroLabel lblDisableScreenChanges;
        private MetroFramework.Controls.MetroToggle tglDisableScreenChanges;
        private MetroFramework.Controls.MetroLabel lblUseCustomDLLs;
        private MetroFramework.Controls.MetroToggle tglUseCustomDLLs;
        private MetroFramework.Controls.MetroLabel lblFPSUnlock;
        private MetroFramework.Controls.MetroToggle tglFPSUnlock;
        private MetroFramework.Controls.MetroLabel lblImportantInfo;
        private MetroFramework.Controls.MetroButton btnSetupLicense;
    }
}
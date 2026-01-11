namespace ConquerLoader.Forms
{
    partial class Main
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnStart = new MetroFramework.Controls.MetroButton();
            this.pBar = new MetroFramework.Controls.MetroProgressBar();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.cbxServers = new MetroFramework.Controls.MetroComboBox();
            this.btnLogModules = new MetroFramework.Controls.MetroButton();
            this.serverStatus = new System.Windows.Forms.Label();
            this.btnSettings = new MetroFramework.Controls.MetroButton();
            this.noty = new System.Windows.Forms.NotifyIcon(this.components);
            this.btnCloseCO = new MetroFramework.Controls.MetroButton();
            this.lblAbout = new System.Windows.Forms.LinkLabel();
            this.cbxResolutions = new MetroFramework.Controls.MetroComboBox();
            this.lblFPSUnlock = new MetroFramework.Controls.MetroLabel();
            this.tglFPSUnlock = new MetroFramework.Controls.MetroToggle();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Location = new System.Drawing.Point(216, 127);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(186, 30);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "START";
            this.btnStart.UseSelectable = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // pBar
            // 
            this.pBar.Location = new System.Drawing.Point(25, 99);
            this.pBar.Name = "pBar";
            this.pBar.Size = new System.Drawing.Size(376, 23);
            this.pBar.TabIndex = 2;
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Worker_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.Worker_ProgressChanged);
            // 
            // cbxServers
            // 
            this.cbxServers.FormattingEnabled = true;
            this.cbxServers.ItemHeight = 23;
            this.cbxServers.Location = new System.Drawing.Point(25, 127);
            this.cbxServers.Name = "cbxServers";
            this.cbxServers.PromptText = "Select a Server";
            this.cbxServers.Size = new System.Drawing.Size(186, 29);
            this.cbxServers.TabIndex = 3;
            this.cbxServers.UseSelectable = true;
            this.cbxServers.SelectedIndexChanged += new System.EventHandler(this.CbxServers_SelectedIndexChanged);
            // 
            // btnLogModules
            // 
            this.btnLogModules.ForeColor = System.Drawing.Color.White;
            this.btnLogModules.Location = new System.Drawing.Point(240, 62);
            this.btnLogModules.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogModules.Name = "btnLogModules";
            this.btnLogModules.Size = new System.Drawing.Size(103, 24);
            this.btnLogModules.TabIndex = 4;
            this.btnLogModules.Text = "Log Modules";
            this.btnLogModules.UseSelectable = true;
            this.btnLogModules.Click += new System.EventHandler(this.BtnLogModules_Click);
            // 
            // serverStatus
            // 
            this.serverStatus.AutoSize = true;
            this.serverStatus.Location = new System.Drawing.Point(22, 62);
            this.serverStatus.Name = "serverStatus";
            this.serverStatus.Size = new System.Drawing.Size(10, 13);
            this.serverStatus.TabIndex = 5;
            this.serverStatus.Text = "-";
            // 
            // btnSettings
            // 
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(347, 62);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(2);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(54, 24);
            this.btnSettings.TabIndex = 6;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseSelectable = true;
            this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // noty
            // 
            this.noty.Visible = true;
            this.noty.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // btnCloseCO
            // 
            this.btnCloseCO.ForeColor = System.Drawing.Color.White;
            this.btnCloseCO.Location = new System.Drawing.Point(162, 62);
            this.btnCloseCO.Margin = new System.Windows.Forms.Padding(2);
            this.btnCloseCO.Name = "btnCloseCO";
            this.btnCloseCO.Size = new System.Drawing.Size(74, 24);
            this.btnCloseCO.TabIndex = 7;
            this.btnCloseCO.Text = "Close CO Process";
            this.btnCloseCO.UseSelectable = true;
            this.btnCloseCO.Click += new System.EventHandler(this.BtnCloseCO_Click);
            // 
            // lblAbout
            // 
            this.lblAbout.ActiveLinkColor = System.Drawing.Color.DarkGray;
            this.lblAbout.AutoSize = true;
            this.lblAbout.LinkColor = System.Drawing.Color.DarkRed;
            this.lblAbout.Location = new System.Drawing.Point(367, 13);
            this.lblAbout.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAbout.Name = "lblAbout";
            this.lblAbout.Size = new System.Drawing.Size(35, 13);
            this.lblAbout.TabIndex = 8;
            this.lblAbout.TabStop = true;
            this.lblAbout.Text = "About";
            this.lblAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LblAbout_LinkClicked);
            // 
            // cbxResolutions
            // 
            this.cbxResolutions.FormattingEnabled = true;
            this.cbxResolutions.ItemHeight = 23;
            this.cbxResolutions.Items.AddRange(new object[] {
            "800x600",
            "1024x768",
            "1920x1080"});
            this.cbxResolutions.Location = new System.Drawing.Point(25, 162);
            this.cbxResolutions.Name = "cbxResolutions";
            this.cbxResolutions.PromptText = "Change Resolution";
            this.cbxResolutions.Size = new System.Drawing.Size(186, 29);
            this.cbxResolutions.TabIndex = 9;
            this.cbxResolutions.UseSelectable = true;
            this.cbxResolutions.SelectedIndexChanged += new System.EventHandler(this.CbxResolutions_SelectedIndexChanged);
            // 
            // lblFPSUnlock
            // 
            this.lblFPSUnlock.AutoSize = true;
            this.lblFPSUnlock.Location = new System.Drawing.Point(240, 168);
            this.lblFPSUnlock.Name = "lblFPSUnlock";
            this.lblFPSUnlock.Size = new System.Drawing.Size(72, 19);
            this.lblFPSUnlock.TabIndex = 33;
            this.lblFPSUnlock.Text = "FPs Unlock";
            // 
            // tglFPSUnlock
            // 
            this.tglFPSUnlock.AutoSize = true;
            this.tglFPSUnlock.Location = new System.Drawing.Point(317, 170);
            this.tglFPSUnlock.Margin = new System.Windows.Forms.Padding(2);
            this.tglFPSUnlock.Name = "tglFPSUnlock";
            this.tglFPSUnlock.Size = new System.Drawing.Size(80, 17);
            this.tglFPSUnlock.TabIndex = 32;
            this.tglFPSUnlock.Text = "Off";
            this.tglFPSUnlock.UseSelectable = true;
            this.tglFPSUnlock.CheckedChanged += new System.EventHandler(this.TglFPSUnlock_CheckedChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 200);
            this.Controls.Add(this.lblFPSUnlock);
            this.Controls.Add(this.tglFPSUnlock);
            this.Controls.Add(this.cbxResolutions);
            this.Controls.Add(this.lblAbout);
            this.Controls.Add(this.btnCloseCO);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.serverStatus);
            this.Controls.Add(this.btnLogModules);
            this.Controls.Add(this.cbxServers);
            this.Controls.Add(this.pBar);
            this.Controls.Add(this.btnStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Padding = new System.Windows.Forms.Padding(13, 60, 13, 13);
            this.Text = "ConquerLoader";
            this.Load += new System.EventHandler(this.Main_Load);
            this.SizeChanged += new System.EventHandler(this.TrayMinimizerForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroButton btnStart;
        private MetroFramework.Controls.MetroProgressBar pBar;
        private System.ComponentModel.BackgroundWorker worker;
        private MetroFramework.Controls.MetroComboBox cbxServers;
        private MetroFramework.Controls.MetroButton btnLogModules;
        private System.Windows.Forms.Label serverStatus;
        private MetroFramework.Controls.MetroButton btnSettings;
        private System.Windows.Forms.NotifyIcon noty;
        private MetroFramework.Controls.MetroButton btnCloseCO;
        private System.Windows.Forms.LinkLabel lblAbout;
        private MetroFramework.Controls.MetroComboBox cbxResolutions;
        private MetroFramework.Controls.MetroLabel lblFPSUnlock;
        private MetroFramework.Controls.MetroToggle tglFPSUnlock;
    }
}


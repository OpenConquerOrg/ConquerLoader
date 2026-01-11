
namespace ConquerLoader.Forms
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.pbAbout = new System.Windows.Forms.PictureBox();
            this.lblAbout1 = new System.Windows.Forms.Label();
            this.lblAbout2 = new System.Windows.Forms.Label();
            this.lblAboutTesters = new System.Windows.Forms.Label();
            this.lblAboutVersion = new System.Windows.Forms.Label();
            this.lblAboutVersionN = new System.Windows.Forms.Label();
            this.lblAboutChangelog = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pbAbout)).BeginInit();
            this.SuspendLayout();
            // 
            // pbAbout
            // 
            this.pbAbout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbAbout.Image = global::ConquerLoader.Properties.Resources.ConquerLoaderICON;
            this.pbAbout.Location = new System.Drawing.Point(34, 97);
            this.pbAbout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbAbout.Name = "pbAbout";
            this.pbAbout.Size = new System.Drawing.Size(178, 194);
            this.pbAbout.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAbout.TabIndex = 0;
            this.pbAbout.TabStop = false;
            this.pbAbout.Click += new System.EventHandler(this.PbAbout_Click);
            // 
            // lblAbout1
            // 
            this.lblAbout1.AutoSize = true;
            this.lblAbout1.Location = new System.Drawing.Point(252, 97);
            this.lblAbout1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAbout1.Name = "lblAbout1";
            this.lblAbout1.Size = new System.Drawing.Size(388, 20);
            this.lblAbout1.TabIndex = 1;
            this.lblAbout1.Text = "Created by Cristian Ocaña Soler (DaRkFoxDeveloper)";
            // 
            // lblAbout2
            // 
            this.lblAbout2.AutoSize = true;
            this.lblAbout2.Location = new System.Drawing.Point(252, 135);
            this.lblAbout2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAbout2.Name = "lblAbout2";
            this.lblAbout2.Size = new System.Drawing.Size(334, 20);
            this.lblAbout2.TabIndex = 2;
            this.lblAbout2.Text = "Translation in Portuguese by Louan Fontenele";
            // 
            // lblAboutTesters
            // 
            this.lblAboutTesters.AutoSize = true;
            this.lblAboutTesters.Location = new System.Drawing.Point(252, 260);
            this.lblAboutTesters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAboutTesters.Name = "lblAboutTesters";
            this.lblAboutTesters.Size = new System.Drawing.Size(256, 20);
            this.lblAboutTesters.TabIndex = 3;
            this.lblAboutTesters.Text = "Testers: Robert Frias, Pezzi Tomas";
            // 
            // lblAboutVersion
            // 
            this.lblAboutVersion.AutoSize = true;
            this.lblAboutVersion.Location = new System.Drawing.Point(252, 174);
            this.lblAboutVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAboutVersion.Name = "lblAboutVersion";
            this.lblAboutVersion.Size = new System.Drawing.Size(67, 20);
            this.lblAboutVersion.TabIndex = 4;
            this.lblAboutVersion.Text = "Version:";
            // 
            // lblAboutVersionN
            // 
            this.lblAboutVersionN.AutoSize = true;
            this.lblAboutVersionN.Location = new System.Drawing.Point(315, 174);
            this.lblAboutVersionN.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAboutVersionN.Name = "lblAboutVersionN";
            this.lblAboutVersionN.Size = new System.Drawing.Size(64, 20);
            this.lblAboutVersionN.TabIndex = 5;
            this.lblAboutVersionN.Text = "v1.0.0.0";
            // 
            // lblAboutChangelog
            // 
            this.lblAboutChangelog.AutoSize = true;
            this.lblAboutChangelog.Location = new System.Drawing.Point(393, 174);
            this.lblAboutChangelog.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAboutChangelog.Name = "lblAboutChangelog";
            this.lblAboutChangelog.Size = new System.Drawing.Size(86, 20);
            this.lblAboutChangelog.TabIndex = 6;
            this.lblAboutChangelog.TabStop = true;
            this.lblAboutChangelog.Text = "Changelog";
            this.lblAboutChangelog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LblAboutChangelog_LinkClicked);
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 311);
            this.Controls.Add(this.lblAboutChangelog);
            this.Controls.Add(this.lblAboutVersionN);
            this.Controls.Add(this.lblAboutVersion);
            this.Controls.Add(this.lblAboutTesters);
            this.Controls.Add(this.lblAbout2);
            this.Controls.Add(this.lblAbout1);
            this.Controls.Add(this.pbAbout);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "About";
            this.Padding = new System.Windows.Forms.Padding(30, 92, 30, 31);
            this.Resizable = false;
            this.Text = "About";
            this.Load += new System.EventHandler(this.About_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbAbout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbAbout;
        private System.Windows.Forms.Label lblAbout1;
        private System.Windows.Forms.Label lblAbout2;
        private System.Windows.Forms.Label lblAboutTesters;
        private System.Windows.Forms.Label lblAboutVersion;
        private System.Windows.Forms.Label lblAboutVersionN;
        private System.Windows.Forms.LinkLabel lblAboutChangelog;
    }
}
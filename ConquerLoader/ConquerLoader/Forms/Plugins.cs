using CLCore;
using System;
using System.Drawing;

namespace ConquerLoader.Forms
{
    public partial class Plugins : MetroFramework.Forms.MetroForm
    {
        public Plugins()
        {
            InitializeComponent();
        }

        private void Plugins_Load(object sender, System.EventArgs e)
        {
            int initLocationX = 23;
            int initLocationY = 76;
            int separationRight = 20;
            int separationBottom = 20;
            int n = 0;
            foreach (IPlugin plugin in CLCore.PluginLoader.Plugins)
            {
                MetroFramework.Controls.MetroButton mBtn = new MetroFramework.Controls.MetroButton
                {
                    Text = plugin.Name,//+ (plugin.RemotePlugin ? " [Remote]" : ""),
                    Location = new Point(initLocationX, initLocationY),
                    Size = new Size(161, 50)
                };
                initLocationX += mBtn.Width + separationRight;
                if (n >= 3)
                {
                    initLocationY += mBtn.Height + separationBottom;
                }
                mBtn.Tag = plugin;
                mBtn.Click += MBtn_Click;
                mBtn.Paint += MBtn_Paint;
                this.Controls.Add(mBtn);
                n++;
            }
        }

        private void MBtn_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            MetroFramework.Controls.MetroButton btn = (MetroFramework.Controls.MetroButton)sender;
            float fontSize = NewFontSize(e.Graphics, btn.Size, btn.Font, btn.Text);
            Font f = new Font("Arial", fontSize, FontStyle.Bold);
            btn.Font = f;
        }

        private void MBtn_Click(object sender, System.EventArgs e)
        {
            IPlugin plugin = ((IPlugin)((MetroFramework.Controls.MetroButton)sender).Tag);
            if (plugin != null) plugin.Configure();
        }
        public static float NewFontSize(Graphics graphics, Size size, Font font, string str)
        {
            SizeF stringSize = graphics.MeasureString(str, font);
            float wRatio = size.Width / stringSize.Width;
            float hRatio = size.Height / stringSize.Height;
            float ratio = Math.Min(hRatio, wRatio);
            return font.Size * ratio;
        }

        private void Plugins_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            // Prevent the form from closing.
            e.Cancel = true;
            // Hide it instead.
            this.Hide();
        }
    }
}

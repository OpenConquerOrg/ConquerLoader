using CLCore;
using CLCore.Models;
using MetroFramework.Controls;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace ConquerLoader
{
    public partial class Wizard : MetroFramework.Forms.MetroForm
    {
        private int _IndexSelectedServer = -1;
        private ServerConfiguration _SelectedServer = null;
        private LoaderConfig _LoaderConfig = null;
        public Wizard()
        {
            InitializeComponent();
            Core.LoadControlTranslations(Controls);
            _LoaderConfig = Core.GetLoaderConfig();
        }
        public Wizard(int IndexSelectedServer)
        {
            InitializeComponent();
            Core.LoadControlTranslations(Controls);
            _IndexSelectedServer = IndexSelectedServer;
            _LoaderConfig = Core.GetLoaderConfig();
        }

        private void Wizard_Load(object sender, System.EventArgs e)
        {
            if (_IndexSelectedServer >= 0)
            {
                _SelectedServer = _LoaderConfig.Servers[_IndexSelectedServer];
                if (_SelectedServer.Group == null)
                {
                    _SelectedServer.Group = new ServerDatGroup();
                }
                LoadServerDatGroups();
                Text = "Edit Server";
                btnAdd.Text = "Edit in List";
                tbxServerName.Text = _SelectedServer.ServerName;
                tbxIP.Text = _SelectedServer.LoginHost;
                tbxConquerExe.Text = _SelectedServer.ExecutableName;
                tbxLoginPort.Text = _SelectedServer.LoginPort.ToString();
                tbxGamePort.Text = _SelectedServer.GamePort.ToString();
                tbxGroupIcon.Text = _SelectedServer.ServerIcon;
                tbxGroup.Text = _SelectedServer.Group.GroupName;
                tbxVersion.Text = _SelectedServer.ServerVersion.ToString();
                tglUseDX9.Checked = _SelectedServer.UseDirectX9;
            } else
            {
                string VersionDatFilename = Path.Combine(Directory.GetCurrentDirectory(), "Version.dat");
                string[] VersionDatLines = new string[] { };
                if (File.Exists("Version.dat"))
                {
                    VersionDatLines = File.ReadAllLines(VersionDatFilename);
                }
                int RealVersion = 0;
                if (VersionDatLines.Length == 1) // Default Version.dat detected
                {
                    File.WriteAllText("Version.dat", "99999" + System.Environment.NewLine + $"#{VersionDatLines[0]}");
                    VersionDatLines = File.ReadAllLines(VersionDatFilename);
                }
                foreach (string str in VersionDatLines)
                {
                    if (str.StartsWith("#"))
                    {
                        string strFixed = str.Replace("#", "");
                        int nVersion = int.Parse(strFixed);
                        if (nVersion > 4000)
                        {
                            RealVersion = nVersion;
                        }
                    }
                }
                tbxVersion.Text = RealVersion.ToString();
                tbxConquerExe.Text = "Conquer.exe"; // Default Conquer Executable
                tbxLoginPort.Text = "9958";
                tbxGamePort.Text = "5816";
                LoadServerDatGroups();
            }
        }

        private void LoadServerDatGroups()
        {
            string p = Path.Combine(Constants.ClientPath, "data", "main", "flash");
            if (Directory.Exists(p))
            {
                List<string> Groups = new List<string>();
                foreach (string s in Directory.GetDirectories(p, "Group*"))
                {
                    string dir = Path.GetFileName(s);
                    Groups.Add(dir);
                }
                tbxGroup.DataSource = Groups;
            }
        }

        private void BtnAdd_Click(object sender, System.EventArgs e)
        {
            if (_IndexSelectedServer >= 0)
            {
                List<string> valids = ServerDatGroupIcons();
                if (!valids.Contains(tbxGroupIcon.Text) && int.Parse(tbxVersion.Text) >= 6000)
                {
                    MessageBox.Show("This group icon is invalid for this client. You don't see this group icon.", "Error detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _SelectedServer.ServerName = tbxServerName.Text;
                _SelectedServer.LoginHost = tbxIP.Text;
                _SelectedServer.GameHost = tbxIP.Text;
                _SelectedServer.ServerVersion = uint.Parse(tbxVersion.Text);
                _SelectedServer.ExecutableName = tbxConquerExe.Text;
                _SelectedServer.LoginPort = uint.Parse(tbxLoginPort.Text);
                _SelectedServer.GamePort = uint.Parse(tbxGamePort.Text);
                _SelectedServer.UseDirectX9 = tglUseDX9.Checked;
                _SelectedServer.ServerIcon = tbxGroupIcon.Text;
                if (_SelectedServer.Group != null)
                {
                    _SelectedServer.Group.GroupIcon = $"{tbxGroup.Text}.swf";
                    _SelectedServer.Group.GroupName = tbxGroup.Text;
                } else
                {
                    _SelectedServer.Group = new ServerDatGroup() { GroupIcon = $"{tbxGroup.Text}.swf", GroupName = tbxGroup.Text };
                }
            }
            else
            {
                List<string> valids = ServerDatGroupIcons();
                if (!valids.Contains(tbxGroupIcon.Text) && int.Parse(tbxVersion.Text) >= 6000)
                {
                    MessageBox.Show("This group icon is invalid for this client. You don't see this group icon.", "Error detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _LoaderConfig.Servers.Add(new ServerConfiguration()
                {
                    GameHost = tbxIP.Text,
                    LoginHost = tbxIP.Text,
                    ExecutableName = tbxConquerExe.Text,
                    ServerName = tbxServerName.Text,
                    UseDirectX9 = tglUseDX9.Checked,
                    ServerVersion = uint.Parse(tbxVersion.Text),
                    LoginPort = 9958,
                    GamePort = 5816,
                    Group = new ServerDatGroup() { GroupIcon = $"{tbxGroup.Text}.swf", GroupName = tbxGroup.Text },
                    ServerIcon = tbxGroupIcon.Text
                });
            }
            Core.SaveLoaderConfig(_LoaderConfig);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Wizard_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            // Prevent the form from closing.
            e.Cancel = true;

            // Hide it instead.
            this.Hide();
        }

        private void TbxVersion_TextChanged(object sender, System.EventArgs e)
        {
            bool isNumber = int.TryParse(tbxVersion.Text, out _);
            if (isNumber)
            {
                if (tbxVersion.Text.Length > 0 && int.Parse(tbxVersion.Text) >= Constants.MinVersionUseRAWServerDat)
                {
                    tbxGamePort.Text = "5816";
                    tbxGroupIcon.Visible = true;
                    lblGroupIcon.Visible = true;
                    lblHelpGroup.Visible = true;
                    tbxGroup.Visible = true;
                }
                else
                {
                    tbxGroupIcon.Visible = false;
                    lblGroupIcon.Visible = false;
                    lblHelpGroup.Visible = false;
                    tbxGroup.Visible = false;
                }
                tglUseDX9.Visible = true;
                lblUseDX9.Visible = true;
                if (tbxVersion.Text.Length > 0 && int.Parse(tbxVersion.Text) < Constants.MinVersionUseDX8DX9Folders)
                {
                    tglUseDX9.Visible = false;
                    lblUseDX9.Visible = false;
                    tglUseDX9.Checked = false;
                }
            }
        }

        private void ValidateNumber_Leave(object sender, System.EventArgs e)
        {
            MetroFramework.Controls.MetroTextBox tbx = (MetroFramework.Controls.MetroTextBox)sender;
            bool isNumber = int.TryParse(tbx.Text, out int n);
            if (!isNumber)
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

        private void TbxIP_Leave(object sender, System.EventArgs e)
        {
            MetroFramework.Controls.MetroTextBox tbx = (MetroFramework.Controls.MetroTextBox)sender;
            bool validIP = IPAddress.TryParse(tbx.Text, out IPAddress IP);
            if (!validIP)
            {
                tbx.Text = "";
                lblHelpIP.Text = "Please, write a valid IP Address.";
            }
        }

        private List<string> ServerDatGroupIcons()
        {
            List<string> GroupIcons = new List<string>();
            string p = Path.Combine(Constants.ClientPath, "data", "main", "flash");
            if (Directory.Exists(p))
            {
                foreach (string s in Directory.GetDirectories(p, $"{tbxGroup.Text}"))
                {
                    string dir = Path.GetFileName(s);
                    foreach (string f in Directory.GetFiles(s))
                    {
                        GroupIcons.Add(dir + "/" + Path.GetFileName(f));
                    }
                }
            }
            return GroupIcons;
        }

        private void TbxGroup_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (tbxGroup.Text.Length <= 0)
            {
                MessageBox.Show("Any group specified", "Specify one valid group", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            tbxGroupIcon.Items.Clear();
            string p = Path.Combine(Constants.ClientPath, "data", "main", "flash");
            string AvailableGroupsNameInClient = "";
            if (Directory.Exists(p))
            {
                foreach (string s in Directory.GetDirectories(p, $"{tbxGroup.Text}"))
                {
                    string dir = Path.GetFileName(s);
                    foreach (string f in Directory.GetFiles(s))
                    {
                        AvailableGroupsNameInClient += dir + "/" + Path.GetFileName(f) + System.Environment.NewLine;
                        if (!tbxGroupIcon.Items.Contains(dir + "/" + Path.GetFileName(f)))
                        {
                            tbxGroupIcon.Items.Add(dir + "/" + Path.GetFileName(f));
                        }
                    }
                }
                //MessageBox.Show(AvailableGroupsNameInClient, "Available Group Icons", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //MessageBox.Show("Any found", "Not detected swf files!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

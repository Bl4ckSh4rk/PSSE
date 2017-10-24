using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pokemon_Shuffle_Save_Editor
{
    public partial class Resources_Popup : Form
    {
        private string dir;
        private string url = "https://github.com/supercarotte/PSSE/wiki/Extract-needed-resource-files-from-the-game";
        private bool dev = false;

        public bool[] retChk
        {
            get
            {
                if (!dev) { return new bool[] { true, true, true, true, true, true, true, true, true, true }; }
                bool[] retChk = new bool[TLP_Files.RowCount - 1];
                for (int i = 0; i < retChk.Length; i++)
                {
                    CheckBox CB = TLP_Files.GetControlFromPosition(3, i + 1) as CheckBox;
                    retChk[i] = (CB != null) ? CB.Checked : false;
                }
                return retChk;
            }
        }

        public Resources_Popup(string[] filesN, string resourcedir, bool dv)
        {
            InitializeComponent();
            dir = resourcedir;
            dev = dv;
            if (!Directory.Exists(resourcedir))
            {
                L_Intro.Text = "No resources folder found.\nClick the OK Button to create it.";
                TLP_Files.RowCount = 0;
                TLP_Files.Visible = TLP_Files.Enabled = false;
                this.Size = this.MinimumSize;
            }
            else
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;
                L_Intro.Text = "Resources folder detected, files lsited below will be used over PSSE's default.\n(Current PSSE version : " + version + ")";
                while (TLP_Files.RowCount != filesN.Length + 1)
                {
                    if (TLP_Files.RowCount < filesN.Length + 1)
                    {
                        TLP_Files.RowCount = TLP_Files.RowCount + 1;
                        TLP_Files.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
                    }
                    else if (TLP_Files.RowCount > filesN.Length) { TLP_Files.RowCount = filesN.Length + 1; }
                }

                if (!dev)
                {
                    TLP_Files.Controls.Remove(L_Date);
                    TLP_Files.Controls.Remove(L_Use);
                    TLP_Files.ColumnCount = 2;
                    L_Date.Enabled = L_Use.Enabled = false;
                }
                int j = 1;
                foreach (string str in filesN)
                {
                    TLP_Files.Controls.Add(new Label() { Text = str, Anchor = AnchorStyles.Left, AutoSize = true }, 0, j);
                    if (File.Exists(resourcedir + str))
                    {
                        TLP_Files.Controls.Add(new PictureBox() { Image = new Bitmap((Image)Properties.Resources.ResourceManager.GetObject("valid")), AutoSize = true }, 1, j);
                        if (dev)
                        {
                            TLP_Files.Controls.Add(new Label() { Text = File.GetLastWriteTime(resourcedir + str).ToString(), Anchor = AnchorStyles.Left, AutoSize = true }, 2, j);
                            TLP_Files.Controls.Add(new CheckBox() { Checked = true, Anchor = AnchorStyles.Left, AutoSize = true }, 3, j);
                        }
                    }
                    j++;
                }
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)  //Allows quit when Esc is pressed
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void B_OK_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(dir))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else //if no resource folder, create it and put a link to github's wiki page inside it.
            {
                Directory.CreateDirectory(dir);
                using (StreamWriter writer = new StreamWriter(dir + "\\" + "More informations here (Wiki)" + ".url"))
                {
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=" + url);
                    writer.Flush();
                }
                System.Diagnostics.Process.Start(dir);
                this.Close();
            }
        }

        private void LL_Wiki_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LL_Wiki.LinkVisited = true;
            System.Diagnostics.Process.Start(url);
        }
    }
}
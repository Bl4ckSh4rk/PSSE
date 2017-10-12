using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Pokemon_Shuffle_Save_Editor.ToolFunctions;

namespace Pokemon_Shuffle_Save_Editor
{
    public partial class Main : Form
    {
        public static Database db = new Database();
        public static byte[] savedata = null;
        public static Keys lastkeys;    //to detect if ItemsGrid was entered from Tab or Shift+Tab

        private List<cbItem> monsel = new List<cbItem>();
        private List<cbItem> skillsel = new List<cbItem>();
        private ShuffleItems SI_Items = new ShuffleItems();

        private bool loaded, updating;
        public static int ltir; //Last TeamIndex Right-clicked, -1 is default

        public Main()
        {
            InitializeComponent();
            for (int i = 1; i < db.MonStopIndex; i++)
                monsel.Add(new cbItem { Text = db.MonsList[i], Value = i });
            monsel = monsel.OrderBy(x => x.Text).ToList();
            CB_MonIndex.DataSource = monsel;
            CB_MonIndex.DisplayMember = "Text";
            CB_MonIndex.ValueMember = "Value";
            CB_MonIndex.DropDownStyle = ComboBoxStyle.DropDown; //DropDownList is similar but user won't see what it types.
            CB_MonIndex.AutoCompleteSource = AutoCompleteSource.ListItems;
            CB_MonIndex.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            FormInit();
        }

        private void FormInit()
        {
            CB_MonIndex.SelectedIndex = 0;
            B_Save.Enabled = GB_Caught.Enabled = GB_HighScore.Enabled = GB_Resources.Enabled = B_CheatsForm.Enabled = ItemsGrid.Enabled = loaded = false;
            PB_Mon.Image = GetCaughtImage((int)CB_MonIndex.SelectedValue, CHK_CaughtMon.Checked);
            PB_Main.Image = PB_Event.Image = PB_Expert.Image = GetStageImage(0, 0);
            PB_Team1.Image = PB_Team2.Image = PB_Team3.Image = PB_Team4.Image = ResizeImage(GetMonImage(0), 48, 48);
            PB_Lollipop.Image = new Bitmap(ResizeImage((Image)Properties.Resources.ResourceManager.GetObject("lollipop"), 24, 24));
            PB_Skill.Image = new Bitmap(ResizeImage((Image)Properties.Resources.ResourceManager.GetObject("skill"), 24, 24));
            PB_Lollipop.Visible = PB_Skill.Visible = false;
            RB_Skill1.Visible = RB_Skill2.Visible = RB_Skill3.Visible = RB_Skill4.Visible = RB_Skill5.Visible = false;
            NUP_Skill1.Visible = NUP_Skill2.Visible = NUP_Skill3.Visible = NUP_Skill4.Visible = NUP_Skill5.Visible = false;
            L_Skill1.Visible = L_Skill2.Visible = L_Skill3.Visible = L_Skill4.Visible = L_Skill5.Visible = false;
            NUP_MainIndex.Maximum = BitConverter.ToInt32(db.StagesMain, 0) - 1;
            NUP_ExpertIndex.Maximum = BitConverter.ToInt32(db.StagesExpert, 0);
            NUP_EventIndex.Maximum = BitConverter.ToInt32(db.StagesEvent, 0) - 1;
            ItemsGrid.SelectedObject = null;
            TB_FilePath.Text = string.Empty;
            savedata = null;
            ltir = -1;
        }

        private void Open(string file)
        {
            if (!IsShuffleSave(file))
            {
                MessageBox.Show("Couldn't open your file, it wasn't detected as a proper Pokemon Shuffle savefile.");
                return;
            }

            B_Save.Enabled = GB_Caught.Enabled = GB_HighScore.Enabled = GB_Resources.Enabled = B_CheatsForm.Enabled = ItemsGrid.Enabled = loaded = true;
            TB_FilePath.Text = file;
            savedata = File.ReadAllBytes(file);
            UpdateForm(null, null);
        }

        private bool IsShuffleSave(string file) // Try to do a better job at filtering files rather than just saying "oh, it's not savedata.bin quit"
        {
            FileInfo info = new FileInfo(file);
            if (info.Length != 74807) return false; // Probably not

            var contents = new byte[8];
            File.OpenRead(file).Read(contents, 0, contents.Length);
            return BitConverter.ToInt64(contents, 0) == 0x4000000009L;
        }

        private void Parse()
        {
            updating = true;

            #region UpdateOwnedBox()
            int ind = (int)CB_MonIndex.SelectedValue;

            //team preview
            int pbIndex = 0;
            foreach (PictureBox pb in new[] { PB_Team1, PB_Team2, PB_Team3, PB_Team4 })
            {
                pb.Image = GetTeamImage(GetMonTeamSlot(pbIndex), (ltir == pbIndex));
                pbIndex++;
            }

            //caught CHK
            CHK_CaughtMon.Checked = GetMon(ind).Caught;

            //level view
            NUP_Lollipop.Maximum = db.Mons[ind].Item4;
            NUP_Lollipop.Value = GetMon(ind).Lollipops;
            NUP_Level.Maximum = 10 + NUP_Lollipop.Maximum;
            NUP_Level.Value = GetMon(ind).Level;

            //Skill level
            for (int i = 0; i < TLP_Skills.ColumnCount; i++)
            {
                for (int j = 0; j < TLP_Skills.RowCount; j++)
                {
                    if (TLP_Skills.GetControlFromPosition(i, j) is RadioButton)
                        (TLP_Skills.GetControlFromPosition(i, j) as RadioButton).Checked = (GetMon(ind).CurrentSkill == j);
                    else if (TLP_Skills.GetControlFromPosition(i, j) is Label)
                    {
                        (TLP_Skills.GetControlFromPosition(i, j) as Label).Text = (db.Mons[(int)CB_MonIndex.SelectedValue].Item6[j] != 0 || j == 0) ? db.SkillsList[db.Mons[(int)CB_MonIndex.SelectedValue].Item6[j] - 1] : "";
                        TT_Skill.SetToolTip((TLP_Skills.GetControlFromPosition(i, j) as Label), (db.Mons[ind].Item6[j] > 0) ? db.SkillsTextList[db.Mons[ind].Item6[j] - 1] : "default");
                    }
                    else if (TLP_Skills.GetControlFromPosition(i, j) is NumericUpDown)
                        (TLP_Skills.GetControlFromPosition(i, j) as NumericUpDown).Value = Math.Max(GetMon(ind).SkillLevel[j], 1);

                    (TLP_Skills.GetControlFromPosition(i, j) as Control).Visible = (GetMon((int)CB_MonIndex.SelectedValue).Caught && j < db.Mons[ind].Rest.Item2); //visibility stuff for convenience
                }
            }

            //Speedup values
            if (db.MegaList.IndexOf(ind) != -1) //temporary fix while there are still some mega forms missing in megastone.bin
            {
                NUP_SpeedUpX.Maximum = db.HasMega[ind][0] ? db.Megas[db.MegaList.IndexOf(ind)].Item2 : 0;
                NUP_SpeedUpY.Maximum = db.HasMega[ind][1] ? db.Megas[db.MegaList.IndexOf(ind, db.MegaList.IndexOf(ind) + 1)].Item2 : 0;
                NUP_SpeedUpX.Value = GetMon(ind).SpeedUpX;
                NUP_SpeedUpY.Value = GetMon(ind).SpeedUpY;
            }
            else
            {
                NUP_SpeedUpX.Maximum = NUP_SpeedUpY.Maximum = 1;
                NUP_SpeedUpX.Value = NUP_SpeedUpY.Value = 0;
            }

            #region Visibility

            L_Level.Visible = L_Skill.Visible = NUP_Level.Visible = PB_Skill.Visible = CHK_CaughtMon.Checked;
            PB_Lollipop.Visible = NUP_Lollipop.Visible = (CHK_CaughtMon.Checked && NUP_Lollipop.Maximum != 0);
            PB_Mon.Image = GetCaughtImage(ind, CHK_CaughtMon.Checked);
            PB_MegaX.Visible = CHK_MegaX.Visible = db.HasMega[ind][0];
            PB_MegaY.Visible = CHK_MegaY.Visible = db.HasMega[ind][1];
            PB_MegaX.Image = db.HasMega[ind][0] ? new Bitmap((Image)Properties.Resources.ResourceManager.GetObject("MegaStone" + db.Mons[ind].Item1.ToString("000") + (db.HasMega[ind][1] ? "_X" : string.Empty))) : new Bitmap(16, 16);
            PB_MegaY.Image = db.HasMega[ind][1] ? new Bitmap((Image)Properties.Resources.ResourceManager.GetObject("MegaStone" + db.Mons[ind].Item1.ToString("000") + "_Y")) : new Bitmap(16, 16);
            CHK_MegaX.Checked = (GetMon(ind).Stone & 1) != 0;
            CHK_MegaY.Checked = (GetMon(ind).Stone & 2) != 0;
            NUP_SpeedUpX.Visible = PB_SpeedUpX.Visible = CHK_CaughtMon.Checked && CHK_MegaX.Visible && CHK_MegaX.Checked;
            NUP_SpeedUpY.Visible = PB_SpeedUpY.Visible = CHK_CaughtMon.Checked && CHK_MegaY.Visible && CHK_MegaY.Checked; //Else NUP_SpeedUpY appears if the next mega in terms of offsets has been obtained
            PB_SpeedUpX.Image = db.HasMega[ind][0] ? new Bitmap(ResizeImage((Image)Properties.Resources.ResourceManager.GetObject("mega_speedup"), 24, 24)) : new Bitmap(16, 16);
            PB_SpeedUpY.Image = db.HasMega[ind][1] ? new Bitmap(ResizeImage((Image)Properties.Resources.ResourceManager.GetObject("mega_speedup"), 24, 24)) : new Bitmap(16, 16);
            RB_Skill1.Enabled = (db.Mons[ind].Rest.Item2 > 1);
            #endregion Visibility            
            #endregion

            #region UpdateResourceBox()
            rsItem rsI = GetRessources();
            NUP_Coins.Value = rsI.Coins;
            NUP_Jewels.Value = rsI.Jewels;
            NUP_Hearts.Value = rsI.Hearts;
            for (int i = 0; i < SI_Items.Items.Length; i++)
                SI_Items.Items[i] = rsI.Items[i];
            for (int i = 0; i < SI_Items.Enchantments.Length; i++)
                SI_Items.Enchantments[i] = rsI.Enhancements[i];
            ItemsGrid.Refresh();
            #endregion

            #region UpdateStageBox()
            Label[] labels = new[] { L_RankM, L_RankEx, L_RankEv };
            NumericUpDown[] nups = new[] { NUP_MainScore, NUP_ExpertScore, NUP_EventScore };
            PictureBox[] pics = new[] { PB_Main, PB_Expert, PB_Event };
            int[] index = new int[] { (int)NUP_MainIndex.Value - 1, (int)NUP_ExpertIndex.Value - 1, (int)NUP_EventIndex.Value };
            byte[][] stages = new byte[][] { db.StagesMain, db.StagesExpert, db.StagesEvent };

            for (int i = 0; i < labels.Length; i++)
            {
                stgItem stgI = GetStage(index[i], i);
                GetRankImage(labels[i], stgI.Rank, stgI.Completed);
                nups[i].Value = stgI.Score;
                pics[i].Image = GetStageImage(BitConverter.ToInt16(stages[i], 0x50 + BitConverter.ToInt32(stages[i], 0x4) * ((i == 0) ? index[i] + 1 : index[i])) & 0x7FF, i);
            }
            #endregion

            updating = false;
        }

        private void UpdateForm(object sender, EventArgs e)
        {
            if (!loaded || updating)
                return;
            updating = true;
            if(!(new Control[] { null, CB_MonIndex, NUP_MainIndex, NUP_ExpertIndex, NUP_ExpertScore}.Contains(sender)))
            {
                //Owned Box Properties
                int ind = (int)CB_MonIndex.SelectedValue;
                ushort set_level = (ushort)(CHK_CaughtMon.Checked ? (NUP_Level.Value == 1 ? 0 : NUP_Level.Value) : 0);
                ushort set_rml = (ushort)(CHK_CaughtMon.Checked ? NUP_Lollipop.Value : 0);
                if (set_level > 10 + set_rml)
                {
                    if ((sender as Control).Name.Contains("Level"))
                        set_rml = (ushort)(set_level - 10);
                    if ((sender as Control).Name.Contains("Lollipop"))
                        set_level = (ushort)(10 + set_rml);
                }
                SetCaught(ind, CHK_CaughtMon.Checked);
                SetLevel(ind, set_level, set_rml);
                SetStone(ind, CHK_MegaX.Checked, CHK_MegaY.Checked);
                SetSpeedup(ind, (db.HasMega[ind][0] && CHK_CaughtMon.Checked && CHK_MegaX.Checked), (int)NUP_SpeedUpX.Value, (db.HasMega[ind][1] && CHK_CaughtMon.Checked && CHK_MegaY.Checked), (int)NUP_SpeedUpY.Value);
                for (int j = 0; j < TLP_Skills.RowCount; j++)
                {
                    int skillLevel = 1;
                    bool iscurrent = false;
                    for (int i = 0; i < TLP_Skills.ColumnCount; i++)
                    {
                        if (TLP_Skills.GetControlFromPosition(i, j) is RadioButton)
                            iscurrent = (TLP_Skills.GetControlFromPosition(i, j) as RadioButton).Checked;
                        else if (TLP_Skills.GetControlFromPosition(i, j) is NumericUpDown)
                            skillLevel = (int)(TLP_Skills.GetControlFromPosition(i, j) as NumericUpDown).Value;
                    }
                    SetSkill(ind, j, (GetMon(ind).Caught) ? skillLevel : 1, iscurrent);
                }

                //Stages Box Properties
                SetScore((int)NUP_MainIndex.Value - 1, 0, (int)NUP_MainScore.Value);
                SetScore((int)NUP_ExpertIndex.Value - 1, 1, (int)NUP_ExpertScore.Value);
                SetScore((int)NUP_EventIndex.Value, 2, (int)NUP_EventScore.Value);

                //Ressources Box Properties
                SetResources((int)NUP_Hearts.Value, (uint)NUP_Coins.Value, (uint)NUP_Jewels.Value, SI_Items.Items, SI_Items.Enchantments);
            }
            Parse();
            updating = false;
        }

        private void B_CheatsForm_Click(object sender, EventArgs e)
        {
            new Cheats().ShowDialog();
            Parse();
        }

        private void B_Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { FileName = "savedata.bin", Filter = ".bin files (*.bin)|*.bin|All files (*.*)|*.*", FilterIndex = 1 };
            if (ofd.ShowDialog() == DialogResult.OK)
                Open(ofd.FileName);
        }

        private void B_Save_Click(object sender, EventArgs e)
        {
            if (!loaded || updating)
                return;
            updating = true;
            SaveFileDialog sfd = new SaveFileDialog { FileName = Path.GetFileName(TB_FilePath.Text), Filter = ".bin files (*.bin)|*.bin|All files (*.*)|*.*", FilterIndex = 1 };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, savedata);
            MessageBox.Show("Saved save file to " + sfd.FileName + ".");
            }            
            updating = false;
        }

        private void PB_Owned_Click(object sender, EventArgs e)
        {
            updating = true;
            if ((sender as Control).Name.Contains("SpeedUpX"))
                NUP_SpeedUpX.Value = (NUP_SpeedUpX.Value == 0) ? NUP_SpeedUpX.Maximum : 0;
            else if ((sender as Control).Name.Contains("SpeedUpY"))
                NUP_SpeedUpY.Value = (NUP_SpeedUpY.Value == 0) ? NUP_SpeedUpY.Maximum : 0;
            else if ((sender as Control).Name.Contains("Lollipop"))
            {
                NUP_Lollipop.Value = (NUP_Lollipop.Value == 0) ? NUP_Lollipop.Maximum : 0;
                NUP_Level.Value = 10 + NUP_Lollipop.Value;
            }
            else if ((sender as Control).Name.Contains("Mon"))
            {
                if (!CHK_CaughtMon.Checked)
                {
                    CHK_CaughtMon.Checked = true;
                    NUP_Lollipop.Value = NUP_Lollipop.Maximum;
                    NUP_Level.Value = NUP_Level.Maximum;
                    CHK_MegaX.Checked = db.HasMega[(int)CB_MonIndex.SelectedValue][0];
                    CHK_MegaY.Checked = db.HasMega[(int)CB_MonIndex.SelectedValue][1];
                    NUP_SpeedUpX.Value = (db.HasMega[(int)CB_MonIndex.SelectedValue][0]) ? NUP_SpeedUpX.Maximum : 0;
                    NUP_SpeedUpY.Value = (db.HasMega[(int)CB_MonIndex.SelectedValue][1]) ? NUP_SpeedUpY.Maximum : 0;
                    for (int i = 0; i < db.Mons[(int)CB_MonIndex.SelectedValue].Rest.Item2; i++)
                        SetSkill((int)CB_MonIndex.SelectedValue, i, (int)NUP_Skill1.Maximum);
                }
                else CHK_CaughtMon.Checked = CHK_MegaX.Checked = CHK_MegaY.Checked = false;
            }
            else if ((sender as Control).Name.Contains("Skill"))
            {
                bool boool = false;
                foreach (int sLv in GetMon((int)CB_MonIndex.SelectedValue).SkillLevel)
                {
                    if (sLv != 5 && sLv != 0)
                        boool = true;
                }
                for (int i = 0; i < db.Rest[(int)CB_MonIndex.SelectedValue].Item2; i++)
                    SetSkill((int)CB_MonIndex.SelectedValue, i, boool ? 5 : 1);
            }
            else return;
            Parse();
            updating = false;
        }

        private void PB_Stage_Click(object sender, EventArgs e)
        {
            updating = true;
            int type, ind;
            //int max;
            if ((sender as Control).Name.Contains("Main"))
            {
                type = 0;
                ind = (int)NUP_MainIndex.Value - 1;
                //max = (int)NUP_MainIndex.Maximum;
            }
            else if ((sender as Control).Name.Contains("Expert"))
            {
                type = 1;
                ind = (int)NUP_ExpertIndex.Value - 1;
                //max = (int)NUP_ExpertIndex.Maximum;
            }
            else if ((sender as Control).Name.Contains("Event"))
            {
                type = 2;
                ind = (int)NUP_EventIndex.Value;
                //max = (int)NUP_EventIndex.Maximum + 1;
            }
            else return;
            if ((e as MouseEventArgs).Button == MouseButtons.Left)    //Left Click = circle ranks down
            {
                if (GetStage(ind, type).Completed)
                {
                    SetRank(ind, type, (GetStage(ind, type).Rank + 3) % 4);
                    if (GetStage(ind, type).Rank == 3) { PatchScore(ind, type); }
                }
                //Nothing happens if uncompleted
            }
            if ((e as MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Right)   //Right Click = (un)completed
            {
                SetStage(ind, type, !GetStage(ind, type).Completed);    //invert completed state
                SetRank(ind, type, GetStage(ind, type).Completed ? 3 : 0);  //rank S or C
                PatchScore(ind, type);
                #region needs better implementation idea
                //if (GetStage(ind, i).Completed && i == 0)
                //{
                //    for (int j = 0; j < ind; j++) //mark every previous stage as completed
                //    {
                //        SetStage(j, i, true);
                //        PatchScore(j, i);
                //    }
                //}
                //else if (i == 0)
                //{
                //    for (int j = ind; j < max; j++) //mark every next stage as uncompleted
                //    {
                //        SetStage(j, i);
                //        SetRank(j, i, 0);
                //        SetScore(j, i, 0);
                //    }
                //}
                #endregion
            }
            Parse();
            updating = false;
        }

        private void PB_Team_Click(object sender, EventArgs e)
        {
            string[] senderNames = new string[] { "Team1", "Team2", "Team3", "Team4" };
            int s = -1;
            for (int i = 0; i < senderNames.Length; i++)
            {
                if ((sender as Control).Name.Contains(senderNames[i]))
                    s = i;
                if (s != -1) { break; }
            }
            if (s < 0 || s >= senderNames.Length) { return; }
            if ((e as MouseEventArgs).Button != MouseButtons.Left && (e as MouseEventArgs).Button != MouseButtons.Right) { return; }

            updating = true;
            if ((e as MouseEventArgs).Button == MouseButtons.Left)
            {
                if (ModifierKeys == Keys.Control)
                {
                    int ind = (int)CB_MonIndex.SelectedValue;
                    SetCaught(ind, true);
                    for (int i = 0; i < senderNames.Length; i++)
                    {
                        if (i != s && GetMonTeamSlot(i) == ind)
                            SetMonTeamSlot(i, GetMonTeamSlot(s));
                    }
                    SetMonTeamSlot(s, ind);
                }
                else
                    CB_MonIndex.SelectedValue = GetMonTeamSlot(s);
            }
            else if ((e as MouseEventArgs).Button == MouseButtons.Right)
            {
                if (ltir == -1)
                    ltir = s;
                else
                {
                    int temp = GetMonTeamSlot(ltir); //important variable, otherwise next instruction changes which pokemon to be set by the one just after.
                    SetMonTeamSlot(ltir, GetMonTeamSlot(s));
                    SetMonTeamSlot(s, temp);
                    ltir = -1;
                }
            }
            Parse();
            updating = false;
        }

        private void TB_Filepath_DoubleClick(object sender, EventArgs e)
        {
            if ((sender as Control).Enabled == false)
                return;
            updating = true;
            loaded = false;
            GroupBox[] list = { GB_Caught, GB_HighScore, GB_Resources };
            foreach (GroupBox gb in list)
            {
                foreach (Control ctrl in gb.Controls)
                {
                    if (ctrl is CheckBox)
                    {
                        (ctrl as CheckBox).Checked = false;
                        if (ctrl != CHK_CaughtMon)
                            (ctrl as CheckBox).Visible = false;
                    }
                    if (ctrl is PictureBox)
                        (ctrl as PictureBox).Image = new Bitmap(ctrl.Width, ctrl.Height);
                    if (ctrl is NumericUpDown)
                    {
                        (ctrl as NumericUpDown).Value = (ctrl as NumericUpDown).Minimum;
                        if (gb == GB_Caught)
                            (ctrl as NumericUpDown).Visible = false;
                    }
                    if (ctrl is Label)
                    {
                        if (gb == GB_Caught)
                            (ctrl as Label).Visible = false;
                        if (ctrl.Name.Contains("Rank"))
                            (ctrl as Label).Text = "-";
                    }
                }
            }
            FormInit();
            updating = false;
        }

        protected string GetDragFilename(DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy)
                return null;
            Array data = e.Data.GetData("FileName") as Array;
            if (data == null || (data.Length != 1) || !(data.GetValue(0) is String))
                return null;
            return ((string[])data)[0];

            //if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            //{
            //    Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
            //    if (data != null)
            //    {
            //        if ((data.Length == 1) && (data.GetValue(0) is String))
            //            return ((string[])data)[0];
            //    }
            //}
            //return null;
        }

        private void Main_DragDrop(object sender, DragEventArgs e)
        {
            Open(GetDragFilename(e));
        }

        private void Main_DragEnter(object sender, DragEventArgs e)
        {
            string filename = GetDragFilename(e);
            e.Effect = (filename != null && IsShuffleSave(filename)) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void ItemsGrid_EnabledChanged(object sender, EventArgs e)
        {
            ItemsGrid.SelectedObject = (ItemsGrid.Enabled) ? SI_Items : null;
        }

        private void ItemsGrid_Enter(object sender, EventArgs e)
        {
            if (lastkeys == Keys.Tab) { ItemsGrid.SelectedGridItem = ItemsGrid.EnumerateAllItems().First((item) => item.PropertyDescriptor != null); }
            else if (lastkeys == (Keys.Tab | Keys.Shift)) { ItemsGrid.SelectedGridItem = ItemsGrid.EnumerateAllItems().Last(); }
        }

        private void B_resources_Click(object sender, EventArgs e)
        {
            updating = true;
            db = new Database(true, (ModifierKeys == Keys.Control));
            if (loaded) { Parse(); }
            updating = false;
        }

        private void UpdateProperty(object s, PropertyValueChangedEventArgs e)
        {
            UpdateForm(s, e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            lastkeys = keyData;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
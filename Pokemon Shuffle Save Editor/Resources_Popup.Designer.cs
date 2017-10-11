namespace Pokemon_Shuffle_Save_Editor
{
    partial class Resources_Popup
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
            this.L_Intro = new System.Windows.Forms.Label();
            this.TLP_Files = new System.Windows.Forms.TableLayoutPanel();
            this.L_Files = new System.Windows.Forms.Label();
            this.L_Found = new System.Windows.Forms.Label();
            this.L_Date = new System.Windows.Forms.Label();
            this.L_Use = new System.Windows.Forms.Label();
            this.B_Cancel = new System.Windows.Forms.Button();
            this.B_OK = new System.Windows.Forms.Button();
            this.LL_Wiki = new System.Windows.Forms.LinkLabel();
            this.TLP_Files.SuspendLayout();
            this.SuspendLayout();
            // 
            // L_Intro
            // 
            this.L_Intro.AutoSize = true;
            this.L_Intro.Location = new System.Drawing.Point(12, 9);
            this.L_Intro.Name = "L_Intro";
            this.L_Intro.Size = new System.Drawing.Size(301, 26);
            this.L_Intro.TabIndex = 0;
            this.L_Intro.Text = "This is a sample text that exists solely for debugging purposes. \r\nIf you see thi" +
    "s, something went wrong.";
            // 
            // TLP_Files
            // 
            this.TLP_Files.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.TLP_Files.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.TLP_Files.ColumnCount = 4;
            this.TLP_Files.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.85714F));
            this.TLP_Files.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.TLP_Files.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.14286F));
            this.TLP_Files.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.TLP_Files.Controls.Add(this.L_Files, 0, 0);
            this.TLP_Files.Controls.Add(this.L_Found, 1, 0);
            this.TLP_Files.Controls.Add(this.L_Date, 2, 0);
            this.TLP_Files.Controls.Add(this.L_Use, 3, 0);
            this.TLP_Files.Location = new System.Drawing.Point(12, 47);
            this.TLP_Files.Name = "TLP_Files";
            this.TLP_Files.RowCount = 11;
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TLP_Files.Size = new System.Drawing.Size(490, 234);
            this.TLP_Files.TabIndex = 1;
            // 
            // L_Files
            // 
            this.L_Files.AutoSize = true;
            this.L_Files.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.L_Files.Location = new System.Drawing.Point(4, 1);
            this.L_Files.Name = "L_Files";
            this.L_Files.Size = new System.Drawing.Size(126, 13);
            this.L_Files.TabIndex = 0;
            this.L_Files.Text = "Supported game files";
            // 
            // L_Found
            // 
            this.L_Found.AutoSize = true;
            this.L_Found.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.L_Found.Location = new System.Drawing.Point(157, 1);
            this.L_Found.Name = "L_Found";
            this.L_Found.Size = new System.Drawing.Size(53, 13);
            this.L_Found.TabIndex = 1;
            this.L_Found.Text = "Found ?";
            // 
            // L_Date
            // 
            this.L_Date.AutoSize = true;
            this.L_Date.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.L_Date.Location = new System.Drawing.Point(223, 1);
            this.L_Date.Name = "L_Date";
            this.L_Date.Size = new System.Drawing.Size(82, 13);
            this.L_Date.TabIndex = 2;
            this.L_Date.Text = "Last modified";
            // 
            // L_Use
            // 
            this.L_Use.AutoSize = true;
            this.L_Use.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.L_Use.Location = new System.Drawing.Point(426, 1);
            this.L_Use.Name = "L_Use";
            this.L_Use.Size = new System.Drawing.Size(51, 13);
            this.L_Use.TabIndex = 3;
            this.L_Use.Text = "Use it ?";
            // 
            // B_Cancel
            // 
            this.B_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.B_Cancel.Location = new System.Drawing.Point(427, 296);
            this.B_Cancel.Name = "B_Cancel";
            this.B_Cancel.Size = new System.Drawing.Size(75, 23);
            this.B_Cancel.TabIndex = 2;
            this.B_Cancel.Text = "Cancel";
            this.B_Cancel.UseVisualStyleBackColor = true;
            // 
            // B_OK
            // 
            this.B_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.B_OK.Location = new System.Drawing.Point(346, 296);
            this.B_OK.Name = "B_OK";
            this.B_OK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.B_OK.Size = new System.Drawing.Size(75, 23);
            this.B_OK.TabIndex = 3;
            this.B_OK.Text = "OK";
            this.B_OK.UseVisualStyleBackColor = true;
            this.B_OK.Click += new System.EventHandler(this.B_OK_Click);
            // 
            // LL_Wiki
            // 
            this.LL_Wiki.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LL_Wiki.AutoSize = true;
            this.LL_Wiki.LinkArea = new System.Windows.Forms.LinkArea(6, 11);
            this.LL_Wiki.Location = new System.Drawing.Point(12, 305);
            this.LL_Wiki.Name = "LL_Wiki";
            this.LL_Wiki.Size = new System.Drawing.Size(212, 17);
            this.LL_Wiki.TabIndex = 4;
            this.LL_Wiki.TabStop = true;
            this.LL_Wiki.Text = "Check PSSE\'s wiki for more informations.";
            this.LL_Wiki.UseCompatibleTextRendering = true;
            this.LL_Wiki.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LL_Wiki_LinkClicked);
            // 
            // Resources_Popup
            // 
            this.AcceptButton = this.B_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.B_Cancel;
            this.ClientSize = new System.Drawing.Size(514, 331);
            this.Controls.Add(this.LL_Wiki);
            this.Controls.Add(this.B_OK);
            this.Controls.Add(this.B_Cancel);
            this.Controls.Add(this.TLP_Files);
            this.Controls.Add(this.L_Intro);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(530, 370);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(530, 120);
            this.Name = "Resources_Popup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game files scan";
            this.TLP_Files.ResumeLayout(false);
            this.TLP_Files.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label L_Intro;
        private System.Windows.Forms.TableLayoutPanel TLP_Files;
        private System.Windows.Forms.Label L_Files;
        private System.Windows.Forms.Label L_Found;
        private System.Windows.Forms.Label L_Date;
        private System.Windows.Forms.Label L_Use;
        private System.Windows.Forms.Button B_Cancel;
        private System.Windows.Forms.Button B_OK;
        private System.Windows.Forms.LinkLabel LL_Wiki;
    }
}
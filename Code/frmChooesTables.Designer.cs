namespace Code_Generator
{
    partial class frmChooesTables
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
            this.components = new System.ComponentModel.Container();
            this.guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(this.components);
            this.btnGenerate = new Guna.UI2.WinForms.Guna2Button();
            this.label1 = new System.Windows.Forms.Label();
            this.chkAllTables = new System.Windows.Forms.CheckedListBox();
            this.chkChooesAllTables = new System.Windows.Forms.CheckBox();
            this.gbSerchingFK = new System.Windows.Forms.GroupBox();
            this.rbJustThis = new System.Windows.Forms.RadioButton();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.gbAddingStaticMethods = new System.Windows.Forms.GroupBox();
            this.rbNo = new System.Windows.Forms.RadioButton();
            this.rbYes = new System.Windows.Forms.RadioButton();
            this.btnClose = new Guna.UI2.WinForms.Guna2CircleButton();
            this.guna2HtmlToolTip1 = new Guna.UI2.WinForms.Guna2HtmlToolTip();
            this.gbSerchingFK.SuspendLayout();
            this.gbAddingStaticMethods.SuspendLayout();
            this.SuspendLayout();
            // 
            // guna2BorderlessForm1
            // 
            this.guna2BorderlessForm1.BorderRadius = 50;
            this.guna2BorderlessForm1.ContainerControl = this;
            this.guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2BorderlessForm1.TransparentWhileDrag = true;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BorderRadius = 20;
            this.btnGenerate.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnGenerate.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnGenerate.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnGenerate.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnGenerate.FillColor = System.Drawing.Color.Teal;
            this.btnGenerate.Font = new System.Drawing.Font("Microsoft YaHei", 14F, System.Drawing.FontStyle.Bold);
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(306, 336);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(161, 45);
            this.btnGenerate.TabIndex = 8;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 25F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Teal;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(253, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(267, 45);
            this.label1.TabIndex = 7;
            this.label1.Text = "Chooes Tables";
            // 
            // chkAllTables
            // 
            this.chkAllTables.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold);
            this.chkAllTables.FormattingEnabled = true;
            this.chkAllTables.Location = new System.Drawing.Point(41, 106);
            this.chkAllTables.Name = "chkAllTables";
            this.chkAllTables.Size = new System.Drawing.Size(479, 220);
            this.chkAllTables.TabIndex = 9;
            this.chkAllTables.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chkAllTables_ItemCheck);
            // 
            // chkChooesAllTables
            // 
            this.chkChooesAllTables.AutoSize = true;
            this.chkChooesAllTables.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold);
            this.chkChooesAllTables.Location = new System.Drawing.Point(41, 65);
            this.chkChooesAllTables.Name = "chkChooesAllTables";
            this.chkChooesAllTables.Size = new System.Drawing.Size(173, 26);
            this.chkChooesAllTables.TabIndex = 10;
            this.chkChooesAllTables.Text = "Chooes All Tables";
            this.chkChooesAllTables.UseVisualStyleBackColor = true;
            this.chkChooesAllTables.CheckedChanged += new System.EventHandler(this.chkChooesAllTables_CheckedChanged);
            // 
            // gbSerchingFK
            // 
            this.gbSerchingFK.Controls.Add(this.rbJustThis);
            this.gbSerchingFK.Controls.Add(this.rbAll);
            this.gbSerchingFK.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold);
            this.gbSerchingFK.Location = new System.Drawing.Point(538, 65);
            this.gbSerchingFK.Name = "gbSerchingFK";
            this.gbSerchingFK.Size = new System.Drawing.Size(224, 97);
            this.gbSerchingFK.TabIndex = 11;
            this.gbSerchingFK.TabStop = false;
            this.gbSerchingFK.Text = "Searching FK OF:";
            // 
            // rbJustThis
            // 
            this.rbJustThis.AutoSize = true;
            this.rbJustThis.Location = new System.Drawing.Point(105, 48);
            this.rbJustThis.Name = "rbJustThis";
            this.rbJustThis.Size = new System.Drawing.Size(99, 26);
            this.rbJustThis.TabIndex = 1;
            this.rbJustThis.TabStop = true;
            this.rbJustThis.Text = "Just This";
            this.rbJustThis.UseVisualStyleBackColor = true;
            // 
            // rbAll
            // 
            this.rbAll.AutoSize = true;
            this.rbAll.Location = new System.Drawing.Point(17, 48);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(50, 26);
            this.rbAll.TabIndex = 0;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "All";
            this.rbAll.UseVisualStyleBackColor = true;
            this.rbAll.CheckedChanged += new System.EventHandler(this.rbAll_CheckedChanged);
            // 
            // gbAddingStaticMethods
            // 
            this.gbAddingStaticMethods.Controls.Add(this.rbNo);
            this.gbAddingStaticMethods.Controls.Add(this.rbYes);
            this.gbAddingStaticMethods.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Bold);
            this.gbAddingStaticMethods.Location = new System.Drawing.Point(538, 204);
            this.gbAddingStaticMethods.Name = "gbAddingStaticMethods";
            this.gbAddingStaticMethods.Size = new System.Drawing.Size(224, 97);
            this.gbAddingStaticMethods.TabIndex = 12;
            this.gbAddingStaticMethods.TabStop = false;
            this.gbAddingStaticMethods.Text = "Adding Static Methods:";
            // 
            // rbNo
            // 
            this.rbNo.AutoSize = true;
            this.rbNo.Location = new System.Drawing.Point(124, 43);
            this.rbNo.Name = "rbNo";
            this.rbNo.Size = new System.Drawing.Size(53, 26);
            this.rbNo.TabIndex = 1;
            this.rbNo.TabStop = true;
            this.rbNo.Text = "No";
            this.rbNo.UseVisualStyleBackColor = true;
            // 
            // rbYes
            // 
            this.rbYes.AutoSize = true;
            this.rbYes.Location = new System.Drawing.Point(17, 43);
            this.rbYes.Name = "rbYes";
            this.rbYes.Size = new System.Drawing.Size(55, 26);
            this.rbYes.TabIndex = 0;
            this.rbYes.TabStop = true;
            this.rbYes.Text = "Yes";
            this.rbYes.UseVisualStyleBackColor = true;
            this.rbYes.CheckedChanged += new System.EventHandler(this.rbYes_CheckedChanged);
            // 
            // btnClose
            // 
            this.btnClose.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnClose.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnClose.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnClose.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnClose.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(255)))));
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Image = global::Code_Generator.Properties.Resources.cross;
            this.btnClose.Location = new System.Drawing.Point(731, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.btnClose.Size = new System.Drawing.Size(42, 42);
            this.btnClose.TabIndex = 13;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // guna2HtmlToolTip1
            // 
            this.guna2HtmlToolTip1.AllowLinksHandling = true;
            this.guna2HtmlToolTip1.MaximumSize = new System.Drawing.Size(0, 0);
            // 
            // frmChooesTables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(785, 410);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.gbAddingStaticMethods);
            this.Controls.Add(this.gbSerchingFK);
            this.Controls.Add(this.chkChooesAllTables);
            this.Controls.Add(this.chkAllTables);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmChooesTables";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmChooesTables";
            this.Load += new System.EventHandler(this.frmChooesTables_Load);
            this.gbSerchingFK.ResumeLayout(false);
            this.gbSerchingFK.PerformLayout();
            this.gbAddingStaticMethods.ResumeLayout(false);
            this.gbAddingStaticMethods.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        private Guna.UI2.WinForms.Guna2Button btnGenerate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chkAllTables;
        private System.Windows.Forms.CheckBox chkChooesAllTables;
        private System.Windows.Forms.GroupBox gbSerchingFK;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.RadioButton rbJustThis;
        private System.Windows.Forms.GroupBox gbAddingStaticMethods;
        private System.Windows.Forms.RadioButton rbNo;
        private System.Windows.Forms.RadioButton rbYes;
        private Guna.UI2.WinForms.Guna2CircleButton btnClose;
        private Guna.UI2.WinForms.Guna2HtmlToolTip guna2HtmlToolTip1;
    }
}
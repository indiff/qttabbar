namespace QTTabBarLib.FileRename
{
    partial class FileRenameDialog
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
            this.pnlMode = new System.Windows.Forms.Panel();
            this.lblMode = new System.Windows.Forms.Label();
            this.cboMode = new System.Windows.Forms.ComboBox();
            this.btnPreset = new System.Windows.Forms.Button();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.lblFind = new System.Windows.Forms.Label();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.lblReplace = new System.Windows.Forms.Label();
            this.txtReplace = new System.Windows.Forms.TextBox();
            this.chkRegex = new System.Windows.Forms.CheckBox();
            this.chkCase = new System.Windows.Forms.CheckBox();
            this.chkPreserveExt = new System.Windows.Forms.CheckBox();
            this.lblCase = new System.Windows.Forms.Label();
            this.cboCase = new System.Windows.Forms.ComboBox();
            this.lblSeq = new System.Windows.Forms.Label();
            this.numStart = new System.Windows.Forms.NumericUpDown();
            this.lblStart = new System.Windows.Forms.Label();
            this.numInc = new System.Windows.Forms.NumericUpDown();
            this.lblInc = new System.Windows.Forms.Label();
            this.numDigits = new System.Windows.Forms.NumericUpDown();
            this.lblDigits = new System.Windows.Forms.Label();
            this.chkSubfolders = new System.Windows.Forms.CheckBox();
            this.lblDepth = new System.Windows.Forms.Label();
            this.numDepth = new System.Windows.Forms.NumericUpDown();
            this.lblDepthHint = new System.Windows.Forms.Label();
            this.lblCollision = new System.Windows.Forms.Label();
            this.cboCollision = new System.Windows.Forms.ComboBox();
            this.lblScope = new System.Windows.Forms.Label();
            this.chkFiles = new System.Windows.Forms.CheckBox();
            this.chkFolders = new System.Windows.Forms.CheckBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lvPreview = new System.Windows.Forms.ListView();
            this.colOriginal = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNew = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlMode.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDigits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDepth)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMode
            // 
            this.pnlMode.Controls.Add(this.lblMode);
            this.pnlMode.Controls.Add(this.cboMode);
            this.pnlMode.Controls.Add(this.btnPreset);
            this.pnlMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMode.Location = new System.Drawing.Point(0, 0);
            this.pnlMode.Name = "pnlMode";
            this.pnlMode.Padding = new System.Windows.Forms.Padding(20, 16, 20, 12);
            this.pnlMode.Size = new System.Drawing.Size(960, 64);
            this.pnlMode.TabIndex = 0;
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new System.Drawing.Point(20, 20);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(99, 23);
            this.lblMode.TabIndex = 0;
            this.lblMode.Text = "重命名模式:";
            // 
            // cboMode
            // 
            this.cboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMode.FormattingEnabled = true;
            this.cboMode.Items.AddRange(new object[] {
            "查找替换",
            "顺序编号",
            "列表重命名"});
            this.cboMode.Location = new System.Drawing.Point(116, 16);
            this.cboMode.Name = "cboMode";
            this.cboMode.Size = new System.Drawing.Size(306, 31);
            this.cboMode.TabIndex = 1;
            // 
            // btnPreset
            // 
            this.btnPreset.Location = new System.Drawing.Point(435, 12);
            this.btnPreset.Name = "btnPreset";
            this.btnPreset.Size = new System.Drawing.Size(100, 30);
            this.btnPreset.TabIndex = 2;
            this.btnPreset.Text = "预设方案";
            this.btnPreset.UseVisualStyleBackColor = true;
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.lblFind);
            this.pnlOptions.Controls.Add(this.txtFind);
            this.pnlOptions.Controls.Add(this.lblReplace);
            this.pnlOptions.Controls.Add(this.txtReplace);
            this.pnlOptions.Controls.Add(this.chkRegex);
            this.pnlOptions.Controls.Add(this.chkCase);
            this.pnlOptions.Controls.Add(this.chkPreserveExt);
            this.pnlOptions.Controls.Add(this.lblCase);
            this.pnlOptions.Controls.Add(this.cboCase);
            this.pnlOptions.Controls.Add(this.lblSeq);
            this.pnlOptions.Controls.Add(this.numStart);
            this.pnlOptions.Controls.Add(this.lblStart);
            this.pnlOptions.Controls.Add(this.numInc);
            this.pnlOptions.Controls.Add(this.lblInc);
            this.pnlOptions.Controls.Add(this.numDigits);
            this.pnlOptions.Controls.Add(this.lblDigits);
            this.pnlOptions.Controls.Add(this.chkSubfolders);
            this.pnlOptions.Controls.Add(this.lblDepth);
            this.pnlOptions.Controls.Add(this.numDepth);
            this.pnlOptions.Controls.Add(this.lblDepthHint);
            this.pnlOptions.Controls.Add(this.lblCollision);
            this.pnlOptions.Controls.Add(this.cboCollision);
            this.pnlOptions.Controls.Add(this.lblScope);
            this.pnlOptions.Controls.Add(this.chkFiles);
            this.pnlOptions.Controls.Add(this.chkFolders);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOptions.Location = new System.Drawing.Point(0, 64);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Padding = new System.Windows.Forms.Padding(20, 8, 20, 8);
            this.pnlOptions.Size = new System.Drawing.Size(960, 226);
            this.pnlOptions.TabIndex = 1;
            // 
            // lblFind
            // 
            this.lblFind.Location = new System.Drawing.Point(20, 17);
            this.lblFind.Name = "lblFind";
            this.lblFind.Size = new System.Drawing.Size(96, 24);
            this.lblFind.TabIndex = 0;
            this.lblFind.Text = "查找内容:";
            this.lblFind.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(122, 12);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(300, 29);
            this.txtFind.TabIndex = 1;
            // 
            // lblReplace
            // 
            this.lblReplace.Location = new System.Drawing.Point(539, 17);
            this.lblReplace.Name = "lblReplace";
            this.lblReplace.Size = new System.Drawing.Size(70, 24);
            this.lblReplace.TabIndex = 2;
            this.lblReplace.Text = "替换为:";
            this.lblReplace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtReplace
            // 
            this.txtReplace.Location = new System.Drawing.Point(615, 12);
            this.txtReplace.Name = "txtReplace";
            this.txtReplace.Size = new System.Drawing.Size(300, 29);
            this.txtReplace.TabIndex = 3;
            // 
            // chkRegex
            // 
            this.chkRegex.AutoSize = true;
            this.chkRegex.Location = new System.Drawing.Point(122, 60);
            this.chkRegex.Name = "chkRegex";
            this.chkRegex.Size = new System.Drawing.Size(151, 27);
            this.chkRegex.TabIndex = 4;
            this.chkRegex.Text = "使用正则表达式";
            this.chkRegex.UseVisualStyleBackColor = true;
            // 
            // chkCase
            // 
            this.chkCase.AutoSize = true;
            this.chkCase.Location = new System.Drawing.Point(282, 60);
            this.chkCase.Name = "chkCase";
            this.chkCase.Size = new System.Drawing.Size(117, 27);
            this.chkCase.TabIndex = 5;
            this.chkCase.Text = "区分大小写";
            this.chkCase.UseVisualStyleBackColor = true;
            // 
            // chkPreserveExt
            // 
            this.chkPreserveExt.AutoSize = true;
            this.chkPreserveExt.Checked = true;
            this.chkPreserveExt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPreserveExt.Location = new System.Drawing.Point(402, 60);
            this.chkPreserveExt.Name = "chkPreserveExt";
            this.chkPreserveExt.Size = new System.Drawing.Size(151, 27);
            this.chkPreserveExt.TabIndex = 6;
            this.chkPreserveExt.Text = "保留文件扩展名";
            this.chkPreserveExt.UseVisualStyleBackColor = true;
            // 
            // lblCase
            // 
            this.lblCase.Location = new System.Drawing.Point(20, 104);
            this.lblCase.Name = "lblCase";
            this.lblCase.Size = new System.Drawing.Size(96, 26);
            this.lblCase.TabIndex = 7;
            this.lblCase.Text = "大小写转换:";
            this.lblCase.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCase
            // 
            this.cboCase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCase.FormattingEnabled = true;
            this.cboCase.Items.AddRange(new object[] {
            "不转换",
            "全部小写",
            "全部大写",
            "首字母大写"});
            this.cboCase.Location = new System.Drawing.Point(122, 99);
            this.cboCase.Name = "cboCase";
            this.cboCase.Size = new System.Drawing.Size(300, 31);
            this.cboCase.TabIndex = 8;
            // 
            // lblSeq
            // 
            this.lblSeq.Location = new System.Drawing.Point(530, 104);
            this.lblSeq.Name = "lblSeq";
            this.lblSeq.Size = new System.Drawing.Size(80, 26);
            this.lblSeq.TabIndex = 9;
            this.lblSeq.Text = "顺序编号:";
            this.lblSeq.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numStart
            // 
            this.numStart.Location = new System.Drawing.Point(616, 99);
            this.numStart.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numStart.Name = "numStart";
            this.numStart.Size = new System.Drawing.Size(60, 29);
            this.numStart.TabIndex = 10;
            this.numStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.ForeColor = System.Drawing.Color.Gray;
            this.lblStart.Location = new System.Drawing.Point(680, 103);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(44, 23);
            this.lblStart.TabIndex = 11;
            this.lblStart.Text = "起始";
            // 
            // numInc
            // 
            this.numInc.Location = new System.Drawing.Point(730, 99);
            this.numInc.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numInc.Name = "numInc";
            this.numInc.Size = new System.Drawing.Size(50, 29);
            this.numInc.TabIndex = 12;
            this.numInc.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblInc
            // 
            this.lblInc.AutoSize = true;
            this.lblInc.ForeColor = System.Drawing.Color.Gray;
            this.lblInc.Location = new System.Drawing.Point(786, 103);
            this.lblInc.Name = "lblInc";
            this.lblInc.Size = new System.Drawing.Size(44, 23);
            this.lblInc.TabIndex = 13;
            this.lblInc.Text = "步长";
            // 
            // numDigits
            // 
            this.numDigits.Location = new System.Drawing.Point(830, 99);
            this.numDigits.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDigits.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDigits.Name = "numDigits";
            this.numDigits.Size = new System.Drawing.Size(50, 29);
            this.numDigits.TabIndex = 14;
            this.numDigits.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // lblDigits
            // 
            this.lblDigits.AutoSize = true;
            this.lblDigits.ForeColor = System.Drawing.Color.Gray;
            this.lblDigits.Location = new System.Drawing.Point(886, 103);
            this.lblDigits.Name = "lblDigits";
            this.lblDigits.Size = new System.Drawing.Size(44, 23);
            this.lblDigits.TabIndex = 15;
            this.lblDigits.Text = "位数";
            // 
            // chkSubfolders
            // 
            this.chkSubfolders.AutoSize = true;
            this.chkSubfolders.Location = new System.Drawing.Point(122, 147);
            this.chkSubfolders.Name = "chkSubfolders";
            this.chkSubfolders.Size = new System.Drawing.Size(134, 27);
            this.chkSubfolders.TabIndex = 16;
            this.chkSubfolders.Text = "包含子文件夹";
            this.chkSubfolders.UseVisualStyleBackColor = true;
            // 
            // lblDepth
            // 
            this.lblDepth.AutoSize = true;
            this.lblDepth.Location = new System.Drawing.Point(530, 148);
            this.lblDepth.Name = "lblDepth";
            this.lblDepth.Size = new System.Drawing.Size(82, 23);
            this.lblDepth.TabIndex = 17;
            this.lblDepth.Text = "扫描深度:";
            // 
            // numDepth
            // 
            this.numDepth.Location = new System.Drawing.Point(618, 142);
            this.numDepth.Name = "numDepth";
            this.numDepth.Size = new System.Drawing.Size(60, 29);
            this.numDepth.TabIndex = 18;
            this.numDepth.ValueChanged += new System.EventHandler(this.numDepth_ValueChanged);
            // 
            // lblDepthHint
            // 
            this.lblDepthHint.AutoSize = true;
            this.lblDepthHint.ForeColor = System.Drawing.Color.Gray;
            this.lblDepthHint.Location = new System.Drawing.Point(702, 144);
            this.lblDepthHint.Name = "lblDepthHint";
            this.lblDepthHint.Size = new System.Drawing.Size(106, 23);
            this.lblDepthHint.TabIndex = 19;
            this.lblDepthHint.Text = "(0 = 无限制)";
            this.lblDepthHint.Click += new System.EventHandler(this.lblDepthHint_Click);
            // 
            // lblCollision
            // 
            this.lblCollision.Location = new System.Drawing.Point(20, 186);
            this.lblCollision.Name = "lblCollision";
            this.lblCollision.Size = new System.Drawing.Size(96, 26);
            this.lblCollision.TabIndex = 20;
            this.lblCollision.Text = "命名冲突:";
            this.lblCollision.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboCollision
            // 
            this.cboCollision.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCollision.FormattingEnabled = true;
            this.cboCollision.Items.AddRange(new object[] {
            "弹出确认",
            "自动追加序号",
            "保留较新文件"});
            this.cboCollision.Location = new System.Drawing.Point(122, 186);
            this.cboCollision.Name = "cboCollision";
            this.cboCollision.Size = new System.Drawing.Size(300, 31);
            this.cboCollision.TabIndex = 21;
            // 
            // lblScope
            // 
            this.lblScope.Location = new System.Drawing.Point(475, 187);
            this.lblScope.Name = "lblScope";
            this.lblScope.Size = new System.Drawing.Size(134, 27);
            this.lblScope.TabIndex = 22;
            this.lblScope.Text = "重命名范围:";
            this.lblScope.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkFiles
            // 
            this.chkFiles.AutoSize = true;
            this.chkFiles.Checked = true;
            this.chkFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFiles.Location = new System.Drawing.Point(615, 186);
            this.chkFiles.Name = "chkFiles";
            this.chkFiles.Size = new System.Drawing.Size(66, 27);
            this.chkFiles.TabIndex = 23;
            this.chkFiles.Text = "文件";
            this.chkFiles.UseVisualStyleBackColor = true;
            // 
            // chkFolders
            // 
            this.chkFolders.AutoSize = true;
            this.chkFolders.Checked = true;
            this.chkFolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFolders.Location = new System.Drawing.Point(685, 186);
            this.chkFolders.Name = "chkFolders";
            this.chkFolders.Size = new System.Drawing.Size(83, 27);
            this.chkFolders.TabIndex = 24;
            this.chkFolders.Text = "文件夹";
            this.chkFolders.UseVisualStyleBackColor = true;
            // 
            // lblPreview
            // 
            this.lblPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.lblPreview.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPreview.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPreview.ForeColor = System.Drawing.Color.DimGray;
            this.lblPreview.Location = new System.Drawing.Point(0, 290);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(960, 32);
            this.lblPreview.TabIndex = 2;
            this.lblPreview.Text = "  预览列表 （原名  →  新名称）";
            this.lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvPreview
            // 
            this.lvPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvPreview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colOriginal,
            this.colNew,
            this.colType,
            this.colStatus});
            this.lvPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPreview.FullRowSelect = true;
            this.lvPreview.GridLines = true;
            this.lvPreview.HideSelection = false;
            this.lvPreview.Location = new System.Drawing.Point(0, 322);
            this.lvPreview.Name = "lvPreview";
            this.lvPreview.Size = new System.Drawing.Size(960, 354);
            this.lvPreview.TabIndex = 3;
            this.lvPreview.UseCompatibleStateImageBehavior = false;
            this.lvPreview.View = System.Windows.Forms.View.Details;
            // 
            // colOriginal
            // 
            this.colOriginal.Text = "原名称";
            this.colOriginal.Width = 350;
            // 
            // colNew
            // 
            this.colNew.Text = "新名称";
            this.colNew.Width = 350;
            // 
            // colType
            // 
            this.colType.Text = "类型";
            this.colType.Width = 80;
            // 
            // colStatus
            // 
            this.colStatus.Text = "状态";
            this.colStatus.Width = 110;
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.pnlBottom.Controls.Add(this.lblStatus);
            this.pnlBottom.Controls.Add(this.btnExecute);
            this.pnlBottom.Controls.Add(this.btnClose);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 676);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            this.pnlBottom.Size = new System.Drawing.Size(960, 64);
            this.pnlBottom.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
            this.lblStatus.Location = new System.Drawing.Point(20, 22);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(44, 23);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "就绪";
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(700, 14);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(120, 36);
            this.btnExecute.TabIndex = 1;
            this.btnExecute.Text = "执行重命名";
            this.btnExecute.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(830, 14);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 36);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // FileRenameDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 740);
            this.Controls.Add(this.lvPreview);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.lblPreview);
            this.Controls.Add(this.pnlOptions);
            this.Controls.Add(this.pnlMode);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileRenameDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "批量重命名";
            this.pnlMode.ResumeLayout(false);
            this.pnlMode.PerformLayout();
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDigits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDepth)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMode;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.ComboBox cboMode;
        private System.Windows.Forms.Button btnPreset;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.Label lblFind;
        private System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.Label lblReplace;
        private System.Windows.Forms.TextBox txtReplace;
        private System.Windows.Forms.CheckBox chkRegex;
        private System.Windows.Forms.CheckBox chkCase;
        private System.Windows.Forms.CheckBox chkPreserveExt;
        private System.Windows.Forms.Label lblCase;
        private System.Windows.Forms.ComboBox cboCase;
        private System.Windows.Forms.Label lblSeq;
        private System.Windows.Forms.NumericUpDown numStart;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.NumericUpDown numInc;
        private System.Windows.Forms.Label lblInc;
        private System.Windows.Forms.NumericUpDown numDigits;
        private System.Windows.Forms.Label lblDigits;
        private System.Windows.Forms.CheckBox chkSubfolders;
        private System.Windows.Forms.Label lblDepth;
        private System.Windows.Forms.NumericUpDown numDepth;
        private System.Windows.Forms.Label lblDepthHint;
        private System.Windows.Forms.Label lblCollision;
        private System.Windows.Forms.ComboBox cboCollision;
        private System.Windows.Forms.Label lblScope;
        private System.Windows.Forms.CheckBox chkFiles;
        private System.Windows.Forms.CheckBox chkFolders;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.ListView lvPreview;
        private System.Windows.Forms.ColumnHeader colOriginal;
        private System.Windows.Forms.ColumnHeader colNew;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnClose;
    }
}

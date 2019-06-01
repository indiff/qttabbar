//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

﻿namespace QuizoPlugins {
    partial class SettingWindow {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this.nudInitialW = new System.Windows.Forms.NumericUpDown();
            this.nudInitialH = new System.Windows.Forms.NumericUpDown();
            this.labelX1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.chbInitialSize = new System.Windows.Forms.CheckBox();
            this.checkBoxResizeMode = new System.Windows.Forms.CheckBox();
            this.nudDelta = new System.Windows.Forms.NumericUpDown();
            this.labelDELTARESIZE = new System.Windows.Forms.Label();
            this.buttonRestoreSize = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.nudPresets_X = new System.Windows.Forms.NumericUpDown();
            this.nudPresets_Y = new System.Windows.Forms.NumericUpDown();
            this.nudPresets_W = new System.Windows.Forms.NumericUpDown();
            this.nudPresets_H = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonSet = new System.Windows.Forms.Button();
            this.buttonDel = new System.Windows.Forms.Button();
            this.chbInitialLoc = new System.Windows.Forms.CheckBox();
            this.nudInitialX = new System.Windows.Forms.NumericUpDown();
            this.nudInitialY = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonRestoreLoc = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonGetCurLoc = new System.Windows.Forms.Button();
            this.buttonGetCurSize = new System.Windows.Forms.Button();
            this.groupBoxPresets = new System.Windows.Forms.GroupBox();
            this.buttonGetCurrentToPreset = new System.Windows.Forms.Button();
            this.chbStartingPreset = new System.Windows.Forms.CheckBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.chbRestoreClosedRct = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_X)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_Y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_W)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_H)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialY)).BeginInit();
            this.groupBoxPresets.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudInitialW
            // 
            this.nudInitialW.Location = new System.Drawing.Point(175, 52);
            this.nudInitialW.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudInitialW.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudInitialW.Name = "nudInitialW";
            this.nudInitialW.Size = new System.Drawing.Size(73, 21);
            this.nudInitialW.TabIndex = 1;
            this.nudInitialW.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudInitialW.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // nudInitialH
            // 
            this.nudInitialH.Location = new System.Drawing.Point(282, 52);
            this.nudInitialH.Minimum = new decimal(new int[] {
            96,
            0,
            0,
            0});
            this.nudInitialH.Name = "nudInitialH";
            this.nudInitialH.Size = new System.Drawing.Size(73, 21);
            this.nudInitialH.TabIndex = 2;
            this.nudInitialH.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudInitialH.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
            // 
            // labelX1
            // 
            this.labelX1.AutoSize = true;
            this.labelX1.Location = new System.Drawing.Point(258, 54);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(14, 13);
            this.labelX1.TabIndex = 3;
            this.labelX1.Text = "H";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(464, 239);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 13;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(545, 239);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 14;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // chbInitialSize
            // 
            this.chbInitialSize.AutoSize = true;
            this.chbInitialSize.Location = new System.Drawing.Point(12, 53);
            this.chbInitialSize.Name = "chbInitialSize";
            this.chbInitialSize.Size = new System.Drawing.Size(119, 17);
            this.chbInitialSize.TabIndex = 1;
            this.chbInitialSize.Text = "Resize on starting :";
            this.chbInitialSize.UseVisualStyleBackColor = true;
            this.chbInitialSize.CheckedChanged += new System.EventHandler(this.chbInitialSize_CheckedChanged);
            // 
            // checkBoxResizeMode
            // 
            this.checkBoxResizeMode.AutoSize = true;
            this.checkBoxResizeMode.Checked = true;
            this.checkBoxResizeMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxResizeMode.Location = new System.Drawing.Point(12, 177);
            this.checkBoxResizeMode.Name = "checkBoxResizeMode";
            this.checkBoxResizeMode.Size = new System.Drawing.Size(183, 17);
            this.checkBoxResizeMode.TabIndex = 5;
            this.checkBoxResizeMode.Text = "Reposition window when resizing";
            this.checkBoxResizeMode.UseVisualStyleBackColor = true;
            // 
            // nudDelta
            // 
            this.nudDelta.Location = new System.Drawing.Point(175, 219);
            this.nudDelta.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.nudDelta.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDelta.Name = "nudDelta";
            this.nudDelta.Size = new System.Drawing.Size(65, 21);
            this.nudDelta.TabIndex = 4;
            this.nudDelta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudDelta.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // labelDELTARESIZE
            // 
            this.labelDELTARESIZE.AutoSize = true;
            this.labelDELTARESIZE.Location = new System.Drawing.Point(12, 221);
            this.labelDELTARESIZE.Name = "labelDELTARESIZE";
            this.labelDELTARESIZE.Size = new System.Drawing.Size(142, 13);
            this.labelDELTARESIZE.TabIndex = 9;
            this.labelDELTARESIZE.Text = "Resize / move delta in pixels";
            // 
            // buttonRestoreSize
            // 
            this.buttonRestoreSize.Location = new System.Drawing.Point(380, 49);
            this.buttonRestoreSize.Name = "buttonRestoreSize";
            this.buttonRestoreSize.Size = new System.Drawing.Size(108, 23);
            this.buttonRestoreSize.TabIndex = 3;
            this.buttonRestoreSize.Text = "Restore size";
            this.buttonRestoreSize.UseVisualStyleBackColor = true;
            this.buttonRestoreSize.Click += new System.EventHandler(this.buttonRestoreSize_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(17, 33);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(190, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // nudPresets_X
            // 
            this.nudPresets_X.Location = new System.Drawing.Point(34, 69);
            this.nudPresets_X.Maximum = new decimal(new int[] {
            25600,
            0,
            0,
            0});
            this.nudPresets_X.Minimum = new decimal(new int[] {
            25600,
            0,
            0,
            -2147483648});
            this.nudPresets_X.Name = "nudPresets_X";
            this.nudPresets_X.Size = new System.Drawing.Size(69, 21);
            this.nudPresets_X.TabIndex = 9;
            this.nudPresets_X.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // nudPresets_Y
            // 
            this.nudPresets_Y.Location = new System.Drawing.Point(128, 69);
            this.nudPresets_Y.Maximum = new decimal(new int[] {
            25600,
            0,
            0,
            0});
            this.nudPresets_Y.Minimum = new decimal(new int[] {
            25600,
            0,
            0,
            -2147483648});
            this.nudPresets_Y.Name = "nudPresets_Y";
            this.nudPresets_Y.Size = new System.Drawing.Size(69, 21);
            this.nudPresets_Y.TabIndex = 10;
            this.nudPresets_Y.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // nudPresets_W
            // 
            this.nudPresets_W.Location = new System.Drawing.Point(34, 98);
            this.nudPresets_W.Maximum = new decimal(new int[] {
            2560,
            0,
            0,
            0});
            this.nudPresets_W.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudPresets_W.Name = "nudPresets_W";
            this.nudPresets_W.Size = new System.Drawing.Size(69, 21);
            this.nudPresets_W.TabIndex = 11;
            this.nudPresets_W.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudPresets_W.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
            // 
            // nudPresets_H
            // 
            this.nudPresets_H.Location = new System.Drawing.Point(128, 98);
            this.nudPresets_H.Maximum = new decimal(new int[] {
            2560,
            0,
            0,
            0});
            this.nudPresets_H.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudPresets_H.Name = "nudPresets_H";
            this.nudPresets_H.Size = new System.Drawing.Size(69, 21);
            this.nudPresets_H.TabIndex = 12;
            this.nudPresets_H.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudPresets_H.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "X";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(109, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "W";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(109, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "H";
            // 
            // buttonSet
            // 
            this.buttonSet.Location = new System.Drawing.Point(220, 20);
            this.buttonSet.Name = "buttonSet";
            this.buttonSet.Size = new System.Drawing.Size(75, 25);
            this.buttonSet.TabIndex = 7;
            this.buttonSet.Text = "Set";
            this.buttonSet.UseVisualStyleBackColor = true;
            this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
            // 
            // buttonDel
            // 
            this.buttonDel.Location = new System.Drawing.Point(220, 49);
            this.buttonDel.Name = "buttonDel";
            this.buttonDel.Size = new System.Drawing.Size(75, 25);
            this.buttonDel.TabIndex = 8;
            this.buttonDel.Text = "Delete";
            this.buttonDel.UseVisualStyleBackColor = true;
            this.buttonDel.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // chbInitialLoc
            // 
            this.chbInitialLoc.AutoSize = true;
            this.chbInitialLoc.Location = new System.Drawing.Point(12, 12);
            this.chbInitialLoc.Name = "chbInitialLoc";
            this.chbInitialLoc.Size = new System.Drawing.Size(104, 17);
            this.chbInitialLoc.TabIndex = 20;
            this.chbInitialLoc.Text = "Starting location";
            this.chbInitialLoc.UseVisualStyleBackColor = true;
            this.chbInitialLoc.CheckedChanged += new System.EventHandler(this.chbInitialLoc_CheckedChanged);
            // 
            // nudInitialX
            // 
            this.nudInitialX.Location = new System.Drawing.Point(175, 11);
            this.nudInitialX.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudInitialX.Minimum = new decimal(new int[] {
            65535,
            0,
            0,
            -2147483648});
            this.nudInitialX.Name = "nudInitialX";
            this.nudInitialX.Size = new System.Drawing.Size(73, 21);
            this.nudInitialX.TabIndex = 21;
            this.nudInitialX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // nudInitialY
            // 
            this.nudInitialY.Location = new System.Drawing.Point(282, 11);
            this.nudInitialY.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudInitialY.Minimum = new decimal(new int[] {
            65535,
            0,
            0,
            -2147483648});
            this.nudInitialY.Name = "nudInitialY";
            this.nudInitialY.Size = new System.Drawing.Size(73, 21);
            this.nudInitialY.TabIndex = 22;
            this.nudInitialY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(152, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "X";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(258, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Y";
            // 
            // buttonRestoreLoc
            // 
            this.buttonRestoreLoc.Location = new System.Drawing.Point(380, 8);
            this.buttonRestoreLoc.Name = "buttonRestoreLoc";
            this.buttonRestoreLoc.Size = new System.Drawing.Size(108, 23);
            this.buttonRestoreLoc.TabIndex = 25;
            this.buttonRestoreLoc.Text = "Restore location";
            this.buttonRestoreLoc.UseVisualStyleBackColor = true;
            this.buttonRestoreLoc.Click += new System.EventHandler(this.buttonRestoreLoc_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(152, 54);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "W";
            // 
            // buttonGetCurLoc
            // 
            this.buttonGetCurLoc.Location = new System.Drawing.Point(494, 8);
            this.buttonGetCurLoc.Name = "buttonGetCurLoc";
            this.buttonGetCurLoc.Size = new System.Drawing.Size(126, 23);
            this.buttonGetCurLoc.TabIndex = 27;
            this.buttonGetCurLoc.Text = "Get current location";
            this.buttonGetCurLoc.UseVisualStyleBackColor = true;
            this.buttonGetCurLoc.Click += new System.EventHandler(this.buttonGetCurLoc_Click);
            // 
            // buttonGetCurSize
            // 
            this.buttonGetCurSize.Location = new System.Drawing.Point(494, 49);
            this.buttonGetCurSize.Name = "buttonGetCurSize";
            this.buttonGetCurSize.Size = new System.Drawing.Size(126, 23);
            this.buttonGetCurSize.TabIndex = 28;
            this.buttonGetCurSize.Text = "Get current size";
            this.buttonGetCurSize.UseVisualStyleBackColor = true;
            this.buttonGetCurSize.Click += new System.EventHandler(this.buttonGetCurSize_Click);
            // 
            // groupBoxPresets
            // 
            this.groupBoxPresets.Controls.Add(this.buttonGetCurrentToPreset);
            this.groupBoxPresets.Controls.Add(this.comboBox1);
            this.groupBoxPresets.Controls.Add(this.buttonSet);
            this.groupBoxPresets.Controls.Add(this.buttonDel);
            this.groupBoxPresets.Controls.Add(this.label2);
            this.groupBoxPresets.Controls.Add(this.nudPresets_X);
            this.groupBoxPresets.Controls.Add(this.nudPresets_Y);
            this.groupBoxPresets.Controls.Add(this.nudPresets_W);
            this.groupBoxPresets.Controls.Add(this.nudPresets_H);
            this.groupBoxPresets.Controls.Add(this.label3);
            this.groupBoxPresets.Controls.Add(this.label5);
            this.groupBoxPresets.Controls.Add(this.label4);
            this.groupBoxPresets.Location = new System.Drawing.Point(296, 91);
            this.groupBoxPresets.Name = "groupBoxPresets";
            this.groupBoxPresets.Size = new System.Drawing.Size(324, 136);
            this.groupBoxPresets.TabIndex = 29;
            this.groupBoxPresets.TabStop = false;
            this.groupBoxPresets.Text = "Presets";
            // 
            // buttonGetCurrentToPreset
            // 
            this.buttonGetCurrentToPreset.Location = new System.Drawing.Point(220, 80);
            this.buttonGetCurrentToPreset.Name = "buttonGetCurrentToPreset";
            this.buttonGetCurrentToPreset.Size = new System.Drawing.Size(75, 23);
            this.buttonGetCurrentToPreset.TabIndex = 20;
            this.buttonGetCurrentToPreset.Text = "Get current";
            this.buttonGetCurrentToPreset.UseVisualStyleBackColor = true;
            this.buttonGetCurrentToPreset.Click += new System.EventHandler(this.buttonGetCurrentToPreset_Click);
            // 
            // chbStartingPreset
            // 
            this.chbStartingPreset.AutoSize = true;
            this.chbStartingPreset.Location = new System.Drawing.Point(12, 94);
            this.chbStartingPreset.Name = "chbStartingPreset";
            this.chbStartingPreset.Size = new System.Drawing.Size(98, 17);
            this.chbStartingPreset.TabIndex = 30;
            this.chbStartingPreset.Text = "Starting preset";
            this.chbStartingPreset.UseVisualStyleBackColor = true;
            this.chbStartingPreset.CheckedChanged += new System.EventHandler(this.chbStartingPreset_CheckedChanged);
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(155, 92);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(121, 21);
            this.comboBox2.TabIndex = 31;
            // 
            // chbRestoreClosedRct
            // 
            this.chbRestoreClosedRct.AutoSize = true;
            this.chbRestoreClosedRct.Location = new System.Drawing.Point(12, 135);
            this.chbRestoreClosedRct.Name = "chbRestoreClosedRct";
            this.chbRestoreClosedRct.Size = new System.Drawing.Size(211, 17);
            this.chbRestoreClosedRct.TabIndex = 32;
            this.chbRestoreClosedRct.Text = "Restore location && size on each closing";
            this.chbRestoreClosedRct.UseVisualStyleBackColor = true;
            this.chbRestoreClosedRct.CheckedChanged += new System.EventHandler(this.chbRestoreClosedRct_CheckedChanged);
            // 
            // SettingWindow
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(632, 274);
            this.Controls.Add(this.chbRestoreClosedRct);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.chbStartingPreset);
            this.Controls.Add(this.buttonGetCurSize);
            this.Controls.Add(this.buttonGetCurLoc);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.buttonRestoreLoc);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.nudInitialY);
            this.Controls.Add(this.nudInitialX);
            this.Controls.Add(this.chbInitialLoc);
            this.Controls.Add(this.buttonRestoreSize);
            this.Controls.Add(this.nudDelta);
            this.Controls.Add(this.labelDELTARESIZE);
            this.Controls.Add(this.checkBoxResizeMode);
            this.Controls.Add(this.chbInitialSize);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelX1);
            this.Controls.Add(this.nudInitialH);
            this.Controls.Add(this.nudInitialW);
            this.Controls.Add(this.groupBoxPresets);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Window Manager";
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDelta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_X)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_Y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_W)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPresets_H)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudInitialY)).EndInit();
            this.groupBoxPresets.ResumeLayout(false);
            this.groupBoxPresets.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudInitialW;
        private System.Windows.Forms.NumericUpDown nudInitialH;
        private System.Windows.Forms.Label labelX1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox chbInitialSize;
        private System.Windows.Forms.CheckBox checkBoxResizeMode;
        private System.Windows.Forms.NumericUpDown nudDelta;
        private System.Windows.Forms.Label labelDELTARESIZE;
        private System.Windows.Forms.Button buttonRestoreSize;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.NumericUpDown nudPresets_X;
        private System.Windows.Forms.NumericUpDown nudPresets_Y;
        private System.Windows.Forms.NumericUpDown nudPresets_W;
        private System.Windows.Forms.NumericUpDown nudPresets_H;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonSet;
        private System.Windows.Forms.Button buttonDel;
        private System.Windows.Forms.CheckBox chbInitialLoc;
        private System.Windows.Forms.NumericUpDown nudInitialX;
        private System.Windows.Forms.NumericUpDown nudInitialY;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonRestoreLoc;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonGetCurLoc;
        private System.Windows.Forms.Button buttonGetCurSize;
        private System.Windows.Forms.GroupBox groupBoxPresets;
        private System.Windows.Forms.Button buttonGetCurrentToPreset;
        private System.Windows.Forms.CheckBox chbStartingPreset;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.CheckBox chbRestoreClosedRct;
    }
}
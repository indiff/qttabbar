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
    partial class FolderViewModeWindow {
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
            this.trackBar1 = new QuizoPlugins.TrackBarEx();
            this.labelDETAIL = new QuizoPlugins.LabelEx();
            this.labelLIST = new QuizoPlugins.LabelEx();
            this.labelICON = new QuizoPlugins.LabelEx();
            this.labelTILE = new QuizoPlugins.LabelEx();
            this.labelTHUMBNAIL = new QuizoPlugins.LabelEx();
            this.labelTHUMBSTRIP = new QuizoPlugins.LabelEx();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar1
            // 
            this.trackBar1.AutoSize = false;
            this.trackBar1.Location = new System.Drawing.Point(1, 1);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(0);
            this.trackBar1.Maximum = 5;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBar1.Size = new System.Drawing.Size(38, 178);
            this.trackBar1.TabIndex = 6;
            this.trackBar1.Value = 1;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // labelDETAIL
            // 
            this.labelDETAIL.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelDETAIL.Image2 = null;
            this.labelDETAIL.Location = new System.Drawing.Point(39, 150);
            this.labelDETAIL.Margin = new System.Windows.Forms.Padding(0);
            this.labelDETAIL.Name = "labelDETAIL";
            this.labelDETAIL.Padding = new System.Windows.Forms.Padding(24, 4, 4, 4);
            this.labelDETAIL.Size = new System.Drawing.Size(107, 28);
            this.labelDETAIL.TabIndex = 5;
            this.labelDETAIL.Text = "Details";
            this.labelDETAIL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelDETAIL.Click += new System.EventHandler(this.labelButtons_Click);
            // 
            // labelLIST
            // 
            this.labelLIST.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelLIST.Image2 = null;
            this.labelLIST.Location = new System.Drawing.Point(39, 121);
            this.labelLIST.Margin = new System.Windows.Forms.Padding(0);
            this.labelLIST.Name = "labelLIST";
            this.labelLIST.Padding = new System.Windows.Forms.Padding(24, 4, 4, 4);
            this.labelLIST.Size = new System.Drawing.Size(107, 28);
            this.labelLIST.TabIndex = 4;
            this.labelLIST.Text = "List";
            this.labelLIST.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelLIST.Click += new System.EventHandler(this.labelButtons_Click);
            // 
            // labelICON
            // 
            this.labelICON.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelICON.Image2 = null;
            this.labelICON.Location = new System.Drawing.Point(39, 92);
            this.labelICON.Margin = new System.Windows.Forms.Padding(0);
            this.labelICON.Name = "labelICON";
            this.labelICON.Padding = new System.Windows.Forms.Padding(24, 4, 4, 4);
            this.labelICON.Size = new System.Drawing.Size(107, 28);
            this.labelICON.TabIndex = 3;
            this.labelICON.Text = "Icons";
            this.labelICON.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelICON.Click += new System.EventHandler(this.labelButtons_Click);
            // 
            // labelTILE
            // 
            this.labelTILE.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTILE.Image2 = null;
            this.labelTILE.Location = new System.Drawing.Point(39, 63);
            this.labelTILE.Margin = new System.Windows.Forms.Padding(0);
            this.labelTILE.Name = "labelTILE";
            this.labelTILE.Padding = new System.Windows.Forms.Padding(24, 4, 4, 4);
            this.labelTILE.Size = new System.Drawing.Size(107, 28);
            this.labelTILE.TabIndex = 2;
            this.labelTILE.Text = "Tiles";
            this.labelTILE.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelTILE.Click += new System.EventHandler(this.labelButtons_Click);
            // 
            // labelTHUMBNAIL
            // 
            this.labelTHUMBNAIL.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTHUMBNAIL.Image2 = null;
            this.labelTHUMBNAIL.Location = new System.Drawing.Point(39, 34);
            this.labelTHUMBNAIL.Margin = new System.Windows.Forms.Padding(0);
            this.labelTHUMBNAIL.Name = "labelTHUMBNAIL";
            this.labelTHUMBNAIL.Padding = new System.Windows.Forms.Padding(24, 4, 4, 4);
            this.labelTHUMBNAIL.Size = new System.Drawing.Size(107, 28);
            this.labelTHUMBNAIL.TabIndex = 1;
            this.labelTHUMBNAIL.Text = "Thumbnails";
            this.labelTHUMBNAIL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelTHUMBNAIL.Click += new System.EventHandler(this.labelButtons_Click);
            // 
            // labelTHUMBSTRIP
            // 
            this.labelTHUMBSTRIP.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelTHUMBSTRIP.Image2 = null;
            this.labelTHUMBSTRIP.Location = new System.Drawing.Point(39, 5);
            this.labelTHUMBSTRIP.Margin = new System.Windows.Forms.Padding(0);
            this.labelTHUMBSTRIP.Name = "labelTHUMBSTRIP";
            this.labelTHUMBSTRIP.Padding = new System.Windows.Forms.Padding(24, 4, 4, 4);
            this.labelTHUMBSTRIP.Size = new System.Drawing.Size(107, 28);
            this.labelTHUMBSTRIP.TabIndex = 0;
            this.labelTHUMBSTRIP.Text = "Filmstrip";
            this.labelTHUMBSTRIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelTHUMBSTRIP.Click += new System.EventHandler(this.labelButtons_Click);
            // 
            // FolderViewModeWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(155, 180);
            this.Controls.Add(this.labelTHUMBSTRIP);
            this.Controls.Add(this.labelTHUMBNAIL);
            this.Controls.Add(this.labelTILE);
            this.Controls.Add(this.labelICON);
            this.Controls.Add(this.labelLIST);
            this.Controls.Add(this.labelDETAIL);
            this.Controls.Add(this.trackBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FolderViewModeWindow";
            this.ShowInTaskbar = false;
            this.Text = "FolderViewModeWindow";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private QuizoPlugins.TrackBarEx trackBar1;
        private QuizoPlugins.LabelEx labelDETAIL;
        private QuizoPlugins.LabelEx labelLIST;
        private QuizoPlugins.LabelEx labelICON;
        private QuizoPlugins.LabelEx labelTILE;
        private QuizoPlugins.LabelEx labelTHUMBNAIL;
        private QuizoPlugins.LabelEx labelTHUMBSTRIP;
    }
}
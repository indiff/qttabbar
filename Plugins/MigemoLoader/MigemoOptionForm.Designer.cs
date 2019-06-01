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

﻿namespace QuizoPlugins
{
	partial class MigemoOptionForm
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.textBoxDLL = new System.Windows.Forms.TextBox();
			this.textBoxDic = new System.Windows.Forms.TextBox();
			this.buttonBrowseDll = new System.Windows.Forms.Button();
			this.buttonBrowseDic = new System.Windows.Forms.Button();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point( 193, 104 );
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size( 75, 23 );
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler( this.buttonOK_Click );
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point( 274, 104 );
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size( 75, 23 );
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// textBoxDLL
			// 
			this.textBoxDLL.Location = new System.Drawing.Point( 85, 12 );
			this.textBoxDLL.Name = "textBoxDLL";
			this.textBoxDLL.Size = new System.Drawing.Size( 231, 21 );
			this.textBoxDLL.TabIndex = 2;
			this.textBoxDLL.TextChanged += new System.EventHandler( this.textBoxes_TextChanged );
			// 
			// textBoxDic
			// 
			this.textBoxDic.Location = new System.Drawing.Point( 85, 39 );
			this.textBoxDic.Name = "textBoxDic";
			this.textBoxDic.Size = new System.Drawing.Size( 231, 21 );
			this.textBoxDic.TabIndex = 3;
			this.textBoxDic.TextChanged += new System.EventHandler( this.textBoxes_TextChanged );
			// 
			// buttonBrowseDll
			// 
			this.buttonBrowseDll.Location = new System.Drawing.Point( 322, 10 );
			this.buttonBrowseDll.Name = "buttonBrowseDll";
			this.buttonBrowseDll.Size = new System.Drawing.Size( 27, 23 );
			this.buttonBrowseDll.TabIndex = 4;
			this.buttonBrowseDll.Text = "...";
			this.buttonBrowseDll.UseVisualStyleBackColor = true;
			this.buttonBrowseDll.Click += new System.EventHandler( this.button_Browse_Click );
			// 
			// buttonBrowseDic
			// 
			this.buttonBrowseDic.Location = new System.Drawing.Point( 322, 37 );
			this.buttonBrowseDic.Name = "buttonBrowseDic";
			this.buttonBrowseDic.Size = new System.Drawing.Size( 27, 23 );
			this.buttonBrowseDic.TabIndex = 5;
			this.buttonBrowseDic.Text = "...";
			this.buttonBrowseDic.UseVisualStyleBackColor = true;
			this.buttonBrowseDic.Click += new System.EventHandler( this.button_Browse_Click );
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point( 12, 83 );
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size( 92, 17 );
			this.checkBox1.TabIndex = 6;
			this.checkBox1.Text = "部分一致検索";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler( this.checkBox1_CheckedChanged );
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point( 12, 15 );
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size( 43, 13 );
			this.label1.TabIndex = 7;
			this.label1.Text = "Dll path";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 12, 42 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 46, 13 );
			this.label2.TabIndex = 8;
			this.label2.Text = "Dic path";
			// 
			// MigemoOptionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size( 361, 139 );
			this.Controls.Add( this.label2 );
			this.Controls.Add( this.label1 );
			this.Controls.Add( this.checkBox1 );
			this.Controls.Add( this.buttonBrowseDic );
			this.Controls.Add( this.buttonBrowseDll );
			this.Controls.Add( this.textBoxDic );
			this.Controls.Add( this.textBoxDLL );
			this.Controls.Add( this.buttonCancel );
			this.Controls.Add( this.buttonOK );
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MigemoOptionForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Migemo Option";
			this.TopMost = true;
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.TextBox textBoxDLL;
		private System.Windows.Forms.TextBox textBoxDic;
		private System.Windows.Forms.Button buttonBrowseDll;
		private System.Windows.Forms.Button buttonBrowseDic;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
	}
}
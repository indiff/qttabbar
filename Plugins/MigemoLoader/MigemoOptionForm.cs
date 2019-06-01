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

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuizoPlugins
{
	public partial class MigemoOptionForm : Form
	{
		public string pathDLL, pathDic;
		public bool fPartialMatch;

		public MigemoOptionForm( string pathDLL, string pathDic, bool fPartialMatch )
		{
			this.pathDLL = pathDLL;
			this.pathDic = pathDic;
			this.fPartialMatch = fPartialMatch;

			InitializeComponent();

			textBoxDLL.Text = this.pathDLL;
			textBoxDic.Text = this.pathDic;
			checkBox1.Checked = this.fPartialMatch;
		}

		private void button_Browse_Click( object sender, EventArgs e )
		{
			using( OpenFileDialog ofd = new OpenFileDialog() )
			{
				bool fDLL = sender == buttonBrowseDll;

				if( fDLL )
				{
					ofd.Filter = "Migemo dll file (*.dll)|*.dll";
					ofd.FileName = pathDLL;
				}
				else
				{
					ofd.Filter = "Dictionary file ( migemo-dict )|*.*";
					ofd.FileName = pathDic;
				}

				if( DialogResult.OK == ofd.ShowDialog() )
				{
					if( fDLL )
					{
						textBoxDLL.Text = ofd.FileName;
					}
					else
					{
						textBoxDic.Text = ofd.FileName;
					}
				}
			}
		}

		private void buttonOK_Click( object sender, EventArgs e )
		{
			DialogResult = DialogResult.OK;
		}

		private void checkBox1_CheckedChanged( object sender, EventArgs e )
		{
			fPartialMatch = checkBox1.Checked;
		}

		private void textBoxes_TextChanged( object sender, EventArgs e )
		{
			if( sender == textBoxDLL )
			{
				pathDLL = textBoxDLL.Text;
				textBoxDLL.ForeColor = File.Exists( pathDLL ) ? ForeColor : Color.Red;
			}
			else
			{
				pathDic = textBoxDic.Text;
				textBoxDic.ForeColor = File.Exists( pathDic ) ? ForeColor : Color.Red;
			}
		}
	}
}
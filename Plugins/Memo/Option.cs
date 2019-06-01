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

using System.Drawing;
using System.Windows.Forms;

namespace QuizoPlugins {
    public class Option : Form {
        private Button buttonCancel;
        private Button buttonOK;
        private ComboBox comboBoxMode;

        public Option() {
            InitializeComponent();
            comboBoxMode.Items.AddRange(new string[] { Memo.ResStrs[1], Memo.ResStrs[2], Memo.ResStrs[3] });
            if((Memo.ConfigValues[0] & 0x80) == 0x80) {
                comboBoxMode.SelectedIndex = ((Memo.ConfigValues[0] & 0x40) == 0x40) ? 2 : 1;
            }
            else {
                comboBoxMode.SelectedIndex = 0;
            }
        }

        private void InitializeComponent() {
            comboBoxMode = new ComboBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            comboBoxMode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMode.FormattingEnabled = true;
            comboBoxMode.Location = new Point(0x25, 0x1d);
            comboBoxMode.Name = "comboBoxMode";
            comboBoxMode.Size = new Size(0xb2, 0x15);
            comboBoxMode.TabIndex = 1;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(0x55, 0x48);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(0x4b, 0x17);
            buttonOK.TabIndex = 2;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(0xa6, 0x48);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(0x4b, 0x17);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(0xfd, 0x6b);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(comboBoxMode);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Option";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Memo Option";
            ResumeLayout(false);
        }

        public int ShowMode {
            get {
                return comboBoxMode.SelectedIndex;
            }
        }
    }
}

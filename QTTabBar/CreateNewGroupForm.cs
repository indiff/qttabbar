//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
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
using System.Linq;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class CreateNewGroupForm : Form {
        private Button buttonCancel;
        private Button buttonOK;
        private CheckBox chkAllTabs;
        private Label label1;
        private string newPath;
        private QTabControl.QTabCollection Tabs;
        private TextBox textBox1;

        public CreateNewGroupForm(string currentPath, QTabControl.QTabCollection tabs) {
            newPath = currentPath;
            Tabs = tabs;
            InitializeComponent();
            textBox1.Text = QTUtility2.MakePathDisplayText(newPath, false);
            string[] strArray = QTUtility.TextResourcesDic["TabBar_NewGroup"];
            Text = strArray[0];
            label1.Text = strArray[1];
            chkAllTabs.Text = strArray[2];
            ActiveControl = textBox1;
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            string key = textBox1.Text;
            int num = 0;
            string tempKey = key;
            while(GroupsManager.GetGroup(tempKey) != null) {
                tempKey = key + " (" + ++num + ")";
            }
            key = tempKey;
            GroupsManager.AddGroup(key, chkAllTabs.Checked 
                    ? Tabs.Select(item => item.CurrentPath) 
                    : new string[] { newPath });
        }

        private void InitializeComponent() {
            buttonOK = new Button();
            buttonCancel = new Button();
            label1 = new Label();
            textBox1 = new TextBox();
            chkAllTabs = new CheckBox();
            SuspendLayout();
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Enabled = false;
            buttonOK.Location = new Point(0x8b, 0x42);
            buttonOK.Size = new Size(0x4b, 0x17);
            buttonOK.TabIndex = 0;
            buttonOK.Text = QTUtility.TextResourcesDic["DialogButtons"][0];
            buttonOK.Click += buttonOK_Click;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(220, 0x42);
            buttonCancel.Size = new Size(0x4b, 0x17);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text =QTUtility.TextResourcesDic["DialogButtons"][1];
            label1.AutoSize = true;
            label1.Location = new Point(12, 0x12);
            label1.Size = new Size(0x41, 12);
            textBox1.Location = new Point(0x7d, 15);
            textBox1.Size = new Size(170, 20);
            textBox1.TabIndex = 2;
            textBox1.TextChanged += textBox1_TextChanged;
            chkAllTabs.AutoSize = true;
            chkAllTabs.Location = new Point(12, 70);
            chkAllTabs.Size = new Size(90, 0x1c);
            chkAllTabs.TabIndex = 3;
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(0x133, 0x73);
            Controls.Add(chkAllTabs);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            buttonOK.Enabled = textBox1.Text.Length != 0;
        }
    }
}

//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano,indiff
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
        // 取消按钮
        private Button buttonCancel;
        // 确定按钮
        private Button buttonOK;
        // 加入所有标签
        private CheckBox chkAllTabs;
        private Label label1;
        // 路径信息
        private string newPath;
        private QTabControl.QTabCollection Tabs;
        // 分组名称
        private TextBox textBox1;
        
        public CreateNewGroupForm(string currentPath, QTabControl.QTabCollection tabs) {
            newPath = currentPath;
            Tabs = tabs;
            InitializeComponent();
            // 通过路径显示分组名称
            textBox1.Text = QTUtility2.MakePathDisplayText(newPath, false);
            string[] strArray = QTUtility.TextResourcesDic["TabBar_NewGroup"]; // 新增标签组;标签组名称:;加入所有标签
            // 设置 form 标题为路径信息
            Text = strArray[0] + " " + currentPath ; // 新增标签组 添加路径的名称
            label1.Text = strArray[1]; // 标签组名称
            chkAllTabs.Text = strArray[2]; // 选择框
            ActiveControl = textBox1;  // 文本框
        }
        /**
         * 确定按钮操作
         */
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
        /**
         * 初始化组件
         */
        private void InitializeComponent() {
            buttonOK = new Button();
            buttonCancel = new Button();
            label1 = new Label();
            textBox1 = new TextBox();
            chkAllTabs = new CheckBox();
            SuspendLayout();

            // 355 * 162  0x133 0x73  宽度设置为屏幕 1/3 高度为屏幕 1/6
            int width = Screen.PrimaryScreen.WorkingArea.Size.Width / 3;
            int height = Screen.PrimaryScreen.WorkingArea.Size.Height / 6;
            ClientSize = new Size(width, height);
            // ClientSize = new Size(0x133, 0x73);

            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Enabled = false;
            buttonOK.Location = new Point(369, 87); // 确定按钮位置
            buttonOK.Size = new Size(94, 29);
            buttonOK.TabIndex = 0;
            buttonOK.Text = QTUtility.TextResourcesDic["DialogButtons"][0]; // 确定
            buttonOK.Click += buttonOK_Click;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(516, 87);  // 取消按钮位置
            buttonCancel.Size = new Size(94, 29); // 0x17 -> 0x19 by indiff
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = QTUtility.TextResourcesDic["DialogButtons"][1];// 取消
            label1.AutoSize = true;
            label1.Location = new Point(21, 30);
            label1.Size = new Size(0x41, 12);
            textBox1.Location = new Point(165, 27);
            textBox1.Size = new Size(445, 27);
            textBox1.TabIndex = 2;
            textBox1.TextChanged += textBox1_TextChanged;
            chkAllTabs.AutoSize = true;
            chkAllTabs.Location = new Point(26, 86);
            chkAllTabs.Size = new Size(109, 24);
            chkAllTabs.TabIndex = 3;
            AcceptButton = buttonOK;
           // AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font ; // DPI 模式会造成界面乱 by indiff
            CancelButton = buttonCancel;

            Controls.Add(chkAllTabs);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            // 是否显示在任务栏
            ShowInTaskbar = false;
            // 进行居中
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            buttonOK.Enabled = textBox1.Text.Length != 0;
        }
    }
}

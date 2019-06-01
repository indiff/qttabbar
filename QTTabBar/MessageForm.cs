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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class MessageForm : Form {
        private Button btnCancel;
        private Button btnOk;
        private Button btnOk2;
        private IContainer components;
        private int duration;
        private int elapsed;
        private Label labelCounter;
        private Label labelMessage;
        private Panel panel1;
        private PictureBox pictureBoxIcon;
        private SystemSound sound;
        private string strExecute;
        private Timer timerClose;

        private MessageForm(string strMessage, string strTitle, string strExecute, MessageBoxIcon icon, int msecDuration) {
            Icon question;
            InitializeComponent();
            Text = strTitle;
            labelMessage.Text = strMessage;
            this.strExecute = strExecute;
            duration = msecDuration;
            labelCounter.Text = (duration / 0x3e8).ToString();
            using(Graphics graphics = labelMessage.CreateGraphics()) {
                SizeF ef = graphics.MeasureString(strMessage, labelMessage.Font, labelMessage.Width);
                int num = ((int)ef.Height) - labelMessage.Height;
                labelMessage.Height = (int)ef.Height;
                panel1.Height += num;
                ClientSize = new Size(ClientSize.Width, ClientSize.Height + num);
                pictureBoxIcon.Size = new Size(0x20, 0x20);
            }
            if(!string.IsNullOrEmpty(this.strExecute)) {
                btnOk.Visible = false;
                btnOk2.Visible = btnCancel.Visible = true;
            }
            switch(icon) {
                case MessageBoxIcon.Question:
                    question = SystemIcons.Question;
                    sound = SystemSounds.Question;
                    break;

                case MessageBoxIcon.Exclamation:
                    question = SystemIcons.Exclamation;
                    sound = SystemSounds.Exclamation;
                    break;

                case MessageBoxIcon.None:
                    question = SystemIcons.Information;
                    sound = null;
                    break;

                case MessageBoxIcon.Hand:
                    question = SystemIcons.Error;
                    sound = SystemSounds.Hand;
                    break;

                default:
                    question = SystemIcons.Asterisk;
                    sound = SystemSounds.Asterisk;
                    break;
            }
            pictureBoxIcon.Image = question.ToBitmap();
        }

        private void btnOk_Click(object sender, EventArgs e) {
            CloseMessageForm();
        }

        private void btnOk2_Click(object sender, EventArgs e) {
            if(!string.IsNullOrEmpty(strExecute)) {
                try {
                    Process.Start(strExecute);
                }
                catch {
                }
            }
        }

        private void CloseMessageForm() {
            timerClose.Enabled = false;
            if(!IsDisposed) {
                Dispose();
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            components = new Container();
            btnOk = new Button();
            btnOk2 = new Button();
            btnCancel = new Button();
            panel1 = new Panel();
            labelMessage = new Label();
            pictureBoxIcon = new PictureBox();
            timerClose = new Timer(components);
            labelCounter = new Label();
            panel1.SuspendLayout();
            ((ISupportInitialize)pictureBoxIcon).BeginInit();
            SuspendLayout();
            btnOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnOk.Font = new Font(Font.FontFamily, 9f);
            btnOk.Location = new Point(0x13e, 0x7a);
            btnOk.Size = new Size(0x4b, 0x1c);
            btnOk.TabIndex = 0;
            btnOk.Text = QTUtility.TextResourcesDic["DialogButtons"][0];
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            btnOk2.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnOk2.Font = new Font(Font.FontFamily, 9f);
            btnOk2.Location = new Point(0xed, 0x7a);
            btnOk2.Size = new Size(0x4b, 0x1c);
            btnOk2.TabIndex = 0;
            btnOk2.Text = QTUtility.TextResourcesDic["DialogButtons"][0];
            btnOk2.UseVisualStyleBackColor = true;
            btnOk2.Visible = false;
            btnOk2.Click += btnOk2_Click;
            btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnCancel.Font = new Font(Font.FontFamily, 9f);
            btnCancel.Location = new Point(0x13e, 0x7a);
            btnCancel.Size = new Size(0x4b, 0x1c);
            btnCancel.TabIndex = 1;
            btnCancel.Text = QTUtility.TextResourcesDic["DialogButtons"][1];
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Visible = false;
            btnCancel.Click += btnOk_Click;
            panel1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            panel1.BackColor = SystemColors.Window;
            panel1.Controls.Add(labelMessage);
            panel1.Controls.Add(pictureBoxIcon);
            panel1.Location = new Point(0, 0);
            panel1.Size = new Size(0x196, 0x70);
            labelMessage.Font = new Font(Font.FontFamily, 9f);
            labelMessage.Location = new Point(0x4d, 0x17);
            labelMessage.Size = new Size(0x13c, 0x34);
            pictureBoxIcon.Location = new Point(0x1c, 20);
            pictureBoxIcon.Size = new Size(0x20, 0x20);
            timerClose.Interval = 0x3e8;
            timerClose.Tick += timerClose_Tick;
            labelCounter.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            labelCounter.AutoSize = true;
            labelCounter.Location = new Point(0x19, 130);
            labelCounter.Size = new Size(0x23, 13);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0x195, 0xa2);
            Controls.Add(labelCounter);
            Controls.Add(panel1);
            Controls.Add(btnOk);
            Controls.Add(btnOk2);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Shown += MessageForm_Shown;
            panel1.ResumeLayout(false);
            ((ISupportInitialize)pictureBoxIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void MessageForm_Shown(object sender, EventArgs e) {
            if(sound != null) {
                sound.Play();
            }
            if(duration == 0) {
                ControlBox = labelCounter.Visible = btnOk.Visible = false;
            }
            timerClose.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if(e.KeyData == (Keys.Control | Keys.C)) {
                QTUtility2.SetStringClipboard(Text + Environment.NewLine + Environment.NewLine + labelMessage.Text);
            }
        }

        public static void Show(string strMessage, string strTitle, string strExecute, MessageBoxIcon icon, int msecDuration) {
            MessageForm form = new MessageForm(strMessage, strTitle, strExecute, icon, msecDuration);
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            form.Location = new Point((workingArea.X + workingArea.Width) - form.Width, (workingArea.Y + workingArea.Height) - form.Height);
            form.ShowMessageForm();
        }

        public static void Show(IntPtr hwndParent, string strMessage, string strTitle, MessageBoxIcon icon, int msecDuration, bool fModal = false, bool fTopMost = false) {
            MessageForm form = new MessageForm(strMessage, strTitle, null, icon, msecDuration);
            Rectangle parentRect;
            if(hwndParent != IntPtr.Zero) {
                RECT rect;
                PInvoke.GetWindowRect(hwndParent, out rect);
                parentRect = rect.ToRectangle();
            }
            else {
                parentRect = Screen.PrimaryScreen.WorkingArea;
            }
            form.Location = new Point(parentRect.Left + ((parentRect.Width - form.Width) / 2),
                    parentRect.Top + ((parentRect.Height - form.Height) / 2));
            if(fModal) {
                form.TopMost = true;
                form.ShowDialog();
            }
            else {
                form.TopMost = fTopMost;
                form.ShowMessageForm();
            }
        }

        private void ShowMessageForm() {
            PInvoke.ShowWindow(Handle, 4);
            MessageForm_Shown(this, EventArgs.Empty);
        }

        private void timerClose_Tick(object sender, EventArgs e) {
            elapsed += 0x3e8;
            labelCounter.Text = ((duration - elapsed) / 0x3e8).ToString();
            labelCounter.Refresh();
            if(elapsed >= duration) {
                timerClose.Stop();
                if(!IsDisposed) {
                    Dispose();
                }
            }
        }
    }
}

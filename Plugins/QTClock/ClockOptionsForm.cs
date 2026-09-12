using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuizoPlugins
{
    internal sealed class ClockOptionsForm : Form
    {
        private readonly RadioButton leftRadioButton;
        private ProgressBar progressBar1;
        private readonly RadioButton rightRadioButton;

        public ClockOptionsForm()
        {
            ClockLocalization.ApplyCurrentCulture();
            Text = ClockLocalization.Get("Options.Title");
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 150);

            Label label = new Label {
                AutoSize = true,
                Location = new Point(18, 18),
                Text = ClockLocalization.Get("Options.DockSide")
            };
            leftRadioButton = new RadioButton {
                AutoSize = true,
                Location = new Point(30, 52),
                Text = ClockLocalization.Get("Options.Left"),
                Checked = ClockSettings.DockSide == NewsDockSide.Left
            };
            rightRadioButton = new RadioButton {
                AutoSize = true,
                Location = new Point(180, 52),
                Text = ClockLocalization.Get("Options.Right"),
                Checked = ClockSettings.DockSide == NewsDockSide.Right
            };

            Button okButton = new Button {
                DialogResult = DialogResult.OK,
                Location = new Point(180, 102),
                Size = new Size(75, 26),
                Text = ClockLocalization.Get("Options.OK")
            };
            Button cancelButton = new Button {
                DialogResult = DialogResult.Cancel,
                Location = new Point(265, 102),
                Size = new Size(75, 26),
                Text = ClockLocalization.Get("Options.Cancel")
            };

            AcceptButton = okButton;
            CancelButton = cancelButton;
            Controls.AddRange(new Control[] { label, leftRadioButton, rightRadioButton, okButton, cancelButton });
        }

        public static void ShowOptions(IWin32Window owner)
        {
            using (ClockOptionsForm form = new ClockOptionsForm())
            {
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    ClockSettings.Save(form.leftRadioButton.Checked ? NewsDockSide.Left : NewsDockSide.Right);
                    NewsForm.GetInstance().ApplyDockSide();
                }
            }
        }

        private void InitializeComponent()
        {
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(160, 126);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(221, 62);
            this.progressBar1.TabIndex = 0;
            // 
            // ClockOptionsForm
            // 
            this.ClientSize = new System.Drawing.Size(418, 301);
            this.Controls.Add(this.progressBar1);
            this.Name = "ClockOptionsForm";
            this.ResumeLayout(false);

        }
    }
}

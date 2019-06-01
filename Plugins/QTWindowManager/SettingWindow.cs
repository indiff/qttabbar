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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QTPlugin.Interop;

namespace QuizoPlugins {
    public partial class SettingWindow : Form {
        private IntPtr hwndExplorer;
        private Dictionary<string, Rectangle> dicPresets;
        private string startingPreset;

        public SettingWindow(Rectangle rctInitial, byte[] config, int delta_RESIZE, IntPtr hwnd, Dictionary<string, Rectangle> dicPresets, string startingPreset) {
            InitializeComponent();


            if(CultureInfo.CurrentCulture.Parent.Name == "ja") {
                string[] strs = Resource.ResStrs_Options_ja.Split(new char[] { ';' });

                chbInitialSize.Text = strs[0];
                chbInitialLoc.Text = strs[1];
                buttonRestoreSize.Text = strs[2];
                buttonRestoreLoc.Text = strs[3];
                checkBoxResizeMode.Text = strs[4];
                labelDELTARESIZE.Text = strs[5];
                groupBoxPresets.Text = strs[6];
                buttonSet.Text = strs[7];
                buttonDel.Text = strs[8];
                buttonOK.Text = strs[9];
                buttonCancel.Text = strs[10];
                buttonGetCurLoc.Text = strs[11];
                buttonGetCurSize.Text = strs[12];
                chbStartingPreset.Text = strs[13];
                buttonGetCurrentToPreset.Text = strs[14];
            }


            hwndExplorer = hwnd;
            this.dicPresets = new Dictionary<string, Rectangle>(dicPresets);
            this.startingPreset = startingPreset;

            Rectangle rctScreen = Screen.FromHandle(hwnd).Bounds;
            nudInitialW.Maximum = rctScreen.Width;
            nudInitialH.Maximum = rctScreen.Height;

            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwnd, out rct);
            Text += " ( " + rct.left + ", " + rct.top + " )  " + rct.Width + " x " + rct.Height;

            try {
                if((config[0] & 0x80) != 0) {
                    chbInitialSize.Checked = true;
                }

                if((config[0] & 0x40) != 0) {
                    checkBoxResizeMode.Checked = false;
                }

                if((config[0] & 0x20) != 0) {
                    chbInitialLoc.Checked = true;
                }

                if((config[0] & 0x10) != 0) {
                    chbStartingPreset.Checked = true;
                }

                if((config[0] & 0x08) != 0) {
                    chbRestoreClosedRct.Checked = true;
                }


                nudInitialX.Value = rctInitial.X;
                nudInitialY.Value = rctInitial.Y;
                nudInitialW.Value = rctInitial.Width;
                nudInitialH.Value = rctInitial.Height;

                if(delta_RESIZE < 33 && delta_RESIZE > 0)
                    nudDelta.Value = delta_RESIZE;

                chbInitialLoc_CheckedChanged(null, EventArgs.Empty);
                chbInitialSize_CheckedChanged(null, EventArgs.Empty);

                if(chbStartingPreset.Checked) {
                    nudInitialX.Enabled = nudInitialY.Enabled = chbInitialLoc.Enabled =
                    buttonRestoreLoc.Enabled = buttonGetCurLoc.Enabled =
                    buttonRestoreSize.Enabled = buttonGetCurSize.Enabled =
                    nudInitialW.Enabled = nudInitialH.Enabled = chbInitialSize.Enabled = false;
                }
                else {
                    comboBox2.Enabled = false;
                }
            }
            catch {
            }

            foreach(string name in this.dicPresets.Keys) {
                comboBox1.Items.Add(name);
                comboBox2.Items.Add(name);
            }

            if(!string.IsNullOrEmpty(this.startingPreset)) {
                int indexStartingPreset = comboBox2.Items.IndexOf(this.startingPreset);
                if(indexStartingPreset != -1) {
                    comboBox2.SelectedIndex = indexStartingPreset;
                }
            }

            if(comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }


        public Point InitialLocation {
            get {
                return new Point((int)nudInitialX.Value, (int)nudInitialY.Value);
            }
        }

        public Size InitialSize {
            get {
                return new Size((int)nudInitialW.Value, (int)nudInitialH.Value);
            }
        }

        public int ResizeDelta {
            get {
                return (int)nudDelta.Value;
            }
        }

        public byte[] ConfigValues {
            get {
                byte[] config = new byte[] { 0, 0, 0, 0 };

                if(chbInitialSize.Checked)
                    config[0] |= 0x80;

                if(!checkBoxResizeMode.Checked)
                    config[0] |= 0x40;

                if(chbInitialLoc.Checked)
                    config[0] |= 0x20;

                if(chbStartingPreset.Checked)
                    config[0] |= 0x10;

                return config;
            }
        }

        public Dictionary<string, Rectangle> Presets {
            get {
                return dicPresets;
            }
        }

        public string StartingPreset {
            get {
                if(comboBox2.SelectedItem != null)
                    return comboBox2.SelectedItem.ToString();
                else
                    return String.Empty;
            }
        }

        private void chbInitialLoc_CheckedChanged(object sender, EventArgs e) {
            nudInitialX.Enabled = nudInitialY.Enabled =
            buttonRestoreLoc.Enabled = buttonGetCurLoc.Enabled =
            chbInitialLoc.Checked;
        }

        private void chbInitialSize_CheckedChanged(object sender, EventArgs e) {
            nudInitialW.Enabled = nudInitialH.Enabled =
            buttonRestoreSize.Enabled = buttonGetCurSize.Enabled =
            chbInitialSize.Checked;
        }

        private void chbStartingPreset_CheckedChanged(object sender, EventArgs e) {
            nudInitialX.Enabled = nudInitialY.Enabled = chbInitialLoc.Enabled =
            buttonRestoreLoc.Enabled = buttonGetCurLoc.Enabled =
            buttonRestoreSize.Enabled = buttonGetCurSize.Enabled =
            nudInitialW.Enabled = nudInitialH.Enabled = chbInitialSize.Enabled = !chbStartingPreset.Checked;

            comboBox2.Enabled = chbStartingPreset.Checked;
        }

        private void chbRestoreClosedRct_CheckedChanged(object sender, EventArgs e) {

        }

        private void buttonRestoreLoc_Click(object sender, EventArgs e) {
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOZORDER = 0x0004;

            if(hwndExplorer != IntPtr.Zero) {
                Point pnt = InitialLocation;

                PInvoke_QTWM.SetWindowPos(hwndExplorer, IntPtr.Zero, pnt.X, pnt.Y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

                QTWindowManager.RemoveMAXIMIZE(hwndExplorer);
            }
        }

        private void buttonRestoreSize_Click(object sender, EventArgs e) {
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOZORDER = 0x0004;

            if(hwndExplorer != IntPtr.Zero) {
                Size size = InitialSize;

                PInvoke_QTWM.SetWindowPos(hwndExplorer, IntPtr.Zero, 0, 0, size.Width, size.Height, SWP_NOMOVE | SWP_NOZORDER);

                QTWindowManager.RemoveMAXIMIZE(hwndExplorer);
            }
        }

        private void buttonGetCurLoc_Click(object sender, EventArgs e) {
            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwndExplorer, out rct);

            nudInitialX.Value = rct.left;
            nudInitialY.Value = rct.top;
        }

        private void buttonGetCurSize_Click(object sender, EventArgs e) {
            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwndExplorer, out rct);

            nudInitialW.Value = rct.Width;
            nudInitialH.Value = rct.Height;
        }

        private void buttonGetCurrentToPreset_Click(object sender, EventArgs e) {
            RECT rct;
            PInvoke_QTWM.GetWindowRect(hwndExplorer, out rct);

            nudPresets_X.Value = rct.left;
            nudPresets_Y.Value = rct.top;
            nudPresets_W.Value = rct.Width;
            nudPresets_H.Value = rct.Height;
        }

        private void buttonSet_Click(object sender, EventArgs e) {
            if(comboBox1.SelectedIndex != -1) {
                if(comboBox1.SelectedItem != null) {
                    string name = comboBox1.SelectedItem.ToString();

                    dicPresets[name] = new Rectangle((int)nudPresets_X.Value, (int)nudPresets_Y.Value, (int)nudPresets_W.Value, (int)nudPresets_H.Value);

                }
            }
            else if(comboBox1.Text.Length > 0) {
                string name = comboBox1.Text;

                dicPresets[name] = new Rectangle((int)nudPresets_X.Value, (int)nudPresets_Y.Value, (int)nudPresets_W.Value, (int)nudPresets_H.Value);

                comboBox1.Items.Add(name);

            }
        }

        private void buttonDel_Click(object sender, EventArgs e) {
            if(comboBox1.SelectedItem != null) {
                dicPresets.Remove(comboBox1.SelectedItem.ToString());
                comboBox1.Items.Remove(comboBox1.SelectedItem);

                comboBox1.Text = String.Empty;

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if(comboBox1.SelectedItem != null) {
                string name = comboBox1.SelectedItem.ToString();

                Rectangle rct;
                if(dicPresets.TryGetValue(name, out rct)) {
                    nudPresets_X.Value = rct.X;
                    nudPresets_Y.Value = rct.Y;
                    nudPresets_W.Value = rct.Width;
                    nudPresets_H.Value = rct.Height;
                }
            }
        }

    }
}
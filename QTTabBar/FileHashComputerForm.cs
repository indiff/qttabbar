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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class FileHashComputerForm : Form {
        private Button btnClear;
        private Button btnClose;
        private Button btnRefresh;
        private int cErr_Prv;
        private CheckBox chbClearOnClose;
        private CheckBox chbFullPath;
        private CheckBox chbShowResult;
        private CheckBox chbTopMost;
        private DataGridViewImageColumn clmn1_Icon;
        private DataGridViewTextBoxColumn clmn2_Path;
        private DataGridViewProgressBarColumn clmn3_Hash;
        private static Color clrNew = Color.FromArgb(0xff, 0xea, 0xff);
        private int cMatched_Prv;
        private ComboBox cmbHashType;
        private static int colorIndex;
        private static int colorIndexModTimeDiffers = 1;
        private static Color[] colors = new Color[] { Color.FromArgb(0xd1, 0xff, 0xff), Color.FromArgb(0xd1, 0xff, 0xd1), Color.FromArgb(0xff, 0xff, 0xd1), Color.FromArgb(0xff, 0xd1, 0xd1), Color.FromArgb(0xff, 0xd1, 0xff), Color.FromArgb(0xd1, 0xd1, 0xff), Color.FromArgb(0xd1, 0xff, 0xe8), Color.FromArgb(0xe8, 0xff, 0xd1) };
        private DataGridView dgvHash;
        private Dictionary<string, List<DataGridViewRow>> dicResult = new Dictionary<string, List<DataGridViewRow>>();
        private volatile bool fCancellationPending;
        private volatile int iThreadsCounter;
        private Panel panel1;
        private Queue<PathRowPairs> qPendings = new Queue<PathRowPairs>();
        private static string VALUE_EMPTY = "                                ";
        private static string VALUE_ERROR = "Error. Failed to open?";

        public FileHashComputerForm() {
            InitializeComponent();
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    chbFullPath.Checked = (int)key.GetValue("MD5FormFullPath", 0) == 1;
                    chbClearOnClose.Checked = (int)key.GetValue("MD5FormClearOnClose", 0) == 1;
                    chbShowResult.Checked = (int)key.GetValue("MD5FormShowResult", 0) == 1;
                    chbTopMost.Checked = (int)key.GetValue("MD5FormTopMost", 0) == 1;
                    int location = (int)key.GetValue("MD5FormLocation", 0x640064);
                    int size = (int)key.GetValue("MD5FormSize", 0xc801c2);
                    int type = (int)key.GetValue("HashType", 0);
                    Location = new Point((short)(location & 0xffff), (short)((location >> 0x10) & 0xffff));
                    Size = new Size(size & 0xffff, (size >> 0x10) & 0xffff);
                    if((type < 0) || (type > (cmbHashType.Items.Count - 1))) {
                        type = 0;
                    }
                    cmbHashType.SelectedIndex = type;
                }
            }
            PInvoke.DragAcceptFiles(Handle, true);
        }

        private void AddDropped(IntPtr hDrop) {
            int capacity = (int)PInvoke.DragQueryFile(hDrop, uint.MaxValue, null, 0);
            if(capacity > 0) {
                List<string> list = new List<string>(capacity);
                for(int i = 0; i < capacity; i++) {
                    StringBuilder lpszFile = new StringBuilder(260);
                    PInvoke.DragQueryFile(hDrop, (uint)i, lpszFile, lpszFile.Capacity);
                    if(File.Exists(lpszFile.ToString())) {
                        list.Add(lpszFile.ToString());
                    }
                }
                if(list.Count > 0) {
                    ShowFileHashForm(list.ToArray());
                }
            }
            PInvoke.DragFinish(hDrop);
        }

        private void AsyncComplete(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((HashInvoker)result.AsyncDelegate).EndInvoke(ar);
            if(IsHandleCreated && !Disposing) {
                BeginInvoke(new MethodInvoker(ComputeFinished));
            }
            iThreadsCounter--;
        }

        private void buttonClear_Click(object sender, EventArgs e) {
            ClearRows();
        }

        private void buttonClose_Click(object sender, EventArgs e) {
            qPendings.Clear();
            if(iThreadsCounter > 0) {
                clmn3_Hash.StopAll();
                fCancellationPending = true;
            }
            else {
                if(chbClearOnClose.Checked) {
                    ClearRows();
                }
                SaveMD5FormStat();
                Hide();
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            string[] list = dgvHash.Rows.Cast<DataGridViewRow>()
                .Select(row => row.Cells[1].ToolTipText).Distinct().ToArray();
            ClearRows();
            ShowFileHashForm(list);
        }

        private void checkBoxFullPath_CheckedChanged(object sender, EventArgs e) {
            bool flag = chbFullPath.Checked;
            dgvHash.SuspendLayout();
            foreach(DataGridViewRow row in dgvHash.Rows) {
                row.Cells[1].Value = flag ? row.Cells[1].ToolTipText : Path.GetFileName(row.Cells[1].ToolTipText);
            }
            dgvHash.ResumeLayout();
        }

        private void checkBoxTopMost_CheckedChanged(object sender, EventArgs e) {
            IntPtr hWndInsertAfter = chbTopMost.Checked ? ((IntPtr)(-1)) : ((IntPtr)(-2));
            PInvoke.SetWindowPos(Handle, hWndInsertAfter, 0, 0, 0, 0, 0x53);
        }

        private void ClearNewColor() {
            foreach(DataGridViewRow row in dgvHash.Rows) {
                row.Cells[0].Style.BackColor = Color.Empty;
            }
        }

        private void ClearRows() {
            dgvHash.SuspendLayout();
            dgvHash.Rows.Clear();
            dgvHash.ResumeLayout();
            colorIndex = 0;
            dicResult.Clear();
            cMatched_Prv = cErr_Prv = 0;
        }

        private void ComputeFinished() {
            if(fCancellationPending) {
                SetButtonsEnabled(true);
                Text = MakeHashTypeText();
                fCancellationPending = false;
                qPendings.Clear();
            }
            else if(qPendings.Count > 0) {
                PathRowPairs pairs = qPendings.Dequeue();
                iThreadsCounter++;
                new HashInvoker(ComputeHashCore).BeginInvoke(pairs.Paths.ToArray(), pairs.Rows.ToArray(), cmbHashType.SelectedIndex, AsyncComplete, null);
            }
            else {
                SetButtonsEnabled(true);
                Text = MakeHashTypeText();
                int num = 0;
                int num2 = 0;
                int count = 0;
                foreach(List<DataGridViewRow> list in dicResult.Values.Where(list => list.Count > 0)) {
                    if(list[0].Cells[2].ToolTipText == VALUE_ERROR) {
                        count = list.Count;
                    }
                    if(list.Count > 1) {
                        num2++;
                        num += list.Count;
                    }
                }
                if(chbShowResult.Checked) {
                    string str = (count > 1) ? "s." : ".";
                    if(cMatched_Prv != num2) {
                        string str2 = (num > 1) ? "s" : string.Empty;
                        string str3 = (num2 > 1) ? "s" : string.Empty;
                        MessageForm.Show(Handle, ((num2 > 0) ? string.Concat(new object[] { num, " file", str2, " matched in ", num2, " ", MakeHashTypeText(), str3 }) : string.Empty) + ((count > 0) ? string.Concat(new object[] { (num2 > 0) ? ", " : string.Empty, count, " error", str }) : "."), "Result", MessageBoxIcon.Asterisk, 0x1388);
                    }
                    else if(cErr_Prv != count) {
                        MessageForm.Show(Handle, count + " error" + str, "Result", MessageBoxIcon.Hand, 0x1388);
                    }
                }
                else if(cMatched_Prv != num2) {
                    SystemSounds.Asterisk.Play();
                }
                else if(cErr_Prv != count) {
                    SystemSounds.Hand.Play();
                }
                cMatched_Prv = num2;
                cErr_Prv = count;
            }
        }

        private void ComputeHashCore(string[] paths, DataGridViewRow[] rows, int iHashType) {
            for(int i = 0; i < paths.Length; i++) {
                try {
                    byte[] buffer;
                    DataGridViewProgressBarCell cell = (DataGridViewProgressBarCell)rows[i].Cells[2];
                    using(FileHashStream stream = new FileHashStream(paths[i], ReportCallbackAsync, cell)) {
                        using(HashAlgorithm algorithm = CreateHashAlgorithm(iHashType)) {
                            buffer = algorithm.ComputeHash(stream);
                        }
                        if(stream.IsAborted) {
                            cell.CalculatingStatus = HashCalcStatus.Aborted;
                            return;
                        }
                    }
                    StringBuilder builder = new StringBuilder(buffer.Length);
                    for(int j = 0; j < buffer.Length; j++) {
                        builder.Append(buffer[j].ToString("X2"));
                    }
                    Invoke(new HashInvoker2(SetRowSync), new object[] { paths[i], builder.ToString(), rows[i] });
                }
                catch {
                    Invoke(new HashInvoker2(SetRowSync), new object[] { paths[i], VALUE_ERROR, rows[i] });
                }
                if(fCancellationPending) {
                    return;
                }
            }
        }

        private HashAlgorithm CreateHashAlgorithm(int index) {
            switch(index) {
                case 1:
                    return new SHA1CryptoServiceProvider();

                case 2:
                    return new SHA256Managed();

                case 3:
                    return new SHA384Managed();

                case 4:
                    return new SHA512Managed();
            }
            return new MD5CryptoServiceProvider();
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e) {
            if(e.RowIndex >= 0 && e.ColumnIndex > 0) {
                string path = dgvHash.Rows[e.RowIndex].Cells[1].ToolTipText;
                InstanceManager.InvokeMain(tabBar => {
                    using(IDLWrapper idlw = new IDLWrapper(Path.GetDirectoryName(path))) {
                        if(idlw.Available) {
                            tabBar.OpenNewTabOrWindow(idlw);
                        }
                    }
                });
            }
        }

        private void dataGridView1_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            if((e.Cell.ColumnIndex == 0) && (e.StateChanged == DataGridViewElementStates.Selected)) {
                dgvHash.Rows[e.Cell.RowIndex].Cells[1].Selected = dgvHash.Rows[e.Cell.RowIndex].Cells[2].Selected = e.Cell.Selected;
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e) {
            if((e.KeyCode == Keys.Delete) && (dgvHash.SelectedCells.Count > 0)) {
                if(iThreadsCounter > 0) {
                    e.Handled = true;
                }
                else {
                    List<DataGridViewRow> list = dgvHash.SelectedCells.Cast<DataGridViewCell>()
                            .Select(cell => cell.OwningRow).Distinct().ToList();
                    dgvHash.SuspendLayout();
                    foreach(DataGridViewRow row2 in list) {
                        dgvHash.Rows.Remove(row2);
                    }
                    dgvHash.ResumeLayout();
                    dicResult.Clear();
                    cMatched_Prv = cErr_Prv = 0;
                    foreach(DataGridViewRow row3 in dgvHash.Rows) {
                        string toolTipText = row3.Cells[2].ToolTipText;
                        if(toolTipText == VALUE_ERROR) {
                            cErr_Prv++;
                        }
                        if(!dicResult.ContainsKey(toolTipText)) {
                            List<DataGridViewRow> list2 = new List<DataGridViewRow>();
                            list2.Add(row3);
                            dicResult[toolTipText] = list2;
                        }
                        else {
                            dicResult[toolTipText].Add(row3);
                        }
                    }
                    cMatched_Prv += dicResult.Values.Count(list3 => list3.Count > 1);
                }
            }
            else if(e.KeyCode == Keys.Escape) {
                Hide();
            }
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e) {
            if((dgvHash.HitTest(e.X, e.Y).RowIndex == -1) && (dgvHash.Columns[0].HeaderCell.Size.Height < e.Y)) {
                foreach(DataGridViewRow row in dgvHash.Rows) {
                    row.Selected = false;
                    row.Cells[0].Style.BackColor = Color.Empty;
                }
            }
        }

        protected override void Dispose(bool disposing) {
            if(IsHandleCreated) {
                PInvoke.DragAcceptFiles(Handle, false);
            }
            fCancellationPending = true;
            while(iThreadsCounter > 0) {
                Application.DoEvents();
            }
            base.Dispose(disposing);
        }

        public void HideHashForm() {
            qPendings.Clear();
            if(iThreadsCounter > 0) {
                if(!chbClearOnClose.Checked) {
                    clmn3_Hash.StopAll();
                }
                fCancellationPending = true;
            }
            if(chbClearOnClose.Checked) {
                ClearRows();
            }
            SaveMD5FormStat();
            Hide();
        }

        private void InitializeComponent() {
            dgvHash = new DataGridViewEx();
            clmn1_Icon = new DataGridViewImageColumn();
            clmn2_Path = new DataGridViewTextBoxColumn();
            clmn3_Hash = new DataGridViewProgressBarColumn();
            panel1 = new Panel();
            btnClear = new Button();
            btnClose = new Button();
            btnRefresh = new Button();
            chbFullPath = new CheckBox();
            chbClearOnClose = new CheckBox();
            chbShowResult = new CheckBox();
            chbTopMost = new CheckBox();
            cmbHashType = new ComboBox();
            ((ISupportInitialize)dgvHash).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            dgvHash.AllowUserToAddRows = false;
            dgvHash.AllowUserToResizeRows = false;
            dgvHash.BackgroundColor = SystemColors.Window;
            dgvHash.BorderStyle = BorderStyle.None;
            dgvHash.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvHash.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHash.Columns.AddRange(new DataGridViewColumn[] { clmn1_Icon, clmn2_Path, clmn3_Hash });
            dgvHash.Dock = DockStyle.Fill;
            dgvHash.GridColor = SystemColors.ControlLight;
            dgvHash.Location = new Point(0, 0);
            dgvHash.ReadOnly = true;
            dgvHash.RowHeadersVisible = false;
            dgvHash.RowTemplate.Height = 0x15;
            dgvHash.Size = new Size(0x1bf, 0x6c);
            dgvHash.MouseDown += dataGridView1_MouseDown;
            dgvHash.KeyDown += dataGridView1_KeyDown;
            dgvHash.CellStateChanged += dataGridView1_CellStateChanged;
            dgvHash.CellMouseDoubleClick += dataGridView1_CellMouseDoubleClick;
            clmn1_Icon.ReadOnly = true;
            clmn1_Icon.Resizable = DataGridViewTriState.False;
            clmn1_Icon.Width = 0x12;
            clmn2_Path.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            clmn2_Path.HeaderText = QTUtility.TextResourcesDic["FileHashComputer"][0];
            clmn2_Path.MinimumWidth = 80;
            clmn2_Path.ReadOnly = true;
            clmn3_Hash.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            clmn3_Hash.HeaderText = QTUtility.TextResourcesDic["FileHashComputer"][1];
            clmn3_Hash.MinimumWidth = 200;
            clmn3_Hash.ReadOnly = true;
            panel1.Controls.Add(cmbHashType);
            panel1.Controls.Add(chbTopMost);
            panel1.Controls.Add(chbShowResult);
            panel1.Controls.Add(chbClearOnClose);
            panel1.Controls.Add(chbFullPath);
            panel1.Controls.Add(btnClear);
            panel1.Controls.Add(btnClose);
            panel1.Controls.Add(btnRefresh);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 0x6c);
            panel1.Size = new Size(0x1bf, 0x5d);
            panel1.Paint += panel1_Paint;
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnClose.Location = new Point(0x171, 0x43);
            btnClose.Size = new Size(0x4b, 0x17);
            btnClose.Text = QTUtility.TextResourcesDic["FileHashComputer"][2];
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += buttonClose_Click;
            btnClear.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnClear.Location = new Point(0x120, 0x43);
            btnClear.Size = new Size(0x4b, 0x17);
            btnClear.Text = QTUtility.TextResourcesDic["FileHashComputer"][3];
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += buttonClear_Click;
            btnRefresh.Location = new Point(0x89, 8);
            btnRefresh.Size = new Size(0x4b, 0x15);
            btnRefresh.Text = QTUtility.TextResourcesDic["FileHashComputer"][4];
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += buttonRefresh_Click;
            cmbHashType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHashType.Items.AddRange(new object[] { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" });
            cmbHashType.Location = new Point(12, 8);
            cmbHashType.Size = new Size(0x79, 0x15);
            chbTopMost.AutoSize = true;
            chbTopMost.Location = new Point(12, 0x43);
            chbTopMost.Size = new Size(0x4b, 0x17);
            chbTopMost.Text = QTUtility.TextResourcesDic["FileHashComputer"][5];
            chbTopMost.UseVisualStyleBackColor = true;
            chbTopMost.CheckedChanged += checkBoxTopMost_CheckedChanged;
            chbShowResult.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            chbShowResult.AutoSize = true;
            chbShowResult.Location = new Point(0xd7, 0x29);
            chbShowResult.Size = new Size(0x5e, 0x11);
            chbShowResult.Text = QTUtility.TextResourcesDic["FileHashComputer"][6];
            chbShowResult.UseVisualStyleBackColor = true;
            chbClearOnClose.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            chbClearOnClose.AutoSize = true;
            chbClearOnClose.Location = new Point(0x60, 0x29);
            chbClearOnClose.Size = new Size(0x5e, 0x11);
            chbClearOnClose.Text = QTUtility.TextResourcesDic["FileHashComputer"][7];
            chbClearOnClose.UseVisualStyleBackColor = true;
            chbFullPath.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            chbFullPath.AutoSize = true;
            chbFullPath.Location = new Point(12, 0x29);
            chbFullPath.Size = new Size(0x5e, 0x11);
            chbFullPath.Text = QTUtility.TextResourcesDic["FileHashComputer"][8];
            chbFullPath.UseVisualStyleBackColor = true;
            chbFullPath.CheckedChanged += checkBoxFullPath_CheckedChanged;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0x1bf, 0xab);
            Controls.Add(dgvHash);
            Controls.Add(panel1);
            MinimumSize = new Size(320, 0xd5);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = QTUtility.TextResourcesDic["FileHashComputer"][1];
            FormClosing += MD5Form_FormClosing;
            ((ISupportInitialize)dgvHash).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private string MakeHashTypeText() {
            switch(cmbHashType.SelectedIndex) {
                case 1:
                    return "SHA-1";

                case 2:
                    return "SHA-256";

                case 3:
                    return "SHA-384";

                case 4:
                    return "SHA-512";
            }
            return "MD5";
        }

        private void MD5Form_FormClosing(object sender, FormClosingEventArgs e) {
            if(e.CloseReason == CloseReason.WindowsShutDown) {
                SaveMD5FormStat();
            }
            else {
                e.Cancel = true;
                HideHashForm();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) {
            e.Graphics.DrawLine(SystemPens.ControlDark, 0, 0, panel1.Width - 1, 0);
        }

        private bool ReportCallbackAsync(DataGridViewProgressBarCell cell) {
            Invoke(new ReportProgressCallback(ReportCellProgress), new object[] { cell });
            return fCancellationPending;
        }

        private bool ReportCellProgress(DataGridViewProgressBarCell cell) {
            cell.Progress(1);
            return false;
        }

        public void SaveMD5FormStat() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                key.SetValue("MD5FormLocation", QTUtility2.Make_INT(Left, Top));
                key.SetValue("MD5FormSize", Width | (Height << 0x10));
                key.SetValue("HashType", cmbHashType.SelectedIndex);
                key.SetValue("MD5FormFullPath", chbFullPath.Checked ? 1 : 0);
                key.SetValue("MD5FormClearOnClose", chbClearOnClose.Checked ? 1 : 0);
                key.SetValue("MD5FormShowResult", chbShowResult.Checked ? 1 : 0);
                key.SetValue("MD5FormTopMost", chbTopMost.Checked ? 1 : 0);
            }
        }

        private void SetButtonsEnabled(bool fEnabled) {
            btnClose.Text = fEnabled ? "Close" : "Stop";
            btnClear.Enabled = btnRefresh.Enabled = cmbHashType.Enabled = fEnabled;
        }

        private void SetCellColorByHash(DataGridViewRow addedRow, string strHash) {
            List<DataGridViewRow> list;
            if(!dicResult.ContainsKey(strHash)) {
                list = new List<DataGridViewRow>();
                list.Add(addedRow);
                dicResult[strHash] = list;
            }
            else {
                list = dicResult[strHash];
                list.Add(addedRow);
            }
            if(list.Count > 1) {
                RowProperties tag = (RowProperties)list[0].Tag;
                if(tag.colorIndex < 0) {
                    foreach(DataGridViewRow row in list) {
                        ((RowProperties)row.Tag).colorIndex = colorIndex;
                        row.Cells[2].Style.BackColor = colors[colorIndex];
                    }
                    colorIndex++;
                    if(colorIndex > (colors.Length - 1)) {
                        colorIndex = 0;
                    }
                }
                else {
                    ((RowProperties)addedRow.Tag).colorIndex = tag.colorIndex;
                    addedRow.Cells[2].Style.BackColor = colors[tag.colorIndex];
                }
            }
        }

        private void SetRowSync(string path, string strHash, DataGridViewRow row) {
            if(dgvHash.IsHandleCreated) {
                DataGridViewProgressBarCell cell = (DataGridViewProgressBarCell)row.Cells[2];
                clmn3_Hash.FinishProgress(cell);
                if(cell.CalculatingStatus == HashCalcStatus.Finished) {
                    row.Cells[2].Value = strHash;
                    row.Cells[2].ToolTipText = strHash;
                    row.Cells[0].Style.BackColor = clrNew;
                    SetCellColorByHash(row, strHash);
                }
            }
        }

        public void ShowFileHashForm(string[] paths) {
            if(!fCancellationPending) {
                ClearNewColor();
                Text = MakeHashTypeText() + QTUtility.TextResourcesDic["FileHashComputer"][9];
                SetButtonsEnabled(false);
                List<string> list = new List<string>();
                List<DataGridViewRow> rows = new List<DataGridViewRow>();
                bool flag = chbFullPath.Checked;
                if(paths != null) {
                    dgvHash.SuspendLayout();
                    foreach(string str in paths) {
                        FileInfo info = new FileInfo(str);
                        if(info.Exists) {
                            DateTime lastWriteTime = info.LastWriteTime;
                            List<DataGridViewRow> list3 = new List<DataGridViewRow>();
                            List<DataGridViewRow> list4 = new List<DataGridViewRow>();
                            int rowIndex = -1;
                            bool flag2 = false;
                            foreach(DataGridViewRow row in dgvHash.Rows) {
                                if(!flag2 && row.Cells[1].ToolTipText.PathEquals(str)) {
                                    if((row.Tag != null) && (lastWriteTime != ((RowProperties)row.Tag).modTime)) {
                                        list3.Add(row);
                                        row.Selected = false;
                                    }
                                    else if(((DataGridViewProgressBarCell)row.Cells[2]).CalculatingStatus == HashCalcStatus.Aborted) {
                                        list4.Add(row);
                                    }
                                    else {
                                        row.Selected = true;
                                        flag2 = true;
                                    }
                                }
                                else {
                                    row.Selected = false;
                                }
                            }
                            foreach(DataGridViewRow row2 in list4) {
                                rowIndex = row2.Index;
                                dgvHash.Rows.Remove(row2);
                            }
                            if(!flag2) {
                                DataGridViewRow item = new DataGridViewRow();
                                item.CreateCells(dgvHash, new object[] { QTUtility.ImageListGlobal.Images[QTUtility.GetImageKey(str, Path.GetExtension(str))], flag ? str : Path.GetFileName(str), VALUE_EMPTY });
                                item.Cells[0].Style.BackColor = clrNew;
                                item.Cells[1].ToolTipText = str;
                                item.Tag = new RowProperties(lastWriteTime);
                                if(list3.Count > 0) {
                                    RowProperties tag = (RowProperties)list3[0].Tag;
                                    if(tag.colorIndexModTimeDiffers < 0) {
                                        list3.Add(item);
                                        foreach(DataGridViewRow row4 in list3) {
                                            ((RowProperties)row4.Tag).colorIndexModTimeDiffers = colorIndexModTimeDiffers;
                                            row4.Cells[1].Style.BackColor = colors[colorIndexModTimeDiffers];
                                        }
                                        colorIndexModTimeDiffers++;
                                        if(colorIndexModTimeDiffers > (colors.Length - 1)) {
                                            colorIndexModTimeDiffers = 0;
                                        }
                                    }
                                    else {
                                        ((RowProperties)item.Tag).colorIndexModTimeDiffers = tag.colorIndexModTimeDiffers;
                                        item.Cells[1].Style.BackColor = colors[tag.colorIndexModTimeDiffers];
                                    }
                                }
                                if(rowIndex != -1) {
                                    dgvHash.Rows.Insert(rowIndex, item);
                                }
                                else {
                                    dgvHash.Rows.Add(item);
                                }
                                dgvHash.FirstDisplayedScrollingRowIndex = dgvHash.Rows.IndexOf(item);
                                list.Add(str);
                                rows.Add(item);
                                clmn3_Hash.InitializeProgress((DataGridViewProgressBarCell)item.Cells[2]);
                            }
                        }
                    }
                    if(list.Count > 0) {
                        if(iThreadsCounter > 0) {
                            qPendings.Enqueue(new PathRowPairs(list, rows));
                        }
                        else {
                            iThreadsCounter++;
                            new HashInvoker(ComputeHashCore).BeginInvoke(list.ToArray(), rows.ToArray(), cmbHashType.SelectedIndex, AsyncComplete, null);
                        }
                    }
                    dgvHash.ResumeLayout();
                }
                Show();
                PInvoke.SetWindowPos(Handle, chbTopMost.Checked ? ((IntPtr)(-1)) : IntPtr.Zero, 0, 0, 0, 0, 0x53);
                if(list.Count == 0) {
                    ComputeFinished();
                }
            }
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == WM.DROPFILES) {
                AddDropped(m.WParam);
            }
            base.WndProc(ref m);
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }

        private sealed class FileHashStream : Stream {
            private ReportProgressCallback callbackCancelAsync;
            private DataGridViewProgressBarCell cell;
            private bool fAborted;
            private bool fEnoughSize;
            private FileStream fs;
            private long lCallbackInterval = 0x64000L;
            private long lCounter;
            private const long MIN_REPORTSIZE = 0x200000L;

            public FileHashStream(string path, ReportProgressCallback callbackCancelAsync, DataGridViewProgressBarCell cell) {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                long length = fs.Length;
                fEnoughSize = length > 0x200000L;
                this.callbackCancelAsync = callbackCancelAsync;
                this.cell = cell;
                this.cell.FileSize = length;
            }

            protected override void Dispose(bool disposing) {
                if(fs != null) {
                    fs.Dispose();
                }
                callbackCancelAsync = null;
                base.Dispose(disposing);
            }

            public override void Flush() {
            }

            public override int Read(byte[] buffer, int offset, int count) {
                if(fEnoughSize) {
                    lCounter += count;
                    if(lCounter >= lCallbackInterval) {
                        lCounter = 0L;
                        if((callbackCancelAsync != null) && callbackCancelAsync(cell)) {
                            fAborted = true;
                            return 0;
                        }
                    }
                }
                return fs.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin) {
                return fs.Seek(offset, origin);
            }

            public override void SetLength(long value) {
            }

            public override void Write(byte[] buffer, int offset, int count) {
            }

            public override bool CanRead {
                get {
                    return fs.CanRead;
                }
            }

            public override bool CanSeek {
                get {
                    return fs.CanSeek;
                }
            }

            public override bool CanWrite {
                get {
                    return false;
                }
            }

            public bool IsAborted {
                get {
                    return fAborted;
                }
            }

            public override long Length {
                get {
                    return fs.Length;
                }
            }

            public override long Position {
                get {
                    return fs.Position;
                }
                set {
                    fs.Position = value;
                }
            }
        }

        private delegate void HashInvoker(string[] paths, DataGridViewRow[] rows, int iHashType);

        private delegate void HashInvoker2(string path, string md5, DataGridViewRow row);

        private sealed class PathRowPairs {
            public List<string> Paths;
            public List<DataGridViewRow> Rows;

            public PathRowPairs(List<string> paths, List<DataGridViewRow> rows) {
                Paths = paths;
                Rows = rows;
            }
        }

        private delegate bool ReportProgressCallback(DataGridViewProgressBarCell cell);

        private sealed class RowProperties {
            public int colorIndex;
            public int colorIndexModTimeDiffers;
            public DateTime modTime;

            public RowProperties(DateTime modTime) {
                colorIndex = colorIndexModTimeDiffers = -1;
                this.modTime = modTime;
            }
        }
    }
}

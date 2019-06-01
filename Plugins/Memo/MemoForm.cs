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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace QuizoPlugins {
    internal sealed class MemoForm : Form {
        private Button button1;
        private ToolStripMenuItem colorToolStripMenuItem;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private IContainer components;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem copyToolStripMenuItem;
        private string currentPath = string.Empty;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem defaultFontToolStripMenuItem;
        private ToolStripMenuItem defaultToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private bool fFirstLoadComplete;
        private bool fNowShown;
        private List<Font> fontList = new List<Font>();
        private ListView listView1;
        private static int NUM_LVTEXTWIDTH = 100;
        private Memo owner;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private static string PATH_DAT = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Quizo\QTTabBar\1.0.9\memo.dat");
        private RichTextBox richTextBox1;
        private Dictionary<string, string> rtfDic;
        private ToolStripMenuItem searchToolStripMenuItem;
        private TextBox textBox1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripTrackBar toolStripTrackBar;
        private Dictionary<string, string> txtDic;
        
        [Serializable]
        public sealed class MemoStore {
            private Rectangle _bounds;
            private Dictionary<string, string> _dic;
            private double _opacity;
            private Dictionary<string, string> _txtDic;

            public Rectangle Bounds {
                get {
                    return _bounds;
                }
                set {
                    _bounds = value;
                }
            }

            public Dictionary<string, string> Dictionary {
                get {
                    return _dic;
                }
                set {
                    _dic = value;
                }
            }

            public double Opacity {
                get {
                    return _opacity;
                }
                set {
                    _opacity = value;
                }
            }

            public Dictionary<string, string> TxtDictionary {
                get {
                    return _txtDic;
                }
                set {
                    _txtDic = value;
                }
            }
        }
    
        public MemoForm(Memo memo) {
            owner = memo;
            InitializeComponent();
            toolStripTrackBar = new ToolStripTrackBar();
            toolStripTrackBar.BlockColorChange = true;
            toolStripTrackBar.ValueChanged += toolStripTrackBar_ValueChanged;
            contextMenuStrip1.Items.Add(toolStripTrackBar);
        }

        private void button1_Click(object sender, EventArgs e) {
            if(textBox1.TextLength == 0) {
                CreateMemoList();
            }
            else {
                string text = textBox1.Text;
                List<string> list = new List<string>();
                foreach(string str2 in txtDic.Keys) {
                    if(txtDic[str2].IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1) {
                        list.Add(str2);
                    }
                }
                listView1.BeginUpdate();
                listView1.Items.Clear();
                foreach(string str3 in list) {
                    string str4 = txtDic[str3];
                    if(str4.Length > NUM_LVTEXTWIDTH) {
                        str4 = str4.Substring(0, NUM_LVTEXTWIDTH) + "...";
                    }
                    ListViewItem item = new ListViewItem(new string[] { str3, str4 });
                    listView1.Items.Add(item);
                }
                listView1.EndUpdate();
            }
        }

        public bool ContainsPath(string path) {
            if(rtfDic == null) {
                LoadDB();
            }
            return rtfDic.ContainsKey(path);
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem == colorToolStripMenuItem) {
                ColorDialog dialog = new ColorDialog();
                if(richTextBox1.SelectionColor != Color.Empty) {
                    dialog.Color = richTextBox1.SelectionColor;
                }
                if(dialog.ShowDialog() == DialogResult.OK) {
                    richTextBox1.SelectionColor = dialog.Color;
                }
                dialog.Dispose();
            }
            else if(e.ClickedItem == defaultToolStripMenuItem) {
                richTextBox1.SelectionColor = SystemColors.WindowText;
            }
            else if(e.ClickedItem == defaultFontToolStripMenuItem) {
                richTextBox1.SelectionFont = richTextBox1.Font;
            }
            else if(e.ClickedItem == cutToolStripMenuItem) {
                richTextBox1.Cut();
            }
            else if(e.ClickedItem == copyToolStripMenuItem) {
                richTextBox1.Copy();
            }
            else if(e.ClickedItem == pasteToolStripMenuItem) {
                richTextBox1.Paste();
            }
            else if(e.ClickedItem == deleteToolStripMenuItem) {
                richTextBox1.SelectedText = string.Empty;
            }
            else if(e.ClickedItem == searchToolStripMenuItem) {
                if(richTextBox1.CanUndo) {
                    FixCurrent();
                }
                CreateMemoList();
                richTextBox1.Visible = false;
                Refresh();
                textBox1.Focus();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            if(richTextBox1.SelectionLength < 1) {
                colorToolStripMenuItem.Visible = defaultToolStripMenuItem.Visible = defaultFontToolStripMenuItem.Visible = false;
                toolStripTrackBar.Visible = true;
                cutToolStripMenuItem.Enabled = copyToolStripMenuItem.Enabled = deleteToolStripMenuItem.Enabled = false;
            }
            else {
                colorToolStripMenuItem.Visible = defaultToolStripMenuItem.Visible = defaultFontToolStripMenuItem.Visible = true;
                toolStripTrackBar.Visible = false;
                cutToolStripMenuItem.Enabled = copyToolStripMenuItem.Enabled = deleteToolStripMenuItem.Enabled = true;
            }
        }

        private void CreateMemoList() {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach(string str in txtDic.Keys) {
                string str2 = txtDic[str];
                if(str2.Length > NUM_LVTEXTWIDTH) {
                    str2 = str2.Substring(0, NUM_LVTEXTWIDTH) + "...";
                }
                ListViewItem item = new ListViewItem(new string[] { str, str2 });
                item.ToolTipText = str;
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }

        protected override void Dispose(bool disposing) {
            owner = null;
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FixCurrent() {
            if(richTextBox1.TextLength > 0) {
                rtfDic[currentPath] = richTextBox1.Rtf;
                txtDic[currentPath] = richTextBox1.Text;
            }
            else {
                rtfDic.Remove(currentPath);
                txtDic.Remove(currentPath);
            }
            SaveDB();
        }

        public void GiveFocus() {
            if(!richTextBox1.Visible) {
                richTextBox1.Visible = true;
            }
            richTextBox1.Focus();
        }

        public void HideMemoForm() {
            try {
                if(fNowShown) {
                    fNowShown = false;
                    Hide();
                    if(richTextBox1.CanUndo) {
                        FixCurrent();
                    }
                }
                foreach(Font font in fontList) {
                    font.Dispose();
                }
                fontList.Clear();
            }
            catch(Exception) {
            }
        }

        private void InitializeComponent() {
            components = new Container();
            richTextBox1 = new RichTextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            cutToolStripMenuItem = new ToolStripMenuItem();
            copyToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            searchToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            colorToolStripMenuItem = new ToolStripMenuItem();
            defaultToolStripMenuItem = new ToolStripMenuItem();
            defaultFontToolStripMenuItem = new ToolStripMenuItem();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            textBox1 = new TextBox();
            button1 = new Button();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            richTextBox1.AcceptsTab = true;
            richTextBox1.AutoWordSelection = true;
            richTextBox1.BorderStyle = BorderStyle.FixedSingle;
            richTextBox1.ContextMenuStrip = contextMenuStrip1;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.HideSelection = false;
            richTextBox1.ImeMode = ImeMode.On;
            richTextBox1.Location = new Point(0, 0);
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox1.Size = new Size(0xe1, 0xe1);
            richTextBox1.TabIndex = 0;
            richTextBox1.TabStop = false;
            richTextBox1.Text = "";
            richTextBox1.LinkClicked += richTextBox1_LinkClicked;
            richTextBox1.KeyDown += richTextBox1_KeyDown;
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, deleteToolStripMenuItem, searchToolStripMenuItem, toolStripSeparator1, colorToolStripMenuItem, defaultToolStripMenuItem, defaultFontToolStripMenuItem });
            contextMenuStrip1.ShowImageMargin = false;
            contextMenuStrip1.Size = new Size(0x80, 0xd0);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            contextMenuStrip1.ItemClicked += contextMenuStrip1_ItemClicked;
            cutToolStripMenuItem.Size = new Size(0x7f, 0x16);
            cutToolStripMenuItem.Text = "Cu&t";
            copyToolStripMenuItem.Size = new Size(0x7f, 0x16);
            copyToolStripMenuItem.Text = "&Copy";
            pasteToolStripMenuItem.Size = new Size(0x7f, 0x16);
            pasteToolStripMenuItem.Text = "&Paste";
            deleteToolStripMenuItem.Size = new Size(0x7f, 0x16);
            deleteToolStripMenuItem.Text = "&Delete";
            searchToolStripMenuItem.Size = new Size(0x7f, 0x16);
            searchToolStripMenuItem.Text = "&Search";
            toolStripSeparator1.Size = new Size(0x7c, 6);
            colorToolStripMenuItem.Size = new Size(0x7f, 0x16);
            colorToolStripMenuItem.Text = "Co&lor";
            defaultToolStripMenuItem.Size = new Size(0x7f, 0x16);
            defaultToolStripMenuItem.Text = "D&efault color";
            defaultFontToolStripMenuItem.Size = new Size(0x7f, 0x16);
            defaultFontToolStripMenuItem.Text = "Default &font";
            listView1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.FullRowSelect = true;
            listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView1.Location = new Point(12, 0x29);
            listView1.MultiSelect = false;
            listView1.ShowGroups = false;
            listView1.ShowItemToolTips = true;
            listView1.Size = new Size(0xc9, 0xac);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.ItemActivate += listView1_ItemActivate;
            columnHeader1.Text = "Path";
            columnHeader1.Width = 0x5d;
            columnHeader2.Text = "Text";
            columnHeader2.Width = 500;
            textBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            textBox1.Location = new Point(12, 14);
            textBox1.Size = new Size(0x83, 20);
            textBox1.TabIndex = 2;
            textBox1.KeyPress += textBox1_KeyPress;
            button1.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            button1.Location = new Point(0x95, 12);
            button1.Size = new Size(0x40, 0x17);
            button1.TabIndex = 3;
            button1.Text = "Search";
            button1.Click += button1_Click;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0xe1, 0xe1);
            Controls.Add(richTextBox1);
            Controls.Add(listView1);
            Controls.Add(button1);
            Controls.Add(textBox1);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MemoForm";
            Opacity = 0.85;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Memo";
            FormClosing += MemoForm_FormClosing;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private void listView1_ItemActivate(object sender, EventArgs e) {
            string text = listView1.SelectedItems[0].Text;
            if(Directory.Exists(text)) {
                owner.OpenDirectory(text);
            }
        }

        private void LoadDB() {
            if(File.Exists(PATH_DAT)) {
                try {
                    using(FileStream stream = new FileStream(PATH_DAT, FileMode.Open)) {
                        BinaryFormatter formatter = new BinaryFormatter();
                        MemoStore store = (MemoStore)formatter.Deserialize(stream);
                        rtfDic = store.Dictionary;
                        Bounds = store.Bounds;
                        if(store.Opacity > 0.2) {
                            Opacity = store.Opacity;
                        }
                        txtDic = store.TxtDictionary;
                    }
                }
                catch(Exception) {
                }
            }
            else {
                Bounds = new Rectangle(Point.Empty, Size);
            }
            if(rtfDic == null) {
                StringComparer currentCultureIgnoreCase = StringComparer.CurrentCultureIgnoreCase;
                rtfDic = new Dictionary<string, string>(currentCultureIgnoreCase);
                txtDic = new Dictionary<string, string>(currentCultureIgnoreCase);
            }
        }

        private void MemoForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(e.CloseReason != CloseReason.WindowsShutDown) {
                e.Cancel = true;
                if(richTextBox1.Visible) {
                    HideMemoForm();
                }
                else {
                    richTextBox1.Visible = true;
                    richTextBox1.Focus();
                }
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e) {
            if(e.Modifiers == Keys.Control) {
                if(e.KeyCode == Keys.Add) {
                    Font selectionFont = richTextBox1.SelectionFont ?? Font;
                    Font item = new Font(Font.FontFamily, selectionFont.Size + 0.75f);
                    richTextBox1.SelectionFont = item;
                    fontList.Add(item);
                }
                else if(e.KeyCode == Keys.Subtract) {
                    Font font = richTextBox1.SelectionFont ?? Font;
                    if(font.Size > 6f) {
                        Font font4 = new Font(Font.FontFamily, font.Size - 0.75f);
                        richTextBox1.SelectionFont = font4;
                        fontList.Add(font4);
                    }
                }
                else if(e.KeyCode == Keys.F) {
                    if(richTextBox1.CanUndo) {
                        FixCurrent();
                    }
                    IntPtr hWnd = PInvoke.SendMessage(listView1.Handle, 0x104e, IntPtr.Zero, IntPtr.Zero);
                    if(hWnd != IntPtr.Zero) {
                        PInvoke.SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, 0x13);
                    }
                    CreateMemoList();
                    richTextBox1.Visible = false;
                    Refresh();
                    textBox1.Focus();
                }
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e) {
            Process.Start(e.LinkText);
        }

        private void SaveDB() {
            MemoStore graph = new MemoStore();
            graph.Bounds = Bounds;
            graph.Dictionary = rtfDic;
            graph.TxtDictionary = txtDic;
            graph.Opacity = Opacity;
            if(!Directory.Exists(Path.GetDirectoryName(PATH_DAT))) {
                Directory.CreateDirectory(Path.GetDirectoryName(PATH_DAT));
            }
            BinaryFormatter formatter = new BinaryFormatter();
            using(FileStream stream = new FileStream(PATH_DAT, FileMode.Create)) {
                formatter.Serialize(stream, graph);
            }
        }

        public void ShowMemoForm(string path) {
            try {
                if(!fFirstLoadComplete) {
                    LoadDB();
                    toolStripTrackBar.Value = (int)(Opacity * 255.0);
                    toolStripTrackBar.BackColor = ProfessionalColors.MenuItemPressedGradientBegin;
                    fFirstLoadComplete = true;
                }
                if(fNowShown && richTextBox1.CanUndo) {
                    FixCurrent();
                }
                currentPath = path;
                if(path.Length > 3) {
                    Text = Path.GetFileName(path);
                }
                else {
                    Text = path;
                }
                richTextBox1.SuspendLayout();
                richTextBox1.Clear();
                if(rtfDic.ContainsKey(currentPath)) {
                    richTextBox1.Rtf = rtfDic[currentPath];
                }
                richTextBox1.Visible = true;
                richTextBox1.ResumeLayout();
                if(!fNowShown) {
                    Show();
                    PInvoke.SetWindowPos(Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x53);
                    fNowShown = true;
                }
            }
            catch(Exception) {
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                e.Handled = true;
                button1.PerformClick();
            }
        }

        private void toolStripTrackBar_ValueChanged(object sender, EventArgs e) {
            Opacity = (toolStripTrackBar.Value) / 255.0;
        }

        protected override bool ShowWithoutActivation {
            get {
                return true;
            }
        }
    }
}

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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    [Serializable]
    internal sealed class QTabItem {
        // Static stuff
        internal const string SEPARATOR_COMMENT = ": ";
        internal const string SEPARATOR_SUBTEXT = "@ ";
        private static StringFormat sfMeasure;
        private static Font font;
        private static Font fontSubText;
        private static RectangleF rctMeasure = new RectangleF(0f, 0f, 512f, 50f);

        public static Font TabFont {
            set {
                if(font != null) {
                    font.Dispose();
                }
                font = new Font(value, FontStyle.Regular);
                if(fontSubText != null) {
                    fontSubText.Dispose();
                }
                float sizeInPoints = font.SizeInPoints;
                fontSubText = new Font(value.FontFamily, (sizeInPoints > 8.25f) ? (sizeInPoints - 0.75f) : sizeInPoints, FontStyle.Regular);
            }
        }

        // Events
        [field: NonSerialized]
        public event EventHandler Closed;

        // Fields
        private string currentPath;
        private Dictionary<string, string> dicFocusedItemName;
        private Dictionary<string, Address[]> dicSelectedItems;
        private bool fNowSlowTip;
        private string imageKey = string.Empty;
        private Stack<LogData> stckHistoryBackward;
        private Stack<LogData> stckHistoryForward;
        private bool tabLocked;
        private string shellToolTip;
        private string titleText;
        [NonSerialized]
        private QTabControl Owner;
        
        // Auto-props
        public List<LogData> Branches { get; private set; }
        public byte[] CurrentIDL { get; set; }
        public string Comment { get; set; }
        public Edges Edge { get; set; }
        public int Row { get; set; }
        public SizeF SubTitleTextSize { get; private set; }
        public Rectangle TabBounds { get; set; }
        public SizeF TitleTextSize { get; private set; }
        public string ToolTipText { get; set; }
        public bool Underline { get; set; }

        // Props
        public string ImageKey {
            get {
                return imageKey;
            }
            set {
                if(Owner != null && Owner.DrawFolderImage && !string.IsNullOrEmpty(value)) {
                    imageKey = QTUtility.GetImageKey(value, null);
                }
                else {
                    imageKey = value;
                }
            }
        }

        public int Index {
            get { return Owner.TabPages.IndexOf(this);  }
        }

        public bool TabLocked {
            get {
                return tabLocked;
            }
            set {
                tabLocked = value;
                RefreshRectangle();
                if(Owner != null) {
                    Owner.Refresh();
                }
            }
        }

        public string Text {
            get {
                return titleText;
            }
            set {
                titleText = value;
                RefreshRectangle();
                if(Owner != null) {
                    Owner.Refresh();
                }
            }
        }

        public string CurrentPath {
            get {
                return currentPath;
            }
            set {
                if(value == null) {
                    currentPath = string.Empty;
                    ImageKey = string.Empty;
                }
                else {
                    currentPath = value;
                    ImageKey = value;
                }
            }
        }

        public int HistoryCount_Back {
            get {
                return stckHistoryBackward.Count;
            }
        }

        public int HistoryCount_Forward {
            get {
                return stckHistoryForward.Count;
            }
        }

        public string PathInitial {
            get {
                if((currentPath != null) && (currentPath.Length > 3)) {
                    char ch = currentPath[0];
                    if((('A' <= ch) && (ch <= 'Z')) || (('a' <= ch) && (ch <= 'z'))) {
                        return ch.ToString();
                    }
                }
                return string.Empty;
            }
        }

        public string ShellToolTip {
            get {
                bool fAllowSlow = Control.ModifierKeys == Keys.Shift;
                if(fNowSlowTip ^ fAllowSlow) {
                    shellToolTip = null;
                }
                if(shellToolTip == null) {
                    fNowSlowTip = fAllowSlow;
                    using(IDLWrapper wrapper = new IDLWrapper(CurrentIDL)) {
                        if(wrapper.Available) {
                            shellToolTip = ShellMethods.GetShellInfoTipText(wrapper.PIDL, fAllowSlow);
                        }
                    }
                    if(((shellToolTip == null) && !string.IsNullOrEmpty(currentPath)) && !currentPath.StartsWith(@"\\")) {
                        shellToolTip = ShellMethods.GetShellInfoTipText(currentPath, fAllowSlow);
                    }
                }
                return shellToolTip;
            }
            set {
                fNowSlowTip = false;
                shellToolTip = value;
            }
        }

        public QTabItem(string title, string path, QTabControl parent) {
            Owner = parent;
            Comment = string.Empty;
            stckHistoryForward = new Stack<LogData>();
            stckHistoryBackward = new Stack<LogData>();
            dicSelectedItems = new Dictionary<string, Address[]>();
            dicFocusedItemName = new Dictionary<string, string>();
            Branches = new List<LogData>();
            CurrentPath = path;
            ToolTipText = string.Empty;
            titleText = title;
            if(font == null) {
                font = new Font(parent.Font, FontStyle.Bold);
            }
            if(fontSubText == null) {
                float sizeInPoints = font.SizeInPoints;
                fontSubText = new Font(parent.Font.FontFamily, (sizeInPoints > 8.25f) ? (sizeInPoints - 0.75f) : sizeInPoints, FontStyle.Bold);
            }
            if(sfMeasure == null) {
                sfMeasure = new StringFormat();
                sfMeasure.FormatFlags |= StringFormatFlags.NoWrap;
            }
            if(title.Length > 0) {
                RefreshRectangle();
            }
            else {
                TabBounds = new Rectangle(TabBounds.X, TabBounds.Y, 100, 0x18);
            }
        }

        public static void CheckSubTexts(QTabControl tabControl) {
            if(!tabControl.AutoSubText) return;
            bool needsRefresh = false;
            char[] separator = new char[] { Path.DirectorySeparatorChar };
            Dictionary<string, List<QTabItem>> commonTextTabs = new Dictionary<string, List<QTabItem>>();
            foreach(QTabItem item in tabControl.TabPages) {
                if(item.CurrentPath.StartsWith("::")) continue;
                string text = item.Text.ToLower();
                if(commonTextTabs.ContainsKey(text)) {
                    commonTextTabs[text].Add(item);
                }
                else {
                    commonTextTabs[text] = new List<QTabItem> { item };
                }
            }
            foreach(List<QTabItem> tabs in commonTextTabs.Values) {
                if(tabs.Count > 1) {
                    if(tabs.All(tab => tab.CurrentPath == tabs[0].CurrentPath)) {
                        foreach(QTabItem tab in tabs.Where(item => item.Comment.Length > 0)) {
                            tab.Comment = string.Empty;
                            tab.RefreshRectangle();
                            needsRefresh = true;
                        }
                    }
                    else {
                        List<string[]> pathArrays = tabs.Select(item => item.CurrentPath
                                .Split(separator).Reverse().Skip(1).ToArray()).ToList();
                        for(int i = 0; i < tabs.Count; i++) {
                            string comment = pathArrays[i].FirstOrDefault(str => !pathArrays.Where(
                                    (path, j) => i != j && path.Contains(str)).Any()) ?? tabs[i].currentPath;
                            if(comment.Length == 2 && comment[1] == ':') {
                                comment += @"\";
                            }
                            if(tabs[i].Comment != comment) {
                                tabs[i].Comment = comment;
                                tabs[i].RefreshRectangle();
                                needsRefresh = true;
                            }
                        }
                    }
                }
                else if(tabs[0].Comment.Length > 0) {
                    needsRefresh = true;
                    tabs[0].Comment = string.Empty;
                    tabs[0].RefreshRectangle();
                }
            }
            if(needsRefresh) {
                tabControl.Refresh();
            }
        }

        public QTabItem Clone(bool fAll = false) {
            QTabItem item = new QTabItem(Text, currentPath, Owner) {
                TabBounds = TabBounds,
                Comment = Comment,
                CurrentIDL = CurrentIDL,
                ToolTipText = ToolTipText,
                shellToolTip = shellToolTip,
                tabLocked = fAll && tabLocked,
                stckHistoryForward = new Stack<LogData>(stckHistoryForward.Reverse()),
                stckHistoryBackward = new Stack<LogData>(stckHistoryBackward.Reverse()),
                dicFocusedItemName = new Dictionary<string, string>(dicFocusedItemName),
                Branches = new List<LogData>(Branches.ToArray()),
                dicSelectedItems = dicSelectedItems.Keys
                        .ToDictionary(str => str, str => dicSelectedItems[str])
            };
            return item;
        }

        public string[] GetHistoryBack() {
            return stckHistoryBackward.Select(data => data.Path).ToArray();
        }

        public string[] GetHistoryForward() {
            return stckHistoryForward.Select(data => data.Path).ToArray();
        }

        public int GetLogHash(bool back, int index) {
            LogData[] dataArray;
            if(back) {
                dataArray = stckHistoryBackward.ToArray();
            }
            else {
                dataArray = stckHistoryForward.ToArray();
            }
            if((index > -1) && (index < dataArray.Length)) {
                return dataArray[index].Hash;
            }
            return -1;
        }

        public IEnumerable<LogData> GetLogs(bool fBack) {
            List<LogData> list = new List<LogData>(fBack ? stckHistoryBackward : stckHistoryForward);
            if(fBack && (list.Count > 0)) {
                list.RemoveAt(0);
            }
            return list;
        }

        public Address[] GetSelectedItemsAt(string path, out string focused) {
            Address[] addressArray;
            dicSelectedItems.TryGetValue(path, out addressArray);
            dicFocusedItemName.TryGetValue(path, out focused);
            return addressArray;
        }

        private static SizeF GetTextSize(string str, Graphics g, bool fTitle) {
            SizeF empty;
            CharacterRange[] ranges = new CharacterRange[] { new CharacterRange(0, str.Length) };
            sfMeasure.SetMeasurableCharacterRanges(ranges);
            Region[] regionArray = g.MeasureCharacterRanges(str, fTitle ? font : fontSubText, rctMeasure, sfMeasure);
            using(regionArray[0]) {
                empty = regionArray[0].GetBounds(g).Size;
                empty.Width += 6f;
            }
            return empty;
        }

        public LogData GoBackward() {
            LogData data = new LogData();
            if(stckHistoryBackward.Count > 1) {
                stckHistoryForward.Push(stckHistoryBackward.Pop());
                data = stckHistoryBackward.Peek();
                CurrentPath = data.Path;
            }
            return data;
        }

        public LogData GoForward() {
            LogData data = new LogData();
            if(stckHistoryForward.Count != 0) {
                stckHistoryBackward.Push(stckHistoryForward.Pop());
                data = stckHistoryBackward.Peek();
                CurrentPath = data.Path;
            }
            return data;
        }

        public void NavigatedTo(Address address, int hash, bool autoNav) {
            NavigatedTo(address.Path, address.ITEMIDLIST, hash, autoNav);
        }

        public void NavigatedTo(string path, byte[] idl, int hash, bool autoNav) {
            if((idl == null) || (idl.Length == 0)) {
                idl = ShellMethods.GetIDLData(path);
            }
            if(autoNav && stckHistoryBackward.Count > 0 && stckHistoryBackward.Peek().AutoNav) {
                stckHistoryBackward.Pop();
            }
            stckHistoryBackward.Push(new LogData(path, idl, hash, autoNav));
            Branches.AddRange(stckHistoryForward.Except(Branches));
            foreach(LogData data2 in stckHistoryBackward) {
                Branches.Remove(data2);
            }
            stckHistoryForward.Clear();
            CurrentPath = path;
            CurrentIDL = idl;
        }

        public void OnClose() {
            if(Closed != null) {
                Closed(null, EventArgs.Empty);
                Closed = null;
            }
            Owner = null;
        }

        public void RefreshRectangle() {
            using(Graphics graphics = Owner.CreateGraphics()) {
                float num = 6f;
                if(Owner.DrawFolderImage) {
                    num = 26f;
                }
                else if(tabLocked) {
                    num = 13f;
                }
                if((Owner.EnableCloseButton && !Owner.TabCloseButtonOnHover) && !Owner.TabCloseButtonOnAlt) {
                    num += 17f;
                }
                TitleTextSize = GetTextSize(titleText, graphics, true);
                SubTitleTextSize = SizeF.Empty;
                if(Comment.Length > 0) {
                    SubTitleTextSize = GetTextSize("@ " + Comment, graphics, false);
                    num += SubTitleTextSize.Width + 3f;
                }
                else {
                    num++;
                }
                int width = (int)(TitleTextSize.Width + num);
                if(Owner.OncePainted && (width > Owner.Width)) {
                    if(Owner.Width > 40) {
                        width = Owner.Width - 40;
                    }
                    else {
                        width = 0x20;
                    }
                }
                TabBounds = new Rectangle(TabBounds.X, TabBounds.Y, width, (int)TitleTextSize.Height);
            }
        }

        public void ResetOwner(QTabControl owner) {
            Owner = owner;
            owner.TabPages.Add(this);
        }

        public void SetSelectedItemsAt(string path, Address[] names, string focused) {
            dicSelectedItems[path] = names;
            dicFocusedItemName[path] = focused;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct LogData : IEquatable<LogData> {
        public string Path;
        public byte[] IDL;
        public int Hash;
        public bool AutoNav;
        public LogData(string path, byte[] idl, int hash, bool autoNav) {
            Path = path;
            IDL = idl;
            Hash = hash;
            AutoNav = autoNav;
        }

        public bool Equals(LogData other) {
            return Path.PathEquals(other.Path) &&
                    (System.IO.Path.IsPathRooted(Path) || (Hash == other.Hash));
        }
    }
}

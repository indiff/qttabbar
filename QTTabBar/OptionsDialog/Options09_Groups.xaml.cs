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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Image = System.Drawing.Image;
using Keys = System.Windows.Forms.Keys;

namespace QTTabBarLib {
    internal partial class Options09_Groups : OptionsDialogTab, IHotkeyContainer {
        private ParentedCollection<GroupEntry> CurrentGroups;
        public event NewHotkeyRequestedHandler NewHotkeyRequested;

        public Options09_Groups() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            tvwGroups.ItemsSource = CurrentGroups = new ParentedCollection<GroupEntry>(null,
                    GroupsManager.Groups.Select(g => new GroupEntry(
                    g.Name, g.ShortcutKey, g.Startup, g.Paths.Select(p => new FolderEntry(p)))));
        }

        public override void ResetConfig() {
            // Should we do anything here?
        }

        public override void CommitConfig() {
            GroupsManager.Groups = new List<Group>(
                    CurrentGroups.Select(g => new Group(
                    g.Name, g.ShortcutKey, g.Startup, g.Folders.Select(f => f.Path).ToList())));

        }

        public IEnumerable<IHotkeyEntry> GetHotkeyEntries() {
            return CurrentGroups.Cast<IHotkeyEntry>();
        }

        private void btnGroupsAddGroup_Click(object sender, RoutedEventArgs e) {
            GroupEntry item = new GroupEntry(QTUtility.TextResourcesDic["Options_Page09_Groups"][6]);
            tvwGroups.Focus();
            IList col = (IList)tvwGroups.ItemsSource;
            object sel = tvwGroups.SelectedItem;
            int idx = sel == null
                    ? tvwGroups.Items.Count
                    : CurrentGroups.IndexOf(sel as GroupEntry ?? (GroupEntry)((FolderEntry)sel).ParentItem) + 1;
            col.Insert(idx, item);
            item.IsSelected = true;
            item.IsEditing = true;
        }

        private void btnGroupsMoveNodeUpDown_Click(object sender, RoutedEventArgs e) {
            UpDownOnTreeView(tvwGroups, sender == btnGroupsMoveNodeUp, false);
        }

        private void btnGroupsAddFolder_Click(object sender, RoutedEventArgs e) {
            GroupEntry group;
            int index;
            bool editGroup;
            if(tvwGroups.Items.Count == 0) {
                group = new GroupEntry(QTUtility.TextResourcesDic["Options_Page09_Groups"][6]);
                CurrentGroups.Add(group);
                group.IsSelected = true;
                index = 0;
                editGroup = true;
            }
            else {
                object sel = tvwGroups.SelectedItem;
                if(sel == null) return;
                if(sel is FolderEntry) {
                    FolderEntry entry = (FolderEntry)sel;
                    group = (GroupEntry)entry.ParentItem;
                    index = group.Folders.IndexOf(entry) + 1;
                }
                else {
                    group = (GroupEntry)sel;
                    index = group.Folders.Count;
                }
                editGroup = false;
            }

            FolderBrowserDialogEx dlg = new FolderBrowserDialogEx();
            if(dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            FolderEntry folder = new FolderEntry(dlg.SelectedPath);
            group.Folders.Insert(index, folder);
            group.IsExpanded = true;
            
            if(editGroup) {
                group.IsSelected = true;
                group.IsEditing = true;
            }
            else {
                folder.IsSelected = true;   
            }
        }

        private void btnGroupsRemoveNode_Click(object sender, RoutedEventArgs e) {
            ITreeViewItem sel = tvwGroups.SelectedItem as ITreeViewItem;
            if(sel == null) return;
            IList col = sel.ParentList;
            int index = col.IndexOf(sel);
            col.RemoveAt(index);
            if(col.Count == 0) return;
            if(index == col.Count) --index;
            ((ITreeViewItem)col[index]).IsSelected = true;
        }

        private void tvwGroups_MouseDown(object sender, MouseButtonEventArgs e) {
            IEditableEntry entry = ((TreeView)sender).SelectedItem as IEditableEntry;
            if(entry != null) entry.IsEditing = false;
        }

        private void tvwGroups_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(NewHotkeyRequested == null) return;
            GroupEntry entry = tvwGroups.SelectedItem as GroupEntry;
            if(entry == null) return;
            Keys newKey;
            if(!NewHotkeyRequested(e, entry.ShortcutKey, out newKey)) return;
            entry.ShortcutKey = newKey;
            e.Handled = true;
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class FolderEntry : INotifyPropertyChanged, IEditableEntry, ITreeViewItem {
            public event PropertyChangedEventHandler PropertyChanged;
            public IList ParentList { get; set; }
            public ITreeViewItem ParentItem { get; set; }
            public string Path { get; set; }
            public bool IsEditing { get; set; }
            public bool IsSelected { get; set; }
            public bool IsExpanded { get; set; }
            public IList ChildrenList { get { return null; } }

            public string DisplayText {
                get {
                    return QTUtility2.MakePathDisplayText(Path, true);
                }
            }
            public Image Icon {
                get {
                    return QTUtility.GetIcon(Path, false).ToBitmap();
                }
            }
            public bool IsVirtualFolder {
                get {
                    return Path.StartsWith("::");
                }
            }

            public FolderEntry(string path) {
                Path = path;
            }

            public FolderEntry() {
            }
        }

        private class GroupEntry : INotifyPropertyChanged, IEditableEntry, ITreeViewItem, IHotkeyEntry {
            public event PropertyChangedEventHandler PropertyChanged;
            public IList ParentList { get; set; }
            public ITreeViewItem ParentItem { get; set; }
            public string Name { get; set; }
            public Image Icon { get; private set; }
            public ParentedCollection<FolderEntry> Folders { get; private set; }
            public bool Startup { get; set; }
            public Keys ShortcutKey { get; set; }

            public string KeyActionText {
                get {
                    string GroupPrefix = QTUtility.TextResourcesDic["Options_Page09_Groups"][7];
                    return string.Format(GroupPrefix, Name);
                }
            }

            public string HotkeyString {
                get { return QTUtility2.MakeKeyString(ShortcutKey); }
            }
            public bool IsEditing { get; set; }
            public bool IsSelected { get; set; }
            public bool IsExpanded { get; set; }
            public IList ChildrenList { get { return Folders; }}

            private void Folders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                if(e.OldItems != null) {
                    foreach(FolderEntry child in e.OldItems) {
                        child.PropertyChanged -= FolderEntry_PropertyChanged;
                    }
                }
                if(e.NewItems != null) {
                    foreach(FolderEntry child in e.NewItems) {
                        child.PropertyChanged += FolderEntry_PropertyChanged;
                    }
                }
                RefreshIcon();
            }

            private void FolderEntry_PropertyChanged(object sender, PropertyChangedEventArgs e) {
                if(Folders.Count > 0 && sender == Folders.First()) {
                    RefreshIcon();
                }
            }

            private void RefreshIcon() {
                Icon = Folders.Count == 0 ? QTUtility.ImageListGlobal.Images["folder"] : Folders.First().Icon;
            }

            public GroupEntry(string name, Keys shortcutKey, bool startup, IEnumerable<FolderEntry> folders) {
                Name = name;
                Startup = startup;
                ShortcutKey = shortcutKey;
                Folders = new ParentedCollection<FolderEntry>(this, folders);
                Folders.CollectionChanged += Folders_CollectionChanged;
                RefreshIcon();
            }

            public GroupEntry(string name) {
                Name = name;
                Folders = new ParentedCollection<FolderEntry>(this);
                Folders.CollectionChanged += Folders_CollectionChanged;
                RefreshIcon();
            }

            public GroupEntry() {
                Folders.CollectionChanged += Folders_CollectionChanged;
                RefreshIcon();
            }
        }

        #endregion
    }
}

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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Image = System.Drawing.Image;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Keys = System.Windows.Forms.Keys;

namespace QTTabBarLib {
    internal partial class Options10_Apps : OptionsDialogTab, IHotkeyContainer {
        private ParentedCollection<AppEntry> CurrentApps;
        public event NewHotkeyRequestedHandler NewHotkeyRequested;

        public Options10_Apps() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            tvwApps.ItemsSource = CurrentApps = new ParentedCollection<AppEntry>(null,
                    AppsManager.BuildNestedStructure(
                    app => new AppEntry(app),
                    (folderName, children) => new AppEntry(folderName, children)));
        }

        public override void ResetConfig() {
            // Should we do anything here?
        }

        public override void CommitConfig() {
            AppsManager.SetUserAppsFromNestedStructure(
                    CurrentApps,
                    entry => entry.IsFolder 
                        ? new UserApp(entry.Name) 
                        : new UserApp(entry.Name, entry.Path, entry.Args, entry.WorkingDir, entry.ShortcutKey),
                    entry => entry.Children);
        }

        private static IEnumerable<AppEntry> FlattenAppList(IEnumerable<AppEntry> AppRoot) {
            foreach(AppEntry app in AppRoot) {
                if(app.IsFolder) {
                    foreach(AppEntry child in FlattenAppList(app.Children)) {
                        yield return child;
                    }
                }
                else {
                    yield return app;
                }
            }
        }

        public IEnumerable<IHotkeyEntry> GetHotkeyEntries() {
            return FlattenAppList(CurrentApps).Cast<IHotkeyEntry>();
        }

        private void tvwApps_MouseDown(object sender, MouseButtonEventArgs e) {
            IEditableEntry entry = ((TreeView)sender).SelectedItem as IEditableEntry;
            if(entry != null) entry.IsEditing = false;
        }

        private void btnAddApp_Click(object sender, RoutedEventArgs e) {
            string path;
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                if(System.Windows.Forms.DialogResult.OK != ofd.ShowDialog()) return;
                path = ofd.FileName;
            }
            AppEntry sel = tvwApps.SelectedItem as AppEntry;
            IList list = sel == null ? CurrentApps : sel.IsFolder ? sel.Children : sel.ParentList;
            int idx = sel == null ? 0 : list.IndexOf(sel) + 1;
            AppEntry entry = new AppEntry(System.IO.Path.GetFileName(path), path);
            list.Insert(idx, entry);
            if(sel != null && sel.IsFolder) sel.IsExpanded = true;
            tvwApps.Focus();
            entry.IsSelected = true;
        }

        private void btnAddAppFolder_Click(object sender, RoutedEventArgs e) {
            AppEntry sel = tvwApps.SelectedItem as AppEntry;
            IList list = sel == null ? CurrentApps : sel.IsFolder ? sel.Children : sel.ParentList;
            int idx = sel == null ? 0 : list.IndexOf(sel) + 1;
            AppEntry entry = new AppEntry(QTUtility.TextResourcesDic["Options_Page10_Apps"][14], new AppEntry[0]);
            list.Insert(idx, entry);
            if(sel != null && sel.IsFolder) sel.IsExpanded = true;
            tvwApps.Focus();
            entry.IsSelected = true;
            entry.IsEditing = true;
        }

        private void btnRemoveApp_Click(object sender, RoutedEventArgs e) {
            AppEntry sel = tvwApps.SelectedItem as AppEntry;
            if(sel == null) return;
            if(sel.IsFolder && sel.Children.Count > 0) {
                var resp = MessageBox.Show(
                        QTUtility.TextResourcesDic["Options_Page10_Apps"][15],
                        QTUtility.TextResourcesDic["OptionsDialog"][3],
                        MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if(resp == MessageBoxResult.Cancel) return;
            }
            IList list = sel.ParentList;
            int index = list.IndexOf(sel);
            list.RemoveAt(index);
            if(list.Count == 0) return;
            if(index == list.Count) --index;
            ((ITreeViewItem)list[index]).IsSelected = true;
        }

        private void btnAppsMoveNodeUpDown_Click(object sender, RoutedEventArgs e) {
            UpDownOnTreeView(tvwApps, sender == btnAppsMoveNodeUp, true);
        }

        private void tvwApps_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(NewHotkeyRequested == null) return;
            AppEntry entry = tvwApps.SelectedItem as AppEntry;
            if(entry == null || entry.IsFolder) return;
            Keys newKey;
            if(!NewHotkeyRequested(e, entry.ShortcutKey, out newKey)) return;
            entry.ShortcutKey = newKey;
            e.Handled = true;
        }

        private void btnVars_Click(object sender, RoutedEventArgs e) {
            var button = ((Button)sender);
            ContextMenu menu = button.ContextMenu;
            menu.PlacementTarget = button;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void miVars_Click(object sender, RoutedEventArgs e) {
            MenuItem mi = (MenuItem)sender;
            Button source = (Button)mi.DataContext;
            TextBox textbox = source == btnArgVars ? txtAppArgs : txtAppDir;
            string var = (string)mi.Tag;
            int caret = textbox.CaretIndex;
            textbox.Text = textbox.Text.Insert(caret, var);
            textbox.CaretIndex = caret + var.Length;
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class AppEntry : INotifyPropertyChanged, IEditableEntry, ITreeViewItem, IHotkeyEntry {
            public event PropertyChangedEventHandler PropertyChanged;
            public IList ParentList { get; set; }
            public ITreeViewItem ParentItem { get; set; }
            public ParentedCollection<AppEntry> Children { get; set; }
            public bool IsFolder { get { return Children != null; } }
            public bool IsEditing { get; set; }
            public bool IsSelected { get; set; }
            public bool IsExpanded { get; set; }
            public IList ChildrenList { get { return Children; } }

            public string Name { get; set; }
            public string Path { get; set; }
            public string Args { get; set; }
            public string WorkingDir { get; set; }
            public Keys ShortcutKey { get; set; }
            public string KeyActionText {
                get {
                    string AppPrefix = QTUtility.TextResourcesDic["Options_Page10_Apps"][17];
                    return string.Format(AppPrefix, Name);
                }
            }

            public string HotkeyString {
                get { return QTUtility2.MakeKeyString(ShortcutKey); }
            }

            public Image Icon {
                get {
                    return IsFolder
                      ? QTUtility.ImageListGlobal.Images["folder"]
                      : QTUtility.GetIcon(Path, false).ToBitmap();
                }
            }

            public AppEntry(string folderName, IEnumerable<AppEntry> children) {
                Name = folderName;
                Children = new ParentedCollection<AppEntry>(this, children);
            }

            public AppEntry(string name, string path) {
                Path = path;
                Name = name;
            }

            public AppEntry(UserApp app) {
                Path = app.Path;
                Name = app.Name;
                Args = app.Args;
                WorkingDir = app.WorkingDir;
                ShortcutKey = app.ShortcutKey;
            }
        }
        #endregion
    }
}

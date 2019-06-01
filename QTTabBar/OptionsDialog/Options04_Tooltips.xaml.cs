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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QTTabBarLib.Interop;
using Image = System.Drawing.Image;

namespace QTTabBarLib {
    internal partial class Options04_Tooltips : OptionsDialogTab {
        private ObservableCollection<FileTypeEntry> TextFileTypes;
        private ObservableCollection<FileTypeEntry> MediaFileTypes;

        public Options04_Tooltips() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            lstTextFileTypes.ItemsSource = TextFileTypes = new ObservableCollection<FileTypeEntry>(
                    WorkingConfig.tips.TextExt.Select(ext => new FileTypeEntry(this, ext)));
            lstMediaFileTypes.ItemsSource = MediaFileTypes = new ObservableCollection<FileTypeEntry>(
                    WorkingConfig.tips.ImageExt.Select(ext => new FileTypeEntry(this, ext)));
            lstTextFileTypes.ScrollIntoView(TextFileTypes.First());
            lstMediaFileTypes.ScrollIntoView(MediaFileTypes.First());
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.tips = new Config._Tips();
            InitializeConfig();
        }

        public override void CommitConfig() {
            WorkingConfig.tips.TextExt = TextFileTypes.Select(entry => entry.DotExtension).ToList();
            WorkingConfig.tips.ImageExt = MediaFileTypes.Select(entry => entry.DotExtension).ToList();
        }

        private void AddNewFileType(ListBox control) {
            ICollection<FileTypeEntry> source = (ICollection<FileTypeEntry>)control.ItemsSource;
            FileTypeEntry item = new FileTypeEntry(this, "");
            source.Add(item);
            control.SelectedItem = item;
            control.ScrollIntoView(item);
            control.Focus();
            item.IsEditing = true;
        }

        private static void RemoveSelectedFileType(ListBox control) {
            var list = (ObservableCollection<FileTypeEntry>)control.ItemsSource;
            int idx = control.SelectedIndex;
            if(idx == -1) return;
            foreach(FileTypeEntry item in new ArrayList(control.SelectedItems)) {
                list.Remove(item);
            }
            control.Focus();
            control.SelectedIndex = Math.Min(idx, list.Count - 1);
        }

        private void btnAddTextFileTypes_Click(object sender, RoutedEventArgs e) {
            AddNewFileType(lstTextFileTypes);
        }

        private void btnAddMediaFileTypes_Click(object sender, RoutedEventArgs e) {
            AddNewFileType(lstMediaFileTypes);
        }

        private void btnRemoveTextFileTypes_Click(object sender, RoutedEventArgs e) {
            RemoveSelectedFileType(lstTextFileTypes);
        }

        private void btnRemoveMediaFileTypes_Click(object sender, RoutedEventArgs e) {
            RemoveSelectedFileType(lstMediaFileTypes);
        }

        private void btnResetTextFileTypes_Click(object sender, RoutedEventArgs e) {
            lstTextFileTypes.ItemsSource = TextFileTypes = new ObservableCollection<FileTypeEntry>(
                    new Config._Tips().TextExt.Select(ext => new FileTypeEntry(this, ext)));
            lstTextFileTypes.ScrollIntoView(TextFileTypes.First());
        }

        private void btnResetMediaFileTypes_Click(object sender, RoutedEventArgs e) {
            lstMediaFileTypes.ItemsSource = MediaFileTypes = new ObservableCollection<FileTypeEntry>(
                    new Config._Tips().ImageExt.Select(ext => new FileTypeEntry(this, ext)));
            lstMediaFileTypes.ScrollIntoView(MediaFileTypes.First());
        }

        private void lstTextFileTypes_OnKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete) RemoveSelectedFileType(lstTextFileTypes);
        }

        private void lstMediaFileTypes_OnKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete) RemoveSelectedFileType(lstMediaFileTypes);
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e) {
            FrameworkElement item = (FrameworkElement)sender;
            FrameworkElement parent = (FrameworkElement)item.Tag;

            if (parent != null && parent.IsFocused) {
                item.Focus();
            }
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class FileTypeEntry : INotifyPropertyChanged, IEditableEntry {
            public event PropertyChangedEventHandler PropertyChanged;
            private Options04_Tooltips parent;

            private bool _IsEditing;
            public bool IsEditing {
                get { return _IsEditing; }
                set {
                    _IsEditing = value;
                    if(!_IsEditing && string.IsNullOrEmpty(Extension)) {
                        parent.TextFileTypes.Remove(this);
                        parent.MediaFileTypes.Remove(this);
                    }
                }
            }

            public bool IsSelected { get; set; }
            public string Extension { get; set; }

            public string DotExtension {
                get {
                    return "." + Extension;
                }
                set {
                    if(!value.StartsWith(".")) {
                        throw new ArgumentException();
                    }
                    Extension = value.Substring(1);
                }
            }
            public string FriendlyName {
                get {
                    // PENDING: Instead of something like GetFileType.

                    SHFILEINFO psfi = new SHFILEINFO();
                    int sz = System.Runtime.InteropServices.Marshal.SizeOf(psfi);
                    // SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES
                    if(IntPtr.Zero == PInvoke.SHGetFileInfo("*" + DotExtension, 0x80, ref psfi, sz, 0x400 | 0x10)) {
                        return null;
                    }
                    else if(string.IsNullOrEmpty(psfi.szTypeName)) {
                        return null;
                    }
                    return psfi.szTypeName;
                }
            }
            public Image Icon {
                get {
                    return QTUtility.GetIcon(DotExtension, true).ToBitmap();
                }
            }
            public FileTypeEntry(Options04_Tooltips parent, string extension) {
                this.parent = parent;
                if(!extension.StartsWith(".")) {
                    extension = "." + extension;
                }
                DotExtension = extension;
            }
        }

        #endregion
    }
}

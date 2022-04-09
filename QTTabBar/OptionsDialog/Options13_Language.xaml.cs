//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using DialogResult = System.Windows.Forms.DialogResult;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace QTTabBarLib {
    internal partial class Options13_Language : OptionsDialogTab {
        private ObservableCollection<LangEntry> LangItems = new ObservableCollection<LangEntry>();
        private ObservableCollection<string> PluginFiles;
        public Options13_Language() {
            InitializeComponent();

            // This goes in the constructor rather than InitializeConfig because it should
            // not be affected by Apply.

            try {
                string[] metatags = {
                    "Author",
                    "Language",
                    "Country",
                    "Version_QTTabBar",
                    "Version_LangFile",
                    "DateModified"
                };
                LangItems.Add(new LangEntry("Author", -1));
                LangItems.Add(new LangEntry("Language", -1));
                LangItems.Add(new LangEntry("Country", -1));
                foreach(var kv in QTUtility.TextResourcesDic.OrderBy(kv => kv.Key)) {
                    if(metatags.Contains(kv.Key)) continue;
                    for(int i = 0; i < kv.Value.Length; i++) {
                        LangItems.Add(new LangEntry(kv.Key, i));
                    }
                }

                ICollectionView view = CollectionViewSource.GetDefaultView(LangItems);
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("Location");
                view.GroupDescriptions.Add(groupDescription);
                lvwLangEditor.ItemsSource = view;
             }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception, "Options13_Language ");

            }           
        }

        public override void InitializeConfig() {
            try {
                lstPluginFiles.ItemsSource = PluginFiles = 
                        new ObservableCollection<string>(WorkingConfig.lang.PluginLangFiles);
              }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception, "Options13_Language InitializeConfig");

            } 
       }

        public override void ResetConfig() {
            InitializeConfig();
        }

        public override void CommitConfig() {
            try {
                 WorkingConfig.lang.PluginLangFiles = PluginFiles.ToArray();
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception, "Options13_Language InitializeConfig");

            }           
       }

        private void btnPluginAdd_Click(object sender, RoutedEventArgs e) {
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = QTUtility.TextResourcesDic["FileFilters"][1] + "|*.xml";
                ofd.RestoreDirectory = true;
                if(DialogResult.OK != ofd.ShowDialog()) return;
                var dict = QTUtility.ReadLanguageFile(ofd.FileName);
                if(dict != null) {
                    PluginFiles.Add(ofd.FileName);
                }
            }
        }

        private void btnPluginRemove_Click(object sender, RoutedEventArgs e) {
            string sel = lstPluginFiles.SelectedValue as string;
            if(sel != null) PluginFiles.Remove(sel);
        }

        private void lvwLangEditor_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key != Key.Enter || lvwLangEditor.SelectedIndex + 1 >= LangItems.Count) return;
            lvwLangEditor.SelectedIndex++;
            ((LangEntry)lvwLangEditor.SelectedValue).IsEditing = true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e) {
            var resp = MessageBox.Show(
                    QTUtility.TextResourcesDic["Options_Page13_Language"][9],
                    QTUtility.TextResourcesDic["OptionsDialog"][3],
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if(resp == MessageBoxResult.Cancel) return;
            foreach(LangEntry entry in LangItems) {
                entry.Reset();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e) {
            using(OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = QTUtility.TextResourcesDic["FileFilters"][1] + "|*.xml";
                ofd.RestoreDirectory = true;
                if(DialogResult.OK != ofd.ShowDialog()) return;
                var dict = QTUtility.ReadLanguageFile(ofd.FileName);
                QTUtility.ValidateTextResources(ref dict);
                foreach(LangEntry entry in LangItems) {
                    if(entry.Index >= 0) {
                        entry.Translated = dict[entry.Key][entry.Index];
                    }
                    else {
                        entry.Translated = dict[entry.Key][0];
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            string path;
            using(SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Filter = QTUtility.TextResourcesDic["FileFilters"][1] + "|*.xml";
                sfd.RestoreDirectory = true;
                sfd.FileName = "Lng_QTTabBar_" + LangItems[1].Translated + ".xml";
                if(DialogResult.OK != sfd.ShowDialog()) return;
                path = sfd.FileName;
            }
            
            try {
                using(XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8)) {
                    writer.WriteStartDocument();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("root");
                    writer.WriteWhitespace(Environment.NewLine);
                    for(int i = 0; i < 3; i++) {
                        LangEntry entry = LangItems[i];
                        if(entry.Translated == "") continue;
                        writer.WriteWhitespace(Environment.NewLine);
                        writer.WriteStartElement(entry.Key);
                        writer.WriteWhitespace(Environment.NewLine);
                        writer.WriteValue(entry.Translated);
                        writer.WriteWhitespace(Environment.NewLine);
                        writer.WriteEndElement();
                        writer.WriteWhitespace(Environment.NewLine);
                    }
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("Version_QTTabBar");
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteValue(QTUtility2.MakeVersionString());
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteStartElement("DateModified");
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteValue(DateTime.Now.ToString("MM/dd/yyyy"));
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteComment(" data start ");
                    writer.WriteWhitespace(Environment.NewLine);
                    RefInt r = new RefInt { i = 3 };
                    while(r.i < LangItems.Count) {
                        string key = LangItems[r.i].Key;
                        string line = GetStrings(key, r).StringJoin(Environment.NewLine);
                        writer.WriteWhitespace(Environment.NewLine);
                        writer.WriteStartElement(key);
                        writer.WriteWhitespace(Environment.NewLine);
                        writer.WriteValue(line);
                        writer.WriteWhitespace(Environment.NewLine);
                        writer.WriteEndElement();
                        writer.WriteWhitespace(Environment.NewLine);                        
                    }
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteComment(" data end ");
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteWhitespace(Environment.NewLine);
                    writer.WriteEndElement();
                    writer.WriteWhitespace(Environment.NewLine);
                }
            }
            catch(XmlException) {
                MessageBox.Show(QTUtility.TextResourcesDic["Options_Page13_Language"][10]);
            }
            catch(Exception exception2) {
                QTUtility2.MakeErrorLog(exception2);
            }
        }

        private class RefInt {
            public int i;
        }
        private IEnumerable<string> GetStrings(string key, RefInt r) {
            const string newline = "\r\n";
            const string escaped = @"\r\n";
            while(r.i < LangItems.Count && LangItems[r.i].Key == key) {
                yield return LangItems[r.i++].Translated.Replace(newline, escaped);
            }
        }

        private void lvwLangEditor_ItemSelected(object sender, RoutedEventArgs e) {
            LangEntry entry = (LangEntry)(((ListViewItem)sender).DataContext);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => entry.IsEditing = true));
        }

        #region 选择框事件
        private void buildinCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ( true != radUseLangFileYes.IsChecked)
            {
                  WorkingConfig.lang.BuiltInLangSelectedIndex = buildinCbx.SelectedIndex;
                  ConfigManager.LoadedConfig = QTUtility2.DeepClone(WorkingConfig);
                  ConfigManager.WriteConfig();
                  ConfigManager.UpdateConfig();
                    //QTUtility.ValidateTextResources
            }
         }
        #endregion

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class LangEntry : INotifyPropertyChanged, IEditableEntry {
            public event PropertyChangedEventHandler PropertyChanged;
            public string Original { get { return Index < 0 ? Key : QTUtility.TextResourcesDic[Key][Index]; } }
            public int Index { get; set; }
            public bool IsEditing { get; set; }
            public string Location { get {
                return Index < 0 ? "** Metadata **" : Key;
                // todo
            }}

            public string Key { get; set; }
            public string Translated { get; set; }

            public LangEntry(string key, int index) {
                Key = key;
                Index = index;
                Reset();
            }

            public void Reset() {
                string[] res;
                if(Index >= 0) {
                    Translated = Original;
                }
                else if(QTUtility.TextResourcesDic.TryGetValue(Key, out res)) {
                    Translated = res[0];
                }
                else {
                    Translated = "";
                }
            }
        }

        #endregion
    }
}

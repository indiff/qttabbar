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
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Keys = System.Windows.Forms.Keys;

namespace QTTabBarLib {
    internal partial class Options08_Keys : OptionsDialogTab, IHotkeyContainer {
        private List<HotkeyEntry> HotkeyEntries;
        public event NewHotkeyRequestedHandler NewHotkeyRequested;

        public Options08_Keys() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            int[] keys = WorkingConfig.keys.Shortcuts;
            HotkeyEntries = new List<HotkeyEntry>();
            for(int i = 0; i < (int)BindAction.KEYBOARD_ACTION_COUNT; ++i) {
                HotkeyEntries.Add(new HotkeyEntry(keys, i));
            }

            var PluginShortcuts = new Dictionary<string, int[]>();
            foreach(var info in PluginManager.PluginInformations) {
                Plugin p;
                if(!PluginManager.TryGetStaticPluginInstance(info.PluginID, out p) || !p.PluginInformation.Enabled) continue;
                string[] actions = null;
                try {
                    if(!p.Instance.QueryShortcutKeys(out actions)) actions = null;
                }
                catch {
                }
                if(actions == null) continue;
                if(WorkingConfig.keys.PluginShortcuts.TryGetValue(info.PluginID, out keys)) {
                    Array.Resize(ref keys, actions.Length);
                    PluginShortcuts[info.PluginID] = keys;
                }
                else {
                    PluginShortcuts[info.PluginID] = new int[actions.Length];
                }
                HotkeyEntries.AddRange(actions.Select((act, i) => new HotkeyEntry(keys, i, act, p.PluginInformation.Name)));
            }
            WorkingConfig.keys.PluginShortcuts = PluginShortcuts;

            ICollectionView view = CollectionViewSource.GetDefaultView(HotkeyEntries);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("PluginName");
            view.GroupDescriptions.Add(groupDescription);
            lvwHotkeys.ItemsSource = view;
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.keys = new Config._Keys();
            InitializeConfig();
        }

        public override void CommitConfig() {
            // Not needed; everything is done through bindings
        }

        public IEnumerable<IHotkeyEntry> GetHotkeyEntries() {
            return HotkeyEntries.Cast<IHotkeyEntry>();
        }

        private void lvwHotkeys_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(NewHotkeyRequested == null) return;
            if(lvwHotkeys.SelectedItems.Count != 1) return;
            HotkeyEntry entry = (HotkeyEntry)lvwHotkeys.SelectedItem;
            Keys newKey;
            if(!NewHotkeyRequested(e, entry.ShortcutKey, out newKey)) return;
            bool wasNotAssigned = !entry.Assigned;
            entry.ShortcutKey = newKey;
            if(wasNotAssigned && entry.Assigned) entry.Enabled = true;
            e.Handled = true;
        }

        private void lvwHotkeys_TextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = true;
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class HotkeyEntry : INotifyPropertyChanged, IHotkeyEntry {
            public event PropertyChangedEventHandler PropertyChanged;
            private int[] raws;
            public int RawKey {
                get { return raws[Index]; }
                set { raws[Index] = value; }
            }
            public bool Enabled {
                get { return (RawKey & QTUtility.FLAG_KEYENABLED) != 0 && RawKey != QTUtility.FLAG_KEYENABLED; }
                set { if(value) RawKey |= QTUtility.FLAG_KEYENABLED; else RawKey &= ~QTUtility.FLAG_KEYENABLED; }
            }
            public Keys ShortcutKey {
                get { return (Keys)(RawKey & ~QTUtility.FLAG_KEYENABLED); }
                set { RawKey = (int)value | (Enabled ? QTUtility.FLAG_KEYENABLED : 0); }
            }
            public bool Assigned {
                get { return ShortcutKey != Keys.None; }
            }
            public string HotkeyString {
                get { return QTUtility2.MakeKeyString(ShortcutKey); }
            }
            public string PluginName { get; set; }
            public string KeyActionText { get {
                return PluginName == "" 
                        ? QTUtility.TextResourcesDic["ShortcutKeys_ActionNames"][Index] 
                        : pluginAction;
            } }
            public int Index { get; set; }

            private string pluginAction;
            public HotkeyEntry(int[] raws, int index, string action = "", string pluginName = "") {
                this.raws = raws;
                Index = index;
                pluginAction = action;
                PluginName = pluginName;
            }
        }

        #endregion
    }
}

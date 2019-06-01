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
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "Folder Memo", Version = "1.0.0.0", Description = "Memo for folder.")]
    public class Memo : IPluginClient {
        internal static byte[] ConfigValues = new byte[4];
        private MemoForm memoForm;
        private IPluginServer pluginServer;
        private static string REG_MEMO = "QuizoPlugins.Memo";
        internal static string[] ResStrs;

        static Memo() {
            ReadSettings();
        }

        public void Close(EndCode code) {
            pluginServer = null;
            if(memoForm != null) {
                memoForm.Dispose();
            }
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
            ShowMemoForm(tab.Address.Path, true);
        }

        public void OnOption() {
            using(Option option = new Option()) {
                if(DialogResult.OK == option.ShowDialog()) {
                    ConfigValues[0] = 0;
                    switch(option.ShowMode) {
                        case 1:
                            ConfigValues[0] = (byte)(ConfigValues[0] | 0x80);
                            goto Label_0078;

                        case 2:
                            ConfigValues[0] = (byte)(ConfigValues[0] | 0xc0);
                            goto Label_0078;
                    }
                }
            }
        Label_0078:
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Quizo\QTTabBar\Plugins\Settings\" + REG_MEMO)) {
                if(key != null) {
                    key.SetValue("Config", ConfigValues);
                }
            }
        }

        public void OnShortcutKeyPressed(int index) {
            if((memoForm != null) && memoForm.Visible) {
                memoForm.HideMemoForm();
            }
            else {
                ITab selectedTab = pluginServer.SelectedTab;
                if(selectedTab != null) {
                    ShowMemoForm(selectedTab.Address.Path, true);
                }
            }
        }

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            if(!this.pluginServer.TryGetLocalizedStrings(this, 4, out ResStrs)) {
                if(CultureInfo.CurrentCulture.Parent.Name == "ja") {
                    ResStrs = Resource.str_ja.Split(new char[] { ';' });
                }
                else {
                    ResStrs = Resource.str_en.Split(new char[] { ';' });
                }
            }
            this.pluginServer.RegisterMenu(this, MenuType.Tab, ResStrs[0], true);
            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
            this.pluginServer.ExplorerStateChanged += pluginServer_ExplorerStateChanged;
        }

        internal void OpenDirectory(string path) {
            pluginServer.CreateTab(new Address(path), -1, false, true);
        }

        private void pluginServer_ExplorerStateChanged(object sender, PluginEventArgs e) {
            if(e.WindowAction == ExplorerWindowActions.Minimized) {
                if(memoForm != null) {
                    memoForm.HideMemoForm();
                }
            }
            else if(e.WindowAction == ExplorerWindowActions.Restored) {
                ITab selectedTab = pluginServer.SelectedTab;
                if(((selectedTab != null) && !string.IsNullOrEmpty(selectedTab.Address.Path)) && Directory.Exists(selectedTab.Address.Path)) {
                    ShowMemoForm(selectedTab.Address.Path, false);
                }
            }
        }

        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            string path = e.Address.Path;
            if(!string.IsNullOrEmpty(path) && Directory.Exists(path)) {
                ShowMemoForm(path, false);
            }
        }

        public bool QueryShortcutKeys(out string[] descriptions) {
            descriptions = new string[] { ResStrs[0] };
            return true;
        }

        private static void ReadSettings() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\Plugins\Settings\" + REG_MEMO, false)) {
                if(key != null) {
                    ConfigValues = (byte[])key.GetValue("Config", new byte[4]);
                }
            }
        }

        private void ShowMemoForm(string path, bool fForce) {
            try {
                if(memoForm == null) {
                    memoForm = new MemoForm(this);
                }
                if(fForce) {
                    memoForm.ShowMemoForm(path);
                }
                else if(((ConfigValues[0] & 0xc0) != 0xc0) && (((ConfigValues[0] & 0x80) == 0x80) || memoForm.ContainsPath(path))) {
                    memoForm.ShowMemoForm(path);
                }
                else {
                    memoForm.HideMemoForm();
                }
            }
            catch(Exception) {
            }
        }

        public static void Uninstall() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Quizo\QTTabBar\Plugins\Settings", true)) {
                if(key != null) {
                    try {
                        key.DeleteSubKey(REG_MEMO, false);
                    }
                    catch {
                    }
                }
            }
        }

        public bool HasOption {
            get {
                return true;
            }
        }
    }
}

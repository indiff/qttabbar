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
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "FolderTreeButton", Version = "1.0.0.0", Description = "Show folder tree for XP")]
    public class QTFolderTreeButton : IBarButton {
        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;
        private string[] ResStrs;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;

            if(!pluginServer.TryGetLocalizedStrings(this, 3, out ResStrs)) {
                if(CultureInfo.CurrentCulture.Parent.Name == "ja")
                    ResStrs = Resource.strQTFolderButton_ja.Split(new char[] { ';' });
                else
                    ResStrs = Resource.strQTFolderButton.Split(new char[] { ';' });
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { ResStrs[0], ResStrs[2] };
            return true;
        }

        public void Close(EndCode code) {
            if(code != EndCode.Hidden) {
                pluginServer = null;
                shellBrowser = null;
            }
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        public void OnShortcutKeyPressed(int index) {
            if(index == 0)
                ShowHideFolderTree();
            else
                FocusOnFolderTree();
        }

        #endregion


        #region IBarButton Members

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.QTFolderButton_large : Resource.QTFolderButton_small;
        }

        public void InitializeItem() {
        }

        public void OnButtonClick() {
            ShowHideFolderTree();
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return ResStrs[1];
            }
        }

        #endregion


        private void ShowHideFolderTree() {
            const uint FCW_TREE = 0x0003;

            IntPtr hwndTree;
            bool fTreeVisible = 0 == shellBrowser.GetControlWindow(FCW_TREE, out hwndTree);

            pluginServer.ExecuteCommand(Commands.ShowFolderTree, !fTreeVisible);
        }

        private void FocusOnFolderTree() {
            const uint FCW_TREE = 0x0003;

            IntPtr hwndTree;
            if(0 == shellBrowser.GetControlWindow(FCW_TREE, out hwndTree)) {
                if(GetFocus() == hwndTree) {
                    pluginServer.ExecuteCommand(Commands.FocusFileList, null);
                }
                else {
                    SetFocus(hwndTree);
                }
            }
        }
    }
}

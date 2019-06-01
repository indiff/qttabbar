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
using System.Runtime.InteropServices;
using System.Text;
using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "Show StatusBar", Version = "0.9.0.0", Description = "ShowStatusBar")]
    public class ShowStatusBar : IPluginClient {
        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, StringBuilder lpszClass, string lpszWindow);

        private static bool fVista = IsVista();
        private const int WM_COMMAND = 0x0111;

        private static bool IsVista() {
            return Environment.OSVersion.Version.Major > 5;
        }


        #region IPluginClient members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { "Show statusbar" };
            return true;
        }

        public void Close(EndCode endCode) {

        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public void OnOption() {
        }

        public void OnShortcutKeyPressed(int index) {
            IntPtr hwndExplr = pluginServer.ExplorerHandle;
            IntPtr hwnd = fVista ? FindWindowEx(hwndExplr, IntPtr.Zero, new StringBuilder("ShellTabWindowClass"), null) : hwndExplr;

            int command = 0xA202;

            if(fVista)
                SendMessage(hwnd, WM_COMMAND, (IntPtr)command, IntPtr.Zero);
            else
                PostMessage(hwnd, WM_COMMAND, (IntPtr)command, IntPtr.Zero);
        }

        #endregion

    }
}

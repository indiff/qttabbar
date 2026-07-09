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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using QTTabBarLib.Interop;
using SHDocVw;

namespace QTTabBarLib {

    // Adds "QTTabBar Options" to a folder background's right-click menu, so QTTabBar
    // is reachable/usable even when Explorer never hosts the toolbar (e.g. newer
    // Windows 11 builds with no rebar/toolband UI left). The per-window attach this
    // used to also offer here ("Enable QTTabBar (experimental)") is now the
    // Options > Window > "Enable QTTabBar on every Explorer window" toggle instead,
    // which covers every window automatically via AutoLoader.
    [Guid("B3FFB6A3-3BC5-4D2F-8C49-6432D7174A3E"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class ContextMenuOptions : IContextMenu, IShellExtInit {
        private const string KEYNAME = @"Directory\Background\shellex\ContextMenuHandlers\QTTabBar";
        private const uint CMD_OPTIONS = 0;
        private const uint MF_STRING = 0x00000000;
        private const uint MF_BYPOSITION = 0x00000400;
        private const uint CMF_DEFAULTONLY = 0x00000001;

        [ComRegisterFunction]
        public static void Register(Type t) {
            string name = t.GUID.ToString("B");
            using(RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name)) {
                key.SetValue(null, "QTTabBar Options Context Menu");
            }
            using(RegistryKey key = Registry.ClassesRoot.CreateSubKey(KEYNAME)) {
                key.SetValue(null, name);
            }
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t) {
            try {
                Registry.ClassesRoot.DeleteSubKeyTree(KEYNAME);
            }
            catch {
            }
        }

        public int QueryContextMenu(IntPtr hMenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags) {
            if((uFlags & CMF_DEFAULTONLY) != 0) return 0;
            // Insert at indexMenu (like the other classic shell extensions) instead of
            // appending, so the item lands in the extensions cluster instead of at the
            // very bottom of the menu.
            PInvoke.InsertMenu(hMenu, indexMenu, MF_BYPOSITION | MF_STRING, (IntPtr)(idCmdFirst + CMD_OPTIONS), "QTTabBar Options");
            return 1; // Highest command offset used (CMD_OPTIONS) + 1.
        }

        public int InvokeCommand(ref CMINVOKECOMMANDINFO pici) {
            long verb = pici.lpVerb.ToInt64();
            if((verb >> 16) != 0) return unchecked((int)0x80070057); // E_INVALIDARG: string verbs unsupported.
            // Normally the toolbar band triggers this on load. Without a band (e.g.
            // Explorer never hosts the toolbar), InstanceManager/Config/etc. are
            // never set up, so force it before doing anything else below.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(QTUtility).TypeHandle);
            if((uint)verb == CMD_OPTIONS) {
                OptionsDialog.Open();
            }
            return 0;
        }

        // Manually drives the same SetSite/ShowDW lifecycle Explorer would normally
        // drive for a docked toolband, so QTTabBarClass attaches (mouse actions,
        // hover previews, ...) to a window without ever being shown in a rebar.
        // Called from AutoLoader, which already has the IWebBrowser2 for its own window
        // as the BHO site. Must be called on the target window's own UI thread -
        // AutoLoader.SetSite satisfies this since Explorer invokes it there directly.
        internal static void AttachToWindow(IWebBrowser2 webBrowser) {
            try {
                // Attaching twice on the same (thread-affine) window creates a second
                // overlapping QTTabBarClass instance and corrupts drag/drop handoff.
                // A real deskband is only ever instantiated once per window by
                // Explorer, so mirror that here.
                if(InstanceManager.GetThreadTabBar() != null) return;

                QTTabBarClass tabBar = new QTTabBarClass();
                tabBar.SetSite(new ExplorerSiteAdapter(webBrowser));
                tabBar.ShowDW(true);
                QTUtility2.flog("ContextMenuOptions.AttachToWindow: SetSite/ShowDW completed");
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex, "ContextMenuOptions.AttachToWindow");
            }
        }

        public void GetCommandString(uint idCmd, uint uFlags, ref int pwReserved, IntPtr commandstring, uint cch) {
        }

        public void Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hKeyProgID) {
        }
    }
}

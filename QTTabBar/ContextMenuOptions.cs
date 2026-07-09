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

    // Adds "QTTabBar Options" (and an experimental "Enable QTTabBar" toggle) to a
    // folder background's right-click menu, so QTTabBar is reachable/usable even
    // when Explorer never hosts the toolbar (e.g. newer Windows 11 builds with no
    // rebar/toolband UI left).
    [Guid("B3FFB6A3-3BC5-4D2F-8C49-6432D7174A3E"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class ContextMenuOptions : IContextMenu, IShellExtInit {
        private const string KEYNAME = @"Directory\Background\shellex\ContextMenuHandlers\QTTabBar";
        private const uint CMD_OPTIONS = 0;
        private const uint CMD_ENABLE = 1;
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
            // appending, so the items land in the extensions cluster instead of at the
            // very bottom of the menu.
            PInvoke.InsertMenu(hMenu, indexMenu, MF_BYPOSITION | MF_STRING, (IntPtr)(idCmdFirst + CMD_OPTIONS), "QTTabBar Options");
            PInvoke.InsertMenu(hMenu, indexMenu + 1, MF_BYPOSITION | MF_STRING, (IntPtr)(idCmdFirst + CMD_ENABLE), "Enable QTTabBar (experimental)");
            return 2; // Highest command offset used (CMD_ENABLE) + 1.
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
            else if((uint)verb == CMD_ENABLE) {
                AttachToWindow(pici.hwnd);
            }
            return 0;
        }

        // Manually drives the same SetSite/ShowDW lifecycle Explorer would normally
        // drive for a docked toolband, so QTTabBarClass attaches (mouse actions,
        // hover previews, ...) to an already-open window without ever being shown
        // in a rebar.
        private const int GA_ROOT = 2;

        private static void AttachToWindow(IntPtr hwnd) {
            try {
                // pici.hwnd is usually a child window (e.g. the folder view), not the
                // top-level frame HWND that IWebBrowser2.HWND reports. Resolve to the
                // root so the match below actually finds the window.
                IntPtr rootHwnd = PInvoke.GetAncestor(hwnd, GA_ROOT);
                if(rootHwnd == IntPtr.Zero) rootHwnd = hwnd;
                QTUtility2.flog("ContextMenuOptions.AttachToWindow: hwnd=" + hwnd + " root=" + rootHwnd);

                // Activate by CLSID and cast to the interface (rather than "new
                // ShellWindows()") to avoid embedded-interop-type identity mismatches
                // that can make the RCW uncastable to the generated coclass wrapper.
                object shellWindowsObj = Activator.CreateInstance(
                        Type.GetTypeFromCLSID(new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39")));
                IShellWindows shellWindows = (IShellWindows)shellWindowsObj;

                IWebBrowser2 webBrowser = null;
                foreach(object item in shellWindows) {
                    IWebBrowser2 wb = item as IWebBrowser2;
                    if(wb == null) continue;
                    QTUtility2.flog("ContextMenuOptions.AttachToWindow: candidate hwnd=" + (IntPtr)wb.HWND);
                    if((IntPtr)wb.HWND == rootHwnd) {
                        webBrowser = wb;
                        break;
                    }
                }
                if(webBrowser == null) {
                    QTUtility2.flog("ContextMenuOptions.AttachToWindow: no matching IWebBrowser2 found");
                    return;
                }
                AttachToWindow(webBrowser);
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex, "ContextMenuOptions.AttachToWindow");
            }
        }

        // Shared by the context-menu path above (resolves an IWebBrowser2 from an hwnd first)
        // and AutoLoader (already has the IWebBrowser2 for its own window as the BHO site).
        // Must be called on the target window's own UI thread - AutoLoader.SetSite and
        // InvokeCommand both satisfy this since Explorer invokes them there directly.
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

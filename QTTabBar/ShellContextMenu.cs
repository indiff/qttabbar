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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BandObjectLib;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    class ShellContextMenu : IDisposable {
        private IContextMenu2 pIContextMenu2;

        /// <summary>
        /// Displays shell shortcut menu.
        /// </summary>
        /// <param name="idlw">IDLWrapper object that specifies shell item</param>
        /// <param name="pntShow">location of the shortcut menu, in screen coordinates.</param>
        /// <param name="hwndParent">Handle of parent control. Parent control will get focus, and receives the messages about drawing 'Send to' submenues.</param>
        /// <param name="fCanRemove">set true to add 'remove this' menu item.</param>
        /// <returns>
        /// 0xFFFF	user selected "Remove this item from menu".
        /// 0xFFFE	user selected "Open containing folder".
        /// 0xFFFD	If the user cancels the menu without making a selection, or if an error occurs
        /// </returns>
        public int Open(IDLWrapper idlw, Point pntShow, IntPtr hwndParent, bool fCanRemove) {
            const uint MF_STRING = 0x00000000;
            const uint MF_SEPARATOR = 0x00000800;
            const uint CMF_NORMAL = 0x00000000;
            const uint CMF_EXTENDEDVERBS = 0x00000100;
            const uint TPM_RETURNCMD = 0x0100;
            const uint S_OK = 0;
            const int COMMANDID_REMOVEITEM = 0xffff; // todo: move to const class
            const int COMMANDID_OPENPARENT = 0xfffe;
            const int COMMANDID_USERCANCEL = 0xfffd;
            IShellFolder shellFolderParent = null;

            try {
                // get IShellFolder
                IntPtr pIDLRelative;
                if(idlw.Available && S_OK == PInvoke.SHBindToParent(idlw.PIDL, ExplorerGUIDs.IID_IShellFolder, out shellFolderParent, out pIDLRelative) && shellFolderParent != null) {
                    // get IContextMenu2
                    IntPtr[] pIDLs = new IntPtr[] { pIDLRelative };
                    uint reserved = 0;
                    object oUnk;

                    if(S_OK == shellFolderParent.GetUIObjectOf(IntPtr.Zero, (uint)pIDLs.Length, pIDLs, ExplorerGUIDs.IID_IContextMenu, ref reserved, out oUnk)) {
                        pIContextMenu2 = oUnk as IContextMenu2;
                        if(pIContextMenu2 != null) {
                            using(ContextMenu contextMenu = new ContextMenu()) {
                                int nResult = -1;
                                uint uFlags = CMF_NORMAL;
                                if((Control.ModifierKeys & Keys.Shift) == Keys.Shift) {
                                    uFlags |= CMF_EXTENDEDVERBS;
                                }

                                pIContextMenu2.QueryContextMenu(contextMenu.Handle, 0, 1, 0xffff, uFlags);

                                // append optional menus
                                if(fCanRemove) {
                                    // "Remove this item from menu"
                                    PInvoke.AppendMenu(contextMenu.Handle, MF_SEPARATOR, IntPtr.Zero, null);
                                    PInvoke.AppendMenu(contextMenu.Handle, MF_STRING, new IntPtr(COMMANDID_REMOVEITEM), QTUtility.ResMain[25]);
                                }
                                if(idlw.HasPath && idlw.Path.Length > 3 && idlw.IsFileSystem /*&& ( idlw.IsFileSystemFolder || idlw.IsFileSystemFile )*/ ) {
                                    // "Open containing folder"
                                    if(!fCanRemove) {
                                        // separator
                                        PInvoke.AppendMenu(contextMenu.Handle, MF_SEPARATOR, IntPtr.Zero, null);
                                    }
                                    PInvoke.AppendMenu(contextMenu.Handle, MF_STRING, new IntPtr(COMMANDID_OPENPARENT), QTUtility.ResMain[26]);
                                }

                                uint commandID = PInvoke.TrackPopupMenu(contextMenu.Handle, TPM_RETURNCMD, pntShow.X, pntShow.Y, 0, hwndParent, IntPtr.Zero);
                                if(commandID != 0) {
                                    if(commandID == COMMANDID_REMOVEITEM) {
                                        return COMMANDID_REMOVEITEM;
                                    }
                                    else if(commandID == COMMANDID_OPENPARENT) {
                                        if(idlw.HasPath) {
                                            try {
                                                QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
                                                if(tabbar != null) {
                                                    using(IDLWrapper idlwParent = idlw.GetParent()) {
                                                        if(idlwParent.Available) {
                                                            tabbar.OpenNewTabOrWindow(idlwParent);
                                                        }
                                                    }
                                                }
                                                // DesktopTool will handle it by itself
                                                nResult = COMMANDID_OPENPARENT;
                                            }
                                            catch {
                                                System.Media.SystemSounds.Asterisk.Play();
                                            }
                                        }
                                    }
                                    else {
                                        CMINVOKECOMMANDINFO cmInfo = new CMINVOKECOMMANDINFO {
                                            cbSize = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFO)),
                                            fMask = 0,
                                            hwnd = hwndParent,
                                            lpVerb = (IntPtr)((commandID - 1) & 0xFFFF),
                                            lpParameters = IntPtr.Zero,
                                            lpDirectory = IntPtr.Zero,
                                            nShow = 1, //SW_SHOWNORMAL;
                                            dwHotKey = 0,
                                            hIcon = IntPtr.Zero
                                        };

                                        // returns S_OK if successful, or an error value otherwise. 
                                        // E_ABORT when user clicked "Open folder link target exists" of link file...( E_ABORT  _HRESULT_TYPEDEF_(0x80004004L) )
                                        nResult = pIContextMenu2.InvokeCommand(ref cmInfo);
                                    }
                                }
                                else {
                                    // 'if the user cancels the menu without making a selection, or if an error occurs' (MSDN)
                                    nResult = COMMANDID_USERCANCEL;
                                }

                                return nResult;
                            }
                        }
                    }
                }

                // if failed to create shell context menu, show 'remove this' menu instead
                if(fCanRemove) {
                    using(ContextMenu contextMenu = new ContextMenu()) {
                        PInvoke.AppendMenu(contextMenu.Handle, MF_STRING, new IntPtr(COMMANDID_REMOVEITEM), QTUtility.ResMain[25]);

                        if(COMMANDID_REMOVEITEM == PInvoke.TrackPopupMenu(contextMenu.Handle, TPM_RETURNCMD, pntShow.X, pntShow.Y, 0, hwndParent, IntPtr.Zero)) {
                            return COMMANDID_REMOVEITEM;
                        }
                    }
                }

                return COMMANDID_USERCANCEL;
            }
            catch {
            }
            finally {
                if(shellFolderParent != null) {
                    Marshal.ReleaseComObject(shellFolderParent);
                }

                if(pIContextMenu2 != null) {
                    Marshal.ReleaseComObject(pIContextMenu2);
                    pIContextMenu2 = null;
                }
            }
            return -1;
        }

        /// <summary>
        /// Displays shell shortcut menu for multiple items. Paths must be sub items of one directory.
        /// </summary>
        /// <param name="lstIDLs"></param>
        /// <param name="pntShow"></param>
        /// <param name="hwndParent"></param>
        /// <returns></returns>
        public int Open(List<byte[]> lstIDLs, Point pntShow, IntPtr hwndParent) {
            // All lstIDLs members must share the same parent, for compatibility.
            const uint S_OK = 0;
            if(lstIDLs == null || lstIDLs.Count == 0) return -1;

            IShellFolder shellFolder = null;
            List<IntPtr> lstRltvPIDLs = new List<IntPtr>();
            IntPtr pIDLFirst = IntPtr.Zero;
            try {
                // build array of relative idls
                for(int i = 0; i < lstIDLs.Count; i++) {
                    if(i == 0) {
                        pIDLFirst = ShellMethods.CreateIDL(lstIDLs[0]);
                    }

                    using(IDLWrapper idlw = new IDLWrapper(lstIDLs[i])) {
                        if(!idlw.Available) continue;
                        IntPtr p = PInvoke.ILClone(PInvoke.ILFindLastID(idlw.PIDL));
                        if(p != IntPtr.Zero) {
                            lstRltvPIDLs.Add(p);
                        }
                    }
                }
                IntPtr[] apidl = lstRltvPIDLs.ToArray();

                if(apidl.Length > 0) {
                    IntPtr pIDLRltv1st;
                    if(S_OK == PInvoke.SHBindToParent(pIDLFirst, ExplorerGUIDs.IID_IShellFolder, out shellFolder, out pIDLRltv1st)) {
                        // get IContextMenu2
                        uint rsv = 0;
                        object oUnk;

                        if(S_OK == shellFolder.GetUIObjectOf(IntPtr.Zero, (uint)apidl.Length, apidl, ExplorerGUIDs.IID_IContextMenu, ref rsv, out oUnk)) {
                            pIContextMenu2 = oUnk as IContextMenu2;
                            if(pIContextMenu2 != null) {
                                using(ContextMenu contextMenu = new ContextMenu()) {
                                    const uint CMF_EXTENDEDVERBS = 0x00000100;
                                    const uint TPM_RETURNCMD = 0x0100;

                                    uint uFlags = 0;
                                    if((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                                        uFlags |= CMF_EXTENDEDVERBS;

                                    pIContextMenu2.QueryContextMenu(contextMenu.Handle, 0, 1, 0xffff, uFlags);

                                    uint commandID = PInvoke.TrackPopupMenu(contextMenu.Handle, TPM_RETURNCMD, pntShow.X, pntShow.Y, 0, hwndParent, IntPtr.Zero);
                                    if(commandID != 0) {
                                        CMINVOKECOMMANDINFO cmInfo = new CMINVOKECOMMANDINFO {
                                            cbSize = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFO)),
                                            fMask = 0,
                                            hwnd = hwndParent,
                                            lpVerb = (IntPtr)((commandID - 1) & 0xFFFF),
                                            lpParameters = IntPtr.Zero,
                                            lpDirectory = IntPtr.Zero,
                                            nShow = 1, //SW_SHOWNORMAL
                                            dwHotKey = 0,
                                            hIcon = IntPtr.Zero
                                        };

                                        // this returns E_ABORT when user clicked "Open folder link target exists" of link file...
                                        pIContextMenu2.InvokeCommand(ref cmInfo);
                                        return 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch {
            }
            finally {
                if(shellFolder != null) {
                    Marshal.ReleaseComObject(shellFolder);
                }
                if(pIDLFirst != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(pIDLFirst);
                }
                foreach(IntPtr pIDL in lstRltvPIDLs.Where(pIDL => pIDL != IntPtr.Zero)) {
                    PInvoke.CoTaskMemFree(pIDL);
                }
                if(pIContextMenu2 != null) {
                    Marshal.ReleaseComObject(pIContextMenu2);
                    pIContextMenu2 = null;
                }
            }
            return -1;
        }

        
        public void Dispose() {
            if(pIContextMenu2 != null) {
                Marshal.FinalReleaseComObject(pIContextMenu2);
                pIContextMenu2 = null;
            }
        }

        public bool TryHandleMenuMsg(int msg, IntPtr wParam, IntPtr lParam) {
            if(pIContextMenu2 == null) return false;
            try {
                pIContextMenu2.HandleMenuMsg(msg, wParam, lParam);
            }
            catch {
            }
            return true;
        }
    }
}

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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    class TreeViewWrapper : IDisposable {
        public delegate void TreeViewMiddleClickedHandler(IShellItem item);
        public event FolderClickedHandler TreeViewClicked;

        private bool fDisposed;
        private INameSpaceTreeControl treeControl;
        private NativeWindowController treeController;
        private NativeWindowController parentController;
        private bool fPreventSelChange;

        public TreeViewWrapper(IntPtr hwnd, INameSpaceTreeControl treeControl) {
            this.treeControl = treeControl;
            treeController = new NativeWindowController(hwnd);
            treeController.MessageCaptured += TreeControl_MessageCaptured;
            parentController = new NativeWindowController(PInvoke.GetParent(hwnd));
            parentController.MessageCaptured += ParentControl_MessageCaptured;
        }

        private bool HandleClick(Point pt, Keys modifierKeys, bool middle) {
            IShellItem item = null;
            try {
                TVHITTESTINFO structure = new TVHITTESTINFO { pt = pt };
                IntPtr wParam = PInvoke.SendMessage(treeController.Handle, 0x1111, IntPtr.Zero, ref structure);
                if(wParam != IntPtr.Zero) {
                    if((structure.flags & 0x10) == 0 && (structure.flags & 0x80) == 0) {
                        treeControl.HitTest(pt, out item);
                        if(item != null) {
                            IntPtr pidl;
                            if(PInvoke.SHGetIDListFromObject(item, out pidl) == 0) {
                                using(IDLWrapper wrapper = new IDLWrapper(pidl)) {
                                    return TreeViewClicked(wrapper, modifierKeys, middle);
                                }
                            }
                        }
                    }
                }
            }
            finally {
                if(item != null) {
                    Marshal.ReleaseComObject(item);
                }
            }
            return false;
        }

        private bool TreeControl_MessageCaptured(ref Message msg) {
            switch(msg.Msg) {
                case WM.USER:
                    fPreventSelChange = false;
                    break;

                case WM.MBUTTONUP:
                    if(treeControl != null && TreeViewClicked != null) {
                        HandleClick(QTUtility2.PointFromLPARAM(msg.LParam), Control.ModifierKeys, true);
                    }
                    break;

                case WM.DESTROY:
                    if(treeControl != null) {
                        Marshal.ReleaseComObject(treeControl);
                        treeControl = null;
                    }
                    break;
            }
            return false;
        }

        private bool ParentControl_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.NOTIFY) {
                NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                switch(nmhdr.code) {
                    case -2: /* NM_CLICK */
                        if(Control.ModifierKeys != Keys.None) {
                            Point pt = Control.MousePosition;
                            PInvoke.ScreenToClient(nmhdr.hwndFrom, ref pt);
                            if(HandleClick(pt, Control.ModifierKeys, false)) {
                                fPreventSelChange = true;
                                PInvoke.PostMessage(nmhdr.hwndFrom, WM.USER, IntPtr.Zero, IntPtr.Zero);
                                return true;                                
                            }
                        }
                        break;

                    case -450: /* TVN_SELECTIONCHANGING */
                        if(fPreventSelChange) {
                            msg.Result = (IntPtr)1;
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        #region IDisposable Members

        public void Dispose() {
            if(fDisposed) return;
            if(treeControl != null) {
                Marshal.ReleaseComObject(treeControl);
                treeControl = null;
            }
            fDisposed = true;
        }

        #endregion
    }
}

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
    class BreadcrumbBar {
        private const int TB_GETBUTTON = WM.USER + 23;
        private const int TB_BUTTONCOUNT = WM.USER + 24;
        private const int TB_COMMANDTOINDEX = WM.USER + 25;
        private const int TB_GETBUTTONINFOW = WM.USER + 63;
        private const int TB_HITTEST = WM.USER + 69;
        private const int TBIF_LPARAM = 0x10;
        private const int TBIF_COMMAND = 0x20;

        protected static readonly UInt32 WM_BREADCRUMBDPA = PInvoke.RegisterWindowMessage("QTTabBar_BreadcrumbDPA");
        public event FolderClickedHandler ItemClicked;
        private NativeWindowController BreadcrumbController;
        private NativeWindowController ParentController;
        private IntPtr hdpa = IntPtr.Zero;

        public BreadcrumbBar(IntPtr hwnd) {
            BreadcrumbController = new NativeWindowController(hwnd);
            BreadcrumbController.MessageCaptured += BreadcrumbController_MessageCaptured;
            ParentController = new NativeWindowController(PInvoke.GetParent(hwnd));
            ParentController.MessageCaptured += ParentController_MessageCaptured;
        }

        private int ButtonCount() {
            return (int)PInvoke.SendMessage(BreadcrumbController.Handle, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        private int HitTest(Point pt) {
            return (int)PInvoke.SendMessage(BreadcrumbController.Handle, TB_HITTEST, IntPtr.Zero, ref pt);
        }

        private int CommandToIndex(int id) {
            return (int)PInvoke.SendMessage(BreadcrumbController.Handle, TB_COMMANDTOINDEX, (IntPtr)id, IntPtr.Zero);
        }

        private int IndexToCommand(int idx) {
            TBBUTTON button = new TBBUTTON();
            PInvoke.SendMessage(BreadcrumbController.Handle, TB_GETBUTTON, (IntPtr)idx, ref button);
            return button.idCommand;
        }

        private IntPtr GetButtonLParam(int itemId) {
            TBBUTTONINFO info = new TBBUTTONINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.dwMask = TBIF_COMMAND | TBIF_LPARAM;
            info.idCommand = itemId;
            int ret = (int)PInvoke.SendMessage(BreadcrumbController.Handle, TB_GETBUTTONINFOW, (IntPtr)itemId, ref info);
            return ret == -1 ? IntPtr.Zero : info.lParam;           
        }

        // Catch left-clicks
        private bool ParentController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM_BREADCRUMBDPA) {
                hdpa = msg.LParam;
                return true;
            }
            else if(msg.Msg == WM.COMMAND) {
                if(hdpa != IntPtr.Zero && ItemClicked != null && Control.ModifierKeys != Keys.None) {
                    int itemId = PInvoke.LoWord(msg.WParam.ToInt32());
                    int idx = CommandToIndex(itemId);
                    if(idx >= 0 && idx < ButtonCount()) {
                        if(DoItemClick(itemId, Control.ModifierKeys, false)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Catch middle clicks
        private bool BreadcrumbController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.MBUTTONUP) {
                if(hdpa != IntPtr.Zero && ItemClicked != null) {
                    int idx = HitTest(QTUtility2.PointFromLPARAM(msg.LParam));
                    if(idx >= 0 && idx <= ButtonCount()) {
                        DoItemClick(IndexToCommand(idx), Control.ModifierKeys, true);
                    }
                }
            }
            return false;
        }

        private bool DoItemClick(int itemId, Keys modKeys, bool middle) {
            IntPtr lParam = GetButtonLParam(itemId);
            IntPtr ptr = PInvoke.DPA_GetPtr(hdpa, (int)lParam); // This pointer is NOT AddRef'd.  Do not Release it!!
            if(ptr == IntPtr.Zero) return false;
            IntPtr pidl;
            PInvoke.SHGetIDListFromObject(ptr, out pidl);
            using(IDLWrapper wrapper = new IDLWrapper(pidl)) {
                return ItemClicked(wrapper, modKeys, middle);
            }
        }
    }
}

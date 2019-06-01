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
using QTPlugin.Interop;

namespace QuizoPlugins {
    internal delegate bool EnumWndProc(IntPtr hwnd, IntPtr lParam);

    internal static class PInvoke_QTWM {
        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT pRect);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        //#define SWP_NOSIZE          0x0001
        //#define SWP_NOMOVE          0x0002
        //#define SWP_NOZORDER        0x0004
        //#define SWP_NOREDRAW        0x0008
        //#define SWP_NOACTIVATE      0x0010
        //#define SWP_FRAMECHANGED    0x0020  /* The frame changed: send WM_NCCALCSIZE */
        //#define SWP_SHOWWINDOW      0x0040
        //#define SWP_HIDEWINDOW      0x0080
        //#define SWP_NOCOPYBITS      0x0100
        //#define SWP_NOOWNERZORDER   0x0200  /* Don't do owner Z ordering */
        //#define SWP_NOSENDCHANGING  0x0400  /* Don't send WM_WINDOWPOSCHANGING */

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        //#define SW_HIDE             0
        //#define SW_SHOWNORMAL       1
        //#define SW_NORMAL           1
        //#define SW_SHOWMINIMIZED    2
        //#define SW_SHOWMAXIMIZED    3
        //#define SW_MAXIMIZE         3
        //#define SW_SHOWNOACTIVATE   4
        //#define SW_SHOW             5
        //#define SW_MINIMIZE         6
        //#define SW_SHOWMINNOACTIVE  7
        //#define SW_SHOWNA           8
        //#define SW_RESTORE          9
        //#define SW_SHOWDEFAULT      10
        //#define SW_FORCEMINIMIZE    11
        //#define SW_MAX              11

        [DllImport("user32.dll")]
        internal static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern int EnumWindows(EnumWndProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int EnumChildWindows(IntPtr hWndParent, EnumWndProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


        internal static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) {
            if(IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return new IntPtr(GetWindowLong(hWnd, nIndex));
        }

        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            if(IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong(hWnd, nIndex, (uint)dwNewLong));
        }

        internal static IntPtr Ptr_OP_OR(IntPtr ptr, uint ui) {
            if(IntPtr.Size == 8)
                return (IntPtr)(((ulong)ptr) | ui);
            else
                return (IntPtr)(((uint)ptr) | ui);
        }

        internal static IntPtr Ptr_OP_AND(IntPtr ptr, uint ui) {
            if(IntPtr.Size == 8)
                return (IntPtr)(((ulong)ptr) & ui);
            else
                return (IntPtr)(((uint)ptr) & ui);
        }

    }
}

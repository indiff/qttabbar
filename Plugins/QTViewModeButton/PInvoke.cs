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
using QTPlugin.Interop;

namespace QuizoPlugins {
    internal static class PInvoke {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
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
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
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

        //#define SWP_DRAWFRAME       SWP_FRAMECHANGED
        //#define SWP_NOREPOSITION    SWP_NOOWNERZORDER

        //#if(WINVER >= 0x0400)
        //#define SWP_DEFERERASE      0x2000
        //#define SWP_ASYNCWINDOWPOS  0x4000
        //#endif /* WINVER >= 0x0400 */


        //#define HWND_TOP        ((HWND)0)
        //#define HWND_BOTTOM     ((HWND)1)
        //#define HWND_TOPMOST    ((HWND)-1)
        //#define HWND_NOTOPMOST  ((HWND)-2)


        //[DllImport( "user32.dll" )]
        //public static extern bool AnimateWindow( IntPtr hwnd, int dwTime, int dwFlags );
        //const int AW_CENTER = 0x00000010;
        //const int AW_HIDE = 0x00010000;
        //const int AW_ACTIVATE = 0x00020000;
        //const int AW_SLIDE = 0x00040000;
        //const int AW_BLEND = 0x00080000;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref RECT lParam);

    }
}

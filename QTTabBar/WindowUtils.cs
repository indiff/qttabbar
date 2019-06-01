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
using System.Runtime.InteropServices;
using BandObjectLib;
using Microsoft.Win32;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class WindowUtils {
        public static void BringExplorerToFront(IntPtr hwndExplr) {
            PInvoke.ShowWindow(hwndExplr, PInvoke.IsIconic(hwndExplr) ? 9 : 5);
            PInvoke.SetForegroundWindow(hwndExplr);
        }

        public static void CloseExplorer(IntPtr hwndExplr, int nCode, bool doAsync = false) {
            if(QTUtility.IsXP && nCode == 0) nCode = 3;
            if(QTUtility.IsXP || doAsync) {
                PInvoke.PostMessage(hwndExplr, WM.CLOSE, IntPtr.Zero, (IntPtr)nCode);
            }
            else {
                PInvoke.SendMessage(hwndExplr, WM.CLOSE, IntPtr.Zero, (IntPtr)nCode);
            }
        }

        public static IntPtr FindChildWindow(IntPtr parent, Predicate<IntPtr> pred) {
            IntPtr ret = IntPtr.Zero;
            PInvoke.EnumChildWindows(parent, (hwnd, lParam) => {
                if(pred(hwnd)) {
                    ret = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return ret;
        }

        public static IntPtr GetShellTabWindowClass(IntPtr hwndExplr) {
            return PInvoke.FindWindowEx(hwndExplr, IntPtr.Zero, "ShellTabWindowClass", null);
        }

        public static IntPtr GetShellTrayWnd() {
            return PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
        }

        public static bool IsExplorerProcessSeparated() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", false)) {
                return ((key != null) && (((int)key.GetValue("SeparateProcess", 0)) != 0));
            }
        }
    }
}

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
using System.Security;

namespace QTTabBarLib.Interop {
    [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("028212A3-B627-47e9-8856-C14265554E4F")]
    public interface INameSpaceTreeControl {
        [PreserveSig]
        int Initialize(IntPtr hwndParent, ref RECT prc, int nsctsFlags);
        [PreserveSig]
        int TreeAdvise(IntPtr punk, out int pdwCookie);
        [PreserveSig]
        int TreeUnadvise(int dwCookie);
        [PreserveSig]
        int AppendRoot(IShellItem psiRoot, int grfEnumFlags, int grfRootStyle, /*IShellItemFilter*/ IntPtr pif);
        [PreserveSig]
        int InsertRoot(int iIndex, IShellItem psiRoot, int grfEnumFlags, int grfRootStyle, /*IShellItemFilter*/ IntPtr pif);
        [PreserveSig]
        int RemoveRoot(IShellItem psiRoot);
        [PreserveSig]
        int RemoveAllRoots();
        [PreserveSig]
        int GetRootItems(out /*IShellItemArray*/ IntPtr ppsiaRootItems);
        [PreserveSig]
        int SetItemState(IShellItem psi, int nstcisMask, int nstcisFlags);
        [PreserveSig]
        int GetItemState(IShellItem psi, int nstcisMask, out int pnstcisFlags);
        [PreserveSig]
        int GetSelectedItems(out /*IShellItemArray*/ IntPtr psiaItems);
        [PreserveSig]
        int GetItemCustomState(IShellItem psi, out int piStateNumber);
        [PreserveSig]
        int SetItemCustomState(IShellItem psi, int iStateNumber);
        [PreserveSig]
        int EnsureItemVisible(IShellItem psi);
        [PreserveSig]
        int SetTheme(string pszTheme);
        [PreserveSig]
        int GetNextItem(IShellItem psi, int nstcgi, out IShellItem ppsiNext);
        [PreserveSig]
        int HitTest([In] ref Point ppt, out IShellItem ppsiOut);
        [PreserveSig]
        int GetItemRect(IShellItem psi, out RECT prect);
        [PreserveSig]
        int CollapseAll();
    }
}
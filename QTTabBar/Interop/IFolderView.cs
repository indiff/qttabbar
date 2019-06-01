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
    [ComImport, SuppressUnmanagedCodeSecurity, Guid("cde725b0-ccc9-4519-917e-325d72fab4ce"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderView {
        [PreserveSig]
        int GetCurrentViewMode(ref FVM pViewMode);
        [PreserveSig]
        int SetCurrentViewMode(FVM ViewMode);
        [PreserveSig]
        int GetFolder(ref Guid riid, out IPersistFolder2 ppv);
        [PreserveSig]
        int Item(int iItemIndex, out IntPtr ppidl);
        [PreserveSig]
        int ItemCount(SVGIO uFlags, out int pcItems);
        [PreserveSig]
        int Items(SVGIO uFlags, ref Guid riid, out IEnumIDList ppv);
        [PreserveSig]
        int GetSelectionMarkedItem(out int piItem);
        [PreserveSig]
        int GetFocusedItem(out int piItem);
        [PreserveSig]
        int GetItemPosition(IntPtr pidl, out Point ppt);
        [PreserveSig]
        int GetSpacing(ref Point ppt);
        [PreserveSig]
        int GetDefaultSpacing(ref Point ppt);
        [PreserveSig]
        int GetAutoArrange();
        [PreserveSig]
        int SelectItem(int iItem, SVSIF dwFlags);
        [PreserveSig]
        int SelectAndPositionItems(uint cidl, IntPtr apidl, IntPtr apt, SVSIF dwFlags);
    }
}

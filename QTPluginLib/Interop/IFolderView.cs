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

namespace QTPlugin.Interop {
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("cde725b0-ccc9-4519-917e-325d72fab4ce")]
    public interface IFolderView {
        [PreserveSig]
        int GetCurrentViewMode(ref FOLDERVIEWMODE pViewMode);
        [PreserveSig]
        int SetCurrentViewMode(FOLDERVIEWMODE ViewMode);
        [PreserveSig]
        int GetFolder(ref Guid riid, out IPersistFolder2 ppv);
        [PreserveSig]
        int Item(int iItemIndex, out IntPtr ppidl);
        [PreserveSig]
        int ItemCount(uint uFlags, out int pcItems);
        [PreserveSig]
        int Items(uint uFlags, [In] ref Guid riid, out object ppv);
        [PreserveSig]
        int GetSelectionMarkedItem(out int piItem);
        [PreserveSig]
        int GetFocusedItem(out int piItem);
        [PreserveSig]
        int GetItemPosition(IntPtr pidl, out POINT ppt);
        [PreserveSig]
        int GetSpacing(ref POINT ppt);
        [PreserveSig]
        int GetDefaultSpacing(ref POINT ppt);
        [PreserveSig]
        int GetAutoArrange();
        [PreserveSig]
        int SelectItem(int iItem, int dwFlags);
        [PreserveSig]
        int SelectAndPositionItems(uint cidl, IntPtr apidl, IntPtr apt, int dwFlags);
    }
}

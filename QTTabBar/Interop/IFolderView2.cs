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
    [ComImport, SuppressUnmanagedCodeSecurity, Guid("1af3a467-214f-4298-908e-06b03e0b39f9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderView2 {
        [PreserveSig]
        int GetCurrentViewMode(ref int pViewMode);
        [PreserveSig]
        int SetCurrentViewMode(int ViewMode);
        [PreserveSig]
        int GetFolder(ref Guid riid, out IPersistFolder2 ppv);
        [PreserveSig]
        int Item(int iItemIndex, out IntPtr ppidl);
        [PreserveSig]
        int ItemCount(uint uFlags, out int pcItems);
        [PreserveSig]
        int Items(uint uFlags, ref Guid riid, out IEnumIDList ppv);
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
        int SelectItem(int iItem, uint dwFlags);
        [PreserveSig]
        int SelectAndPositionItems(uint cidl, IntPtr apidl, IntPtr apt, int dwFlags);
        [PreserveSig]
        int SetGroupBy(ref int key, bool fAscending);
        [PreserveSig]
        int GetGroupBy(out int pkey, out bool pfAscending);
        [PreserveSig]
        /* NOT DECLARED */ int SetViewProperty(IntPtr pidl, ref int propkey, ref object propvar); // ?
        [PreserveSig]
        /* NOT DECLARED */ int GetViewProperty(IntPtr pidl, ref int propkey, out object propvar); // ?
        [PreserveSig]
        int SetTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);
        [PreserveSig]
        int SetExtendedTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);
        [PreserveSig]
        int SetText(int iType, [MarshalAs(UnmanagedType.LPWStr)] string pwszText);
        [PreserveSig]
        int SetCurrentFolderFlags(int dwMask, int dwFlags);
        [PreserveSig]
        int GetCurrentFolderFlags(out int pdwFlags);
        [PreserveSig]
        int GetSortColumnCount(out int pcColumns);
        [PreserveSig]
        /* NOT DECLARED */ int SetSortColumns(/*ref SORTCOLUMN rgSortColumns, int cColumns*/);
        [PreserveSig]
        /* NOT DECLARED */ int GetSortColumns(/*out SORTCOLUMN, int cColumns*/);
        [PreserveSig]
        int GetItem(int iItem, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        [PreserveSig]
        int GetVisibleItem(int iStart, bool fPrevious, out int piItem);
        [PreserveSig]
        int GetSelectedItem(int iStart, out int piItem);
        [PreserveSig]
        /* NOT DECLARED */ int GetSelection(/* bool fNoneImpliesFolder, out IShellItemArray ppsia */);
        [PreserveSig]
        int GetSelectionState(IntPtr pidl, out int pdwFlags);
        [PreserveSig]
        int InvokeVerbOnSelection([MarshalAs(UnmanagedType.LPWStr)] string pszVerb);
        [PreserveSig]
        int SetViewModeAndIconSize(int uViewMode, int iImageSize);
        [PreserveSig]
        int GetViewModeAndIconSize(out int puViewMode, out int piImageSize);
        [PreserveSig]
        int SetGroupSubsetCount(uint cVisibleRows);
        [PreserveSig]
        int GetGroupSubsetCount(out uint pcVisibleRows);
        [PreserveSig]
        int SetRedraw(bool fRedrawOn);
        [PreserveSig]
        int IsMoveInSameFolder();
        [PreserveSig]
        int DoRename();
    }
}
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
    [ComImport, Guid("93F2F68C-1D1B-11d3-A30E-00C04F79ABD1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellFolder2 {
        [PreserveSig]
        int ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, ref uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
        [PreserveSig]
        int EnumObjects(IntPtr hwnd, int grfFlags, out IEnumIDList ppenumIDList);
        [PreserveSig]
        int BindToObject(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);
        [PreserveSig]
        int BindToStorage(IntPtr pidl, IntPtr pbc, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);
        [PreserveSig]
        int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
        [PreserveSig]
        int CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);
        [PreserveSig]
        int GetAttributesOf(uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0, SizeConst = 0)] IntPtr[] apidl, ref uint rgfInOut);
        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1, SizeConst = 0)] IntPtr[] apidl, [In] ref Guid riid, ref uint rgfReserved, [MarshalAs(UnmanagedType.Interface)] out object ppv);
        [PreserveSig]
        int GetDisplayNameOf(IntPtr pidl, uint uFlags, out STRRET pName);
        [PreserveSig]
        int SetNameOf(IntPtr hwndOwner, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
        [PreserveSig]
        int GetDefaultSearchGUID(out Guid pguid);
        [PreserveSig]
        int EnumSearches(out IntPtr ppenum);
        [PreserveSig]
        int GetDefaultColumn(int dwRes, out uint pSort, out uint pDisplay);
        [PreserveSig]
        int GetDefaultColumnState(int iColumn, out uint pcsFlags);
        [PreserveSig]
        int GetDetailsEx(IntPtr pidl, [In] ref SHCOLUMNID pscid, out VARIANT pv);
        [PreserveSig]
        int GetDetailsOf(IntPtr pidl, int iColumn, out SHELLDETAILS psd);
        [PreserveSig]
        int MapColumnToSCID(int iColumn, out SHCOLUMNID pscid);
    }
}

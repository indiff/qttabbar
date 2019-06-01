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

using System.Runtime.InteropServices;
using System.Security;

namespace QTTabBarLib.Interop {
    [ComImport, Guid("7EBFDD80-AD18-11d3-A4C5-00C04F72D6B8"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITravelLogStg {
        [PreserveSig]
        int CreateEntry([In, MarshalAs(UnmanagedType.BStr)] string pszUrl, [In, MarshalAs(UnmanagedType.BStr)] string pszTitle, [In, MarshalAs(UnmanagedType.Interface)] ITravelLogEntry ptleRelativeTo, [In] bool fPrepend, [MarshalAs(UnmanagedType.Interface)] out ITravelLogEntry pptle);
        [PreserveSig]
        int TravelTo([In, MarshalAs(UnmanagedType.Interface)] ITravelLogEntry ptle);
        [PreserveSig]
        int EnumEntries([In] int flags, [MarshalAs(UnmanagedType.Interface)] out IEnumTravelLogEntry ppenum);
        [PreserveSig]
        int FindEntries([In] int flags, [In, MarshalAs(UnmanagedType.BStr)] string pszUrl, [MarshalAs(UnmanagedType.Interface)] out IEnumTravelLogEntry ppenum);
        [PreserveSig]
        int GetCount([In] int flags, out int pcEntries);
        [PreserveSig]
        int RemoveEntry([In, MarshalAs(UnmanagedType.Interface)] ITravelLogEntry ptle);
        [PreserveSig]
        int GetRelativeEntry([In] int iOffset, [MarshalAs(UnmanagedType.Interface)] out ITravelLogEntry ptle);
    }
}

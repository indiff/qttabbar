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
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("7EBFDD85-AD18-11d3-A4C5-00C04F72D6B8")]
    public interface IEnumTravelLogEntry {
        [PreserveSig]
        int Next([In] uint cElt, [MarshalAs(UnmanagedType.Interface)] out ITravelLogEntry rgElt, [Out] uint pcEltFetched);
        [PreserveSig]
        int Skip([In] uint cElt);
        [PreserveSig]
        int Reset();
        [PreserveSig]
        int Clone([MarshalAs(UnmanagedType.Interface)] out IEnumTravelLogEntry ppEnum);
    }
}

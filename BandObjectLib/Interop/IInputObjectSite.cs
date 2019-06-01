//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2002-2010  Pavel Zolnikov, Quizo, Paul Accisano
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

namespace BandObjectLib {
    [ComImport, SuppressUnmanagedCodeSecurity, Guid("f1db8392-7331-11d0-8c99-00a0c92dbfe8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInputObjectSite {
        [PreserveSig]
        int OnFocusChangeIS([MarshalAs(UnmanagedType.IUnknown)] object punkObj, int fSetFocus);
    }
}

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

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BandObjectLib {
    [ComImport, Guid("012dd920-7b26-11d0-8ca9-00a0c92dbfe8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    internal interface IDockingWindow {
        void GetWindow(out IntPtr phwnd);
        void ContextSensitiveHelp([In] bool fEnterMode);
        void ShowDW([In] bool fShow);
        void CloseDW([In] uint dwReserved);
        void ResizeBorderDW(IntPtr prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] object punkToolbarSite, bool fReserved);
    }
}

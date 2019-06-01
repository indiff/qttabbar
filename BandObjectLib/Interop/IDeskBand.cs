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
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("EB0FE172-1A3A-11D0-89B3-00A0C90A90AC"), SuppressUnmanagedCodeSecurity]
    internal interface IDeskBand {
        void GetWindow(out IntPtr phwnd);
        void ContextSensitiveHelp([In] bool fEnterMode);
        void ShowDW([In] bool fShow);
        void CloseDW([In] uint dwReserved);
        void ResizeBorderDW(IntPtr prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] object punkToolbarSite, bool fReserved);
        void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi);
    }
}

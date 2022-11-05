//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
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
using BandObjectLib;

namespace QTTabBarLib.Interop {
    [ComImport, 
     SuppressUnmanagedCodeSecurity,
     Guid("79D16DE4-ABEE-4021-8D9D-9169B261D657"), 
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDeskBand2 {
        // IOleWindow methods
        [PreserveSig]
        void GetWindow(out IntPtr phwnd);

        [PreserveSig]
        void ContextSensitiveHelp([In] bool fEnterMode);

        // IDockingWindow methods
        [PreserveSig]
        void ShowDW([In] bool fShow);
        [PreserveSig]
        void CloseDW([In] uint dwReserved);
        [PreserveSig]
        void ResizeBorderDW(IntPtr prcBorder, [In, MarshalAs(UnmanagedType.IUnknown)] object punkToolbarSite, bool fReserved);

        // IDeskBand methods
        [PreserveSig]
        void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi);

        // IDeskband2 methods
        void CanRenderComposited(out bool pfCanRenderComposited);
        void SetCompositionState(bool fCompositionEnabled);
        void GetCompositionState(out bool pfCompositionEnabled);
    }
}

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
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E3-0000-0000-C000-000000000046")]
    public interface IShellView {
        [PreserveSig]
        int GetWindow(out IntPtr phwnd);
        [PreserveSig]
        int ContextSensitiveHelp(bool fEnterMode);
        [PreserveSig]
        int TranslateAccelerator(ref MSG pmsg);
        [PreserveSig]
        int EnableModeless(bool fEnable);
        [PreserveSig]
        int UIActivate(uint uState);
        [PreserveSig]
        int Refresh();
        [PreserveSig]
        int CreateViewWindow(IShellView psvPrevious, ref FOLDERSETTINGS pfs, ref IShellBrowser psb, ref RECT prcView, out IntPtr phWnd);
        [PreserveSig]
        int DestroyViewWindow();
        [PreserveSig]
        int GetCurrentInfo(ref FOLDERSETTINGS lpfs);
        [PreserveSig]
        int AddPropertySheetPages(int dwReserved, IntPtr pfn, IntPtr lparam);
        [PreserveSig]
        int SaveViewState();
        [PreserveSig]
        int SelectItem(IntPtr pidlItem, uint uFlags);
        [PreserveSig]
        int GetItemObject(uint uItem, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }
}

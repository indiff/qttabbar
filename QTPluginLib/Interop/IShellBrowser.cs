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
    [ComImport, Guid("000214E2-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellBrowser {
        [PreserveSig]
        int GetWindow(out IntPtr phwnd);
        [PreserveSig]
        int ContextSensitiveHelp(bool fEnterMode);
        [PreserveSig]
        int InsertMenusSB(IntPtr hmenuShared, IntPtr lpMenuWidths);
        [PreserveSig]
        int SetMenuSB(IntPtr hmenuShared, IntPtr holemenuRes, IntPtr hwndActiveObject);
        [PreserveSig]
        int RemoveMenusSB(IntPtr hmenuShared);
        [PreserveSig]
        int SetStatusTextSB([MarshalAs(UnmanagedType.BStr)] string pszStatusText);
        [PreserveSig]
        int EnableModelessSB(bool fEnable);
        [PreserveSig]
        int TranslateAcceleratorSB(ref MSG pmsg, ushort wID);
        [PreserveSig]
        int BrowseObject(IntPtr pidl, uint wFlags);
        [PreserveSig]
        int GetViewStateStream(uint grfMode, IntPtr ppStrm);
        [PreserveSig]
        int GetControlWindow(uint id, out IntPtr phwnd);
        [PreserveSig]
        int SendControlMsg(uint id, uint uMsg, IntPtr wParam, IntPtr lParam, out IntPtr pret);
        [PreserveSig]
        int QueryActiveShellView(out IShellView ppshv);
        [PreserveSig]
        int OnViewWindowActive(IShellView pshv);
        [PreserveSig]
        int SetToolbarItems(IntPtr lpButtons, uint nButtons, uint uFlags);
    }
}

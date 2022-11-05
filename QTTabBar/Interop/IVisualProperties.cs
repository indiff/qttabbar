//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano, indiff
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace QTTabBarLib.Interop
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("e693cf68-d967-4112-8763-99172aee5e5a")]
  [SuppressUnmanagedCodeSecurity]
  [ComImport]
  internal interface IVisualProperties
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetWatermark(IntPtr hbmp, VPWATERMARKFLAGS vpwf);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetColor(VPCOLORFLAGS vpcf, int cr);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetColor(VPCOLORFLAGS vpcf, out int pcr);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetItemHeight(int cyItemInPixels);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetItemHeight(out int cyItemInPixels);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetFont(IntPtr plf, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetFont(out IntPtr plf);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int SetTheme([MarshalAs(UnmanagedType.LPWStr)] string pszSubAppName, [MarshalAs(UnmanagedType.LPWStr)] string pszSubIdList);
  }
}

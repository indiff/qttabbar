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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace QTTabBarLib.Interop {
    [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1")]
    public interface IExtractImage {
        [PreserveSig]
        int GetLocation(StringBuilder pszPathBuffer, int cch, ref int pdwPriority, ref Size prgSize, int dwRecClrDepth, ref int pdwFlags);
        [PreserveSig]
        int Extract(out IntPtr phBmpThumbnail);
    }
}

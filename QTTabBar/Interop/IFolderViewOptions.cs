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
    [ComImport, SuppressUnmanagedCodeSecurity, Guid("3cc974d2-b302-4d36-ad3e-06d93f695d3f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderViewOptions {
        [PreserveSig]
        int SetFolderViewOptions(int fvoMask, int fvoFlags);
        [PreserveSig]
        int GetFolderViewOptions(out int pfvoFlags);
    }

    public static class FVO {
        public const int DEFAULT            = 0x00;
        public const int VISTALAYOUT        = 0x01;
        public const int CUSTOMPOSITION     = 0x02;
        public const int CUSTOMORDERING     = 0x04;
        public const int SUPPORTHYPERLINKS  = 0x08;
        public const int NOANIMATIONS       = 0x10;
        public const int NOSCROLLTIPS       = 0x20;
    }
}

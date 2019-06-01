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
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("F676C15D-596A-4ce2-8234-33996F445DB1")]
    public interface IThumbnailCache {
        [PreserveSig]
        int GetThumbnail(IShellItem pShellItem, uint cxyRequestedThumbSize, uint flags, out ISharedBitmap ppvThumb, ref uint pOutFlags, [In, Out] ref WTS_THUMBNAILID pThumbnailID);
        [PreserveSig]
        int GetThumbnailByID(ref WTS_THUMBNAILID thumbnailID, uint cxyRequestedThumbSize, out ISharedBitmap ppvThumb, ref uint pOutFlags);
    }
}

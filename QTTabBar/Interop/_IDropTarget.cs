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

using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace QTTabBarLib.Interop {
    [ComImport, Guid("00000122-0000-0000-C000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface _IDropTarget {
        [PreserveSig]
        int DragEnter(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect);
        [PreserveSig]
        int DragOver(int grfKeyState, Point pt, ref DragDropEffects pdwEffect);
        [PreserveSig]
        int DragLeave();
        [PreserveSig]
        int DragDrop(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect);
    }
}

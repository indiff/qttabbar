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

namespace QTTabBarLib.Interop {
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("37A378C0-F82D-11CE-AE65-08002B2E1262"), SuppressUnmanagedCodeSecurity]
    public interface IShellFolderView {
        [PreserveSig]
        int Rearrange(IntPtr lParamSort);
        [PreserveSig]
        int GetArrangeParam(out IntPtr plParamSort);
        void ArrangeGrid();
        void AutoArrange();
        void GetAutoArrange();
        [PreserveSig]
        int AddObject(IntPtr pidl, out int puItem);
        [PreserveSig]
        int GetObject(out IntPtr ppidl, int uItem);
        [PreserveSig]
        int RemoveObject(IntPtr pidl, out int puItem);
        void GetObjectCount(out int puCount);
        void SetObjectCount(int uCount, int dwFlags);
        void UpdateObject(IntPtr pidlOld, IntPtr pidlNew, out int puItem);
        void RefreshObject(IntPtr pidl, out int puItem);
        [PreserveSig]
        int SetRedraw(bool bRedraw);
        void GetSelectedCount(out int puSelected);
        void GetSelectedObjects(out IntPtr pppidl, out int puItems);
        void IsDropOnSource(IntPtr pDropTarget);
        void GetDragPoint(ref Point ppt);
        void GetDropPoint(ref Point ppt);
        void MoveIcons(IntPtr pDataObject);
        void SetItemPos(IntPtr pidl, ref Point ppt);
        void IsBkDropTarget(IntPtr pDropTarget);
        void SetClipboard(bool bMove);
        void SetPoints(ref IntPtr pDataObject);
        void GetItemSpacing(IntPtr pSpacing);
        void SetCallback(IntPtr pNewCB, out IntPtr ppOldCB);
        void Select(int dwFlags);
        void QuerySupport(IntPtr pdwSupport);
        void SetAutomationObject(IntPtr pdisp);
    }
}

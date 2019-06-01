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
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using BandObjectLib;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace QTTabBarLib.Interop {
    public class DropTargetWrapper : _IDropTarget, IDisposable {
        private const short CF_HDROP = 15;
        private IntPtr hwnd;
        private int iLastKeyState;
        private const int MK_ALT = 0x20;
        private const int MK_CONTROL = 8;
        private const int MK_LBUTTON = 1;
        private const int MK_RBUTTON = 2;
        private const int MK_SHIFT = 4;

        public event EventHandler DragDropEnd;

        public event DragFileDropEventHandler DragFileDrop;

        public event DragFileEnterEventHandler DragFileEnter;

        public event EventHandler DragFileLeave;

        public event DragEventHandler DragFileOver;

        public DropTargetWrapper(Control ctrl) {
            hwnd = ctrl.Handle;
            PInvoke.RegisterDragDrop(hwnd, this);
        }

        public void Dispose() {
            if(hwnd != IntPtr.Zero) {
                PInvoke.RevokeDragDrop(hwnd);
                hwnd = IntPtr.Zero;
            }
        }

        public int DragDrop(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
            try {
                if(DragFileOver != null) {
                    DragEventArgs e = new DragEventArgs(null, grfKeyState, pt.X, pt.Y, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll, pdwEffect);
                    DragFileOver(null, e);
                    pdwEffect = e.Effect;
                }
                else {
                    pdwEffect = DragDropEffects.Copy;
                }
                if(pdwEffect != DragDropEffects.None) {
                    if(DragFileDrop != null) {
                        IntPtr ptr;
                        byte[] buffer;
                        switch(DragFileDrop(out ptr, out buffer)) {
                            case -1:
                                return 0;

                            case 0: {
                                    IShellFolder ppv = null;
                                    object obj2 = null;
                                    Guid riid = ExplorerGUIDs.IID_IShellFolder;
                                    Guid guid2 = ExplorerGUIDs.IID_IDropTarget;
                                    using(IDLWrapper wrapper = new IDLWrapper(buffer)) {
                                        if(wrapper.Available && wrapper.IsDropTarget) {
                                            try {
                                                IntPtr ptr2;
                                                if(PInvoke.SHBindToParent(wrapper.PIDL, riid, out ppv, out ptr2) == 0) {
                                                    uint rgfReserved = 0;
                                                    IntPtr[] apidl = new IntPtr[] { ptr2 };
                                                    if(ppv.GetUIObjectOf(ptr, 1, apidl, ref guid2, ref rgfReserved, out obj2) == 0) {
                                                        _IDropTarget target = obj2 as _IDropTarget;
                                                        if(target != null) {
                                                            DragDropEffects effects = pdwEffect;
                                                            if(target.DragEnter(pDataObj, iLastKeyState, pt, ref effects) == 0) {
                                                                effects = pdwEffect;
                                                                if(target.DragOver(iLastKeyState, pt, ref effects) == 0) {
                                                                    if((iLastKeyState & 2) != 0) {
                                                                        pdwEffect = DragDropEffects.Link | DragDropEffects.Move | DragDropEffects.Copy;
                                                                    }
                                                                    return target.DragDrop(pDataObj, iLastKeyState, pt, ref pdwEffect);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch {
                                            }
                                            finally {
                                                if(ppv != null) {
                                                    Marshal.ReleaseComObject(ppv);
                                                }
                                                if(obj2 != null) {
                                                    Marshal.ReleaseComObject(obj2);
                                                }
                                                if(DragDropEnd != null) {
                                                    DragDropEnd(this, EventArgs.Empty);
                                                }
                                            }
                                        }
                                    }
                                    return 0;
                                }
                        }
                    }
                    FORMATETC format = new FORMATETC();
                    format.cfFormat = 15;
                    format.ptd = IntPtr.Zero;
                    format.dwAspect = DVASPECT.DVASPECT_CONTENT;
                    format.lindex = -1;
                    format.tymed = TYMED.TYMED_HGLOBAL;
                    STGMEDIUM medium = new STGMEDIUM();
                    try {
                        pDataObj.GetData(ref format, out medium);
                        PInvoke.SendMessage(hwnd, 0x233, medium.unionmember, IntPtr.Zero);
                    }
                    catch {
                    }
                    finally {
                        PInvoke.ReleaseStgMedium(ref medium);
                    }
                }
            }
            finally {
                if(pDataObj != null) {
                    Marshal.FinalReleaseComObject(pDataObj);
                }
            }
            return 0;
        }

        public int DragEnter(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
            try {
                if(DragFileEnter != null) {
                    FORMATETC format = new FORMATETC();
                    format.cfFormat = 15;
                    format.ptd = IntPtr.Zero;
                    format.dwAspect = DVASPECT.DVASPECT_CONTENT;
                    format.lindex = -1;
                    format.tymed = TYMED.TYMED_HGLOBAL;
                    if(pDataObj.QueryGetData(ref format) >= 0) {
                        STGMEDIUM medium = new STGMEDIUM();
                        try {
                            try {
                                pDataObj.GetData(ref format, out medium);
                                pdwEffect = DragFileEnter(medium.unionmember, pt, grfKeyState);
                            }
                            catch {
                                pdwEffect = DragDropEffects.None;
                            }
                            goto Label_00A0;
                        }
                        finally {
                            PInvoke.ReleaseStgMedium(ref medium);
                        }
                    }
                    pdwEffect = DragDropEffects.None;
                }
                else {
                    pdwEffect = DragDropEffects.Copy;
                }
            }
            finally {
                if(pDataObj != null) {
                    Marshal.FinalReleaseComObject(pDataObj);
                }
            }
        Label_00A0:
            return 0;
        }

        public int DragLeave() {
            if(DragFileLeave != null) {
                DragFileLeave(null, EventArgs.Empty);
            }
            return 0;
        }

        public int DragOver(int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
            iLastKeyState = grfKeyState;
            if(DragFileOver != null) {
                DragEventArgs e = new DragEventArgs(null, grfKeyState, pt.X, pt.Y, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll, DragDropEffects.None);
                DragFileOver(null, e);
                pdwEffect = e.Effect;
            }
            else {
                pdwEffect = DragDropEffects.Copy;
            }
            return 0;
        }

        public static DragDropEffects MakeEffect(int grfKeyState, int iSourceState) {
            switch((grfKeyState & 0x2c)) {
                case 12:
                case 0x20:
                    return DragDropEffects.Link;

                case 4:
                    if(iSourceState == 3) {
                        return DragDropEffects.Copy;
                    }
                    return DragDropEffects.Move;

                case 8:
                    return DragDropEffects.Copy;
            }
            if((iSourceState != 1) && (iSourceState != 3)) {
                return DragDropEffects.Move;
            }
            return DragDropEffects.Copy;
        }

        public delegate int DragFileDropEventHandler(out IntPtr hwnd, out byte[] idlReal);

        public delegate DragDropEffects DragFileEnterEventHandler(IntPtr hDrop, Point pnt, int grfKeyState);
    }
}

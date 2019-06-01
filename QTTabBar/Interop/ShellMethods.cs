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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using BandObjectLib;

namespace QTTabBarLib.Interop {
    public static class ShellMethods {
        public static bool ClipboardContainsFileDropList(IntPtr hwnd) {
            if((hwnd != IntPtr.Zero) && PInvoke.OpenClipboard(hwnd)) {
                try {
                    for(uint i = PInvoke.EnumClipboardFormats(0); i != 0; i = PInvoke.EnumClipboardFormats(i)) {
                        if(i == 15) {
                            return true;
                        }
                    }
                }
                finally {
                    PInvoke.CloseClipboard();
                }
            }
            return false;
        }

        private static List<string> ClipboardGetFileDropList(IntPtr hwnd) {
            List<string> list = new List<string>();
            if(PInvoke.OpenClipboard(hwnd)) {
                try {
                    IntPtr clipboardData = PInvoke.GetClipboardData(15);
                    if(!(clipboardData != IntPtr.Zero)) {
                        return list;
                    }
                    IntPtr hDrop = PInvoke.GlobalLock(clipboardData);
                    if(!(hDrop != IntPtr.Zero)) {
                        return list;
                    }
                    try {
                        uint num = PInvoke.DragQueryFile(hDrop, uint.MaxValue, null, 0);
                        if(num > 0) {
                            for(uint i = 0; i < num; i++) {
                                StringBuilder lpszFile = new StringBuilder(260);
                                PInvoke.DragQueryFile(hDrop, i, lpszFile, lpszFile.Capacity);
                                if(lpszFile.Length > 0) {
                                    list.Add(lpszFile.ToString());
                                }
                            }
                        }
                        return list;
                    }
                    finally {
                        PInvoke.GlobalUnlock(clipboardData);
                    }
                }
                finally {
                    PInvoke.CloseClipboard();
                }
            }
            return list;
        }

        public static IntPtr CreateIDL(byte[] data) {
            if((data == null) || (data.Length == 0)) {
                return IntPtr.Zero;
            }
            int length = data.Length;
            IntPtr zero = IntPtr.Zero;
            try {
                zero = Marshal.AllocCoTaskMem(length);
            }
            catch {
                return zero;
            }
            Marshal.Copy(data, 0, zero, length);
            return zero;
        }

        public static bool DeleteFile(List<string> lstPaths, bool fShiftKey, IntPtr hwnd) {
            string s = MakeFILEOPPATHS(lstPaths);
            if(s.Length > 1) {
                using(SafePtr pFrom = new SafePtr(s)) {
                    SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT {
                        hwnd = hwnd,
                        wFunc = 3,
                        pFrom = pFrom,
                        fFlags = fShiftKey ? ((short)0x4000) : ((short)0x40)
                    };
                    try {
                        if(PInvoke.SHFileOperation(ref lpFileOp) == 0) {
                            return !lpFileOp.fAnyOperationsAborted;
                        }
                    }
                    catch(Exception exception) {
                        QTUtility2.MakeErrorLog(exception);
                    }                    
                }
            }
            return false;
        }

        public static DragDropEffects DoDragDrop(string path, Control control) {
            DragDropEffects none;
            if(string.IsNullOrEmpty(path)) {
                return DragDropEffects.None;
            }
            IShellFolder ppv = null;
            object obj2 = null;
            IntPtr zero = IntPtr.Zero;
            try {
                IntPtr ptr2;
                zero = PInvoke.ILCreateFromPath(path);
                if(((zero == IntPtr.Zero) || (PInvoke.SHBindToParent(zero, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr2) != 0)) || (ppv == null)) {
                    return DragDropEffects.None;
                }
                IntPtr[] apidl = new IntPtr[] { ptr2 };
                uint rgfReserved = 0;
                Guid riid = ExplorerGUIDs.IID_IDataObject;
                ppv.GetUIObjectOf(IntPtr.Zero, (uint)apidl.Length, apidl, ref riid, ref rgfReserved, out obj2);
                if(obj2 != null) {
                    return control.DoDragDrop(obj2, DragDropEffects.Link | DragDropEffects.Move | DragDropEffects.Copy);
                }
                none = DragDropEffects.None;
            }
            finally {
                if(obj2 != null) {
                    Marshal.ReleaseComObject(obj2);
                }
                if(ppv != null) {
                    Marshal.ReleaseComObject(ppv);
                }
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
            }
            return none;
        }

        public static DragDropEffects DoDragDrop(List<string> lstPaths, Control control, bool fSameDirecotry) {
            DragDropEffects none;
            if((lstPaths == null) || (lstPaths.Count == 0)) {
                return DragDropEffects.None;
            }
            IShellFolder ppv = null;
            object obj2 = null;
            List<IntPtr> list = new List<IntPtr>();
            try {
                IntPtr[] ptrArray;
                if(fSameDirecotry) {
                    IntPtr ptr2;
                    IntPtr item = PInvoke.ILCreateFromPath(lstPaths[0]);
                    list.Add(item);
                    if((PInvoke.SHBindToParent(item, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr2) != 0) || (ppv == null)) {
                        return DragDropEffects.None;
                    }
                    List<IntPtr> list2 = new List<IntPtr>();
                    if(ptr2 != IntPtr.Zero) {
                        list2.Add(ptr2);
                    }
                    for(int i = 1; i < lstPaths.Count; i++) {
                        IntPtr ptr3 = PInvoke.ILCreateFromPath(lstPaths[i]);
                        if(ptr3 != IntPtr.Zero) {
                            list.Add(ptr3);
                            IntPtr ptr4 = PInvoke.ILFindLastID(ptr3);
                            if(ptr4 != IntPtr.Zero) {
                                list2.Add(ptr4);
                            }
                        }
                    }
                    if(list2.Count == 0) {
                        return DragDropEffects.None;
                    }
                    ptrArray = list2.ToArray();
                }
                else {
                    list.AddRange(lstPaths.Select(PInvoke.ILCreateFromPath).Where(ptr5 => ptr5 != IntPtr.Zero));
                    PInvoke.SHGetDesktopFolder(out ppv);
                    if((list.Count == 0) || (ppv == null)) {
                        return DragDropEffects.None;
                    }
                    ptrArray = list.ToArray();
                }
                uint rgfReserved = 0;
                Guid riid = ExplorerGUIDs.IID_IDataObject;
                ppv.GetUIObjectOf(IntPtr.Zero, (uint)ptrArray.Length, ptrArray, ref riid, ref rgfReserved, out obj2);
                if(obj2 != null) {
                    return control.DoDragDrop(obj2, DragDropEffects.Link | DragDropEffects.Move | DragDropEffects.Copy);
                }
                none = DragDropEffects.None;
            }
            finally {
                if(obj2 != null) {
                    Marshal.ReleaseComObject(obj2);
                }
                if(ppv != null) {
                    Marshal.ReleaseComObject(ppv);
                }
                foreach(IntPtr ptr6 in list) {
                    if(ptr6 != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(ptr6);
                    }
                }
            }
            return none;
        }

        public static uint GetAttributes(IntPtr pIDL, bool fValidate) {
            if(pIDL != IntPtr.Zero) {
                uint rgfInOut = 0x60010100;
                if(fValidate) {
                    rgfInOut |= 0x1000000;
                }
                Guid riid = ExplorerGUIDs.IID_IShellFolder;
                IShellFolder ppv = null;
                try {
                    IntPtr ptr;
                    if(PInvoke.SHBindToParent(pIDL, riid, out ppv, out ptr) == 0) {
                        IntPtr[] apidl = new IntPtr[] { ptr };
                        int hr = ppv.GetAttributesOf(1, apidl, ref rgfInOut);
                        if(fValidate) {
                            return (hr >= 0) ? 0 : uint.MaxValue;
                        }
                        if(hr >= 0) {
                            return rgfInOut;
                        }
                    }
                    return uint.MaxValue;
                }
                catch {
                }
                finally {
                    if(ppv != null) {
                        Marshal.ReleaseComObject(ppv);
                    }
                }
            }
            return uint.MaxValue;
        }

        public static string GetDisplayName(string path) {
            using(IDLWrapper wrapper = new IDLWrapper(path)) {
                if(wrapper.Available) {
                    return GetDisplayName(wrapper.PIDL, true);
                }
            }
            return path;
        }

        public static string GetDisplayName(IntPtr pIDL, bool fDisplayName) {
            IShellFolder ppv = null;
            Guid riid = ExplorerGUIDs.IID_IShellFolder;
            try {
                IntPtr ptr;
                STRRET strret;
                if((PInvoke.SHBindToParent(pIDL, riid, out ppv, out ptr) != 0) || (ppv == null)) {
                    return string.Empty;
                }
                uint uFlags = fDisplayName ? 0 : 0x8000u;
                StringBuilder pszBuf = new StringBuilder(260);
                if(ppv.GetDisplayNameOf(ptr, uFlags, out strret) == 0) {
                    PInvoke.StrRetToBuf(ref strret, ptr, pszBuf, pszBuf.Capacity);
                }
                return pszBuf.ToString();
            }
            catch {
            }
            finally {
                if(ppv != null) {
                    Marshal.ReleaseComObject(ppv);
                }
            }
            return string.Empty;
        }

        public static string GetDisplayName(IShellFolder shellFolder, IntPtr pIDLLast, bool fDisplayName) {
            STRRET strret;
            uint uFlags = fDisplayName ? 0 : 0x8000u;
            StringBuilder pszBuf = new StringBuilder(260);
            if(shellFolder.GetDisplayNameOf(pIDLLast, uFlags, out strret) == 0) {
                PInvoke.StrRetToBuf(ref strret, pIDLLast, pszBuf, pszBuf.Capacity);
            }
            return pszBuf.ToString();
        }

        public static string GetFolderPath(int folder) {
            StringBuilder pszPath = new StringBuilder(260);
            PInvoke.SHGetFolderPath(IntPtr.Zero, folder, IntPtr.Zero, 0, pszPath);
            return pszPath.ToString();
        }

        public static byte[] GetIDLData(IntPtr pIDL) {
            uint num = PInvoke.ILGetSize(pIDL);
            if(num != 0) {
                byte[] destination = new byte[num];
                Marshal.Copy(pIDL, destination, 0, (int)num);
                return destination;
            }
            return null;
        }

        public static byte[] GetIDLData(string path) {
            IntPtr pIDL = PInvoke.ILCreateFromPath(path);
            byte[] iDLData = GetIDLData(pIDL);
            PInvoke.CoTaskMemFree(pIDL);
            return iDLData;
        }

        public static IntPtr GetLinkTargetIDL(string lnkPath) {
            _ShellLinkObject o = null;
            try {
                IntPtr ptr;
                o = new _ShellLinkObject();
                ((IPersistFile)o).Load(lnkPath, 0);
                ((IShellLinkW)o).GetIDList(out ptr);
                return ptr;
            }
            catch {
            }
            finally {
                if(o != null) {
                    Marshal.ReleaseComObject(o);
                }
            }
            return IntPtr.Zero;
        }

        public static string GetLinkTargetPath(string lnkPath) {
            _ShellLinkObject o = null;
            try {
                o = new _ShellLinkObject();
                ((IPersistFile)o).Load(lnkPath, 0);
                IShellLinkW kw = (IShellLinkW)o;
                StringBuilder pszFile = new StringBuilder(260);
                kw.GetPath(pszFile, 260, IntPtr.Zero, 0);
                return pszFile.ToString();
            }
            catch {
            }
            finally {
                if(o != null) {
                    Marshal.ReleaseComObject(o);
                }
            }
            return string.Empty;
        }

        public static string GetPath(IntPtr pIDL) {
            if(!(pIDL != IntPtr.Zero)) {
                return string.Empty;
            }
            StringBuilder pszPath = new StringBuilder(260);
            if(PInvoke.SHGetPathFromIDList(pIDL, pszPath)) {
                return pszPath.ToString();
            }
            return GetDisplayName(pIDL, false);
        }

        private static bool GetPrefferdDropEffect(IntPtr hwnd) {
            uint uFormat = PInvoke.RegisterClipboardFormat("Preferred DropEffect");
            bool flag = false;
            if(PInvoke.OpenClipboard(hwnd)) {
                try {
                    IntPtr clipboardData = PInvoke.GetClipboardData(uFormat);
                    if(!(clipboardData != IntPtr.Zero)) {
                        return flag;
                    }
                    IntPtr source = PInvoke.GlobalLock(clipboardData);
                    try {
                        if(source != IntPtr.Zero) {
                            byte[] destination = new byte[4];
                            Marshal.Copy(source, destination, 0, 4);
                            flag = (destination[0] & 2) != 0;
                        }
                    }
                    finally {
                        PInvoke.GlobalUnlock(clipboardData);
                    }
                }
                finally {
                    PInvoke.CloseClipboard();
                }
            }
            return flag;
        }

        public static bool GetShellFolder(IntPtr pIDLFolder, out IShellFolder shellFolder) {
            shellFolder = null;
            if(PInvoke.ILGetSize(pIDLFolder) == 2) {
                return (0 == PInvoke.SHGetDesktopFolder(out shellFolder));
            }
            IShellFolder ppshf = null;
            Guid riid = ExplorerGUIDs.IID_IShellFolder;
            try {
                if(PInvoke.SHGetDesktopFolder(out ppshf) == 0) {
                    return (0 == ppshf.BindToObject(pIDLFolder, IntPtr.Zero, ref riid, out shellFolder));
                }
            }
            catch {
            }
            finally {
                if(ppshf != null) {
                    Marshal.ReleaseComObject(ppshf);
                }
            }
            return false;
        }

        public static string GetShellInfoTipText(IntPtr pIDL, bool fAllowSlow) {
            if(pIDL != IntPtr.Zero) {
                IShellFolder ppv = null;
                IQueryInfo o = null;
                try {
                    IntPtr ptr;
                    if(PInvoke.SHBindToParent(pIDL, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr) == 0) {
                        Guid riid = ExplorerGUIDs.IID_IQueryInfo;
                        IntPtr[] apidl = new IntPtr[] { ptr };
                        uint rgfReserved = 0;
                        object obj2;
                        if(ppv.GetUIObjectOf(IntPtr.Zero, 1, apidl, ref riid, ref rgfReserved, out obj2) == 0) {
                            string str;
                            o = obj2 as IQueryInfo;
                            if((o != null) && (o.GetInfoTip(fAllowSlow ? 8 : 0, out str) == 0)) {
                                return str;
                            }
                        }
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
                finally {
                    if(ppv != null) {
                        Marshal.ReleaseComObject(ppv);
                    }
                    if(o != null) {
                        Marshal.ReleaseComObject(o);
                    }
                }
            }
            return null;
        }

        public static string GetShellInfoTipText(string path, bool fAllowSlow) {
            IntPtr pIDL = PInvoke.ILCreateFromPath(path);
            string shellInfoTipText = null;
            if(pIDL != IntPtr.Zero) {
                shellInfoTipText = GetShellInfoTipText(pIDL, fAllowSlow);
                PInvoke.CoTaskMemFree(pIDL);
            }
            return shellInfoTipText;
        }

        public static string GetSpecialFolderCLSID(int folder, bool fDisplayName) {
            IntPtr ptr;
            if((PInvoke.SHGetFolderLocation(IntPtr.Zero, folder, IntPtr.Zero, 0, out ptr) == 0) && (ptr != IntPtr.Zero)) {
                string displayName = GetDisplayName(ptr, fDisplayName);
                PInvoke.CoTaskMemFree(ptr);
                return displayName;
            }
            return string.Empty;
        }

        public static FileSystemInfo GetTargetIfFolderLink(DirectoryInfo di, out bool fTargetIsDirectory) {
            fTargetIsDirectory = true;
            try {
                if((di.Attributes & FileAttributes.ReadOnly) == 0) {
                    return di;
                }
                FileInfo[] files = di.GetFiles();
                if((files.Length != 2) || ((files[0].Name != "desktop.ini" || files[1].Name != "target.lnk") && (files[0].Name != "target.lnk" || files[1].Name != "desktop.ini"))) {
                    return di;
                }
                string lnkPath = (files[1].Name == "target.lnk") ? files[1].FullName : files[0].FullName;
                string linkTargetPath = GetLinkTargetPath(lnkPath);
                if(string.IsNullOrEmpty(linkTargetPath)) {
                    return di;
                }
                DirectoryInfo info = new DirectoryInfo(linkTargetPath);
                if(info.Exists) {
                    return info;
                }
                if(File.Exists(linkTargetPath)) {
                    fTargetIsDirectory = false;
                    return new FileInfo(linkTargetPath);
                }
            }
            catch {
            }
            return di;
        }

        private static bool IsTargetPathContained(List<string> lstPaths, string pathTarget) {
            foreach(string str in lstPaths) {
                try {
                    if(pathTarget.PathEquals(Path.GetDirectoryName(str))) {
                        return true;
                    }
                }
                catch {
                }
            }
            return false;
        }

        private static string MakeFILEOPPATHS(List<string> paths) {
            return paths.StringJoin("\0") + "\0\0";
        }

        public static bool PasteFile(string pathTarget, IntPtr hwnd) {
            try {
                List<string> paths = ClipboardGetFileDropList(hwnd);
                if(paths.Count == 0) {
                    return false;
                }
                string s = MakeFILEOPPATHS(paths);
                if(s.Length > 1) {
                    bool flag;
                    if(pathTarget.Length != 3) {
                        pathTarget = pathTarget.TrimEnd(new char[] { '\\' });
                    }
                    pathTarget = GetTargetIfFolderLink(new DirectoryInfo(pathTarget), out flag).FullName;
                    if(flag) {
                        bool prefferdDropEffect = GetPrefferdDropEffect(hwnd);
                        short num = 0x40;
                        if(IsTargetPathContained(paths, pathTarget)) {
                            num = (short)(num | 8);
                        }
                        using(SafePtr pFrom = new SafePtr(s))
                        using(SafePtr pTo = new SafePtr(pathTarget + '\0')) {
                            SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT {
                                hwnd = hwnd,
                                wFunc = prefferdDropEffect ? 1 : 2,
                                pFrom = pFrom,
                                pTo = pTo,
                                fFlags = num
                            };
                            try {
                                if(PInvoke.SHFileOperation(ref lpFileOp) == 0) {
                                    return !lpFileOp.fAnyOperationsAborted;
                                }
                            }
                            catch(Exception exception) {
                                QTUtility2.MakeErrorLog(exception);
                            }                            
                        }
                    }
                }
            }
            catch(Exception exception2) {
                QTUtility2.MakeErrorLog(exception2, null);
            }
            return false;
        }

        public static bool PathIsFolder(string targetPath) {
            if(targetPath.StartsWith("::")) {
                using(IDLWrapper wrapper = new IDLWrapper(targetPath)) {
                    return wrapper.IsFolder;
                }
            }
            else {
                return Directory.Exists(targetPath);
            }
        }

        private static IntPtr PtrPlus(IntPtr p, int cOffset) {
            if(IntPtr.Size == 4) {
                return (IntPtr)(((int)p) + cOffset);
            }
            return (IntPtr)(((long)p) + cOffset);
        }

        public static bool SetClipboardFileDropPaths(List<string> lstPaths, bool fCut, IntPtr hwnd) {
            string str = MakeFILEOPPATHS(lstPaths);
            if((hwnd != IntPtr.Zero) && (str.Length > 1)) {
                if(!PInvoke.OpenClipboard(hwnd)) {
                    return false;
                }
                PInvoke.EmptyClipboard();
                try {
                    DROPFILES structure = new DROPFILES();
                    structure.pFiles = Marshal.SizeOf(structure);
                    structure.fWide = true;
                    IntPtr hMem = PInvoke.GlobalAlloc(0x42, (IntPtr)(Marshal.SizeOf(structure) + (str.Length * Marshal.SystemMaxDBCSCharSize)));
                    if(hMem != IntPtr.Zero) {
                        IntPtr ptr = PInvoke.GlobalLock(hMem);
                        Marshal.StructureToPtr(structure, ptr, false);
                        Marshal.Copy(str.ToCharArray(), 0, PtrPlus(ptr, Marshal.SizeOf(structure)), str.Length);
                        PInvoke.GlobalUnlock(hMem);
                    }
                    IntPtr ptr3 = PInvoke.GlobalAlloc(0x42, (IntPtr)4);
                    if(ptr3 != IntPtr.Zero) {
                        IntPtr destination = PInvoke.GlobalLock(ptr3);
                        byte[] source = new byte[4];
                        source[0] = fCut ? ((byte)2) : ((byte)5);
                        Marshal.Copy(source, 0, destination, 4);
                        PInvoke.GlobalUnlock(ptr3);
                    }
                    if((hMem != IntPtr.Zero) && (ptr3 != IntPtr.Zero)) {
                        uint uFormat = PInvoke.RegisterClipboardFormat("Preferred DropEffect");
                        PInvoke.SetClipboardData(15, hMem);
                        PInvoke.SetClipboardData(uFormat, ptr3);
                        return true;
                    }
                }
                finally {
                    PInvoke.CloseClipboard();
                }
            }
            return false;
        }

        public static IntPtr ShellGetPath2(IntPtr hwnd) {
            IntPtr zero = IntPtr.Zero;
            try {
                uint num;
                PInvoke.GetWindowThreadProcessId(hwnd, out num);
                IntPtr hData = PInvoke.SendMessage(hwnd, 0x40c, (IntPtr)num, IntPtr.Zero);
                if(!(hData != IntPtr.Zero)) {
                    return zero;
                }
                IntPtr pidl = PInvoke.SHLockShared(hData, num);
                if(pidl != IntPtr.Zero) {
                    zero = PInvoke.ILClone(pidl);
                    PInvoke.SHUnlockShared(pidl);
                }
                PInvoke.SHFreeShared(hData, num);
            }
            catch {
            }
            return zero;
        }

        public static void ShowProperties(byte[] idl, IntPtr hwnd = default(IntPtr)) {
            using(SafePtr sptr = new SafePtr("properties"))
            using(IDLWrapper wrapper = new IDLWrapper(idl)) {
                if(wrapper.Available) {
                    SHELLEXECUTEINFO structure = new SHELLEXECUTEINFO {
                        hwnd = hwnd,
                        fMask = 0x40C,
                        lpVerb = sptr,
                        lpIDList = wrapper.PIDL
                    };
                    structure.cbSize = Marshal.SizeOf(structure);
                    PInvoke.ShellExecuteEx(ref structure);
                }
            }
        }

        public static bool TryGetFolderPath(int folder, out string path) {
            StringBuilder pszPath = new StringBuilder(260);
            if((PInvoke.SHGetFolderPath(IntPtr.Zero, folder, IntPtr.Zero, 0, pszPath) == 0) && (pszPath.Length > 0)) {
                path = pszPath.ToString();
                return true;
            }
            path = null;
            return false;
        }

        public static bool TryMakeSubDirTipPath(ref string path) {
            if(!string.IsNullOrEmpty(path)) {
                bool flag;
                if(path.StartsWith("::")) {
                    using(IDLWrapper wrapper = new IDLWrapper(path)) {
                        return wrapper.IsFolder;
                    }
                }
                if(path.StartsWith(@"a:\", StringComparison.OrdinalIgnoreCase) || path.StartsWith(@"b:\", StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
                if(!Directory.Exists(path)) {
                    if(!File.Exists(path) || !Path.GetExtension(path).PathEquals(".lnk")) {
                        return false;
                    }
                    path = GetLinkTargetPath(path);
                    if(!Directory.Exists(path)) {
                        return false;
                    }
                }
                FileSystemInfo targetIfFolderLink = GetTargetIfFolderLink(new DirectoryInfo(path), out flag);
                if(flag) {
                    bool fSearchHidden;
                    bool fSearchSystem;
                    QTUtility.GetHiddenFileSettings(out fSearchHidden, out fSearchSystem);
                    bool flag4 = Config.Tips.SubDirTipsFiles;
                    path = targetIfFolderLink.FullName;
                    using(FindFile file = new FindFile(path, fSearchHidden, fSearchSystem)) {
                        return (file.SubDirectoryExists() || (flag4 && file.SubFileExists()));
                    }
                }
            }
            return false;
        }
    }
}

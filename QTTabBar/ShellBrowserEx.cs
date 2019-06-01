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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BandObjectLib;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    public class ShellBrowserEx : IDisposable {
        private IShellBrowser shellBrowser;
        private IFolderView folderView;

        public ShellBrowserEx(IShellBrowser shellBrowser) {
            this.shellBrowser = shellBrowser;
            OnNavigateComplete();
        }

        public FVM ViewMode {
            get {
                FVM pViewMode = 0;
                return folderView != null && folderView.GetCurrentViewMode(ref pViewMode) == 0
                        ? pViewMode : FVM.ICON;
            }
            set {
                if(folderView != null) folderView.SetCurrentViewMode(value);
            }
        }

        public void DeleteSelection(bool fNuke) {
            List<string> lstPaths = new List<string>();
            foreach(var idlw in GetItems(true)) {
                if(idlw.HasPath /* && idlw.CanDelete */ ) { // todo
                    lstPaths.Add(idlw.Path);
                }
                else {
                    // not deletable object found, cancel
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }

            if(lstPaths.Count > 0) {
                IntPtr handle;
                shellBrowser.GetWindow(out handle);
                ShellMethods.DeleteFile(lstPaths, fNuke, handle);
            }
            else {
                // no item selected
                System.Media.SystemSounds.Beep.Play();
            }
        }

        public void Dispose() {
            if(shellBrowser != null) {
                Marshal.FinalReleaseComObject(shellBrowser);
                shellBrowser = null;
            }
            if(folderView != null) {
                Marshal.ReleaseComObject(folderView);
                folderView = null;
            }
        }
        
        public IntPtr GetExplorerHandle() {
            IntPtr hwnd;
            shellBrowser.GetWindow(out hwnd);
            IntPtr parent = PInvoke.GetParent(hwnd);
            return parent != IntPtr.Zero ? parent : hwnd;
        }

        public int GetFocusedIndex() {
            int focusedIndex;
            return folderView != null && folderView.GetFocusedItem(out focusedIndex) == 0 
                    ? focusedIndex : -1;
        }

        public IDLWrapper GetFocusedItem() {
            int focusedIndex = GetFocusedIndex();
            return focusedIndex == -1 ? new IDLWrapper() : GetItem(focusedIndex);
        }

        public IShellBrowser GetIShellBrowser() {
            return shellBrowser;
        }

        public IDLWrapper GetItem(int idx, bool noAppend = false) {
            if(folderView == null) return new IDLWrapper();
            IntPtr ppidl = IntPtr.Zero;
            try {
                folderView.Item(idx, out ppidl);
                if(noAppend || ppidl == IntPtr.Zero) {
                    return new IDLWrapper(ppidl);
                }
                using(IDLWrapper path = GetShellPath()) {
                    return new IDLWrapper(PInvoke.ILCombine(path.PIDL, ppidl));
                }
            }
            finally {
                if(ppidl != IntPtr.Zero && !noAppend) {
                    PInvoke.CoTaskMemFree(ppidl);
                }
            }
            
        }

        public int GetItemCount() {
            int count;
            return folderView != null && folderView.ItemCount(SVGIO.ALLVIEW, out count) == 0 ? count : 0;
        }

        public IEnumerable<IDLWrapper> GetItems(bool selectedOnly = false, bool noAppend = false) {
            if(folderView == null) yield break;
            Guid guid = ExplorerGUIDs.IID_IEnumIDList;
            IEnumIDList list = null;
            try {
                using(IDLWrapper path = noAppend ? null : GetShellPath()) {
                    folderView.Items(SVGIO.FLAG_VIEWORDER | (selectedOnly ? SVGIO.SELECTION : SVGIO.ALLVIEW), ref guid, out list);
                    if(list == null) yield break;
                    IntPtr ptr;
                    while(list.Next(1, out ptr, null) == 0) {
                        using(IDLWrapper wrapper1 = new IDLWrapper(ptr)) {
                            if(!wrapper1.Available) continue;
                            if(noAppend) {
                                yield return wrapper1;
                            }
                            else {
                                using(IDLWrapper wrapper2 = new IDLWrapper(PInvoke.ILCombine(path.PIDL, wrapper1.PIDL))) {
                                    yield return wrapper2;
                                }
                            }
                        }
                    }
                }
            }
            finally {
                if(list != null) {
                    Marshal.ReleaseComObject(list);
                }
            }
        }

        public int GetSelectedCount() {
            int count;
            return folderView != null && folderView.ItemCount(SVGIO.SELECTION, out count) == 0 ? count : 0;
        }

        public IDLWrapper GetShellPath() {
            if(folderView == null) return new IDLWrapper();
            IPersistFolder2 ppv = null;
            try {
                Guid riid = ExplorerGUIDs.IID_IPersistFolder2;
                if(folderView.GetFolder(ref riid, out ppv) == 0) {
                    IntPtr ptr;
                    ppv.GetCurFolder(out ptr);
                    return new IDLWrapper(ptr);
                }
            }
            catch {
            }
            finally {
                if(ppv != null) {
                    Marshal.ReleaseComObject(ppv);
                }
            }
            return new IDLWrapper();
        }

        public IDLWrapper ILAppend(IntPtr ptr) {
            if(ptr == IntPtr.Zero) {
                return new IDLWrapper(ptr);
            }
            using(IDLWrapper path = GetShellPath()) {
                return new IDLWrapper(path.Available
                        ? PInvoke.ILCombine(path.PIDL, ptr)
                        : IntPtr.Zero);
            }
        }

        public bool IsFolderTreeVisible() {
            IntPtr ptr;
            return IsFolderTreeVisible(out ptr);
        }

        public bool IsFolderTreeVisible(out IntPtr hwnd) {
            hwnd = IntPtr.Zero;
            return QTUtility.IsXP && shellBrowser != null && 0 == shellBrowser.GetControlWindow(3, out hwnd);
        }

        // Call this on navigate to refresh the FolderView
        public void OnNavigateComplete() {
            if(shellBrowser == null) return;
            if(folderView != null) {
                Marshal.ReleaseComObject(folderView);
                folderView = null;
            }
            IShellView ppshv;
            if(shellBrowser.QueryActiveShellView(out ppshv) == 0) {
                folderView = ppshv as IFolderView;
            }
        }

        public int Navigate(IDLWrapper idlw, SBSP flags = SBSP.SAMEBROWSER) {
            if(idlw.Available && shellBrowser != null) {
                try {
                    return shellBrowser.BrowseObject(idlw.PIDL, flags);
                }
                catch(COMException) {
                }
            }
            return 1;
        }

        public void SelectItem(int idx) {
            if(folderView != null) folderView.SelectItem(idx, SVSIF.SELECT | SVSIF.DESELECTOTHERS | SVSIF.ENSUREVISIBLE | SVSIF.FOCUSED);
        }

        internal void SetStatusText(string status) {
            if(shellBrowser != null) shellBrowser.SetStatusTextSB(status);
        }

        public void SetUsingListView(bool listview) {
            if(shellBrowser != null) {
                IFolderViewOptions fvo = shellBrowser as IFolderViewOptions;
                if(fvo != null) {
                    fvo.SetFolderViewOptions(FVO.VISTALAYOUT, listview ? FVO.VISTALAYOUT : FVO.DEFAULT);
                }                
            }
        }

        // todo: idlify.
        public bool TryGetHotTrackPath(int iItem, out string path) {
            return TryGetHotTrackPath(iItem, out path, null);
        }

        public bool TryGetHotTrackPath(int iItem, out string path, string matchName) {
            path = null;
            try {
                using(IDLWrapper wrapper = GetItem(iItem, true)) {
                    if(wrapper.Available) {
                        if(!string.IsNullOrEmpty(matchName) && matchName != wrapper.ParseName) {
                            return false;
                        }
                        using(IDLWrapper wrapper2 = ILAppend(wrapper.PIDL)) {
                            path = wrapper2.ParseName;
                            if(!string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0) {
                                return true;
                            }
                            path = null;
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
            return false;
        }

        public bool TryGetSelection(out Address[] adSelectedItems, bool fDisplayName) {
            if(GetSelectedCount() == 0) {
                adSelectedItems = new Address[0];
                return false;
            }

            adSelectedItems = GetItems(true).Select(wrapper => fDisplayName
                     ? new Address(wrapper.PIDL, wrapper.DisplayName)
                     : new Address(wrapper.PIDL, wrapper.ParseName)).ToArray();
            return true;
        }

        public bool TryGetSelection(out Address[] adSelectedItems, out string pathFocused, bool fDisplayName) {
            using(IDLWrapper wrapper = GetFocusedItem()) {
                pathFocused = wrapper.ParseName;
            }
            return TryGetSelection(out adSelectedItems, fDisplayName);
        }

        public bool TrySetSelection(Address[] addresses, string pathToFocus, bool fDeselectOthers) {
            IShellView shellView = folderView as IShellView;
            if(addresses == null || folderView == null || shellView == null) return false;
            try {
                bool fFirst = true;
                bool fFocused = false;
                bool fFocusingNeeded = !string.IsNullOrEmpty(pathToFocus);

                if(fDeselectOthers) folderView.SelectItem(0, SVSIF.DESELECTOTHERS);

                foreach(Address ad in addresses) {
                    using(IDLWrapper wrapper = new IDLWrapper(ad)) {
                        if(!wrapper.Available) continue;
                        IntPtr pIDLCHILD = PInvoke.ILFindLastID(wrapper.PIDL);
                        SVSIF svsi = SVSIF.SELECT;
                        if(fFirst) {
                            svsi |= SVSIF.ENSUREVISIBLE;
                            if(!fFocusingNeeded) {
                                fFocused = true;
                                svsi |= SVSIF.FOCUSED;
                            }
                            if(fDeselectOthers)
                                svsi |= SVSIF.DESELECTOTHERS;
                            fFirst = false;
                        }

                        if(!fFocused && ad.Path == pathToFocus) {
                            fFocused = true;
                            svsi |= SVSIF.FOCUSED;
                        }

                        shellView.SelectItem(pIDLCHILD, svsi);
                    }
                }

                if(!fFocused && fFocusingNeeded) {
                    using(IDLWrapper wrapper = new IDLWrapper(pathToFocus)) {
                        IntPtr pIDLFOCUSCHILD = PInvoke.ILFindLastID(wrapper.PIDL);
                        shellView.SelectItem(pIDLFOCUSCHILD, SVSIF.FOCUSED | SVSIF.ENSUREVISIBLE);
                    }
                }
                return true;
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex);
            }
            return false;
        }
    }
}

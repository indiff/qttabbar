//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    public class ListViewMonitor : IDisposable {
        public event EventHandler ListViewChanged;
        // Windows 11's native Explorer tabs (22H2+) create one ShellTabWindowClass
        // container per tab inside the same window; earlier Windows has exactly one.
        // A view (SHELLDLL_DefView) is only ever created inside its own container, so
        // every container must be watched, and containers appear/disappear as native
        // tabs open and close.
        private List<NativeWindowController> containerControllers = new List<NativeWindowController>();
        private NativeWindowController explorerController;
        // All still-live captured views, so switching back to a native tab reuses the
        // instance that already subclasses its listview instead of subclassing twice.
        private List<AbstractListView> liveViews = new List<AbstractListView>();
        private ShellBrowserEx ShellBrowser;
        private IntPtr hwndExplorer;
        private IntPtr hwndSubDirTipMessageReflect;
        private bool fDisposed;

        internal ListViewMonitor(ShellBrowserEx shellBrowser, IntPtr hwndExplorer, IntPtr hwndSubDirTipMessageReflect) {
            ShellBrowser = shellBrowser;
            this.hwndExplorer = hwndExplorer;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;
            if(QTUtility.IsXP) {
                AddContainer(hwndExplorer);
            }
            else {
                IntPtr hwndContainer = IntPtr.Zero;
                while((hwndContainer = PInvoke.FindWindowEx(hwndExplorer, hwndContainer, "ShellTabWindowClass", null)) != IntPtr.Zero) {
                    AddContainer(hwndContainer);
                }
                // New native tabs create their container after this monitor exists -
                // the explorer window gets the WM_PARENTNOTIFY for those.
                explorerController = new NativeWindowController(hwndExplorer);
                explorerController.MessageCaptured += ExplorerController_MessageCaptured;
            }
        }

        public AbstractListView CurrentListView { get; private set; }
        public AbstractListView PreviousListView { get; private set; }

        private void AddContainer(IntPtr hwnd) {
            NativeWindowController controller = new NativeWindowController(hwnd);
            controller.MessageCaptured += ContainerController_MessageCaptured;
            containerControllers.Add(controller);
        }

        // The active native tab's container is always first in the explorer window's
        // child z-order (switching tabs just reorders them).
        private IntPtr ActiveContainer() {
            return QTUtility.IsXP ? hwndExplorer : WindowUtils.GetShellTabWindowClass(hwndExplorer);
        }

        private bool ExplorerController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.PARENTNOTIFY &&
               PInvoke.LoWord((int)msg.WParam) == WM.CREATE &&
               PInvoke.GetClassName(msg.LParam) == "ShellTabWindowClass") {
                AddContainer(msg.LParam);
            }
            return false;
        }

        private bool ContainerController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM.PARENTNOTIFY &&
               PInvoke.LoWord((int)msg.WParam) == WM.CREATE) {
                string name = PInvoke.GetClassName(msg.LParam);
                if(name == "SHELLDLL_DefView" && msg.HWnd == ActiveContainer()) {
                    RecaptureHandles(msg.LParam, fForceNew: true);
                }
            }
            else if(msg.Msg == WM.WINDOWPOSCHANGED && msg.HWnd == ActiveContainer()) {
                WINDOWPOS wp = (WINDOWPOS)Marshal.PtrToStructure(msg.LParam, typeof(WINDOWPOS));
                if((wp.flags & SWP.NOZORDER) == 0) {
                    if(containerControllers.Count > 1) {
                        // Switching native tabs creates and shows nothing - the incoming tab's
                        // container is just moved to the top of the z-order. This is the only
                        // signal that a different tab's existing view is now the one on screen.
                        IntPtr hwndShellView = WindowUtils.FindChildWindow(msg.HWnd,
                                hwnd => PInvoke.GetClassName(hwnd) == "SHELLDLL_DefView");
                        if(hwndShellView != IntPtr.Zero) {
                            RecaptureHandles(hwndShellView);
                        }
                    }
                    else {
                        // Single native tab: no tab-switch signal ever comes, so if Explorer
                        // recreated the list view in place - leaving the current view bound to a
                        // now-dead window - this relayout is our chance to notice and re-bind it.
                        EnsureCurrentViewLive();
                    }
                }
            }
            return false;
        }

        public void Initialize() {
            // EnumChildWindows walks depth-first from the first (= active) container,
            // so with multiple native tabs this finds the active tab's view.
            IntPtr hwndShellView = WindowUtils.FindChildWindow(hwndExplorer, hwnd => PInvoke.GetClassName(hwnd) == "SHELLDLL_DefView");
            if(hwndShellView == IntPtr.Zero) {
                if(CurrentListView != null) {
                    CurrentListView.Dispose();
                }
                CurrentListView = new AbstractListView();
                ListViewChanged(this, null);
            }
            else {
                RecaptureHandles(hwndShellView);
            }
        }

        // A live view lingers in liveViews when its WM_DESTROY is missed (the event
        // never fires). Drop any whose window is gone before we trust the by-handle
        // cache below - otherwise, once Windows recycles that HWND value, the reuse
        // lookup hands back a dead view still subclassing the defunct window, so it
        // never sees the active tab's mouse messages and double-click silently dies
        // while Explorer-native back/forward (which never touches this cache) works.
        private void PruneDeadViews() {
            for(int i = liveViews.Count - 1; i >= 0; i--) {
                AbstractListView v = liveViews[i];
                if(v == CurrentListView || v == PreviousListView) continue;
                // Drop dead-window views (missed WM_DESTROY) and stranded AbstractListView
                // placeholders (Handle == Zero, left behind after visiting a virtual folder
                // like Home). Neither can ever be reused - the by-handle lookup skips Zero
                // and never matches a defunct window - so both are pure leak.
                if(v.Handle == IntPtr.Zero || !PInvoke.IsWindow(v.Handle)) {
                    liveViews.RemoveAt(i);
                    v.Dispose();
                }
            }
        }

        // The active tab's list view can be recreated in place (certain in-tab navigations
        // on Windows 11) without our subclass ever receiving WM_DESTROY, leaving CurrentListView
        // bound to a dead window. It then gets no mouse messages, so double-click silently stops
        // working while Explorer-native back/forward (which never touch this subclass) keep going.
        // With a single native tab there is no tab-switch or DefView-create signal to recapture on,
        // so nothing re-binds it until the user opens another tab - exactly the "make a new tab and
        // it starts working again" workaround. Callers with a reliable per-navigation or relayout
        // signal call this to heal the current view immediately. Cheap no-op while it is still alive.
        public void EnsureCurrentViewLive() {
            if(fDisposed || CurrentListView == null) return;
            // Handle == Zero is a legitimate virtual-folder placeholder (Home etc.), not a dead view.
            if(CurrentListView.Handle == IntPtr.Zero || PInvoke.IsWindow(CurrentListView.Handle)) return;
            IntPtr hwndShellView = WindowUtils.FindChildWindow(ActiveContainer(),
                    hwnd => PInvoke.GetClassName(hwnd) == "SHELLDLL_DefView");
            if(hwndShellView != IntPtr.Zero) {
                RecaptureHandles(hwndShellView, fForceNew: true);
            }
        }

        // fForceNew is set by callers that just watched a brand-new list view get
        // created (SHELLDLL_DefView CREATE). Such an HWND can never legitimately be an
        // existing live tab, so we must not short-circuit on a handle match or reuse a
        // cached view for it - a match there is a recycled-handle impostor, not a tab
        // the user switched back to (that path is the WINDOWPOSCHANGED trigger, which
        // passes fForceNew=false and keeps the reuse fast-path).
        private void RecaptureHandles(IntPtr hwndShellView, bool fForceNew = false) {
            bool fIsSysListView = false;
            IntPtr hwndListView = WindowUtils.FindChildWindow(hwndShellView, hwnd => {
                string name = PInvoke.GetClassName(hwnd);
                if(name == "SysListView32") {
                    fIsSysListView = true;
                    return true;
                }
                else if(!QTUtility.IsXP && name == "DirectUIHWND") {
                    fIsSysListView = false;
                    return true;
                }
                return false;
            });

            PruneDeadViews();

            if(CurrentListView != null) {
                if(!fForceNew && CurrentListView.Handle == hwndListView) {
                    return;
                }
                PreviousListView = CurrentListView;
            }

            AbstractListView live = hwndListView == IntPtr.Zero ? null
                    : liveViews.Find(view => view.Handle == hwndListView);
            if(live != null && !fForceNew) {
                // Back on a native tab whose view is still alive and subclassed.
                CurrentListView = live;
                ListViewChanged(this, null);
                return;
            }
            if(live != null) {
                // fForceNew: the HWND belongs to a freshly created view, so this cached
                // entry is a stale collision. Drop it so we subclass the real window.
                liveViews.Remove(live);
                if(live != PreviousListView) live.Dispose();
            }

            if(hwndListView == IntPtr.Zero)
            {
                QTUtility2.log("new AbstractListView");
                CurrentListView = new AbstractListView();
            }
            else if(fIsSysListView) {
                QTUtility2.log("new ExtendedSysListView32");
                CurrentListView = new ExtendedSysListView32(ShellBrowser, hwndShellView, hwndListView, hwndSubDirTipMessageReflect);
            }
            else {
                QTUtility2.log("new ExtendedItemsView");
                CurrentListView = new ExtendedItemsView(ShellBrowser, hwndShellView, hwndListView, hwndSubDirTipMessageReflect);
            }
            CurrentListView.ListViewDestroyed += ListView_Destroyed;
            liveViews.Add(CurrentListView);
            ListViewChanged(this, null);
        }

        private void ListView_Destroyed(object sender, EventArgs args) {
            liveViews.Remove((AbstractListView)sender);
            if(sender == CurrentListView) {
                if(PreviousListView != null) {
                    CurrentListView = PreviousListView;
                    PreviousListView = null;
                }
                else {
                    CurrentListView = new AbstractListView();
                }
                ListViewChanged(this, null);
            }
            else if(sender == PreviousListView) {
                PreviousListView = null;
            }
            ((AbstractListView)sender).Dispose();
        }

        #region IDisposable Members

        public void Dispose() {
            if(fDisposed) return;
            foreach(AbstractListView view in liveViews) {
                if(view != CurrentListView) view.Dispose();
            }
            liveViews.Clear();
            if(CurrentListView != null) {
                CurrentListView.Dispose();
                CurrentListView = null;
            }
            fDisposed = true;
        }

        #endregion
    }
}

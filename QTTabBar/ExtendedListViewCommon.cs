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
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTPlugin;
using QTTabBarLib.Interop;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using Timer = System.Windows.Forms.Timer;

namespace QTTabBarLib {
    internal abstract class ExtendedListViewCommon : AbstractListView {

        #region Delegates
        internal delegate bool DoubleClickHandler(Point pt);
        internal delegate void EndLabelEditHandler(LVITEM item);
        internal delegate bool ItemActivatedHandler(Keys modKeys);
        internal delegate void ItemCountChangedHandler(int count);
        internal delegate bool MiddleClickHandler(Point pt);
        internal delegate bool MouseActivateHandler(ref int result);
        internal delegate void SelectionChangedHandler();
        internal delegate void RefreshHandler();
        #endregion

        #region Events
        internal event DoubleClickHandler DoubleClick;            // OK
        internal event EndLabelEditHandler EndLabelEdit;          // SysListView Only
        internal event ItemActivatedHandler SelectionActivated;        // OK
        internal event ItemCountChangedHandler ItemCountChanged;  // OK
        internal event MiddleClickHandler MiddleClick;            // OK
        internal event MouseActivateHandler MouseActivate;        // OK
        internal event SelectionChangedHandler SelectionChanged;  // OK
        internal event EventHandler SubDirTip_MenuClosed;
        internal event ToolStripItemClickedEventHandler SubDirTip_MenuItemClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MenuItemRightClicked;
        internal event EventHandler SubDirTip_MultipleMenuItemsClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MultipleMenuItemsRightClicked;
        #endregion

        protected static readonly UInt32 WM_AFTERPAINT = PInvoke.RegisterWindowMessage("QTTabBar_AfterPaint");
        protected static readonly UInt32 WM_REMOTEDISPOSE = PInvoke.RegisterWindowMessage("QTTabBar_RemoteDispose");
        protected static readonly UInt32 WM_REGISTERDRAGDROP = PInvoke.RegisterWindowMessage("QTTabBar_RegisterDragDrop");
        protected static readonly UInt32 WM_ISITEMSVIEW = PInvoke.RegisterWindowMessage("QTTabBar_IsItemsView");
        protected static readonly UInt32 WM_ACTIVATESEL = PInvoke.RegisterWindowMessage("QTTabBar_ActivateSelection");

        protected NativeWindowController ListViewController;
        protected NativeWindowController ShellViewController;
        private DropTargetPassthrough dropTargetPassthrough;
        protected bool fThumbnailPending;
        protected bool fTrackMouseEvent;
        protected IntPtr hwndExplorer;
        private IntPtr hwndSubDirTipMessageReflect;
        protected readonly ShellBrowserEx ShellBrowser;
        protected int subDirIndex = -1;
        protected SubDirTipForm subDirTip;
        protected int thumbnailIndex = -1;
        protected ThumbnailTooltipForm thumbnailTooltip;
        private Timer timer_HoverSubDirTipMenu;
        private Timer timer_Thumbnail;
        protected bool fDragging;

        internal ExtendedListViewCommon(ShellBrowserEx shellBrowser, IntPtr hwndShellView, IntPtr hwndListView, IntPtr hwndSubDirTipMessageReflect) {
            ShellBrowser = shellBrowser;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;

            ListViewController = new NativeWindowController(hwndListView);
            ListViewController.MessageCaptured += ListViewController_MessageCaptured;
            ShellViewController = new NativeWindowController(hwndShellView);
            ShellViewController.MessageCaptured += ShellViewController_MessageCaptured;

            TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.dwFlags = 2;
            structure.hwndTrack = ListViewController.Handle;
            PInvoke.TrackMouseEvent(ref structure);

            timer_HoverSubDirTipMenu = new Timer();
            timer_HoverSubDirTipMenu.Interval = SystemInformation.MouseHoverTime * 6 / 5;
            timer_HoverSubDirTipMenu.Tick += timer_HoverSubDirTipMenu_Tick;

            hwndExplorer = PInvoke.GetAncestor(hwndShellView, 3 /* GA_ROOTOWNER */);

            // If we're late to the party, we'll have to get the IDropTarget the
            // old-fashioned way.  Careful!  Calling RegisterDragDrop will go 
            // through the hook!
            IntPtr ptr = PInvoke.GetProp(hwndListView, "OleDropTargetInterface");
            dropTargetPassthrough = TryMakeDTPassthrough(ptr);
            if(dropTargetPassthrough != null) {
                PInvoke.RevokeDragDrop(hwndListView);
                PInvoke.RegisterDragDrop(hwndListView, dropTargetPassthrough);
            }
        }

        public override IntPtr Handle {
            get { return ListViewController.Handle; }
        }
       
        #region IDisposable Members

        public override void Dispose(bool fDisposing) {
            if(fDisposed) return;
            // Never call NativeWindow.ReleaseHandle().  EVER!!!
            if(ListViewController != null) {
                ListViewController.MessageCaptured -= ListViewController_MessageCaptured;
                ListViewController = null;
            }
            if(ShellViewController != null) {
                ShellViewController.MessageCaptured -= ShellViewController_MessageCaptured;
                ShellViewController = null;
            }
            if(timer_HoverSubDirTipMenu != null) {
                timer_HoverSubDirTipMenu.Dispose();
                timer_HoverSubDirTipMenu = null;
            }
            if(timer_Thumbnail != null) {
                timer_Thumbnail.Dispose();
                timer_Thumbnail = null;
            }
            if(thumbnailTooltip != null) {
                thumbnailTooltip.Dispose();
                thumbnailTooltip = null;
            }
            if(subDirTip != null) {
                subDirTip.Dispose();
                subDirTip = null;
            }
            if(dropTargetPassthrough != null) {
                dropTargetPassthrough.Dispose();
                dropTargetPassthrough = null;
            }
            base.Dispose(fDisposing);
        }

        #endregion

        protected abstract IntPtr GetEditControl();

        protected abstract Rectangle GetFocusedItemRect(); 

        public override int GetHotItem() {
            return HitTest(Control.MousePosition, true);
        }

        protected abstract Point GetSubDirTipPoint(bool fByKey);
        
        protected abstract bool HandleCursorLoop(Keys key);

        public override void HandleF2() {
            IntPtr hWnd = GetEditControl();
            if(hWnd == IntPtr.Zero) return;
            string str;
            using(SafePtr lParam = new SafePtr(520)) {
                if(0 >= ((int)PInvoke.SendMessage(hWnd, 13, (IntPtr)260, lParam))) return;
                str = Marshal.PtrToStringUni(lParam);
            }
            if(str.Length <= 2) return;
            int num = str.LastIndexOf(".");
            if(num != -1) {
                IntPtr ptr3 = PInvoke.SendMessage(hWnd, 0xb0, IntPtr.Zero, IntPtr.Zero);
                int start = QTUtility2.GET_X_LPARAM(ptr3);
                int length = QTUtility2.GET_Y_LPARAM(ptr3);
                if((length - start) >= 0) {
                    if((start == 0) && (length == num)) {
                        start = length = num;
                    }
                    else if((start == length) && (length == num)) {
                        start = num + 1;
                        length = str.Length;
                    }
                    else if((start == (num + 1)) && (length == str.Length)) {
                        start = 0;
                        length = -1;
                    }
                    else if((start == 0) && (length == str.Length)) {
                        start = 0;
                        length = 0;
                    }
                    else {
                        start = 0;
                        length = num;
                    }
                    PInvoke.SendMessage(hWnd, 0xb1, (IntPtr)start, (IntPtr)length);
                }   
            }
        }

        public override void HandleShiftKey() {
            if(!Config.Tips.ShowPreviewsWithShift) {
                HideThumbnailTooltip(5);
            }

            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(MouseIsOverListView()) {
                        RefreshSubDirTip();
                    }
                }
                else if(!SubDirTipMenuIsShowing()) {
                    HideSubDirTip(6);
                }
            }
        }

        public override bool HasFocus() {
            return (ListViewController != null &&
                PInvoke.GetFocus() == ListViewController.Handle);
        }

        public override void HideSubDirTip(int iReason = -1) {
            if(subDirTip == null || !subDirTip.IsShowing) return;
            bool fForce = iReason < 0;
            if(fForce || !subDirTip.IsShownByKey) {
                subDirTip.HideSubDirTip(fForce);
                subDirIndex = -1;
            }
        }

        public override void HideSubDirTipMenu() {
            if(subDirTip != null) {
                subDirTip.HideMenu();
            }
        }

        public override void HideSubDirTip_ExplorerInactivated() {
            if((subDirTip != null) && subDirTip.IsShowing) {
                subDirTip.OnExplorerInactivated();
            }
        }

        public override void HideThumbnailTooltip(int iReason = -1) {
            if((thumbnailTooltip != null) && thumbnailTooltip.IsShowing) {
                if(((iReason == 0) || (iReason == 7)) || (iReason == 9)) {
                    thumbnailTooltip.IsShownByKey = false;
                }
                if(thumbnailTooltip.HideToolTip()) {
                    thumbnailIndex = -1;
                }
            }
        }

        public override int HitTest(IntPtr LParam) {
            return HitTest(QTUtility2.PointFromLPARAM(LParam), false);
        }

        public abstract override int HitTest(Point pt, bool ScreenCoords);

        public abstract override bool HotItemIsSelected(); 

        // If the ListView is in Details mode, returns true only if the mouse
        // is over the ItemName column.  Returns true always for any other mode.
        // This function only returns valid results if the mouse is known to be
        // over an item.  Otherwise, its return value is undefined.
        public abstract override bool IsTrackingItemName();

        protected virtual bool ListViewController_MessageCaptured(ref Message msg) {

            if(msg.Msg == WM_AFTERPAINT) {
                RefreshSubDirTip(true);
                return true;
            }
            else if(msg.Msg == WM_REGISTERDRAGDROP) {
                IntPtr ptr = Marshal.ReadIntPtr(msg.WParam);
                if(dropTargetPassthrough != null) {
                    // If this is the RegisterDragDrop call from the constructor,
                    // don't mess it up!
                    if(dropTargetPassthrough.Pointer == ptr) {
                        return true;
                    }
                    dropTargetPassthrough.Dispose();
                }
                dropTargetPassthrough = TryMakeDTPassthrough(ptr);
                if(dropTargetPassthrough != null) {
                    Marshal.WriteIntPtr(msg.WParam, dropTargetPassthrough.Pointer);
                }
                return true;
            }

            switch(msg.Msg) {
                case WM.DESTROY:
                    HideThumbnailTooltip(7);
                    HideSubDirTip(7);
                    ListViewController.DefWndProc(ref msg);
                    OnListViewDestroyed();
                    return true;

                case WM.PAINT:
                    // It's very dangerous to do automation-related things
                    // during WM_PAINT.  So, use PostMessage to do it later.
                    PInvoke.PostMessage(ListViewController.Handle, WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                    break;

                case WM.MOUSEMOVE:
                    ResetTrackMouseEvent();
                    break;

                case WM.LBUTTONDBLCLK:
                    if(DoubleClick != null) {
                        return DoubleClick(QTUtility2.PointFromLPARAM(msg.LParam));
                    }
                    break;
                
                case WM.MBUTTONUP:
                    if(MiddleClick != null) {
                        MiddleClick(QTUtility2.PointFromLPARAM(msg.LParam));
                    }
                    break;

                case WM.MOUSEWHEEL: {
                    IntPtr handle = PInvoke.WindowFromPoint(QTUtility2.PointFromLPARAM(msg.LParam));
                    if(handle != IntPtr.Zero && handle != msg.HWnd) {
                        Control control = Control.FromHandle(handle);
                        if(control != null) {
                            DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                            if((reorderable != null) && reorderable.CanScroll) {
                                PInvoke.SendMessage(handle, WM.MOUSEWHEEL, msg.WParam, msg.LParam);
                            }
                        }
                    }
                    break;
                }

                case WM.MOUSELEAVE:
                    fTrackMouseEvent = true;
                    HideThumbnailTooltip(4);
                    if(((subDirTip != null) && !subDirTip.MouseIsOnThis()) && !subDirTip.MenuIsShowing) {
                        HideSubDirTip(5);
                    }
                    break;
            }
            return false;
        }

        private DropTargetPassthrough TryMakeDTPassthrough(IntPtr pDropTarget) {
            if(pDropTarget != IntPtr.Zero) {
                object obj = Marshal.GetObjectForIUnknown(pDropTarget);
                try {
                    if(obj is _IDropTarget) {

                        // For some reason, the RCW doesn't work in dropTargetPassthrough's
                        // functions if it's created now.  So, we'll just keep the pointer,
                        // and create the RCW each time we need it.
                        return new DropTargetPassthrough(pDropTarget, this);
                    }
                }
                finally {
                    Marshal.ReleaseComObject(obj);
                }
            }
            return null;
        }

        public override bool MouseIsOverListView() {
            return (ListViewController != null &&
                PInvoke.WindowFromPoint(Control.MousePosition) == ListViewController.Handle);
        }

        protected bool OnDoubleClick(Point pt) {
            return DoubleClick != null && DoubleClick(pt);
        }

        protected virtual void OnDragBegin() {
            fDragging = true;
        }

        protected virtual void OnDragEnd() {
            if(subDirTip != null) {
                subDirTip.HideMenu();
            }
            timer_HoverSubDirTipMenu.Enabled = false;
            RefreshSubDirTip(true);
            fDragging = false;
        }

        protected virtual void OnDragOver(Point pt) {
            timer_HoverSubDirTipMenu.Enabled = false;
            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(Control.ModifierKeys == Keys.Shift) {
                        timer_HoverSubDirTipMenu_Tick(null, null);
                    }
                }
                else {
                    timer_HoverSubDirTipMenu.Enabled = true;    
                }
            }
        }

        protected void OnEndLabelEdit(LVITEM item) {
            if(EndLabelEdit != null) {
                EndLabelEdit(item);
            }
        }

        protected bool OnGetInfoTip(int iItem, bool byKey) {
            if(Config.Tips.ShowTooltipPreviews && (!Config.Tips.ShowPreviewsWithShift ^ (Control.ModifierKeys == Keys.Shift))) {
                if(((thumbnailTooltip != null) && thumbnailTooltip.IsShowing) && (iItem == thumbnailIndex)) {
                    return true;
                }
                else if(byKey) {
                    Rectangle rect = GetFocusedItemRect();
                    Point pt = new Point(rect.Right - 32, rect.Bottom - 16);
                    PInvoke.ClientToScreen(Handle, ref pt);
                    return ShowThumbnailTooltip(iItem, pt, true);
                }
                else {
                    return ShowThumbnailTooltip(iItem, Control.MousePosition, false);
                }
            }
            return false;
        }

        protected void OnHotItemChanged(int iItem) {
            Keys modifierKeys = Control.ModifierKeys;
            if(Config.Tips.ShowTooltipPreviews) {
                if((thumbnailTooltip != null) && (thumbnailTooltip.IsShowing || fThumbnailPending)) {
                    if(!Config.Tips.ShowPreviewsWithShift ^ (modifierKeys == Keys.Shift)) {
                        if(iItem != thumbnailIndex) {
                            if(iItem > -1 && IsTrackingItemName()) {
                                if(ShowThumbnailTooltip(iItem, Control.MousePosition, false)) {
                                    return;
                                }
                            }
                            if(thumbnailTooltip.HideToolTip()) {
                                thumbnailIndex = -1;
                            }
                        }
                    }
                    else if(thumbnailTooltip.HideToolTip()) {
                        thumbnailIndex = -1;
                    }
                }
            }
            RefreshSubDirTip();
            
            return;
        }

        protected bool OnSelectionActivated(Keys modKeys) {
            return SelectionActivated != null && SelectionActivated(modKeys);
        }

        protected void OnItemCountChanged() {
            if(ItemCountChanged != null) {
                ItemCountChanged(ShellBrowser.GetItemCount());
            }
        }

        protected bool OnKeyDown(Keys key) {
            if(Config.Tips.ShowTooltipPreviews) {
                if(Config.Tips.ShowPreviewsWithShift) {
                    if(key != Keys.ShiftKey) {
                        HideThumbnailTooltip(2);
                    }
                }
                else {
                    HideThumbnailTooltip(2);
                }
            }
            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(key != Keys.ShiftKey) {
                        HideSubDirTip(3);
                    }
                }
                else if(key != Keys.ControlKey) {
                    HideSubDirTip(3);
                }
            }

            if(Config.Tweaks.WrapArrowKeySelection && Control.ModifierKeys == Keys.None) {
                if(key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down) {
                    return HandleCursorLoop(key);
                }
            }
            
            return false;
        }

        protected bool OnMiddleClick(Point pt) {
            return MiddleClick != null && MiddleClick(pt);
        }

        protected bool OnMouseActivate(ref int result) {
            return MouseActivate != null && MouseActivate(ref result);
        }

        protected void OnSelectionChanged() {
            if(SelectionChanged != null) {
                SelectionChanged();
            }
        }

        protected virtual bool OnShellViewNotify(NMHDR nmhdr, ref Message msg) {
            if(nmhdr.hwndFrom != ListViewController.Handle) {
                if(nmhdr.code == -12 /*NM_CUSTOMDRAW*/ && nmhdr.idFrom == IntPtr.Zero) {
                    ResetTrackMouseEvent();
                }
            }
            return false;
        }

        public abstract override bool PointIsBackground(Point pt, bool screenCoords); 

        public override void RefreshSubDirTip(bool force = false) {
            if(fDragging) {
                OnDragOver(Control.MousePosition);
            }
            else if(Config.Tips.ShowSubDirTips && Control.MouseButtons == MouseButtons.None) {
                if((!Config.Tips.SubDirTipsWithShift ^ (Control.ModifierKeys == Keys.Shift)) && hwndExplorer == PInvoke.GetForegroundWindow()) {
                    int iItem = GetHotItem();
                    if(subDirTip != null && (subDirTip.MouseIsOnThis() || subDirTip.MenuIsShowing)) {
                        return;
                    }
                    if(!force && subDirIndex == iItem && (!QTUtility.IsXP || (iItem != -1))) {
                        return;
                    }
                    if(!QTUtility.IsXP) {
                        subDirIndex = iItem;
                    }
                    if(iItem > -1 && ShowSubDirTip(iItem, false, false)) {
                        if(QTUtility.IsXP) {
                            subDirIndex = iItem;
                        }
                        return;
                    }
                }
                HideSubDirTip(2);
                subDirIndex = -1;
            }
        }

        public void RemoteDispose() {
            PInvoke.PostMessage(Handle, WM_REMOTEDISPOSE, IntPtr.Zero, IntPtr.Zero);
        }

        private void ResetTrackMouseEvent() {
            if(fTrackMouseEvent) {
                fTrackMouseEvent = false;
                TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
                structure.cbSize = Marshal.SizeOf(structure);
                structure.dwFlags = 2;
                structure.hwndTrack = Handle;
                PInvoke.TrackMouseEvent(ref structure);
            }
        }

        public override void ScrollHorizontal(int amount) {
            if(ListViewController != null) {
                // We'll intercept this message later for the ItemsView.  It's
                // important to use PostMessage here to prevent reentry issues
                // with the Automation Thread.
                PInvoke.PostMessage(ListViewController.Handle, LVM.SCROLL, (IntPtr)(-amount), IntPtr.Zero);
            }
        }

        public override void SetFocus() {
            if(ListViewController != null) {
                PInvoke.SetFocus(ListViewController.Handle);
            }
        }

        public override void SetRedraw(bool redraw) {
            if(ListViewController != null) {
                PInvoke.SetRedraw(ListViewController.Handle, redraw);
            }
        }

        protected virtual bool ShellViewController_MessageCaptured(ref Message msg) {

            switch(msg.Msg) {
                case WM.MOUSEACTIVATE:
                    int res = (int)msg.Result;
                    bool ret = OnMouseActivate(ref res);
                    msg.Result = (IntPtr)res;
                    return ret;

                case WM.NOTIFY:
                    NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                    return OnShellViewNotify(nmhdr, ref msg);
            }
            return false;
        }

        public override void ShowAndClickSubDirTip() {
            try {
                Address[] addressArray;
                string str;
                if(ShellBrowser.TryGetSelection(out addressArray, out str, false) && ((addressArray.Length == 1) && !string.IsNullOrEmpty(addressArray[0].Path))) {
                    string path = addressArray[0].Path;
                    if(!path.StartsWith("::") && !Directory.Exists(path)) {
                        if(!Path.GetExtension(path).PathEquals(".lnk")) {
                            return;
                        }
                        path = ShellMethods.GetLinkTargetPath(path);
                        if(string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
                            return;
                        }
                    }
                    if(subDirTip == null) {
                        subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, true, this);
                        subDirTip.MenuClosed += subDirTip_MenuClosed;
                        subDirTip.MenuItemClicked += subDirTip_MenuItemClicked;
                        subDirTip.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                        subDirTip.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                        subDirTip.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                    }

                    int iItem = ShellBrowser.GetFocusedIndex();
                    if(iItem != -1) {
                        ShowSubDirTip(iItem, true, false);
                        subDirTip.PerformClickByKey();
                    }
                }
            }
            catch {
            }
        }

        private bool ShowSubDirTip(int iItem, bool fByKey, bool fSkipForegroundCheck) {
            string str;
            if((fSkipForegroundCheck || (hwndExplorer == PInvoke.GetForegroundWindow())) && ShellBrowser.TryGetHotTrackPath(iItem, out str)) {
                bool flag = false;
                try {
                    if(!ShellMethods.TryMakeSubDirTipPath(ref str)) {
                        return false;
                    }
                    Point pnt = GetSubDirTipPoint(fByKey);
                    if(subDirTip == null) {
                        subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, true, this);
                        subDirTip.MenuClosed += subDirTip_MenuClosed;
                        subDirTip.MenuItemClicked += subDirTip_MenuItemClicked;
                        subDirTip.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                        subDirTip.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                        subDirTip.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                        if(dropTargetPassthrough != null) {
                            PInvoke.RegisterDragDrop(subDirTip.Handle, dropTargetPassthrough);
                        }
                    }
                    subDirTip.ShowSubDirTip(str, null, pnt);
                    flag = true;
                }
                catch {
                }
                return flag;
            }
            return false;
        }

        private bool ShowThumbnailTooltip(int iItem, Point pnt, bool fKey) {
            string linkTargetPath;
            if(ShellBrowser.TryGetHotTrackPath(iItem, out linkTargetPath)) {
                if((linkTargetPath.StartsWith("::") || linkTargetPath.StartsWith(@"\\")) || linkTargetPath.ToLower().StartsWith(@"a:\")) {
                    return false;
                }
                string ext = Path.GetExtension(linkTargetPath).ToLower();
                if(ext == ".lnk") {
                    linkTargetPath = ShellMethods.GetLinkTargetPath(linkTargetPath);
                    if(linkTargetPath.Length == 0) {
                        return false;
                    }
                    ext = Path.GetExtension(linkTargetPath).ToLower();
                }
                if(ThumbnailTooltipForm.ExtIsSupported(ext)) {
                    if(thumbnailTooltip == null) {
                        thumbnailTooltip = new ThumbnailTooltipForm();
                        thumbnailTooltip.ThumbnailVisibleChanged += thumbnailTooltip_ThumbnailVisibleChanged;
                        timer_Thumbnail = new Timer();
                        timer_Thumbnail.Interval = 400;
                        timer_Thumbnail.Tick += timer_Thumbnail_Tick;
                    }
                    if(thumbnailTooltip.IsShownByKey && !fKey) {
                        thumbnailTooltip.IsShownByKey = false;
                        return true;
                    }
                    thumbnailIndex = iItem;
                    thumbnailTooltip.IsShownByKey = fKey;
                    return thumbnailTooltip.ShowToolTip(linkTargetPath, pnt);
                }
                HideThumbnailTooltip(6);
            }
            return false;
        }

        public override bool SubDirTipMenuIsShowing() {
            return subDirTip != null && subDirTip.MenuIsShowing;
        }

        private void subDirTip_MenuClosed(object sender, EventArgs e) {
            if(SubDirTip_MenuClosed != null) {
                SubDirTip_MenuClosed(sender, e);
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(SubDirTip_MenuItemClicked != null) {
                SubDirTip_MenuItemClicked(sender, e);
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(SubDirTip_MenuItemRightClicked != null) {
                SubDirTip_MenuItemRightClicked(sender, e);
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            if(SubDirTip_MultipleMenuItemsClicked != null) {
                SubDirTip_MultipleMenuItemsClicked(sender, e);
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(SubDirTip_MultipleMenuItemsRightClicked != null) {
                SubDirTip_MultipleMenuItemsRightClicked(sender, e);
            }
        }

        private void thumbnailTooltip_ThumbnailVisibleChanged(object sender, QEventArgs e) {
            timer_Thumbnail.Enabled = false;
            if(e.Direction == ArrowDirection.Up) {
                fThumbnailPending = false;
            }
            else {
                fThumbnailPending = true;
                timer_Thumbnail.Enabled = true;
            }
        }

        private void timer_HoverSubDirTipMenu_Tick(object sender, EventArgs e) {
            timer_HoverSubDirTipMenu.Enabled = false;
            if(Control.MouseButtons != MouseButtons.None && !(subDirTip != null && subDirTip.IsMouseOnMenus)) {
                int iItem = GetHotItem();
                if(iItem == subDirIndex) {
                    return;
                }
                if(subDirTip != null) {
                    subDirTip.HideMenu();
                }
                // TODO: Check if the item is the Recycle Bin and deny if it is.
                // string.Equals(wrapper.Path, "::{645FF040-5081-101B-9F08-00AA002F954E}"
                if(ShowSubDirTip(iItem, false, true)) {
                    subDirIndex = iItem;
                    if(hwndExplorer != IntPtr.Zero) {
                        WindowUtils.BringExplorerToFront(hwndExplorer);
                    }
                    PInvoke.SetFocus(ListViewController.Handle);
                    PInvoke.SetForegroundWindow(ListViewController.Handle);
                    HideThumbnailTooltip();
                    subDirTip.ShowMenu();
                    return;
                }
            }
            if(subDirTip != null && !subDirTip.IsMouseOnMenus) {
                HideSubDirTip(10);
            }
        }

        private void timer_Thumbnail_Tick(object sender, EventArgs e) {
            timer_Thumbnail.Enabled = false;
            fThumbnailPending = false;
        }

        private class DropTargetPassthrough : _IDropTarget, IDisposable {
            private IntPtr passthrough;
            private ExtendedListViewCommon parent;
            private bool fDraggingOnListView;
            private Point pointLastDrag = new Point(0, 0);

            public DropTargetPassthrough(IntPtr passthrough, ExtendedListViewCommon parent) {
                this.passthrough = passthrough;
                Marshal.AddRef(passthrough);
                this.parent = parent;
                Pointer = Marshal.GetComInterfaceForObject(this, typeof(_IDropTarget));
            }

            public IntPtr Pointer { get; private set; }

            public int DragEnter(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
                fDraggingOnListView = parent.MouseIsOverListView();
                if(fDraggingOnListView) {
                    parent.OnDragBegin();
                }
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragEnter(pDataObj, grfKeyState, pt, ref pdwEffect);
                }
            }

            public int DragOver(int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
                if(pt != pointLastDrag) {
                    pointLastDrag = pt;
                    parent.OnDragOver(pt);
                }
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragOver(grfKeyState, pt, ref pdwEffect);
                }
            }

            public int DragLeave() {
                if(parent.subDirTip != null && !parent.subDirTip.IsMouseOnMenus) {
                    if((fDraggingOnListView && !parent.subDirTip.MouseIsOnThis())
                            || (!fDraggingOnListView && !parent.MouseIsOverListView())) {
                        parent.OnDragEnd();
                    }
                }
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragLeave();
                }
            }

            public int DragDrop(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
                parent.OnDragEnd();
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragDrop(pDataObj, grfKeyState, pt, ref pdwEffect);
                }
            }

            public void Dispose() {
                if(passthrough != IntPtr.Zero) {
                    Marshal.Release(passthrough);
                    passthrough = IntPtr.Zero;
                }
                if(Pointer != IntPtr.Zero) {
                    Marshal.Release(Pointer);
                    Pointer = IntPtr.Zero;
                }
            }

            private class DTWrapper : IDisposable {
                public DTWrapper(IntPtr pDropTarget) {
                    DropTarget = (_IDropTarget)Marshal.GetObjectForIUnknown(pDropTarget);
                }

                public _IDropTarget DropTarget { get; private set; }

                public void Dispose() {
                    if(DropTarget != null) {
                        Marshal.ReleaseComObject(DropTarget);
                        DropTarget = null;
                    }
                }
            }
        }
    }
}

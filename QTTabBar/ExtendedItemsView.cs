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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class ExtendedItemsView : ExtendedListViewCommon {
        private Point lastLButtonPoint;
        private Int64 lastLButtonTime;
        private Point lastMouseMovePoint;
        private CachedListItemElement hotElement;
        private CachedListItemElement focusedElement;

        internal ExtendedItemsView(ShellBrowserEx ShellBrowser, IntPtr hwndShellView, IntPtr hwndListView, IntPtr hwndSubDirTipMessageReflect)
                : base(ShellBrowser, hwndShellView, hwndListView, hwndSubDirTipMessageReflect) {
        }

        private AutomationElement ListItemElementFromPoint(AutomationElementFactory factory, Point pt) {
            if(PInvoke.WindowFromPoint(pt) != Handle) return null;
            AutomationElement elem = factory.FromPoint(pt);
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            elem = elem.GetParent();
            if(elem == null) return null;
            if(elem.GetClassName() == "UIItem") return elem;
            return null;
        }

        protected override IntPtr GetEditControl() {
            return WindowUtils.FindChildWindow(Handle, hwnd => PInvoke.GetClassName(hwnd) == "Edit");
        }

        public override int GetHotItem() {
            if(hotElement != null) return hotElement.Index;
            if(IsHotTrackingEnabled()) return -1;
            return HitTest(Control.MousePosition, true);
        }

        protected override Rectangle GetFocusedItemRect() {
            if(focusedElement == null) {
                RefreshCache();
                if(focusedElement == null) return new Rectangle(0, 0, 0, 0);
            }
            return focusedElement.FullRect;
        }

        /**
         * 获取子文件夹提示的位置
         */
        protected override Point GetSubDirTipPoint(bool fByKey) {
            CachedListItemElement targetElement;
            if(fByKey) {
                if(focusedElement == null) {
                    RefreshCache();
                }
                targetElement = focusedElement;
            }
            else {
                if(hotElement == null) {
                    RefreshCache();
                }
                targetElement = hotElement;
            }
            if(targetElement == null) return new Point(0, 0);

            int x, y;
            Point ret;
            switch(ShellBrowser.ViewMode) {
                case FVM.CONTENT: // 内容
                    y = targetElement.FullRect.Bottom;
                    x = targetElement.LabelRect.Left;
                    ret = new Point(x, y - 16);
                    break;

                case FVM.DETAILS: // 详细信息
                    x = targetElement.LabelRect.Right;
                    y = targetElement.LabelRect.Top;
                    y += (targetElement.LabelRect.Bottom - y) / 2;
                    ret = new Point(x - 16, y - 8);
                    break;

                case FVM.SMALLICON: // 小图标
                    x = targetElement.FullRect.Right;
                    y = targetElement.FullRect.Top;
                    x -= (targetElement.FullRect.Bottom - y) / 2;
                    y += (targetElement.FullRect.Bottom - y) / 2;
                    ret = new Point(x - 8, y - 8);
                    break;

                case FVM.TILE: // 平铺
                    y = targetElement.FullRect.Bottom;
                    x = targetElement.IconRect.Right;
                    ret = new Point(x - 16, y - 16);
                    break;

                case FVM.THUMBSTRIP: // 缩略图
                case FVM.THUMBNAIL:  // 略图    
                case FVM.ICON: // 图标
                    x = targetElement.FullRect.Right;
                    y = targetElement.IconRect.Bottom;
                    ret = new Point(x - 16, y - 16);
                    break;

                case FVM.LIST: // 列表
                    x = targetElement.FullRect.Right;
                    y = targetElement.FullRect.Bottom;
                    ret = new Point(x, y - 15);
                    break;

                default:
                    x = targetElement.FullRect.Right;
                    y = targetElement.FullRect.Bottom;
                    ret = new Point(x - 16, y - 16);
                    break;
            }
            PInvoke.ClientToScreen(Handle, ref ret);
            return ret;
        }

        private Point GetWindowPos() {
            RECT rect;
            PInvoke.GetWindowRect(Handle, out rect);
            return new Point(rect.left, rect.top);
        }

        // 使用箭头键时候环绕选择文件夹
        protected override bool HandleCursorLoop(Keys key) {
            int focusedIdx = ShellBrowser.GetFocusedIndex();
            int itemCount = ShellBrowser.GetItemCount();
            int selectMe = -1;
            FVM viewMode = ShellBrowser.ViewMode;
            switch(viewMode) {
                case FVM.CONTENT:
                case FVM.DETAILS:
                    if(key == Keys.Up && focusedIdx == 0) {
                        selectMe = itemCount - 1;
                    }
                    else if(key == Keys.Down && focusedIdx == itemCount - 1) {
                        selectMe = 0;
                    }
                    break;

                case FVM.ICON:
                case FVM.SMALLICON:
                case FVM.THUMBNAIL:
                case FVM.TILE:
                case FVM.LIST:
                    Keys KeyNextItem, KeyPrevItem, KeyNextPage, KeyPrevPage;
                    if(viewMode == FVM.LIST) {
                        KeyNextItem = Keys.Down;
                        KeyPrevItem = Keys.Up;
                        KeyNextPage = Keys.Right;
                        KeyPrevPage = Keys.Left;
                    }
                    else {
                        KeyNextItem = Keys.Right;
                        KeyPrevItem = Keys.Left;
                        KeyNextPage = Keys.Down;
                        KeyPrevPage = Keys.Up;
                    }
                    int pageCount = AutomationManager.DoQuery(factory => {
                        AutomationElement elem = factory.FromHandle(Handle);
                        if(elem == null) return -1;
                        return viewMode == FVM.LIST ? elem.GetRowCount() : elem.GetColumnCount();
                    });
                    if(pageCount == -1) return false;
                    int page = focusedIdx % pageCount;
                    if(key == KeyNextItem && (page == pageCount - 1 || focusedIdx == itemCount - 1)) {
                        selectMe = focusedIdx - page;
                    }
                    else if(key == KeyPrevItem && page == 0) {
                        selectMe = Math.Min(focusedIdx + pageCount - 1, itemCount - 1);
                    }
                    else if(key == KeyNextPage && focusedIdx + pageCount >= itemCount) {
                        selectMe = page;
                    }
                    else if(key == KeyPrevPage && focusedIdx < pageCount) {
                        int x = itemCount - focusedIdx - 1;
                        selectMe = x - x % pageCount + focusedIdx;
                    }
                    break;
            }
            if(selectMe >= 0) {
                ShellBrowser.SelectItem(selectMe);
                return true;
            }
            else {
                return false;
            }
        }

        // 处理Shift键盘事件
        public override void HandleShiftKey() {
            if(!Config.Tips.ShowPreviewsWithShift) {
                HideThumbnailTooltip(5);
            }

            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(MouseIsOverListView()) {
                        // HandleShiftKey is called by a Hook callback, which apparently causes
                        // problems with automation.  Use PostMessage to update the SubDirTip later.
                        hotElement = null;
                        PInvoke.PostMessage(Handle, (int) WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                    }
                }
                else if(!SubDirTipMenuIsShowing()) {
                    HideSubDirTip(6);
                }
            }
        }

        // hitTest的底层实现：当控件接收到触摸事件的时候，不管能不能处理事件，都会调用hitTest方法，此方法的底层实现是：
        // 1：先看自己是否能接受触摸事件  
        // 2：再看触摸点是否在自己身上 
        // 3：从后往前遍历子控件，拿到子控件后，再次重复1，2步骤，要把父控件上的坐标点转换为子控件坐标系下的点，再次执行hitTest方法。
        // 4：若是最后还没有找到合适的view，那么就return self，自己就是合适的view
        // 返回值：返回一个索引 index ，根据坐标点所在元素view位置来判断。 未找到返回-1
        public override int HitTest(Point pt, bool ScreenCoords) {
            if(hotElement != null) {
                Point pt2 = pt;
                if(ScreenCoords) {
                    PInvoke.ScreenToClient(Handle, ref pt2);
                }
                if(hotElement.FullRect.Contains(pt2)) {
                    return hotElement.Index;
                }
            }
            if(!ScreenCoords) {
                PInvoke.ClientToScreen(Handle, ref pt);
            }
            if(subDirTip != null && subDirTip.IsShowing && subDirTip.Bounds.Contains(pt)) {
                return subDirIndex;
            }
            if(PInvoke.WindowFromPoint(pt) != Handle) {
                return -1;
            }
            return AutomationManager.DoQuery(factory => {
                AutomationElement elem = ListItemElementFromPoint(factory, pt);
                if(elem == null) return -1;
                hotElement = new CachedListItemElement(elem, this);
                return hotElement.Index;
            });
        }

        public override bool HotItemIsSelected() {
            return AutomationManager.DoQuery(factory => {
                AutomationElement elem = ListItemElementFromPoint(factory, Control.MousePosition);
                return elem == null ? false : elem.IsSelected();
            });
        }

        public override bool IsTrackingItemName() {
            if(ShellBrowser.ViewMode != FVM.DETAILS) return true;
            Point pt = Control.MousePosition;
            if(hotElement != null) {
                PInvoke.ScreenToClient(Handle, ref pt);
                return hotElement.LabelRect.Contains(pt);
            }
            else {
                return AutomationManager.DoQuery(factory => {
                    AutomationElement elem = factory.FromPoint(pt);
                    return elem != null && elem.GetAutomationId() == "System.ItemNameDisplay";
                });
            }
        }

        private bool IsHotTrackingEnabled() {
            if(thumbnailTooltip != null && (thumbnailTooltip.IsShowing || fThumbnailPending)) {
                return true;
            }
            if(!fDragging && Config.Tips.ShowSubDirTips &&
                    (!Config.Tips.SubDirTipsWithShift || (Control.ModifierKeys & Keys.Shift) != 0) &&
                    Control.MouseButtons == MouseButtons.None &&
                    hwndExplorer == PInvoke.GetForegroundWindow()) {
                return true;
            }
            return false;
        }

        protected override bool ListViewController_MessageCaptured(ref Message msg) {
            if(msg.Msg == WM_AFTERPAINT) {
                RefreshCache();
            }
            else if(msg.Msg == WM_ISITEMSVIEW) {
                msg.Result = (IntPtr)1;
                return true;
            }

            if(base.ListViewController_MessageCaptured(ref msg)) {
                return true;
            }

            switch(msg.Msg) {
                case LVM.GETSELECTEDCOLUMN:
                {
                    QTUtility2.log("GETSELECTEDCOLUMN");
                    return true;
                }
                case LVM.SCROLL: {
                    int amount = msg.WParam.ToInt32();
                    SetRedraw(false);
                    AutomationManager.DoQuery(factory => {
                        AutomationElement elem = factory.FromHandle(Handle);
                        amount /= SystemInformation.MouseWheelScrollDelta;
                        bool dec = amount < 0;
                        if(dec) {
                            amount = -amount;
                        }
                        int lines = SystemInformation.MouseWheelScrollLines;
                        if(lines < 0) {
                            elem.ScrollHorizontal(dec
                                    ? ScrollAmount.LargeDecrement
                                    : ScrollAmount.LargeIncrement, amount);
                        }
                        else {
                            elem.ScrollHorizontal(dec
                                    ? ScrollAmount.SmallDecrement
                                    : ScrollAmount.SmallIncrement, amount*lines);
                        }
                        return 0;
                    });
                    SetRedraw(true);
                    return true;
                }

                case WM.MOUSEACTIVATE: {
                    int res = (int)msg.Result;
                    bool ret = OnMouseActivate(ref res);
                    msg.Result = (IntPtr)res;
                    return ret;
                }

                case WM.LBUTTONDOWN: {
                    // The ItemsView's window class doesn't have the CS_DBLCLKS
                    // class style, which means we won't be receiving the
                    // WM.LBUTTONDBLCLK message.  We'll just have to make do
                    // without...                    
                    Int64 now = DateTime.Now.Ticks;
                    Point pt = QTUtility2.PointFromLPARAM(msg.LParam);
                    if((now - lastLButtonTime)/10000 <= SystemInformation.DoubleClickTime) {
                        Size size = SystemInformation.DoubleClickSize;
                        if(Math.Abs(pt.X - lastLButtonPoint.X) <= size.Width) {
                            if(Math.Abs(pt.Y - lastLButtonPoint.Y) <= size.Height) {
                                lastLButtonTime = 0;
                                return OnDoubleClick(pt);
                            }
                        }
                    }
                    lastLButtonPoint = pt;
                    lastLButtonTime = now;
                    return false;
                }

                case WM.MOUSEMOVE: {
                    Point pt = QTUtility2.PointFromLPARAM(msg.LParam);
                    if(pt != lastMouseMovePoint) {
                        lastMouseMovePoint = pt;
                        if(focusedElement != null) {
                            if(hotElement == null && focusedElement.FullRect.Contains(pt)) {
                                PInvoke.PostMessage(Handle, WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                            }
                            else if(hotElement != null && hotElement.Index == focusedElement.Index && !focusedElement.FullRect.Contains(pt)) {
                                PInvoke.PostMessage(Handle, WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                            }
                        }
                    }
                    break;
                }

                case WM.MOUSELEAVE:
                    if(focusedElement != null) {
                        if(hotElement != null && hotElement.Index == focusedElement.Index) {
                            PInvoke.PostMessage(Handle, WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    break;
                
                case WM.KEYDOWN:
                    return OnKeyDown((Keys)msg.WParam);
                
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                    RefreshCache();
                    break;

                case WM.NOTIFY: {
                    NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                    if(nmhdr.code == -530 /* TTN_NEEDTEXT */) {
                        NMTTDISPINFO dispinfo = (NMTTDISPINFO)Marshal.PtrToStructure(msg.LParam, typeof(NMTTDISPINFO));
                        if((dispinfo.uFlags & 0x20 /* TTF_TRACK */) != 0) {
                            return OnGetInfoTip(ShellBrowser.GetFocusedIndex(), true);
                        }
                        else {
                            int i = GetHotItem();
                            if(i != -1 && IsTrackingItemName()) {
                                return OnGetInfoTip(i, false);
                            }
                        }
                    }
                    break;
                }
            }
            return false;
        }

        protected override void OnDragBegin() {
            base.OnDragBegin();
            RefreshCache();
        }

        protected override void OnDragEnd() {
            base.OnDragEnd();
            RefreshCache();
            RefreshSubDirTip(true);
        }

        protected override void OnDragOver(Point pt) {
            base.OnDragOver(pt);
            if(hotElement != null) {
                PInvoke.ScreenToClient(Handle, ref pt);
                if(!hotElement.FullRect.Contains(pt)) {
                    hotElement = null;
                }
            } 
        }

        public override bool PointIsBackground(Point pt, bool screenCoords) {
            if(!screenCoords) {
                PInvoke.ClientToScreen(ListViewController.Handle, ref pt);
            }
            return AutomationManager.DoQuery(factory => {
                AutomationElement elem = factory.FromPoint(pt);
                if(elem == null) return false;
                string className = elem.GetClassName();
                return className == "UIItemsView" || className == "UIGroupItem";
            });
        }

        private void RefreshCache() {
            int prevItem = hotElement == null ? -1 : hotElement.Index;
            hotElement = null;
            focusedElement = null;
            if(!IsHotTrackingEnabled()) return;
            bool hasFocus = HasFocus();
            bool hasMouse = MouseIsOverListView();
            int nextItem = AutomationManager.DoQuery(factory => {
                AutomationElement elem;
                if(hasFocus) {
                    elem = factory.FromKeyboardFocus();
                    if(elem != null) {
                        focusedElement = new CachedListItemElement(elem, this);
                    }
                }
                if(hasMouse) {
                    elem = ListItemElementFromPoint(factory, Control.MousePosition);
                    if(elem == null) return -1;
                    hotElement = new CachedListItemElement(elem, this);
                    return hotElement.Index;
                }
                return -1;
            });
            if(nextItem != prevItem) {
                OnHotItemChanged(nextItem);
            }
        }

        protected override bool ShellViewController_MessageCaptured(ref Message msg) {
            if(base.ShellViewController_MessageCaptured(ref msg)) {
                return true;
            }
            // QTUtility2.debugMessage(msg);
            if(msg.Msg == WM_ACTIVATESEL)
            {
                QTUtility2.log("WM_ACTIVATESEL");
                int mk = Marshal.ReadInt32(msg.WParam);
                Keys modKeys = Keys.None;
                if((mk & 0x04) != 0) modKeys |= Keys.Shift;
                if((mk & 0x08) != 0) modKeys |= Keys.Control;
                if((mk & 0x20) != 0) modKeys |= Keys.Alt;
                if(OnSelectionActivated(modKeys)) {
                    msg.Result = (IntPtr)1;
                }
                return true;
            }

            switch(msg.Msg) {
                /*case 8:  // SFVM_SELECTIONCHANGED          8 /* undocumented #1#
                    QTUtility2.log("SFVM_SELECTIONCHANGED msgWParam " + msg.WParam);
                    QTUtility2.log("SFVM_SELECTIONCHANGED msgLParam " + msg.LParam);
                    break;*/
                // Undocumented message that seems to be fired every time the 
                // selection changes.
                case WM.USER + 163:  // 1024 +163 = 1187  => 4A3
                    // var msgWParam = msg.WParam; //  0 
                    // var msgLParam = msg.LParam; //  0
                    // QTUtility2.log("selection changes msgWParam " + msg.WParam);
                    // QTUtility2.log("selection changes msgLParam " + msg.LParam);
                    OnSelectionChanged(ref msg);
                    break;
                
                // Undocumented message that seems to be fired every time the
                // item count changes.
                case WM.USER + 174:
                    OnItemCountChanged();
                    break;
            }
            return false;
        }



        private class CachedListItemElement {
            public CachedListItemElement(AutomationElement elem, ExtendedItemsView parent) {
                Point offset = parent.GetWindowPos();
                offset = new Point(-offset.X, -offset.Y);
                Index = elem.GetItemIndex();
                Rectangle rect = elem.GetBoundingRect();
                rect.Offset(offset);
                FullRect = rect;
                bool foundLabel = false;
                bool foundIcon = false;
                foreach(AutomationElement child in elem.GetChildren()) {
                    if(!foundLabel && child.GetAutomationId() == "System.ItemNameDisplay") {
                        rect = child.GetBoundingRect();
                        rect.Offset(offset);
                        LabelRect = rect;
                        if(foundIcon) break;
                        foundLabel = true;
                    }
                    else if(!foundIcon && child.GetClassName() == "UIImage") {
                        rect = child.GetBoundingRect();
                        rect.Offset(offset);
                        IconRect = rect;
                        if(foundLabel) break;
                        foundIcon = true;
                    }
                }
            }
            
            public int Index { get; private set; }
            public Rectangle FullRect { get; private set; }
            public Rectangle LabelRect { get; private set; }
            public Rectangle IconRect { get; private set; }
        }
    }
}

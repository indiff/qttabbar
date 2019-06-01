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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class DropDownMenuReorderable : DropDownMenuBase {
        private const int COUNT_SCROLL = 3;
        private ToolStripControlHost downButton;
        private ToolStripItem draggingItem;
        private bool fBlockItemAddRemove;
        private bool fCancelClosingAncestors;
        private bool fCheckOnEdgeClick;
        private bool fEnableScroll;
        private bool fNowScrollButtonsRequired;
        private bool fReordered;
        private bool fReorderEnabled;
        private bool fSpaceKeyExecute;
        private bool fSuppressMouseMove;
        protected bool fSuppressMouseMove_Scroll;
        private bool fSuppressMouseMoveOnce;
        private bool fVirtualMode;
        private int iSuppressStartIndex;
        private List<string> lstProhibitedKeys;
        private Dictionary<string, List<ToolStripItem>> MenuItemGroups;
        public IntPtr MessageParent;
        private static MethodInfo miScroll;
        private MouseButtons mouseButtons;
        private string pathParent;
        private static PropertyInfo piScroll;
        private static PropertyInfo piScrollButtonDn;
        private static PropertyInfo piScrollButtonUp;
        private Stack<ToolStripItem> stcVirtualItems_Bottom;
        private Stack<ToolStripItem> stcVirtualItems_Top;
        private ToolStripMenuItem tsmiPreviousEdgeClicked;
        private ToolStripControlHost upButton;
        private const int VIRTUAL_MAX_CONTAINEDITEMCOUNT = 0x40;
        private const int VIRTUAL_THRESHOLD_MINITEMCOUNT = 0x80;

        public event ItemRightClickedEventHandler ItemMiddleClicked;

        public event ItemRightClickedEventHandler ItemRightClicked;

        public event MouseEventHandler MouseDragMove;

        public event EventHandler MouseScroll;

        public event EventHandler MouseUpBeforeDrop;

        public event MenuReorderedEventHandler ReorderFinished;

        public DropDownMenuReorderable(IContainer container)
            : base(container) {
            iSuppressStartIndex = -1;
            MenuItemGroups = new Dictionary<string, List<ToolStripItem>>();
            lstProhibitedKeys = new List<string>();
            fReorderEnabled = true;
            fEnableScroll = true;
        }

        public DropDownMenuReorderable(IContainer container, bool respondModKeys, bool enableShiftKey)
            : base(container, respondModKeys, enableShiftKey) {
            iSuppressStartIndex = -1;
            MenuItemGroups = new Dictionary<string, List<ToolStripItem>>();
            lstProhibitedKeys = new List<string>();
            fReorderEnabled = true;
            fEnableScroll = true;
        }

        public DropDownMenuReorderable(IContainer container, bool respondModKeys, bool enableShiftKey, bool enableReorder)
            : base(container, respondModKeys, enableShiftKey) {
            iSuppressStartIndex = -1;
            MenuItemGroups = new Dictionary<string, List<ToolStripItem>>();
            lstProhibitedKeys = new List<string>();
            fReorderEnabled = true;
            fEnableScroll = true;
            fReorderEnabled = enableReorder;
        }

        public void AddItem(ToolStripItem item, string key) {
            Items.Add(item);
            if(!MenuItemGroups.ContainsKey(key)) {
                MenuItemGroups[key] = new List<ToolStripItem>();
            }
            MenuItemGroups[key].Add(item);
        }

        public void AddItemsRange(ToolStripItem[] items, string key) {
            Items.AddRange(items);
            if(!MenuItemGroups.ContainsKey(key)) {
                MenuItemGroups[key] = new List<ToolStripItem>();
            }
            MenuItemGroups[key].AddRange(items);
        }

        public void AddItemsRangeVirtual(List<QMenuItem> lstItems) {
            if(lstItems.Count < 0x80) {
                fVirtualMode = false;
                Items.AddRange(lstItems.ToArray());
            }
            else {
                fVirtualMode = true;
                if(stcVirtualItems_Top == null) {
                    stcVirtualItems_Top = new Stack<ToolStripItem>();
                    stcVirtualItems_Bottom = new Stack<ToolStripItem>();
                }
                ToolStripMenuItem[] toolStripItems = new ToolStripMenuItem[0x40];
                for(int i = lstItems.Count - 1; i > -1; i--) {
                    if(i < 0x40) {
                        toolStripItems[i] = lstItems[i];
                    }
                    else {
                        stcVirtualItems_Bottom.Push(lstItems[i]);
                    }
                }
                Items.AddRange(toolStripItems);
            }
        }

        protected void CancelClosingAncestors(bool fCancel, bool fClose) {
            fCancelClosingAncestors = fCancel;
            DropDownMenuReorderable owner = null;
            if((OwnerItem != null) && (OwnerItem is ToolStripDropDownItem)) {
                owner = OwnerItem.Owner as DropDownMenuReorderable;
            }
            if(!fCancel && fClose) {
                Close(ToolStripDropDownCloseReason.ItemClicked);
            }
            if(owner != null) {
                owner.CancelClosingAncestors(fCancel, fClose);
            }
        }

        private void ChangeSelection(ToolStripItem tsi) {
            typeof(ToolStripDropDownMenu).GetMethod("ChangeSelection", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { tsi });
            typeof(ToolStrip).GetMethod("ClearAllSelectionsExcept", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, new object[] { tsi });
        }

        private void CloseChildDropDown() {
            foreach(ToolStripMenuItem item in Items.OfType<ToolStripMenuItem>()
                    .Where(item => item.HasDropDownItems && item.DropDown.Visible)) {
                item.DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                break;   
            }
        }

        private bool ContainsInIdenticalMenuGroup(ToolStripItem dragging, ToolStripItem covered) {
            if(!fReorderEnabled) {
                return false;
            }
            if(dragging is ToolStripSeparator) {
                return false;
            }
            string item = string.Empty;
            string str2 = string.Empty;
            foreach(string str3 in MenuItemGroups.Keys) {
                if(MenuItemGroups[str3].Contains(dragging)) {
                    item = str3;
                }
                if(MenuItemGroups[str3].Contains(covered)) {
                    str2 = str3;
                }
            }
            return (((item.Length != 0) && (str2.Length != 0)) && ((item == str2) && !lstProhibitedKeys.Contains(item)));
        }

        protected override void Dispose(bool disposing) {
            DisposeVirtual(true);
            base.Dispose(disposing);
        }

        private void DisposeVirtual(bool disposing) {
            if(stcVirtualItems_Top != null) {
                while(stcVirtualItems_Top.Count > 0) {
                    stcVirtualItems_Top.Pop().Dispose();
                }
            }
            if(stcVirtualItems_Bottom != null) {
                while(stcVirtualItems_Bottom.Count > 0) {
                    stcVirtualItems_Bottom.Pop().Dispose();
                }
            }
        }

        private void GetScrollButtons() {
            try {
                if(piScrollButtonUp == null) {
                    piScrollButtonUp = typeof(ToolStripDropDownMenu).GetProperty("UpScrollButton", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if(piScrollButtonDn == null) {
                    piScrollButtonDn = typeof(ToolStripDropDownMenu).GetProperty("DownScrollButton", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                upButton = (ToolStripControlHost)piScrollButtonUp.GetValue(this, null);
                downButton = (ToolStripControlHost)piScrollButtonDn.GetValue(this, null);
            }
            catch {
            }
        }

        private bool HandleArrowKeyVirtual(bool fUp) {
            int num = -1;
            for(int i = 0; i < Items.Count; i++) {
                if(Items[i].Selected) {
                    num = i;
                }
            }
            if(num != -1) {
                bool flag = (fUp && (stcVirtualItems_Top.Count == 0)) || (!fUp && (stcVirtualItems_Bottom.Count == 0));
                bool flag2 = (fUp && (num == 0)) || (!fUp && (num == (Items.Count - 1)));
                bool flag3 = ((fUp && (-1 < num)) && (num < 2)) || ((!fUp && (-1 < num)) && (num > (Items.Count - 3)));
                if(!fUp) {
                    ToolStripItem nextItem = null;
                    if(flag2) {
                        if(flag) {
                            ScrollEndVirtual(!fUp);
                            Select(true, !fUp);
                            return true;
                        }
                        fBlockItemAddRemove = true;
                        SuspendLayout();
                        ToolStripItem item2 = Items[0];
                        Items.RemoveAt(0);
                        stcVirtualItems_Top.Push(item2);
                        nextItem = stcVirtualItems_Bottom.Pop();
                        Items.Add(nextItem);
                        ResumeLayout();
                        fBlockItemAddRemove = false;
                    }
                    if(nextItem == null) {
                        nextItem = GetNextItem(Items[num], ArrowDirection.Down);
                    }
                    ChangeSelection(nextItem);
                    return true;
                }
                if(flag3) {
                    if(flag) {
                        if(flag2) {
                            ScrollEndVirtual(!fUp);
                            Select(true, !fUp);
                            return true;
                        }
                    }
                    else {
                        fBlockItemAddRemove = true;
                        SuspendLayout();
                        ToolStripItem item = Items[Items.Count - 1];
                        Items.RemoveAt(Items.Count - 1);
                        stcVirtualItems_Bottom.Push(item);
                        ToolStripItem item4 = stcVirtualItems_Top.Pop();
                        Items.Insert(0, item4);
                        ResumeLayout();
                        fBlockItemAddRemove = false;
                    }
                }
            }
            return false;
        }

        private void HandlePageKeys(Keys keys) {
            bool fUp = keys == Keys.Prior;
            int num = -1;
            int num2 = -1;
            Rectangle displayRectangle = DisplayRectangle;
            for(int i = 0; i < Items.Count; i++) {
                if(num == -1) {
                    if(Items[i].Bounds.Y >= displayRectangle.Y) {
                        num = i;
                    }
                }
                else if(displayRectangle.Bottom < Items[i].Bounds.Bottom) {
                    num2 = i - 1;
                    break;
                }
            }
            if(num2 == -1) {
                num2 = Items.Count - 1;
            }
            if((fUp && (num > -1)) && !Items[num].Selected) {
                Items[num].Select();
            }
            else if((!fUp && (num2 > -1)) && !Items[num2].Selected) {
                Items[num2].Select();
            }
            else {
                if(num != -1) {
                    int count = (num2 - num) + 1;
                    if(count > 0) {
                        int num5 = fUp ? (num - count) : (num2 + count);
                        if((-1 < num5) && (num5 < Items.Count)) {
                            ChangeSelection(Items[num5]);
                            return;
                        }
                        ScrollMenu(fUp, count);
                    }
                }
                for(int j = 0; j < Items.Count; j++) {
                    if(fUp && (Items[j].Bounds.Y >= displayRectangle.Y)) {
                        Items[j].Select();
                        return;
                    }
                    if(!fUp && (displayRectangle.Bottom < Items[j].Bounds.Bottom)) {
                        if(j > 0) {
                            Items[j - 1].Select();
                        }
                        return;
                    }
                }
                if(!fUp) {
                    Items[Items.Count - 1].Select();
                }
            }
        }

        private void HideToolTip() {
            if(ShowItemToolTips) {
                BindingFlags bindingAttr = BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance;
                try {
                    typeof(ToolStrip).GetMethod("UpdateToolTip", bindingAttr).Invoke(this, new object[1]);
                    Type type = Type.GetType("System.Windows.Forms.MouseHoverTimer, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    PropertyInfo property = typeof(ToolStrip).GetProperty("MouseHoverTimer", bindingAttr);
                    type.GetMethod("Cancel", Type.EmptyTypes).Invoke(property.GetValue(this, null), null);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
            }
        }

        public void InsertItem(int index, ToolStripItem item, string key) {
            Items.Insert(index, item);
            if(!MenuItemGroups.ContainsKey(key)) {
                MenuItemGroups[key] = new List<ToolStripItem>();
            }
            MenuItemGroups[key].Add(item);
        }

        private static bool IsPseudoMnemonic(char charCode, string text) {
            if(!string.IsNullOrEmpty(text)) {
                char ch = char.ToUpper(charCode, CultureInfo.CurrentCulture);
                if((char.ToUpper(text[0], CultureInfo.CurrentCulture) == ch) || (char.ToLower(charCode, CultureInfo.CurrentCulture) == char.ToLower(text[0], CultureInfo.CurrentCulture))) {
                    return true;
                }
            }
            return false;
        }

        public void ItemsClear() {
            MenuItemGroups.Clear();
            Items.Clear();
        }

        public void ItemsClearVirtual() {
            Items.Clear();
            DisposeVirtual(false);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e) {
            UpdateToolTipByKey(null);
            draggingItem = null;
            fSuppressMouseMove_Scroll = false;
            fBlockItemAddRemove = false;
            fReordered = false;
            tsmiPreviousEdgeClicked = null;
            if(fCancelClosingAncestors) {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e) {
            if(!fBlockItemAddRemove) {
                base.OnItemAdded(e);
            }
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e) {
            if(fSuppressMouseMove) {
                fSuppressMouseMove = false;
                if(Items.IndexOf(e.ClickedItem) != iSuppressStartIndex) {
                    if(MouseUpBeforeDrop != null) {
                        MouseUpBeforeDrop(this, EventArgs.Empty);
                    }
                    return;
                }
                iSuppressStartIndex = -1;
            }
            if(fCheckOnEdgeClick) {
                ToolStripMenuItem clickedItem = e.ClickedItem as ToolStripMenuItem;
                if(clickedItem != null) {
                    Rectangle rectangle = RectangleToScreen(e.ClickedItem.Bounds);
                    if(RightToLeft == RightToLeft.Yes) {
                        int num = fImageMarginModified ? 0x1f : 0x19;
                        rectangle.X += rectangle.Width - num;
                        rectangle.Width = num;
                    }
                    else {
                        rectangle.Width = fImageMarginModified ? 0x1f : 0x19;
                    }
                    if(rectangle.Contains(MousePosition)) {
                        clickedItem.Checked = !clickedItem.Checked;
                        if((ModifierKeys == Keys.Shift) && (tsmiPreviousEdgeClicked != null)) {
                            int index = Items.IndexOf(clickedItem);
                            int num3 = Items.IndexOf(tsmiPreviousEdgeClicked);
                            if(((index != num3) && (index != -1)) && (num3 != -1)) {
                                int num4 = Math.Min(index, num3);
                                int num5 = Math.Max(index, num3);
                                if(num5 < Items.Count) {
                                    bool flag = clickedItem.Checked;
                                    for(int i = num4; i <= num5; i++) {
                                        ToolStripMenuItem item2 = Items[i] as ToolStripMenuItem;
                                        if(item2 != null) {
                                            item2.Checked = flag;
                                        }
                                    }
                                }
                            }
                        }
                        tsmiPreviousEdgeClicked = clickedItem;
                        return;
                    }
                }
            }
            if(!(e.ClickedItem is DirectoryMenuItem) || (((DirectoryMenuItem)e.ClickedItem).DropDownItems.Count != 0)) {
                base.OnItemClicked(e);
            }
        }

        protected virtual void OnItemMiddleClicked(ItemRightClickedEventArgs e) {
            if(ItemMiddleClicked != null) {
                ItemMiddleClicked(this, e);
                if(e.HRESULT != 0xffff) {
                    Close(ToolStripDropDownCloseReason.ItemClicked);
                }
            }
        }

        protected override void OnItemRemoved(ToolStripItemEventArgs e) {
            if(!fBlockItemAddRemove) {
                base.OnItemRemoved(e);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            if((e.KeyCode == Keys.Apps) && (ItemRightClicked != null)) {
                foreach(ToolStripItem item in Items) {
                    if(item.Selected) {
                        if((item is ToolStripMenuItem) && ((ToolStripMenuItem)item).HasDropDownItems) {
                            ((ToolStripMenuItem)item).DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                        }
                        CancelClosingAncestors(true, false);
                        ItemRightClickedEventArgs args = new ItemRightClickedEventArgs(item, true, PointToScreen(new Point(item.Bounds.X, item.Bounds.Y + item.Height)));
                        ItemRightClicked(this, args);
                        CancelClosingAncestors(false, (args.HRESULT != 0xffff) && (args.HRESULT != 0xfffd));
                        break;
                    }
                }
            }
            else if((e.KeyCode == Keys.Space) && fSpaceKeyExecute) {
                foreach(ToolStripItem item2 in base.Items) {
                    if(item2.Selected) {
                        OnItemClicked(new ToolStripItemClickedEventArgs(item2));
                        return;
                    }
                }
            }
            base.OnKeyUp(e);
        }

        protected override void OnLayout(LayoutEventArgs e) {
            base.OnLayout(e);
            if(fEnableScroll) {
                try {
                    if(piScroll == null) {
                        piScroll = typeof(ToolStripDropDownMenu).GetProperty("RequiresScrollButtons", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance);
                    }
                    fNowScrollButtonsRequired = (bool)piScroll.GetValue(this, null);
                    if(fNowScrollButtonsRequired && (upButton == null)) {
                        GetScrollButtons();
                    }
                }
                catch {
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if(fReorderEnabled) {
                mouseButtons = e.Button;
                draggingItem = GetItemAt(e.Location);
                if((draggingItem != null) && string.Equals(draggingItem.GetType().ToString(), "System.Windows.Forms.ToolStripScrollButton")) {
                    draggingItem = null;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if(((e.Button == mouseButtons) && (mouseButtons == MouseButtons.Left)) && (draggingItem != null)) {
                ToolStripItem itemAt = GetItemAt(e.Location);
                if((itemAt != null) && (itemAt != draggingItem)) {
                    if(ContainsInIdenticalMenuGroup(draggingItem, itemAt)) {
                        int index = Items.IndexOf(itemAt);
                        if(index != -1) {
                            if((draggingItem is ToolStripDropDownItem) && (((ToolStripDropDownItem)draggingItem).DropDownItems.Count > 0)) {
                                ((ToolStripDropDownItem)draggingItem).DropDown.Hide();
                            }
                            fBlockItemAddRemove = true;
                            SuspendLayout();
                            Items.Remove(draggingItem);
                            Items.Insert(index, draggingItem);
                            ResumeLayout();
                            fBlockItemAddRemove = false;
                            fReordered = true;
                        }
                    }
                    else {
                        fReordered = true;
                    }
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if((fReordered && (ReorderFinished != null)) && (draggingItem != null)) {
                if(fReorderEnabled) {
                    ReorderFinished(this, new ToolStripItemClickedEventArgs(draggingItem));
                }
                draggingItem = null;
            }
            ToolStripItem itemAt = GetItemAt(e.Location);
            if((!fReordered && (itemAt != null)) && !(itemAt is ToolStripSeparator)) {
                if(e.Button == MouseButtons.Right) {
                    if(ItemRightClicked != null) {
                        itemAt.Select();
                        if((itemAt is ToolStripMenuItem) && ((ToolStripMenuItem)itemAt).HasDropDownItems) {
                            ((ToolStripMenuItem)itemAt).DropDown.Close(ToolStripDropDownCloseReason.AppFocusChange);
                        }
                        CancelClosingAncestors(true, false);
                        ItemRightClickedEventArgs args = new ItemRightClickedEventArgs(itemAt, false, e.Location);
                        ItemRightClicked(this, args);
                        CancelClosingAncestors(false, (args.HRESULT != 0xffff) && (args.HRESULT != 0xfffd));
                    }
                }
                else {
                    base.OnMouseUp(e);
                    if(e.Button == MouseButtons.Middle) {
                        OnItemMiddleClicked(new ItemRightClickedEventArgs(itemAt, false, e.Location));
                    }
                }
            }
            else {
                fReordered = false;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if(fEnableScroll && fNowScrollButtonsRequired) {
                Point mousePosition = MousePosition;
                if(Bounds.Contains(mousePosition)) {
                    ScrollMenu(e.Delta > 0, 3);
                }
                else {
                    Control control = FromHandle(PInvoke.WindowFromPoint(mousePosition));
                    if(control != null) {
                        DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                        if(reorderable != null) {
                            reorderable.OnMouseWheel(e);
                        }
                    }
                }
            }
            base.OnMouseWheel(e);
        }

        protected override void OnOpening(CancelEventArgs e) {
            if((OwnerItem != null) && ((OwnerItem.IsDisposed || (OwnerItem.Owner == null)) || OwnerItem.Owner.IsDisposed)) {
                e.Cancel = true;
            }
            else {
                base.OnOpening(e);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Prior:
                case Keys.Next:
                    HandlePageKeys(e.KeyCode);
                    e.IsInputKey = true;
                    return;

                case Keys.End:
                case Keys.Home:
                    if(fVirtualMode) {
                        ScrollEndVirtual(e.KeyCode == Keys.Home);
                    }
                    goto Label_014F;

                case Keys.Right:
                    if(!(OwnerItem is ToolStripDropDownButton)) {
                        foreach(ToolStripItem item in Items) {
                            if(item.Selected) {
                                if((item is ToolStripMenuItem) && !((ToolStripMenuItem)item).HasDropDownItems) {
                                    e.IsInputKey = true;
                                }
                                break;
                            }
                        }
                    }
                    goto Label_014F;

                case Keys.Return:
                    if(!fCancelClosingAncestors) {
                        foreach(ToolStripItem item2 in Items) {
                            if(item2.Selected) {
                                QMenuItem item3 = item2 as QMenuItem;
                                if((item3 != null) && item3.HasDropDownItems) {
                                    e.IsInputKey = true;
                                    item2.PerformClick();
                                }
                                break;
                            }
                        }
                        goto Label_014F;
                    }
                    e.IsInputKey = true;
                    break;

                default:
                    goto Label_014F;
            }
            return;
        Label_014F:
            base.OnPreviewKeyDown(e);
        }

        public override bool PreProcessMessage(ref Message msg) {
            if(fCancelClosingAncestors) {
                return false;
            }
            if(msg.Msg == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.WParam));
                switch(wParam) {
                    case Keys.Up:
                    case Keys.Down:
                        if(MouseScroll != null) {
                            MouseScroll(this, EventArgs.Empty);
                        }
                        if(fVirtualMode && HandleArrowKeyVirtual(wParam == Keys.Up)) {
                            return true;
                        }
                        break;
                }
            }
            return base.PreProcessMessage(ref msg);
        }

        protected override bool ProcessMnemonic(char charCode) {
            if((Visible && Enabled) && !fCancelClosingAncestors) {
                ToolStripItem item = null;
                int num = 0;
                for(int i = 0; i < DisplayedItems.Count; i++) {
                    if(DisplayedItems[i].Selected) {
                        item = DisplayedItems[i];
                        num = i;
                        break;
                    }
                }
                ToolStripItem item2 = null;
                int num3 = num;
                for(int j = 0; j < DisplayedItems.Count; j++) {
                    ToolStripItem item3 = DisplayedItems[num3];
                    num3 = (num3 + 1) % DisplayedItems.Count;
                    if((((item3 is ToolStripMenuItem) && !string.IsNullOrEmpty(item3.Text)) && (item3.Enabled && ((item3.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text))) && IsPseudoMnemonic(charCode, item3.Text)) {
                        if(item2 != null) {
                            if(item2 == item) {
                                item3.Select();
                            }
                            else {
                                item2.Select();
                            }
                            return true;
                        }
                        item2 = item3;
                    }
                }
                if(item2 != null) {
                    item2.PerformClick();
                    return true;
                }
            }
            return false;
        }

        private void ScrollEndVirtual(bool fUp) {
            fBlockItemAddRemove = true;
            SuspendLayout();
            if(fUp) {
                while(stcVirtualItems_Top.Count > 0) {
                    Items.Insert(0, stcVirtualItems_Top.Pop());
                }
                while(Items.Count > 0x40) {
                    ToolStripItem item = Items[Items.Count - 1];
                    Items.RemoveAt(Items.Count - 1);
                    stcVirtualItems_Bottom.Push(item);
                }
            }
            else {
                List<ToolStripItem> list = new List<ToolStripItem>();
                while(stcVirtualItems_Bottom.Count > 0) {
                    list.Add(stcVirtualItems_Bottom.Pop());
                }
                Items.AddRange(list.ToArray());
                while(Items.Count > 0x40) {
                    ToolStripItem item2 = Items[0];
                    Items.RemoveAt(0);
                    stcVirtualItems_Top.Push(item2);
                }
            }
            ResumeLayout();
            Refresh();
            fBlockItemAddRemove = false;
        }

        protected void ScrollMenu(bool fUp, int count) {
            fSuppressMouseMove_Scroll = true;
            fSuppressMouseMoveOnce = true;
            HideToolTip();
            SuspendLayout();
            int num = ScrollMenuCore(fUp, count);
            if((num < count) && fVirtualMode) {
                ScrollMenuVirtual(fUp, count - num);
            }
            ResumeLayout();
            Refresh();
            fSuppressMouseMove_Scroll = false;
            if(MouseScroll != null) {
                MouseScroll(this, EventArgs.Empty);
            }
        }

        private int ScrollMenuCore(bool fUp, int count) {
            if(count >= 1) {
                ToolStripControlHost host = fUp ? upButton : downButton;
                if((host != null) && host.Visible) {
                    Control control = host.Control;
                    if((control != null) && control.Enabled) {
                        CloseChildDropDown();
                        if(miScroll == null) {
                            miScroll = typeof(ToolStripDropDownMenu).GetMethod("ScrollInternal", BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(bool) }, null);
                        }
                        fSuspendPainting = true;
                        try {
                            miScroll.Invoke(this, new object[] { fUp });
                            for(int i = 1; i < count; i++) {
                                if(control.Enabled) {
                                    miScroll.Invoke(this, new object[] { fUp });
                                }
                                else {
                                    return i;
                                }
                            }
                            return count;
                        }
                        finally {
                            fSuspendPainting = false;
                        }
                    }
                }
            }
            return 0;
        }

        private bool ScrollMenuVirtual(bool fUp, int count) {
            if((fUp && (stcVirtualItems_Top.Count == 0)) || (!fUp && (stcVirtualItems_Bottom.Count == 0))) {
                return false;
            }
            fBlockItemAddRemove = true;
            CloseChildDropDown();
            for(int i = 0; i < count; i++) {
                if(fUp) {
                    ToolStripItem item = Items[Items.Count - 1];
                    Items.RemoveAt(Items.Count - 1);
                    stcVirtualItems_Bottom.Push(item);
                    ToolStripItem item2 = stcVirtualItems_Top.Pop();
                    Items.Insert(0, item2);
                    if(stcVirtualItems_Top.Count != 0) {
                        continue;
                    }
                    break;
                }
                ToolStripItem item3 = Items[0];
                Items.RemoveAt(0);
                stcVirtualItems_Top.Push(item3);
                ToolStripItem item4 = stcVirtualItems_Bottom.Pop();
                Items.Add(item4);
                if(stcVirtualItems_Bottom.Count == 0) {
                    break;
                }
            }
            fBlockItemAddRemove = false;
            return true;
        }

        public void UpdateToolTipByKey(ToolStripMenuItem item) {
            try {
                if(ShowItemToolTips) {
                    ToolTip tip = typeof(ToolStrip).GetProperty("ToolTip", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null) as ToolTip;
                    if(tip != null) {
                        tip.Hide(this);
                        tip.Active = false;
                        if((item != null) && !string.IsNullOrEmpty(item.ToolTipText)) {
                            tip.Active = true;
                            Point point = new Point(item.Bounds.Width / 2, item.Bounds.Top + 0x20);
                            tip.Show(item.ToolTipText, this, point, tip.AutoPopDelay);
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        protected override void WndProc(ref Message m) {
            if((MessageParent != IntPtr.Zero) && (((m.Msg == WM.INITMENUPOPUP) || (m.Msg == WM.DRAWITEM)) || (m.Msg == WM.MEASUREITEM))) {
                PInvoke.SendMessage(MessageParent, (uint)m.Msg, m.WParam, m.LParam);
            }
            else {
                if(m.Msg == WM.MOUSELEAVE) {
                    if(fCancelClosingAncestors) {
                        return;
                    }
                }
                else if(m.Msg == WM.MOUSEFIRST) {
                    if(fSuppressMouseMove_Scroll) {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    if(fSuppressMouseMove) {
                        if(MouseDragMove != null) {
                            MouseDragMove(this, new MouseEventArgs(MouseButtons, 0, QTUtility2.GET_X_LPARAM(m.LParam), QTUtility2.GET_Y_LPARAM(m.LParam), 0));
                        }
                        return;
                    }
                    if(fSuppressMouseMoveOnce) {
                        fSuppressMouseMoveOnce = false;
                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }

        public bool CanScroll {
            get {
                return (fEnableScroll && fNowScrollButtonsRequired);
            }
        }

        public bool CheckOnEdgeClick {
            set {
                fCheckOnEdgeClick = value;
            }
        }

        public string Path {
            get {
                return pathParent;
            }
            set {
                pathParent = value;
            }
        }

        public IList<string> ProhibitedKey {
            get {
                return lstProhibitedKeys;
            }
        }

        public bool ReorderEnabled {
            set {
                fReorderEnabled = value;
            }
        }

        public bool SpaceKeyExecute {
            set {
                fSpaceKeyExecute = value;
            }
        }

        public bool SuppressMouseMove {
            set {
                fSuppressMouseMove = value;
            }
        }

        public bool SuppressMouseMoveOnce {
            set {
                fSuppressMouseMoveOnce = value;
            }
        }

        public int SuppressStartIndex {
            set {
                iSuppressStartIndex = value;
            }
        }
    }
}

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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal class DropDownMenuBase : ToolStripDropDownMenu {
        private bool fChangeImageSelected;
        protected bool fEnableShiftKey;
        private static bool fFirstDropDownOpened;
        protected static bool fImageMarginModified;
        private bool fOnceKeyDown;
        private static bool fRendererInitialized;
        protected bool fRespondModKeys;
        protected bool fSuspendPainting;
        public const string IMAGEKEY_BACK = "back";
        private const string IMAGEKEY_CONTROL = "control";
        public const string IMAGEKEY_CURRENT = "current";
        public const string IMAGEKEY_FORWARD = "forward";
        private const string IMAGEKEY_SHIFT = "shift";
        private QMenuItem lastKeyImageChangedItem;
        private QMenuItem lastMouseActiveItem;
        private IntPtr lparamPreviousMouseMove;
        private List<QMenuItem> lstQMIResponds;
        private static ToolStripRenderer menuRenderer;
        private static int nCurrentRenderer = -1;

        private static event EventHandler menuRendererChanged;

        public DropDownMenuBase(IContainer container) {
            lstQMIResponds = new List<QMenuItem>();
            if(!fRendererInitialized) {
                fRendererInitialized = true;
                InitializeMenuRenderer();
            }
            if(container != null) {
                container.Add(this);
            }
            Renderer = menuRenderer;
            menuRendererChanged = (EventHandler)Delegate.Combine(menuRendererChanged, new EventHandler(DropDownMenuBase_menuRendererChanged));
        }

        public DropDownMenuBase(IContainer container, bool fRespondModKeys, bool fEnableShiftKey)
            : this(container) {
            this.fRespondModKeys = fRespondModKeys;
            this.fEnableShiftKey = fEnableShiftKey;
        }

        public DropDownMenuBase(IContainer container, bool fRespondModKeys, bool fEnableShiftKey, bool fChangeImageSelected)
            : this(container) {
            this.fRespondModKeys = fRespondModKeys;
            this.fEnableShiftKey = fEnableShiftKey;
            this.fChangeImageSelected = fChangeImageSelected;
        }

        protected override void Dispose(bool disposing) {
            menuRendererChanged = (EventHandler)Delegate.Remove(menuRendererChanged, new EventHandler(DropDownMenuBase_menuRendererChanged));
            lstQMIResponds.Clear();
            base.Dispose(disposing);
        }

        private void DropDownMenuBase_menuRendererChanged(object sender, EventArgs e) {
            if(InvokeRequired) {
                Invoke(new MethodInvoker(RefreshRenderer));
            }
            else {
                RefreshRenderer();
            }
        }

        public static void ExitMenuMode() {
            Type type = Type.GetType("System.Windows.Forms.ToolStripManager+ModalMenuFilter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            if(type != null) {
                type.GetMethod("ExitMenuMode", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            }
        }

        public static bool InitializeMenuRenderer() {
            bool flag = false;
            bool fVista = false;
            if(QTUtility.IsXP) {
                // TODO: the menu renderer is OS dependent now.  Not going to change.
                if(nCurrentRenderer != 1) {
                    menuRenderer = new XPMenuRenderer(true);
                    nCurrentRenderer = 1;
                    flag = true;
                }
            }
            else if(nCurrentRenderer != 2) {
                menuRenderer = new VistaMenuRenderer(true);
                nCurrentRenderer = 2;
                flag = fVista = true;
            }
            if(flag) {
                SetImageMargin(fVista);
                if(menuRendererChanged != null) {
                    menuRendererChanged(null, EventArgs.Empty);
                }
            }
            return flag;
        }

        private static bool IsCursorOnTheEdgeOfScreen(out AnchorStyles dir, out int marginFar) {
            Point mousePosition = MousePosition;
            Rectangle workingArea = Screen.FromPoint(mousePosition).WorkingArea;
            dir = AnchorStyles.None;
            marginFar = workingArea.Right - mousePosition.X;
            if(marginFar < 0x80) {
                dir |= AnchorStyles.Right;
            }
            if((workingArea.Bottom - mousePosition.Y) < 0x80) {
                dir |= AnchorStyles.Bottom;
            }
            return (dir != AnchorStyles.None);
        }

        private static bool IsQmiResponds(QMenuItem qmi) {
            return ((((qmi != null) && (qmi.Genre != MenuGenre.Application)) && (qmi.Genre != MenuGenre.RecentFile)) && (qmi.Target != MenuTarget.File));
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            fOnceKeyDown = false;
            ResetImageKeys();
            base.OnClosed(e);
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e) {
            QMenuItem qmi = e.Item as QMenuItem;
            if(IsQmiResponds(qmi)) {
                lstQMIResponds.Add(qmi);
            }
            base.OnItemAdded(e);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem.ImageKey != "current") {
                base.OnItemClicked(e);
            }
        }

        protected override void OnItemRemoved(ToolStripItemEventArgs e) {
            QMenuItem qmi = e.Item as QMenuItem;
            if(IsQmiResponds(qmi)) {
                lstQMIResponds.Remove(qmi);
            }
            base.OnItemRemoved(e);
        }

        protected override void OnOpening(CancelEventArgs e) {
            if(!fFirstDropDownOpened) {
                QTUtility.ImageListGlobal.Images.Add("control", Resources_Image.imgNewWindow);
                QTUtility.ImageListGlobal.Images.Add("shift", Resources_Image.imgNewTab);
                QTUtility.ImageListGlobal.Images.Add("back", Resources_Image.imgBack);
                QTUtility.ImageListGlobal.Images.Add("forward", Resources_Image.imgForward);
                QTUtility.ImageListGlobal.Images.Add("current", Resources_Image.imgCurrent);
                fFirstDropDownOpened = true;
            }
            base.OnOpening(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if(!fRespondModKeys) {
                base.OnPreviewKeyDown(e);
                return;
            }
            if(((fEnableShiftKey && e.Shift) || (e.Control || fChangeImageSelected)) && ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.Up))) {
                SuspendLayout();
                try {
                    int index;
                    ToolStripItem item = Items.Cast<ToolStripItem>().FirstOrDefault(item2 => item2.Selected);
                    if(item != null) {
                        QMenuItem item3 = item as QMenuItem;
                        if(item3 != null) {
                            if((item3.Genre == MenuGenre.Application) || (item3.Genre == MenuGenre.RecentFile)) {
                                base.OnPreviewKeyDown(e);
                                return;
                            }
                            item3.RestoreOriginalImage();
                        }
                        index = Items.IndexOf(item);
                    }
                    else if(e.KeyCode == Keys.Down) {
                        index = Items.Count - 1;
                    }
                    else {
                        index = 0;
                    }
                    if(index != -1) {
                        int num2;
                        if(e.KeyCode == Keys.Down) {
                            if(index == (Items.Count - 1)) {
                                num2 = 0;
                            }
                            else {
                                num2 = index + 1;
                            }
                            for(int i = 0; (Items[num2] is ToolStripSeparator) && (i < Items.Count); i++) {
                                if(num2 == (Items.Count - 1)) {
                                    num2 = 0;
                                }
                                else {
                                    num2++;
                                }
                            }
                        }
                        else {
                            if(index == 0) {
                                num2 = Items.Count - 1;
                            }
                            else {
                                num2 = index - 1;
                            }
                            for(int j = 0; (Items[num2] is ToolStripSeparator) && (j < Items.Count); j++) {
                                if(num2 == 0) {
                                    num2 = Items.Count - 1;
                                }
                                else {
                                    num2--;
                                }
                            }
                        }
                        if(Items[num2].Enabled) {
                            QMenuItem item4 = Items[num2] as QMenuItem;
                            if(((item4 != null) && (item4.Genre != MenuGenre.Application)) && (item4.Target == MenuTarget.Folder)) {
                                switch(ModifierKeys) {
                                    case Keys.Control:
                                        item4.ImageKey = "control";
                                        goto Label_0254;

                                    case Keys.Shift:
                                        item4.ImageKey = "shift";
                                        goto Label_0254;
                                }
                                item4.RestoreOriginalImage(false, item4.Genre == MenuGenre.Navigation);
                            }
                        }
                    }
                }
                finally {
                    ResumeLayout(false);
                }
            }
        Label_0254:
            base.OnPreviewKeyDown(e);
        }

        public override bool PreProcessMessage(ref Message msg) {
            if(msg.Msg == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.WParam));
                if((wParam == Keys.Escape) && ((OwnerItem == null) || !(OwnerItem is ToolStripMenuItem))) {
                    Close(ToolStripDropDownCloseReason.Keyboard);
                    base.PreProcessMessage(ref msg);
                    return true;
                }
            }
            return base.PreProcessMessage(ref msg);
        }

        private void RefreshRenderer() {
            Renderer = menuRenderer;
        }

        private void ResetImageKeys() {
            if(fRespondModKeys) {
                SuspendLayout();
                foreach(QMenuItem item in lstQMIResponds) {
                    item.RestoreOriginalImage();
                }
                ResumeLayout(false);
            }
        }

        private static void SetImageMargin(bool fVista) {
            if(fImageMarginModified != fVista) {
                fImageMarginModified = fVista;
                try {
                    typeof(ToolStripDropDownMenu).GetField("DefaultImageMarginWidth", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, fVista ? 0x1f : 0x19);
                }
                catch {
                }
            }
        }

        public bool UpdateToolTip_OnTheEdge(ToolStripItem item) {
            AnchorStyles styles;
            int num;
            if(ShowItemToolTips && IsCursorOnTheEdgeOfScreen(out styles, out num)) {
                try {
                    FieldInfo field = typeof(ToolStrip).GetField("currentlyActiveTooltipItem", BindingFlags.NonPublic | BindingFlags.Instance);
                    ToolStripItem item2 = (ToolStripItem)field.GetValue(this);
                    ToolTip tip = (ToolTip)typeof(ToolStrip).GetProperty("ToolTip", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this, null);
                    if(tip != null) {
                        if(item != item2) {
                            tip.Hide(this);
                            tip.Active = false;
                            field.SetValue(this, item);
                            IntPtr handle = PInvoke.GetCursor();
                            if(handle != IntPtr.Zero) {
                                Cursor cursor = new Cursor(handle);
                                tip.Active = true;
                                Point position = Cursor.Position;
                                position.Y += Cursor.Size.Height - cursor.HotSpot.Y;
                                using(Font font = SystemFonts.StatusFont) {
                                    using(Graphics graphics = CreateGraphics()) {
                                        SizeF ef = graphics.MeasureString(item.ToolTipText, font);
                                        if((num < ef.Width) || (styles == (AnchorStyles.Right | AnchorStyles.Bottom))) {
                                            position.X = Bounds.X - ((int)ef.Width);
                                        }
                                        else {
                                            position.X += 0x20;
                                        }
                                    }
                                }
                                tip.Show(item.ToolTipText, this, PointToClient(position), tip.AutoPopDelay);
                            }
                        }
                        return true;
                    }
                }
                catch {
                }
            }
            return false;
        }

        protected override void WndProc(ref Message m) {
            try {
                QMenuItem ownerItem;
                if(!fRespondModKeys) {
                    base.WndProc(ref m);
                    return;
                }
                int wParam = (int)((long)m.WParam);
                switch(m.Msg) {
                    case WM.KEYDOWN:
                        break;

                    case WM.KEYUP:
                        if(fOnceKeyDown && ((wParam == 0x10) || (wParam == 0x11))) {
                            bool flag2 = false;
                            foreach(QMenuItem item4 in lstQMIResponds.Where(item4 => item4.Selected)) {
                                if(item4.Enabled) {
                                    Keys modifierKeys = ModifierKeys;
                                    if(modifierKeys == Keys.Control) {
                                        item4.ImageKey = "control";
                                    }
                                    else if(fEnableShiftKey && (modifierKeys == Keys.Shift)) {
                                        item4.ImageKey = "shift";
                                    }
                                    else {
                                        item4.RestoreOriginalImage(fChangeImageSelected, false);
                                    }
                                    lastKeyImageChangedItem = item4;
                                }
                                flag2 = true;
                                break;
                            }
                            ownerItem = OwnerItem as QMenuItem;
                            if(ownerItem != null) {
                                DropDownMenuBase owner = ownerItem.Owner as DropDownMenuBase;
                                if((owner != null) && owner.Visible) {
                                    if(flag2) {
                                        PInvoke.SendMessage(owner.Handle, 0x2a3, IntPtr.Zero, IntPtr.Zero);
                                    }
                                    else {
                                        QTUtility2.SendCOPYDATASTRUCT(owner.Handle, (IntPtr)wParam, string.Empty, (IntPtr)1);
                                    }
                                }
                            }
                        }
                        goto Label_07C2;

                    case WM.MOUSEMOVE:
                        goto Label_0562;

                    case WM.MOUSELEAVE:
                        goto Label_072E;

                    case WM.PAINT:
                        if(fSuspendPainting) {
                            PInvoke.ValidateRect(m.HWnd, IntPtr.Zero);
                        }
                        else {
                            base.WndProc(ref m);
                        }
                        return;

                    case WM.COPYDATA: {
                            COPYDATASTRUCT copydatastruct = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                            ownerItem = GetItemAt(PointToClient(MousePosition)) as QMenuItem;
                            if(!(copydatastruct.dwData == IntPtr.Zero)) {
                                goto Label_04B6;
                            }
                            if(ownerItem == null) {
                                goto Label_0462;
                            }
                            Keys keys3 = ModifierKeys;
                            if((wParam == 0x11) && ((keys3 & Keys.Shift) != Keys.Shift)) {
                                ownerItem.ImageKey = "control";
                            }
                            else if((fEnableShiftKey && (wParam == 0x10)) && ((keys3 & Keys.Control) != Keys.Control)) {
                                ownerItem.ImageKey = "shift";
                            }
                            else {
                                ownerItem.RestoreOriginalImage(fChangeImageSelected, false);
                            }
                            lastKeyImageChangedItem = ownerItem;
                            goto Label_07C2;
                        }
                    default:
                        goto Label_07C2;
                }
                fOnceKeyDown = true;
                if((((int)((long)m.LParam)) & 0x40000000) == 0) {
                    if((wParam == 0x10) || (wParam == 0x11)) {
                        bool flag = false;
                        foreach(QMenuItem item2 in lstQMIResponds.Where(item2 => item2.Selected)) {
                            if(item2.Enabled) {
                                Keys keys = ModifierKeys;
                                if((wParam == 0x11) && ((keys & Keys.Shift) != Keys.Shift)) {
                                    item2.ImageKey = "control";
                                }
                                else if((fEnableShiftKey && (wParam == 0x10)) && ((keys & Keys.Control) != Keys.Control)) {
                                    item2.ImageKey = "shift";
                                }
                                else {
                                    item2.RestoreOriginalImage(fChangeImageSelected, false);
                                }
                                lastKeyImageChangedItem = item2;
                            }
                            flag = true;
                            break;
                        }
                        ownerItem = OwnerItem as QMenuItem;
                        if(ownerItem != null) {
                            DropDownMenuBase base2 = ownerItem.Owner as DropDownMenuBase;
                            if((base2 != null) && base2.Visible) {
                                if(flag) {
                                    PInvoke.SendMessage(base2.Handle, 0x2a3, IntPtr.Zero, IntPtr.Zero);
                                }
                                else {
                                    QTUtility2.SendCOPYDATASTRUCT(base2.Handle, (IntPtr)wParam, string.Empty, IntPtr.Zero);
                                }
                            }
                        }
                    }
                    else if((wParam == 13) && ((fEnableShiftKey && (ModifierKeys == Keys.Shift)) || (ModifierKeys == Keys.Control))) {
                        foreach(ToolStripItem item3 in Items) {
                            if(item3.Selected) {
                                if(item3.Enabled) {
                                    OnItemClicked(new ToolStripItemClickedEventArgs(item3));
                                }
                                break;
                            }
                        }
                    }
                }
                goto Label_07C2;
            Label_0462:
                ownerItem = OwnerItem as QMenuItem;
                if(ownerItem != null) {
                    DropDownMenuBase base4 = ownerItem.Owner as DropDownMenuBase;
                    if((base4 != null) && base4.Visible) {
                        QTUtility2.SendCOPYDATASTRUCT(base4.Handle, (IntPtr)wParam, string.Empty, IntPtr.Zero);
                    }
                }
                goto Label_07C2;
            Label_04B6:
                if(ownerItem != null) {
                    Keys keys4 = ModifierKeys;
                    if(keys4 == Keys.Control) {
                        ownerItem.ImageKey = "control";
                    }
                    else if(fEnableShiftKey && (keys4 == Keys.Shift)) {
                        ownerItem.ImageKey = "shift";
                    }
                    else {
                        ownerItem.RestoreOriginalImage(fChangeImageSelected, false);
                    }
                    lastKeyImageChangedItem = ownerItem;
                }
                else {
                    ownerItem = OwnerItem as QMenuItem;
                    if(ownerItem != null) {
                        DropDownMenuBase base5 = ownerItem.Owner as DropDownMenuBase;
                        if((base5 != null) && base5.Visible) {
                            QTUtility2.SendCOPYDATASTRUCT(base5.Handle, (IntPtr)wParam, string.Empty, (IntPtr)1);
                        }
                    }
                }
                goto Label_07C2;
            Label_0562:
                if((m.WParam == IntPtr.Zero) && (m.LParam == lparamPreviousMouseMove)) {
                    m.Result = IntPtr.Zero;
                    return;
                }
                lparamPreviousMouseMove = m.LParam;
                if((!fEnableShiftKey || ((wParam & 4) != 4)) && (((wParam & 8) != 8) && !fChangeImageSelected)) {
                    goto Label_07C2;
                }
                ToolStripItem itemAt = GetItemAt(QTUtility2.PointFromLPARAM(m.LParam));
                if(itemAt == null) {
                    base.WndProc(ref m);
                    return;
                }
                ownerItem = itemAt as QMenuItem;
                if(!IsQmiResponds(ownerItem)) {
                    goto Label_06F8;
                }
                if(ownerItem == lastMouseActiveItem) {
                    goto Label_07C2;
                }
                if(lstQMIResponds.Count > 0x1c) {
                    fSuspendPainting = true;
                }
                SuspendLayout();
                if(ownerItem.Enabled) {
                    switch(wParam) {
                        case 8:
                            ownerItem.ImageKey = "control";
                            goto Label_06AB;

                        case 4:
                            ownerItem.ImageKey = "shift";
                            goto Label_06AB;
                    }
                    if(((ownerItem.Genre == MenuGenre.Navigation) && (ownerItem.MenuItemArguments != null)) && (!ownerItem.MenuItemArguments.IsBack || (ownerItem.MenuItemArguments.Index != 0))) {
                        ownerItem.ImageKey = ownerItem.MenuItemArguments.IsBack ? "back" : "forward";
                    }
                    else {
                        ownerItem.RestoreOriginalImage();
                    }
                }
            Label_06AB:
                if(lastMouseActiveItem != null) {
                    lastMouseActiveItem.RestoreOriginalImage();
                }
                if((ownerItem != lastKeyImageChangedItem) && (lastKeyImageChangedItem != null)) {
                    lastKeyImageChangedItem.RestoreOriginalImage();
                    lastKeyImageChangedItem = null;
                }
                lastMouseActiveItem = ownerItem;
                fSuspendPainting = false;
                ResumeLayout(false);
                goto Label_07C2;
            Label_06F8:
                if(lastMouseActiveItem != null) {
                    lastMouseActiveItem.RestoreOriginalImage();
                    lastMouseActiveItem = null;
                }
                if(lastKeyImageChangedItem != null) {
                    lastKeyImageChangedItem.RestoreOriginalImage();
                    lastKeyImageChangedItem = null;
                }
                goto Label_07C2;
            Label_072E:
                ResetImageKeys();
                lastMouseActiveItem = null;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "MSG:" + m.Msg.ToString("X") + ", WPARAM:" + m.WParam.ToString("X") + ", LPARAM:" + m.LParam.ToString("X"));
            }
        Label_07C2:
            base.WndProc(ref m);
            fSuspendPainting = false;
        }

        public static ToolStripRenderer CurrentRenderer {
            get {
                return menuRenderer;
            }
        }
    }

    internal sealed class ContextMenuStripEx : ContextMenuStrip {
        private bool fDefaultShowCheckMargin;
        private static ToolStripRenderer menuRenderer;
        private static int nCurrentRenderer;

        private static event EventHandler menuRendererChanged;

        static ContextMenuStripEx() {
            InitializeMenuRenderer();
        }

        public ContextMenuStripEx(IContainer container, bool fShowCheckMargin) {
            if(container != null) {
                container.Add(this);
            }
            fDefaultShowCheckMargin = fShowCheckMargin;
            ShowCheckMargin = fShowCheckMargin || (nCurrentRenderer == 2);
            Renderer = menuRenderer;
            menuRendererChanged = (EventHandler)Delegate.Combine(menuRendererChanged, new EventHandler(ContextMenuStripEx_menuRendererChanged));
        }

        private void ContextMenuStripEx_menuRendererChanged(object sender, EventArgs e) {
            if(InvokeRequired) {
                Invoke(new MethodInvoker(SetRenderer));
            }
            else {
                SetRenderer();
            }
        }

        protected override void Dispose(bool disposing) {
            menuRendererChanged = (EventHandler)Delegate.Remove(menuRendererChanged, new EventHandler(ContextMenuStripEx_menuRendererChanged));
            base.Dispose(disposing);
        }

        public void EnsureHandleCreated() {
            if(!IsHandleCreated) {
                CreateHandle();
            }
        }

        public static void InitializeMenuRenderer() {
            bool changed = false;
            if(QTUtility.IsXP) {
                // TODO: the menu renderer is OS dependent now.  Not going to change.
                if(nCurrentRenderer != 1) {
                    menuRenderer = new XPMenuRenderer(false);
                    nCurrentRenderer = 1;
                    changed = true;
                }
            }
            else if(nCurrentRenderer != 2) {
                menuRenderer = new VistaMenuRenderer(false);
                nCurrentRenderer = 2;
                changed = true;
            }
            if(changed && (menuRendererChanged != null)) {
                menuRendererChanged(null, EventArgs.Empty);
            }
        }

        private void SetRenderer() {
            Renderer = menuRenderer;
            ShowCheckMargin = (nCurrentRenderer == 2) || fDefaultShowCheckMargin;
        }
    }
}

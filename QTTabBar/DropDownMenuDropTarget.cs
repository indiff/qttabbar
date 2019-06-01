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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BandObjectLib;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class DropDownMenuDropTarget : DropDownMenuReorderable {
        private Bitmap bmpInsertL;
        private Bitmap bmpInsertR;
        private DropTargetWrapper dropTargetWrapper;
        private static bool fContainsFileDropList;
        private bool fDrawDropTarget;
        private bool fDrivesContained;
        private bool fEnableShiftKeyTemp;
        private bool fEnterVirtualBottom;
        private bool fIsRootMenu;
        // TODO: the value of fKeyTargetIsThis seems pretty unintuitive.  Fix it.
        private bool fKeyTargetIsThis;
        private bool fRespondModKeysTemp;
        private bool fShownByKey;
        private bool fSuppressMouseUp;
        private bool fThisPathExists;
        private bool fTop;
        private IntPtr hwndDialogParent;
        private int iDDRetval;
        private int iItemDragOverRegion;
        private int iScrollLine;
        private ToolStripItem itemHover;
        private ToolStripItem itemKeyInsertionMarkPrev;
        private MethodInfo miUnselect;
        private string strDraggingDrive;
        private string strDraggingStartPath;
        private static string strExtExecutable;
        private string strTargetPath;
        private Timer timerScroll;

        public event EventHandler MenuDragEnter;

        public DropDownMenuDropTarget(IContainer container, bool respondModKeys, bool enableShiftKey, bool isRoot, IntPtr hwndDialogParent)
            : base(container, respondModKeys, enableShiftKey, false) {
            iDDRetval = -1;
            iItemDragOverRegion = -1;
            fIsRootMenu = isRoot;
            this.hwndDialogParent = hwndDialogParent;
            HandleCreated += DropDownMenuDropTarget_HandleCreated;
        }

        private void BeginScrollTimer(ToolStripItem item, Point pntClient) {
            int y = pntClient.Y;
            int height = item.Bounds.Height;
            if(CanScroll && ((y < ((height * 0.5) + 11.0)) || ((Height - (height + 11)) < y))) {
                if(timerScroll == null) {
                    timerScroll = new Timer();
                    timerScroll.Tick += timerScroll_Tick;
                }
                else if(timerScroll.Enabled) {
                    return;
                }
                timerScroll.Tag = y < ((height * 0.5) + 11.0);
                iScrollLine = 1;
                if((y < 0x10) || ((Height - 0x10) < y)) {
                    timerScroll.Interval = 100;
                    if((y < 9) || ((Height - 9) < y)) {
                        iScrollLine = 2;
                    }
                }
                else {
                    timerScroll.Interval = 250;
                }
                fSuppressMouseMove_Scroll = true;
                timerScroll.Enabled = false;
                timerScroll.Enabled = true;
            }
            else if(timerScroll != null) {
                timerScroll.Enabled = false;
            }
        }

        private void CloseAllDropDown() {
            foreach(QMenuItem item in DisplayedItems.OfType<QMenuItem>().Where(item => item.Selected)) {
                if(item.HasDropDownItems && item.DropDown.Visible) {
                    item.HideDropDown();
                }
                miUnselect.Invoke(item, null);
                break;
            }
        }

        private void CopyCutFiles(bool fCut) {
            List<string> lstPaths = new List<string>();
            DropDownMenuDropTarget root = GetRoot(this);
            if(root != null) {
                GetCheckedItem(root, lstPaths, fCut, true);
                if(lstPaths.Count == 0) {
                    foreach(ToolStripItem item in DisplayedItems) {
                        if(item.Selected) {
                            QMenuItem item2 = item as QMenuItem;
                            if((item2 != null) && !string.IsNullOrEmpty(item2.Path)) {
                                item2.IsCut = fCut;
                                lstPaths.Add(item2.Path);
                            }
                            break;
                        }
                    }
                }
                if(ShellMethods.SetClipboardFileDropPaths(lstPaths, fCut, hwndDialogParent)) {
                    fContainsFileDropList = true;
                }
            }
        }

        private void CopyFileNames(bool fPath) {
            List<string> lstPaths = new List<string>();
            DropDownMenuDropTarget root = GetRoot(this);
            if(root != null) {
                GetCheckedItem(root, lstPaths, false, true);
            }
            if(lstPaths.Count == 0) {
                foreach(ToolStripItem item in DisplayedItems) {
                    if(!item.Selected) {
                        continue;
                    }
                    QMenuItem item2 = item as QMenuItem;
                    if((item2 != null) && !string.IsNullOrEmpty(item2.Path)) {
                        string path = item2.Path;
                        if(!fPath) {
                            try {
                                path = System.IO.Path.GetFileName(path);
                            }
                            catch {
                            }
                        }
                        if(!string.IsNullOrEmpty(path)) {
                            QTUtility2.SetStringClipboard(path);
                            fContainsFileDropList = false;
                            itemKeyInsertionMarkPrev = null;
                            Invalidate();
                        }
                    }
                    break;
                }
            }
            else {
                string str = string.Empty;
                foreach(string str3 in lstPaths) {
                    if(fPath) {
                        str = str + str3 + Environment.NewLine;
                    }
                    else {
                        try {
                            str = str + System.IO.Path.GetFileName(str3) + Environment.NewLine;
                            continue;
                        }
                        catch {
                            continue;
                        }
                    }
                }
                if(str.Length > 0) {
                    QTUtility2.SetStringClipboard(str);
                    fContainsFileDropList = false;
                    itemKeyInsertionMarkPrev = null;
                    Invalidate();
                }
            }
        }

        private void DeleteFiles(bool fShiftKey) {
            List<string> lstPaths = new List<string>();
            DropDownMenuDropTarget root = GetRoot(this);
            if(root != null) {
                GetCheckedItem(root, lstPaths, false, false);
                if(lstPaths.Count == 0) {
                    foreach(ToolStripItem item in DisplayedItems) {
                        if(item.Selected) {
                            QMenuItem item2 = item as QMenuItem;
                            if((item2 != null) && !string.IsNullOrEmpty(item2.Path)) {
                                lstPaths.Add(item2.Path);
                            }
                            break;
                        }
                    }
                }
                ShellMethods.DeleteFile(lstPaths, fShiftKey, hwndDialogParent);
                if(QTUtility.IsXP) {
                    root.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
            }
        }

        protected override void Dispose(bool disposing) {
            if(dropTargetWrapper != null) {
                dropTargetWrapper.Dispose();
                dropTargetWrapper = null;
            }
            if(bmpInsertL != null) {
                bmpInsertL.Dispose();
                bmpInsertL = null;
            }
            if(bmpInsertR != null) {
                bmpInsertR.Dispose();
                bmpInsertR = null;
            }
            if(timerScroll != null) {
                timerScroll.Dispose();
                timerScroll = null;
            }
            base.Dispose(disposing);
        }

        private void DropDownMenuDropTarget_HandleCreated(object sender, EventArgs e) {
            dropTargetWrapper = new DropTargetWrapper(this);
            dropTargetWrapper.DragFileEnter += dropTargetWrapper_DragFileEnter;
            dropTargetWrapper.DragFileOver += dropTargetWrapper_DragFileOver;
            dropTargetWrapper.DragFileLeave += dropTargetWrapper_DragFileLeave;
            dropTargetWrapper.DragFileDrop += dropTargetWrapper_DragFileDrop;
            dropTargetWrapper.DragDropEnd += dropTargetWrapper_DragDropEnd;
            try {
                miUnselect = typeof(ToolStripItem).GetMethod("Unselect", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch {
            }
        }

        private void dropTargetWrapper_DragDropEnd(object sender, EventArgs e) {
            CancelClosingAncestors(false, false);
            ShowItemToolTips = true;
            Close(ToolStripDropDownCloseReason.AppFocusChange);
        }

        private int dropTargetWrapper_DragFileDrop(out IntPtr hwnd, out byte[] idlReal) {
            fRespondModKeys = fRespondModKeysTemp;
            fEnableShiftKey = fEnableShiftKeyTemp;
            hwnd = IntPtr.Zero;
            idlReal = null;
            try {
                if((itemHover != null) && !string.IsNullOrEmpty(strTargetPath)) {
                    byte[] iDLData = ShellMethods.GetIDLData(strTargetPath);
                    if((iDLData != null) && (iDLData.Length > 0)) {
                        idlReal = iDLData;
                        CancelClosingAncestors(true, false);
                        ShowItemToolTips = false;
                        return 0;
                    }
                }
            }
            finally {
                strDraggingDrive = null;
                strDraggingStartPath = null;
                strTargetPath = null;
                itemHover = null;
            }
            return -1;
        }

        private DragDropEffects dropTargetWrapper_DragFileEnter(IntPtr hDrop, Point pnt, int grfKeyState) {
            fRespondModKeysTemp = fRespondModKeys;
            fEnableShiftKeyTemp = fEnableShiftKey;
            fRespondModKeys = false;
            fEnableShiftKey = false;
            if(MenuDragEnter != null) {
                MenuDragEnter(this, EventArgs.Empty);
            }
            fDrivesContained = false;
            switch(QTTabBarClass.HandleDragEnter(hDrop, out strDraggingDrive, out strDraggingStartPath)) {
                case -1:
                    return DragDropEffects.None;

                case 0:
                    return DropTargetWrapper.MakeEffect(grfKeyState, 0);

                case 1:
                    return DropTargetWrapper.MakeEffect(grfKeyState, 1);

                case 2:
                    fDrivesContained = true;
                    return DragDropEffects.None;
            }
            return DragDropEffects.None;
        }

        private void dropTargetWrapper_DragFileLeave(object sender, EventArgs e) {
            fRespondModKeys = fRespondModKeysTemp;
            fEnableShiftKey = fEnableShiftKeyTemp;
            strDraggingDrive = null;
            strDraggingStartPath = null;
            strTargetPath = null;
            itemHover = null;
            fSuppressMouseUp = true;
            iItemDragOverRegion = -1;
            if(Bounds.Contains(MousePosition)) {
                ToolStripDropDown tsdd = this;
                while(tsdd.OwnerItem != null && tsdd.OwnerItem.GetCurrentParent() is ToolStripDropDown) {
                    tsdd = (ToolStripDropDown)tsdd.OwnerItem.GetCurrentParent();
                }
                tsdd.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }
            else {
                Invalidate();
            }
        }

        private void dropTargetWrapper_DragFileOver(object sender, DragEventArgs e) {
            int iSourceState = -1;
            Point point = PointToClient(new Point(e.X, e.Y));
            ToolStripItem itemAt = GetItemAt(point);
            bool flag = false;
            if(itemAt != null) {
                Rectangle bounds = itemAt.Bounds;
                bool flag2 = (bounds.Bottom - point.Y) >= (point.Y - bounds.Top);
                flag = fTop != flag2;
                fTop = flag2;
            }
            bool flag3 = ((fTop && (iItemDragOverRegion != 0)) || (!fTop && (iItemDragOverRegion != 1))) && (itemAt == Items[Items.Count - 1]);
            if((itemAt != itemHover) || flag3) {
                if(itemAt != null) {
                    iItemDragOverRegion = fTop ? 0 : 1;
                    QMenuItem item2 = itemAt as QMenuItem;
                    if(item2 != null) {
                        bool flag4 = item2 is SubDirTipForm.ToolStripMenuItemEx;
                        if((flag3 && !fTop) && ShellMethods.PathIsFolder(Path)) {
                            fDrawDropTarget = false;
                            strTargetPath = Path;
                            iSourceState = MakeDragOverRetval();
                            if((!flag4 && item2.HasDropDownItems) && !item2.DropDown.Visible) {
                                OnMouseLeave(EventArgs.Empty);
                                OnMouseMove(new MouseEventArgs(MouseButtons, 0, point.X, point.Y, 0));
                            }
                        }
                        else if(flag4) {
                            bool flag5;
                            if(PathIsExecutable(item2.Path, out flag5)) {
                                fDrawDropTarget = true;
                                if(flag5) {
                                    iSourceState = -1;
                                    CloseAllDropDown();
                                }
                                else {
                                    strTargetPath = item2.Path;
                                    item2.Select();
                                    iSourceState = 2;
                                }
                            }
                            else {
                                fDrawDropTarget = false;
                                if(ShellMethods.PathIsFolder(Path)) {
                                    strTargetPath = Path;
                                    iSourceState = MakeDragOverRetval();
                                }
                                CloseAllDropDown();
                            }
                        }
                        else if(ShellMethods.PathIsFolder(item2.TargetPath)) {
                            fDrawDropTarget = true;
                            strTargetPath = item2.TargetPath;
                            iSourceState = MakeDragOverRetval();
                            OnMouseLeave(EventArgs.Empty);
                            OnMouseMove(new MouseEventArgs(MouseButtons, 0, point.X, point.Y, 0));
                        }
                    }
                }
                flag = true;
            }
            else {
                iSourceState = iDDRetval;
            }
            if(itemAt != null) {
                BeginScrollTimer(itemAt, point);
            }
            itemHover = itemAt;
            iDDRetval = iSourceState;
            if(flag) {
                Invalidate();
            }
            if(iSourceState == -1) {
                strTargetPath = null;
                e.Effect = DragDropEffects.None;
            }
            else if(fDrivesContained) {
                e.Effect = DragDropEffects.Link;
            }
            else if(iSourceState == 2) {
                e.Effect = DragDropEffects.Copy;
            }
            else {
                e.Effect = DropTargetWrapper.MakeEffect(e.KeyState, iSourceState);
            }
        }

        private static void GetCheckedItem(DropDownMenuDropTarget ddmdtRoot, List<string> lstPaths, bool fCut, bool fSetCut) {
            foreach(QMenuItem item2 in ddmdtRoot.Items.OfType<QMenuItem>()) {
                if(item2.Checked) {
                    if(!string.IsNullOrEmpty(item2.Path)) {
                        lstPaths.Add(item2.Path);
                        if(fSetCut) {
                            item2.IsCut = fCut;
                        }
                    }
                    else if(fSetCut) {
                        item2.IsCut = false;
                    }
                    continue;
                }
                if(fSetCut) {
                    item2.IsCut = false;
                }
                if(item2.HasDropDownItems) {
                    GetCheckedItem((DropDownMenuDropTarget)item2.DropDown, lstPaths, fCut, fSetCut);
                }
            }
        }

        private static DropDownMenuDropTarget GetRoot(DropDownMenuDropTarget ddmdt) {
            if(ddmdt.fIsRootMenu) {
                return ddmdt;
            }
            ToolStripItem ownerItem = ddmdt.OwnerItem;
            if(ownerItem != null) {
                ToolStrip owner = ownerItem.Owner;
                if(owner != null) {
                    DropDownMenuDropTarget target = owner as DropDownMenuDropTarget;
                    if(target != null) {
                        return GetRoot(target);
                    }
                }
            }
            return null;
        }

        private bool IsKeyTargetItem(ToolStripItem item) {
            bool flag;
            SubDirTipForm.ToolStripMenuItemEx ex = item as SubDirTipForm.ToolStripMenuItemEx;
            return ((ex == null) || (PathIsExecutable(ex.Path, out flag) && !flag));
        }

        private int MakeDragOverRetval() {
            if(strTargetPath.PathEquals(strDraggingStartPath)) {
                return 3;
            }
            if((strDraggingDrive != null) && string.Equals(strDraggingDrive, strTargetPath.Substring(0, 3), StringComparison.OrdinalIgnoreCase)) {
                return 0;
            }
            return 1;
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e) {
            fSuppressMouseUp = false;
            fDrawDropTarget = false;
            iItemDragOverRegion = -1;
            fKeyTargetIsThis = false;
            itemKeyInsertionMarkPrev = null;
            base.OnClosed(e);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e) {
            if(fSuppressMouseUp) {
                fSuppressMouseUp = false;
            }
            else {
                base.OnItemClicked(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            fSuppressMouseUp = false;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            bool fKeyTargetIsThis = this.fKeyTargetIsThis;
            this.fKeyTargetIsThis = false;
            itemKeyInsertionMarkPrev = null;
            if(fKeyTargetIsThis) {
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnOpened(EventArgs e) {
            try {
                fThisPathExists = Directory.Exists(Path);
                fContainsFileDropList = ShellMethods.ClipboardContainsFileDropList(hwndDialogParent);
            }
            catch {
            }
            if(((((OwnerItem == null) || (OwnerItem.Owner == null)) || !OwnerItem.Owner.RectangleToScreen(OwnerItem.Bounds).Contains(MousePosition)) && (!fIsRootMenu || fShownByKey)) && (((DisplayedItems.Count > 0) && !IsKeyTargetItem(DisplayedItems[0])) && fContainsFileDropList)) {
                fKeyTargetIsThis = true;
                itemKeyInsertionMarkPrev = DisplayedItems[0];
            }
            base.OnOpened(e);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if(itemHover != null) {
                Rectangle bounds = itemHover.Bounds;
                if(fDrawDropTarget) {
                    return;
                }
                using(Pen pen = new Pen(Color.Black, 2f)) {
                    using(Pen pen2 = new Pen(Color.Black, 1f)) {
                        if(fTop) {
                            e.Graphics.DrawLine(pen, 3, bounds.Top, bounds.Right - 2, bounds.Top);
                            e.Graphics.DrawLine(pen2, 3, bounds.Top - 3, 3, bounds.Top + 2);
                            e.Graphics.DrawLine(pen2, 4, bounds.Top - 2, 4, bounds.Top + 1);
                            e.Graphics.DrawLine(pen2, (bounds.Right - 2), (bounds.Top - 3), (bounds.Right - 2), (bounds.Top + 2));
                            e.Graphics.DrawLine(pen2, (bounds.Right - 3), (bounds.Top - 2), (bounds.Right - 3), (bounds.Top + 1));
                        }
                        else {
                            e.Graphics.DrawLine(pen, 3, bounds.Bottom, bounds.Right - 2, bounds.Bottom);
                            e.Graphics.DrawLine(pen2, 3, bounds.Bottom - 3, 3, bounds.Bottom + 2);
                            e.Graphics.DrawLine(pen2, 4, bounds.Bottom - 2, 4, bounds.Bottom + 1);
                            e.Graphics.DrawLine(pen2, (bounds.Right - 2), (bounds.Bottom - 3), (bounds.Right - 2), (bounds.Bottom + 2));
                            e.Graphics.DrawLine(pen2, (bounds.Right - 3), (bounds.Bottom - 2), (bounds.Right - 3), (bounds.Bottom + 1));
                        }
                    }
                    return;
                }
            }
            if(itemKeyInsertionMarkPrev != null) {
                Bitmap bmpInsertR;
                Rectangle rectangle2 = itemKeyInsertionMarkPrev.Bounds;
                if(QTUtility.IsRTL) {
                    if(this.bmpInsertR == null) {
                        this.bmpInsertR = Resources_Image.imgInsertR;
                    }
                    bmpInsertR = this.bmpInsertR;
                }
                else {
                    if(bmpInsertL == null) {
                        bmpInsertL = Resources_Image.imgInsertL;
                    }
                    bmpInsertR = bmpInsertL;
                }
                e.Graphics.DrawImage(bmpInsertR, new Rectangle(2, rectangle2.Bottom - 6, 12, 12), new Rectangle(0, 0, 12, 12), GraphicsUnit.Pixel);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
            if(((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down)) && (fThisPathExists && fContainsFileDropList)) {
                for(int i = 0; i < DisplayedItems.Count; i++) {
                    ToolStripItem item = DisplayedItems[i];
                    if(item.Selected) {
                        int index = Items.IndexOf(item);
                        if(index != -1) {
                            int num3;
                            if(e.KeyCode == Keys.Up) {
                                if(index == 0) {
                                    num3 = Items.Count - 1;
                                }
                                else {
                                    num3 = index - 1;
                                }
                            }
                            else if(index == (Items.Count - 1)) {
                                num3 = 0;
                            }
                            else {
                                num3 = index + 1;
                            }
                            item = Items[num3];
                            if(((num3 == 0) && (e.KeyCode == Keys.Down)) && (!fEnterVirtualBottom && IsKeyTargetItem(Items[Items.Count - 1]))) {
                                itemKeyInsertionMarkPrev = Items[Items.Count - 1];
                                fEnterVirtualBottom = true;
                                fKeyTargetIsThis = true;
                                Invalidate();
                                return;
                            }
                            fEnterVirtualBottom = false;
                            if(!IsKeyTargetItem(item)) {
                                itemKeyInsertionMarkPrev = item;
                                fKeyTargetIsThis = true;
                                Invalidate();
                                return;
                            }
                            itemKeyInsertionMarkPrev = null;
                            if(fKeyTargetIsThis) {
                                Invalidate();
                            }
                            fKeyTargetIsThis = false;
                        }
                        break;
                    }
                }
            }
            base.OnPreviewKeyDown(e);
        }

        private void PasteFiles() {
            string pathTarget = fKeyTargetIsThis 
                    ? Path 
                    : (from ToolStripItem item in DisplayedItems
                       where item.Selected && item is QMenuItem
                       select ((QMenuItem)item).Path).FirstOrDefault();
            if(pathTarget == null) return;
            ShellMethods.PasteFile(pathTarget, hwndDialogParent);
            if(!QTUtility.IsXP) return;
            DropDownMenuDropTarget ddmdtRoot = GetRoot(this);
            if(ddmdtRoot != null) {
                ddmdtRoot.Close(ToolStripDropDownCloseReason.ItemClicked);
            }
        }

        private static bool PathIsExecutable(string path, out bool fLinkTargetIsNotDropTarget) {
            fLinkTargetIsNotDropTarget = false;
            if(string.IsNullOrEmpty(path)) {
                return false;
            }
            string extension = System.IO.Path.GetExtension(path);
            if(string.IsNullOrEmpty(extension)) {
                return false;
            }
            if(strExtExecutable == null) {
                strExtExecutable = Environment.GetEnvironmentVariable("PATHEXT") ??
                        ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC";
            }
            if(!extension.PathEquals(".lnk")) {
                return (strExtExecutable.IndexOf(extension, StringComparison.OrdinalIgnoreCase) != -1);
            }
            string linkTargetPath = ShellMethods.GetLinkTargetPath(path);
            if(File.Exists(linkTargetPath)) {
                string str3 = System.IO.Path.GetExtension(linkTargetPath);
                if(strExtExecutable.IndexOf(str3, StringComparison.OrdinalIgnoreCase) != -1) {
                    return true;
                }
            }
            fLinkTargetIsNotDropTarget = true;
            return true;
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
            bool flag = (((int)((long)m.LParam)) & 0x40000000) != 0;
            Keys keys = keyData & Keys.KeyCode;
            Keys keys2 = keyData & ~Keys.KeyCode;
            if(keys2 == Keys.Control) {
                if(flag) {
                    return true;
                }
                switch(keys) {
                    case Keys.V:
                        PasteFiles();
                        return true;

                    case Keys.C:
                        CopyCutFiles(false);
                        return true;

                    case Keys.X:
                        CopyCutFiles(true);
                        return true;
                }
            }
            switch(keys) {
                case Keys.Down:
                    if(fEnterVirtualBottom) {
                        return true;
                    }
                    break;

                case Keys.Up:
                    fEnterVirtualBottom = false;
                    break;

                case Keys.Delete:
                    if(!flag && ((keyData == Keys.Delete) || (keyData == (Keys.Shift | Keys.Delete)))) {
                        DeleteFiles(keyData != Keys.Delete);
                    }
                    return true;
            }
            int num = ((int)keyData) | 0x100000;
            if((num != Config.Keys.Shortcuts[0x1b]) && (num != Config.Keys.Shortcuts[0x1c])) {
                return base.ProcessCmdKey(ref m, keyData);
            }
            if(!flag) {
                CopyFileNames(num == Config.Keys.Shortcuts[0x1b]);
            }
            return true;
        }

        public void SetShowingByKey(bool value) {
            fShownByKey = value;
        }

        public void SetSuppressMouseUp() {
            fSuppressMouseUp = true;
        }

        private void timerScroll_Tick(object sender, EventArgs e) {
            timerScroll.Enabled = false;
            fSuppressMouseMove_Scroll = false;
            if(!IsDisposed && Visible) {
                ScrollMenu((bool)timerScroll.Tag, iScrollLine);
            }
        }
    }
}

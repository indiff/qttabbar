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
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class SubDirTipForm : Form {
        private IContainer components;
        private DropDownMenuDropTarget contextMenuSubDir;
        private string currentDir;
        private byte[] currentIDL;
        private ToolStripMenuItem draggingItem;
        private string draggingPath;
        private ExtComparer extComparer = new ExtComparer();
        private bool fClickClose;
        private bool fDesktop;
        private bool fDragStarted;
        private bool fDropHilitedOpened;
        private bool fShownByKey;
        private bool fSuppressThumbnail;
        private IntPtr hwndDialogParent;
        private IntPtr hwndMessageReflect;
        private bool isShowing;
        private int iThumbnailIndex = -1;
        private int iToolTipIndex = -1;
        private LabelEx lblSubDirBtn;
        private AbstractListView listView;
        private List<Rectangle> lstRcts = new List<Rectangle>();
        private List<string> lstTempDirectoryPaths = new List<string>();
        private bool menuIsShowing;
        private const string NAME_DUMMY = "dummy";
        private const string NAME_RECYCLEBIN = "$RECYCLE.BIN";
        private const string NAME_RECYLER = "RECYCLER";
        private const string NAME_VOLUMEINFO = "System Volume Information";
        private Point pntDragStart;
        private static Size SystemDragSize = SystemInformation.DragSize;
        private ThumbnailTooltipForm thumbnailTip;
        private Timer timerToolTipByKey;
        private ToolStripMeuItemComparer tsmiComparer;

        public event EventHandler MenuClosed;
        public event ToolStripItemClickedEventHandler MenuItemClicked;
        public event ItemRightClickedEventHandler MenuItemRightClicked;
        public event EventHandler MultipleMenuItemsClicked;
        public event ItemRightClickedEventHandler MultipleMenuItemsRightClicked;

        public SubDirTipForm(IntPtr hwndMessageReflect, bool fEnableShiftKeyOnDDMR, AbstractListView lvw) {
            listView = lvw;
            this.hwndMessageReflect = hwndMessageReflect;
            hwndDialogParent = listView.Handle;
            fDesktop = !fEnableShiftKeyOnDDMR;
            InitializeComponent();
            contextMenuSubDir.ImageList = QTUtility.ImageListGlobal;
            contextMenuSubDir.MessageParent = hwndMessageReflect;
            IntPtr handle = lblSubDirBtn.Handle;
            PInvoke.SetWindowLongPtr(handle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(handle, -20), 0x8000000));
        }

        private void CheckedItemsClick() {
            List<string> lstCheckedPaths = new List<string>();
            List<QMenuItem> lstCheckedItems = new List<QMenuItem>();
            if(GetCheckedItems(contextMenuSubDir, lstCheckedPaths, lstCheckedItems, false)) {
                lstTempDirectoryPaths.Clear();
                foreach(QMenuItem item in lstCheckedItems) {
                    if((item is ToolStripMenuItemEx) || (item.IDLData != null)) {
                        MenuItemClicked(this, new ToolStripItemClickedEventArgs(item));
                        continue;
                    }
                    lstTempDirectoryPaths.Add(item.Path);
                }
                if((lstTempDirectoryPaths.Count > 0) && (MultipleMenuItemsClicked != null)) {
                    MultipleMenuItemsClicked(this, EventArgs.Empty);
                    lstTempDirectoryPaths.Clear();
                }
            }
        }

        private void CheckedItemsRightClick(ItemRightClickedEventArgs e) {
            List<string> lstCheckedPaths = new List<string>();
            List<QMenuItem> lstCheckedItems = new List<QMenuItem>();
            if(!GetCheckedItems(contextMenuSubDir, lstCheckedPaths, lstCheckedItems, false)) return;
            if(lstCheckedPaths.Count <= 1) {
                if(lstCheckedPaths.Count == 1) {
                    MenuItemRightClicked(this, e);
                }
            }
            else {
                string path = lstCheckedPaths.FirstOrDefault(str => 
                        !string.IsNullOrEmpty(str) && str.Length > 3 && !str.StartsWith("::"));
                if(path != null) {
                    try {
                        string directoryName = Path.GetDirectoryName(path);
                        if(!string.IsNullOrEmpty(directoryName)) {
                            if(lstCheckedPaths.All(str => directoryName.PathEquals(Path.GetDirectoryName(str)))) {
                                lstTempDirectoryPaths = new List<string>(lstCheckedPaths);
                                MultipleMenuItemsRightClicked(this, e);
                                lstTempDirectoryPaths.Clear();
                                return;
                            }
                        }
                    }
                    catch {
                    }
                }
                SystemSounds.Beep.Play();
            }
        }

        public void ClearThumbnailCache() {
            if(thumbnailTip != null) {
                thumbnailTip.ClearCache();
            }
        }

        private void contextMenuSubDir_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            menuIsShowing = false;
            fSuppressThumbnail = false;
            contextMenuSubDir.SuppressMouseMove = false;
            fDropHilitedOpened = false;
            contextMenuSubDir.Path = null;
            draggingPath = null;
            draggingItem = null;
            lstRcts.Clear();
            HideThumbnailTooltip(true);
            iThumbnailIndex = -1;
            iToolTipIndex = -1;
            lblSubDirBtn.SetPressed(false);
            if(!fClickClose) {
                if(fShownByKey) {
                    fShownByKey = false;
                    HideSubDirTip();
                }
                else if(!listView.MouseIsOverListView()) {
                    HideSubDirTip();
                }
            }
            fClickClose = false;
            contextMenuSubDir.SuspendLayout();
            if(!fDragStarted) {
                while(contextMenuSubDir.Items.Count > 0) {
                    contextMenuSubDir.Items[0].Dispose();
                }
            }
            fDragStarted = false;
            contextMenuSubDir.ItemsClearVirtual();
            contextMenuSubDir.ResumeLayout();
            DropDownMenuBase.ExitMenuMode();
            if(MenuClosed != null) {
                MenuClosed(this, EventArgs.Empty);
            }
        }

        private QMenuItem CreateDirectoryItem(DirectoryInfo diSub, string title, bool fIcon, bool fLink) {
            bool flag;
            FileSystemInfo targetIfFolderLink = ShellMethods.GetTargetIfFolderLink(diSub, out flag);
            if(!flag) {
                return null;
            }
            QMenuItem item = new QMenuItem(title, MenuTarget.Folder, MenuGenre.SubDirTip);
            item.Exists = true;
            item.HasIcon = fIcon;
            string fullName = diSub.FullName;
            if(fIcon) {
                item.SetImageReservationKey(fullName, null);
            }
            else if(!fLink) {
                item.ImageKey = "folder";
            }
            item.Path = fullName;
            item.TargetPath = targetIfFolderLink.FullName;
            item.QueryVirtualMenu += directoryItem_QueryVirtualMenu;
            return item;
        }

        private List<QMenuItem> CreateMenu(DirectoryInfo di, string pathChild) {
            List<QMenuItem> list = new List<QMenuItem>();
            List<QMenuItem> collection = new List<QMenuItem>();
            bool flag = true;
            try {
                flag = new DriveInfo(di.FullName).DriveFormat == "NTFS";
            }
            catch {
            }
            try {
                bool flag2, flag3;
                QTUtility.GetHiddenFileSettings(out flag3, out flag2);
                const FileAttributes attributes = FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Hidden;
                int num = 0;
                foreach(DirectoryInfo info in di.GetDirectories()) {
                    try {
                        string fullName = info.FullName;
                        string name = info.Name;
                        if((((fullName.Length != 0x1c) || !name.PathEquals("System Volume Information")) && ((fullName.Length != 15) || !name.PathEquals("$RECYCLE.BIN"))) && ((fullName.Length != 11) || !name.PathEquals("RECYCLER"))) {
                            FileAttributes attributes2 = info.Attributes;
                            if(QTUtility.IsXP || ((attributes2 & attributes) != attributes)) {
                                bool flag5 = (attributes2 & FileAttributes.System) != 0;
                                bool flag6 = (attributes2 & FileAttributes.ReadOnly) != 0;
                                bool flag7 = (attributes2 & FileAttributes.Hidden) != 0;
                                if((!flag5 || flag2) && (!flag7 || flag3)) {
                                    bool fTruncated;
                                    string title = QTUtility2.MakeNameEllipsis(name, out fTruncated);
                                    QMenuItem item = CreateDirectoryItem(info, title, flag5 || flag6, false);
                                    if(item != null) {
                                        if(fTruncated) {
                                            item.OriginalTitle = name;
                                        }
                                        if((pathChild != null) && (item.Path == pathChild)) {
                                            item.BackColor = QTUtility2.MakeModColor(SystemColors.Highlight);
                                            pathChild = null;
                                        }
                                        list.Add(item);
                                    }
                                    else {
                                        string path = fullName;
                                        ToolStripMenuItemEx ex = new ToolStripMenuItemEx(title);
                                        ex.Exists = true;
                                        ex.SetImageReservationKey(path, null);
                                        ex.ThumbnailIndex = 0xffff + num++;
                                        ex.ThumbnailPath = path;
                                        ex.Path = path;
                                        ex.Name = name;
                                        ex.Extension = Path.GetExtension(path).ToLower();
                                        if(fTruncated) {
                                            ex.OriginalTitle = name;
                                        }
                                        ex.MouseMove += tsmi_Files_MouseMove;
                                        ex.MouseDown += tsmi_MouseDown;
                                        ex.MouseUp += tsmi_MouseUp;
                                        collection.Add(ex);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception exception) {
                        QTUtility2.MakeErrorLog(exception, "creating subdir menu");
                    }
                }
                if(!flag) {
                    if(tsmiComparer == null) {
                        tsmiComparer = new ToolStripMeuItemComparer();
                    }
                    list.Sort(tsmiComparer);
                }
                if(!Config.Tips.SubDirTipsFiles) {
                    return list;
                }
                int num2 = 0;
                string str5 = ".lnk";
                string str6 = ".url";
                foreach(FileInfo info2 in di.GetFiles()) {
                    try {
                        FileAttributes attributes3 = info2.Attributes;
                        bool flag8 = (attributes3 & FileAttributes.System) != 0;
                        bool flag9 = (attributes3 & FileAttributes.Hidden) != 0;
                        if((!flag8 || flag2) && (!flag9 || flag3)) {
                            string fileNameWithoutExtension;
                            bool fTruncated;
                            string lnkPath = info2.FullName;
                            string str8 = lnkPath;
                            string ext = info2.Extension.ToLower();
                            if(ext == str5) {
                                string linkTargetPath = ShellMethods.GetLinkTargetPath(lnkPath);
                                if(!string.IsNullOrEmpty(linkTargetPath)) {
                                    DirectoryInfo diSub = new DirectoryInfo(linkTargetPath);
                                    if(diSub.Exists) {
                                        string str12 = QTUtility2.MakeNameEllipsis(Path.GetFileNameWithoutExtension(info2.Name), out fTruncated);
                                        QMenuItem item2 = CreateDirectoryItem(diSub, str12, false, true);
                                        if(item2 != null) {
                                            item2.Path = lnkPath;
                                            item2.TargetPath = linkTargetPath;
                                            item2.Name = info2.Name;
                                            item2.Extension = ext;
                                            if(fTruncated) {
                                                item2.OriginalTitle = info2.Name;
                                            }
                                            item2.HasIcon = true;
                                            item2.SetImageReservationKey(lnkPath, ext);
                                            collection.Add(item2);
                                        }
                                        continue;
                                    }
                                    str8 = linkTargetPath;
                                }
                            }
                            if((ext == str5) || (ext == str6)) {
                                fileNameWithoutExtension = Path.GetFileNameWithoutExtension(info2.Name);
                            }
                            else {
                                fileNameWithoutExtension = info2.Name;
                            }
                            ToolStripMenuItemEx ex2 = new ToolStripMenuItemEx(QTUtility2.MakeNameEllipsis(fileNameWithoutExtension, out fTruncated));
                            ex2.ThumbnailIndex = num2++;
                            ex2.ThumbnailPath = str8;
                            ex2.Path = lnkPath;
                            ex2.Name = info2.Name;
                            ex2.Extension = ext;
                            if(fTruncated) {
                                ex2.OriginalTitle = fileNameWithoutExtension;
                            }
                            ex2.Exists = true;
                            ex2.SetImageReservationKey(lnkPath, ext);
                            ex2.MouseMove += tsmi_Files_MouseMove;
                            ex2.MouseDown += tsmi_MouseDown;
                            ex2.MouseUp += tsmi_MouseUp;
                            collection.Add(ex2);
                        }
                    }
                    catch(Exception exception2) {
                        QTUtility2.MakeErrorLog(exception2, "creating subfile menu");
                    }
                }
                collection.Sort(extComparer);
                list.AddRange(collection);
            }
            catch {
            }
            return list;
        }

        // TODO: clean
        private List<QMenuItem> CreateMenuFromIDL(IDLWrapper idlw, byte[] idlChild) {
            List<QMenuItem> list = new List<QMenuItem>();
            List<QMenuItem> collection = new List<QMenuItem>();
            if(idlw.Available) {
                IShellFolder shellFolder = null;
                IEnumIDList ppenumIDList = null;
                IntPtr zero = IntPtr.Zero;
                IntPtr ptr2 = IntPtr.Zero;
                if(idlChild != null) {
                    zero = ShellMethods.CreateIDL(idlChild);
                    ptr2 = PInvoke.ILFindLastID(zero);
                }
                bool dummy;
                bool flag;
                QTUtility.GetHiddenFileSettings(out flag, out dummy);
                int grfFlags = 0x60;
                if(flag) {
                    grfFlags |= 0x80;
                }
                try {
                    IntPtr ptr3;
                    if(!ShellMethods.GetShellFolder(idlw.PIDL, out shellFolder) || (shellFolder.EnumObjects(IntPtr.Zero, grfFlags, out ppenumIDList) != 0)) {
                        return list;
                    }
                    int num2 = 0;
                    while(ppenumIDList.Next(1, out ptr3, null) == 0) {
                        IntPtr pIDL = PInvoke.ILCombine(idlw.PIDL, ptr3);
                        string str = ShellMethods.GetDisplayName(shellFolder, ptr3, false);
                        if(!string.IsNullOrEmpty(str)) {
                            uint rgfInOut = 0x60000000;
                            IntPtr[] apidl = new IntPtr[] { ptr3 };
                            if(shellFolder.GetAttributesOf(1, apidl, ref rgfInOut) == 0) {
                                bool fTruncated;
                                bool flag2 = (rgfInOut & 0x20000000) == 0x20000000;
                                bool flag3 = (rgfInOut & 0x40000000) == 0x40000000;
                                string name = ShellMethods.GetDisplayName(shellFolder, ptr3, true);
                                string title = QTUtility2.MakeNameEllipsis(name, out fTruncated);
                                if(flag3 && !flag2) {
                                    ToolStripMenuItemEx ex = new ToolStripMenuItemEx(title);
                                    ex.ThumbnailIndex = num2++;
                                    ex.ThumbnailPath = str;
                                    ex.Path = str;
                                    ex.Name = name;
                                    ex.Extension = Path.GetExtension(str).ToLower();
                                    if(fTruncated) {
                                        ex.OriginalTitle = name;
                                    }
                                    ex.Exists = true;
                                    ex.SetImageReservationKey(str, Path.GetExtension(str).ToLower());
                                    ex.MouseMove += tsmi_Files_MouseMove;
                                    ex.MouseDown += tsmi_MouseDown;
                                    ex.MouseUp += tsmi_MouseUp;
                                    collection.Add(ex);
                                }
                                else {
                                    QMenuItem item = new QMenuItem(title, flag2 ? MenuTarget.Folder : MenuTarget.File, MenuGenre.SubDirTip);
                                    if(str.Length == 3) {
                                        if(!QTUtility.ImageListGlobal.Images.ContainsKey(str)) {
                                            QTUtility.ImageListGlobal.Images.Add(str, QTUtility.GetIcon(pIDL));
                                        }
                                        item.ImageKey = str;
                                    }
                                    else {
                                        item.SetImageReservationKey(str, flag2 ? null : Path.GetExtension(str).ToLower());
                                    }
                                    item.Exists = flag3;
                                    item.Path = str;
                                    item.TargetPath = str;
                                    item.ForceToolTip = true;
                                    if((idlChild != null) && (shellFolder.CompareIDs((IntPtr)0x10000000, ptr3, ptr2) == 0)) {
                                        item.BackColor = QTUtility2.MakeModColor(SystemColors.Highlight);
                                    }
                                    item.IDLData = ShellMethods.GetIDLData(pIDL);
                                    item.QueryVirtualMenu += directory_FromIDL_QueryVirtualMenu;
                                    list.Add(item);
                                }
                            }
                            if(ptr3 != IntPtr.Zero) {
                                PInvoke.CoTaskMemFree(ptr3);
                            }
                            if(pIDL != IntPtr.Zero) {
                                PInvoke.CoTaskMemFree(pIDL);
                            }
                        }
                    }
                    collection.Sort(extComparer);
                    list.AddRange(collection);
                }
                catch {
                }
                finally {
                    if(shellFolder != null) {
                        Marshal.ReleaseComObject(shellFolder);
                    }
                    if(ppenumIDList != null) {
                        Marshal.ReleaseComObject(ppenumIDList);
                    }
                    if(zero != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(zero);
                    }
                }
            }
            return list;
        }

        private List<QMenuItem> CreateParentMenu(IDLWrapper idlw, List<QMenuItem> lst) {
            if(lst == null) {
                lst = new List<QMenuItem>();
            }
            using(IDLWrapper wrapper = idlw.GetParent()) {
                if(!wrapper.Available || !wrapper.HasPath) {
                    return lst;
                }
                bool isDesktop = PInvoke.ILGetSize(wrapper.PIDL) == 2;
                QMenuItem item = new QMenuItem(ShellMethods.GetDisplayName(wrapper.PIDL, true), MenuTarget.Folder, MenuGenre.SubDirTip);
                if(!QTUtility.ImageListGlobal.Images.ContainsKey(wrapper.Path)) {
                    QTUtility.ImageListGlobal.Images.Add(wrapper.Path, QTUtility.GetIcon(wrapper.PIDL));
                }
                item.ImageKey = item.Path = item.TargetPath = wrapper.Path;
                item.IDLDataChild = idlw.IDL;
                item.PathChild = idlw.Path;
                item.MouseMove += tsmi_Folder_MouseMove;
                DropDownMenuDropTarget target = new DropDownMenuDropTarget(null, true, !fDesktop, false, hwndDialogParent);
                target.SuspendLayout();
                target.CheckOnEdgeClick = true;
                target.MessageParent = hwndMessageReflect;
                target.Items.Add(new ToolStripMenuItem("dummy"));
                target.ImageList = QTUtility.ImageListGlobal;
                target.SpaceKeyExecute = true;
                target.MouseLeave += ddmr_MouseLeave;
                target.ItemRightClicked += ddmr_ItemRightClicked;
                target.Opened += ddmr_Opened;
                target.MenuDragEnter += ddmr_MenuDragEnter;
                item.DropDown = target;
                item.DropDownOpening += tsmi_DropDownOpening;
                item.DropDownItemClicked += ddmr_ItemClicked;
                if(wrapper.IsFileSystem) {
                    item.MouseDown += tsmi_MouseDown;
                    item.MouseUp += tsmi_MouseUp;
                    target.MouseDragMove += ddmr_MouseDragMove;
                    target.MouseUpBeforeDrop += ddmr_MouseUpBeforeDrop;
                    target.KeyUp += ddmr_KeyUp;
                    target.PreviewKeyDown += ddmr_PreviewKeyDown;
                    target.MouseScroll += ddmr_MouseScroll;
                    target.Path = wrapper.Path;
                }
                target.ResumeLayout();
                lst.Add(item);
                if(!isDesktop) {
                    CreateParentMenu(wrapper, lst);
                }
            }
            return lst;
        }

        private void ddmr_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(MenuItemClicked != null) {
                if((e.ClickedItem is ToolStripMenuItem) && ((ToolStripMenuItem)e.ClickedItem).Checked) {
                    CheckedItemsClick();
                    contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
                else {
                    contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                    MenuItemClicked(this, e);
                }
            }
        }

        private void ddmr_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(MenuItemRightClicked != null) {
                fSuppressThumbnail = true;
                if(((e.ClickedItem is ToolStripMenuItem) && ((ToolStripMenuItem)e.ClickedItem).Checked) && (MultipleMenuItemsRightClicked != null)) {
                    CheckedItemsRightClick(e);
                }
                else {
                    MenuItemRightClicked(this, e);
                }
                ((DropDownMenuReorderable)sender).SuppressMouseMove = false;
                if(e.HRESULT != 0) {
                    fSuppressThumbnail = false;
                }
            }
        }

        private void ddmr_KeyUp(object sender, KeyEventArgs e) {
            if(((Keys.Left > e.KeyCode) || (e.KeyCode > Keys.Down)) && (((e.KeyCode != Keys.End) && (e.KeyCode != Keys.Home)) && ((e.KeyCode != Keys.Prior) && (e.KeyCode != Keys.Next)))) {
                if(e.KeyCode == Keys.Escape) {
                    HideThumbnailTooltip(true);
                }
            }
            else {
                DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
                reorderable.UpdateToolTipByKey(null);
                foreach(ToolStripItem item in reorderable.Items) {
                    if(item.Selected) {
                        ToolStripMenuItemEx tsmi = item as ToolStripMenuItemEx;
                        if(tsmi == null) {
                            break;
                        }
                        reorderable.SuppressMouseMoveOnce = true;
                        if(!ShowThumbnailTooltip(tsmi, true) && (tsmi.ThumbnailIndex != iToolTipIndex)) {
                            if(timerToolTipByKey == null) {
                                timerToolTipByKey = new Timer(components);
                                timerToolTipByKey.Interval = SystemInformation.MouseHoverTime;
                                timerToolTipByKey.Tick += timerToolTipByKey_Tick;
                            }
                            timerToolTipByKey.Tag = tsmi;
                            timerToolTipByKey.Enabled = false;
                            timerToolTipByKey.Enabled = true;
                        }
                        return;
                    }
                }
                HideThumbnailTooltip(true);
            }
        }

        private void ddmr_MenuDragEnter(object sender, EventArgs e) {
            if(!listView.HasFocus()) {
                listView.SetFocus();
            }
        }

        private void ddmr_MouseDragMove(object sender, MouseEventArgs e) {
            if(((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right)) && (draggingItem != null)) {
                Size size = new Size(Math.Abs((e.X - pntDragStart.X)), Math.Abs((e.Y - pntDragStart.Y)));
                if((size.Width > SystemDragSize.Width) || (size.Height > SystemDragSize.Height)) {
                    DropDownMenuDropTarget ddmrt = (DropDownMenuDropTarget)sender;
                    ddmrt.SuppressMouseMove = false;
                    if(draggingItem.Checked) {
                        DoDragDropCheckedItems(ddmrt);
                    }
                    else if(!string.IsNullOrEmpty(draggingPath) && ((draggingPath.Length > 3) || Directory.Exists(draggingPath))) {
                        fDragStarted = true;
                        List<ToolStripItem> list = contextMenuSubDir.Items.Cast<ToolStripItem>().ToList();
                        ShellMethods.DoDragDrop(draggingPath, this);
                        if(!fDragStarted) {
                            foreach(ToolStripItem item2 in list) {
                                item2.Dispose();
                            }
                        }
                        fDragStarted = false;
                        contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                    }
                }
            }
        }

        private void ddmr_MouseLeave(object sender, EventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            reorderable.SuppressMouseMove = false;
            if(!reorderable.Bounds.Contains(MousePosition)) {
                HideThumbnailTooltip();
            }
        }

        private void ddmr_MouseScroll(object sender, EventArgs e) {
            HideThumbnailTooltip(true);
        }

        private void ddmr_MouseUpBeforeDrop(object sender, EventArgs e) {
            draggingPath = null;
            draggingItem = null;
            HideThumbnailTooltip();
        }

        private void ddmr_Opened(object sender, EventArgs e) {
            lstRcts.Add(((DropDownMenuReorderable)sender).Bounds);
        }

        private void ddmr_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if((Keys.Left <= e.KeyCode) && (e.KeyCode <= Keys.Down)) {
                if(timerToolTipByKey != null) {
                    timerToolTipByKey.Enabled = false;
                }
                if((iToolTipIndex != -1) && ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))) {
                    ((DropDownMenuReorderable)sender).UpdateToolTipByKey(null);
                }
                iToolTipIndex = -1;
            }
        }

        private void directory_FromIDL_QueryVirtualMenu(object sender, EventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            string path = item.Path;
            item.MouseMove += tsmi_Folder_MouseMove;
            bool flag = (path.ToLower() == @"a:\") || (path.ToLower() == @"b:\");
            bool flag2 = (path.Length == 3) ? (!flag && new DriveInfo(path).IsReady) : true;
            if((item.Target == MenuTarget.Folder) && flag2) {
                DropDownMenuDropTarget target = new DropDownMenuDropTarget(null, true, !fDesktop, false, hwndDialogParent);
                target.SuspendLayout();
                target.CheckOnEdgeClick = true;
                target.MessageParent = hwndMessageReflect;
                target.Items.Add(new ToolStripMenuItem("dummy"));
                target.ImageList = QTUtility.ImageListGlobal;
                target.SpaceKeyExecute = true;
                target.Path = path;
                target.MouseLeave += ddmr_MouseLeave;
                target.ItemRightClicked += ddmr_ItemRightClicked;
                target.Opened += ddmr_Opened;
                target.MouseScroll += ddmr_MouseScroll;
                if(item.Exists) {
                    target.MouseDragMove += ddmr_MouseDragMove;
                    target.MouseUpBeforeDrop += ddmr_MouseUpBeforeDrop;
                    target.KeyUp += ddmr_KeyUp;
                    target.PreviewKeyDown += ddmr_PreviewKeyDown;
                    target.MenuDragEnter += ddmr_MenuDragEnter;
                }
                item.DropDown = target;
                item.DropDownOpening += tsmi_DropDownOpening;
                item.DropDownItemClicked += ddmr_ItemClicked;
                target.ResumeLayout();
            }
        }

        private void directoryItem_QueryVirtualMenu(object sender, EventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            item.MouseMove += tsmi_Folder_MouseMove;
            item.MouseDown += tsmi_MouseDown;
            item.MouseUp += tsmi_MouseUp;
            bool fSearchHidden;
            bool fSearchSystem;
            QTUtility.GetHiddenFileSettings(out fSearchHidden, out fSearchSystem);
            bool flag3 = Config.Tips.SubDirTipsFiles;
            bool flag4;
            using(FindFile file = new FindFile(item.TargetPath, fSearchHidden, fSearchSystem)) {
                flag4 = file.SubDirectoryExists() || (flag3 && file.SubFileExists());
            }
            if(flag4) {
                DropDownMenuDropTarget target = new DropDownMenuDropTarget(null, true, !fDesktop, false, hwndDialogParent);
                target.SuspendLayout();
                target.CheckOnEdgeClick = true;
                target.MessageParent = hwndMessageReflect;
                target.Items.Add(new ToolStripMenuItem("dummy"));
                target.ImageList = QTUtility.ImageListGlobal;
                target.SpaceKeyExecute = true;
                target.Path = item.TargetPath;
                target.MouseLeave += ddmr_MouseLeave;
                target.MouseDragMove += ddmr_MouseDragMove;
                target.MouseUpBeforeDrop += ddmr_MouseUpBeforeDrop;
                target.ItemRightClicked += ddmr_ItemRightClicked;
                target.Opened += ddmr_Opened;
                target.KeyUp += ddmr_KeyUp;
                target.PreviewKeyDown += ddmr_PreviewKeyDown;
                target.MenuDragEnter += ddmr_MenuDragEnter;
                target.MouseScroll += ddmr_MouseScroll;
                item.DropDown = target;
                item.DropDownOpening += tsmi_DropDownOpening;
                item.DropDownItemClicked += ddmr_ItemClicked;
                target.ResumeLayout();
            }
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            if(thumbnailTip != null) {
                thumbnailTip.Dispose();
                thumbnailTip = null;
            }
            base.Dispose(disposing);
        }

        private void DoDragDropCheckedItems(DropDownMenuDropTarget ddmrt) {
            List<string> lstCheckedPaths = new List<string>();
            List<QMenuItem> lstCheckedItems = new List<QMenuItem>();
            if(GetCheckedItems(contextMenuSubDir, lstCheckedPaths, lstCheckedItems, true)) {
                if(lstCheckedPaths.Count > 0) {
                    try {
                        string directoryName = Path.GetDirectoryName(lstCheckedPaths[0]);
                        if(lstCheckedPaths.Any(str2 => !string.Equals(
                                directoryName, Path.GetDirectoryName(str2), StringComparison.OrdinalIgnoreCase))) {
                            SystemSounds.Beep.Play();
                            ddmrt.SetSuppressMouseUp();
                            return;
                        }
                        fDragStarted = true;
                        List<ToolStripItem> list3 = contextMenuSubDir.Items.Cast<ToolStripItem>().ToList();
                        ShellMethods.DoDragDrop(lstCheckedPaths, this, true);
                        if(!fDragStarted) {
                            foreach(ToolStripItem item2 in list3) {
                                item2.Dispose();
                            }
                        }
                        fDragStarted = false;
                        contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                    }
                    catch {
                    }
                }
                else {
                    SystemSounds.Beep.Play();
                    ddmrt.SetSuppressMouseUp();
                }
            }
        }

        private bool GetCheckedItems(DropDownMenuReorderable ddmr, List<string> lstCheckedPaths, List<QMenuItem> lstCheckedItems, bool fDragDrop) {
            bool flag = false;
            foreach(QMenuItem item2 in ddmr.Items.OfType<QMenuItem>()) {
                if(item2.Checked) {
                    flag = true;
                    lstCheckedItems.Add(item2);
                    lstCheckedPaths.Add(item2.Path);
                }
                else if(!fDragDrop && item2.HasDropDownItems && GetCheckedItems((DropDownMenuReorderable)item2.DropDown, lstCheckedPaths, lstCheckedItems, false)) {
                    flag = true;
                }
            }
            return flag;
        }

        public void HideMenu() {
            if(menuIsShowing) {
                contextMenuSubDir.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }
        }

        public void HideSubDirTip(bool fForce = false) {
            if(fForce) {
                fShownByKey = false;
            }
            isShowing = false;
            currentDir = contextMenuSubDir.Path = string.Empty;
            currentIDL = null;
            if(menuIsShowing) {
                contextMenuSubDir.Close(ToolStripDropDownCloseReason.AppFocusChange);
            }
            PInvoke.ShowWindow(Handle, 0);
        }

        private void HideThumbnailTooltip() {
            if(((thumbnailTip != null) && thumbnailTip.IsShowing) && thumbnailTip.HideToolTip()) {
                iThumbnailIndex = -1;
            }
        }

        private void HideThumbnailTooltip(bool fKey) {
            if(thumbnailTip != null) {
                if(fKey) {
                    thumbnailTip.IsShownByKey = false;
                }
                HideThumbnailTooltip();
            }
        }

        private void InitializeComponent() {
            components = new Container();
            contextMenuSubDir = new DropDownMenuDropTarget(components, true, !fDesktop, true, hwndDialogParent);
            lblSubDirBtn = new LabelEx();
            SuspendLayout();
            contextMenuSubDir.SpaceKeyExecute = true;
            contextMenuSubDir.CheckOnEdgeClick = true;
            contextMenuSubDir.Closed += contextMenuSubDir_Closed;
            contextMenuSubDir.ItemClicked += ddmr_ItemClicked;
            contextMenuSubDir.ItemRightClicked += ddmr_ItemRightClicked;
            contextMenuSubDir.MouseDragMove += ddmr_MouseDragMove;
            contextMenuSubDir.MouseLeave += ddmr_MouseLeave;
            contextMenuSubDir.MouseUpBeforeDrop += ddmr_MouseUpBeforeDrop;
            contextMenuSubDir.MouseScroll += ddmr_MouseScroll;
            contextMenuSubDir.KeyUp += ddmr_KeyUp;
            contextMenuSubDir.PreviewKeyDown += ddmr_PreviewKeyDown;
            contextMenuSubDir.MenuDragEnter += ddmr_MenuDragEnter;
            lblSubDirBtn.BackColor = SystemColors.Window;
            lblSubDirBtn.Location = new Point(0, 0);
            lblSubDirBtn.Size = new Size(0x10, 0x10);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(15, 15);
            Controls.Add(lblSubDirBtn);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
        }

        public bool MouseIsOnThis() {
            return RectangleToScreen(lblSubDirBtn.Bounds).Contains(MousePosition);
        }

        protected override void OnClick(EventArgs e) {
            if(menuIsShowing) {
                RECT rect2;
                lblSubDirBtn.SetPressed(false);
                PInvoke.GetWindowRect(Handle, out rect2);
                PInvoke.SetWindowPos(Handle, (IntPtr)(-1), rect2.left - 1, rect2.top - 1, 15, 15, 0x10);
                fClickClose = true;
                contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
                return;
            }
            if(string.IsNullOrEmpty(currentDir)) {
                return;
            }
            contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
            List<QMenuItem> lstItems = null;
            try {
                if(currentIDL != null) {
                    using(IDLWrapper wrapper = new IDLWrapper(currentIDL)) {
                        lstItems = CreateMenuFromIDL(wrapper, null);
                    }
                }
                else if(currentDir.StartsWith("::")) {
                    using(IDLWrapper wrapper2 = new IDLWrapper(currentDir)) {
                        lstItems = CreateMenuFromIDL(wrapper2, null);
                    }
                }
                else {
                    lstItems = CreateMenu(new DirectoryInfo(currentDir), null);
                }
            }
            catch {
            }
            if((lstItems != null) && (lstItems.Count > 0)) {
                RECT rect;
                PInvoke.GetWindowRect(Handle, out rect);
                PInvoke.SetWindowPos(Handle, (IntPtr)(-1), rect.left + 1, rect.top + 1, 15, 15, 0x10);
                lblSubDirBtn.SetPressed(true);
                contextMenuSubDir.SuspendLayout();
                contextMenuSubDir.AddItemsRangeVirtual(lstItems);
                contextMenuSubDir.ResumeLayout();
                Rectangle workingArea = Screen.FromHandle(Handle).WorkingArea;
                Rectangle bounds = contextMenuSubDir.Bounds;
                if((((rect.right + 1) + bounds.Width) > workingArea.Right) && (((rect.bottom + 1) + bounds.Height) > workingArea.Bottom)) {
                    rect.right -= bounds.Width + 0x10;
                    if(fDropHilitedOpened) {
                        Point mousePosition = MousePosition;
                        if((rect.right < mousePosition.X) && (mousePosition.X < (rect.right + bounds.Width))) {
                            rect.right -= ((rect.right + bounds.Width) - mousePosition.X) + 0x11;
                        }
                    }
                }
                contextMenuSubDir.SetShowingByKey(fShownByKey);
                contextMenuSubDir.Show(new Point(rect.right + 1, rect.bottom + 1));
                contextMenuSubDir.Items[0].Select();
                menuIsShowing = true;
            }
        }

        public void OnExplorerInactivated() {
            if(!menuIsShowing) {
                HideSubDirTip();
            }
            else {
                Point mousePosition = MousePosition;
                if(!contextMenuSubDir.Bounds.Contains(mousePosition)) {
                    if(lstRcts.All(rect => !rect.Contains(mousePosition))) {
                        HideSubDirTip();
                    }
                }
            }
        }

        public void PerformClickByKey() {
            if(isShowing && !menuIsShowing) {
                fShownByKey = true;
                OnClick(EventArgs.Empty);
            }
        }

        public void ShowMenu() {
            if(isShowing) {
                fDropHilitedOpened = true;
                OnClick(EventArgs.Empty);
            }
        }

        public bool ShowMenuWithoutShowForm(string path, Point pnt, bool fParent) {
            if(string.IsNullOrEmpty(path)) {
                return false;
            }
            contextMenuSubDir.Close(ToolStripDropDownCloseReason.ItemClicked);
            currentDir = path;
            currentIDL = null;
            if(fParent) {
                contextMenuSubDir.Path = null;
            }
            else {
                contextMenuSubDir.Path = path;
            }
            List<QMenuItem> lstItems = null;
            try {
                if(fParent) {
                    using(IDLWrapper wrapper = new IDLWrapper(path)) {
                        lstItems = CreateParentMenu(wrapper, null);
                        lstItems.Reverse();
                    }
                }
                else if(path.StartsWith("::")) {
                    using(IDLWrapper wrapper2 = new IDLWrapper(path)) {
                        lstItems = CreateMenuFromIDL(wrapper2, null);
                    }
                }
                else {
                    DirectoryInfo di = new DirectoryInfo(currentDir);
                    lstItems = CreateMenu(di, null);
                }
            }
            catch {
            }
            if((lstItems != null) && (lstItems.Count > 0)) {
                contextMenuSubDir.SuspendLayout();
                contextMenuSubDir.MaximumSize = Size.Empty;
                contextMenuSubDir.AddItemsRangeVirtual(lstItems);
                contextMenuSubDir.ResumeLayout();
                Screen screen = Screen.FromPoint(pnt);
                if(fParent) {
                    int y = pnt.Y;
                    pnt.Y -= contextMenuSubDir.Height;
                    if(screen.Bounds.Top > pnt.Y) {
                        if(((y - screen.Bounds.Top) - 8) < (10 + (lstItems[0].Height * 4))) {
                            pnt.X += 0x1a;
                        }
                        else {
                            contextMenuSubDir.MaximumSize = new Size(0, (y - screen.Bounds.Top) - 8);
                            pnt.Y = screen.Bounds.Top + 8;
                        }
                    }
                }
                else if((screen.Bounds.Height - pnt.Y) < contextMenuSubDir.Height) {
                    contextMenuSubDir.MaximumSize = new Size(0, (screen.Bounds.Height - pnt.Y) - 0x20);
                }
                contextMenuSubDir.Show(pnt);
                if(fParent) {
                    contextMenuSubDir.Items[lstItems.Count - 1].Select();
                }
                else {
                    contextMenuSubDir.Items[0].Select();
                }
                menuIsShowing = true;
                return true;
            }
            return false;
        }

        public void ShowSubDirTip(string path, byte[] idl, Point pnt) {
            lblSubDirBtn.SetPressed(false);
            IntPtr hwnd = PInvoke.WindowFromPoint(new Point(pnt.X, pnt.Y + 2));
            if(hwnd == lblSubDirBtn.Handle || hwnd == listView.Handle) {
                isShowing = true;
                currentDir = contextMenuSubDir.Path = path;
                currentIDL = idl;
                PInvoke.SetWindowPos(Handle, (IntPtr)(-1), pnt.X, pnt.Y, 15, 15, 0x10);
                PInvoke.ShowWindow(Handle, 4);
            }
        }

        private bool ShowThumbnailTooltip(ToolStripMenuItemEx tsmi, bool fKey) {
            if((menuIsShowing && (draggingPath == null)) && !fSuppressThumbnail) {
                if((!Config.Tips.SubDirTipsPreview ^ (ModifierKeys == Keys.Shift)) && ThumbnailTooltipForm.ExtIsSupported(Path.GetExtension(tsmi.ThumbnailPath).ToLower())) {
                    if(iThumbnailIndex == tsmi.ThumbnailIndex) {
                        return false;
                    }
                    if(thumbnailTip == null) {
                        thumbnailTip = new ThumbnailTooltipForm();
                    }
                    if(thumbnailTip.IsShownByKey && !fKey) {
                        thumbnailTip.IsShownByKey = false;
                        return false;
                    }
                    thumbnailTip.IsShownByKey = fKey;
                    iThumbnailIndex = tsmi.ThumbnailIndex;
                    if(thumbnailTip.ShowToolTip(tsmi.ThumbnailPath, tsmi.Owner.RectangleToScreen(tsmi.Bounds))) {
                        tsmi.ToolTipText = null;
                        return true;
                    }
                }
                if(tsmi.ToolTipText == null) {
                    string originalTitle = tsmi.OriginalTitle;
                    string shellInfoTipText = ShellMethods.GetShellInfoTipText(tsmi.Path, false);
                    if(shellInfoTipText != null) {
                        if(originalTitle == null) {
                            originalTitle = shellInfoTipText;
                        }
                        else {
                            originalTitle = originalTitle + "\r\n" + shellInfoTipText;
                        }
                    }
                    tsmi.ToolTipText = originalTitle;
                }
                HideThumbnailTooltip(fKey);
            }
            return false;
        }

        private void timerToolTipByKey_Tick(object sender, EventArgs e) {
            try {
                timerToolTipByKey.Enabled = false;
                ToolStripMenuItemEx tag = timerToolTipByKey.Tag as ToolStripMenuItemEx;
                if(((tag != null) && !tag.IsDisposed) && menuIsShowing) {
                    DropDownMenuReorderable owner = tag.Owner as DropDownMenuReorderable;
                    if(((owner != null) && owner.Visible) && (!owner.IsDisposed && !owner.Disposing)) {
                        owner.UpdateToolTipByKey(tag);
                        iToolTipIndex = tag.ThumbnailIndex;
                    }
                }
                timerToolTipByKey.Tag = null;
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        private void tsmi_DropDownOpening(object sender, EventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            item.DropDown.SuspendLayout();
            item.DropDownItems[0].Dispose();
            item.DropDownOpening -= tsmi_DropDownOpening;
            List<QMenuItem> lstItems = null;
            try {
                if(item.TargetPath.StartsWith("::")) {
                    using(IDLWrapper wrapper = new IDLWrapper(item.TargetPath)) {
                        lstItems = CreateMenuFromIDL(wrapper, item.IDLDataChild);
                    }
                }
                else {
                    DirectoryInfo di = new DirectoryInfo(item.TargetPath);
                    lstItems = CreateMenu(di, item.PathChild);                    
                }
            }
            catch {
            }
            if((lstItems != null) && (lstItems.Count > 0)) {
                ((DropDownMenuReorderable)item.DropDown).AddItemsRangeVirtual(lstItems);
            }
            item.DropDown.ResumeLayout();
        }

        private void tsmi_Files_MouseMove(object sender, MouseEventArgs e) {
            ShowThumbnailTooltip((ToolStripMenuItemEx)sender, false);
        }

        private void tsmi_Folder_MouseMove(object sender, MouseEventArgs e) {
            HideThumbnailTooltip();
            QMenuItem item = (QMenuItem)sender;
            if(item.ForceToolTip || (ModifierKeys == Keys.Shift)) {
                if((item.ToolTipText == null) || (item.ToolTipText == item.OriginalTitle)) {
                    string originalTitle = item.OriginalTitle;
                    string shellInfoTipText = ShellMethods.GetShellInfoTipText(item.Path, true);
                    if(shellInfoTipText != null) {
                        if(originalTitle == null) {
                            originalTitle = shellInfoTipText;
                        }
                        else {
                            originalTitle = originalTitle + "\r\n" + shellInfoTipText;
                        }
                    }
                    item.ToolTipText = originalTitle;
                }
            }
            else if(item.OriginalTitle != null) {
                item.ToolTipText = item.OriginalTitle;
            }
        }

        private void tsmi_MouseDown(object sender, MouseEventArgs e) {
            if((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right)) {
                QMenuItem item = (QMenuItem)sender;
                DropDownMenuReorderable owner = (DropDownMenuReorderable)item.Owner;
                owner.SuppressStartIndex = owner.Items.IndexOf(item);
                owner.SuppressMouseMove = true;
                draggingItem = item;
                draggingPath = item.Path;
                pntDragStart = owner.PointToClient(MousePosition);
            }
        }

        private void tsmi_MouseUp(object sender, MouseEventArgs e) {
            draggingPath = null;
            draggingItem = null;
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == WM.MOUSEACTIVATE) {
                if(((((int)((long)m.LParam)) >> 0x10) & 0xffff) == 0x201) {
                    OnClick(EventArgs.Empty);
                }
                m.Result = (IntPtr)4;
            }
            else if(((m.Msg == WM.INITMENUPOPUP) || (m.Msg == WM.DRAWITEM)) || (m.Msg == WM.MEASUREITEM)) {
                if(hwndMessageReflect != IntPtr.Zero) {
                    PInvoke.SendMessage(hwndMessageReflect, (uint)m.Msg, m.WParam, m.LParam);
                }
            }
            else {
                base.WndProc(ref m);
            }
        }

        public List<string> ExecutedDirectories {
            get {
                return new List<string>(lstTempDirectoryPaths);
            }
        }

        public bool IsMouseOnMenus {
            get {
                if(!contextMenuSubDir.Visible) {
                    return false;
                }
                Point mousePosition = MousePosition;
                return contextMenuSubDir.Bounds.Contains(mousePosition) || lstRcts.Any(rect => rect.Contains(mousePosition));
            }
        }

        public bool IsShowing {
            get {
                return isShowing;
            }
        }

        public bool IsShownByKey {
            get {
                return fShownByKey;
            }
        }

        public bool MenuIsShowing {
            get {
                return menuIsShowing;
            }
        }

        private sealed class ExtComparer : IComparer<QMenuItem> {
            public int Compare(QMenuItem x, QMenuItem y) {
                if((x.Extension.Length == 0) && (y.Extension.Length == 0)) {
                    return string.Compare(x.Name, y.Name);
                }
                if(x.Extension.Length == 0) {
                    return 1;
                }
                if(y.Extension.Length == 0) {
                    return -1;
                }
                int num = string.Compare(x.Extension, y.Extension);
                if(num == 0) {
                    return string.Compare(x.Name, y.Name);
                }
                return num;
            }
        }

        private sealed class LabelEx : Label {
            private static Bitmap bmpCold = Resources_Image.imgSubDirBtnCold;
            private static Bitmap bmpPrssed = Resources_Image.imgSubDirBtnPress;
            private bool fPressed;

            protected override void OnPaint(PaintEventArgs e) {
                e.Graphics.DrawImage(fPressed ? bmpPrssed : bmpCold, new Rectangle(0, 0, 15, 15), new Rectangle(0, 0, 15, 15), GraphicsUnit.Pixel);
            }

            public void SetPressed(bool fPressed) {
                this.fPressed = fPressed;
                Invalidate();
            }
        }

        internal sealed class ToolStripMenuItemEx : QMenuItem {
            private int thumbnailIndex;
            private string thumbnailPath;

            public ToolStripMenuItemEx(string title)
                : base(title, MenuTarget.File, MenuGenre.SubDirTip) {
            }

            public int ThumbnailIndex {
                get {
                    return thumbnailIndex;
                }
                set {
                    thumbnailIndex = value;
                }
            }

            public string ThumbnailPath {
                get {
                    return thumbnailPath;
                }
                set {
                    thumbnailPath = value;
                }
            }
        }

        private sealed class ToolStripMeuItemComparer : IComparer<QMenuItem> {
            public int Compare(QMenuItem x, QMenuItem y) {
                return string.Compare(x.Text, y.Text);
            }
        }
    }
}

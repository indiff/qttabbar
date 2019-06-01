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
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class MenuUtility {
        private static Font StartUpTabFont;

        // TODO: this is absent from Quizo's sources.  Figure out why.
        private static void AddChildrenOnOpening(DirectoryMenuItem parentItem) {
            bool fTruncated;
            DirectoryInfo info = new DirectoryInfo(parentItem.Path);
            EventPack eventPack = parentItem.EventPack;
            foreach(DirectoryInfo info2 in info.GetDirectories()
                    .Where(info2 => (info2.Attributes & FileAttributes.Hidden) == 0)) {
                string text = QTUtility2.MakeNameEllipsis(info2.Name, out fTruncated);
                DropDownMenuReorderable reorderable = new DropDownMenuReorderable(null);
                reorderable.MessageParent = eventPack.MessageParentHandle;
                reorderable.ItemRightClicked += eventPack.ItemRightClickEventHandler;
                reorderable.ImageList = QTUtility.ImageListGlobal;
                DirectoryMenuItem item = new DirectoryMenuItem(text);
                item.SetImageReservationKey(info2.FullName, null);
                item.Path = info2.FullName;
                item.EventPack = eventPack;
                item.ModifiedDate = info2.LastWriteTime;
                if(fTruncated) {
                    item.ToolTipText = info2.Name;
                }
                item.DropDown = reorderable;
                item.DoubleClickEnabled = true;
                item.DropDownItems.Add(new ToolStripMenuItem());
                item.DropDownItemClicked += realDirectory_DropDownItemClicked;
                item.DropDownOpening += realDirectory_DropDownOpening;
                item.DoubleClick += eventPack.DirDoubleClickEventHandler;
                parentItem.DropDownItems.Add(item);                
            }
            foreach(FileInfo info3 in info.GetFiles()
                    .Where(info3 => (info3.Attributes & FileAttributes.Hidden) == 0)) {
                string fileNameWithoutExtension;
                string ext = info3.Extension.ToLower();
                switch(ext) {
                    case ".lnk":
                    case ".url":
                        fileNameWithoutExtension = Path.GetFileNameWithoutExtension(info3.Name);
                        break;

                    default:
                        fileNameWithoutExtension = info3.Name;
                        break;
                }
                string str4 = fileNameWithoutExtension;
                QMenuItem item2 = new QMenuItem(QTUtility2.MakeNameEllipsis(fileNameWithoutExtension, out fTruncated), MenuTarget.File, MenuGenre.Application);
                item2.Path = info3.FullName;
                item2.SetImageReservationKey(info3.FullName, ext);
                item2.MouseMove += qmi_File_MouseMove;
                if(fTruncated) {
                    item2.ToolTipText = str4;
                }
                parentItem.DropDownItems.Add(item2);
            }
        }

        public static List<ToolStripItem> CreateAppLauncherItems(IntPtr hwndParent, ShellBrowserEx shellBrowser, bool fReorderEnabled, ItemRightClickedEventHandler rightClickHandler, EventHandler dirDoubleClickEvent, bool fFromTaskBar) {
            // todo: refresh?
            EventPack ep = new EventPack(hwndParent, rightClickHandler, dirDoubleClickEvent, fFromTaskBar);
            return AppsManager.BuildNestedStructure(
                    userApp => CreateMenuItem_AppLauncher(userApp, ep, shellBrowser),
                    (folderName, children) => CreateMenuItem_AppLauncher_Virtual(
                        folderName, children, fReorderEnabled, ep)).ToList();
        }

        public static List<ToolStripItem> CreateGroupItems(ToolStripDropDownItem dropDownItem) {
            List<ToolStripItem> ret = new List<ToolStripItem>();
            DropDownMenuReorderable dropDown = null;
            if(dropDownItem != null) {
                dropDown = (DropDownMenuReorderable)dropDownItem.DropDown;
                while(dropDown.Items.Count > 0) {
                    dropDown.Items[0].Dispose();
                }
                dropDown.ItemsClear();
            }
            const string key = "groups";
            foreach(Group group in GroupsManager.Groups) {
                if(group.Paths.Count == 0 || !QTUtility2.PathExists(group.Paths[0])) continue;
                QMenuItem item = new QMenuItem(group.Name, MenuGenre.Group);
                item.SetImageReservationKey(group.Paths[0], null);
                if(dropDown != null) {
                    dropDown.AddItem(item, key);
                }
                ret.Add(item);
                if(!group.Startup) continue;
                if(StartUpTabFont == null) {
                    StartUpTabFont = new Font(item.Font, FontStyle.Underline);
                }
                item.Font = StartUpTabFont;
            }
            if(dropDownItem != null) {
                dropDownItem.Enabled = dropDown.Items.Count > 0;
            }
            return ret;
        }

        public static QMenuItem CreateMenuItem(MenuItemArguments mia) {
            QMenuItem item = new QMenuItem(QTUtility2.MakePathDisplayText(mia.Path, false), mia);
            if(((mia.Genre == MenuGenre.Navigation) && mia.IsBack) && (mia.Index == 0)) {
                item.ImageKey = "current";
            }
            else {
                item.SetImageReservationKey(mia.Path, null);
            }
            item.ToolTipText = QTUtility2.MakePathDisplayText(mia.Path, true);
            return item;
        }

        private static ToolStripItem CreateMenuItem_AppLauncher(UserApp app, EventPack ep, ShellBrowserEx shellBrowser) {
            string path = app.Path;
            try {
                path = Environment.ExpandEnvironmentVariables(path);
            }
            catch {
            }
            MenuItemArguments mia = new MenuItemArguments(app, shellBrowser, MenuGenre.Application);
            if(path.StartsWith(@"\\") || path.StartsWith("::") || !Directory.Exists(path)) {
                mia.Target = MenuTarget.File;
                QMenuItem item = new QMenuItem(app.Name, mia) { Name = app.Name };
                item.SetImageReservationKey(path, Path.GetExtension(path));
                item.MouseMove += qmi_File_MouseMove;
                if(!ep.FromTaskBar && app.ShortcutKey != Keys.None) {
                    item.ShortcutKeyDisplayString = QTUtility2.MakeKeyString(app.ShortcutKey).Replace(" ", string.Empty);
                }
                return item;
            }
            else {
                mia.Target = MenuTarget.Folder;
                DropDownMenuReorderable reorderable = new DropDownMenuReorderable(null) {
                    MessageParent = ep.MessageParentHandle,
                    ImageList = QTUtility.ImageListGlobal
                };
                reorderable.ItemRightClicked += ep.ItemRightClickEventHandler;
                DirectoryMenuItem item = new DirectoryMenuItem(app.Name) {
                    Name = app.Name,
                    Path = path,
                    MenuItemArguments = mia,
                    EventPack = ep,
                    ModifiedDate = Directory.GetLastWriteTime(path),
                    DropDown = reorderable,
                    DoubleClickEnabled = true
                };
                item.DropDownItems.Add(new ToolStripMenuItem());
                item.DropDownItemClicked += realDirectory_DropDownItemClicked;
                item.DropDownOpening += realDirectory_DropDownOpening;
                item.DoubleClick += ep.DirDoubleClickEventHandler;
                item.SetImageReservationKey(path, null);
                return item;
            }
        }

        private static ToolStripItem CreateMenuItem_AppLauncher_Virtual(string name, ToolStripItem[] items, bool fReorderEnabled, EventPack ep) {
            if(items.Length == 0) return null;
            MenuItemArguments mia = new MenuItemArguments(name, MenuTarget.VirtualFolder, MenuGenre.Application)
            { App = new UserApp(name) };
            DropDownMenuReorderable reorderable = new DropDownMenuReorderable(null) {
                ReorderEnabled = fReorderEnabled,
                MessageParent = ep.MessageParentHandle,
                ImageList = QTUtility.ImageListGlobal
            };
            reorderable.AddItemsRange(items, "userappItem");
            reorderable.ItemRightClicked += ep.ItemRightClickEventHandler;
            reorderable.ItemClicked += virtualDirectory_DropDownItemClicked;
            reorderable.ReorderFinished += virtualDirectory_ReorderFinished;
            return new QMenuItem(name, mia) {
                ImageKey = "folder",
                Name = name,
                DropDown = reorderable
            };
        }

        // todo: check vs quizo's
        public static List<ToolStripItem> CreateRecentFilesItems() {
            List<ToolStripItem> ret = new List<ToolStripItem>();
            List<string> toRemove = new List<string>();
            if(StaticReg.ExecutedPathsList.Count > 0) {
                foreach(string path in StaticReg.ExecutedPathsList.Reverse()) {
                    if(QTUtility2.IsNetworkPath(path) || File.Exists(path)) {
                        QMenuItem item = new QMenuItem(QTUtility2.MakeNameEllipsis(Path.GetFileName(path)), MenuGenre.RecentFile);
                        item.Path = item.ToolTipText = path;
                        item.SetImageReservationKey(path, Path.GetExtension(path));
                        ret.Add(item);
                    }
                    else {
                        toRemove.Add(path);
                    }   
                }
            }
            foreach(string str in toRemove) {
                StaticReg.ExecutedPathsList.Remove(str);
            }
            return ret;
        }

        // todo: check vs quizo's
        public static List<ToolStripItem> CreateUndoClosedItems(ToolStripDropDownItem dropDownItem) {
            List<ToolStripItem> ret = new List<ToolStripItem>();
            string[] reversedLog = StaticReg.ClosedTabHistoryList.Reverse().ToArray();
            if(dropDownItem != null) {
                while(dropDownItem.DropDownItems.Count > 0) {
                    dropDownItem.DropDownItems[0].Dispose();
                }
            }
            if(reversedLog.Length > 0) {
                if(dropDownItem != null) {
                    dropDownItem.Enabled = true;
                }
                foreach(string entry in reversedLog) {
                    if(entry.Length <= 0) continue;
                    if(!QTUtility2.PathExists(entry)) {
                        StaticReg.ClosedTabHistoryList.Remove(entry);
                    }
                    else {
                        QMenuItem item = CreateMenuItem(new MenuItemArguments(entry, MenuTarget.Folder, MenuGenre.History));
                        if(dropDownItem != null) {
                            dropDownItem.DropDownItems.Add(item);
                        }
                        ret.Add(item);
                    }
                }
            }
            else if(dropDownItem != null) {
                dropDownItem.Enabled = false;
            }
            return ret;
        }

        public static void GroupMenu_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            string path = TrackGroupContextMenu(e.ClickedItem.Text, e.IsKey ? e.Point : Control.MousePosition, reorderable.Handle);
            if(!string.IsNullOrEmpty(path)) {
                Action<QTTabBarClass> open = tabBar => {
                    using(IDLWrapper idlw = new IDLWrapper(path)) {
                        tabBar.OpenNewTabOrWindow(idlw);
                    }
                };
                QTTabBarClass threadBar = InstanceManager.GetThreadTabBar();
                if(threadBar != null) {
                    open(threadBar);
                }
                else {
                    InstanceManager.InvokeMain(open);
                }
            }
            else {
                e.HRESULT = 0xfffd;
            }
        }

        private static void qmi_File_MouseMove(object sender, MouseEventArgs e) {
            QMenuItem item = (QMenuItem)sender;
            if(item.ToolTipText != null || string.IsNullOrEmpty(item.Path)) return;
            string str = item.Path.StartsWith("::") ? item.Text : Path.GetFileName(item.Path);
            string shellInfoTipText = ShellMethods.GetShellInfoTipText(item.Path, false);
            if(shellInfoTipText != null) {
                if(str == null) {
                    str = shellInfoTipText;
                }
                else {
                    str = str + "\r\n" + shellInfoTipText;
                }
            }
            item.ToolTipText = str;
        }

        private static void realDirectory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (e.ClickedItem is DirectoryMenuItem) { return;}

            try {
                Process.Start(((QMenuItem)e.ClickedItem).Path);
            } catch {
                MessageBox.Show(
                    String.Format(
                        QTUtility.TextResourcesDic["ErrorDialogs"][0],
                        e.ClickedItem.Name
                    ),
                    QTUtility.TextResourcesDic["ErrorDialogs"][1],
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Hand
                );
            }
        }

        private static void realDirectory_DropDownOpening(object sender, EventArgs e) {
            DirectoryMenuItem parentItem = (DirectoryMenuItem)sender;
            if(!parentItem.OnceOpened) {
                parentItem.OnceOpened = true;
                parentItem.DropDown.SuspendLayout();
                parentItem.DropDownItems[0].Dispose();
                AddChildrenOnOpening(parentItem);
                parentItem.DropDown.ResumeLayout();
                if(!QTUtility.IsXP) {
                    parentItem.DropDown.BringToFront();
                }
            }
            else {
                DateTime lastWriteTime = Directory.GetLastWriteTime(parentItem.Path);
                if(parentItem.ModifiedDate != lastWriteTime) {
                    parentItem.DropDown.SuspendLayout();
                    parentItem.ModifiedDate = lastWriteTime;
                    while(parentItem.DropDownItems.Count > 0) {
                        parentItem.DropDownItems[0].Dispose();
                    }
                    AddChildrenOnOpening(parentItem);
                    parentItem.DropDown.ResumeLayout();
                }
            }
        }

        // TODO: what does this do?!
        // TODO: whatever it does, it should be returning an idl, not a path.
        public static string TrackGroupContextMenu(string groupName, Point pnt, IntPtr pDropDownHandle) {
            string name = string.Empty;
            Group g = GroupsManager.GetGroup(groupName);
            if(g == null) return name;
            ContextMenu menu = new ContextMenu();
            if(!QTUtility.IsXP) {
                foreach(string str2 in g.Paths) {
                    string text;
                    if(str2.StartsWith(@"\\")) {
                        text = str2;
                    }
                    else {
                        text = ShellMethods.GetDisplayName(str2);
                    }
                    MenuItem item = new MenuItem(text);
                    item.Name = str2;
                    menu.MenuItems.Add(item);
                }
            }
            else {
                foreach(string path in g.Paths) {
                    string displayName;
                    if(path.StartsWith(@"\\")) {
                        displayName = path;
                    }
                    else {
                        displayName = ShellMethods.GetDisplayName(path);
                    }
                    MenuItemEx ex = new MenuItemEx(displayName);
                    ex.Name = path;
                    ex.Image = QTUtility.ImageListGlobal.Images[QTUtility.GetImageKey(path, null)];
                    menu.MenuItems.Add(ex);
                }
            }
            List<IntPtr> list = new List<IntPtr>();
            if(!QTUtility.IsXP) {
                for(int k = 0; k < g.Paths.Count; k++) {
                    string imageKey = QTUtility.GetImageKey(g.Paths[k], null);
                    IntPtr hbitmap = ((Bitmap)QTUtility.ImageListGlobal.Images[imageKey]).GetHbitmap(Color.Black);
                    if(hbitmap != IntPtr.Zero) {
                        list.Add(hbitmap);
                        PInvoke.SetMenuItemBitmaps(menu.Handle, k, 0x400, hbitmap, IntPtr.Zero);
                    }
                }
            }
            uint maxValue = uint.MaxValue;
            if(menu.MenuItems.Count > 0) {
                maxValue = PInvoke.TrackPopupMenu(menu.Handle, 0x180, pnt.X, pnt.Y, 0, pDropDownHandle, IntPtr.Zero);
                if(maxValue != 0) {
                    for(int m = 0; m < menu.MenuItems.Count; m++) {
                        if(maxValue == PInvoke.GetMenuItemID(menu.Handle, m)) {
                            name = menu.MenuItems[m].Name;
                            break;
                        }
                    }
                }
            }
            menu.Dispose();
            if(!QTUtility.IsXP) {
                foreach(IntPtr ptr2 in list) {
                    PInvoke.DeleteObject(ptr2);
                }
            }
            if(maxValue != 0) {
                return name;
            }
            return null;
        }

        private static void virtualDirectory_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem == null || clickedItem.Target != MenuTarget.File) return;
            MenuItemArguments mia = clickedItem.MenuItemArguments;
            AppsManager.Execute(mia.App, mia.ShellBrowser);
        }

        private static void virtualDirectory_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            AppsManager.HandleReorder(((DropDownMenuReorderable)sender).Items.Cast<ToolStripItem>());
        }
    }

    internal sealed class EventPack {
        public EventHandler DirDoubleClickEventHandler;
        public bool FromTaskBar;
        public ItemRightClickedEventHandler ItemRightClickEventHandler;
        public IntPtr MessageParentHandle;

        public EventPack(IntPtr hwnd, ItemRightClickedEventHandler handlerRightClick, EventHandler handlerDirDblClick, bool fFromTaskBar) {
            MessageParentHandle = hwnd;
            ItemRightClickEventHandler = handlerRightClick;
            DirDoubleClickEventHandler = handlerDirDblClick;
            FromTaskBar = fFromTaskBar;
        }
    }
}

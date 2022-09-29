//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano, indiff
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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BandObjectLib;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    /***
     * 最小化的右下角
     */
    internal class TrayIcon
    {
        private struct Instance
        {
            public int ShowWindowCode;
            public IntPtr ExplorerHandle;
            public IntPtr TabBarHandle;
            public string CurrentPath;
            public string[] TabNames;
            public string[] TabPaths;
        }

        private ContextMenuStripEx contextMenuNotifyIcon;
        private Dictionary<IntPtr, Instance> dicNotifyIcon = new Dictionary<IntPtr, Instance>();
        private Icon icoNotify;
        private NotifyIcon notifyIcon;

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) RestoreAllWindowsFromTray();
        }

        private void contextMenuNotifyIcon_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem is ToolStripSeparator) return;
            if (e.ClickedItem.Tag is IntPtr)
            {
                ShowExplorerWindow((IntPtr)e.ClickedItem.Tag, true);
            }
            else
            {
                if ((int)e.ClickedItem.Tag == 0)
                {
                    RestoreAllWindowsFromTray();
                }
                else
                {
                    // Close all
                    notifyIcon.Visible = false;
                    contextMenuNotifyIcon.Hide();
                    var handles = dicNotifyIcon.Values.Select(i => i.ExplorerHandle).ToList();
                    dicNotifyIcon.Clear(); // <--- important, clear the dict first!
                    // otherwise, the closing windows will try to call back
                    // and we'll deadlock
                    foreach (var ptr in handles) WindowUtils.CloseExplorer(ptr, 2);
                }
            }
        }

        private void contextMenuNotifyIcon_SubItems_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var tag = (IntPtr)e.ClickedItem.Tag;
            var index = ((ToolStripMenuItem)sender).DropDownItems.IndexOf(e.ClickedItem);
            ShowExplorerWindow(tag, true);
            InstanceManager.SelectTabOnOtherTabBar(tag, index);
        }

        public void AddToTrayIcon(IntPtr tabBarHandle, IntPtr explorerHandle, string currentPath, string[] tabNames,
            string[] tabPaths)
        {
            CreateNotifyIcon();
            dicNotifyIcon[tabBarHandle] = new Instance
            {
                ShowWindowCode = PInvoke.IsZoomed(explorerHandle) ? 3 : 1,
                CurrentPath = currentPath,
                ExplorerHandle = explorerHandle,
                TabBarHandle = tabBarHandle,
                TabNames = tabNames,
                TabPaths = tabPaths
            };
            ShowExplorerWindow(tabBarHandle, false);
        }

        private void CreateNotifyIcon()
        {
            if (notifyIcon != null) return;
            var thread = new Thread(() =>
            {
                // We need to make a new thread because we have to guarantee
                // contextMenuNotifyIcon is only accessed from a single thread.
                // InstanceManager could call us from many different threads.
                lock (dicNotifyIcon)
                {
                    if (notifyIcon != null) return; // double check to prevent race conditions
                    icoNotify = QTUtility.GetIcon(string.Empty, false);
                    contextMenuNotifyIcon = new ContextMenuStripEx(null, false);
                    contextMenuNotifyIcon.ImageList = QTUtility.ImageListGlobal;
                    contextMenuNotifyIcon.ItemClicked += contextMenuNotifyIcon_ItemClicked;
                    contextMenuNotifyIcon.EnsureHandleCreated();
                    notifyIcon = new NotifyIcon
                    {
                        Icon = icoNotify,
                        ContextMenuStrip = contextMenuNotifyIcon,
                        Visible = false
                    };
                    notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
                    Monitor.Pulse(dicNotifyIcon);
                }

                Application.Run();
            }) { IsBackground = true };
            lock (dicNotifyIcon)
            {
                thread.Start();
                Monitor.Wait(dicNotifyIcon);
            }
        }

        private void RestoreAllWindowsFromTray()
        {
            foreach (var ptr in dicNotifyIcon.Keys.ToList()) ShowExplorerWindow(ptr, true);
        }

        public void RestoreWindow(IntPtr tabBarHandle)
        {
            CreateNotifyIcon();
            if (dicNotifyIcon.ContainsKey(tabBarHandle)) ShowExplorerWindow(tabBarHandle, true);
        }

        private void ShowExplorerWindow(IntPtr tabBarHandle, bool fShow)
        {
            if (contextMenuNotifyIcon.InvokeRequired)
            {
                contextMenuNotifyIcon.Invoke(new Action(() => ShowExplorerWindow(tabBarHandle, fShow)));
                return;
            }

            Instance inst;
            if (!dicNotifyIcon.TryGetValue(tabBarHandle, out inst)) return;
            ITaskbarList o = null;
            try
            {
                object obj2;
                var rclsid = ExplorerGUIDs.CLSID_TaskbarList;
                var riid = ExplorerGUIDs.IID_ITaskbarList;
                PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj2);
                o = (ITaskbarList)obj2;
                o.HrInit();
                if (fShow)
                {
                    dicNotifyIcon.Remove(tabBarHandle);
                    o.AddTab(inst.ExplorerHandle);
                    PInvoke.ShowWindow(inst.ExplorerHandle, inst.ShowWindowCode);
                    PInvoke.SetForegroundWindow(inst.ExplorerHandle);
                    notifyIcon.Visible = dicNotifyIcon.Count > 0;
                }
                else
                {
                    PInvoke.ShowWindow(inst.ExplorerHandle, 0);
                    o.DeleteTab(inst.ExplorerHandle);
                    notifyIcon.Visible = true;
                }

                UpdateContextMenu();
                if (notifyIcon.Visible)
                {
                    var count = dicNotifyIcon.Count;
                    notifyIcon.Text = count == 1
                        ? QTUtility.TextResourcesDic["TrayIcon"][0]
                        : string.Format(QTUtility.TextResourcesDic["TrayIcon"][1], count);
                }
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception);
            }
            finally
            {
                if (o != null) Marshal.ReleaseComObject(o);
            }
        }

        private void UpdateContextMenu()
        {
            contextMenuNotifyIcon.Hide();
            contextMenuNotifyIcon.SuspendLayout();
            contextMenuNotifyIcon.Items.Clear();
            foreach (var inst in dicNotifyIcon.Values)
            {
                var lpString = new StringBuilder(260);
                PInvoke.GetWindowText(inst.ExplorerHandle, lpString, lpString.Capacity);
                var item = new ToolStripMenuItem(lpString.ToString());
                item.Tag = inst.TabBarHandle;
                if (inst.CurrentPath.Length > 0)
                {
                    item.ToolTipText = QTUtility2.MakePathDisplayText(inst.CurrentPath, true);
                    item.ImageKey = QTUtility.GetImageKey(inst.CurrentPath, null);
                }

                var j = Math.Min(inst.TabNames.Length, inst.TabPaths.Length);
                if (j > 1)
                {
                    for (var i = 0; i < j; i++)
                        item.DropDownItems.Add(new ToolStripMenuItem(inst.TabNames[i])
                        {
                            Tag = inst.TabBarHandle,
                            ToolTipText = QTUtility2.MakePathDisplayText(inst.TabPaths[i], true),
                            ImageKey = QTUtility.GetImageKey(inst.TabPaths[i], null)
                        });
                    if (item.DropDownItems.Count > 0)
                    {
                        item.DropDownItemClicked += contextMenuNotifyIcon_SubItems_DropDownItemClicked;
                        item.DropDown.ImageList = QTUtility.ImageListGlobal;
                    }
                }

                contextMenuNotifyIcon.Items.Add(item);
            }

            // QTUtility.TextResourcesDic["OptionsDialog"] 设置国际化
            var resDic = QTUtility.TextResourcesDic["TrayIcon"];
            var isRightFlag = null != resDic && resDic.Length > 2;
            var strRestore = isRightFlag ? resDic[2] : "Restore All";
            var strClose = isRightFlag ? resDic[3] : "Close All";

            contextMenuNotifyIcon.Items.Add(new ToolStripSeparator());
            contextMenuNotifyIcon.Items.Add(new ToolStripMenuItem(strRestore) { Tag = 0 });
            contextMenuNotifyIcon.Items.Add(new ToolStripMenuItem(strClose) { Tag = 1 });
            contextMenuNotifyIcon.ResumeLayout();
        }
    }
}
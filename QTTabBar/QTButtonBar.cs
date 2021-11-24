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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    [ComVisible(true), Guid("d2bf470e-ed1c-487f-a666-2bd8835eb6ce")]
    public sealed class QTButtonBar : BandObject {

        internal const int BII_NAVIGATION_DROPDOWN  = -1;
		internal const int BII_SEPARATOR			=  0;
        internal const int BII_NAVIGATION_BACK      =  1;
        internal const int BII_NAVIGATION_FWRD      =  2;
		internal const int BII_GROUP				=  3;
		internal const int BII_RECENTTAB			=  4;
		internal const int BII_APPLICATIONLAUNCHER	=  5;
        internal const int BII_NEWWINDOW            =  6;
        internal const int BII_CLONE                =  7;
        internal const int BII_LOCK                 =  8;
		internal const int BII_MISCTOOL				=  9;
        internal const int BII_TOPMOST              = 10;
        internal const int BII_CLOSE_CURRENT        = 11;
		internal const int BII_CLOSE_ALLBUTCURRENT	= 12;
        internal const int BII_CLOSE_WINDOW         = 13;
        internal const int BII_CLOSE_LEFT           = 14;
        internal const int BII_CLOSE_RIGHT          = 15;
        internal const int BII_GOUPONELEVEL         = 16;
        internal const int BII_REFRESH_SHELLBROWSER = 17;
        internal const int BII_SHELLSEARCH          = 18;
        //internal const int BII_OPTION               = 19;  todo...
        //internal const int BII_RECENTFILE           = 20;
		internal const int BII_WINDOWOPACITY        = 19;
        internal const int BII_FILTERBAR            = 20;
        internal const int BII_OPTION = 21;  // todo...

        /// <summary>
        ///  内部的按钮的个数 add by qwop.
        /// </summary>
        internal const int INTERNAL_BUTTON_COUNT    = 22;
       

        private static readonly Regex reAsterisc = new Regex(@"\\\*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex reQuestion = new Regex(@"\\\?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Size sizeLargeButton = new Size(24, 24);
        private static readonly Size sizeSmallButton = new Size(16, 16);
        private static readonly ImageStrip imageStrip_Large = new ImageStrip(sizeLargeButton);
        private static readonly ImageStrip imageStrip_Small = new ImageStrip(sizeSmallButton);

        private VisualStyleRenderer BackgroundRenderer;
        private const int BARHEIGHT_LARGE = 34;
        private const int BARHEIGHT_SMALL = 26;
        private IContainer components;
        private DropDownMenuReorderable ddmrGroupButton;
        private DropDownMenuReorderable ddmrRecentlyClosed;
        private DropDownMenuReorderable ddmrUserAppButton;
        private DropTargetWrapper dropTargetWrapper;
        private IntPtr ExplorerHandle;
        private bool fRearranging;
        private bool fSearchBoxInputStart;
        private ShellContextMenu shellContextMenu = new ShellContextMenu();
        private const int INTERVAL_REARRANGE = 300;
        private const int INTERVAL_SEARCHSTART = 250;
        private int iSearchResultCount = -1;
        private List<ToolStripItem> lstPluginCustomItem = new List<ToolStripItem>();
        private List<IntPtr> lstPUITEMIDCHILD = new List<IntPtr>();
        private DropDownMenuBase NavDropDown;
        private ToolStripSearchBox searchBox;
        private ShellBrowserEx shellBrowser;
        private string strSearch = string.Empty;
        private Timer timerSearchBox_Rearrange;
        private Timer timerSerachBox_Search;
        private ToolStripClasses toolStrip;

        public QTButtonBar() {
            InitializeComponent();
        }

        private void ActivatedByClickOnThis() {
            Point point = toolStrip.PointToClient(MousePosition);
            ToolStripItem itemAt = toolStrip.GetItemAt(point);
            if((itemAt != null) && itemAt.Enabled) {
                if(itemAt is ToolStripSplitButton) {
                    if(((itemAt.Bounds.X + ((ToolStripSplitButton)itemAt).ButtonBounds.Width) + ((ToolStripSplitButton)itemAt).SplitterBounds.Width) < point.X) {
                        ((ToolStripSplitButton)itemAt).ShowDropDown();
                    }
                    else {
                        ((ToolStripSplitButton)itemAt).PerformButtonClick();
                    }
                }
                else if(itemAt is ToolStripDropDownItem) {
                    ((ToolStripDropDownItem)itemAt).ShowDropDown();
                }
                else {
                    itemAt.PerformClick();
                }
            }
        }

        private void AddHistoryItems(ToolStripDropDownItem button) {
            QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
            if(tabBar != null) {
                button.DropDownItems.Clear();
                List<QMenuItem> list = tabBar.CreateNavBtnMenuItems(true);
                if(list.Count != 0) {
                    button.DropDownItems.AddRange(list.ToArray());
                    button.DropDownItems.AddRange(tabBar.CreateBranchMenu(true, components, navBranchRoot_DropDownItemClicked).ToArray());
                }
                else {
                    ToolStripMenuItem item = new ToolStripMenuItem("none");
                    item.Enabled = false;
                    button.DropDownItems.Add(item);
                }
            }
        }

        private void AddUserAppItems() {
            if(ddmrUserAppButton == null) return;
            while(ddmrUserAppButton.Items.Count > 0) {
                ddmrUserAppButton.Items[0].Dispose();
            }

            // todo: the button bar should have its *own* ShellBrowserEx!
            QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
            if(tabBar == null) return;
            List<ToolStripItem> lstItems = MenuUtility.CreateAppLauncherItems(Handle, tabBar.GetShellBrowser(),
                    !Config.BBar.LockDropDownButtons, ddmr45_ItemRightClicked, userAppsSubDir_DoubleClicked, false);
            ddmrUserAppButton.AddItemsRange(lstItems.ToArray(), "u");
        }

        private void AsyncComplete(IAsyncResult ar) {
            AsyncResult result = (AsyncResult)ar;
            ((WaitTimeoutCallback)result.AsyncDelegate).EndInvoke(ar);
            if(IsHandleCreated) {
                Invoke(new MethodInvoker(CallBackSearchBox));
            }
        }

        private static int BarHeight {
            get { return Config.BBar.LargeButtons ? BARHEIGHT_LARGE : BARHEIGHT_SMALL; }
        }

        private void CallBackSearchBox() {
            Explorer.Refresh();
        }

        private static bool CheckDisplayName(IShellFolder shellFolder, IntPtr pIDLLast, Regex re) {
            if(pIDLLast != IntPtr.Zero) {
                STRRET strret;
                uint uFlags = 0;
                StringBuilder pszBuf = new StringBuilder(260);
                if(shellFolder.GetDisplayNameOf(pIDLLast, uFlags, out strret) == 0) {
                    PInvoke.StrRetToBuf(ref strret, pIDLLast, pszBuf, pszBuf.Capacity);
                }
                if(pszBuf.Length > 0) {
                    return re.IsMatch(pszBuf.ToString());
                }
            }
            return false;
        }

        private void ClearToolStripItems() {
            List<ToolStripItem> list = toolStrip.Items.Cast<ToolStripItem>()
                    .Except(lstPluginCustomItem).ToList();
            toolStrip.Items.Clear();
            lstPluginCustomItem.Clear();
            foreach(ToolStripItem item in list) {
                item.Dispose();
            }
        }

        public override void CloseDW(uint dwReserved) {
            try {
                if(shellContextMenu != null) {
                    shellContextMenu.Dispose();
                    shellContextMenu = null;
                }
                foreach(IntPtr ptr in lstPUITEMIDCHILD) {
                    if(ptr != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(ptr);
                    }
                }
                if(dropTargetWrapper != null) {
                    dropTargetWrapper.Dispose();
                    dropTargetWrapper = null;
                }
                InstanceManager.UnregisterButtonBar();
                fFinalRelease = false;
                base.CloseDW(dwReserved);
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "buttonbar closing");
            }
        }
        
        private void copyButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
            if(tabBar != null) {
                tabBar.DoFileTools(((DropDownMenuBase)sender).Items.IndexOf(e.ClickedItem));
            }
        }

        private void copyButton_Opening(object sender, CancelEventArgs e) {
            Address[] addressArray;
            string str;
            toolStrip.HideToolTip();
            DropDownMenuBase base2 = (DropDownMenuBase)sender;
            for(int i = 0; i < 5; i++) {
                int num2 = Config.Keys.Shortcuts[0x1b + i];
                if(num2 > 0x100000) {
                    num2 -= 0x100000;
                    ((ToolStripMenuItem)base2.Items[i]).ShortcutKeyDisplayString = QTUtility2.MakeKeyString((Keys)num2).Replace(" ", string.Empty);
                }
                else {
                    ((ToolStripMenuItem)base2.Items[i]).ShortcutKeyDisplayString = string.Empty;
                }
            }
            QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
            if((tabBar != null) && tabBar.TryGetSelection(out addressArray, out str, false)) {
                base2.Items[0].Enabled = base2.Items[1].Enabled = addressArray.Length > 0;
                base2.Items[2].Enabled = base2.Items[3].Enabled = true;
            }
            else {
                base2.Items[0].Enabled = base2.Items[1].Enabled = base2.Items[2].Enabled = base2.Items[3].Enabled = false;
            }
        }

        private ToolStripDropDownButton CreateDropDownButton(int index) {
            ToolStripDropDownButton button = new ToolStripDropDownButton();
            switch(index) {
                case -1:
                    if(NavDropDown == null) {
                        NavDropDown = new DropDownMenuBase(components, true, true, true);
                        NavDropDown.ImageList = QTUtility.ImageListGlobal;
                        NavDropDown.ItemClicked += dropDownButtons_DropDown_ItemClicked;
                        NavDropDown.Closed += dropDownButtons_DropDown_Closed;
                    }
                    button.DropDown = NavDropDown;
                    button.Tag = -1;
                    button.AutoToolTip = false;
                    break;

                case 3:
                    if(ddmrGroupButton == null) {
                        ddmrGroupButton = new DropDownMenuReorderable(components, true, false);
                        ddmrGroupButton.ImageList = QTUtility.ImageListGlobal;
                        ddmrGroupButton.ReorderEnabled = !Config.BBar.LockDropDownButtons;
                        ddmrGroupButton.ItemRightClicked += MenuUtility.GroupMenu_ItemRightClicked;
                        ddmrGroupButton.ItemMiddleClicked += ddmrGroupButton_ItemMiddleClicked;
                        ddmrGroupButton.ReorderFinished += dropDownButtons_DropDown_ReorderFinished;
                        ddmrGroupButton.ItemClicked += dropDownButtons_DropDown_ItemClicked;
                        ddmrGroupButton.Closed += dropDownButtons_DropDown_Closed;
                    }
                    button.DropDown = ddmrGroupButton;
                    button.Enabled = GroupsManager.GroupCount > 0;
                    break;

                case 4:
                    if(ddmrRecentlyClosed == null) {
                        ddmrRecentlyClosed = new DropDownMenuReorderable(components, true, false);
                        ddmrRecentlyClosed.ImageList = QTUtility.ImageListGlobal;
                        ddmrRecentlyClosed.ReorderEnabled = false;
                        ddmrRecentlyClosed.MessageParent = Handle;
                        ddmrRecentlyClosed.ItemRightClicked += ddmr45_ItemRightClicked;
                        ddmrRecentlyClosed.ItemClicked += dropDownButtons_DropDown_ItemClicked;
                        ddmrRecentlyClosed.Closed += dropDownButtons_DropDown_Closed;
                    }
                    button.DropDown = ddmrRecentlyClosed;
                    button.Enabled = StaticReg.ClosedTabHistoryList.Count > 0;
                    break;

                case 5:
                    if(ddmrUserAppButton == null) {
                        ddmrUserAppButton = new DropDownMenuReorderable(components);
                        ddmrUserAppButton.ImageList = QTUtility.ImageListGlobal;
                        ddmrUserAppButton.ReorderEnabled = !Config.BBar.LockDropDownButtons;
                        ddmrUserAppButton.MessageParent = Handle;
                        ddmrUserAppButton.ItemRightClicked += ddmr45_ItemRightClicked;
                        ddmrUserAppButton.ReorderFinished += dropDownButtons_DropDown_ReorderFinished;
                        ddmrUserAppButton.ItemClicked += dropDownButtons_DropDown_ItemClicked;
                        ddmrUserAppButton.Closed += dropDownButtons_DropDown_Closed;
                    }
                    button.DropDown = ddmrUserAppButton;
                    button.Enabled = AppsManager.UserApps.Any();
                    break;
            }
            button.DropDownOpening += dropDownButtons_DropDownOpening;
            return button;
        }

        internal void CreateItems()
        {
            string[] ButtonItemsDisplayName = QTUtility.TextResourcesDic["ButtonBar_BtnName"];
            ManageImageList();
            toolStrip.SuspendLayout();
            if(iSearchResultCount != -1) {
                Explorer.Refresh();
            }
            RefreshSearchBox(false);
            if(searchBox != null) {
                searchBox.Dispose();
                timerSerachBox_Search.Dispose();
                timerSearchBox_Rearrange.Dispose();
                searchBox = null;
                timerSerachBox_Search = null;
                timerSearchBox_Rearrange = null;
            }
            ClearToolStripItems();
            toolStrip.ShowItemToolTips = true;
            Height = Config.BBar.LargeButtons ? 34 : 26;
            bool showButtonLabels = Config.BBar.ShowButtonLabels;
            UnloadPluginsOnCreation();
            foreach(int index in Config.BBar.ButtonIndexes) {
                ToolStripItem item;
                switch(index) {
                    case BII_SEPARATOR:
                        toolStrip.Items.Add(new ToolStripSeparator {Tag = 0});
                        continue;

                    case BII_GROUP:
                    case BII_RECENTTAB:
                    case BII_APPLICATIONLAUNCHER:
                        item = CreateDropDownButton(index);
                        break;

                    case BII_MISCTOOL: // 复制工具的
                        string[] strArray = QTUtility.TextResourcesDic["ButtonBar_Misc"];
                        DropDownMenuBase base2 = new DropDownMenuBase(components) {
                                ShowCheckMargin = !QTUtility.IsXP,
                                ShowImageMargin = false
                        };
                        base2.Items.AddRange(new ToolStripItem[] {
                                new ToolStripMenuItem(strArray[0]),
                                new ToolStripMenuItem(strArray[1]),
                                new ToolStripMenuItem(strArray[2]),
                                new ToolStripMenuItem(strArray[3]),
                                new ToolStripMenuItem(strArray[4]),
                                new ToolStripMenuItem(strArray[6])
                        });
                        base2.ItemClicked += copyButton_DropDownItemClicked;
                        base2.Opening += copyButton_Opening;
                        item = new ToolStripDropDownButton {DropDown = base2};
                        break;

                    case BII_TOPMOST:
                        item = new ToolStripButton {CheckOnClick = true};
                        break;

                    case BII_WINDOWOPACITY:
                        ToolStripTrackBar bar = new ToolStripTrackBar {
                            Tag = index,
                            ToolTipText = ButtonItemsDisplayName[19]
                        };
                        int crKey, dwFlg;
                        byte bAlpha;
                        if(PInvoke.GetLayeredWindowAttributes(ExplorerHandle, out crKey, out bAlpha, out dwFlg)) {
                            bar.SetValueWithoutEvent(bAlpha);
                        }
                        bar.ValueChanged += trackBar_ValueChanged;
                        toolStrip.Items.Add(bar);
                        continue;

                    case BII_FILTERBAR:
                        searchBox = new ToolStripSearchBox(
                                Config.BBar.LargeButtons, 
                                Config.BBar.LockSearchBarWidth,
                                ButtonItemsDisplayName[0x12], 
                                SearchBoxWidth) {
                            ToolTipText = ButtonItemsDisplayName[20], 
                            Tag = index
                        };
                        searchBox.ErasingText += searchBox_ErasingText;
                        searchBox.ResizeComplete += searchBox_ResizeComplete;
                        searchBox.TextChanged += searchBox_TextChanged;
                        searchBox.KeyPress += searchBox_KeyPress;
                        searchBox.GotFocus += searchBox_GotFocus;
                        toolStrip.Items.Add(searchBox);
                        timerSerachBox_Search = new Timer(components) {Interval = 250};
                        timerSerachBox_Search.Tick += timerSerachBox_Search_Tick;
                        timerSearchBox_Rearrange = new Timer(components) {Interval = 300};
                        timerSearchBox_Rearrange.Tick += timerSearchBox_Rearrange_Tick;
                        continue;

                    default:
                        if(index >= INTERNAL_BUTTON_COUNT) {
                            if(index.HiWord() > 0) CreatePluginItem(index);
                            continue;
                        }
                        item = new ToolStripButton();
                        break;
                }
                item.DisplayStyle = showButtonLabels
                        ? ToolStripItemDisplayStyle.ImageAndText
                        : ToolStripItemDisplayStyle.Image;
                item.ImageScaling = ToolStripItemImageScaling.None;
                item.Text = item.ToolTipText = ButtonItemsDisplayName[index];
                item.Image = (Config.BBar.LargeButtons ? imageStrip_Large[index - 1] : imageStrip_Small[index - 1])
                       .Clone(new Rectangle(Point.Empty, Config.BBar.LargeButtons ? sizeLargeButton : sizeSmallButton), PixelFormat.Format32bppArgb);
               
                // todo delete qwop 
                // MessageBox.Show(index + " -> " + imageStrip_Large.LstImagesLength());

                item.Tag = index;
                toolStrip.Items.Add(item);
                if((index == BII_NAVIGATION_BACK && Array.IndexOf(Config.BBar.ButtonIndexes, BII_NAVIGATION_FWRD) == -1) || index == BII_NAVIGATION_FWRD) {
                    toolStrip.Items.Add(CreateDropDownButton(BII_NAVIGATION_DROPDOWN));
                }
            }
            if(Config.BBar.ButtonIndexes.Length == 0) {
                toolStrip.Items.Add(new ToolStripSeparator {Tag = 0});
            }

            // todo: check
            QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
            if(tabBar != null) {
                tabBar.rebarController.RefreshHeight();
            }
            RefreshButtons();

            toolStrip.ResumeLayout();
            toolStrip.RaiseOnResize();
        }

        private void CreatePluginItem(int buttonIndex) {
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            if(tabbar == null) return;
            QTTabBarClass.PluginServer pluginServer = tabbar.pluginServer;
            if(pluginServer == null) return;
            string pluginID = Config.BBar.ActivePluginIDs[buttonIndex.HiWord() - 1];
            try {
                bool showText = Config.BBar.ShowButtonLabels;
                PluginInformation pi = PluginManager.PluginInformations.FirstOrDefault(info => info.PluginID == pluginID);
                if(pi == null || !pi.Enabled) return;
                Plugin plugin;
                pluginServer.TryGetPlugin(pluginID, out plugin);
                if(plugin == null) {
                    plugin = pluginServer.Load(pi, null);
                }
                if(plugin == null) return;

                ToolStripItem itemToAdd = null;
                if(plugin.Instance is IBarDropButton) {
                    IBarDropButton instance = (IBarDropButton)plugin.Instance;
                    instance.InitializeItem();
                    if(instance.IsSplitButton) {
                        itemToAdd = new ToolStripSplitButton(instance.Text) {
                                ImageScaling = ToolStripItemImageScaling.None,
                                DropDownButtonWidth = Config.BBar.LargeButtons ? 14 : 11,
                                DisplayStyle = showText
                                        ? ToolStripItemDisplayStyle.ImageAndText
                                        : ToolStripItemDisplayStyle.Image,
                                ToolTipText = instance.Text,
                                Image = instance.GetImage(Config.BBar.LargeButtons)
                        };
                        DropDownMenuReorderable reorder = new DropDownMenuReorderable(components);
                        reorder.ItemClicked += pluginDropDown_ItemClicked;
                        reorder.ItemRightClicked += pluginDropDown_ItemRightClicked;
                        ((ToolStripSplitButton)itemToAdd).DropDown = reorder;
                        ((ToolStripSplitButton)itemToAdd).DropDownOpening += pluginDropDown_DropDownOpening;
                        ((ToolStripSplitButton)itemToAdd).ButtonClick += pluginButton_ButtonClick;
                    }
                    else {
                        itemToAdd = new ToolStripDropDownButton(instance.Text) {
                                ImageScaling = ToolStripItemImageScaling.None,
                                DisplayStyle = showText
                                        ? ToolStripItemDisplayStyle.ImageAndText
                                        : ToolStripItemDisplayStyle.Image,
                                ToolTipText = instance.Text,
                                Image = instance.GetImage(Config.BBar.LargeButtons)
                        };
                        DropDownMenuReorderable reorder = new DropDownMenuReorderable(components);
                        reorder.ItemClicked += pluginDropDown_ItemClicked;
                        reorder.ItemRightClicked += pluginDropDown_ItemRightClicked;
                        ((ToolStripDropDownButton)itemToAdd).DropDown = reorder;
                        ((ToolStripDropDownButton)itemToAdd).DropDownOpening += pluginDropDown_DropDownOpening;
                    }
                }
                else if(plugin.Instance is IBarButton) {
                    IBarButton instance = (IBarButton)plugin.Instance;
                    instance.InitializeItem();
                    itemToAdd = new ToolStripButton(instance.Text) {
                            ImageScaling = ToolStripItemImageScaling.None,
                            DisplayStyle = showText
                                    ? ToolStripItemDisplayStyle.ImageAndText
                                    : ToolStripItemDisplayStyle.Image,
                            ToolTipText = instance.Text,
                            Image = instance.GetImage(Config.BBar.LargeButtons)
                    };
                    itemToAdd.Click += pluginButton_ButtonClick;
                }
                else if(plugin.Instance is IBarCustomItem) {
                    IBarCustomItem instance = (IBarCustomItem)plugin.Instance;
                    DisplayStyle displayStyle = showText ? DisplayStyle.ShowTextLabel : DisplayStyle.NoLabel;
                    itemToAdd = instance.CreateItem(Config.BBar.LargeButtons, displayStyle);
                    if(itemToAdd != null) {
                        itemToAdd.ImageScaling = ToolStripItemImageScaling.None;
                        lstPluginCustomItem.Add(itemToAdd);
                    }
                }
                else if(plugin.Instance is IBarMultipleCustomItems) {
                    IBarMultipleCustomItems instance = (IBarMultipleCustomItems)plugin.Instance;
                    if(buttonIndex.LoWord() >= instance.Count) return;
                    if(!plugin.BackgroundButtonEnabled) {
                        // This is to maintain backwards compatibility.
                        instance.Initialize(instance.Count.RangeSelect(i => i).ToArray());
                    }
                    DisplayStyle style = showText ? DisplayStyle.ShowTextLabel : DisplayStyle.NoLabel;
                    itemToAdd = instance.CreateItem(Config.BBar.LargeButtons, style, buttonIndex.LoWord());
                    if(itemToAdd != null) {
                        lstPluginCustomItem.Add(itemToAdd);
                    }
                }

                if(itemToAdd != null) {
                    itemToAdd.Tag = buttonIndex;
                    toolStrip.Items.Add(itemToAdd);
                    if(pi.PluginType == PluginType.Background || pi.PluginType == PluginType.BackgroundMultiple) {
                        plugin.BackgroundButtonEnabled = true;
                    }
                }
            }
            catch(Exception exception) {
                PluginManager.HandlePluginException(exception, ExplorerHandle, pluginID, "Loading plugin button.");
            }
        }

        private void ddmr45_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                bool fCanRemove = sender == ddmrRecentlyClosed;
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    e.HRESULT = shellContextMenu.Open(wrapper, e.IsKey ? e.Point : MousePosition, ((DropDownMenuReorderable)sender).Handle, fCanRemove);
                }
                if(fCanRemove && (e.HRESULT == 0xffff)) {
                    StaticReg.ClosedTabHistoryList.Remove(clickedItem.Path);
                    clickedItem.Dispose();
                }
            }
        }

        private static void ddmrGroupButton_ItemMiddleClicked(object sender, ItemRightClickedEventArgs e) {
            InstanceManager.GetThreadTabBar().ReplaceByGroup(e.ClickedItem.Text);
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        internal bool ClickItem(int index) {
            ToolStripItem item = toolStrip.Items.Cast<ToolStripItem>()
                    .FirstOrDefault(item2 => item2.Tag != null && (int)item2.Tag == index);
            if(item == null) {
                return false;
            }
            else if(item is ToolStripDropDownItem) {
                ((ToolStripDropDownItem)item).ShowDropDown();
            }
            else {
                item.PerformClick();
            }
            return true;
        }

        private static void dropDownButtons_DropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            DropDownMenuBase.ExitMenuMode();
        }

        private void dropDownButtons_DropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            ToolStripItem ownerItem = ((ToolStripDropDown)sender).OwnerItem;
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(ownerItem == null || ownerItem.Tag == null || clickedItem == null) return;
            int tag = (int)ownerItem.Tag;
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            Keys modifierKeys = ModifierKeys;
            switch(tag) {
                case BII_NAVIGATION_DROPDOWN:
                    MenuItemArguments mia = clickedItem.MenuItemArguments;
                    if(modifierKeys == Keys.Control) {
                        using(IDLWrapper wrapper = new IDLWrapper(mia.Path)) {
                            tabbar.OpenNewWindow(wrapper);
                        }
                    }
                    else {
                        if(modifierKeys == Keys.Shift) tabbar.CloneCurrentTab();  
                        tabbar.NavigateToHistory(mia.Path, mia.IsBack, mia.Index);
                    }
                    return;

                case BII_GROUP:
                    ddmrGroupButton.Close();
                    if(ModifierKeys == (Keys.Control | Keys.Shift)) {
                        Group g = GroupsManager.GetGroup(e.ClickedItem.Text);
                        g.Startup = !g.Startup;
                        GroupsManager.SaveGroups();
                    }
                    else {
                        tabbar.OpenGroup(e.ClickedItem.Text, modifierKeys == Keys.Control);
                    }
                    return;

                case BII_RECENTTAB:
                    using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                        tabbar.OpenNewTabOrWindow(wrapper);
                    }
                    return;

                case BII_APPLICATIONLAUNCHER:
                    if(clickedItem.Target == MenuTarget.File) {
                        AppsManager.Execute(clickedItem.MenuItemArguments.App, clickedItem.MenuItemArguments.ShellBrowser);
                    }
                    return;
            }
        }

        private void dropDownButtons_DropDown_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            DropDownMenuReorderable reorderable = (DropDownMenuReorderable)sender;
            switch(((int)reorderable.OwnerItem.Tag)) {
                case 3:
                    GroupsManager.HandleReorder(reorderable.Items.Cast<ToolStripItem>());
                    break;

                case 5:
                    AppsManager.HandleReorder(reorderable.Items.Cast<ToolStripItem>());
                    break;
            }
            QTTabBarClass.SyncTaskBarMenu();
        }

        private void dropDownButtons_DropDownOpening(object sender, EventArgs e) {
            toolStrip.HideToolTip();
            ToolStripDropDownItem button = (ToolStripDropDownItem)sender;
            button.DropDown.SuspendLayout();
            switch(((int)button.Tag)) {
                case -1:
                    AddHistoryItems(button);
                    break;

                case 3:
                    MenuUtility.CreateGroupItems(button);
                    break;

                case 4:
                    MenuUtility.CreateUndoClosedItems(button);
                    break;

                case 5:
                    AddUserAppItems();
                    break;
            }
            button.DropDown.ResumeLayout();
        }

        public override void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO dbi) {
            if((dbi.dwMask & DBIM.ACTUAL) != (0)) {
                dbi.ptActual.X = Size.Width;
                dbi.ptActual.Y = BarHeight;
            }
            if((dbi.dwMask & DBIM.INTEGRAL) != (0)) {
                dbi.ptIntegral.X = -1;
                dbi.ptIntegral.Y = 10;
            }
            if((dbi.dwMask & DBIM.MAXSIZE) != (0)) {
                dbi.ptMaxSize.X = -1;
                dbi.ptMaxSize.Y = BarHeight;
            }
            if((dbi.dwMask & DBIM.MINSIZE) != (0)) {
                dbi.ptMinSize.X = MinSize.Width;
                dbi.ptMinSize.Y = BarHeight;
            }
            if((dbi.dwMask & DBIM.MODEFLAGS) != (0)) {
                dbi.dwModeFlags = DBIMF.NORMAL;
            }
            if((dbi.dwMask & DBIM.BKCOLOR) != (0)) {
                dbi.dwMask &= ~DBIM.BKCOLOR;
            }
            if((dbi.dwMask & DBIM.TITLE) != (0)) {
                dbi.wszTitle = null;
            }
        }
        // 初始化组件
        private void InitializeComponent() {
            components = new Container();
            toolStrip = new ToolStripClasses();
            toolStrip.SuspendLayout();
            SuspendLayout();
            toolStrip.Dock = DockStyle.Fill;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImeMode = ImeMode.Disable;
            toolStrip.Renderer = new ToolbarRenderer();
            toolStrip.BackColor = Color.Transparent;
            toolStrip.ItemClicked += toolStrip_ItemClicked;
            toolStrip.GotFocus += toolStrip_GotFocus;
            toolStrip.MouseDoubleClick += toolStrip_MouseDoubleClick;
            toolStrip.MouseActivated += toolStrip_MouseActivated;
            toolStrip.PreviewKeyDown += toolStrip_PreviewKeyDown;
            Controls.Add(toolStrip);
            Height = BarHeight;
            MinSize = new Size(20, BarHeight);
            toolStrip.ResumeLayout(false);
            ResumeLayout();
        }
        
        // 加载默认的图片资源
        private static void LoadDefaultImages(bool fWriteReg) {
            imageStrip_Large.TransparentColor = imageStrip_Small.TransparentColor = Color.Empty;
            Bitmap bmp = Resources_Image.ButtonStrip24;
            Bitmap bitmap2 = Resources_Image.ButtonStrip16;
            imageStrip_Large.AddStrip(bmp);
            imageStrip_Small.AddStrip(bitmap2);
            bmp.Dispose();
            bitmap2.Dispose();
            if(fWriteReg) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    key.SetValue("Buttons_ImagePath", string.Empty);
                }
            }
        }
        
       // 通过路径 加载外部图片
        private static bool LoadExternalImage(string path) {
            Bitmap bitmap;
            Bitmap bitmap2;
            if(LoadExternalImage(path, out bitmap, out bitmap2)) {
                imageStrip_Large.AddStrip(bitmap);
                imageStrip_Small.AddStrip(bitmap2);
                bitmap.Dispose();
                bitmap2.Dispose();
                if(Path.GetExtension(path).PathEquals(".bmp")) {
                    imageStrip_Large.TransparentColor = imageStrip_Small.TransparentColor = Color.Magenta;
                }
                else {
                    imageStrip_Large.TransparentColor = imageStrip_Small.TransparentColor = Color.Empty;
                }
                return true;
            }
            return false;
        }

  internal static bool LoadExternalImage(string path, out Bitmap bmpLarge, out Bitmap bmpSmall)
        {
            bmpLarge = (bmpSmall = null);
            if (File.Exists(path))
            {
                try
                {
                    using (Bitmap bitmap = new Bitmap(path))
                    {
                        // if ((bitmap.Width >= 0x1b0) && (bitmap.Height >= 40))
                        /* if ((bitmap.Width >= 0x1b0) && (bitmap.Height >= 0x18))
                         {
                             bmpLarge = bitmap.Clone(new Rectangle(0, 0, 0x1b0, 0x18), PixelFormat.Format32bppArgb);
                             bmpSmall = bitmap.Clone(new Rectangle(0, 0x18, 0x120, 0x10), PixelFormat.Format32bppArgb);
                             return true;
                         }*/

                        if ((bitmap.Width >= 504) && (bitmap.Height >= 24))
                        {
                            bmpLarge = bitmap.Clone(new Rectangle(0, 0, 504, 24), PixelFormat.Format32bppArgb);
                            // bmpSmall = bitmap.Clone(new Rectangle(0, 0x18, 0x120, 0x10), PixelFormat.Format32bppArgb);
                            bmpSmall = (Bitmap)ResizeBitMap(bmpLarge, 336, 16);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex);
                }
            }
            return false;
        }

        internal static Image ResizeBitMap(Bitmap original, int desiredWidth, int desiredHeight)
        {
            //throw error if bouning box is to small
            if (desiredWidth < 4 || desiredHeight < 4)
                throw new InvalidOperationException("Bounding Box of Resize Photo must be larger than 4X4 pixels.");

            //store image widths in variable for easier use
            var oW = (decimal)original.Width;
            var oH = (decimal)original.Height;
            var dW = (decimal)desiredWidth;
            var dH = (decimal)desiredHeight;

            //check if image already fits
            if (oW < dW && oH < dH)
                return original; //image fits in bounding box, keep size (center with css) If we made it bigger it would stretch the image resulting in loss of quality.

            //check for double squares
            if (oW == oH && dW == dH)
            {
                //image and bounding box are square, no need to calculate aspects, just downsize it with the bounding box
                Bitmap square = new Bitmap(original, (int)dW, (int)dH);
                // original.Dispose();
                return square;
            }

            //check original image is square
            if (oW == oH)
            {
                //image is square, bounding box isn't.  Get smallest side of bounding box and resize to a square of that center the image vertically and horizontally with Css there will be space on one side.
                int smallSide = (int)Math.Min(dW, dH);
                Bitmap square = new Bitmap(original, smallSide, smallSide);
                // original.Dispose();
                return square;
            }

            //not dealing with squares, figure out resizing within aspect ratios            
            if (oW > dW && oH > dH) //image is wider and taller than bounding box
            {
                var r = Math.Min(dW, dH) / Math.Min(oW, oH); //two dimensions so figure out which bounding box dimension is the smallest and which original image dimension is the smallest, already know original image is larger than bounding box
                var nH = oH * r; //will downscale the original image by an aspect ratio to fit in the bounding box at the maximum size within aspect ratio.
                var nW = oW * r;
                var resized = new Bitmap(original, (int)nW, (int)nH);
                //  original.Dispose();
                return resized;
            }
            else
            {
                if (oW > dW) //image is wider than bounding box
                {
                    var r = dW / oW; //one dimension (width) so calculate the aspect ratio between the bounding box width and original image width
                    var nW = oW * r; //downscale image by r to fit in the bounding box...
                    var nH = oH * r;
                    var resized = new Bitmap(original, (int)nW, (int)nH);
                    //  original.Dispose();
                    return resized;
                }
                else
                {
                    //original image is taller than bounding box
                    var r = dH / oH;
                    var nH = oH * r;
                    var nW = oW * r;
                    var resized = new Bitmap(original, (int)nW, (int)nH);
                    //  original.Dispose();
                    return resized;
                }
            }
        }

        private static void ManageImageList() {
            if(Config.BBar.ImageStripPath == null) {
                LoadDefaultImages(false);
            }
            else if(Config.BBar.ImageStripPath.Length == 0 || !LoadExternalImage(Config.BBar.ImageStripPath)) {
                LoadDefaultImages(true);
            }
        }

        private static void navBranchRoot_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            InstanceManager.GetThreadTabBar()
                .NavigateBranchCurrent(((QMenuItem)e.ClickedItem).MenuItemArguments.Index);
        }

        protected override void OnExplorerAttached() {
            ExplorerHandle = (IntPtr)Explorer.HWND;
            InstanceManager.RegisterButtonBar(this);
            dropTargetWrapper = new DropTargetWrapper(this);
            QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();

            // If the TabBar and its PluginManager already exist, that means
            // the ButtonBar must have been closed when the Explorer window
            // opened, so we won't get an initialization message.  Do 
            // initialization now.
            if(tabBar != null && tabBar.pluginServer != null) {
                // todo check
                CreateItems();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(VisualStyleRenderer.IsSupported) {
                if(BackgroundRenderer == null) {
                    BackgroundRenderer = new VisualStyleRenderer(VisualStyleElement.Rebar.Band.Normal);
                }
                BackgroundRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                if(ReBarHandle != IntPtr.Zero) {
                    int colorref = (int)PInvoke.SendMessage(ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    using(SolidBrush brush = new SolidBrush(QTUtility2.MakeColor(colorref))) {
                        e.Graphics.FillRectangle(brush, e.ClipRectangle);
                        return;
                    }
                }
                base.OnPaintBackground(e);
            }
        }

        private void pluginButton_ButtonClick(object sender, EventArgs e) {
            ToolStripItem item = (ToolStripItem)sender;
            Plugin plugin;
            string pluginID = Config.BBar.ActivePluginIDs[((int)item.Tag).HiWord() - 1];
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            if(tabbar != null && tabbar.pluginServer.TryGetPlugin(pluginID, out plugin)) {
                try {
                    ((IBarButton)plugin.Instance).OnButtonClick();
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, ExplorerHandle, plugin.PluginInformation.Name, "On button clicked.");
                }
            }
        }

        private void pluginDropDown_DropDownOpening(object sender, EventArgs e) {
            toolStrip.HideToolTip();
            ToolStripDropDownItem item = (ToolStripDropDownItem)sender;
            item.DropDown.SuspendLayout();
            Plugin plugin;
            string pluginID = Config.BBar.ActivePluginIDs[((int)item.Tag).HiWord() - 1];
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            if(tabbar != null && tabbar.pluginServer.TryGetPlugin(pluginID, out plugin)) {
                try {
                    ((IBarDropButton)plugin.Instance).OnDropDownOpening((ToolStripDropDownMenu)item.DropDown);
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, ExplorerHandle, plugin.PluginInformation.Name, "On dropdwon menu is showing.");
                }
            }
            item.DropDown.ResumeLayout();
        }

        private void pluginDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            ToolStripDropDownItem ownerItem = (ToolStripDropDownItem)((DropDownMenuReorderable)sender).OwnerItem;
            Plugin plugin;
            string pluginID = Config.BBar.ActivePluginIDs[((int)ownerItem.Tag).HiWord() - 1];
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            if(tabbar != null && tabbar.pluginServer.TryGetPlugin(pluginID, out plugin)) {
                try {
                    ((IBarDropButton)plugin.Instance).OnDropDownItemClick(e.ClickedItem, MouseButtons.Left);
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, ExplorerHandle, plugin.PluginInformation.Name, "On dropdown menu is clicked.");
                }
            }
        }

        private void pluginDropDown_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            ToolStripDropDownItem ownerItem = (ToolStripDropDownItem)((DropDownMenuReorderable)sender).OwnerItem;
            Plugin plugin;
            string pluginID = Config.BBar.ActivePluginIDs[((int)ownerItem.Tag).HiWord() - 1];
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            if(tabbar != null && tabbar.pluginServer.TryGetPlugin(pluginID, out plugin)) {
                try {
                    ((IBarDropButton)plugin.Instance).OnDropDownItemClick(e.ClickedItem, MouseButtons.Right);
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, ExplorerHandle, plugin.PluginInformation.Name, "On dropdown menu is right clicked.");
                }
            }
        }

        // TODO this doesn't even work.
        private void RearrangeFolderView() {
            IShellView ppshv = null;
            try {
                if(ShellBrowser.GetIShellBrowser().QueryActiveShellView(out ppshv) == 0) {
                    IntPtr ptr;
                    IShellFolderView view2 = (IShellFolderView)ppshv;
                    if((view2.GetArrangeParam(out ptr) == 0) && ((((int)ptr) & 0xffff) != 0)) {
                        fRearranging = true;
                        view2.Rearrange(ptr);
                        view2.Rearrange(ptr);
                        fRearranging = false;
                    }
                }
            }
            catch {
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
            }
        }

        internal void RefreshSearchBox(bool fBrowserRefreshRequired) {
            if(fRearranging) return;
            if(searchBox != null) {
                searchBox.RefreshText();
            }
            if(timerSerachBox_Search != null) {
                timerSerachBox_Search.Stop();
            }
            if(timerSearchBox_Rearrange != null) {
                timerSearchBox_Rearrange.Stop();
            }
            strSearch = string.Empty;
            fSearchBoxInputStart = false;
            iSearchResultCount = -1;
            try {
                foreach(IntPtr ptr in lstPUITEMIDCHILD) {
                    if(ptr != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(ptr);
                    }
                }
            }
            catch {
            }
            lstPUITEMIDCHILD.Clear();
            if(fBrowserRefreshRequired) {
                new WaitTimeoutCallback(QTTabBarClass.WaitTimeout).BeginInvoke(100, AsyncComplete, null);
            }
        }

        [ComRegisterFunction]
        private static void Register(Type t) {
            string name = t.GUID.ToString("B");
            const string str2 = "QTTab Standard Buttons";
            using(RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name)) {
                key.SetValue(null, str2);
                key.SetValue("MenuText", str2);
                key.SetValue("HelpText", str2);
            }
            using(RegistryKey key2 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar")) {
                key2.SetValue(name, "QTButtonBar");
            }
        }

        private void searchBox_ErasingText(object sender, CancelEventArgs e) {
            e.Cancel = lstPUITEMIDCHILD.Count != 0;
        }

        private void searchBox_GotFocus(object sender, EventArgs e) {
            OnGotFocus(e);
        }

        private void searchBox_KeyPress(object sender, KeyPressEventArgs e) {
            if(e.KeyChar == '\r') {
                string text = searchBox.Text;
                if(text.Length > 0) {
                    ShellViewIncrementalSearch(text);
                    e.Handled = true;
                }
            }
            else if(e.KeyChar == '\x001b') {
                searchBox.Text = "";
                QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
                if(tabBar != null) {
                    tabBar.GetListView().SetFocus();
                    searchBox.RefreshText();
                    e.Handled = true;
                }
            }
        }

        private void searchBox_ResizeComplete(object sender, EventArgs e) {
            int width = SearchBoxWidth = searchBox.Width;
            toolStrip.RaiseOnResize();
            InstanceManager.ButtonBarBroadcast(bbar => {
                if(bbar.searchBox != null) {
                    bbar.searchBox.Width = width;
                    bbar.toolStrip.RaiseOnResize();
                }
            }, false);
        }

        private void searchBox_TextChanged(object sender, EventArgs e) {
            timerSerachBox_Search.Stop();
            timerSearchBox_Rearrange.Stop();
            string text = searchBox.Text;
            if(!text.StartsWith("/") || ((text.Length >= 3) && text.EndsWith("/"))) {
                fSearchBoxInputStart = true;
                strSearch = text;
                iSearchResultCount = -1;
                // TODO: If the item count is less than a certain cutoff, skip the timer and just call it directly.
                timerSerachBox_Search.Start();
            }
        }

        private static int SearchBoxWidth {
            get {
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root)) {
                    return key == null 
                            ? 100
                            : Math.Max(Math.Min((int)key.GetValue("SearchBoxWidth", 100), 1024), 32);
                }
            }
            set {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    key.SetValue("SearchBoxWidth", value);
                }
            }
        }

        // TODO clean
        private bool ShellViewIncrementalSearch(string str) {
            var listView = InstanceManager.GetThreadTabBar().GetListView();
            listView.HideSubDirTip(9);
            listView.HideThumbnailTooltip(9);

            IShellView ppshv = null;
            IShellFolder shellFolder = null;
            IntPtr zero = IntPtr.Zero;
            bool addedItems = false;
            try {
                if(ShellBrowser.GetIShellBrowser().QueryActiveShellView(out ppshv) == 0) {
                    int num;
                    IFolderView view2 = ppshv as IFolderView;
                    IShellFolderView view3 = ppshv as IShellFolderView;
                    if(view2 == null || view3 == null) {
                        return false;
                    }
                    IPersistFolder2 ppv = null;
                    try {
                        Guid riid = ExplorerGUIDs.IID_IPersistFolder2;
                        if(view2.GetFolder(ref riid, out ppv) == 0) {
                            ppv.GetCurFolder(out zero);
                        }
                    }
                    finally {
                        if(ppv != null) {
                            Marshal.ReleaseComObject(ppv);
                        }
                    }
                    if(zero == IntPtr.Zero) {
                        QTUtility2.MakeErrorLog(null, "failed current pidl");
                        return false;
                    }
                    view2.ItemCount(SVGIO.ALLVIEW, out num);
                    AbstractListView lvw = InstanceManager.GetThreadTabBar().GetListView();
                    lvw.SetRedraw(false);
                    try {
                        Regex regex;
                        QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
                        if(str.StartsWith("/") && str.EndsWith("/")) {
                            try {
                                regex = new Regex(str.Substring(1, str.Length - 2), RegexOptions.IgnoreCase);
                            }
                            catch {
                                SystemSounds.Asterisk.Play();
                                return false;
                            }
                        }
                        else if(tabbar == null || tabbar.pluginServer.FilterPlugin == null || !tabbar.pluginServer.FilterPlugin.QueryRegex(str, out regex) || regex == null) {
                            string input = Regex.Escape(str);
                            input = reAsterisc.Replace(input, ".*");
                            regex = new Regex(reQuestion.Replace(input, "."), RegexOptions.IgnoreCase);
                        }
                        int num2 = num;
                        if(!ShellMethods.GetShellFolder(zero, out shellFolder)) {
                            return false;
                        }
                        bool useFC = tabbar != null && tabbar.pluginServer.FilterCorePlugin != null;
                        IFilterCore iFilterCore = null;
                        QTPlugin.Interop.IShellFolder folder3 = null;
                        if(useFC) {
                            iFilterCore = tabbar.pluginServer.FilterCorePlugin;
                            folder3 = (QTPlugin.Interop.IShellFolder)shellFolder;
                        }

                        if(!useFC && (regex.ToString().Length == 0 || regex.ToString() == ".*")) {
                            addedItems = lstPUITEMIDCHILD.Count > 0;
                            foreach(IntPtr pIDLChild in lstPUITEMIDCHILD) {
                                int num7;
                                view3.AddObject(pIDLChild, out num7);
                                PInvoke.CoTaskMemFree(pIDLChild);
                            }
                            lstPUITEMIDCHILD.Clear();
                        }
                        else {
                            List<IntPtr> collection = new List<IntPtr>();
                            for(int i = 0; i < num2; i++) {
                                IntPtr ptr4;
                                if(view2.Item(i, out ptr4) != 0) continue;
                                if((useFC && iFilterCore.IsMatch(folder3, ptr4, regex)) || (!useFC && CheckDisplayName(shellFolder, ptr4, regex))) {
                                    PInvoke.CoTaskMemFree(ptr4);
                                }
                                else {
                                    int num4;
                                    collection.Add(ptr4);
                                    if(view3.RemoveObject(ptr4, out num4) == 0) {
                                        num2--;
                                        i--;
                                    }
                                }
                            }
                            int count = lstPUITEMIDCHILD.Count;
                            for(int j = 0; j < count; j++) {
                                IntPtr pIDLChild = lstPUITEMIDCHILD[j];
                                if((!useFC || !iFilterCore.IsMatch(folder3, pIDLChild, regex)) && (useFC || !CheckDisplayName(shellFolder, pIDLChild, regex))) continue;
                                int num7;
                                lstPUITEMIDCHILD.RemoveAt(j);
                                count--;
                                j--;
                                view3.AddObject(pIDLChild, out num7);
                                PInvoke.CoTaskMemFree(pIDLChild);
                                addedItems = true;
                            }
                            lstPUITEMIDCHILD.AddRange(collection);
                        }
                        view2.ItemCount(SVGIO.ALLVIEW, out iSearchResultCount);
                    }
                    finally {
                        lvw.SetRedraw(true);
                    }
                    ShellBrowser.SetStatusText(string.Concat(
                            iSearchResultCount,
                            " / ", 
                            iSearchResultCount + lstPUITEMIDCHILD.Count,
                            QTUtility.TextResourcesDic["ButtonBar_Misc"][5]
                    ));
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
                addedItems = false;
            }
            finally {
                if(ppshv != null) {
                    Marshal.ReleaseComObject(ppshv);
                }
                if((shellFolder != null) && (Marshal.ReleaseComObject(shellFolder) != 0)) {
                    QTUtility2.MakeErrorLog(null, "shellfolder is not released.");
                }
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
            }
            return addedItems;
        }

        protected override bool ShouldHaveBreak() {
            bool breakBar = true;
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    breakBar = ((int)key.GetValue("BreakButtonBar", 1) == 1);
                }
            }
            return breakBar;
        }

        public override void ShowDW(bool fShow) {
            base.ShowDW(fShow);
            if(!fShow) {
                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    key.SetValue("BreakButtonBar", BandHasBreak() ? 1 : 0);
                }
            }
        }

        private void timerSearchBox_Rearrange_Tick(object sender, EventArgs e) {
            if(!fSearchBoxInputStart) {
                timerSearchBox_Rearrange.Stop();
                RearrangeFolderView();
            }
        }

        private void timerSerachBox_Search_Tick(object sender, EventArgs e) {
            timerSerachBox_Search.Stop();
            bool flag = ShellViewIncrementalSearch(strSearch);
            fSearchBoxInputStart = false;
            if(flag) {
                //timerSearchBox_Rearrange.Start();
            }
        }

        private void toolStrip_GotFocus(object sender, EventArgs e) {
            if(IsHandleCreated) {
                OnGotFocus(e);
            }
        }

        private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem == null || e.ClickedItem.Tag == null) return;
            InstanceManager.GetThreadTabBar().ProcessButtonBarClick((int)e.ClickedItem.Tag);
        }

        private void toolStrip_MouseActivated(object sender, EventArgs e) {
            ActivatedByClickOnThis();
        }

        private void toolStrip_MouseDoubleClick(object sender, MouseEventArgs e) {
            if(toolStrip.GetItemAt(e.Location) == null) {
                InstanceManager.GetThreadTabBar().OnMouseDoubleClick();
            }
        }

        private void toolStrip_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Left:
                case Keys.Right:
                case Keys.F6:
                case Keys.Tab:
                    e.IsInputKey = true;
                    DropDownMenuBase.ExitMenuMode();
                    break;

                case Keys.Up:
                    break;

                default:
                    return;
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e) {
            int num = ((ToolStripTrackBar)sender).Value;
            PInvoke.SetWindowLongPtr(ExplorerHandle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0x80000));
            PInvoke.SetLayeredWindowAttributes(ExplorerHandle, 0, (byte)num, 2);
            Explorer.StatusText = (((num * 100) / 0xff)) + "%";
        }

        public override int TranslateAcceleratorIO(ref MSG msg) {
            if(msg.message == WM.KEYDOWN) {
                Keys wParam = (Keys)((int)((long)msg.wParam));
                switch(wParam) {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Tab:
                    case Keys.F6: {
                            switch(wParam) {
                                case Keys.Right:
                                case Keys.Left:
                                    if(toolStrip.Items.OfType<ToolStripControlHost>()
                                            .Any(item => item.Visible && item.Enabled && item.Selected)) {
                                        return 1;
                                    }
                                    break;
                            }
                            bool flag = (ModifierKeys == Keys.Shift) || (wParam == Keys.Left);
                            if(flag && toolStrip.OverflowButton.Selected) {
                                for(int j = toolStrip.Items.Count - 1; j > -1; j--) {
                                    ToolStripItem item2 = toolStrip.Items[j];
                                    if(item2.Visible && item2.Enabled) {
                                        item2.Select();
                                        return 0;
                                    }
                                }
                            }
                            for(int i = 0; i < toolStrip.Items.Count; i++) {
                                if(toolStrip.Items[i].Selected) {
                                    ToolStripItem start = toolStrip.Items[i];
                                    if(start is ToolStripControlHost) {
                                        toolStrip.Select();
                                    }
                                    while((start = toolStrip.GetNextItem(start, flag ? ArrowDirection.Left : ArrowDirection.Right)) != null) {
                                        int index = toolStrip.Items.IndexOf(start);
                                        if(flag) {
                                            if((index > i) || (start is ToolStripOverflowButton)) {
                                                return 1;
                                            }
                                        }
                                        else if(index < i) {
                                            if(toolStrip.OverflowButton.Visible) {
                                                toolStrip.OverflowButton.Select();
                                                return 0;
                                            }
                                            return 1;
                                        }
                                        ToolStripControlHost host = start as ToolStripControlHost;
                                        if(host != null) {
                                            host.Control.Select();
                                            return 0;
                                        }
                                        if(start.Enabled) {
                                            start.Select();
                                            return 0;
                                        }
                                    }
                                    return 1;
                                }
                            }
                            break;
                        }
                    case Keys.Down:
                    case Keys.Space:
                    case Keys.Return:
                        if(toolStrip.OverflowButton.Selected) {
                            toolStrip.OverflowButton.ShowDropDown();
                            return 0;
                        }
                        for(int k = 0; k < toolStrip.Items.Count; k++) {
                            if(toolStrip.Items[k].Selected) {
                                ToolStripItem item4 = toolStrip.Items[k];
                                if(item4 is ToolStripDropDownItem) {
                                    ((ToolStripDropDownItem)item4).ShowDropDown();
                                }
                                else {
                                    if((item4 is ToolStripSearchBox) && ((wParam == Keys.Return) || (wParam == Keys.Space))) {
                                        return 1;
                                    }
                                    if(wParam != Keys.Down) {
                                        item4.PerformClick();
                                    }
                                }
                                return 0;
                            }
                        }
                        break;

                    case Keys.Back:
                        if(toolStrip.Items.OfType<ToolStripControlHost>().Any(item5 => item5.Selected)) {
                            PInvoke.SendMessage(msg.hwnd, WM.CHAR, msg.wParam, msg.lParam);
                            return 0;
                        }
                        break;

                    case Keys.A:
                    case Keys.C:
                    case Keys.V:
                    case Keys.X:
                    case Keys.Z:
                        if(((ModifierKeys == Keys.Control) && (searchBox != null)) && searchBox.Selected) {
                            PInvoke.TranslateMessage(ref msg);
                            if(wParam == Keys.A) {
                                searchBox.TextBox.SelectAll();
                            }
                            return 0;
                        }
                        break;

                    case Keys.Delete:
                        if(toolStrip.Items.OfType<ToolStripControlHost>().Any(item6 => item6.Selected)) {
                            PInvoke.SendMessage(msg.hwnd, WM.KEYDOWN, msg.wParam, msg.lParam);
                            return 0;
                        }
                        break;
                }
            }
            return 1;
        }

        public override void UIActivateIO(int fActivate, ref MSG Msg) {
            if(fActivate != 0) {
                toolStrip.Focus();
                if(toolStrip.Items.Count != 0) {
                    ToolStripItem item;
                    if(ModifierKeys != Keys.Shift) {
                        for(int i = 0; i < toolStrip.Items.Count; i++) {
                            item = toolStrip.Items[i];
                            if((item.Enabled && item.Visible) && !(item is ToolStripSeparator)) {
                                var tsch = item as ToolStripControlHost;
                                if(tsch != null) {
                                    tsch.Control.Select();
                                    return;
                                }
                                item.Select();
                                return;
                            }
                        }
                    }
                    else if(toolStrip.OverflowButton.Visible) {
                        toolStrip.OverflowButton.Select();
                    }
                    else {
                        for(int j = toolStrip.Items.Count - 1; j > -1; j--) {
                            item = toolStrip.Items[j];
                            if((item.Enabled && item.Visible) && !(item is ToolStripSeparator)) {
                                ToolStripControlHost tsch = item as ToolStripControlHost;
                                if(tsch != null) {
                                    tsch.Control.Select();
                                    return;
                                }
                                item.Select();
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void UnloadPluginsOnCreation() {
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            if(tabbar == null) return;
            foreach(Plugin plugin in tabbar.pluginServer.Plugins) {
                PluginType pluginType = plugin.PluginInformation.PluginType;
                string pluginID = plugin.PluginInformation.PluginID;
                if(pluginType == PluginType.Interactive) {
                    if(!Config.BBar.ActivePluginIDs.Contains(pluginID)) {
                        tabbar.pluginServer.UnloadPluginInstance(pluginID, EndCode.Unloaded);
                    }
                }
                else if((pluginType == PluginType.Background || pluginType == PluginType.BackgroundMultiple)
                        && plugin.BackgroundButtonEnabled && !Config.BBar.ActivePluginIDs.Contains(pluginID)) {
                    try {
                        if(plugin.Instance != null) {
                            plugin.Instance.Close(EndCode.Hidden);
                        }
                    }
                    catch(Exception exception) {
                        PluginManager.HandlePluginException(exception, ExplorerHandle, plugin.PluginInformation.Name, "Closing plugin button. (EndCode.Hidden)");
                    }
                    plugin.BackgroundButtonEnabled = false;
                }
            }
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t) {
            string name = t.GUID.ToString("B");
            try {
                using(RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Toolbar")) {
                    key.DeleteValue(name, false);
                }
            }
            catch {
            }
            try {
                using(RegistryKey key2 = Registry.ClassesRoot.CreateSubKey("CLSID")) {
                    key2.DeleteSubKeyTree(name);
                }
            }
            catch {
            }
        }

        private void userAppsSubDir_DoubleClicked(object sender, EventArgs e) {
            ddmrUserAppButton.Close();
            using(IDLWrapper wrapper = new IDLWrapper(((QMenuItem)sender).Path)) {
                InstanceManager.GetThreadTabBar().OpenNewTabOrWindow(wrapper);
            }
        }

        internal void FocusSearchBox() {
            if(searchBox != null) {
                searchBox.TextBox.Focus();
            }
        }

        internal void RefreshButtons() {
            if(NavDropDown != null && NavDropDown.Visible) {
                NavDropDown.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            if(ddmrGroupButton != null && ddmrGroupButton.Visible) {
                ddmrGroupButton.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            if(ddmrRecentlyClosed != null && ddmrRecentlyClosed.Visible) {
                ddmrRecentlyClosed.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            if(ddmrUserAppButton != null && ddmrUserAppButton.Visible) {
                ddmrUserAppButton.Close(ToolStripDropDownCloseReason.AppClicked);
            }
            QTTabBarClass tabbar = InstanceManager.GetThreadTabBar();
            int index = 0;
            int count = 0;
            // 判断tabbar不为空
            if (null != tabbar && !tabbar.IsDisposed) {
                index = tabbar.SelectedTabIndex;
                count = tabbar.TabCount;
            }
            // 判断 toolStrip  Items不为空
            if ( null != toolStrip && toolStrip.Items != null && toolStrip.Items.Count > 0 )
            foreach(ToolStripItem item in toolStrip.Items) {
                if (item == null) continue;
                if(item.Tag == null) continue;
                switch((int)item.Tag) {
                    case BII_NAVIGATION_BACK:
                        item.Enabled = tabbar.CanNavigateBackward;
                        break;
                    case BII_NAVIGATION_FWRD:
                        item.Enabled = tabbar.CanNavigateForward;
                        break;
                    case BII_NAVIGATION_DROPDOWN:
                        item.Enabled = tabbar.CanNavigateBackward || tabbar.CanNavigateForward;
                        break;
                    case BII_CLOSE_LEFT:
                        item.Enabled = index > 0;
                        break;
                    case BII_CLOSE_RIGHT:
                        item.Enabled = index < count - 1;
                        break;
                    case BII_CLOSE_ALLBUTCURRENT:
                    case BII_CLOSE_CURRENT:
                        item.Enabled = count > 1;
                        break;
                    case BII_GROUP:
                        item.Enabled = GroupsManager.GroupCount > 0;
                        break;
                    case BII_APPLICATIONLAUNCHER:
                        item.Enabled = AppsManager.UserApps.Any();
                        break;
                    case BII_RECENTTAB:
                        item.Enabled = StaticReg.ClosedTabHistoryList.Count > 0;
                        break;
                    // todo: recent files
                    case BII_TOPMOST:
                        // todo: simplify this, and make CreateItems set this value correctly too.
                        ((ToolStripButton)item).Checked = PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 8) == new IntPtr(8); // todo
                        break;
                }
            }
        }

        internal void RefreshStatusText() {
            if(iSearchResultCount <= 0) return;
            int newCount = ShellBrowser.GetItemCount();
            if(newCount >= iSearchResultCount) return;
            iSearchResultCount = newCount;
            ShellBrowser.SetStatusText(string.Concat(
                    iSearchResultCount,
                    " / ",
                    iSearchResultCount + lstPUITEMIDCHILD.Count,
                    QTUtility.TextResourcesDic["ButtonBar_Misc"][5]));
        }

        internal bool SetSearchBarText(string text) {
            if(searchBox == null) return false;
            searchBox.Focus();
            searchBox.Text = text ?? "";
            return true;
        }

        internal bool UpdatePluginItem(string pid, IBarButton instance, bool fEnabled, bool fRefreshImage) {
            int p = Array.IndexOf(Config.BBar.ActivePluginIDs, pid);
            if(p == -1) return false;
            p = (p + 1) << 16;
            ToolStripItem item = toolStrip.Items.Cast<ToolStripItem>().FirstOrDefault(tsi => (int)tsi.Tag == p);
            if(item == null) return false;
            item.Enabled = fEnabled;
            try {
                item.ToolTipText = instance.Text;
                if(fRefreshImage) {
                    item.Image = instance.GetImage(Config.BBar.LargeButtons);
                }
            }
            catch(Exception exception) {
                PluginManager.HandlePluginException(exception, ExplorerHandle, pid, "Refreshing plugin image and text.");
            }
            return true;
        }

        protected override void WndProc(ref Message m) {
            switch(m.Msg) {
                case WM.INITMENUPOPUP:
                case WM.DRAWITEM:
                case WM.MEASUREITEM:
                    if(m.HWnd == Handle && shellContextMenu.TryHandleMenuMsg(m.Msg, m.WParam, m.LParam)) {
                        return;
                    }
                    break;

                case WM.DROPFILES:
                    PInvoke.SendMessage(InstanceManager.GetThreadTabBar().Handle, 0x233, m.WParam, IntPtr.Zero);
                    return;

                case WM.APP:
                    m.Result = toolStrip.IsHandleCreated ? toolStrip.Handle : IntPtr.Zero;
                    return;

                case WM.CONTEXTMENU:
                    if(     (ddmrGroupButton == null || !ddmrGroupButton.Visible) &&
                            (ddmrUserAppButton == null || !ddmrUserAppButton.Visible) && 
                            (ddmrRecentlyClosed == null || !ddmrRecentlyClosed.Visible)) {
                        InstanceManager.GetThreadTabBar().ShowContextMenu(false);
                    }
                    return;
            }
            base.WndProc(ref m);
        }

        private ShellBrowserEx ShellBrowser {
            get {
                if(shellBrowser == null) {
                    QTTabBarClass tabBar = InstanceManager.GetThreadTabBar();
                    if(tabBar != null) {
                        shellBrowser = tabBar.GetShellBrowser();
                    }
                }
                return shellBrowser;
            }
        }
    }

    internal sealed class ImageStrip : IDisposable {
        private List<Bitmap> lstImages = new List<Bitmap>();
        private Size size;
        private Color transparentColor;

        public ImageStrip(Size size) {
            this.size = size;
        }

        public int LstImagesLength() {
            return lstImages.ToArray().Length;
        }

        public void AddStrip(Bitmap bmp) {
            int width = bmp.Width;
            int num2 = 0;
            bool flag = transparentColor != Color.Empty;
            if(((width % size.Width) != 0) || (bmp.Height != size.Height)) {
                throw new ArgumentException("size invalid.");
            }
            Rectangle rect = new Rectangle(Point.Empty, size);
            while((width - size.Width) > -1) {
                Bitmap image = bmp.Clone(rect, PixelFormat.Format32bppArgb);
                if(flag) {
                    image.MakeTransparent(transparentColor);
                }
                if((lstImages.Count > num2) && (lstImages[num2] != null)) {
                    using(Graphics graphics = Graphics.FromImage(lstImages[num2])) {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(image, 0, 0);
                        image.Dispose();
                        goto Label_00E4;
                    }
                }
                lstImages.Add(image);
            Label_00E4:
                num2++;
                width -= size.Width;
                rect.X += size.Width;
            }
        }

        public void Dispose() {
            foreach(Bitmap bitmap in lstImages) {
                if(bitmap != null) {
                    bitmap.Dispose();
                }
            }
            lstImages.Clear();
        }

        public Bitmap this[int index] {
            get {
                return lstImages[index];
            }
        }

        public Color TransparentColor {
            set {
                transparentColor = value;
            }
        }
    }
}

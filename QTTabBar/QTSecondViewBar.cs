//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2023  indiff
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Common;
using QTTabBarLib.ExplorerBrowser;
using QTTabBarLib.Interop;
using IShellFolder = QTTabBarLib.Interop.IShellFolder;

namespace QTTabBarLib
{
    [ComVisible(true), Guid("d2bf470e-ed1c-487f-a888-2bd8835eb6ce")]
    // public sealed class QTSecondViewBar : TabBarBase
    public sealed class QTSecondViewBar : TabBarBase
    {
        internal ExplorerManager explorerManager;
        private Panel viewContainer;
        private Panel controlContainer;
        private SplitContainer addressBarContainer;
        private SplitContainer splitContainer;
        private BreadcrumbBar breadCrumbs;
        // private FilterBox filterBox;
        private IContainer components;
        internal ContextMenuStripEx contextMenuTab;

        // private QTabItem CurrentTab;
        // private BreadcrumbsAddressBar breadCrumbs;
        // private int BandHeight;

        private const int VIEW_MIN_HEIGHT = 64;
        private bool fNowResizing;
        private int prefSize;
        private bool UserResizing;
        // private IntPtr ExplorerHandle;
        // private AbstractListView listView = new AbstractListView();
        private ShellContextMenu shellContextMenu = new ShellContextMenu();
        // private ListViewMonitor listViewManager;
        // public override bool HostedByThirdViewBar => true;

        private ExplorerBrowser.WindowsForms.ExplorerBrowser explorerBrowser;

        public ShellObject CurrentLocation
        {
            get
            {
                return CurrentLocation;
            }

            set
            {
                CurrentLocation = value;
            }
        }


        public QTSecondViewBar()
        {
            try
            {
                Application.EnableVisualStyles();
                BandHeight = 500;
                this.InitializeComponent();
                // this.tabManager = (TabManagerBase) new QSecondViewBar.TabManagerSecond(this);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex);
            }
        }

        private void OpenDefaultLocation()
        {
            try
            {
                using (IDLWrapper wrapper = new IDLWrapper(Config.Window.DefaultLocation))
                {
                    QTUtility2.log("tabControl1_PlusButtonClicked others default " + Config.Window.DefaultLocation);
                    if (ShellBrowser.Navigate(wrapper) != 0)
                    {
                        listView.SetFocus();
                    }
                }
            }
            catch
            {
            }
        }

        private void InitializeComponent()
        {
            this.RightToLeft = RightToLeft.Yes;
            this.viewContainer = new Panel();
            this.splitContainer = new SplitContainer();
            this.controlContainer = new Panel();
            this.addressBarContainer = new SplitContainer();
            this.components = (IContainer)new Container();


            // contextMenuTab = new ContextMenuStripEx(components, false);
            // contextMenuSys = new ContextMenuStripEx(components, false);

            // 
            // viewContainer
            // 
            // 使用 Anchor 属性可以定义在调整控件的父控件大小时如何自动调整控件的大小。将控件锚定到其父控件后，可确保当调整父控件的大小时锚定的边缘与父控件的边缘的相对位置保持不变。
            this.viewContainer.Anchor = AnchorStyles.Top |
                                        AnchorStyles.Bottom |
                                        AnchorStyles.Left |
                                        AnchorStyles.Right
                ;
            this.viewContainer.BackColor = Color.Transparent;
            // this.viewContainer.BackColor = Color.Beige;
            this.viewContainer.Dock = DockStyle.Fill;
            this.viewContainer.Location = new Point(0, 0);
            this.viewContainer.Size = new Size(100, 256);
            this.viewContainer.Name = "viewContainer";
            this.viewContainer.TabIndex = 1;
            this.viewContainer.Padding = new Padding(0, 0, 0, 2);
            this.viewContainer.Margin = Padding.Empty;
            this.viewContainer.TabIndex = 0;
            // this.viewContainer.Text = "view container";

            /*if (CurrentLocation == null)
            {
                CurrentLocation = (ShellObject)KnownFolders.Computer;
                QTUtility2.log("CurrentLocation init");
            }*/

            explorerBrowser = new ExplorerBrowser.WindowsForms.ExplorerBrowser();
            explorerBrowser.Dock = DockStyle.Fill;
            // explorerBrowser.NavigationOptions.PaneVisibility.Navigation = PaneVisibilityState.Show;
            explorerBrowser.NavigationOptions.PaneVisibility.Navigation = PaneVisibilityState.Hide;
            explorerBrowser.NavigationOptions.PaneVisibility.Commands = PaneVisibilityState.Hide;
            explorerBrowser.NavigationOptions.PaneVisibility.AdvancedQuery = PaneVisibilityState.Hide;
            explorerBrowser.NavigationOptions.PaneVisibility.CommandsOrganize = PaneVisibilityState.Hide;
            explorerBrowser.NavigationOptions.PaneVisibility.CommandsView = PaneVisibilityState.Hide;
            explorerBrowser.NavigationOptions.PaneVisibility.Preview = PaneVisibilityState.Hide;
            explorerBrowser.NavigationOptions.PaneVisibility.Query = PaneVisibilityState.Hide;

            viewContainer.Controls.Add(explorerBrowser);

            this.tabControl1 = new QTabControl();
            tabControl1.SuspendLayout();
            tabControl1.SetRedraw(false);
            using (IDLWrapper wrapper = new IDLWrapper(Config.Window.DefaultLocation))
            {
                CurrentTab = new QTabItem(QTUtility2.MakePathDisplayText(wrapper.Path, false), 
                    wrapper.Path,
                    tabControl1);
                // tab.NavigatedTo(wrapper.Path, wrapper.IDL, -1, false);
                CurrentTab.ToolTipText = QTUtility2.MakePathDisplayText(wrapper.Path, true);
            }

            // CurrentTab = new QTabItem(string.Empty, string.Empty, tabControl1);
            tabControl1.TabPages.Add(CurrentTab);
            tabControl1.Dock = DockStyle.Fill;
            // tabControl1.ContextMenuStrip = contextMenuTab;
            tabControl1.RefreshOptions(true);
            tabControl1.Selecting += tabControl1_Selecting;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // tabControl1.RowCountChanged += tabControl1_RowCountChanged;
            // tabControl1.Deselecting += tabControl1_Deselecting;
          
           
            // tabControl1.GotFocus += Controls_GotFocus;
            // tabControl1.MouseEnter += tabControl1_MouseEnter;
            // tabControl1.MouseLeave += tabControl1_MouseLeave;
            // tabControl1.MouseDown += tabControl1_MouseDown;
            // tabControl1.MouseUp += tabControl1_MouseUp;
            // tabControl1.MouseMove += tabControl1_MouseMove;
            // tabControl1.MouseDoubleClick += tabControl1_MouseDoubleClick;
            // tabControl1.ItemDrag += tabControl1_ItemDrag;
            // tabControl1.PointedTabChanged += tabControl1_PointedTabChanged;
            // tabControl1.TabCountChanged += tabControl1_TabCountChanged;
            // tabControl1.CloseButtonClicked += tabControl1_CloseButtonClicked;
            // tabControl1.TabIconMouseDown += tabControl1_TabIconMouseDown;
            // // 注册蓝色新增按钮的点击事件
            // tabControl1.PlusButtonClicked += tabControl1_PlusButtonClicked;


            // contextMenuTab.Items.Add(new ToolStripMenuItem());
            // contextMenuTab.ShowImageMargin = false;
            // contextMenuSys.Items.Add(new ToolStripMenuItem());
            // contextMenuSys.ShowImageMargin = false;


            // contextMenuTab.ItemClicked += contextMenuTab_ItemClicked;
            // contextMenuTab.Opening += contextMenuTab_Opening;
            // contextMenuTab.Closed += contextMenuTab_Closed;
            //
            // contextMenuSys.ItemClicked += contextMenuSys_ItemClicked;
            // contextMenuSys.Opening += contextMenuSys_Opening;

            this.tabControl1.Text = "QTTabBar TabControl";

            // tabControl1.RowCountChanged += tabControl1_RowCountChanged;
            // tabControl1.Deselecting += tabControl1_Deselecting;
            // tabControl1.Selecting += tabControl1_Selecting;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // tabControl1.GotFocus += Controls_GotFocus;
            // tabControl1.MouseEnter += tabControl1_MouseEnter;
            // tabControl1.MouseLeave += tabControl1_MouseLeave;
            // tabControl1.MouseDown += tabControl1_MouseDown;
            // tabControl1.MouseUp += tabControl1_MouseUp;
            // tabControl1.MouseMove += tabControl1_MouseMove;
            // tabControl1.MouseDoubleClick += tabControl1_MouseDoubleClick;
            // tabControl1.ItemDrag += tabControl1_ItemDrag;
            // tabControl1.PointedTabChanged += tabControl1_PointedTabChanged;
            // tabControl1.TabCountChanged += tabControl1_TabCountChanged;
            // tabControl1.CloseButtonClicked += tabControl1_CloseButtonClicked;
            // tabControl1.TabIconMouseDown += tabControl1_TabIconMouseDown;
            // // 注册蓝色新增按钮的点击事件
            tabControl1.PlusButtonClicked += tabControl1_PlusButtonClicked;

            
            // 标签组件容器
            this.controlContainer.BackColor = System.Drawing.Color.Transparent;
            // this.controlContainer.Dock = DockStyle.Bottom;
            // this.controlContainer.Dock = DockStyle.Top;
            this.controlContainer.Dock = DockStyle.Left;
            this.controlContainer.Location = new Point(0, 0);
            // this.controlContainer.Location = new Point(20, 100);
            // this.controlContainer.Location = new Point(0, 256);
            // this.controlContainer.Size = new Size(256, CalcBandHeight(1));
            this.controlContainer.Size = new Size(100, Config.Skin.TabHeight );
            this.controlContainer.Margin = Padding.Empty;
            // this.controlContainer.Padding = Padding.Empty;
            this.controlContainer.Padding = new Padding(0, 2, 5, 0);
            // this.controlContainer.Padding = this.ControlContainerPadding;
            this.controlContainer.TabIndex = 1;
            this.controlContainer.Text = "ControlContainer";
            this.controlContainer.Visible = true;

            // 添加标签栏
            this.controlContainer.Controls.Add(this.tabControl1);

            // 获取或设置控件绑定到的容器的边缘并确定控件如何随其父级一起调整大小。（即指控件挂靠的方向）
            // this.controlContainer.Anchor = AnchorStyles.Top |
            //                                AnchorStyles.Bottom |
            //                                AnchorStyles.Left |
            //                                AnchorStyles.Right;

                // !this.GetObjectForExtraViewBar<bool>(Config.Bool(Scts.ExtraViewNoTab2nd), Config.Bool(Scts.ExtraViewNoTab3rd));

            // explorerBrowser.NavigationPending += new EventHandler<NavigationPendingEventArgs>(explorerBrowser_NavigationPending);
            // explorerBrowser.NavigationComplete += explorerBrowser_NavigationComplete;
            // explorerBrowser.NavigationFailed +=  new EventHandler<NavigationFailedEventArgs>(explorerBrowser_NavigationFailed);

            this.splitContainer.BackColor = Color.Transparent;
            // this.splitContainer.BackColor = this.IsVertical ? this.VerticalExplorerBarBackgroundColor : System.Drawing.Color.Transparent;
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.FixedPanel = FixedPanel.Panel1;
            this.splitContainer.Location = new Point(0, 0);
            this.splitContainer.Margin = Padding.Empty;
            // this.splitContainer.Panel1.Padding = new Padding(2, 0, 0, 0);

            this.splitContainer.Panel1Collapsed = true;
            this.splitContainer.Panel1.Padding = Padding.Empty;
                // new Padding(0, Graphic.SelectValueByScaling<int>(ExplorerManager.WindowScaling, 16, 11, 6), 0, 0);

            // 添加 视图容器、标签容器
            // this.splitContainer.Panel2.BackColor = Color.Transparent;
            this.splitContainer.Panel2.Controls.Add((Control)this.viewContainer);
            this.splitContainer.Panel2.Controls.Add((Control)this.controlContainer);
            // this.splitContainer.Panel2.Padding = new Padding(2, 0, 0, 0);
            this.splitContainer.Panel2.Padding = Padding.Empty;
                // this.IsVertical ? Graphic.Translate(new Padding(2, 0, 0, 0)) : Padding.Empty;
            this.splitContainer.Size = new Size(256, 256);
            this.splitContainer.Margin = Padding.Empty;
            // this.splitContainer.SplitterDistance = 48;
            // this.splitContainer.SplitterDistance = 10;
            
            // this.splitContainer.SplitterWidth = 6;
            // this.splitContainer.SplitterWidth = Graphic.ScaleBy(ExplorerManager.WindowScaling, 6);
            // this.splitContainer.SplitterMoving += new SplitterCancelEventHandler(this.splitContainer_SplitterMoving);
            // this.splitContainer.SplitterMoved += new SplitterEventHandler(this.splitContainer_SplitterMoved);
            this.splitContainer.TabIndex = 0;
            
            this.Controls.Add((Control)this.splitContainer);
            this.MinSize = new Size(16, 64);
            this.MaxSize = new Size(this.MaxSize.Width, -1);
            this.Margin = this.Padding = Padding.Empty;
            this.Font = Graphic.CreateDefaultFont();

            contextMenuTab = new ContextMenuStripEx(components, false);
            contextMenuSys = new ContextMenuStripEx(components, false);
            tabControl1.SuspendLayout();
            contextMenuSys.SuspendLayout();
            contextMenuTab.SuspendLayout();
            SuspendLayout();

            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            this.splitContainer.ResumeLayout(false);
            this.viewContainer.ResumeLayout(false);
            this.controlContainer.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        public Padding ControlContainerPadding
        {
            get
            {
                int left = QTUtility.LaterThan7 ? 1 : 0;
                // Config.Get(Scts.ExtraViewTabBarPos3rd) == 1
                return this.IsVertical && true ?
                    (!QTUtility.RightToLeft ? new Padding(left, 2, 5, 0) :
                        new Padding(5, 2, 0, 0)) 
                    :
                    (!QTUtility.RightToLeft ? new Padding(left, 1, 5, 0) : 
                        new Padding(5, 1, 0, 0));

            }
        }

        /*protected  int CalcBandHeight(int count)
        {
            return count * 
                (Graphic.ScaleBy(ExplorerManager.WindowScaling, Config.Skin.TabHeight) - 3) + 
                3 + 
                this.ControlContainerPadding.Vertical;
        }*/

   
        protected void InitializeContextMenus()
        {
            this.contextMenuTab = new ContextMenuStripEx(this.components, true);
            // this.contextMenuTab.ImageList = ;
            this.contextMenuTab.Items.Add((ToolStripItem)new ToolStripMenuItem("测试"));
            /*this.contextMenuTab.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuTab_ItemClicked);
            this.contextMenuTab.Opening += new CancelEventHandler(this.contextMenuTab_Opening);
            this.contextMenuTab.Closed += new ToolStripDropDownClosedEventHandler(this.contextMenuTab_Closed);
            this.tabControl.TabRightClick += new EventHandler<QEventArgs>(this.tabControl_TabRightClick);
            this.tabControl.TabBarRightClick += new EventHandler<QEventArgs>(this.tabControl_TabBarRightClick);
            this.contextMenuBar = new DropDownMenuEx(this.components, false, false, false);
            this.contextMenuBar.ExplorerHandle = this.Handle;
            this.contextMenuBar.ImageList = Graphic.ImageList;
            this.contextMenuBar.Items.Add((ToolStripItem)new ToolStripMenuItem());
            this.contextMenuBar.UsePrefix = true;
            this.contextMenuBar.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuBar_ItemClicked);
            this.contextMenuBar.Opening += new CancelEventHandler(this.contextMenuBar_Opening);
            this.contextMenuBar.Closed += new ToolStripDropDownClosedEventHandler(this.contextMenuBar_Closed);*/
        }

        private volatile bool FirstNavigationCompleted;

        protected bool fShownDW;
        public override void ShowDW(bool fShow)
        {
            this.Visible = this.fShownDW = fShow;
            /*if ((fShow && !FirstNavigationCompleted) && ((Explorer != null) && (Explorer.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE)))
            {
                InitializeInstallation();
            }

            if (!fShow)
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root))
                {
                    key.SetValue("BreakTabBar", BandHasBreak() ? 1 : 0);
                }
            }*/
            this.UpdateView(fShow);
            base.ShowDW(fShow);

            if (this.rebarWindowSubclass != null)
                this.rebarWindowSubclass.Disabled = !fShow;
            if (this.baseBarWindowSubclass != null)
                this.baseBarWindowSubclass.Disabled = !fShow;
            // base.RefreshRebarBand();
            // RefreshRebarBand();
        }



        private void UpdateView(bool fShow)
        {
            try
            {
                this.viewContainer.SuspendLayout();
                while (this.viewContainer.Controls.Count > 1)
                    this.viewContainer.Controls.RemoveAt(0);
                this.viewContainer.ResumeLayout();
            }
            catch (Exception ex)
            {
                string optional = ".UpdateView";
                QTUtility2.MakeErrorLog(ex, optional);
            }
            finally
            {
                // this.tabControl1.ParentChanged = true;
            }
        }


        public void RefreshRebarBand()
        {
            QTUtility2.log("RefreshRebarBand start");
            // REBARBANDINFO* lParam = stackalloc REBARBANDINFO[1];
            REBARBANDINFO lParam = new REBARBANDINFO();
            // lParam.cbSize = sizeof(REBARBANDINFO);
            lParam.cbSize = Marshal.SizeOf(lParam);
            lParam.fMask = 32;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lParam));
            Marshal.StructureToPtr(lParam, ptr, false);

            int wParam = (int)PInvoke.SendMessage(this.ReBarHandle, 1040, this.BandID, 0);
            if (wParam == -1)
                return;
            // PInvoke.SendMessage(this.ReBarHandle, 1052, (IntPtr)wParam, ptr);
            PInvoke.SendMessage(ReBarHandle, 1052, (IntPtr)wParam, ptr);
            // PInvoke.SendMessage(this.Handle, RB.SETBANDINFOW, (void*)wParam, ref structure);
            lParam.cyChild = this.IsVertical ? this.Width : this.Height;
            lParam.cyMinChild = this.IsVertical ? this.Width : this.Height;
            PInvoke.SendMessage(this.ReBarHandle, 1035, (IntPtr)wParam, ptr);
            lParam = (REBARBANDINFO)Marshal.PtrToStructure(ptr, typeof(REBARBANDINFO));
            Marshal.FreeHGlobal(ptr);

            PInvoke.SetWindowPos(this.ReBarHandle,
                IntPtr.Zero,
                0, 0, 400, 500,
                SWP.NOZORDER | SWP.NOACTIVATE
                // SWP.NOMOVE | SWP.NOZORDER | SWP.NOACTIVATE
                );
            PInvoke.SendMessage(this.ReBarHandle, 561, 0, 0);
            PInvoke.SendMessage(this.ReBarHandle, 562, 0, 0);
            QTUtility2.log("RefreshRebarBand end");
        }

        private void InitializeInstallation()
        {
            InitializeOpenedWindow();
            object locationURL = Explorer.LocationURL;
            if (ShellBrowser != null)
            {
                using (IDLWrapper wrapper = ShellBrowser.GetShellPath())
                {
                    if (wrapper.Available)
                    {
                        locationURL = wrapper.Path;
                    }
                }
            }
            QTUtility2.log("QTTabBarClass InitializeInstallation  pDisp :" + null + " locationURL :" + (string)locationURL);
            Explorer_NavigateComplete2(null, ref locationURL);
        }

        /**
         * 初始化已经打开的窗口
         */
        private bool IsShown;
        private void InitializeOpenedWindow()
        {
            IsShown = true;
            //  安装钩子
            QTUtility2.log("QTTabBarClass InitializeOpenedWindow  InstallHooks");
            InstallHooks();
            /*if (QTUtility.WindowAlpha < 0xff)
            {
                QTUtility2.log("QTTabBarClass SetWindowLongPtr SetLayeredWindowAttributes");
                PInvoke.SetWindowLongPtr(ExplorerHandle, -20, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(ExplorerHandle, -20), 0x80000));
                PInvoke.SetLayeredWindowAttributes(ExplorerHandle, 0, QTUtility.WindowAlpha, 2);
            }*/
            listViewManager = new ListViewMonitor(ShellBrowser, ExplorerHandle, Handle);
            listViewManager.ListViewChanged += ListViewMonitor_ListViewChanged;
            listViewManager.Initialize();
        }

        private HookProc hookProc_GetMsg;
        private HookProc hookProc_Key;
        private HookProc hookProc_Mouse;
        private bool fHookInstalled;

        private WindowSubclass rebarWindowSubclass;
        private WindowSubclass baseBarWindowSubclass;

        // 安装钩子
        private void InstallHooks()
        {
            if (this.fHookInstalled)
                return;
            this.fHookInstalled = true;
            // hookProc_Key = new HookProc(CallbackKeyboardProc);
            // hookProc_Mouse = new HookProc(CallbackMouseProc);
            // hookProc_GetMsg = new HookProc(CallbackGetMsgProc);
            // int currentThreadId = PInvoke.GetCurrentThreadId();
            // hHook_Key = PInvoke.SetWindowsHookEx(2, hookProc_Key, IntPtr.Zero, currentThreadId);
            // hHook_Mouse = PInvoke.SetWindowsHookEx(7, hookProc_Mouse, IntPtr.Zero, currentThreadId);
            // hHook_Msg = PInvoke.SetWindowsHookEx(3, hookProc_GetMsg, IntPtr.Zero, currentThreadId);
            this.baseBarWindowSubclass = new WindowSubclass(
                PInvoke.GetWindowLongPtr(this.ReBarHandle, GWL.HWNDPARENT),
                new WindowSubclass.SubclassingProcedure(this.baseBarSubclassProc));
            this.rebarWindowSubclass =
                new WindowSubclass(this.ReBarHandle,
                    new WindowSubclass.SubclassingProcedure(this.rebarSubclassProc));
            WindowUtils.HideBasebarCloseButton(this.ReBarHandle);
            /*explorerController = new NativeWindowController(ExplorerHandle);
            explorerController.MessageCaptured += explorerController_MessageCaptured;
            if (ReBarHandle != IntPtr.Zero)
            {
                rebarController = new RebarController(this, ReBarHandle, BandObjectSite as IOleCommandTarget);
            }
            if (!QTUtility.IsXP)
            {
                TravelToolBarHandle = GetTravelToolBarWindow32();
                if (TravelToolBarHandle != IntPtr.Zero)
                {
                    travelBtnController = new NativeWindowController(TravelToolBarHandle);
                    travelBtnController.MessageCaptured += travelBtnController_MessageCaptured;
                }
            }
            dropTargetWrapper = new DropTargetWrapper(this);
            dropTargetWrapper.DragFileEnter += dropTargetWrapper_DragFileEnter;
            dropTargetWrapper.DragFileOver += dropTargetWrapper_DragFileOver;
            dropTargetWrapper.DragFileLeave += dropTargetWrapper_DragFileLeave;
            dropTargetWrapper.DragFileDrop += dropTargetWrapper_DragFileDrop;*/
        }

        internal int BaseBarPreferredSize { get; set; }

        private unsafe bool baseBarSubclassProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                // case 20:
                case WM.ERASEBKGND: // 0x0014当窗口背景必须被擦除时（例如在窗口改变大小时）
                    QTUtility2.log("WM.ERASEBKGND " + this.IsVertical);
                    Rectangle rectangle = new Rectangle(Point.Empty, PInvoke.GetWindowRect(msg.HWnd).Size);
                    if (this.IsVertical)
                    {
                        if (QTUtility.RightToLeft)
                        {
                            Graphic.FillRectangleRTL(msg.WParam, this.VerticalExplorerBarBackgroundColor, rectangle);
                            if (QTUtility.IsWindows7)
                            {
                                Graphic.DrawLineRTL(msg.WParam, SystemColors.Control, new Point(rectangle.Width - 1, 0), new Point(rectangle.Width - 1, rectangle.Height));
                                Graphic.DrawLineRTL(msg.WParam, SystemColors.ControlDark, new Point(rectangle.Width - 2, 0), new Point(rectangle.Width - 2, rectangle.Height));
                            }
                        }
                        else
                        {
                            using (Graphics graphics = Graphics.FromHdc(msg.WParam))
                            {
                                using (SolidBrush solidBrush = new SolidBrush(this.VerticalExplorerBarBackgroundColor))
                                    graphics.FillRectangle((Brush)solidBrush, rectangle);
                                if (QTUtility.IsWindows7)
                                {
                                    graphics.DrawLine(SystemPens.ControlDark, new Point(rectangle.Width - 1, 0), new Point(rectangle.Width - 1, rectangle.Height));
                                    graphics.DrawLine(SystemPens.Control, new Point(rectangle.Width - 2, 0), new Point(rectangle.Width - 2, rectangle.Height));
                                }
                            }
                        }
                        msg.Result = (IntPtr)1;
                    }
                    else
                    {
                        if (QTUtility.RightToLeft)
                        {
                            Graphic.FillRectangleRTL(msg.WParam, this.HorizontalExplorerBarBackgroundColor, rectangle);
                        }
                        else
                        {
                            using (Graphics graphics = Graphics.FromHdc(msg.WParam))
                            {
                                using (SolidBrush solidBrush = new SolidBrush(this.HorizontalExplorerBarBackgroundColor))
                                    graphics.FillRectangle((Brush)solidBrush, 0, 0, rectangle.Width, rectangle.Height);
                            }
                        }
                        msg.Result = (IntPtr)1;
                    }
                    return true;
                // case 70:
                case WM.WINDOWPOSCHANGING:  // 当窗口位置、大小、Z顺序要改变时会发送 WM_WINDOWPOSCHANGING
                    QTUtility2.log("WM.WINDOWPOSCHANGING " + this.IsVertical);
                    WINDOWPOS* lparam = (WINDOWPOS*)(void*)msg.LParam;
                    if (!this.UserResizing && this.BaseBarPreferredSize != 0 &&
                        !QTUtility2.HasFlag(lparam->flags, SWP.NOSIZE))
                        // !lparam->flags.HasFlag((Enum)SWP.NOSIZE))
                    {
                        Rectangle bounds = PInvoke.GetWindowRect((IntPtr)this.Explorer.HWND);
                        if (this.IsVertical)
                        {
                            if (bounds.X > -30000)
                                this.BaseBarPreferredSize = Math.Min(this.BaseBarPreferredSize, (int)((double)bounds.Width * 0.75));
                            lparam->cx = this.BaseBarPreferredSize;

                            var IsTabBarAvailable = this != null && this.IsHandleCreated && !this.IsDisposed && this.ReBarHandle != IntPtr.Zero;
                            var IsTabBarVisible = IsTabBarAvailable && this.fShownDW;
                            var IsTopTabBarVisible = IsTabBarVisible ;
                                                     // && !(this is QTHorizontalExplorerBar);

                            // if (this.explorerManager.Toolbars.IsTopTabBarVisible)
                            
                            if (IsTopTabBarVisible)
                            {
                                this.baseBarWindowSubclass.DefaultWindowProcedure(ref msg);
                                // this.explorerManager.Toolbars.TabBar.StayAboveDefaultView();
                                return true;
                            }
                            break;
                        }
                        if (bounds.X > -30000)
                            this.BaseBarPreferredSize = Math.Min(this.BaseBarPreferredSize, (int)((double)bounds.Height * 0.75));
                        lparam->cy = this.BaseBarPreferredSize;
                        break;
                    }

                    if (!QTUtility2.HasFlag(lparam->flags, SWP.NOSIZE))
                    // if (!lparam->flags.HasFlag((Enum)SWP.NOSIZE))
                    {
                        // Rectangle bounds = this.explorerManager.Bounds;
                        Rectangle bounds = PInvoke.GetWindowRect((IntPtr)this.Explorer.HWND);
                        if (bounds.X > -32000 && bounds.Y > -32000)
                        {
                            if (this.IsVertical)
                            {
                                lparam->cx = Math.Min(lparam->cx, (int)((double)bounds.Width * 0.75));
                                break;
                            }
                            lparam->cy = Math.Min(lparam->cy, (int)((double)bounds.Height * 0.75));
                            break;
                        }
                        break;
                    }
                    break;
                case 123:
                    QTUtility2.log("123 " + this.IsVertical);
                    return true;
                case 163:
                    // this.OnBaseBarBorderDoubleClick((int)(long)msg.WParam);
                    QTUtility2.log("163 " + this.IsVertical);
                    break;
                case 561:
                    QTUtility2.log("561 " + this.IsVertical);
                    this.UserResizing = true;
                    break;
                case 562:
                    QTUtility2.log("562 " + this.IsVertical);
                    try
                    {
                        if (this.IsVertical)
                        {
                            this.BaseBarPreferredSize = PInvoke.GetWindowRect(this.Handle).Width;
                            this.baseBarWindowSubclass.DefaultWindowProcedure(ref msg);
                            // this.RefreshRebar();
                            // if (this.explorerManager.Toolbars.IsTopTabBarVisible)
                            //     this.explorerManager.Toolbars.TabBar.StayAboveDefaultView();
                            return true;
                        }
                        this.BaseBarPreferredSize = PInvoke.GetWindowRect(this.Handle).Height;
                        break;
                    }
                    finally
                    {
                        this.UserResizing = false;
                    }
            }
            return false;
        }

        protected override unsafe void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 70:
                    WINDOWPOS* lparam = (WINDOWPOS*)(void*)m.LParam;
                    if (this.IsVertical)
                    {
                        // if (!lparam->flags.HasFlag((Enum)SWP.NOMOVE))
                        if (!QTUtility2.HasFlag(lparam->flags, SWP.NOMOVE))
                        {
                            int num = QTUtility.IsWindows7 ? 28 : 25;
                            lparam->y -= num;
                            lparam->cy += num;
                            lparam->cx += 4;
                            break;
                        }
                        break;
                    }
                    int num1 = lparam->x - 2;
                    lparam->x = 2;
                    lparam->cx += num1;
                    break;
                case 123:
                    if (MenuUtility.InMenuLoop)
                        return;
                    Point point = MCR.GET_POINT_LPARAM(m.LParam);
                    if (this.HitTest(point))
                    {
                        // this.OnCommandContainerContextMenu(point);
                        return;
                    }
                    if (point.X == -1 && point.Y == -1)
                    {
                        Rectangle windowRect = PInvoke.GetWindowRect(m.HWnd);
                        point = new Point(windowRect.Right - 4, windowRect.Bottom - 4);
                    }
                    // this.contextMenuBar.Show(point);
                    return;
            }
            base.WndProc(ref m);
        }

        public bool HitTest(Point pnt)
        {
            return !this.splitContainer.Panel1Collapsed && this.splitContainer.RectangleToScreen(this.splitContainer.Panel1.Bounds).Contains(pnt);
        }


        /*private bool HasFlag(SWP pbit, SWP pflag)
        {
            if ((pflag & pbit) == pflag)
                return true;
            else
                return false;
        }*/

       
       

        private bool rebarSubclassProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case WM.PAINT: // WM_PAINT 0x000F 15 要求一个窗口重绘自己
                    QTUtility2.log("rebarSubclassProc WM_PAINT " + this.IsVertical);
                    msg.Result = PInvoke.DefWindowProc(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
                    return true;
                case WM.ERASEBKGND: // WM_ERASEBKGND 0x0014 20 当窗口背景必须被擦除时（例如在窗口改变大小时）
                    QTUtility2.log("rebarSubclassProc WM_ERASEBKGND " + this.IsVertical);
                    RECT pRect;
                    PInvoke.GetWindowRect(msg.HWnd, out pRect);
                    Rectangle rct = new Rectangle(0, 0, pRect.Width, pRect.Height);
                    Graphic.FillRectangleRTL(msg.WParam, this.IsVertical ? this.VerticalExplorerBarBackgroundColor : this.HorizontalExplorerBarBackgroundColor, rct, QTUtility.RightToLeft);
                    msg.Result = (IntPtr)1;
                    return true;
                default:
                    return false;
            }
        }

        private IntPtr hHook_Key;
        private IntPtr hHook_Mouse;
        private IntPtr hHook_Msg;
        /**
        * 注册快捷键的回调
        */
        private IntPtr CallbackKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const uint KB_TRANSITION_FLAG = 0x80000000;
            const uint KB_PREVIOUS_STATE_FLAG = 0x40000000;
            if (nCode < 0 || NowModalDialogShown)
            {
                return PInvoke.CallNextHookEx(hHook_Key, nCode, wParam, lParam);
            }

            try
            {
                uint flags = (uint)((long)lParam);
                bool isKeyPress = (flags & KB_TRANSITION_FLAG) == 0;
                bool isRepeat = (flags & KB_PREVIOUS_STATE_FLAG) != 0;
                Keys key = (Keys)((int)wParam);

                if (key == Keys.ShiftKey)
                {
                    if (isKeyPress || !isRepeat)
                    {
                        listView.HandleShiftKey();
                    }
                }

                if (isKeyPress)
                {
                    /*if (HandleKEYDOWN(key, isRepeat))
                    {
                        return new IntPtr(1);
                    }*/
                }
                else
                {
                    listView.HideThumbnailTooltip(3);
                    /*if (NowTabDragging && DraggingTab != null)
                    {
                        Cursor = Cursors.Default;
                    }*/

                    switch (key)
                    {
                        case Keys.ControlKey:
                            if (Config.Keys.UseTabSwitcher)
                            {
                                // HideTabSwitcher(true);
                            }
                            break;

                        case Keys.Menu: // Alt key
                            if (Config.Tabs.ShowCloseButtons && Config.Tabs.CloseBtnsWithAlt)
                            {
                                tabControl1.ShowCloseButton(false);
                            }
                            break;

                        case Keys.Tab:
                            /*if (Config.Keys.UseTabSwitcher && tabSwitcher != null && tabSwitcher.IsShown)
                            {
                                tabControl1.SetPseudoHotIndex(tabSwitcher.SelectedIndex);
                            }*/
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex,
                        String.Format("LParam: {0:x4}, WParam: {1:x4}", (long)lParam, (long)wParam));
            }
            return PInvoke.CallNextHookEx(hHook_Key, nCode, wParam, lParam);
        }


        // 绑定 鼠标组合快捷键
        private IntPtr CallbackMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && !NowModalDialogShown)
                {
                    IntPtr ptr = (IntPtr)1;
                    switch (((int)wParam))
                    {
                        case WM.MOUSEWHEEL:
                            /*if (!HandleMOUSEWHEEL(lParam))
                            {
                                break;
                            }*/
                            return ptr;

                        /*case WM.MBUTTONDOWN: // add by indiff
                            QTUtility2.log("CallbackMouseProc Handle_MButtonUp_Tree");
                            if (Handle_MButtonUp_Tree(ExplorerHandle,lParam))
                            {
                                break;
                            }
                            return ptr;*/

                        case WM.XBUTTONDOWN:

                        case WM.XBUTTONUP:
                            MouseButtons mouseButtons = MouseButtons;
                            Keys modifierKeys = ModifierKeys;
                            MouseChord chord = mouseButtons == MouseButtons.XButton1
                                    ? MouseChord.X1
                                    : mouseButtons == MouseButtons.XButton2 ? MouseChord.X2 : MouseChord.None;
                            if (chord == MouseChord.None) break;
                            chord = QTUtility.MakeMouseChord(chord, modifierKeys);
                            BindAction action;
                            if (!Config.Mouse.GlobalMouseActions.TryGetValue(chord, out action))
                            {
                                break;
                            }
                            if (((int)wParam) == WM.XBUTTONUP && !Explorer.Busy)
                            {
                                // DoBindAction(action);
                            }
                            return ptr;
                    }
                }
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, String.Format("LParam: {0:x4}, WParam: {1:x4}", (long)lParam, (long)wParam));
            }
            return PInvoke.CallNextHookEx(hHook_Mouse, nCode, wParam, lParam);
        }

        private int iSequential_WM_CLOSE;
        private readonly int WM_NEWTREECONTROL = PInvoke.RegisterWindowMessage("QTSecondViewBar_NewTreeControl");
        private readonly int WM_BROWSEOBJECT = PInvoke.RegisterWindowMessage("QTSecondViewBar_BrowseObject");
        private readonly int WM_HEADERINALLVIEWS = PInvoke.RegisterWindowMessage("QTSecondViewBar_HeaderInAllViews");
        private readonly int WM_LISTREFRESHED = PInvoke.RegisterWindowMessage("QTSecondViewBar_ListRefreshed");
        private readonly int WM_SHOWHIDEBARS = PInvoke.RegisterWindowMessage("QTSecondViewBar_ShowHideBars");
        private readonly int WM_CHECKPULSE = PInvoke.RegisterWindowMessage("QTSecondViewBar_CheckPulse");
        private readonly int WM_SELECTFILE = PInvoke.RegisterWindowMessage("QTTabBar_SelectFile");
        /// <summary>
        /// 注册windows消息
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr CallbackGetMsgProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                try
                {
                    if (QTUtility.IsXP)
                    {
                        if (msg.message == WM.CLOSE)
                        {
                            if (iSequential_WM_CLOSE > 0)
                            {
                                Marshal.StructureToPtr(new MSG(), lParam, false);
                                return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);
                            }
                            iSequential_WM_CLOSE++;
                        }
                        else
                        {
                            iSequential_WM_CLOSE = 0;
                        }
                    }

                    if (msg.message == WM_NEWTREECONTROL)
                    {
                        QTUtility2.log("CallbackGetMsgProc WM_NEWTREECONTROL");
                        object obj = Marshal.GetObjectForIUnknown(msg.wParam);
                        try
                        {
                            if (obj != null)
                            {
                                IOleWindow window = obj as IOleWindow;
                                if (window != null)
                                {
                                    IntPtr hwnd;
                                    window.GetWindow(out hwnd);
                                    if (hwnd != IntPtr.Zero && PInvoke.IsChild(ExplorerHandle, hwnd))
                                    {
                                        hwnd = WindowUtils.FindChildWindow(hwnd,
                                                child => PInvoke.GetClassName(child) == "SysTreeView32");
                                        if (hwnd != IntPtr.Zero)
                                        {
                                            INameSpaceTreeControl control = obj as INameSpaceTreeControl;
                                            if (control != null)
                                            {
                                                // if (treeViewWrapper != null)
                                                // {
                                                //     treeViewWrapper.Dispose();
                                                // }
                                                // treeViewWrapper = new TreeViewWrapper(hwnd, control);
                                                // treeViewWrapper.TreeViewClicked += FolderLinkClicked;
                                                QTUtility2.log("CallbackGetMsgProc regedit TreeViewClicked");
                                                obj = null; // Release the object only if we didn't get this far.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (obj != null)
                            {
                                QTUtility2.log("ReleaseComObject obj");
                                Marshal.ReleaseComObject(obj);
                            }
                        }
                        return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);
                    }
                    else if (msg.message == WM_LISTREFRESHED)
                    {
                        // HandleF5();
                        return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);
                    }

                    switch (msg.message)
                    {
                        /* TODO: Handle FolderView clicks on XP.
                        case WM.LBUTTONDOWN:
                        case WM.LBUTTONUP:
                            if((QTUtility.IsXP && !Config.NoMidClickTree) && ((((int)((long)msg.wParam)) & 4) != 0)) {
                                HandleLBUTTON_Tree(msg, msg.message == 0x201);
                            }
                            break;
                        */
                        case WM.ERASEBKGND: // 0x0014当窗口背景必须被擦除时（例如在窗口改变大小时）
                        {
                            Rectangle rectangle = new Rectangle(Point.Empty, PInvoke.GetWindowRect(msg.hwnd).Size);
                            if (this.IsVertical)
                            {
                                if (QTUtility.RightToLeft)
                                {
                                    Graphic.FillRectangleRTL(msg.wParam, this.VerticalExplorerBarBackgroundColor, rectangle);

                                    if (QTUtility.IsWindows7)
                                    {
                                        Graphic.DrawLineRTL(msg.wParam, SystemColors.Control, new Point(rectangle.Width - 1, 0), new Point(rectangle.Width - 1, rectangle.Height));
                                        Graphic.DrawLineRTL(msg.wParam, SystemColors.ControlDark, new Point(rectangle.Width - 2, 0), new Point(rectangle.Width - 2, rectangle.Height));
                                    }
                                }
                                else
                                {
                                    using (Graphics graphics = Graphics.FromHdc(msg.wParam))
                                    {
                                        using (SolidBrush solidBrush = new SolidBrush(this.VerticalExplorerBarBackgroundColor))
                                            graphics.FillRectangle((Brush)solidBrush, rectangle);
                                        if (QTUtility.IsWindows7)
                                        {
                                            graphics.DrawLine(SystemPens.ControlDark, new Point(rectangle.Width - 1, 0), new Point(rectangle.Width - 1, rectangle.Height));
                                            graphics.DrawLine(SystemPens.Control, new Point(rectangle.Width - 2, 0), new Point(rectangle.Width - 2, rectangle.Height));
                                        }
                                    }
                                }

                                msg.Result = (IntPtr) 1;
                            }
                            else
                            {
                                if (QTUtility.RightToLeft)
                                {
                                    Graphic.FillRectangleRTL(msg.wParam, this.HorizontalExplorerBarBackgroundColor, rectangle);
                                }
                                else
                                {
                                    using (Graphics graphics = Graphics.FromHdc(msg.wParam))
                                    {
                                        using (SolidBrush solidBrush = new SolidBrush(this.HorizontalExplorerBarBackgroundColor))
                                            graphics.FillRectangle((Brush) solidBrush, 0, 0, rectangle.Width, rectangle.Height);
                                    }
                                }
                                msg.Result = (IntPtr) 1;
                            }

                            break;
                        }
                        case WM.MBUTTONUP:
                            if (!Explorer.Busy) // && !Config.NoMidClickTree
                            {
                                QTUtility2.log("CallbackGetMsgProc MBUTTONUP NoMidClickTree");
                                // Handle_MButtonUp_Tree(msg);
                            }
                            break;
                        case WM.SYSCOLORCHANGE:
                            QTUtility.InNightMode = QTUtility.getNightMode();
                            QTUtility2.log("SYSCOLORCHANGE SwitchNighMode");
                            Config.Skin.SwitchNighMode(QTUtility.InNightMode); // 如果关闭自动变色则不进行变色
                            ConfigManager.UpdateConfig(true);
                            this.tabControl1.InitializeColors();
                            PInvoke.SetRedraw(ExplorerHandle, true);
                            PInvoke.RedrawWindow(ExplorerHandle, IntPtr.Zero, IntPtr.Zero, 0x289);
                            break;

                        case WM.CLOSE:  // 关闭窗口
                            if (QTUtility.IsXP)
                            {
                                if ((msg.hwnd == ExplorerHandle) && HandleCLOSE(msg.lParam))
                                {
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                                break;
                            }


                            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root))
                            {
                                string[] list = (from QTabItem item2 in tabControl1.TabPages
                                                 where item2.TabLocked
                                                 select item2.CurrentPath).ToArray();
                                // MessageBox.Show(String.Join(",", list));
                                QTUtility2.WriteRegBinary(list, "TabsLocked", key);
                            }
                            if (msg.hwnd == WindowUtils.GetShellTabWindowClass(ExplorerHandle))
                            { // 如果标签的 handle 与资源管理器的匹配
                                try
                                {
                                    bool flag = tabControl1.TabCount == 1;
                                    string currentPath = tabControl1.SelectedTab.CurrentPath;
                                    if (!Directory.Exists(currentPath) && // 如果当前路径目录不存在
                                       currentPath.Length > 3
                                        /* && currentPath.Substring(1, 2) == @":\" */ )
                                    {
                                        if (flag)
                                        {
                                            WindowUtils.CloseExplorer(ExplorerHandle, 2);
                                        }
                                        else
                                        {
                                            CloseTab(tabControl1.SelectedTab, true);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    QTUtility2.MakeErrorLog(e, "CallbackGetMsgProc WM.Close");
                                }
                                Marshal.StructureToPtr(new MSG(), lParam, false);
                            }
                            break;

                        case WM.COMMAND:
                            if (QTUtility.IsXP)
                            {
                                int num = ((int)((long)msg.wParam)) & 0xffff;
                                if (num == 0xa021)
                                {
                                    WindowUtils.CloseExplorer(ExplorerHandle, 3);
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                            }
                            break;

                        /*case 48648:  // not work
                            QTUtility2.log("48648");
                            break;*/
                    }
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex, String.Format("Message: {0:x4}", msg.message));
                }
            }
            return PInvoke.CallNextHookEx(hHook_Msg, nCode, wParam, lParam);
        }

        public Color HorizontalExplorerBarBackgroundColor
        {
            get
            {
                return !QTUtility.InNightMode ? ShellColors.ExplorerBarHrztBGColor : ShellColors.Default;
            }
        }

        protected Color VerticalExplorerBarBackgroundColor
        {
            get
            {
                return !QTUtility.InNightMode ? ShellColors.ExplorerBarVertBGColor : ShellColors.Default;
            }
        }

        protected override bool IsTabSubFolderMenuVisible
        {
            get { return isTabSubFolderMenuVisible1; }
        }

        protected override int CalcBandHeight(int count)
        {
            return 1;
        }

        public bool IsVertical
        {
            get
            {
                return true;
            }
        }

        internal  bool IsBottomBar
        {
            get
            {
                return false;
            }
        }


        private List<string> CloseAllTabsExcept(QTabItem leaveThisOne, bool leaveLocked = true)
        {
            List<QTabItem> tabs = tabControl1.TabPages.Where(item =>
                !(leaveLocked && item.TabLocked) && item != leaveThisOne).ToList();
            List<string> paths = tabs.Select(tab => tab.CurrentPath).ToList();
            CloseTabs(tabs, !leaveLocked);
            return paths;
        }

        private void CloseTabs(IEnumerable<QTabItem> tabs, bool fCritical = false)
        {
            tabControl1.SetRedraw(false);
            bool closeCurrent = false;
            foreach (QTabItem tab in tabs)
            {
                if (tab == CurrentTab)
                    closeCurrent = true;
                else
                    CloseTab(tab, fCritical, true);
            }
            if (closeCurrent)
            {
                CloseTab(CurrentTab, fCritical);
            }
            else
            {
                // TryCallButtonBar(bbar => bbar.RefreshButtons());
                QTabItem.CheckSubTexts(tabControl1);
            }
            if (tabControl1.TabCount > 0)
            {
                tabControl1.SetRedraw(true);
            }
        }


        // 关闭标签， 如果锁定则不关闭
        private bool CloseTab(QTabItem closingTab, bool fCritical, bool fSkipSync = false)
        {
            if (closingTab == null)
            {
                return false;
            }
            if ((!fCritical && closingTab.TabLocked) && QTUtility2.PathExists(closingTab.CurrentPath))
            {
                return false;
            }
            int index = tabControl1.TabPages.IndexOf(closingTab);
            if (index == -1)
            {
                return false;
            }
            // lstActivatedTabs.Remove(closingTab);
            // AddToHistory(closingTab);
            tabControl1.TabPages.Remove(closingTab);
            closingTab.OnClose();
            if (closingTab != CurrentTab)
            {
                if (!fSkipSync)
                {
                    // TryCallButtonBar(bbar => bbar.RefreshButtons());
                    QTabItem.CheckSubTexts(tabControl1);
                }
                return true;
            }
            CurrentTab = null;
            int tabCount = tabControl1.TabCount;
            if (tabCount == 0) return true;
            QTabItem tabPage = null;
            switch (Config.Tabs.NextAfterClosed)
            {
                case TabPos.Right:
                    tabPage = tabControl1.TabPages[index == tabCount ? index - 1 : index];
                    break;

                case TabPos.Left:
                    tabPage = tabControl1.TabPages[index == 0 ? 0 : index - 1];
                    break;

                case TabPos.Rightmost:
                    tabPage = tabControl1.TabPages[tabCount - 1];
                    break;

                case TabPos.Leftmost:
                    tabPage = tabControl1.TabPages[0];
                    break;

                case TabPos.LastActive:
                    if (lstActivatedTabs.Count > 0)
                    {
                        QTabItem lastTab = lstActivatedTabs[lstActivatedTabs.Count - 1];
                        lstActivatedTabs.RemoveAt(lstActivatedTabs.Count - 1);
                        tabPage = tabControl1.TabPages.Contains(lastTab)
                                ? lastTab
                                : tabControl1.TabPages[0];
                    }
                    else
                    {
                        tabPage = tabControl1.TabPages[0];
                    }
                    break;
            }
            if (tabPage != null)
            {
                tabControl1.SelectTab(tabPage);
            }
            else
            {
                tabControl1.SelectTab(0);
            }
            if (!fSkipSync)
            {
                // TryCallButtonBar(bbar => bbar.RefreshButtons());
            }
            return true;
        }

        /**
        * 处理关闭操作
        */
        private bool HandleCLOSE(IntPtr lParam)
        {
            bool flag = Config.Window.CloseBtnClosesSingleTab;
            bool flag2 = Config.Window.CloseBtnClosesUnlocked;
            List<string> closingPaths = new List<string>();
            int num = (int)lParam;
            switch (num)
            {
                case 1:
                    closingPaths = CloseAllTabsExcept(null, flag2);
                    if (tabControl1.TabCount > 0)
                    {
                        return true;
                    }
                    break;

                case 2:
                    return false;

                default:
                    {
                        bool flag3 = QTUtility2.PathExists(CurrentTab.CurrentPath);
                        if ((QTUtility.IsXP && flag3) && (num == 0))
                        {
                            return true;
                        }
                        if (!flag3)
                        {
                            CloseTab(CurrentTab, true);
                            return (tabControl1.TabCount > 0);
                        }
                        if (flag2 && !flag)
                        {
                            closingPaths = CloseAllTabsExcept(null);
                            if (tabControl1.TabCount > 0)
                            {
                                return true;
                            }
                            QTUtility.SaveClosing(closingPaths);
                            return false;
                        }
                        Keys modifierKeys = ModifierKeys;
                        if ((modifierKeys == (Keys.Control | Keys.Shift)) || !flag)
                        {
                            foreach (QTabItem item2 in tabControl1.TabPages)
                            {
                                closingPaths.Add(item2.CurrentPath);
                                // AddToHistory(item2);
                            }
                            QTUtility.SaveClosing(closingPaths);
                            return false;
                        }
                        if (modifierKeys == Keys.Control)
                        {
                            closingPaths = CloseAllTabsExcept(null);
                        }
                        else
                        {
                            closingPaths.Add(CurrentTab.CurrentPath);
                            CloseTab(CurrentTab, false);
                        }
                        if (tabControl1.TabCount > 0)
                        {
                            return true;
                        }
                        QTUtility.SaveClosing(closingPaths);
                        return false;
                    }
            }
            QTUtility.SaveClosing(closingPaths);
            return false;
        }

        private void ListViewMonitor_ListViewChanged(object sender, EventArgs args)
        {
            if (listViewManager != null) // 修复空指针问题 by indiff
            {
                listView = listViewManager.CurrentListView;
                ExtendedListViewCommon elvc = listView as ExtendedListViewCommon;
                if (elvc != null)
                {
                    // elvc.ItemCountChanged += ListView_ItemCountChanged;
                    // elvc.SelectionActivated += ListView_SelectionActivated;
                    // elvc.SelectionChanged += ListView_SelectionChanged;
                    // elvc.MiddleClick += ListView_MiddleClick;
                    // elvc.DoubleClick += ListView_DoubleClick;
                    // elvc.EndLabelEdit += ListView_EndLabelEdit;
                    // elvc.MouseActivate += ListView_MouseActivate;
                    // elvc.SubDirTip_MenuItemClicked += subDirTip_MenuItemClicked;
                    // elvc.SubDirTip_MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                    // elvc.SubDirTip_MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                    // elvc.SubDirTip_MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                }
            }
            // HandleF5();
        }

        public override void CloseDW(uint dwReserved)
        {
            try
            {
                /*string[] list1 = (from ITab tab in pluginServer.GetTabs()
                                 where tab.Locked
                                 select tab.Address.Path).ToArray();
                MessageBox.Show(String.Join(",", list1));
               

                MessageBox.Show("关闭窗口:" + tabControl1.TabPages.Count );
                string[] list = (from QTabItem item2 in tabControl1.TabPages
                                 where item2.TabLocked
                                 select item2.CurrentPath).ToArray();
                MessageBox.Show(String.Join(",", list));
 */
                this.viewContainer.Controls.Clear();
                foreach (QTabItem tab in this.tabControl1.TabPages)
                    tab.OnClose();

                this.UninstallHooks();
                if (listViewManager != null)
                {
                    listViewManager.Dispose();
                    listViewManager = null;
                }

                if (TravelLog != null)
                {
                    QTUtility2.log("ReleaseComObject TravelLog");
                    Marshal.FinalReleaseComObject(TravelLog);
                    TravelLog = null;
                }
                if (shellContextMenu != null)
                {
                    shellContextMenu.Dispose();
                    shellContextMenu = null;
                }
                if (ShellBrowser != null)
                {
                    ShellBrowser.Dispose();
                    ShellBrowser = null;
                }
                /*foreach (ITravelLogEntry entry in LogEntryDic.Values)
                {
                    if (entry != null)
                    {
                        QTUtility2.log("ReleaseComObject entry");
                        Marshal.FinalReleaseComObject(entry);
                    }
                }*/
                fFinalRelease = true;
            }
            catch (Exception exception2)
            {
                QTUtility2.MakeErrorLog(exception2, "tabbar closing");
            }
            base.CloseDW(dwReserved);
        }

        private void UninstallHooks()
        {
            if (this.rebarWindowSubclass != null)
            {
                this.rebarWindowSubclass.ReleaseHandle();
                this.rebarWindowSubclass = (WindowSubclass)null;
            }
            if (this.baseBarWindowSubclass == null)
                return;
            this.baseBarWindowSubclass.ReleaseHandle();
            this.baseBarWindowSubclass = (WindowSubclass)null;
        }

        // public virtual void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi) {
        public override void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO dbi)
        {
            base.GetBandInfo(dwBandID, dwViewMode, ref dbi);
            try
            {
                if ((dbi.dwMask & DBIM.INTEGRAL) != (0))
                {
                    // dbi.ptActual.X = Size.Width;
                    // dbi.ptActual.Y = BandHeight;
                    dbi.ptIntegral.X = 1;
                    dbi.ptIntegral.Y = 1;
                }

                if (this.fNowResizing && (dbi.dwMask & DBIM.MINSIZE) != (DBIM)0)
                {
                    dbi.ptMinSize.X = this.prefSize;
                    dbi.ptMinSize.Y = this.prefSize;
                    this.fNowResizing = false;
                }

                // 可以更改带对象的高度  
                // 不会显示大小调整手柄，以允许用户移动或调整带对象的大小。  
                // DBIMF.NOMARGINS 带对象不应显示边距。

                if ((dbi.dwMask & DBIM.MODEFLAGS) != (0))
                {
                    dbi.dwModeFlags = DBIMF.VARIABLEHEIGHT | DBIMF.NOMARGINS;
                }

                /*if ((dbi.dwMask & DBIM.TITLE) != (0))
                {
                    dbi.wszTitle = "second";
                }*/
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex);
            }
        }

        /*protected override void OnExplorerAttached()
        {
            try
            {
                this.explorerManager = InstanceManager.Register((BandObject)this, (object)this.bandObjectSite);
                this.InitializeWidth();
                this.viewContainer.CreateControl();
                this.InstallHooks();
                this.ProcessStartups();
                this.ProcessStartups1();
                this.fProcessingStartups = false;
                base.OnExplorerAttached();
            }
            catch (Exception ex)
            {
                DebugUtil.AppendToCriticalExceptionLog(ex);
            }
        }*/

        /*
        internal static bool IsUnnavigatablePath(ItemIDList idl)
        {
            using (ShellItem pidl2 = new ShellItem((byte[])idl, false))
            {
                string path = pidl2.Path;
                using (ShellItem pidl1 = new ShellItem("::{26EE0668-A00A-44D7-9371-BEB064C98683}"))
                {
                    if (!string.Equals("::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0", path))
                    {
                        if (!string.Equals(pidl1.Path, path, StringComparison.OrdinalIgnoreCase))
                        {
                            if (PInvoke.ILIsParent((IntPtr)pidl1, (IntPtr)pidl2, false))
                                return true;
                        }
                    }
                }
                if (string.Equals(path, "::{9343812E-1C37-4A49-A12E-4B2D810D956B}", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }*/
        protected bool fEventsActivated;

        private void ActivateEvents(bool fActive)
        {
            if (fActive)
            {
                if (this.fEventsActivated)
                    return;
                // this.explorerBrowser.KeyDown = new EventHandler
                // this.explorerManager.MouseHookProc += new HookProc(this.explorerManager_MouseHookProc);
                // this.explorerManager.KeyDown += new EventHandler<KeyHookEventArgs>(this.explorerManager_KeyDown);
                // this.explorerManager.KeyUp += new EventHandler<KeyHookEventArgs>(this.explorerManager_KeyUp);
                // this.explorerManager.ExplorerManagerEvent += new EventHandler<ExplorerManagerEventArgs>(this.explorerManager_ExplorerManagerEvent);
                // this.explorerManager.SubFolderMenuEvents += new EventHandler<ExplorerManagerEventArgs>(this.explorerManager_SubFolderMenuEvents);
                this.fEventsActivated = true;
            }
            else
            {
                if (!this.fEventsActivated)
                    return;
                // this.explorerManager.MouseHookProc -= new HookProc(this.explorerManager_MouseHookProc);
                // this.explorerManager.KeyDown -= new EventHandler<KeyHookEventArgs>(this.explorerManager_KeyDown);
                // this.explorerManager.KeyUp -= new EventHandler<KeyHookEventArgs>(this.explorerManager_KeyUp);
                // this.explorerManager.ExplorerManagerEvent -= new EventHandler<ExplorerManagerEventArgs>(this.explorerManager_ExplorerManagerEvent);
                // this.explorerManager.SubFolderMenuEvents -= new EventHandler<ExplorerManagerEventArgs>(this.explorerManager_SubFolderMenuEvents);
                this.fEventsActivated = false;
            }
        }

        protected bool fProcessingStartups = true;
        protected override void OnExplorerAttached()
        {
            QTUtility2.log("QTTabBarClass OnExplorerAttached");
            ExplorerHandle = (IntPtr)Explorer.HWND;
            /*try
            {
                object obj2;
                object obj3;
                _IServiceProvider bandObjectSite = (_IServiceProvider)BandObjectSite;
                bandObjectSite.QueryService(ExplorerGUIDs.IID_IShellBrowser, ExplorerGUIDs.IID_IUnknown, out obj2);
                ShellBrowser = new ShellBrowserEx((IShellBrowser)obj2);
                HookLibManager.InitShellBrowserHook(ShellBrowser.GetIShellBrowser());
                if (Config.Tweaks.ForceSysListView)
                {
                    ShellBrowser.SetUsingListView(true);
                }
                bandObjectSite.QueryService(ExplorerGUIDs.IID_ITravelLogStg, ExplorerGUIDs.IID_ITravelLogStg, out obj3);
                TravelLog = (ITravelLogStg)obj3;
            }
            catch (COMException exception)
            {
                QTUtility2.MakeErrorLog(exception);
            }*/
            // this.explorerManager = InstanceManager.Register((BandObject)this, (object)this.bandObjectSite);

            // Explorer.BeforeNavigate2 += Explorer_BeforeNavigate2;
            // Explorer.NavigateComplete2 += Explorer_NavigateComplete2;
            // this.viewContainer.CreateControl();
            // this.InstallHooks();
            this.fProcessingStartups = false;
            Activate();
            base.OnExplorerAttached();
        }

        private VisualStyleRenderer bgRenderer;
        private bool isTabSubFolderMenuVisible;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (VisualStyleRenderer.IsSupported)
            {
                if (bgRenderer == null)
                {
                    bgRenderer = new VisualStyleRenderer(VisualStyleElement.Rebar.Band.Normal);
                }
                bgRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else
            {
                if (ReBarHandle != IntPtr.Zero)
                {
                    int colorref = (int)PInvoke.SendMessage(ReBarHandle, 0x414, IntPtr.Zero, IntPtr.Zero);
                    using (SolidBrush brush = new SolidBrush(QTUtility2.MakeColor(colorref)))
                    {
                        e.Graphics.FillRectangle(brush, e.ClipRectangle);
                        return;
                    }
                }
                base.OnPaintBackground(e);
            }
        }

        private void Explorer_BeforeNavigate2(object pDisp,
            ref object URL,
            ref object Flags,
            ref object TargetFrameName,
            ref object PostData,
            ref object Headers,
            ref bool Cancel)
        {
            // DebugUtil.WriteLine("QTTabBarClass Explorer_BeforeNavigate2:" ); // add by qwop.
            QTUtility2.log("QTSecondViewBar Explorer_BeforeNavigate2  pDisp :" + pDisp
                                                                             + " URL :" + (string)URL
                                                                             + " Flags :" + Flags
                                                                             + " TargetFrameName :" + TargetFrameName
                                                                             + " PostData :" + PostData
                                                                             + " Headers :" + Headers
                                                                             + " Cancel :" + Cancel

            );
            /*if (!IsShown)
            {
                DoFirstNavigation(true, (string)URL);
            }*/
        }

        private void Explorer_NavigateComplete2(object pDisp, ref object URL)
        {
            // add by indiff.
            QTUtility2.log("QTSecondViewBar Explorer_NavigateComplete2  pDisp :"
                           + pDisp
                           + " URL :" + (string)URL
            );
        }

        private bool isFirst = true;
        private bool isTabSubFolderMenuVisible1;

        public void Activate()
        {
            try
            {
                if (isFirst)
                {
                    // explorerBrowser.Navigate((ShellObject)KnownFolders.Computer);
                    explorerBrowser.Navigate(CurrentTab.CurrentPath);
                    isFirst = false;
                }
                if (explorerBrowser != null)
                    explorerBrowser.UIActivate();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                // explorerBrowser.Navigate((ShellObject)KnownFolders.Computer);
                explorerBrowser.Navigate(CurrentTab.CurrentPath);
            }
        }

        [ComRegisterFunction]
        private static void Register(Type t)
        {
            string name = t.GUID.ToString("B");
            string str = QTUtility.IsChinese ? "左侧视图" : (QTUtility.IsJapanese ? "エクストラ ビュー (左)" : "Extra View (left)");
            string helpStr = QTUtility.IsChinese ? "左侧扩展视图" : (QTUtility.IsJapanese ? "左にさらにビューを追加します。" : "Extra View (left)");
            using (RegistryKey subKey = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name))
            {
                subKey.SetValue(null, "");
                subKey.SetValue("MenuText", (object)str);
                subKey.SetValue("HelpText", (object)helpStr);
                // 垂直资源管理器栏	CATID_InfoBand
                // public const string CATID_CommBand = "{00021494-0000-0000-C000-000000000046}";
                subKey.CreateSubKey("Implemented Categories\\{00021493-0000-0000-C000-000000000046}"); 
                // subKey.CreateSubKey(@"Implemented Categories\{00021494-0000-0000-C000-000000000046}"); 
            }

            /*using (RegistryKey subKey =
                   Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar"))
            {
                subKey.SetValue(name, (object)"QTSecondViewBar");
            }*/
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t)
        {
            string str = t.GUID.ToString("B");
            try
            {
                using (RegistryKey subKey = Registry.ClassesRoot.OpenSubKey("CLSID", true ))
                {
                    // subKey.DeleteSubKeyTree(str, false);
                    subKey.DeleteSubKeyTree(str);
                }

                /*using (RegistryKey subKey =
                       Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Toolbar", true ))
                {
                    subKey.DeleteValue(str, false);
                }*/
                    
            }
            catch
            {
            }

            try
            {
               
            }
            catch
            {
            }
        }

        /*protected override bool IsTabSubFolderMenuVisible
        {
            get { return isTabSubFolderMenuVisible; }
        }

        protected override int CalcBandHeight(int count)
        {
            throw new NotImplementedException();
        }*/


        #region 标签栏事件区
        public RebarController rebarController;
        protected string CurrentAddress;
        protected QTabItem CurrentTab;
        protected int BandHeight;
        public static int BandHeightSpace = 3;
        protected ShellBrowserEx ShellBrowser;

        protected List<QTabItem> lstActivatedTabs = new List<QTabItem>(0x10);
        protected IntPtr ExplorerHandle;
        protected Dictionary<int, ITravelLogEntry> LogEntryDic = new Dictionary<int, ITravelLogEntry>();
        protected AbstractListView listView = new AbstractListView();
        protected ListViewMonitor listViewManager;

        protected List<ToolStripItem> lstPluginMenuItems_Sys;
        protected List<ToolStripItem> lstPluginMenuItems_Tab;
        protected ITravelLogStg TravelLog;
        public QTTabBarClass.PluginServer pluginServer { get; set; }

        protected bool NavigatedByCode;

        protected bool NowTabsAddingRemoving;
        protected bool NowInTravelLog;
        protected bool NowModalDialogShown;
        protected bool NowOpenedByGroupOpener;
        protected bool NowTabCloned;
        protected bool NowTabCreated;
        protected bool NowTabDragging;
        protected bool NowTopMost;
        protected bool fNavigatedByTabSelection;
        protected int CurrentTravelLogIndex;
        protected int navBtnsFlag;
        // TODO add fields
        protected ToolStripClasses toolStrip;
        protected ToolStripButton buttonBack;
        protected ToolStripButton buttonForward;
        protected ToolStripDropDownButton buttonNavHistoryMenu;
        protected IntPtr TravelToolBarHandle;

        protected void AddToHistory(QTabItem closingTab)
        {
            string currentPath = closingTab.CurrentPath;
            if ((Config.Misc.KeepHistory && !string.IsNullOrEmpty(currentPath)) && !IsSearchResultFolder(currentPath))
            {
                if (QTUtility2.IsShellPathButNotFileSystem(currentPath) && (currentPath.IndexOf("???") == -1))
                {
                    currentPath = currentPath + "???" + closingTab.GetLogHash(true, 0);
                }
                StaticReg.ClosedTabHistoryList.Add(currentPath);
                InstanceManager.ButtonBarBroadcast(bbar => bbar.RefreshButtons(), true);
            }
        }

        private static bool TryCallButtonBar(Action<QTButtonBar> action)
        {
            QTButtonBar bbar = InstanceManager.GetThreadButtonBar();
            if (bbar == null) return false;
            action(bbar);
            return true;
        }

     
        // TODO: Optional params
        protected bool CloseTab(QTabItem closingTab)
        {
            return ((tabControl1.TabCount > 1) && CloseTab(closingTab, false));
        }

        protected void ShowMessageNavCanceled(string failedPath, bool fModal)
        {
            QTUtility2.log("QTTabBarClass ShowMessageNavCanceled: " + failedPath);
            QTUtility2.MakeErrorLog(null, string.Format("Failed navigation: {0}", failedPath));
            MessageForm.Show(ExplorerHandle,
                string.Format(QTUtility.TextResourcesDic["TabBar_Message"][0], failedPath),
                string.Empty,
                MessageBoxIcon.Asterisk,
                0x2710,
                fModal);
        }

        protected void CancelFailedTabChanging(string newPath)
        {
            if (!CloseTab(tabControl1.SelectedTab, true))
            {
                if (tabControl1.TabCount == 1)
                {
                    WindowUtils.CloseExplorer(ExplorerHandle, 2);
                }
                else
                {
                    QTUtility2.log("CancelFailedTabChanging 1");
                    ShowMessageNavCanceled(newPath, false);
                    if (CurrentTab == null)
                    {
                        tabControl1.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                StaticReg.ClosedTabHistoryList.Remove(newPath); // 最近关闭的删除掉.
                if (tabControl1.TabCount == 0)
                {
                    QTUtility2.log("CancelFailedTabChanging 2");
                    ShowMessageNavCanceled(newPath, true);
                    WindowUtils.CloseExplorer(ExplorerHandle, 2);
                }
                else
                {
                    if (CurrentTab == null)
                    {
                        tabControl1.SelectedIndex = 0;
                    }
                    else
                    {
                        tabControl1.SelectTab(CurrentTab);
                    }
                    QTUtility2.log("CancelFailedTabChanging 3");
                    ShowMessageNavCanceled(newPath, false);
                }
            }
        }

        protected bool NavigateToPastSpecialDir(int hash)
        {
            IEnumTravelLogEntry ppenum = null;
            try
            {
                ITravelLogEntry entry2;
                if (TravelLog.EnumEntries(0x31, out ppenum) != 0)
                {
                    goto Label_007C;
                }
            Label_0013:
                do
                {
                    if (ppenum.Next(1, out entry2, 0) != 0)
                    {
                        goto Label_007C;
                    }
                    if (entry2 != LogEntryDic[hash])
                    {
                        goto Label_0057;
                    }
                }
                while (TravelLog.TravelTo(entry2) != 0);
                NowInTravelLog = true;
                CurrentTravelLogIndex++;
                return true;
            Label_0057:
                if (entry2 != null)
                {
                    Marshal.ReleaseComObject(entry2);
                }
                goto Label_0013;
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception);
            }
            finally
            {
                if (ppenum != null)
                {
                    QTUtility2.log("ReleaseComObject ppenum");
                    Marshal.ReleaseComObject(ppenum);
                }
            }
        Label_007C:
            return false;
        }

        /**
        * TODO config to refresh  when tab control selected index changed
        * 当切换标签的时候，配置是否进行刷新
        * 出现异常情况
        * System.NullReferenceException: 未将对象引用设置到对象的实例。
          在 QTTabBarLib.Interop.IShellBrowser.BrowseObject(IntPtr pidl, SBSP wFlags)
          在 QTTabBarLib.ShellBrowserEx.Navigate(IDLWrapper idlw, SBSP flags)
          在 QTTabBarLib.QTTabBarClass.tabControl1_SelectedIndexChanged(Object sender, EventArgs e)
        */
        protected void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            QTabItem selectedTab = tabControl1.SelectedTab;
            string currentPath = selectedTab.CurrentPath;
            if (IsSpecialFolderNeedsToTravel(currentPath) &&
               LogEntryDic.ContainsKey(selectedTab.GetLogHash(true, 0)))
            {
                NavigatedByCode = true;
                CurrentTab = selectedTab;
                QTUtility2.log("tabControl1_SelectedIndexChanged IsSpecialFolderNeedsToTravel");
                while (lstActivatedTabs.Remove(CurrentTab))
                {
                }
                lstActivatedTabs.Add(CurrentTab);
                if (lstActivatedTabs.Count > 15)
                {
                    lstActivatedTabs.RemoveAt(0);
                }
                fNavigatedByTabSelection = NavigateToPastSpecialDir(CurrentTab.GetLogHash(true, 0));
                if (pluginServer != null)
                {
                    pluginServer.OnTabChanged(tabControl1.SelectedIndex, selectedTab.CurrentIDL, selectedTab.CurrentPath);
                }
                if (tabControl1.Focused)
                {
                    listView.SetFocus();
                }
            }
            else
            {
                IDLWrapper idlw = null;
                if ((selectedTab.CurrentIDL != null) && (selectedTab.CurrentIDL.Length > 0))
                {
                    idlw = new IDLWrapper(selectedTab.CurrentIDL);
                }
                if ((idlw == null) || !idlw.Available)
                {
                    idlw = new IDLWrapper(selectedTab.CurrentPath);
                }
                using (idlw)
                {
                    if (!idlw.Available)
                    {
                        QTUtility2.log("tabControl1_SelectedIndexChanged !idlw.Available");
                        CancelFailedTabChanging(currentPath);
                        return;
                    }
                    
                    CurrentTab = selectedTab;
                    while (lstActivatedTabs.Remove(CurrentTab))
                    {
                    }
                    lstActivatedTabs.Add(CurrentTab);
                    if (lstActivatedTabs.Count > 15)
                    {
                        lstActivatedTabs.RemoveAt(0);
                    }
                    
                    if (((currentPath != CurrentAddress) ||
                        (QTUtility.IsXP && (currentPath == QTUtility.PATH_SEARCHFOLDER))) ||
                       NowTabCloned)
                    {
                        NavigatedByCode = true;
                        fNavigatedByTabSelection = true;
                        NowTabCloned = false;
                        // if (ShellBrowser.Navigate(idlw) != 0)
                        // 扩展导航的 ShellBrowser 为空
                        if (explorerBrowser.Navigate(idlw.Path))
                        {
                            QTUtility2.log("tabControl1_SelectedIndexChanged explorerBrowser.Navigate");
                            CancelFailedTabChanging(currentPath);
                            return;
                        }
                        
                    }
                    else
                    {
                        SyncTravelState();
                    }
                }
                if (tabControl1.Focused)
                {
                    listView.SetFocus();
                }
                if (pluginServer != null)
                {
                    pluginServer.OnTabChanged(tabControl1.SelectedIndex, CurrentTab.CurrentIDL, CurrentTab.CurrentPath);
                }
            }
            QTUtility2.log("tabControl1_SelectedIndexChanged end");
        }

        protected void SyncTravelState()
        {
            if (CurrentTab != null)
            {
                navBtnsFlag = ((CurrentTab.HistoryCount_Back > 1) ? 1 : 0) | ((CurrentTab.HistoryCount_Forward > 0) ? 2 : 0);
                if (Config.Tabs.ShowNavButtons && (toolStrip != null))
                {
                    buttonBack.Enabled = (navBtnsFlag & 1) != 0;
                    buttonForward.Enabled = (navBtnsFlag & 2) != 0;
                    buttonNavHistoryMenu.Enabled = navBtnsFlag != 0;
                }
                TryCallButtonBar(bbar => bbar.RefreshButtons());
                QTabItem.CheckSubTexts(tabControl1);
                SyncToolbarTravelButton();
            }
        }

        private void SyncToolbarTravelButton()
        {
            if (!QTUtility.IsXP)
            {
                IntPtr ptr = (IntPtr)0x10001;
                IntPtr ptr2 = (IntPtr)0x10000;
                bool flag = (navBtnsFlag & 1) != 0;
                bool flag2 = (navBtnsFlag & 2) != 0;
                PInvoke.SendMessage(TravelToolBarHandle, 0x401, (IntPtr)0x100, flag ? ptr : ptr2);
                PInvoke.SendMessage(TravelToolBarHandle, 0x401, (IntPtr)0x101, flag2 ? ptr : ptr2);
                PInvoke.SendMessage(TravelToolBarHandle, 0x401, (IntPtr)0x102, (flag || flag2) ? ptr : ptr2);
            }
        }

        protected bool IsSpecialFolderNeedsToTravel(string path)
        {
            int index = path.IndexOf("*?*?*");
            if (index != -1)
            {
                path = path.Substring(0, index);
            }
            if (!IsSearchResultFolder(path))
            {
                if (path.PathEquals("::{13E7F612-F261-4391-BEA2-39DF4F3FA311}"))
                {
                    return true;
                }
                if (!path.PathStartsWith(QTUtility.ResMisc[0]) &&
                    (
                        !path.EndsWith(QTUtility.ResMisc[0], StringComparison.OrdinalIgnoreCase) 
                         || 
                         Path.IsPathRooted(path)
                     ))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsSearchResultFolder(string path)
        {
            return path.PathStartsWith(QTUtility.IsXP ? QTUtility.ResMisc[2] : QTUtility.PATH_SEARCHFOLDER);
        }

        protected void tabControl1_RowCountChanged(object sender, QEventArgs e)
        {
            SetBarRows(e.RowCount);
        }


        protected void SetBarRows(int count)
        {
            QTUtility2.log("QTTabBarClass SetBarRows");
            // BandHeight = (count * (Config.Skin.TabHeight - 3 )) ;
            // BandHeight = (count * (Config.Skin.TabHeight - 3));
            // BandHeight = (count * (Config.Skin.TabHeight + BandHeightSpace));
            BandHeight = (count * (Config.Skin.TabHeight) + BandHeightSpace);
            // BandHeight = (count * (Config.Skin.TabHeight + 10 )) ;
            // fix bug
            /**
           异常文本
System.NullReferenceException: 未将对象引用设置到对象的实例。
   在 QTTabBarLib.QTTabBarClass.SetBarRows(Int32 count)
   在 QTTabBarLib.QTabControl.CalculateItemRectangle_MultiRows()
   在 QTTabBarLib.QTabControl.OnPaint_MultipleRow(PaintEventArgs e)
   在 QTTabBarLib.QTabControl.OnPaint(PaintEventArgs e)
   在 System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)
   在 System.Windows.Forms.Control.WmPaint(Message& m)
   在 System.Windows.Forms.Control.WndProc(Message& m)
   在 QTTabBarLib.QTabControl.WndProc(Message& m)
   在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
   在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
             **/
            if (null != rebarController)
            {
                rebarController.RefreshHeight();
            }
        }

        protected void tabControl1_Deselecting(object sender, QTabCancelEventArgs e)
        {
            if (e.TabPageIndex != -1)
            {
                SaveSelectedItems(e.TabPage);
            }
        }

        /**
         * 保存选中项
         */
        protected void SaveSelectedItems(QTabItem tab)
        {
            Address[] addressArray;
            string str;
            if (
                ((tab != null) && !string.IsNullOrEmpty(CurrentAddress)) &&
                ShellBrowser.TryGetSelection(out addressArray, out str, false, ShellBrowser))
            {
                if (addressArray != null && addressArray.Length > 0)
                {
                    QTUtility2.log("SaveSelectedItems addressArray " + addressArray[0].Path);
                }
                tab.SetSelectedItemsAt(CurrentAddress, addressArray, str);
            }
        }


        protected void tabControl1_Selecting(object sender, QTabCancelEventArgs e)
        {
            if (NowTabsAddingRemoving)
            {
                QTUtility2.log("tabControl1_Selecting");
                e.Cancel = true;
            }
        }



        // 新增+号按钮的添加新标签事件
        private void 
            tabControl1_PlusButtonClicked(object sender, QTabCancelEventArgs e)
        {
            // 新标签按钮 qwop
            string clipPath = QTUtility2.GetStringClipboard();
            if (String.IsNullOrEmpty(clipPath))
            {
                // 如果获取不到剪贴板路径，则打开默认
                openDefault();
                return;
            }
            clipPath = clipPath.Trim().Trim(new char[] { ' ', '"' });
            string[] pathArr = { "a:\\", "b:\\", "c:\\", "d:\\", "e:\\", "f:\\", "g:\\", "h:\\", "i:\\" };
            bool blockSelecting = false, fForceNew = true;
            // 如果剪贴板是一个文件路径，并且存在则打开父级目录或者跟级目录
            if (File.Exists(clipPath))
            {
                try
                {
                    QTUtility2.log("tabControl1_PlusButtonClicked file exist " + clipPath);
                    string pathRoot = Path.GetPathRoot(clipPath);
                    DirectoryInfo di = new DirectoryInfo(clipPath);
                    if (Directory.Exists(di.Parent.FullName))
                    {
                        OpenNewTab(di.Parent.FullName, blockSelecting, fForceNew);

                    }
                    else
                    {
                        OpenNewTab(pathRoot, blockSelecting, fForceNew);
                    }

                    string selectMe = Path.GetFileName(clipPath);
                    ShellBrowser.TrySetSelection(new Address[] { new Address(selectMe) }, null, true);
                }
                catch (Exception e1)
                {
                    QTUtility2.MakeErrorLog(e1, "tabControl1_PlusButtonClicked for file:" + clipPath);
                    // openDefault();
                }
            }
            else if (Directory.Exists(clipPath)) // 剪贴板直接是一个目录则打开标签
            {
                try
                {
                    QTUtility2.log("tabControl1_PlusButtonClicked Directory exist " + clipPath);
                    OpenNewTab(clipPath, blockSelecting, fForceNew);
                    return;
                }
                catch (Exception e1)
                {
                    QTUtility2.MakeErrorLog(e1, "tabControl1_PlusButtonClicked for director :" + clipPath);
                    // openDefault();
                }
            }
            else
            {

                // 打开指定盘符目录
                /*
                for ( int i = 0; i < pathArr.Length; i++ )
                {
                    if (Directory.Exists(pathArr[i]))
                    {
                        OpenNewTab(pathArr[i], blockSelecting, fForceNew);
                        break;
                    }
                }
                */
                openDefault();
            }
        }

        private void openDefault()
        {
            bool isOpend = false;
            // 打开配置的路径地址
            using (IDLWrapper wrapper = new IDLWrapper(Config.Window.DefaultLocation))
            {
                QTUtility2.log("tabControl1_PlusButtonClicked others default " + Config.Window.DefaultLocation);
                OpenNewTab(wrapper, false, true);
                isOpend = true;
            }

            if (!isOpend)
            {
                string idl = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // 我的电脑， 默认打开
                using (IDLWrapper w = new IDLWrapper(idl))
                {
                    QTUtility2.log("tabControl1_PlusButtonClicked 我的电脑 ");
                    OpenNewTab(w, false, true);
                }
            }
        }

        private bool OpenNewTab(string path, bool blockSelecting = false, bool fForceNew = false)
        {
            using (IDLWrapper wrapper = new IDLWrapper(path))
            {
                if (wrapper.Available)
                {
                    return OpenNewTab(wrapper, blockSelecting, fForceNew);
                }
            }
            return false;
        }


        internal bool OpenNewTab(IDLWrapper idlwGiven, bool blockSelecting = false, bool fForceNew = false)
        {
            // Check that the folder exists and is navigable.
            if (idlwGiven == null || !idlwGiven.Available || !idlwGiven.HasPath || !idlwGiven.IsReadyIfDrive || idlwGiven.IsLinkToDeadFolder)
            {
                QTUtility.SoundPlay();
                return false;
            }

            // If the IDL is a link, resolve it.  Otherwise keep using the one we're given.
            using (IDLWrapper idlwLink = idlwGiven.ResolveTargetIfLink())
            {
                IDLWrapper idlw = idlwLink ?? idlwGiven;

                // Recheck a few things
                if (!idlw.Available || !idlw.HasPath || !idlw.IsReadyIfDrive || !idlw.IsFolder)
                {
                    QTUtility.SoundPlay();
                    return false;
                }

                if (blockSelecting)
                {
                    NowTabsAddingRemoving = true;
                }
                try
                {
                    // Check if it's already open
                    if (!fForceNew && Config.Tabs.NeverOpenSame)
                    {
                        QTabItem tabPage = tabControl1.TabPages.FirstOrDefault(
                                item2 => item2.CurrentPath.PathEquals(idlw.Path));
                        if (tabPage != null)
                        {
                            if (Config.Tabs.ActivateNewTab)
                            {
                                tabControl1.SelectTab(tabPage);
                            }
                            TryCallButtonBar(bbar => bbar.RefreshButtons());
                            return false;
                        }
                    }

                    // TODO
                    // This entire block is a mystery to me, and I think it should be
                    // removed. It's gone in Quizo's version.
                    string path = idlw.Path;
                    if (!idlw.Special && !path.StartsWith("::"))
                    {
                        string directoryName = Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            using (IDLWrapper wrapper = new IDLWrapper(directoryName))
                            {
                                if (wrapper.Special && idlw.Available)
                                {
                                    IShellFolder ppv = null;
                                    try
                                    {
                                        IntPtr ptr;
                                        if (PInvoke.SHBindToParent(idlw.PIDL, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr) == 0)
                                        {
                                            using (IDLWrapper wrapper2 = new IDLWrapper(PInvoke.ILCombine(wrapper.PIDL, ptr)))
                                            {
                                                if (wrapper2.Available && wrapper2.HasPath)
                                                {
                                                    if (!blockSelecting && Config.Tabs.ActivateNewTab)
                                                    {
                                                        NowTabCreated = true;
                                                        tabControl1.SelectTab(CreateNewTab(wrapper2));
                                                    }
                                                    else
                                                    {
                                                        CreateNewTab(wrapper2);
                                                        TryCallButtonBar(bbar => bbar.RefreshButtons());
                                                        QTabItem.CheckSubTexts(tabControl1);
                                                    }
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    finally
                                    {
                                        if (ppv != null)
                                        {
                                            QTUtility2.log("ReleaseComObject ppv");
                                            Marshal.ReleaseComObject(ppv);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // This should work for everything...
                    if (!blockSelecting && Config.Tabs.ActivateNewTab)
                    {
                        NowTabCreated = true;
                        tabControl1.SelectTab(CreateNewTab(idlw));
                    }
                    else
                    {
                        CreateNewTab(idlw);
                        TryCallButtonBar(bbar => bbar.RefreshButtons());
                        QTabItem.CheckSubTexts(tabControl1);
                    }
                }
                finally
                {
                    if (blockSelecting)
                    {
                        NowTabsAddingRemoving = false;
                    }
                }
            }
            return true;
        }


        // 创建新的tab页
        private QTabItem CreateNewTab(IDLWrapper idlw)
        {
            string path = idlw.Path;
            QTabItem tab = new QTabItem(QTUtility2.MakePathDisplayText(path, false), path, tabControl1);
            tab.NavigatedTo(path, idlw.IDL, -1, false);
            tab.ToolTipText = QTUtility2.MakePathDisplayText(path, true);
            AddInsertTab(tab);
            return tab;
        }


        private void AddInsertTab(QTabItem tab)
        {
            QTUtility2.log("QTTabBarClass AddInsertTab  ");
            switch (Config.Tabs.NewTabPosition)
            {
                case TabPos.Leftmost:
                    tabControl1.TabPages.Insert(0, tab);
                    break;

                case TabPos.Right:
                case TabPos.Left:
                {
                    int index = tabControl1.TabPages.IndexOf(CurrentTab);
                    if (index == -1)
                    {
                        tabControl1.TabPages.Add(tab);
                    }
                    else
                    {
                        tabControl1.TabPages.Insert(Config.Tabs.NewTabPosition == TabPos.Right ? (index + 1) : index, tab);
                    }
                    break;
                }

                default: // TabPos.Rightmost
                    tabControl1.TabPages.Add(tab);
                    break;
            }
        }
        #endregion
    }
}
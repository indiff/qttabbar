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
using System.Media;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BandObjectLib;
using Microsoft.Win32;
using QTTabBarLib.Interop;
using IShellBrowser = QTTabBarLib.Interop.IShellBrowser;
using MSG = BandObjectLib.MSG;
using Timer = System.Windows.Forms.Timer;


namespace QTTabBarLib {
    [ComVisible(true)]
    [Guid("D2BF470E-ED1C-487F-A555-2BD8835EB6CE")]
    public sealed class QTDesktopTool : BandObject, IDeskBand2 {
        // FUNCTIONS  									  |	THREAD		
        // -----------------------------------------------+-----------
        // taskbar toolbar								  | taskbar
        // Menu											  | taskbar (even if shown by double-click on destop)
        // *subDirTip_TB	(real directory menu in menu) |	taskbar
        //
        // Sub Directory Menu							  |	desktop
        // Thumbnail for desktop item					  |	desktop


        #region ---------- Fields ----------

        // PLATFORM INVOKE & INTEROPS
        private IntPtr hHook_MsgDesktop;
        private IntPtr hHook_MsgShell_TrayWnd;
        private IntPtr hHook_KeyDesktop;
        private HookProc hookProc_Msg_Desktop;
        private HookProc hookProc_Msg_ShellTrayWnd;
        private HookProc hookProc_Keys_Desktop;

        private IntPtr hwndShellTray, hwndThis;
        private ExtendedSysListView32 slvDesktop;

        private ShellContextMenu iContextMenu2 = new ShellContextMenu(); // for taskbar thread, events handled at WndProc
        private ShellBrowserEx ShellBrowser;

        private NativeWindowController shellViewListener;


        // CONTROLS
        private System.ComponentModel.IContainer components;
        private DropDownMenuReorderable contextMenu, ddmrGroups, ddmrHistory, ddmrUserapps, ddmrRecentFile;
        private TitleMenuItem tmiLabel_Group, tmiLabel_History, tmiLabel_UserApp, tmiLabel_RecentFile;
        private TitleMenuItem tmiGroup, tmiHistory, tmiUserApp, tmiRecentFile;

        private Timer timerHooks;
        private int iHookTimeout;

        private List<ToolStripItem> lstGroupItems = new List<ToolStripItem>();
        private List<ToolStripItem> lstUndoClosedItems = new List<ToolStripItem>();
        private List<ToolStripItem> lstUserAppItems = new List<ToolStripItem>();
        private List<ToolStripItem> lstRecentFileItems = new List<ToolStripItem>();

        private ContextMenuStripEx contextMenuForSetting;

        private ToolStripMenuItem tsmiTaskBar,
                                  tsmiDesktop,
                                  tsmiLockItems,
                                  tsmiVSTitle,
                                  tsmiOnGroup,
                                  tsmiOnHistory,
                                  tsmiOnUserApps,
                                  tsmiOnRecentFile,
                                  tsmiOneClick,
                                  tsmiAppKeys;

        private ToolStripMenuItem tsmiExperimental;

        private ThumbnailTooltipForm thumbnailTooltip;
        private Timer timer_Thumbnail, timer_ThumbnailMouseHover, timer_Thumbnail_NoTooltip;

        private SubDirTipForm subDirTip, subDirTip_TB;
        private ShellContextMenu iContextMenu2_Desktop = new ShellContextMenu(); // for desktop thread, handled at shellViewListener_MessageCaptured
        private int thumbnailIndex = -1;
        private int itemIndexDROPHILITED = -1;
        private int thumbnailIndex_Inactive = -1;
        private Timer timer_HoverSubDirTipMenu;

        private VisualStyleRenderer bgRenderer;
        private StringFormat stringFormat;


        // SETTINGS & FLAGS		
        private bool[] ExpandState = {false, false, false, false};
        private UniqueList<int> lstItemOrder = new UniqueList<int> {0, 1, 2, 3};
        private List<bool> lstRefreshRequired = new List<bool> { true, true, true, true };

        private bool fCancelClosing;
        private bool fNowMouseHovering;


        // CONSTANTS
        private const int ITEMTYPE_COUNT = 4;
        private const int ITEMINDEX_GROUP = 0;
        private const int ITEMINDEX_RECENTTAB = 1;
        private const int ITEMINDEX_APPLAUNCHER = 2;
        private const int ITEMINDEX_RECENTFILE = 3;


        private const string TEXT_TOOLBAR = "QTTab Desktop Tool";
        private const string MENUKEY_LABEL_GROUP = "labelG";
        private const string MENUKEY_LABEL_HISTORY = "labelH";
        private const string MENUKEY_LABEL_USERAPP = "labelU";
        private const string MENUKEY_LABEL_RECENT = "labelR";

        private const string MENUKEY_ITEM_GROUP = "groupItem";
        private const string MENUKEY_ITEM_HISTORY = "historyItem";
        private const string MENUKEY_ITEM_USERAPP = "userappItem";
        private const string MENUKEY_ITEM_RECENT = "recentItem";

        private const string MENUKEY_SUBMENUS = "submenu";
        private const string MENUKEY_LABELS = "label";

        private const string TSS_NAME_GRP = "groupSep";
        private const string TSS_NAME_APP = "appSep";


        private const string CLSIDSTR_TRASHBIN = "::{645FF040-5081-101B-9F08-00AA002F954E}";

        #endregion



        public QTDesktopTool() {
            // Methods are called in this order:
            // ctor -> SetSite -> InitializeComponent -> 
            // (touches Handle property, WM_CREATE) -> OnHandleCreated -> (OnVisibleChanged)
            QTUtility.Initialize();
        }


        private void InitializeComponent() {
            // handle creation
            // todo: doesn't look necessary...
            hwndThis = Handle;

            bool reorderEnabled = !Config.Desktop.LockMenu;

            components = new System.ComponentModel.Container();
            contextMenu = new DropDownMenuReorderable(components, true, false);
            contextMenuForSetting = new ContextMenuStripEx(components, true); // todo: , false);
            tmiLabel_Group = new TitleMenuItem(MenuGenre.Group, true);
            tmiLabel_History = new TitleMenuItem(MenuGenre.History, true);
            tmiLabel_UserApp = new TitleMenuItem(MenuGenre.Application, true);
            tmiLabel_RecentFile = new TitleMenuItem(MenuGenre.RecentFile, true);

            contextMenu.SuspendLayout();
            contextMenuForSetting.SuspendLayout();
            SuspendLayout();
            //
            // contextMenu
            //
            contextMenu.ProhibitedKey.Add(MENUKEY_ITEM_HISTORY);
            contextMenu.ProhibitedKey.Add(MENUKEY_ITEM_RECENT);
            contextMenu.ReorderEnabled = reorderEnabled;
            contextMenu.MessageParent = Handle;
            contextMenu.ImageList = QTUtility.ImageListGlobal;
            contextMenu.ItemClicked += dropDowns_ItemClicked;
            contextMenu.Closing += contextMenu_Closing;
            contextMenu.ReorderFinished += contextMenu_ReorderFinished;
            contextMenu.ItemRightClicked += dropDowns_ItemRightClicked;
            if(!QTUtility.IsXP) {
                contextMenu.CreateControl();
            }
            //
            // ddmrGroups 
            //
            ddmrGroups = new DropDownMenuReorderable(components, true, false);
            ddmrGroups.ReorderEnabled = reorderEnabled;
            ddmrGroups.ImageList = QTUtility.ImageListGlobal;
            ddmrGroups.ReorderFinished += dropDowns_ReorderFinished;
            ddmrGroups.ItemClicked += dropDowns_ItemClicked;
            ddmrGroups.ItemRightClicked += dropDowns_ItemRightClicked;
            //
            // tmiGroup 
            //
            tmiGroup = new TitleMenuItem(MenuGenre.Group, false) {DropDown = ddmrGroups};
            //
            // ddmrHistory
            //
            ddmrHistory = new DropDownMenuReorderable(components, true, false);
            ddmrHistory.ReorderEnabled = false;
            ddmrHistory.ImageList = QTUtility.ImageListGlobal;
            ddmrHistory.MessageParent = Handle;
            ddmrHistory.ItemClicked += dropDowns_ItemClicked;
            ddmrHistory.ItemRightClicked += dropDowns_ItemRightClicked;
            //
            // tmiHistory 
            //
            tmiHistory = new TitleMenuItem(MenuGenre.History, false);
            tmiHistory.DropDown = ddmrHistory;
            //
            // ddmrUserapps 
            //
            ddmrUserapps = new DropDownMenuReorderable(components);
            ddmrUserapps.ReorderEnabled = reorderEnabled;
            ddmrUserapps.ImageList = QTUtility.ImageListGlobal;
            ddmrUserapps.MessageParent = Handle;
            ddmrUserapps.ReorderFinished += dropDowns_ReorderFinished;
            ddmrUserapps.ItemClicked += dropDowns_ItemClicked;
            ddmrUserapps.ItemRightClicked += dropDowns_ItemRightClicked;
            //
            // tmiUserApp
            //
            tmiUserApp = new TitleMenuItem(MenuGenre.Application, false);
            tmiUserApp.DropDown = ddmrUserapps;
            //
            // ddmrRecentFile
            //
            ddmrRecentFile = new DropDownMenuReorderable(components, false, false, false);
            ddmrRecentFile.ImageList = QTUtility.ImageListGlobal;
            ddmrRecentFile.MessageParent = Handle;
            ddmrRecentFile.ItemClicked += dropDowns_ItemClicked;
            ddmrRecentFile.ItemRightClicked += dropDowns_ItemRightClicked;
            //
            // tmiRecentFile
            //
            tmiRecentFile = new TitleMenuItem(MenuGenre.RecentFile, false);
            tmiRecentFile.DropDown = ddmrRecentFile;
            //
            // contextMenuForSetting
            //
            tsmiTaskBar = new ToolStripMenuItem();
            tsmiDesktop = new ToolStripMenuItem();
            tsmiLockItems = new ToolStripMenuItem();
            tsmiVSTitle = new ToolStripMenuItem();
            tsmiTaskBar.Checked = Config.Desktop.TaskBarDblClickEnabled;
            tsmiDesktop.Checked = Config.Desktop.DesktopDblClickEnabled;
            tsmiLockItems.Checked = Config.Desktop.LockMenu;
            tsmiVSTitle.Checked = Config.Desktop.TitleBackground;

            tsmiOnGroup = new ToolStripMenuItem();
            tsmiOnHistory = new ToolStripMenuItem();
            tsmiOnUserApps = new ToolStripMenuItem();
            tsmiOnRecentFile = new ToolStripMenuItem();
            tsmiOneClick = new ToolStripMenuItem();
            tsmiAppKeys = new ToolStripMenuItem();
            tsmiOnGroup.Checked = Config.Desktop.IncludeGroup;
            tsmiOnHistory.Checked = Config.Desktop.IncludeRecentTab;
            tsmiOnUserApps.Checked = Config.Desktop.IncludeApplication;
            tsmiOnRecentFile.Checked = Config.Desktop.IncludeRecentFile;
            tsmiOneClick.Checked = Config.Desktop.OneClickMenu;
            tsmiAppKeys.Checked = Config.Desktop.EnableAppShortcuts;
            
            tsmiExperimental = new ToolStripMenuItem(QTUtility.TextResourcesDic["Misc_Strings"][6]);
            tsmiExperimental.DropDown.Items.Add(new ToolStripMenuItem(QTUtility.TextResourcesDic["Misc_Strings"][7]));
            //TODO does this respect RTL settings?
            tsmiExperimental.DropDownDirection = ToolStripDropDownDirection.Left;
            tsmiExperimental.DropDownItemClicked += tsmiExperimental_DropDownItemClicked;
            tsmiExperimental.DropDownOpening += tsmiExperimental_DropDownOpening;

            contextMenuForSetting.Items.AddRange(new ToolStripItem[] {
                    tsmiTaskBar, tsmiDesktop, new ToolStripSeparator(),
                    tsmiOnGroup, tsmiOnHistory, tsmiOnUserApps, tsmiOnRecentFile,
                    new ToolStripSeparator(),
                    tsmiLockItems, tsmiVSTitle, tsmiOneClick, tsmiAppKeys, tsmiExperimental
            });
            contextMenuForSetting.ItemClicked += contextMenuForSetting_ItemClicked;
            RefreshStringResources();

            //
            // QTCoTaskBar
            //
            ContextMenuStrip = contextMenuForSetting;
            Width = Config.Desktop.Width;
            MinSize = new Size(8, 22);
            Dock = DockStyle.Fill;
            MouseClick += desktopTool_MouseClick;
            MouseDoubleClick += desktopTool_MouseDoubleClick;

            contextMenu.ResumeLayout(false);
            contextMenuForSetting.ResumeLayout(false);
            ResumeLayout(false);
        }

        #region ---------- Overriding Methods ----------

        public override void SetSite(object pUnkSite) {
            if(BandObjectSite != null) {
                Marshal.ReleaseComObject(BandObjectSite);
            }
            BandObjectSite = (IInputObjectSite)pUnkSite;
            Application.EnableVisualStyles();

            ReadSetting();
            InitializeComponent();
            InstallDesktopHook();

            TitleMenuItem.DrawBackground = tsmiVSTitle.Checked;
        }

        public override void CloseDW(uint dwReserved) {
            // called when the user disables the Desktop Tool
            // this seems not to be called on log off / shut down...

            if(iContextMenu2 != null) {
                Marshal.ReleaseComObject(iContextMenu2);
                iContextMenu2 = null;
            }

            // dispose controls in the thread they're created.
            if(thumbnailTooltip != null) {
                thumbnailTooltip.Invoke(d => d.Dispose());
                thumbnailTooltip = null;
            }
            if(subDirTip != null) {
                subDirTip.Invoke(d => d.Dispose());
                subDirTip = null;
            }

            // unhook, unsubclass
            if(hHook_MsgDesktop != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(hHook_MsgDesktop);
                hHook_MsgDesktop = IntPtr.Zero;
            }

            if(hHook_MsgShell_TrayWnd != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(hHook_MsgShell_TrayWnd);
                hHook_MsgShell_TrayWnd = IntPtr.Zero;
            }

            if(hHook_KeyDesktop != IntPtr.Zero) {
                PInvoke.UnhookWindowsHookEx(hHook_KeyDesktop);
                hHook_KeyDesktop = IntPtr.Zero;
            }

            if(shellViewListener != null) {
                shellViewListener.ReleaseHandle();
                shellViewListener = null;
            }

            base.CloseDW(dwReserved);
        }

        public override void GetClassID(out Guid pClassID) {
            pClassID = typeof(QTDesktopTool).GUID;
        }

        protected override void WndProc(ref Message m) {
            const int MA_NOACTIVATEANDEAT = 4;
            const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

            switch(m.Msg) {
                case WM.INITMENUPOPUP:
                case WM.DRAWITEM:
                case WM.MEASUREITEM:

                    // these messages are forwarded to draw sub items in 'Send to" of shell context menu.
                    if(iContextMenu2 != null) {
                        iContextMenu2.TryHandleMenuMsg(m.Msg, m.WParam, m.LParam);
                        return;
                    }
                    break;


                case WM.MOUSEACTIVATE:
                    if(Config.Desktop.OneClickMenu) {
                        if(m.LParam.HiWord() == WM.LBUTTONDOWN) {
                            if(contextMenu.Visible) {
                                contextMenu.Close(ToolStripDropDownCloseReason.AppClicked);
                                m.Result = (IntPtr)MA_NOACTIVATEANDEAT;
                                return;
                            }
                        }
                    }
                    break;

                case WM_DWMCOMPOSITIONCHANGED:
                    Invalidate();
                    break;

                // TODO: UpdateConfig should call this through invoke, if it's even necessary.
                /*
                case MC.QTDT_REFRESH_TEXTRES:
                    RefreshStringResources();
                    break;*/
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            // background
            if(VisualStyleRenderer.IsSupported) {
                if(bgRenderer == null) {
                    bgRenderer = new VisualStyleRenderer(VisualStyleElement.Taskbar.BackgroundTop.Normal);
                }
                bgRenderer.DrawParentBackground(e.Graphics, e.ClipRectangle, this);
            }
            else {
                base.OnPaintBackground(e);
            }

            // strings
            if(!fNowMouseHovering) return;
            Color clr = VisualStyleRenderer.IsSupported ? SystemColors.Window : SystemColors.WindowText;

            stringFormat = stringFormat ?? new StringFormat {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };
            
            using(SolidBrush sb = new SolidBrush(Color.FromArgb(128, clr))) {
                e.Graphics.DrawString(TEXT_TOOLBAR, Font, sb,
                        new Rectangle(0, 5, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 6),
                        stringFormat);
            }

            using(Pen p = new Pen(Color.FromArgb(128, clr))) {
                e.Graphics.DrawRectangle(p,
                        new Rectangle(0, 2, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 3));
            }
        }

        protected override void OnMouseEnter(EventArgs e) {
            fNowMouseHovering = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) {
            fNowMouseHovering = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        #endregion

        #region ---------- Hooks and subclassings ----------


        private void InstallDesktopHook() {
            const int WH_KEYBOARD = 2;
            const int WH_GETMESSAGE = 3;

            IntPtr hwndDesktop = GetDesktopHwnd();
            if(timerHooks == null) {
                if(hwndDesktop == IntPtr.Zero) {
                    // wait till desktop window is created 
                    timerHooks = new Timer();
                    timerHooks.Tick += (sender, args) => {
                        if(++iHookTimeout > 5) {
                            timerHooks.Stop();
                            MessageBox.Show(
                                QTUtility.TextResourcesDic["ErrorDialogs"][8]
                            );
                            return;
                        }
                        InstallDesktopHook();
                    };
                    timerHooks.Interval = 3000;
                    timerHooks.Start();
                    return;
                }
            }
            else {
                if(hwndDesktop == IntPtr.Zero) {
                    return;
                }
                else {
                    timerHooks.Stop();
                    timerHooks.Dispose();
                    timerHooks = null;
                }
            }

            // Now we've got desktop window handle
            hwndShellTray = WindowUtils.GetShellTrayWnd();

            hookProc_Msg_Desktop = CallbackGetMsgProc_Desktop;
            hookProc_Msg_ShellTrayWnd = CallbackGetMsgProc_ShellTrayWnd;
            hookProc_Keys_Desktop = CallbackKeyProc_Desktop;

            uint id1, id2;
            int threadID_Desktop = PInvoke.GetWindowThreadProcessId(hwndDesktop, out id1);
            int threadID_ShellTray = PInvoke.GetWindowThreadProcessId(hwndShellTray, out id2);

            hHook_MsgDesktop = PInvoke.SetWindowsHookEx(WH_GETMESSAGE, hookProc_Msg_Desktop, IntPtr.Zero,
                    threadID_Desktop);
            hHook_MsgShell_TrayWnd = PInvoke.SetWindowsHookEx(WH_GETMESSAGE, hookProc_Msg_ShellTrayWnd,
                    IntPtr.Zero, threadID_ShellTray);
            hHook_KeyDesktop = PInvoke.SetWindowsHookEx(WH_KEYBOARD, hookProc_Keys_Desktop, IntPtr.Zero,
                    threadID_Desktop);

            // get IFolderView on the desktop thread...
            // todo: quizo claims this must be done on the desktop thread.  I disagree.  test.
            GetFolderView();

            // subclassing ShellView
            // todo: hmm, is this allowed here?  I would think you'd have to do this in the desktop's thread.
            const int GWL_HWNDPARENT = -8; // todo: constify
            IntPtr hwndShellView = PInvoke.GetWindowLongPtr(hwndDesktop, GWL_HWNDPARENT);
            slvDesktop = new ExtendedSysListView32(ShellBrowser, hwndShellView, hwndDesktop, hwndThis);
            slvDesktop.SelectionActivated += ListView_SelectionActivated;
            slvDesktop.MiddleClick += ListView_MiddleClick;
            slvDesktop.MouseActivate += ListView_MouseActivate;
            slvDesktop.SubDirTip_MenuItemClicked += subDirTip_MenuItemClicked;
            slvDesktop.SubDirTip_MenuItemRightClicked += subDirTip_MenuItemRightClicked;
            slvDesktop.SubDirTip_MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
            slvDesktop.SubDirTip_MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
        }

        private void GetFolderView() {
            // desktop thread
            const int SWC_DESKTOP = 0x00000008;
            const int SWFO_NEEDDISPATCH = 0x00000001;

            SHDocVw.ShellWindows shellWindows = null;
            try {
                shellWindows = new SHDocVw.ShellWindows();
                object oNull1 = null, oNull2 = null;
                int pHWND;
                object o = shellWindows.FindWindowSW(ref oNull1, ref oNull2, SWC_DESKTOP, out pHWND, SWFO_NEEDDISPATCH);

                _IServiceProvider sp = o as _IServiceProvider;
                if(sp == null) return;
                object oShellBrowser;
                sp.QueryService(ExplorerGUIDs.IID_IShellBrowser, ExplorerGUIDs.IID_IUnknown, out oShellBrowser);
                ShellBrowser = new ShellBrowserEx(oShellBrowser as IShellBrowser);
            }
            catch {
            }
            finally {
                if(shellWindows != null) {
                    Marshal.ReleaseComObject(shellWindows);
                }
            }
        }

        // todo
        private bool shellViewListener_MessageCaptured(ref Message msg) {
            switch(msg.Msg) {
                case WM.INITMENUPOPUP:
                case WM.DRAWITEM:
                case WM.MEASUREITEM:

                    // these messages are forwarded to draw sub items in 'Send to" of shell context menu on SubDirTip menu.

                    if(iContextMenu2_Desktop != null) {
                        iContextMenu2_Desktop.TryHandleMenuMsg(msg.Msg, msg.WParam, msg.LParam);
                        return true;
                    }
                    break;
            }
            return false;
        }

        private IntPtr CallbackGetMsgProc_Desktop(int nCode, IntPtr wParam, IntPtr lParam) {
            const int WM_LBUTTONDBLCLK = 0x0203;
            //const int WM_MBUTTONUP = 0x0208;
            const int WM_MOUSEWHEEL = 0x020A;

            if(nCode >= 0) {
                MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                switch(msg.message) {
                    case WM_MOUSEWHEEL:

                        // redirect mouse wheel to menu
                        IntPtr hwnd = PInvoke.WindowFromPoint(QTUtility2.PointFromLPARAM(msg.lParam));
                        if(hwnd != IntPtr.Zero && hwnd != msg.hwnd) {
                            Control ctrl = FromHandle(hwnd);
                            if(ctrl != null) {
                                DropDownMenuReorderable ddmr = ctrl as DropDownMenuReorderable;
                                if(ddmr != null) {
                                    if(ddmr.CanScroll) {
                                        PInvoke.SendMessage(hwnd, WM_MOUSEWHEEL, msg.wParam, msg.lParam);
                                    }
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                            }
                        }
                        break;


                    case WM_LBUTTONDBLCLK:

                        if(msg.hwnd == slvDesktop.Handle && Config.Desktop.DesktopDblClickEnabled) {
                            int index = PInvoke.ListView_HitTest(slvDesktop.Handle, msg.lParam);
                            if(index == -1) {
                                // do the menu on Taskbar thread.
                                this.Invoke(() => {
                                    int x = QTUtility2.GET_X_LPARAM(msg.lParam);
                                    int y = QTUtility2.GET_Y_LPARAM(msg.lParam);
                                    PInvoke.SetForegroundWindow(hwndShellTray);
                                    ShowMenu(new Point(x, y));
                                });
                            }
                        }
                        break;

                        // todo: why would this be needed?
                        /*
                    case WM_MBUTTONUP:

                        if(msg.hwnd == slvDesktop.Handle && Config.Positive(Scts.ViewIconMiddleClicked)) {
                            int iItem = PInvoke.ListView_HitTest(hwndListView, msg.lParam);

                            if(iItem != -1) {
                                if(HandleTabFolderActions(iItem, ModifierKeys, false)) {
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                            }
                        }
                        break;*/


                        /* todo: as I said, shouldn't be necessary, but test.
                    case MC.QTDT_GETFOLDERVIEW:

                        if(msg.hwnd == hwndListView) {
                            GetFolderView();
                        }
                        break;*/
                }
            }

            return PInvoke.CallNextHookEx(hHook_MsgDesktop, nCode, wParam, lParam);
        }

        private IntPtr CallbackGetMsgProc_ShellTrayWnd(int nCode, IntPtr wParam, IntPtr lParam) {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;
            const int WM_MOUSEWHEEL = 0x020A;

            if(nCode >= 0) {
                MSG msg = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));

                switch(msg.message) {
                    case WM_NCLBUTTONDBLCLK:
                        if(Config.Desktop.TaskBarDblClickEnabled && msg.hwnd == hwndShellTray) {
                            ShowMenu(MousePosition);
                            Marshal.StructureToPtr(new MSG(), lParam, false);
                        }
                        break;

                    case WM_MOUSEWHEEL:
                        IntPtr hwnd =
                                PInvoke.WindowFromPoint(new Point(QTUtility2.GET_X_LPARAM(msg.lParam),
                                        QTUtility2.GET_Y_LPARAM(msg.lParam)));
                        if(hwnd != IntPtr.Zero && hwnd != msg.hwnd) {
                            Control ctrl = FromHandle(hwnd);
                            if(ctrl != null) {
                                DropDownMenuReorderable ddmr = ctrl as DropDownMenuReorderable;
                                if(ddmr != null && ddmr.CanScroll) {
                                    PInvoke.SendMessage(hwnd, WM_MOUSEWHEEL, msg.wParam, msg.lParam);
                                    Marshal.StructureToPtr(new MSG(), lParam, false);
                                }
                            }
                        }
                        break;
                }
            }

            return PInvoke.CallNextHookEx(hHook_MsgShell_TrayWnd, nCode, wParam, lParam);
        }

        private IntPtr CallbackKeyProc_Desktop(int nCode, IntPtr wParam, IntPtr lParam) {
            if(nCode >= 0) {
                if(((ulong)lParam & 0x80000000) == 0) {
                    // transition state == 0, key is pressed
                    if(HandleKEYDOWN_Desktop(wParam, (((ulong)lParam & 0x40000000) == 0x40000000)))
                        return new IntPtr(1);
                }
                else {
                    // transition state == 1, key is released
                    if(Config.Tips.SubDirTipsWithShift) {
                        slvDesktop.HideSubDirTip();
                    }
                }
            }
            return PInvoke.CallNextHookEx(hHook_KeyDesktop, nCode, wParam, lParam);
        }


        private bool HandleKEYDOWN_Desktop(IntPtr wParam, bool fRepeat) {
            Keys rawKey = (Keys)(int)wParam;
            int key = (int)wParam | (int)ModifierKeys;

            const int VK_F2 = 0x71;

            if(rawKey == Keys.ShiftKey) {
                if(!fRepeat) {
                    if(Config.Tips.ShowTooltipPreviews && !Config.Tips.ShowPreviewsWithShift) {
                        slvDesktop.HideThumbnailTooltip();
                    }
                    if(Config.Tips.ShowSubDirTips && !Config.Tips.SubDirTipsWithShift && !slvDesktop.SubDirTipMenuIsShowing()) {
                        slvDesktop.HideSubDirTip();
                    }
                }
                return false;
            }
            else if(rawKey == Keys.Delete) {
                if(!fRepeat) {
                    if(Config.Tips.ShowTooltipPreviews) {
                        slvDesktop.HideThumbnailTooltip();
                    }
                    if(Config.Tips.ShowSubDirTips && !slvDesktop.SubDirTipMenuIsShowing()) {
                        slvDesktop.HideSubDirTip();
                    }
                }
                return false;
            }
            else if(key == VK_F2) { //F2
                if(Config.Tweaks.F2Selection) {
                    slvDesktop.HandleF2();
                }
                return false;
            }


            key |= QTUtility.FLAG_KEYENABLED;
            if(key == Config.Keys.Shortcuts[(int)BindAction.CopyCurrentFolderPath]) {
                if(!fRepeat) DoFileTools(0);
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.CopyCurrentFolderName]) {
                if(!fRepeat) DoFileTools(1);
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.CopySelectedPaths]) {
                if(!fRepeat) DoFileTools(2);
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.CopySelectedNames]) {
                if(!fRepeat) DoFileTools(3);
            } /* todo
            else if(key == Config.Keys.Shortcuts[(int)BindAction.ShowHashWindow]) {
                if(!fRepeat) DoFileTools(4);
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.CopyFileHash]) {
                if(!fRepeat) DoFileTools(6);
            } */
            else if(key == Config.Keys.Shortcuts[(int)BindAction.CopyCurrentFolderName]) {
                if(!fRepeat) DoFileTools(0);
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.ShowSDTSelected]) {
                // Show SubDirTip for selected folder.
                if(Config.Tips.ShowSubDirTips) {
                    if(!fRepeat) DoFileTools(5);
                    return true;
                }
            } /* todo
            else if(key == Config.Keys.Shortcuts[(int)BindAction.ShowPreviewSelected]) {
                if(Config.Tips.ShowTooltipPreviews) {
                    slvDesktop.ShowThumbnailTooltipForSelectedItem();
                    return true;
                }
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.ItemDelete]) {
                if(!fRepeat) ShellBrowser.DeleteSelection(false);
                return true;
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.ItemDeleteNuke]) {
                if(!fRepeat) ShellBrowser.DeleteSelection(true);
                return true;
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.InvertSelection]) {
                if(!fRepeat) {
                    WindowUtils.ExecuteMenuCommand(hwndShellView, ExplorerMenuCommand.InvertSelection);
                }
                return true;
            }
            else if(key == Config.Keys.Shortcuts[(int)BindAction.Paste]) {
                if(!fRepeat) {
                    using(IDLWrapper idlw = new IDLWrapper(new byte[] {0, 0}, false)) {
                        if(idlw.Available && idlw.IsReadOnly &&
                                ShellMethods.ClipboardContainsFileDropList(slvDesktop.Handle, false, true)) {
                            WindowUtils.ExecuteMenuCommand(hwndShellView, ExplorerMenuCommand.PasteShortcut);
                        }
                        else {
                            System.Media.SystemSounds.Beep.Play();
                        }
                    }
                }
                return true;
            } 
            else if(key == QTUtility.ShortcutKeys[KeyShortcuts.CreateNewFolder] ||
                    key == QTUtility.ShortcutKeys[KeyShortcuts.CreateNewTxtFile]) {
                if(!fRepeat) {
                    using(IDLWrapper idlw = new IDLWrapper(new byte[] { 0, 0 }, false)) {
                        ShellMethods.CreateNewItem(ShellBrowser, idlw,
                                key == QTUtility.ShortcutKeys[KeyShortcuts.CreateNewFolder]);
                    }
                }
                return true;
            }

            else if(key == QTUtility.ShortcutKeys[KeyShortcuts.CreateShortcut] ||
                    key == QTUtility.ShortcutKeys[KeyShortcuts.CopyToFolder] ||
                            key == QTUtility.ShortcutKeys[KeyShortcuts.MoveToFolder]) {
                if(!fRepeat) {
                    if(GetSelectionCount() > 0) {
                        ExplorerMenuCommand command = ExplorerMenuCommand.CreateShortcut;
                        if(key == QTUtility.ShortcutKeys[KeyShortcuts.CopyToFolder]) {
                            command = ExplorerMenuCommand.CopyToFolder;
                        }
                        else if(key == QTUtility.ShortcutKeys[KeyShortcuts.MoveToFolder]) {
                            command = ExplorerMenuCommand.MoveToFolder;
                        }

                        WindowUtils.ExecuteMenuCommand(hwndShellView, command);
                    }
                    else {
                        System.Media.SystemSounds.Beep.Play();
                    }
                }
                return true;
            }
            */
            else {
                key &= ~QTUtility.FLAG_KEYENABLED;
                if(Config.Desktop.EnableAppShortcuts) {
                    // Check for app hotkeys
                    foreach(UserApp app in AppsManager.UserApps.Where(a => a.ShortcutKey == (Keys)key)) {
                        AppsManager.Execute(app, ShellBrowser);
                        return true;
                    }
                }

                // Check for group hotkey
                foreach(Group g in GroupsManager.Groups.Where(g => g.ShortcutKey == (Keys)key)) {
                    // todo: OpenGroup(g.Name, false);
                    return true;
                }
            }

            return false;
        }

        private static IntPtr GetDesktopHwnd() {
            IntPtr hwndProgman = PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
            IntPtr hwndSHELLDLL_DefView = PInvoke.FindWindowEx(hwndProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
            if(hwndSHELLDLL_DefView == IntPtr.Zero) {
                // seems to be reparented after desktop window created in Windows7

                IntPtr hwndWorkerW = PInvoke.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
                while(hwndWorkerW != IntPtr.Zero) {
                    hwndSHELLDLL_DefView = PInvoke.FindWindowEx(hwndWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if(hwndSHELLDLL_DefView != IntPtr.Zero) {
                        break;
                    }
                    hwndWorkerW = PInvoke.FindWindowEx(IntPtr.Zero, hwndWorkerW, "WorkerW", null);
                }
            }
            return PInvoke.FindWindowEx(hwndSHELLDLL_DefView, IntPtr.Zero, "SysListView32", null);
        }

        #endregion

        #region ---------- List View Events ----------

        private bool ListView_SelectionActivated(Keys modKeys) {
            // Handles item activation in Desktop thread

            // Default		..... New Tab / Navigate to
            // C			..... New Window
            // S			..... New Tab without selecting
            // C + S + A    ..... Open all sub folders in new tabs

            bool fEnqExec = Config.Misc.KeepRecentFiles;
            if(!Config.Tabs.ActivateNewTab) { // do not activate new tab
                if((modKeys & Keys.Shift) == Keys.Shift) {
                    modKeys &= ~Keys.Shift;
                }
                else {
                    modKeys |= Keys.Shift;
                }
            }
            // TODO: The tab bar reads the modkeys by itself.  But, it should use the ones already read if possible...
            // Hm...
            List<byte[]> lstIDLs = new List<byte[]>();
            List<string> lstFiles = new List<string>();

            foreach(IDLWrapper idlOrig in ShellBrowser.GetItems(true)) {
                using(IDLWrapper idlLink = idlOrig.ResolveTargetIfLink()) {
                    IDLWrapper idlw = idlLink ?? idlOrig;
                    if(!idlw.Available || !idlw.IsReadyIfDrive || idlw.IsLinkToDeadFolder) continue;
                    if(idlw.IsFolder) {
                        lstIDLs.Add(idlw.IDL);
                    }
                    if(fEnqExec) {
                        if(idlw.HasPath) {
                            lstFiles.Add(idlw.Path);
                        }
                    }
                }
            }

            if(lstIDLs.Count == 0) {
                if(fEnqExec && lstFiles.Count > 0) {
                    foreach(string path in lstFiles) {
                        StaticReg.ExecutedPathsList.Add(path);
                    }
                }
                return false;
            }
            else {
                OpenFolders(lstIDLs);
                return true;
            }
            
        }

        private bool ListView_MiddleClick(Point pt) {
            MouseChord chord = QTUtility.MakeMouseChord(MouseChord.Middle, ModifierKeys);
            BindAction action;
            if(Config.Mouse.ItemActions.TryGetValue(chord, out action)) {
                int index = slvDesktop.HitTest(pt, false);
                if(index <= -1) {
                    return false;
                }
                using(IDLWrapper wrapper = ShellBrowser.GetItem(index)) {
                    //return DoBindAction(action, false, null, wrapper);
                    // todo
                }
            }
            return false;
        }

        private bool ListView_MouseActivate(ref int result) {
            // The purpose of this is to prevent accidentally
            // renaming an item when clicking out of a SubDirTip menu.
            bool ret = false;
            if(slvDesktop.SubDirTipMenuIsShowing()) {
                if(ShellBrowser.GetSelectedCount() == 1 && slvDesktop.HotItemIsSelected()) {
                    result = 2;
                    slvDesktop.HideSubDirTipMenu();
                    slvDesktop.SetFocus();
                    ret = true;
                }
            }
            slvDesktop.RefreshSubDirTip(true);
            return ret;
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            QMenuItem clickedItem = (QMenuItem)e.ClickedItem;
            if(clickedItem.Target == MenuTarget.Folder) {
                using(IDLWrapper wrapper = clickedItem.IDLData != null
                                            ? new IDLWrapper(clickedItem.IDLData)
                                            : new IDLWrapper(clickedItem.TargetPath)) {
                    OpenFolder(wrapper);
                }
            }
            else {
                // todo: is this right?
                try {
                    Process.Start(new ProcessStartInfo(clickedItem.Path) {
                        WorkingDirectory = Path.GetDirectoryName(clickedItem.Path) ?? "",
                        ErrorDialog = true,
                        ErrorDialogParentHandle = IntPtr.Zero
                    });
                    if(Config.Misc.KeepRecentFiles) {
                        StaticReg.ExecutedPathsList.Add(clickedItem.Path);
                    }
                }
                catch {
                }   
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem clickedItem = e.ClickedItem as QMenuItem;
            if(clickedItem != null) {
                using(IDLWrapper wrapper = new IDLWrapper(clickedItem.Path)) {
                    e.HRESULT = iContextMenu2_Desktop.Open(wrapper, e.IsKey ? e.Point : MousePosition, ((SubDirTipForm)sender).Handle, false);
                }
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            List<string> paths = ((SubDirTipForm)sender).ExecutedDirectories;
            // TODO: IDLIFY, for the love of god!!
            OpenFolders(paths.Select(IDLWrapper.PathToIDL).ToList());
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            List<string> executedDirectories = ((SubDirTipForm)sender).ExecutedDirectories;
            // TODO: Replace ExecutedDirectories with ExecutedIDLs.
            List<byte[]> executedIDLs = executedDirectories.Select(path => {
                using(IDLWrapper wrapper = new IDLWrapper(path)) {
                    return wrapper.IDL;
                }
            }).ToList();
            e.HRESULT = iContextMenu2_Desktop.Open(executedIDLs, e.IsKey ? e.Point : MousePosition, ((SubDirTipForm)sender).Handle);
        }

        #endregion

#if false
        #region ---------- Tip Controls ----------

        // 2 tip controls are created in desktop thread

        private bool ShowThumbnailTooltip(IntPtr pIDL, int iItem, bool fKey) {
            string path;
            StringBuilder sb = new StringBuilder(260);
            if(PInvoke.SHGetPathFromIDList(pIDL, sb)) {
                path = sb.ToString();

                if(File.Exists(path)) {
                    if(path.StartsWith(IDLWrapper.INDICATOR_NAMESPACE) || path.StartsWith(IDLWrapper.INDICATOR_NETWORK) ||
                            path.ToLower().StartsWith(@"a:\"))
                        return false;

                    string ext = Path.GetExtension(path).ToLower();
                    if(ext == ".lnk") {
                        path = ShellMethods.GetLinkTargetPath(path);
                        if(path.Length == 0)
                            return false;

                        ext = Path.GetExtension(path).ToLower();
                    }

                    if(ThumbnailTooltipForm.ExtIsSupported(ext)) {
                        if(thumbnailTooltip == null) {
                            thumbnailTooltip = new ThumbnailTooltipForm();
                            thumbnailTooltip.ThumbnailVisibleChanged +=
                                    new EventHandler(thumbnailTooltip_ThumbnailVisibleChanged);

                            timer_Thumbnail = new System.Windows.Forms.Timer(components);
                            timer_Thumbnail.Interval = 400;
                            timer_Thumbnail.Tick += new EventHandler(timer_Thumbnail_Tick);
                        }

                        if(thumbnailTooltip.IsShownByKey && !fKey) {
                            thumbnailTooltip.IsShownByKey = false;
                            return true;
                        }

                        thumbnailIndex = iItem;
                        thumbnailTooltip.IsShownByKey = fKey;

                        RECT rct = GetLVITEMRECT(hwndListView, iItem, false, 0);

                        return thumbnailTooltip.ShowToolTip(path, new Point(rct.right - 16, rct.bottom - 8));
                    }
                }
            }
            HideThumbnailTooltip();
            return false;
        }

        private void ShowThumbnailTooltipForSelectedItem() {
            int iItem;
            List<IntPtr> lst = GetSelectedItemPIDL(out iItem);
            try {
                if(lst.Count == 1) {
                    ShowThumbnailTooltip(lst[0], iItem, true);
                }
            }
            finally {
                foreach(IntPtr pidl in lst) {
                    if(pidl != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(pidl);
                    }
                }
            }
        }

        private void HideThumbnailTooltip() {
            if(thumbnailTooltip != null && thumbnailTooltip.Visible) {
                thumbnailTooltip.HideToolTip();
            }
        }

        private void thumbnailTooltip_ThumbnailVisibleChanged(object sender, EventArgs e) {
            timer_Thumbnail.Enabled = false;

            if(thumbnailTooltip.Visible) {
                fThumbnailPending = false;
            }
            else {
                fThumbnailPending = true;
                timer_Thumbnail.Enabled = true;

                thumbnailIndex = -1;
            }
        }

        private void timer_Thumbnail_Tick(object sender, EventArgs e) {
            timer_Thumbnail.Enabled = false;
            fThumbnailPending = false;
        }

        private void timer_ThumbnailMouseHover_Tick(object sender, EventArgs e) {
            // show preview for hot item when desktop does not have focus

            timer_ThumbnailMouseHover.Enabled = false;

            if(thumbnailTooltip != null && thumbnailTooltip.Visible &&
                    thumbnailIndex_Inactive == thumbnailIndex)
                return;

            Point pnt = MousePosition;
            PInvoke.MapWindowPoints(IntPtr.Zero, hwndListView, ref pnt, 1);
            if(thumbnailIndex_Inactive ==
                    PInvoke.ListView_HitTest(hwndListView, QTUtility2.Make_LPARAM(pnt.X, pnt.Y))) {
                IntPtr pidl = GetItemPIDL(thumbnailIndex_Inactive);
                try {
                    if(pidl != IntPtr.Zero) {
                        RECT rct = GetLVITEMRECT(hwndListView, thumbnailIndex_Inactive, false, 0);
                        ShowThumbnailTooltip(pidl, thumbnailIndex_Inactive,
                                !PInvoke.PtInRect(ref rct, MousePosition));
                    }
                }
                finally {
                    if(pidl != IntPtr.Zero) {
                        PInvoke.CoTaskMemFree(pidl);
                    }
                }
            }
            thumbnailIndex_Inactive = -1;
        }

        private void timer_Thumbnail_NoTooltip_Tick(object sender, EventArgs e) {
            timer_Thumbnail_NoTooltip.Enabled = false;
        }



        private bool ShowSubDirTip(IntPtr pIDL, int iItem, bool fSkipFocusCheck) {
            // desktop thread ( desktop hook -> mouse hottrack, desktop hook -> keydown )

            if(fSkipFocusCheck || Config.Bool(Scts.SubDirTipForInactiveWindow) ||
                    hwndListView == PInvoke.GetFocus()) {
                try {
                    string path = ShellMethods.GetDisplayName(pIDL, false);
                    byte[] idl = ShellMethods.GetIDLData(pIDL);
                    bool fQTG;

                    if(QTTabBarClass.TryMakeSubDirTipPath(ref path, ref idl, false, out fQTG)) {
                        FOLDERVIEWMODE folderViewMode = FOLDERVIEWMODE.FVM_ICON;
                                // folderView.GetCurrentViewMode( ref folderViewMode ); 

                        RECT rct = GetLVITEMRECT(hwndListView, iItem, true, folderViewMode);
                        Point pnt = new Point(rct.right - 16, rct.bottom - 16);

                        if(subDirTip == null) {
                            //IntPtr hwndMessageParent = shellViewListener != null ? shellViewListener.Handle : IntPtr.Zero;

                            subDirTip = new SubDirTipForm(hwndShellView, hwndListView, false);
                            subDirTip.MenuItemClicked += subDirTip_MenuItemClicked;
                            subDirTip.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                            subDirTip.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                            subDirTip.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                        }

                        subDirTip.ShowSubDirTip(idl, pnt, hwndListView, fQTG);
                        return true;
                    }
                }
                catch(Exception ex) {
                    DebugUtil.AppendToExceptionLog(ex, null);
                }
            }
            return false;
        }

        private void HideSubDirTip() {
            // desktop thread
            if(subDirTip != null && subDirTip.Visible) {
                subDirTip.HideSubDirTip(false);
            }

            itemIndexDROPHILITED = -1;
        }

        private void HideSubDirTip_DesktopInactivated() {
            //  desktop thread
            if(subDirTip != null && subDirTip.Visible) {
                subDirTip.OnExplorerInactivated();
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            // this can run in both desktop and taskbar thread

            IntPtr hwndDialogParent = sender == subDirTip ? hwndListView : hwndShellTray;
                    // desktop thread or taskbar thread

            QMenuItem qmi = (QMenuItem)e.ClickedItem;

            if(qmi.Genre == MenuGenre.SubDirTip_QTGRootItem) {
                Thread thread = new Thread(OpenGroup);
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start(new object[] {new string[] {qmi.Name}, ModifierKeys});
            }
            else if(qmi.Target == MenuTarget.Folder) {
                using(IDLWrapper idlw = new IDLWrapper(qmi.IDL)) {
                    if(!idlw.IsDeadLink(hwndDialogParent)) {
                        Keys modKey = ModifierKeys;
                        if(!Config.Bool(Scts.ActivateNewTab)) {
                            if(modKey == Keys.Shift) {
                                modKey = Keys.None;
                            }
                            else if(modKey == Keys.None) {
                                modKey = Keys.Shift;
                            }
                        }

                        if(idlw.IsLink) {
                            if(!String.IsNullOrEmpty(qmi.TargetPath) &&
                                    qmi.TargetPath.StartsWith(IDLWrapper.INDICATOR_NETWORK) &&
                                            -1 == qmi.TargetPath.IndexOf(@"\", 2) &&
                                                    !ShellMethods.IsIDLNullOrEmpty(qmi.IDLTarget)) {
                                // link target is network server root ( "\\server" ), prevent opening window
                                OpenTab(new object[] {null, modKey, qmi.IDLTarget});
                                return;
                            }
                        }
                        OpenTab(new object[] {null, modKey, idlw.IDL});
                    }
                }
            }
            else {
                using(IDLWrapper idlw = new IDLWrapper(qmi.IDL)) {
                    if(!idlw.IsDeadLink(hwndDialogParent)) {
                        string work = String.Empty;

                        SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO();
                        sei.cbSize = Marshal.SizeOf(sei);
                        sei.nShow = SHOWWINDOW.SHOWNORMAL;
                        sei.fMask = SEEMASK.IDLIST;
                        sei.lpIDList = idlw.PIDL;
                        sei.hwnd = hwndDialogParent;

                        if(!String.IsNullOrEmpty(qmi.Path)) {
                            work = QTUtility2.MakeDefaultWorkingDirecotryStr(qmi.Path);
                            if(work.Length > 0) {
                                sei.lpDirectory = Marshal.StringToCoTaskMemUni(work);
                            }
                        }

                        try {
                            if(PInvoke.ShellExecuteEx(ref sei)) {
                                QTUtility.AddRecentFiles(
                                        new string[][] {
                                                work.Length > 0
                                                        ? new string[] {qmi.Path, String.Empty, work}
                                                        : new string[] {qmi.Path}
                                        }, hwndThis);
                            }
                        }
                        finally {
                            if(sei.lpDirectory != IntPtr.Zero) {
                                Marshal.FreeCoTaskMem(sei.lpDirectory);
                            }
                        }
                    }
                }
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            // this can run in both desktop and taskbar thread

            SubDirTipForm sdtf = (SubDirTipForm)sender;
            IntPtr hwndDialogParent = sdtf == subDirTip ? hwndListView : hwndShellTray;

            // SubDirTip_QTGRootItem
            string[] arrGrps = sdtf.ExecutedGroups;
            if(arrGrps.Length > 0) {
                Thread thread = new Thread(OpenGroup);
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start(new object[] {arrGrps, ModifierKeys});
            }

            // folders
            if(sdtf.ExecutedIDLs.Count > 0) {
                List<byte[]> lstIDLs = new List<byte[]>();

                foreach(byte[] idl in sdtf.ExecutedIDLs) {
                    using(IDLWrapper idlw = new IDLWrapper(idl, false)) {
                        if(idlw.IsLink) {
                            IDLWrapper idlwTarget;
                            if(idlw.TryGetLinkTarget(hwndDialogParent, out idlwTarget)) {
                                if(idlwTarget.IsFolder) {
                                    if(idlw.IsFolder) {
                                        lstIDLs.Add(idlw.IDL);
                                    }
                                    else {
                                        lstIDLs.Add(idlwTarget.IDL);
                                    }
                                }
                            }
                        }
                        else {
                            lstIDLs.Add(idlw.IDL);
                        }
                    }
                }

                Keys modKey = ModifierKeys;
                if(!Config.Bool(Scts.ActivateNewTab)) {
                    if(modKey == Keys.Shift) {
                        modKey = Keys.None;
                    }
                    else if(modKey == Keys.None) {
                        modKey = Keys.Shift;
                    }
                }

                if(lstIDLs.Count == 1) {
                    OpenTab(new object[] {null, modKey, lstIDLs[0]});
                }
                else if(lstIDLs.Count > 1) {
                    Thread thread = new Thread(OpenFolders2);
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start(new object[] {lstIDLs, modKey});
                }
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            // the calling thread can be taskBar or desktop

            using(IDLWrapper idlw = new IDLWrapper(((QMenuItem)e.ClickedItem).IDL, false)) {
                if(sender == subDirTip) {
                    e.Result = ShellMethods.PopUpShellContextMenu(idlw, e.IsKey ? e.Point : MousePosition,
                            ref iContextMenu2_Desktop, subDirTip.Handle, false);
                }
                else // subDirTip_TB
                {
                    e.Result = ShellMethods.PopUpShellContextMenu(idlw, e.IsKey ? e.Point : MousePosition,
                            ref iContextMenu2, subDirTip_TB.Handle, false);
                }

                if(e.Result == MC.COMMANDID_OPENPARENT) {
                    using(IDLWrapper idlwParent = new IDLWrapper(ShellMethods.GetParentIDL(idlw.PIDL))) {
                        if(idlwParent.Available) {
                            Thread thread = new Thread(OpenTab);
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.IsBackground = true;
                            thread.Start(new object[] {null, ModifierKeys, idlwParent.IDL});
                        }
                    }
                }
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(sender == subDirTip) {
                e.Result = ShellMethods.PopUpShellContextMenu(subDirTip.ExecutedIDLs,
                        e.IsKey ? e.Point : MousePosition, ref iContextMenu2_Desktop, subDirTip.Handle);
            }
            else if(sender == subDirTip_TB) {
                e.Result = ShellMethods.PopUpShellContextMenu(subDirTip_TB.ExecutedIDLs,
                        e.IsKey ? e.Point : MousePosition, ref iContextMenu2, subDirTip_TB.Handle);
            }
        }


        private void timer_HoverSubDirTipMenu_Tick(object sender, EventArgs e) {
            // drop hilited and MouseHoverTime elapsed 

            // desktop thread
            timer_HoverSubDirTipMenu.Enabled = false;
            int iItem = itemIndexDROPHILITED;

            if(MouseButtons != MouseButtons.None) {
                Point pnt = MousePosition;
                PInvoke.MapWindowPoints(IntPtr.Zero, hwndListView, ref pnt, 1);
                if(iItem == PInvoke.ListView_HitTest(hwndListView, QTUtility2.Make_LPARAM(pnt.X, pnt.Y))) {
                    using(IDLWrapper idlw = new IDLWrapper(GetItemPIDL(iItem))) {
                        if(idlw.Available) {
                            if(subDirTip != null) {
                                subDirTip.HideMenu();
                            }

                            if(!String.Equals(idlw.Path, CLSIDSTR_TRASHBIN, StringComparison.OrdinalIgnoreCase)) {
                                if(ShowSubDirTip(idlw.PIDL, iItem, true)) {
                                    itemIndexDROPHILITED = iItem;
                                    PInvoke.SetFocus(hwndListView);
                                    PInvoke.SetForegroundWindow(hwndListView);
                                    HideThumbnailTooltip();
                                    subDirTip.ShowMenuForDropHilited(GetDesktopIconSize());
                                    return;
                                }
                            }
                        }
                    }
                }

                if(subDirTip != null) {
                    if(subDirTip.IsMouseOnMenus) {
                        itemIndexDROPHILITED = -1;
                        return;
                    }
                }
            }

            HideSubDirTip();
        }


        private static RECT GetLVITEMRECT(IntPtr hwndListView, int iItem, bool fSubDirTip, FOLDERVIEWMODE fvm) {
            // get the bounding rectangle of item specified by iItem, in the screen coordinates.
            // fSubDirTip	true to get RECT depending on view style, false to get RECT by LVIR_BOUNDS

            const uint LVM_FIRST = 0x1000;
            const uint LVM_GETVIEW = (LVM_FIRST + 143);
            const uint LVM_GETITEMW = (LVM_FIRST + 75);
            const uint LVM_GETSTRINGWIDTHW = (LVM_FIRST + 87);
            const uint LVM_GETITEMSPACING = (LVM_FIRST + 51);
            const int LVIR_BOUNDS = 0;
            const int LVIR_ICON = 1;
            const int LVIR_LABEL = 2;
            const int LV_VIEW_ICON = 0x0000;
            const int LV_VIEW_DETAILS = 0x0001;
            const int LV_VIEW_LIST = 0x0003;
            const int LV_VIEW_TILE = 0x0004;
            const int LVIF_TEXT = 0x00000001;

            int view = (int)PInvoke.SendMessage(hwndListView, LVM_GETVIEW, IntPtr.Zero, IntPtr.Zero);
            int code = view == LV_VIEW_DETAILS ? LVIR_LABEL : LVIR_BOUNDS;

            bool fIcon = false; // for XP
            bool fList = false; // for XP

            if(fSubDirTip) {
                switch(view) {
                    case LV_VIEW_ICON:
                        fIcon = !QTUtility.IsVista;
                        code = LVIR_ICON;
                        break;

                    case LV_VIEW_DETAILS:
                        code = LVIR_LABEL;
                        break;

                    case LV_VIEW_LIST:
                        if(!QTUtility.IsVista) {
                            fList = true;
                            code = LVIR_ICON;
                        }
                        else {
                            code = LVIR_LABEL;
                        }
                        break;

                    case LV_VIEW_TILE:
                        code = LVIR_ICON;
                        break;

                    default:
                        // Here only in case of Vista LV_VIEW_SMALLICON.
                        code = LVIR_BOUNDS;
                        break;
                }
            }

            // get item rectangle
            RECT rct = PInvoke.ListView_GetItemRect(hwndListView, iItem, 0, code);

            // convert to screen coordinates
            PInvoke.MapWindowPoints(hwndListView, IntPtr.Zero, ref rct, 2);

            // adjust rct
            // these magic numbers have no logical meanings
            if(fIcon) {
                // XP, subdirtip.
                // THUMBNAIL, THUMBSTRIP or ICON.
                if(fvm == FOLDERVIEWMODE.FVM_THUMBNAIL || fvm == FOLDERVIEWMODE.FVM_THUMBSTRIP) {
                    rct.right -= 13;
                }
                else // fvm == FVM_ICON
                {
                    int currentIconSpacing =
                            (int)(long)PInvoke.SendMessage(hwndListView, LVM_GETITEMSPACING, IntPtr.Zero, IntPtr.Zero);
                    Size sz = SystemInformation.IconSize;
                    rct.right = rct.left + (((currentIconSpacing & 0xFFFF) - sz.Width)/2) + sz.Width + 8;
                    rct.bottom = rct.top + sz.Height + 6;
                }
            }
            else if(fList) {
                // XP, subdirtip.
                // calculate item text rectangle
                LVITEM lvitem = new LVITEM();
                lvitem.pszText = Marshal.AllocCoTaskMem(520);
                lvitem.cchTextMax = 260;
                lvitem.iItem = iItem;
                lvitem.mask = LVIF_TEXT;
                IntPtr pLI = Marshal.AllocCoTaskMem(Marshal.SizeOf(lvitem));
                Marshal.StructureToPtr(lvitem, pLI, false);

                PInvoke.SendMessage(hwndListView, LVM_GETITEMW, IntPtr.Zero, pLI);

                int w = (int)PInvoke.SendMessage(hwndListView, LVM_GETSTRINGWIDTHW, IntPtr.Zero, lvitem.pszText);
                w += 20;

                Marshal.FreeCoTaskMem(lvitem.pszText);
                Marshal.FreeCoTaskMem(pLI);

                rct.right += w;
                rct.top += 2;
                rct.bottom += 2;
            }

            return rct;
        }

        private static int GetDesktopIconSize() {
            const string KEYNAME = @"Software\Microsoft\Windows\Shell\Bags\1\Desktop";
            const string VALNAME = "IconSize";

            using(RegistryKey rk = Registry.CurrentUser.OpenSubKey(KEYNAME)) {
                if(rk != null) {
                    return (int)rk.GetValue(VALNAME, 48);
                }
            }
            return 48;
        }

        #endregion
#endif

        #region ---------- Settings ----------

        private void ReadSetting() {
            lstItemOrder.Clear();
            lstItemOrder.Add(QTUtility.ValidateMinMax(Config.Desktop.FirstItem, 0, ITEMTYPE_COUNT));
            lstItemOrder.Add(QTUtility.ValidateMinMax(Config.Desktop.SecondItem, 0, ITEMTYPE_COUNT));
            lstItemOrder.Add(QTUtility.ValidateMinMax(Config.Desktop.ThirdItem, 0, ITEMTYPE_COUNT));
            lstItemOrder.Add(QTUtility.ValidateMinMax(Config.Desktop.FourthItem, 0, ITEMTYPE_COUNT));
            for(int i = 0; i < ITEMTYPE_COUNT; i++) {
                if(!lstItemOrder.Contains(i)) {
                    lstItemOrder.Add(i);
                }
            }
            ExpandState[0] = Config.Desktop.GroupExpanded;
            ExpandState[1] = Config.Desktop.RecentTabExpanded;
            ExpandState[2] = Config.Desktop.ApplicationExpanded;
            ExpandState[3] = Config.Desktop.RecentFileExpanded;
        }

        private void SaveSetting() {
            Config.Desktop.FirstItem = lstItemOrder[0];
            Config.Desktop.SecondItem = lstItemOrder[1];
            Config.Desktop.ThirdItem = lstItemOrder[2];
            Config.Desktop.FourthItem = lstItemOrder[3];
            Config.Desktop.GroupExpanded = ExpandState[0];
            Config.Desktop.RecentTabExpanded = ExpandState[1];
            Config.Desktop.ApplicationExpanded = ExpandState[2];
            Config.Desktop.RecentFileExpanded = ExpandState[3];
            Config.Desktop.TaskBarDblClickEnabled = tsmiTaskBar.Checked;
            Config.Desktop.DesktopDblClickEnabled = tsmiDesktop.Checked;
            Config.Desktop.LockMenu = tsmiLockItems.Checked;
            Config.Desktop.TitleBackground = tsmiVSTitle.Checked;
            Config.Desktop.IncludeGroup = tsmiOnGroup.Checked;
            Config.Desktop.IncludeRecentTab = tsmiOnHistory.Checked;
            Config.Desktop.IncludeApplication = tsmiOnUserApps.Checked;
            Config.Desktop.IncludeRecentFile = tsmiOnRecentFile.Checked;
            Config.Desktop.OneClickMenu = tsmiOneClick.Checked;
            Config.Desktop.EnableAppShortcuts = tsmiAppKeys.Checked;
            Config.Desktop.Width = Width;
            ConfigManager.WriteConfig(true);
        }

        private void contextMenuForSetting_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem is ToolStripSeparator)
                return;

            if(e.ClickedItem == tsmiTaskBar) {
                tsmiTaskBar.Checked = !tsmiTaskBar.Checked;
            }
            else if(e.ClickedItem == tsmiDesktop) {
                tsmiDesktop.Checked = !tsmiDesktop.Checked;
            }
            else if(e.ClickedItem == tsmiLockItems) {
                tsmiLockItems.Checked = !tsmiLockItems.Checked;

                contextMenu.ReorderEnabled =
                        ddmrGroups.ReorderEnabled = !tsmiLockItems.Checked;
                // todo
                //ddmrUserapps.SetChildrenReorderEnabled(!tsmiLockItems.Checked);
            }

            else if(e.ClickedItem == tsmiOnGroup) {
                tsmiOnGroup.Checked = !tsmiOnGroup.Checked;

                lstRefreshRequired[ITEMINDEX_GROUP] = true;
            }
            else if(e.ClickedItem == tsmiOnHistory) {
                tsmiOnHistory.Checked = !tsmiOnHistory.Checked;

                lstRefreshRequired[ITEMINDEX_RECENTTAB] = true;
            }
            else if(e.ClickedItem == tsmiOnUserApps) {
                tsmiOnUserApps.Checked = !tsmiOnUserApps.Checked;

                lstRefreshRequired[ITEMINDEX_APPLAUNCHER] = true;
            }
            else if(e.ClickedItem == tsmiOnRecentFile) {
                tsmiOnRecentFile.Checked = !tsmiOnRecentFile.Checked;

                lstRefreshRequired[ITEMINDEX_RECENTFILE] = true;
            }

            else if(e.ClickedItem == tsmiVSTitle) {
                tsmiVSTitle.Checked = !tsmiVSTitle.Checked;

                TitleMenuItem.DrawBackground = tsmiVSTitle.Checked;
            }
            else if(e.ClickedItem == tsmiOneClick) {
                tsmiOneClick.Checked = !tsmiOneClick.Checked;
            }
            else if(e.ClickedItem == tsmiAppKeys) {
                tsmiAppKeys.Checked = !tsmiAppKeys.Checked;
            }

            SaveSetting();
        }

        private void RefreshStringResources() {
            string[] ResTaskbar = QTUtility.TextResourcesDic["TaskBar_Menu"];

            tsmiTaskBar.Text = ResTaskbar[0];
            tsmiDesktop.Text = ResTaskbar[1];
            tsmiLockItems.Text = ResTaskbar[2];
            tsmiVSTitle.Text = ResTaskbar[3];
            tsmiOneClick.Text = ResTaskbar[4];
            tsmiAppKeys.Text = ResTaskbar[5];

            string[] titles = QTUtility.TextResourcesDic["TaskBar_Titles"];

            tsmiOnGroup.Text =
                    tmiLabel_Group.Text =
                            tmiGroup.Text = titles[0];

            tsmiOnHistory.Text =
                    tmiHistory.Text =
                            tmiLabel_History.Text = titles[1];

            tsmiOnUserApps.Text =
                    tmiUserApp.Text =
                            tmiLabel_UserApp.Text = titles[2];

            tsmiOnRecentFile.Text =
                    tmiRecentFile.Text =
                            tmiLabel_RecentFile.Text = titles[3];
        }

        #endregion


        #region ---------- Event Handlers ----------


        private void desktopTool_MouseClick(object sender, MouseEventArgs e) {
            // single click mode
            if(e.Button == MouseButtons.Left && Config.Desktop.OneClickMenu) {
                ShowMenu(MousePosition);
            }
        }

        private void desktopTool_MouseDoubleClick(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left && !Config.Desktop.OneClickMenu) {
                ShowMenu(MousePosition);
            }
        }


        private void contextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            if(fCancelClosing) {
                e.Cancel = true;
                fCancelClosing = false;
            }
            else {
                if(fRootReordered) {
                    fRootReordered = false;

                    List<int> lst = new List<int>();
                    for(int i = 0; i < contextMenu.Items.Count; i++) {
                        if(lst.Count == ITEMTYPE_COUNT)
                            break;

                        ToolStripItem item = contextMenu.Items[i];

                        if(item is TitleMenuItem) {
                            if(item == tmiGroup || item == tmiLabel_Group) {
                                lst.Add(ITEMINDEX_GROUP);
                                continue;
                            }
                            if(item == tmiHistory || item == tmiLabel_History) {
                                lst.Add(ITEMINDEX_RECENTTAB);
                                continue;
                            }
                            if(item == tmiUserApp || item == tmiLabel_UserApp) {
                                lst.Add(ITEMINDEX_APPLAUNCHER);
                                continue;
                            }
                            if(item == tmiRecentFile || item == tmiLabel_RecentFile) {
                                lst.Add(ITEMINDEX_RECENTFILE);
                                continue;
                            }
                        }
                    }
                    if(lst.Count < 4) {
                        foreach(int t in lstItemOrder) {
                            if(!lst.Contains(t)) {
                                lst.Add(t);
                            }
                        }
                    }

                    lstItemOrder.Clear();
                    foreach(int t in lst) {
                        lstItemOrder.Add(t);
                    }

                    SaveSetting();
                }
            }
        }

        private bool fRootReordered;

        private void contextMenu_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            fRootReordered = e.ClickedItem is TitleMenuItem;
            QMenuItem qmi = e.ClickedItem as QMenuItem;
            if(qmi == null) return;

            if(qmi.Genre == MenuGenre.Group) {
                lstGroupItems = contextMenu.Items.Cast<ToolStripItem>().Where(tsi =>
                        (tsi is QMenuItem && ((QMenuItem)tsi).Genre == MenuGenre.Group) ||
                        (tsi is ToolStripSeparator && tsi.Name == TSS_NAME_GRP)).ToList();
                GroupsManager.HandleReorder(lstGroupItems);
            }
            else if(qmi.Genre == MenuGenre.Application) {
                lstUserAppItems = contextMenu.Items.Cast<ToolStripItem>().Where(tsi =>
                    (tsi is QMenuItem && ((QMenuItem)tsi).Genre == MenuGenre.Application) ||
                    (tsi is ToolStripSeparator && tsi.Name == TSS_NAME_APP)).ToList();
                AppsManager.HandleReorder(lstUserAppItems);
            }
        }

        private void dropDowns_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            // since menu is created on taskbar thread 
            // this runs on the thread

            TitleMenuItem tmi = e.ClickedItem as TitleMenuItem;
            if(tmi != null) {
                if(tmi.IsOpened) {
                    // Close
                    OnLabelTitleClickedToClose(tmi.Genre);
                }
                else {
                    // Open
                    OnSubMenuTitleClickedToOpen(tmi.Genre);
                }

                return;
            }

            QMenuItem qmi = e.ClickedItem as QMenuItem;
            if(qmi == null) {
                return;
            }

            if(qmi.Genre == MenuGenre.Group) {
                OpenGroup(qmi.Text);
            }
            else if(qmi.Genre == MenuGenre.History) {
                using(IDLWrapper idlw = new IDLWrapper(qmi.IDLData)) {
                    OpenFolder(idlw);    
                }
            }
            else if(qmi.Genre == MenuGenre.Application && qmi.Target == MenuTarget.File) {
                // User apps
                AppsManager.Execute(qmi.MenuItemArguments.App, ShellBrowser);
            }
            else if(qmi.Genre == MenuGenre.RecentFile) {
                // Todo: unify
                try {
                    string toolTipText = e.ClickedItem.ToolTipText ?? "";
                    ProcessStartInfo startInfo = new ProcessStartInfo(toolTipText) {
                        WorkingDirectory = Path.GetDirectoryName(toolTipText),
                        ErrorDialog = true,
                        ErrorDialogParentHandle = Handle
                    };
                    Process.Start(startInfo);
                    StaticReg.ExecutedPathsList.Add(toolTipText);
                }
                catch {
                    SystemSounds.Hand.Play();
                }
            }
        }

        private void dropDowns_ItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            QMenuItem qmi = e.ClickedItem as QMenuItem;

            // Is not valid menu item, or Virutal folder, do nothing
            if(qmi == null || qmi.Target == MenuTarget.VirtualFolder) {
                // cancel closing.
                //e.Result = MC.COMMANDID_USERCANCEL; hmm...
                return;
            }

            Point pnt = e.IsKey ? e.Point : MousePosition;

            if(qmi.Genre == MenuGenre.Group) {
                // Group
                MenuUtility.GroupMenu_ItemRightClicked(sender, e);
            }
            else {
                // RecentlyClosed, User apps, Recent files.

                //						menu items can be removed	|	menu items have idl data
                // RecentClosedTab					Y				|				Y
                // User Apps						N				|				N
                // RecentFiles						Y				|				N

                bool fCanRemove = qmi.Genre != MenuGenre.Application;
                const int COMMANDID_REMOVEITEM = 0xffff; // todo: move to const class
                const int COMMANDID_OPENPARENT = 0xfffe;
                const int COMMANDID_USERCANCEL = 0xfffd;

                using(
                        IDLWrapper idlw = qmi.Genre == MenuGenre.History
                                ? new IDLWrapper(qmi.IDLData, false)
                                : new IDLWrapper(qmi.Path)) {
                    e.HRESULT = iContextMenu2.Open(idlw, pnt, ((DropDownMenuReorderable)sender).Handle, fCanRemove);

                    if(e.HRESULT == COMMANDID_OPENPARENT) {
                        using(IDLWrapper idlwParent = idlw.GetParent()) {
                            if(idlwParent.Available) {
                                OpenFolder(idlwParent);
                            }
                        }
                    }
                    else if(e.HRESULT == COMMANDID_REMOVEITEM) {
                        if(qmi.Genre == MenuGenre.History) {
                            StaticReg.ClosedTabHistoryList.Remove(qmi.Path);
                            lstUndoClosedItems.Remove(qmi);
                        }
                        else if(qmi.Genre == MenuGenre.RecentFile) {
                            StaticReg.ClosedTabHistoryList.Remove(qmi.Path);
                            lstRecentFileItems.Remove(qmi);
                        }
                        qmi.Dispose();
                    }
                }
            }
        }

        private void dropDowns_ReorderFinished(object sender, ToolStripItemClickedEventArgs e) {
            if(sender == ddmrGroups) {
                lstGroupItems.Clear();
                lstGroupItems.AddRange(ddmrGroups.Items.Cast<ToolStripItem>());
                GroupsManager.HandleReorder(lstGroupItems);
            }
            else if(sender == ddmrUserapps) {
                lstUserAppItems.Clear();
                lstUserAppItems.AddRange(ddmrUserapps.Items.Cast<ToolStripItem>());
                AppsManager.HandleReorder(lstUserAppItems);
            }
        }

        private void directoryMenuItems_DoubleClick(object sender, EventArgs e) {
            // DirectoryMenuItem is clicked.
            // It's guaranteed that sender is DirectoryMenuItem.

            string path = ((DirectoryMenuItem)sender).Path;
            if(!Directory.Exists(path)) return;
            try {
                using(var pidl = new IDLWrapper(path)) { // todo: idlize
                    OpenFolder(pidl);    
                }
            }
            catch {
                MessageBox.Show(
                    String.Format(QTUtility.TextResourcesDic["ErrorDialogs"][9], path),
                    QTUtility.TextResourcesDic["ErrorDialogs"][4], 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error
                );
            }
        }


        private void tsmiExperimental_DropDownOpening(object sender, EventArgs e) {
            if(tsmiExperimental.DropDownItems.Count == 1) {
                tsmiExperimental.DropDownItems[0].Dispose();
                tsmiExperimental.DropDownOpening -= tsmiExperimental_DropDownOpening;

                tsmiExperimental.DropDown.SuspendLayout();
                for(int i = 0; i < 4; i++) {
                    tsmiExperimental.DropDown.Items.Add(
                        QTUtility.TextResourcesDic["Desktop"][i]
                    );
                }
                tsmiExperimental.DropDown.ResumeLayout();
            }
        }

        private void tsmiExperimental_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            int index = tsmiExperimental.DropDown.Items.IndexOf(e.ClickedItem);
            switch(index) {
                case 0:
                    ShellBrowser.ViewMode = FVM.LIST;
                    break;

                case 1:
                    ShellBrowser.ViewMode = FVM.DETAILS;
                    break;

                case 2:
                    ShellBrowser.ViewMode = FVM.TILE;
                    break;

                case 3:
                    ShellBrowser.ViewMode = FVM.ICON;
                    break;    
            }
        }



        #endregion


        #region ---------- Menu Creation ----------


        private bool[] BuildMenuItems() {
            List<bool> lst = new List<bool> {false, false, false, false};

            // group
            if(lstRefreshRequired[ITEMINDEX_GROUP]) {
                lstRefreshRequired[ITEMINDEX_GROUP] = false;

                // clear items
                foreach(ToolStripItem tsi in lstGroupItems) {
                    tsi.Dispose();
                }
                lstGroupItems.Clear();

                if(Config.Desktop.IncludeGroup) {
                    lst[ITEMINDEX_GROUP] = true;
                    lstGroupItems = MenuUtility.CreateGroupItems(null);
                    foreach(var tss in lstGroupItems.OfType<ToolStripSeparator>()) {
                        tss.Name = TSS_NAME_GRP;
                    }
                }
                else {
                    contextMenu.Items.Remove(tmiLabel_Group);
                    contextMenu.Items.Remove(tmiGroup);
                }
            }

            // recent tab
            {
                lstRefreshRequired[ITEMINDEX_RECENTTAB] = false;

                // clear items
                foreach(ToolStripItem item in lstUndoClosedItems) {
                    item.Dispose();
                }
                lstUndoClosedItems.Clear();

                if(Config.Desktop.IncludeRecentTab) {
                    lst[ITEMINDEX_RECENTTAB] = true;

                    lstUndoClosedItems = MenuUtility.CreateUndoClosedItems(null);
                }
                else {
                    contextMenu.Items.Remove(tmiLabel_History);
                    contextMenu.Items.Remove(tmiHistory);
                }
            }

            // application launcher
            if(lstRefreshRequired[ITEMINDEX_APPLAUNCHER]) {
                lstRefreshRequired[ITEMINDEX_APPLAUNCHER] = false;

                // clear items
                foreach(ToolStripItem item in lstUserAppItems) {
                    item.Dispose();
                }
                lstUserAppItems.Clear();

                if(Config.Desktop.IncludeApplication) {
                    lst[ITEMINDEX_APPLAUNCHER] = true;

                    lstUserAppItems = MenuUtility.CreateAppLauncherItems(
                            Handle,
                            ShellBrowser,
                            !Config.Desktop.LockMenu,
                            dropDowns_ItemRightClicked,
                            directoryMenuItems_DoubleClick,
                            true);
                }
                else {
                    contextMenu.Items.Remove(tmiLabel_UserApp);
                    contextMenu.Items.Remove(tmiUserApp);
                }
            }

            // recent file
            if(lstRefreshRequired[ITEMINDEX_RECENTFILE]) {
                lstRefreshRequired[ITEMINDEX_RECENTFILE] = false;

                // clear items
                foreach(ToolStripItem item in lstRecentFileItems) {
                    item.Dispose();
                }
                lstRecentFileItems.Clear();

                if(Config.Desktop.IncludeRecentFile) {
                    lst[ITEMINDEX_RECENTFILE] = true;

                    lstRecentFileItems = MenuUtility.CreateRecentFilesItems();
                }
                else {
                    contextMenu.Items.Remove(tmiLabel_RecentFile);
                    contextMenu.Items.Remove(tmiRecentFile);
                }
            }

            return lst.ToArray();
        }

        private void ShowMenu(Point popUpPoint) {
            // Note:
            //		this method must be executed on Taskbar thread
            //		and set taskbar foreground beforehand.

            contextMenu.SuspendLayout();
            ddmrGroups.SuspendLayout();
            ddmrHistory.SuspendLayout();
            ddmrUserapps.SuspendLayout();
            ddmrRecentFile.SuspendLayout();

            // sync texts

            bool[] flags = BuildMenuItems();

            foreach(int index in lstItemOrder) {
                if(flags[index]) {
                    switch(index) {
                        case ITEMINDEX_GROUP:
                            AddMenuItems_Group();
                            break;

                        case ITEMINDEX_RECENTTAB:
                            AddMenuItems_RecentTab();
                            break;

                        case ITEMINDEX_APPLAUNCHER:
                            AddMenuItems_AppLauncher();
                            break;

                        case ITEMINDEX_RECENTFILE:
                            AddMenuItems_RecentFile();
                            break;
                    }
                }
            }

            ddmrUserapps.ResumeLayout();
            ddmrHistory.ResumeLayout();
            ddmrGroups.ResumeLayout();
            ddmrRecentFile.ResumeLayout();
            contextMenu.ResumeLayout();

            if(contextMenu.Items.Count > 0) {
                if(!QTUtility.IsXP) contextMenu.SendToBack();
                contextMenu.Show(popUpPoint);
            }
        }

        private void AddMenuItems_Group() {
            if(ExpandState[ITEMINDEX_GROUP]) {
                // Opened
                int index = GetInsertionIndex(ITEMINDEX_GROUP);
                contextMenu.InsertItem(index, tmiLabel_Group, MENUKEY_LABEL_GROUP);
                foreach(ToolStripItem item in lstGroupItems) {
                    contextMenu.InsertItem(++index, item, MENUKEY_ITEM_GROUP);
                }
            }
            else {
                // Closed
                ddmrGroups.AddItemsRange(lstGroupItems.ToArray(), MENUKEY_ITEM_GROUP);
                contextMenu.InsertItem(GetInsertionIndex(ITEMINDEX_GROUP), tmiGroup, MENUKEY_SUBMENUS);
            }
        }

        private void AddMenuItems_RecentTab() {
            if(ExpandState[ITEMINDEX_RECENTTAB]) {
                int index = GetInsertionIndex(ITEMINDEX_RECENTTAB);
                contextMenu.InsertItem(index, tmiLabel_History, MENUKEY_LABEL_HISTORY);
                foreach(ToolStripItem item in lstUndoClosedItems) {
                    contextMenu.InsertItem(++index, item, MENUKEY_ITEM_HISTORY);
                }
            }
            else {
                ddmrHistory.AddItemsRange(lstUndoClosedItems.ToArray(), MENUKEY_ITEM_HISTORY);
                contextMenu.InsertItem(GetInsertionIndex(ITEMINDEX_RECENTTAB), tmiHistory, MENUKEY_SUBMENUS);
            }
        }

        private void AddMenuItems_AppLauncher() {
            if(ExpandState[ITEMINDEX_APPLAUNCHER]) {
                int index = GetInsertionIndex(ITEMINDEX_APPLAUNCHER);
                contextMenu.InsertItem(index, tmiLabel_UserApp, MENUKEY_LABEL_USERAPP);
                foreach(ToolStripItem item in lstUserAppItems) {
                    contextMenu.InsertItem(++index, item, MENUKEY_ITEM_USERAPP);
                }
            }
            else {
                ddmrUserapps.AddItemsRange(lstUserAppItems.ToArray(), MENUKEY_ITEM_USERAPP);
                contextMenu.InsertItem(GetInsertionIndex(ITEMINDEX_APPLAUNCHER), tmiUserApp, MENUKEY_SUBMENUS);
            }
        }

        private void AddMenuItems_RecentFile() {
            if(ExpandState[ITEMINDEX_RECENTFILE]) {
                int index = GetInsertionIndex(ITEMINDEX_RECENTFILE);
                contextMenu.InsertItem(index, tmiLabel_RecentFile, MENUKEY_LABEL_RECENT);
                foreach(ToolStripItem item in lstRecentFileItems) {
                    contextMenu.InsertItem(++index, item, MENUKEY_ITEM_RECENT);
                }
            }
            else {
                ddmrRecentFile.AddItemsRange(lstRecentFileItems.ToArray(), MENUKEY_ITEM_RECENT);
                contextMenu.InsertItem(GetInsertionIndex(ITEMINDEX_RECENTFILE), tmiRecentFile,
                        MENUKEY_SUBMENUS);
            }
        }

        private int GetInsertionIndex(int ITEMINDEX) {
            int prev = -1;
            for(int i = 0; i < lstItemOrder.Count; i++) {
                if(lstItemOrder[i] == ITEMINDEX) {
                    if(i != 0) {
                        prev = lstItemOrder[i - 1];
                    }
                    break;
                }
            }

            if(prev == -1) {
                return 0;
            }
            else {
                for(int i = 0; i < contextMenu.Items.Count; i++) {
                    TitleMenuItem titleItem = contextMenu.Items[i] as TitleMenuItem;
                    if(titleItem != null) {
                        if(GenreToInt32(titleItem.Genre) == prev) {
                            // previous item found
                            for(int j = i + 1; j < contextMenu.Items.Count; j++) {
                                if(contextMenu.Items[j] is TitleMenuItem) {
                                    return j;
                                }
                            }

                            return contextMenu.Items.Count;
                        }
                    }
                }

                // previsous items not found...
                return GetInsertionIndex(prev);
            }
        }

        private static int GenreToInt32(MenuGenre genre) {
            switch(genre) {
                default:
                case MenuGenre.Group:
                    return ITEMINDEX_GROUP;

                case MenuGenre.History:
                    return ITEMINDEX_RECENTTAB;

                case MenuGenre.Application:
                    return ITEMINDEX_APPLAUNCHER;

                case MenuGenre.RecentFile:
                    return ITEMINDEX_RECENTFILE;
            }
        }



        private void OnLabelTitleClickedToClose(MenuGenre genre) {
            int labelIndex = GenreToInt32(genre);

            ExpandState[labelIndex] = !ExpandState[labelIndex];
            fCancelClosing = true;

            ToolStripMenuItem labelToRemove;
            List<ToolStripItem> listToRemove;
            ToolStripMenuItem itemToAdd;
            string key;

            if(labelIndex == 0) {
                labelToRemove = tmiLabel_Group;
                listToRemove = lstGroupItems;
                itemToAdd = tmiGroup;
                key = MENUKEY_ITEM_GROUP;
            }
            else if(labelIndex == 1) {
                labelToRemove = tmiLabel_History;
                listToRemove = lstUndoClosedItems;
                itemToAdd = tmiHistory;
                key = MENUKEY_ITEM_HISTORY;
            }
            else if(labelIndex == 2) {
                labelToRemove = tmiLabel_UserApp;
                listToRemove = lstUserAppItems;
                itemToAdd = tmiUserApp;
                key = MENUKEY_ITEM_USERAPP;
            }
            else //if( labelIndex == 3 )
            {
                labelToRemove = tmiLabel_RecentFile;
                listToRemove = lstRecentFileItems;
                itemToAdd = tmiRecentFile;
                key = MENUKEY_ITEM_RECENT;
            }

            int index = contextMenu.Items.IndexOf(labelToRemove);

            contextMenu.SuspendLayout();

            contextMenu.Items.Remove(labelToRemove);
            foreach(ToolStripItem tsi in listToRemove) {
                contextMenu.Items.Remove(tsi);
            }

            contextMenu.InsertItem(index, itemToAdd, MENUKEY_SUBMENUS);

            ((DropDownMenuReorderable)itemToAdd.DropDown).AddItemsRange(listToRemove.ToArray(), key);


            contextMenu.ResumeLayout();

        }

        private void OnSubMenuTitleClickedToOpen(MenuGenre genre) {
            int menuIndex = 0;
            switch(genre) {
                case MenuGenre.Group:
                    menuIndex = 0;
                    break;
                case MenuGenre.History:
                    menuIndex = 1;
                    break;
                case MenuGenre.Application:
                    menuIndex = 2;
                    break;
                case MenuGenre.RecentFile:
                    menuIndex = 3;
                    break;
            }

            ExpandState[menuIndex] = !ExpandState[menuIndex];
            fCancelClosing = true;

            ToolStripMenuItem itemToRemove;
            ToolStripMenuItem labelToAdd;
            List<ToolStripItem> listToAdd;
            string key;

            if(menuIndex == 0) {
                itemToRemove = tmiGroup;
                labelToAdd = tmiLabel_Group;
                listToAdd = lstGroupItems;
                key = MENUKEY_ITEM_GROUP;
            }
            else if(menuIndex == 1) {
                itemToRemove = tmiHistory;
                labelToAdd = tmiLabel_History;
                listToAdd = lstUndoClosedItems;
                key = MENUKEY_ITEM_HISTORY;
            }
            else if(menuIndex == 2) {
                itemToRemove = tmiUserApp;
                labelToAdd = tmiLabel_UserApp;
                listToAdd = lstUserAppItems;
                key = MENUKEY_ITEM_USERAPP;
            }
            else //if( menuIndex == 3 )
            {
                itemToRemove = tmiRecentFile;
                labelToAdd = tmiLabel_RecentFile;
                listToAdd = lstRecentFileItems;
                key = MENUKEY_ITEM_RECENT;
            }

            itemToRemove.DropDown.Hide();

            contextMenu.SuspendLayout();

            int index = contextMenu.Items.IndexOf(itemToRemove);
            contextMenu.Items.Remove(itemToRemove);

            contextMenu.InsertItem(index, labelToAdd, MENUKEY_LABELS + menuIndex);

            foreach(ToolStripItem tsi in listToAdd) {
                contextMenu.InsertItem(++index, tsi, key);
            }

            contextMenu.ResumeLayout();

        }

        /* todo: hmm, this looks like it could be useful later.
         * 
        private SubDirTipForm CreateSubDirTip() {
            // creates menu drop down for real folder contained application menu drop down
            // runs on TaskBar thread

            if(subDirTip_TB == null) {
                subDirTip_TB = new SubDirTipForm(Handle, hwndShellTray, false);
                subDirTip_TB.MenuItemClicked += subDirTip_MenuItemClicked;
                subDirTip_TB.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                subDirTip_TB.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                subDirTip_TB.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
            }
            return subDirTip_TB;
        }*/

        #endregion


        #region ---------- Menu actions ----------


        private void OpenGroup(string group) {
            bool fForceNewWindow = (ModifierKeys == Keys.Control);
            if(!fForceNewWindow && Config.Window.CaptureNewWindows && InstanceManager.GetTotalInstanceCount() > 0) {
                InstanceManager.BeginInvokeMain(tabbar => tabbar.OpenGroup(@group, false));
            }
            else {
                Group g = GroupsManager.GetGroup(group);
                if(g == null || g.Paths.Count == 0) return;
                StaticReg.CreateWindowGroup = group;
                using(var pidl = new IDLWrapper(g.Paths[0])) {
                    // todo: ensure it gets locked and what not
                    OpenWindow(pidl);
                }                
            }
        }

        private static void OpenWindow(IDLWrapper pidl) {
            const int SW_SHOWNORMAL = 1;
            const int SEE_MASK_IDLIST = 0x00000004;
            SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO {
                cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO)),
                nShow = SW_SHOWNORMAL,
                fMask = SEE_MASK_IDLIST,
                lpIDList = pidl.PIDL
            };
            PInvoke.ShellExecuteEx(ref sei);
        }

        private void OpenFolder(IDLWrapper pidl, bool fForceTab = false) {
            OpenFolders(new List<byte[]> { pidl.IDL }, fForceTab);
        }

        private static void OpenFolders(List<byte[]> lstIDLs, bool fForceTab = false) {
            if(lstIDLs.Count == 0) return;
            if((fForceTab || Config.Window.CaptureNewWindows) && InstanceManager.GetTotalInstanceCount() > 0) {
                InstanceManager.BeginInvokeMain(tabbar => {
                    bool first = true;
                    foreach(byte[] idl in lstIDLs) {
                        using(var idlw = new IDLWrapper(idl)) {
                            tabbar.OpenNewTab(idlw, !first);    
                        }
                        first = false;
                        // todo: bring to front
                    }
                });
            }
            else {
                StaticReg.CreateWindowIDLs.Assign(lstIDLs.Skip(1));
                using(IDLWrapper idlw = new IDLWrapper(lstIDLs[0])) {
                    OpenWindow(idlw);
                }
            }
        }

        private void DoFileTools(int index) {
            // desktop thread

            // 0	copy path
            // 1	copy name
            // 2	copy path current
            // 3	copy name current
            // 4	file hash
            // 5	show SubDirTip for selected folder
            // 6	copy file hash

            try {
                if(index == 2 || index == 3) {
                    // Send desktop path/name to Clipboard.
                    string str = String.Empty;
                    if(index == 2) {
                        str = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    }
                    else {
                        byte[] idl = new byte[] {0, 0};
                        using(IDLWrapper idlw = new IDLWrapper(idl)) {
                            if(idlw.Available) {
                                str = idlw.DisplayName;
                            }
                        }
                    }

                    if(str.Length > 0) {
                        QTUtility2.SetStringClipboard(str);
                    }
                    return;
                }

                // File Hash
                if(index == 4 || index == 6) {
                    List<string> lstPaths = new List<string>();
                    foreach(IDLWrapper idlw in ShellBrowser.GetItems(true)) {
                        if(idlw.IsLink) {
                            string pathLinkTarget = ShellMethods.GetLinkTargetPath(idlw.Path);
                            if(File.Exists(pathLinkTarget)) {
                                lstPaths.Add(pathLinkTarget);
                            }
                        }
                        else if(idlw.IsFileSystemFile) {
                            lstPaths.Add(idlw.Path);
                        }   
                    }

                    /* TODO
                    if(index == 4) {
                        FileHashComputer.ShowForm(lstPaths.ToArray());
                    }
                    else {
                        FileHashComputer.GetForPath(lstPaths, hwndListView);
                    }*/
                    return;
                }


                // Show subdirtip.
                if(index == 5) {
                    slvDesktop.ShowAndClickSubDirTip();
                    return;
                }

                // Copy name/path
                if(index == 0 || index == 1) {
                    string str = ShellBrowser.GetItems(true)
                            .Select(idlw => index == 0 ? idlw.ParseName : idlw.DisplayName)
                            .StringJoin(Environment.NewLine);
                    if(str.Length > 0) QTUtility2.SetStringClipboard(str);
                }
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex);
            }
        }


        #endregion


        #region ---------- Inner Classes ----------

        private sealed class TitleMenuItem : ToolStripMenuItem {
            private static StringFormat sf;
            private static Bitmap bmpTitle;
            public static bool DrawBackground { private get; set; }

            private Bitmap bmpArrow_Cls, bmpArrow_Opn;
            public MenuGenre Genre { get; private set; }
            public bool IsOpened { get; private set; }

            public TitleMenuItem(MenuGenre genre, bool fOpened) {
                Genre = genre;
                IsOpened = fOpened;

                bmpArrow_Opn = Resources_Image.menuOpen;
                bmpArrow_Cls = Resources_Image.menuClose;

                if(sf == null)
                    Init();
            }

            private static void Init() {
                sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;

                bmpTitle = Resources_Image.TitleBar;
            }

            protected override void Dispose(bool disposing) {
                if(bmpArrow_Opn != null) {
                    bmpArrow_Opn.Dispose();
                    bmpArrow_Opn = null;
                }
                if(bmpArrow_Cls != null) {
                    bmpArrow_Cls.Dispose();
                    bmpArrow_Cls = null;
                }

                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e) {
                if(DrawBackground) {
                    Rectangle rct = new Rectangle(1, 0, Bounds.Width, Bounds.Height);

                    // draw background	100x24
                    e.Graphics.DrawImage(bmpTitle,
                            new Rectangle(new Point(1, 0), new Size(1, Bounds.Height)),
                            new Rectangle(Point.Empty, new Size(1, 24)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle,
                            new Rectangle(new Point(2, 0), new Size(Bounds.Width - 3, 1)),
                            new Rectangle(new Point(1, 0), new Size(98, 1)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle,
                            new Rectangle(new Point(Bounds.Width - 1, 0), new Size(1, Bounds.Height)),
                            new Rectangle(new Point(99, 0), new Size(1, 24)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle,
                            new Rectangle(new Point(2, Bounds.Height - 1), new Size(Bounds.Width - 3, 1)),
                            new Rectangle(new Point(1, 23), new Size(98, 1)), GraphicsUnit.Pixel);
                    e.Graphics.DrawImage(bmpTitle,
                            new Rectangle(new Point(2, 1), new Size(Bounds.Width - 3, Bounds.Height - 2)),
                            new Rectangle(new Point(1, 1), new Size(98, 22)), GraphicsUnit.Pixel);

                    // draw overwrite highlight
                    if(Selected) {
                        SolidBrush sb = new SolidBrush(Color.FromArgb(96, SystemColors.Highlight));
                        e.Graphics.FillRectangle(sb, rct);
                        sb.Dispose();
                    }

                    // draw arrow
                    if(HasDropDownItems) {
                        int y = (rct.Height - 16)/2;
                        if(y < 0)
                            y = 5;
                        else
                            y += 5;

                        using(SolidBrush sb = new SolidBrush(Color.FromArgb(Selected ? 255 : 128, Color.White))) {
                            Point p = new Point(rct.Width - 15, y);
                            Point[] ps = {p, new Point(p.X, p.Y + 8), new Point(p.X + 4, p.Y + 4)};
                            e.Graphics.FillPolygon(sb, ps);
                        }
                    }

                    // draw string
                    e.Graphics.DrawString(Text, Font, Brushes.White,
                            new RectangleF(34, 2, rct.Width - 34, rct.Height - 2), sf);
                }
                else
                    base.OnPaint(e);

                // draw image		//Resources_Image.menuOpen : Resources_Image.menuClose,
                e.Graphics.DrawImage(IsOpened ? bmpArrow_Cls : bmpArrow_Opn, new Rectangle(5, 4, 16, 16));
            }

            protected override Point DropDownLocation {
                get {
                    // show dropdown in the screen where parent dropdown is contained.
                    // multi monitor support...

                    Point pnt = base.DropDownLocation;

                    if(pnt != Point.Empty && HasDropDownItems) {
                        ToolStrip tsOwner = Owner;
                                // this ToolStrip must be ToolStripDropDown. not MenuStrip or something.
                        if(tsOwner != null && !Screen.FromPoint(tsOwner.Bounds.Location).Bounds.Contains(pnt)) {
                            pnt.X = tsOwner.Bounds.X - DropDown.Width;
                        }
                    }

                    return pnt;
                }
            }
        }

        #endregion


        #region ---------- IDeskBand2 members ----------

        public void CanRenderComposited(out bool pfCanRenderComposited) {
            pfCanRenderComposited = true;
        }

        public void SetCompositionState(bool fCompositionEnabled) {
        }

        public void GetCompositionState(out bool pfCompositionEnabled) {
            pfCompositionEnabled = true;
        }

        #endregion


        #region ---------- Register / Unregister ----------


        [ComRegisterFunction]
        private static void Register(Type t) {
            // todo: localize?
            const string strDesktopTool = "QTTab Desktop Tool";

            // CLSID
            using(RegistryKey rkClass = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + t.GUID.ToString("B"))) {
                rkClass.SetValue(null, strDesktopTool);
                rkClass.SetValue("MenuText", strDesktopTool);
                rkClass.SetValue("HelpText", strDesktopTool);
                rkClass.CreateSubKey(@"Implemented Categories\{00021492-0000-0000-C000-000000000046}");
            }
        }

        [ComUnregisterFunction]
        private static void Unregister(Type t) {
            try {
                using(RegistryKey rkClass = Registry.ClassesRoot.CreateSubKey(@"CLSID")) {
                    rkClass.DeleteSubKeyTree(t.GUID.ToString("B"));
                }
            }
            catch {
            }
        }

        #endregion

    }
}
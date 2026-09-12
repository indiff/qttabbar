using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using QTTabBarLib.FileRename;
using QTTabBarLib.Interop;

namespace QTTabBarLib
{
    internal sealed class ExplorerManager : NativeWindow, IDisposable, ICommandInvokerWindow
    {
        [ThreadStatic]
        public static ExplorerManager ThreadInstance;
        [ThreadStatic]
        public static IntPtr ThreadExplorerHandle;
        [ThreadStatic]
        private static int windowDpi;
        [ThreadStatic]
        public static bool StartUpProcessComplete;
        private SHDocVw.WebBrowser explorer;
        private IShellBrowser shellBrowser;
        // private IBandSite bandSite;
        // private ShellFolderView shellFolderView;
        private HookProc hookProc_Key;
        private HookProc hookProc_Mouse;
        private HookProc hookProc_GetMsg;
        private HookProc hookProc_CallWndProc;
        private IntPtr hHook_Key;
        private IntPtr hHook_Mouse;
        private IntPtr hHook_GetMsg;
        private IntPtr hHook_CallWndProc;
        private IntPtr hwndExplorer;
        private IntPtr hwndRebar;
        private IntPtr hwndListView;
        private IntPtr hwndShellTab;
        private IntPtr hwndAddressBandRoot;
        private IntPtr hwndBreadcrumbParent;
        private IntPtr hwndBreadcrumbToolbarWindow;
        private WindowSubclass explorerWindowSubclass;
        private WindowSubclass rebarWindowSubclass;
        private WindowSubclass shellTabWindowSubclass;
        private WindowSubclass addressBandRootWindowSubclass;
        private WindowSubclass rebarParentWindowSubclass;
        private bool fNowClosing;
        private bool fUnregistered;
        // private NavigationPane navPane;
        // private NavigationPane navigationPaneMouseEvent;
        // private ToolbarImageHelper toolbarImageHelper;
        // private PluginManager pluginManager;
        private static int DefaultRebarCOLORREF = -1;
        private bool disposed;
        private bool fVerticalBarDisplayed;
        private bool fHorizontalExplorerBarDisplayed;
        private bool fFirstNavigationComplete;
        private bool fFirstEnumDone;
        private bool fNowInitializingInstallation;
        private bool fProcessingPendings;
        private byte[] currentIDL = new byte[2];
        private IContainer components;
        // private SyncControl syncControl;
        private bool fHorizontalyMaximized;
        private Rectangle rctHorizontalyMaximizing;
        private Rectangle rctHorizontalyMaximized;
        private Point pntWndDragStartHMaximized;
        private bool fInitializedOnSetSite;
        public static bool fNowActivateNofocusExpected;
        private bool fTreeViewColorPending = true;
        private bool fTreeViewColorPendingOnStartUp;
        private bool fSuppressRightButtonUp;
        [ThreadStatic]
        public static bool CurrentExplorerIsRooted;
        // private static ResourceCache<BmpCacheKey, Bitmap> watermarkImageCache = new ResourceCache<BmpCacheKey, Bitmap>(new Func<BmpCacheKey, Bitmap>(KeyResourceConverters.ToBitmap));
        private System.Windows.Forms.Timer timerPollingShrinkToolbar;
        private System.Windows.Forms.Timer timerShrinkStart;
        private System.Windows.Forms.Timer timerVerticalBarShrinkStart;
        private System.Windows.Forms.Timer timerBottomBarShrinkStart;
        private System.Windows.Forms.Timer timerExpandTimerOnMaximized;
        private bool fHorizontalToolbarShrinkPending;
        private bool fVerticalBarShrinkPending;
        private bool fBottomBarShrinkPending;
        private const int POLLINGINTERVAL_SHRINK = 333;
        private static int FOLDERBAND_HEIGHT = 34;
        private ContextMenuStripEx contextMenFolderBand;
        private TabSwitchForm tabSwitcher;
        private bool fSuppressFocusing41008;
        // private static ConcurrentDictionary<ExplorerManager, SHOWWINDOW> dicNotifyIcon;
        private static NotifyIcon notifyIcon;
        private static ContextMenuStripEx contextMenuNotifyIcon;
        private static Icon icoNotify;
        private static ExplorerManager explorerManager_NotifyIconOwner;
        private static bool fNotifyIconChanged;
        private static bool fNotifyIconContextMenued;
        public static bool fMergingAllWindow;
        private FileRenameDialog fileRenameDialog;
        private ExplorerManager.ToolbarManager toolbarManager;
        private bool fInteractivePluginsUnloadingPending;
        private bool fNoMoreDWMCOLORIZATIONCOLORCHANGED;
        private int modalCounter;
        // private ExtraViewResizer extraViewResizer;
        private bool fFirstDpiChangeNotified;
        // private Rebar rebar;
        private static volatile bool fNowOptionDialogOpening;
        private bool fOptionDialogCreated;
        // private static OptionDialog optionsDialog;
        // private static RefreshProcessInfo refreshProcessInfo;
        // private Dictionary<int, EventData> dicUserEventData;
        internal QTabItem tabDragSourceInTheWindow;
        // public IList<ItemIDList> PendingFoldersDefault;
        public bool PendingFoldersDefaultProcessed;
        public IList<string> PendingGroupsDefault;
        public IList<string> PendingGroupsExtraView;
        // public IList<ItemIDList> PendingFoldersExtraView;
        // public CommandInfo PendingCommandExtraView;
        public QTabItem PendingTabExtraView;
        public QTabItem PendingModifyTabExtraView2nd;
        public QTabItem PendingModifyTabExtraView3rd;
        public QTabItem PendingTabDefaultView;
        public bool StartUpSelectionPendingExtraView;

        public ExplorerManager.ToolbarManager Toolbars
        {
            get
            {
                return this.toolbarManager;
            }
        }

        public static int WindowDpi
        {
            get
            {
                return ExplorerManager.windowDpi != 0 ? ExplorerManager.windowDpi : 96;
            }
        }

        public IntPtr CommandWindowHandle { get; private set; }

        public static float WindowScaling
        {
            get
            {
                return ExplorerManager.windowDpi != 0 ? (float)ExplorerManager.windowDpi / 96f : 1f;
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public sealed class ToolbarManager
        {
            private ExplorerManager explorerManager;
            private QTTabBarClass tabBar;
            private QTTabBarClass tabBarAnother;
            // private QCommandBar commandBar1st;
            // private QCommandBar2nd commandBar2nd;
            // private QCommandBarVertical commandBarVrt;
            // private QManagementBar managementBar;
            private QTSecondViewBar secondViewBar;
            // private QThirdViewBar thirdViewBar;
            private bool fNowHiding3rdViewBar;

            public ToolbarManager(ExplorerManager explorerManager)
            {
                this.explorerManager = explorerManager;
            }

             public void Show(Toolbar toolbar, bool fShow, bool fSaveBandLayout = false)
             {
        try
        {
          string str = (string) null;
          switch (toolbar)
          {
            case Toolbar.TabBar:
              str = typeof (QTTabBarClass).GUID.ToString("B");
              break;
            case Toolbar.CommandBar1:
              // str = typeof (QCommandBar).GUID.ToString("B");
              break;
            case Toolbar.CommandBar2:
              // str = typeof (QCommandBar2nd).GUID.ToString("B");
              break;
            case Toolbar.CommandBarVertical:
              // if (this.IsThirdViewBarVisible && !this.fNowHiding3rdViewBar)
              // {
              //   if (fShow)
              //   {
              //     this.ThirdViewBar.ShowVerticalCommandBar();
              //     return;
              //   }
              //   this.ThirdViewBar.HideVerticalCommandBar();
              //   return;
              // }
              // this.save3rdViewBarWidth();
              // fSaveBandLayout = false;
              // str = typeof (QCommandBarVertical).GUID.ToString("B");
              break;
            case Toolbar.BottomTabBar:
              fSaveBandLayout = false;
              // str = typeof (QTHorizontalExplorerBar).GUID.ToString("B");
              break;
            case Toolbar.ManagementBar:
              fSaveBandLayout = false;
              // if (!fShow)
              //   this.NowHidingManagementBarByCode = true;
              // str = typeof (QManagementBar).GUID.ToString("B");
              break;
            case Toolbar.SecondViewBar:
              fSaveBandLayout = false;
              // str = typeof (QSecondViewBar).GUID.ToString("B");
              break;
            case Toolbar.ThirdViewBar:
              fSaveBandLayout = false;
              // str = typeof (QThirdViewBar).GUID.ToString("B");
              break;
          }
          if (str != null)
          {
            object obj1 = (object) str;
            object obj2 = (object) fShow;
            /*try
            {
              // ISSUE: variable of a compiler-generated type
              SHDocVw.WebBrowser explorer = this.explorerManager.explorer;
              ref object local1 = ref obj1;
              ref object local2 = ref obj2;
              object missing = Type.Missing;
              ref object local3 = ref missing;
              // ISSUE: reference to a compiler-generated method
              explorer.ShowBrowserBar(ref local1, ref local2, ref local3);
            }
            catch (Exception ex)
            {
            }*/
          }
          if (!fSaveBandLayout)
            return;
          // this.explorerManager.SaveCurrentBandLayout();
        }
        finally
        {
          this.fNowHiding3rdViewBar = false;
        }
      }

        }
        
    }

}

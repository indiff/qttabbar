using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QTTabBarLib.Interop
{
    internal sealed class ExplorerManager : NativeWindow, IDisposable
        // , ICommandInvokerWindow
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
    private IBandSite bandSite;
    private ShellFolderView shellFolderView;
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
    private NavigationPane navPane;
    private NavigationPane navigationPaneMouseEvent;
    private ToolbarImageHelper toolbarImageHelper;
    private PluginManager pluginManager;
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
    private SyncControl syncControl;
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
    private static ResourceCache<BmpCacheKey, Bitmap> watermarkImageCache = new ResourceCache<BmpCacheKey, Bitmap>(new Func<BmpCacheKey, Bitmap>(KeyResourceConverters.ToBitmap));
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
    private static ConcurrentDictionary<ExplorerManager, SHOWWINDOW> dicNotifyIcon;
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
    private ExtraViewResizer extraViewResizer;
    private bool fFirstDpiChangeNotified;
    private Rebar rebar;
    private static volatile bool fNowOptionDialogOpening;
    private bool fOptionDialogCreated;
    private static OptionDialog optionsDialog;
    private static RefreshProcessInfo refreshProcessInfo;
    private Dictionary<int, EventData> dicUserEventData;
    internal QTabItem tabDragSourceInTheWindow;
    public IList<ItemIDList> PendingFoldersDefault;
    public bool PendingFoldersDefaultProcessed;
    public IList<string> PendingGroupsDefault;
    public IList<string> PendingGroupsExtraView;
    public IList<ItemIDList> PendingFoldersExtraView;
    public CommandInfo PendingCommandExtraView;
    public QTabItem PendingTabExtraView;
    public QTabItem PendingModifyTabExtraView2nd;
    public QTabItem PendingModifyTabExtraView3rd;
    public QTabItem PendingTabDefaultView;
    public bool StartUpSelectionPendingExtraView;

    public static int WindowDpi { return  ExplorerManager.windowDpi != 0 ? ExplorerManager.windowDpi : 96; }

    public static float WindowScaling { ExplorerManager.windowDpi != 0 ? (float) ExplorerManager.windowDpi / 96f : 1f;

    public event WindowSubclass.SubclassingProcedure ExplorerMessageCaptured;

    public event WindowSubclass.SubclassingProcedure ShellTabMessageCaptured;

    public event EventHandler<ExplorerManagerEventArgs> SubFolderMenuEvents;

    public event EventHandler<ExplorerManagerEventArgs> NavigateComplete;

    public event EventHandler<ExplorerManagerEventArgs> NavigateError;

    public event EventHandler<KeyHookEventArgs> KeyDown;

    public event EventHandler<KeyHookEventArgs> KeyUp;

    public event HookProc MouseHookProc;

    public event EventHandler<ExplorerManagerEventArgs> ExplorerManagerEvent;

    public event ThreadMessageEventHandler CloseMessagePosted;

    public ExplorerManager(object site, int threadid)
    {
      try
      {
        ExplorerManager.ThreadInstance = this;
        ExplorerManager.ThreadExplorerHandle = IntPtr.Zero;
        ExplorerManager.StartUpProcessComplete = false;
        ExplorerManager.CurrentExplorerIsRooted = false;
        this.ThreadID = threadid;
        this.ProcessID = PInvoke.GetCurrentProcessId();
        _IServiceProvider iserviceProvider = (_IServiceProvider) site;
        object ppvObject1;
        int num1 = iserviceProvider.QueryService(COMGUIDS.SID_SWebBrowserApp, COMGUIDS.IID_IWebBrowserApp, out ppvObject1);
        if (!MCR.SUCCEEDED(num1))
          throw new Exception("QueryService failed - SID_SWebBrowserApp", Marshal.GetExceptionForHR(num1));
        this.explorer = ppvObject1 as SHDocVw.WebBrowser;
        ExplorerManager.ThreadExplorerHandle = this.hwndExplorer = (IntPtr) this.explorer.HWND;
        this.Dpi = ExplorerManager.windowDpi = DpiManager.GetDpiForWindow(ExplorerManager.ThreadExplorerHandle);
        object ppvObject2;
        int num2 = iserviceProvider.QueryService(COMGUIDS.SID_SShellBrowser, COMGUIDS.IID_IShellBrowser, out ppvObject2);
        if (!MCR.SUCCEEDED(num2))
          throw new Exception("QueryService failed - SID_SShellBrowser", Marshal.GetExceptionForHR(num2));
        this.shellBrowser = ppvObject2 as IShellBrowser;
        this.pluginManager = new PluginManager(this);
        this.components = (IContainer) new Container();
        this.syncControl = new SyncControl();
        this.toolbarManager = new ExplorerManager.ToolbarManager(this);
        this.extraViewResizer = new ExtraViewResizer(this);
        this.CreateHandle(new CreateParams());
        this.RegisterPrimaryExplorerManager(true);
        this.InitializeCustomizer();
        this.InstallHooks();
        this.InitializeWindow();
        Application.EnableVisualStyles();
        Application.EnterThreadModal += new EventHandler(this.Application_EnterThreadModal);
        Application.LeaveThreadModal += new EventHandler(this.Application_LeaveThreadModal);
        this.pluginManager.LoadStartupPlugins();
        this.ViewSyncManager = new ViewSyncManager(this);
        this.ViewSyncManager.SyncCanceled += new EventHandler(this.ViewSyncManager_SyncCanceled);
        ActionDelayer.Add(new Func<bool>(this.ProcessDelayedStartUps), 500, 150, 4);
      }
      catch (Exception ex)
      {
        DebugUtil.AppendToCriticalExceptionLog(ex);
      }
    }

    public void InitializeBandSite(object site)
    {
      if (this.bandSite != null)
        return;
      this.bandSite = site as IBandSite;
      if (this.rebar == null)
        return;
      this.rebar = new Rebar(this.bandSite, this.hwndRebar);
    }

    private void InitializeCustomizer()
    {
      this.View = new DefaultView(this);
      this.View.NavigationComplete += new EventHandler<ExplorerBrowserNavigationEventArgs>(this.View_NavigationComplete);
      this.View.NavigationFailed += new EventHandler<ExplorerBrowserNavigationEventArgs>(this.View_NavigationFailed);
      this.View.CustomViewEvent += new EventHandler<CustomViewEventArgs>(this.View_CustomViewEvent);
      this.View.TipControlEvent += new EventHandler<TipControlEventArgs>(this.View_TipControlEvent);
      this.View.Scrolled += new EventHandler<ViewScrollEventArgs>(this.View_Scrolled);
    }

    public void Initialize(IntPtr hwndRebar)
    {
      this.hwndRebar = hwndRebar;
      if (this.hwndRebar != IntPtr.Zero)
      {
        if (this.rebarWindowSubclass == null)
        {
          this.rebarWindowSubclass = new WindowSubclass(this.hwndRebar, new WindowSubclass.SubclassingProcedure(this.rebarSubclassProc));
          if (Config.Bool(Scts.RebarBackgroundColor))
          {
            if (ExplorerManager.DefaultRebarCOLORREF == -1)
              ExplorerManager.DefaultRebarCOLORREF = (int) (long) PInvoke.SendMessage(this.hwndRebar, 1044, IntPtr.Zero, IntPtr.Zero);
            PInvoke.SendMessage(this.hwndRebar, 1043, IntPtr.Zero, (IntPtr) MCR.MakeCOLORREF(Config.RebarBackgroundColor));
          }
        }
        if (this.rebarParentWindowSubclass == null)
          this.rebarParentWindowSubclass = new WindowSubclass(PInvoke.GetWindowLongPtr(this.hwndRebar, GWL.HWNDPARENT), new WindowSubclass.SubclassingProcedure(this.rebarParentSubclassProc));
      }
      if (!this.fInitializedOnSetSite && Config.Bool(Scts.ViewVistaLayout) && this.shellBrowser is IFolderViewOptions shellBrowser)
        shellBrowser.SetFolderViewOptions(FOLDERVIEWOPTIONS.FVO_VISTALAYOUT, FOLDERVIEWOPTIONS.FVO_VISTALAYOUT);
      bool fHorizontal = Config.Bool(Scts.ExplorerBarPriority);
      this.InitializeExplorerBars(fHorizontal);
      this.InitializeExplorerBars(!fHorizontal);
      this.fInitializedOnSetSite = true;
    }

    private void InitializeExplorerBars(bool fHorizontal)
    {
      if (fHorizontal)
      {
        if (this.fHorizontalExplorerBarDisplayed)
          return;
        this.fHorizontalExplorerBarDisplayed = true;
        if (Config.Bool(Scts.BottomTabBarEnabled))
        {
          this.Toolbars.Show(Toolbar.BottomTabBar, true);
        }
        else
        {
          if (!Config.Bool(Scts.ExtraViewBarEnabled2nd))
            return;
          this.Toolbars.Show(Toolbar.SecondViewBar, true);
        }
      }
      else
      {
        if (this.fVerticalBarDisplayed)
          return;
        this.fVerticalBarDisplayed = true;
        if (Config.Bool(Scts.CommandBar_VerticalEnabled))
        {
          if (Config.Bool(Scts.AutoHideCommandBarV))
            this.VerticalBarShrunken = true;
          else
            this.Toolbars.Show(Toolbar.CommandBarVertical, true);
        }
        else
        {
          if (!Config.Bool(Scts.ExtraViewBarEnabled3rd))
            return;
          this.Toolbars.Show(Toolbar.ThirdViewBar, true);
        }
      }
    }

    private void RegisterPrimaryExplorerManager(bool fInit)
    {
      if (!this.IsAlive)
        return;
      if (!fInit)
        InstanceManager.PushExplorer(this.ThreadID, this);
      using (RegistryKey rk = RegistryUtil.OpenUserRoot(true))
      {
        if (rk == null)
          return;
        RegistryUtil.WritePtr(nameof (ExplorerManager), rk, this.Handle);
      }
    }

    public object Invoke(Delegate d, object[] args = null) { !this.IsClosing && !this.IsDisposed && this.syncControl != null ? this.syncControl.Invoke(d, args) : (object) null;

    public IAsyncResult BeginInvoke(Delegate d, object[] args = null) { !this.IsClosing && !this.IsDisposed && this.syncControl != null ? this.syncControl.BeginInvoke(d, args) : (IAsyncResult) null;

    public bool InvokeRequired { this.syncControl != null && this.syncControl.InvokeRequired;

    protected override unsafe void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        case 74:
          if (this.InModal)
            break;
          COPYDATASTRUCT* lparam = (COPYDATASTRUCT*) (void*) m.LParam;
          int num1 = MCR.LOWORD(lparam->dwData);
          switch (num1)
          {
            case 7:
              TargetView targetView1 = (TargetView) MCR.HIWORD(lparam->dwData);
              QTabItem tab = QTabItem.Deserialize(lparam->lpData, targetView1 != 0);
              if (tab == null)
                return;
              if (targetView1 == TargetView.Default)
              {
                if (this.Toolbars.IsTabBarAvailable)
                {
                  this.Toolbars.TabBar.ModifyActiveTab(tab, false);
                  return;
                }
                this.PendingTabDefaultView = tab;
                return;
              }
              if (this.Toolbars.IsExtraViewAvailable(targetView1))
              {
                this.Toolbars.GetExtraViewBar(targetView1).ModifyActiveTab(tab, false);
                return;
              }
              if (targetView1 != TargetView.Second)
              {
                if (targetView1 != TargetView.Third)
                  return;
                this.PendingModifyTabExtraView3rd = tab;
                return;
              }
              this.PendingModifyTabExtraView2nd = tab;
              return;
            case 8:
            case 9:
              if (PInvoke.IsIconic(this.hwndExplorer))
                PInvoke.ShowWindow(this.hwndExplorer, SHOWWINDOW.RESTORE);
              int[] numArray = InterProcessCommunication.DeserializePtr<int>(lparam->lpData, lparam->cbData, out InterProcComHeader _);
              if (num1 == 8)
              {
                if (numArray == null || numArray.Length != 2)
                  return;
                this.MoveWindow(new Point(numArray[0], numArray[1]));
                return;
              }
              if (numArray == null || numArray.Length != 4)
                return;
              this.MoveWindowToScreen(new Rectangle(numArray[0], numArray[1], numArray[2], numArray[3]));
              return;
            case 16:
              m.Result = (IntPtr) 1;
              InterProcComHeader header1;
              byte[][] idls = InterProcessCommunication.DeserializePtr<byte[]>(lparam->lpData, lparam->cbData, out header1);
              BarCommandOptions option1_1 = (BarCommandOptions) header1.Option1;
              TargetView targetView2 = (TargetView) header1.Option2;
              switch (targetView2)
              {
                case TargetView.Other:
                  targetView2 = this.Toolbars.GetTheOtherView(this.GetFocusedTargetView());
                  break;
                case TargetView.None:
                  TargetView theOtherView = (TargetView) Config.Get(Scts.CapturingView);
                  bool flag1 = option1_1.HasFlag((Enum) BarCommandOptions.OtherView);
                  if (theOtherView != 0 ^ flag1)
                  {
                    if (flag1)
                      theOtherView = this.Toolbars.GetTheOtherView(theOtherView);
                    if (this.CaptureByExtraView((IList<byte[]>) idls, theOtherView, option1_1))
                      return;
                  }
                  targetView2 = TargetView.Default;
                  break;
              }
              this.OpenInTabBar((IList<byte[]>) idls, targetView2, option1_1);
              return;
            case 17:
              m.Result = (IntPtr) 1;
              InterProcComHeader header2;
              string[] groups = InterProcessCommunication.DeserializePtr<string>(lparam->lpData, lparam->cbData, out header2);
              BarCommandOptions option1_2 = (BarCommandOptions) header2.Option1;
              TargetView targetView3 = (TargetView) header2.Option2;
              if (targetView3 == TargetView.None)
              {
                int num2 = Config.Get(Scts.CapturingView);
                if (num2 != 0 && this.CaptureByExtraView((IList<string>) groups, option1_2, num2 == 1 ? TargetView.Second : TargetView.Third))
                  return;
                targetView3 = TargetView.Default;
              }
              this.OpenGroupsInTabBar(groups, option1_2, targetView3);
              return;
            case 18:
              InterProcComHeader header3;
              if (!this.NavigateInPlaceExtraView(InterProcessCommunication.DeserializePtr<byte>(lparam->lpData, lparam->cbData, out header3), (BarCommandOptions) header3.Option1, (TargetView) header3.Option2))
                return;
              m.Result = (IntPtr) 1;
              return;
            default:
              return;
          }
        case 32773:
          if (this.InModal)
            break;
          bool flag2 = false;
          foreach (TabBarBase tabBar in this.Toolbars.TabBars)
          {
            if (-1 != tabBar.TabManager.RestoreTabs())
              flag2 = true;
          }
          if (flag2)
          {
            if (Config.Bool(Scts.MoveCaptorWindow))
              WindowUtil.MoveWindow(this.Handle, Control.MousePosition);
            this.BringToFront();
          }
          m.Result = (IntPtr) 1;
          break;
        case 47616:
          this.HandleExplorerManagerMessage(ref m);
          break;
        case 47617:
          m.Result = (IntPtr) (this.InModal ? 1 : 0);
          break;
        case 48643:
          Config.Initialize(true);
          Initializer.LoadSettings();
          ExplorerManager.RefreshProcess(false, m.WParam != IntPtr.Zero, m.LParam != IntPtr.Zero);
          break;
        case 48647:
          this.RefreshThread();
          break;
        case 48648:
          ShellNotifyInfo* sni = (ShellNotifyInfo*) (void*) m.LParam;
          bool fPrimary = m.WParam != IntPtr.Zero;
          switch ((SHCNE) sni->lEvent)
          {
            case SHCNE.RENAMEITEM:
            case SHCNE.RENAMEFOLDER:
              if (this.Toolbars.FirstVisibleCommandControlOwner())
              {
                using (ShellItem shellItem = new ShellItem(this.currentIDL, false))
                {
                  if (PInvoke.ILIsParent(shellItem.PIDL, sni->pidl2, true))
                  {
                    this.NotifyBrowserRefresh(false);
                    break;
                  }
                  break;
                }
              }
              else
                break;
            case SHCNE.CREATE:
            case SHCNE.DELETE:
            case SHCNE.MKDIR:
            case SHCNE.RMDIR:
              if (this.Toolbars.FirstVisibleCommandControlOwner())
              {
                using (ShellItem shellItem = new ShellItem(this.currentIDL, false))
                {
                  if (PInvoke.ILIsParent(shellItem.PIDL, sni->pidl1, true))
                  {
                    this.NotifyBrowserRefresh(false);
                    break;
                  }
                  break;
                }
              }
              else
                break;
            case SHCNE.UPDATEITEM:
              if (fPrimary)
                Graphic.UpdateImageList(sni->pidl1);
              if (this.View.VistaLayout && Config.Bool(Scts.ViewWatermarking))
              {
                this.RefreshAllWatermarsk(sni->pidl1);
                break;
              }
              break;
          }
          bool flag3 = true;
          if (this.Toolbars.IsTabBarAvailable)
            flag3 = this.Toolbars.TabBar.NotifyShellChange(*sni);
          if (!flag3)
            break;
          this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco { cco.NotifyShellChange(*sni, fPrimary)));
          this.Toolbars.AllTabAvailableExtraViewBar((Action<QSecondViewBar>) (qsv { qsv.HandleShellChange(*sni)));
          break;
        default:
          if (m.Msg == RegisteredMessage.NotifyPreviewTipEvent && (int) (long) m.WParam == 1)
          {
            this.View.OnThumbnailMouseMove();
            break;
          }
          base.WndProc(ref m);
          break;
      }
    }

    private void HandleExplorerManagerMessage(ref Message msg)
    {
      switch (MCR.LOWORD(msg.WParam))
      {
        case 0:
          this.InvokeCommand(BarCommand.ViewRedraw);
          if (ExplorerManager.optionsDialog == null || !ExplorerManager.optionsDialog.IsHandleCreated)
            break;
          if (!this.fOptionDialogCreated)
            break;
          try
          {
            ExplorerManager.optionsDialog.Invoke((Delegate) new MethodInvoker(this.odCallback_NotifyViewColor));
            break;
          }
          catch (Exception ex)
          {
            break;
          }
        case 1:
          this.InvokeCommand(BarCommand.GlobalOption);
          break;
        case 2:
          msg.Result = (IntPtr) (this.Toolbars.IsTabBarAvailable ? 1 : 0);
          break;
        case 3:
          if (Config.Bool(Scts.BottomTabBarEnabled))
          {
            this.Toolbars.Show(Toolbar.BottomTabBar, true);
            break;
          }
          if (!this.Toolbars.IsBottomTabBarAvailable)
            break;
          this.Toolbars.Show(Toolbar.TabBar, true, true);
          break;
        case 4:
          CommandBarSyncStaticCommand command = (CommandBarSyncStaticCommand) MCR.LOWORD(msg.LParam);
          int value = MCR.HIWORD(msg.LParam);
          this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco { cco.RefreshBarStaticState(command, value)));
          break;
        case 5:
          CommandBarSyncGenres flags = (CommandBarSyncGenres) MCR.LOWORD(msg.LParam);
          CommandBarType types = (CommandBarType) MCR.HIWORD(msg.LParam);
          this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco {
          {
            if (!types.HasFlag((Enum) cco.BarType))
              return;
            cco.RefreshInstance(flags);
          }));
          break;
        case 6:
          this.Toolbars.AllExtraCustomViews((Action<ExtraView>) (view { view.Refresh()));
          break;
        case 7:
          msg.Result = this.hwndExplorer;
          break;
        case 8:
          msg.Result = (IntPtr) QEnvironment.CurrentVersion.Major;
          break;
        case 9:
          ReflectionUtil.RendererRefreshCache();
          this.Toolbars.AllBandObjects.ForEach<BandObject>((Action<BandObject>) (bo { bo.RefreshThemeColors()));
          PInvoke.RedrawWindow(this.hwndExplorer, IntPtr.Zero, IntPtr.Zero, RDW.INVALIDATE | RDW.ERASE | RDW.ALLCHILDREN | RDW.UPDATENOW);
          break;
      }
    }

    private bool ProcessDelayedStartUps()
    {
      bool fFore = Config.Bool(Scts.FolderTreeForeColorEnabled);
      bool fBack = Config.Bool(Scts.FolderTreeBackColorEnabled);
      if (!(fBack | fFore))
        return true;
      this.fTreeViewColorPending = true;
      return this.TrySetTreeColors(fFore, fBack);
    }

    private void ProcessStartupsFallback()
    {
      TargetView view = (TargetView) Config.Get(Scts.CapturingView);
      if (view == TargetView.Default)
      {
        if (ExplorerOpener.MultipleItems)
        {
          this.fProcessingPendings = true;
          this.Toolbars.Show(Config.Bool(Scts.TabBarPriority) ? Toolbar.BottomTabBar : Toolbar.TabBar, true);
          this.fProcessingPendings = false;
        }
        else
          ExplorerOpener.Read();
      }
      else
      {
        this.fProcessingPendings = true;
        this.Toolbars.ShowExtraView(view, true);
        this.fProcessingPendings = false;
      }
    }

    public DefaultView View { get; private set; }

    public NavigationPane NavigationPane
    {
      get
      {
        if (this.navPane == null)
          this.navPane = new NavigationPane(this.ShellBrowser);
        return this.navPane;
      }
    }

    private bool ExplorerIsRooted
    {
      get
      {
        if (!QEnvironment.IsShellProcess)
        {
          using (ShellItem shellItem = this.View.ShellItem)
          {
            using (ShellItem parent1 = shellItem.Parent)
            {
              using (ShellItem si = new ShellItem(shellItem.Path))
              {
                using (ShellItem parent2 = si.Parent)
                {
                  if (parent1.IDL != null)
                  {
                    if (parent1.IDL.Length == 2)
                    {
                      if (parent2.IDL != null && parent2.IDL.Length == 2)
                      {
                        if (shellItem.IsIdentical(si))
                          goto label_22;
                      }
                      return true;
                    }
                  }
                }
              }
            }
          }
        }
label_22:
        return false;
      }
    }

    private void OnExplorerClosing()
    {
      this.fNowClosing = true;
      InstanceManager.UnregisterExplorerManager(this);
      this.fUnregistered = true;
      try
      {
        if (ExplorerManager.dicNotifyIcon != null)
        {
          ExplorerManager.dicNotifyIcon.TryRemove(this, out SHOWWINDOW _);
          if (ExplorerManager.explorerManager_NotifyIconOwner == this)
          {
            if (ExplorerManager.notifyIcon != null)
            {
              ExplorerManager.notifyIcon.Dispose();
              ExplorerManager.notifyIcon = (NotifyIcon) null;
            }
            if (ExplorerManager.contextMenuNotifyIcon != null)
            {
              ExplorerManager.contextMenuNotifyIcon.Dispose();
              ExplorerManager.contextMenuNotifyIcon = (ContextMenuStripEx) null;
            }
            ExplorerManager.explorerManager_NotifyIconOwner = (ExplorerManager) null;
            if (!ExplorerManager.fMergingAllWindow)
            {
              if (ExplorerManager.dicNotifyIcon.Count > 0)
              {
                using (IEnumerator<ExplorerManager> enumerator = ExplorerManager.dicNotifyIcon.Keys.GetEnumerator())
                {
                  if (enumerator.MoveNext())
                  {
                    ExplorerManager current = enumerator.Current;
                    current.Invoke((Delegate) new ParameterizedThreadStart(ExplorerManager.CreateNotifyIcon), new object[1]
                    {
                      (object) current
                    });
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        DebugUtil.AppendToCriticalExceptionLog(ex, "emclosing1");
      }
      this.SaveCurrentBandLayout();
      this.SaveExtraBarRatios();
      try
      {
        this.Toolbars.Show(Toolbar.ManagementBar, this.Toolbars.ManagementBarIsRequired);
      }
      catch (Exception ex)
      {
        DebugUtil.AppendToCriticalExceptionLog(ex, "emclosing2");
      }
      try
      {
        if (Config.Bool(Scts.SaveWindowTransparency))
        {
          using (RegistryKey registryKey = RegistryUtil.OpenUserRoot(true))
          {
            if (registryKey != null)
            {
              byte pbAlpha;
              Config.WindowAlpha = 524288 == (int) PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE), 524288) ? (!PInvoke.GetLayeredWindowAttributes(this.hwndExplorer, out int _, out pbAlpha, out int _) ? byte.MaxValue : pbAlpha) : byte.MaxValue;
              registryKey.SetValue("WindowAlpha", (object) Config.WindowAlpha);
            }
          }
        }
      }
      catch (Exception ex)
      {
        DebugUtil.AppendToCriticalExceptionLog(ex, "3");
      }
      try
      {
        if (!this.Toolbars.IsTabBarAvailable)
        {
          RecentTabManager.AddOnClose((IEnumerable<LogData>) new LogData(this.currentIDL).AsArray<LogData>(), true, this.Handle);
          RestorationData.Save((IList<LogData>) new LogData(this.currentIDL).AsArray<LogData>(), (IList<bool>) false.AsArray<bool>(), (IEnumerable<GroupItemInfo>) null);
        }
      }
      catch (Exception ex)
      {
        DebugUtil.AppendToCriticalExceptionLog(ex, "4");
      }
      try
      {
        if (UserEvents.WindowClosing == 0)
          return;
        UserEvents.Fire(UserEvents.WindowClosing, (ICommandInvokerWindow) this, (object) null, EventSource.WindowClosing);
      }
      catch (Exception ex)
      {
      }
    }

    private void OnInstanceUnregistered(bool fInteractive)
    {
      try
      {
        this.fNowClosing = true;
        if (fInteractive)
          this.fInteractivePluginsUnloadingPending = true;
        if (!this.Toolbars.AllToolbarReleased)
          return;
        if (this.pluginManager != null)
        {
          if (this.fInteractivePluginsUnloadingPending)
            this.pluginManager.Close(true);
          this.pluginManager.Close(false);
          this.pluginManager = (PluginManager) null;
        }
        if (ExplorerManager.optionsDialog != null && this.fOptionDialogCreated)
        {
          this.fOptionDialogCreated = false;
          try
          {
            ExplorerManager.optionsDialog.Invoke((Delegate) new MethodInvoker(this.odCallback_Close));
          }
          catch (Exception ex)
          {
          }
        }
        this.Dispose();
      }
      catch (Exception ex)
      {
        DebugUtil.AppendToCriticalExceptionLog(ex);
      }
    }

    public void Dispose()
    {
      this.fNowClosing = true;
      if (this.disposed)
        return;
      this.disposed = true;
      if (!this.fUnregistered)
      {
        InstanceManager.UnregisterExplorerManager(this);
        this.fUnregistered = true;
      }
      try
      {
        Application.EnterThreadModal -= new EventHandler(this.Application_EnterThreadModal);
        Application.LeaveThreadModal -= new EventHandler(this.Application_LeaveThreadModal);
        if (this.components != null)
        {
          this.components.Dispose();
          this.components = (IContainer) null;
        }
        if (this.syncControl != null)
        {
          this.syncControl.Dispose();
          this.syncControl = (SyncControl) null;
        }
        if (this.View != null)
        {
          this.View.Dispose();
          this.View = (DefaultView) null;
        }
        if (this.rebar != null)
        {
          this.rebar.Uninitialize();
          this.rebar = (Rebar) null;
        }
        if (this.fileRenameDialog != null)
        {
          if (this.fileRenameDialog.IsHandleCreated && this.fileRenameDialog.Visible)
            this.fileRenameDialog.Close();
          this.fileRenameDialog.Dispose();
          this.fileRenameDialog = (FileRenameDialog) null;
        }
        if (this.navPane != null)
        {
          this.navPane.Cleanup();
          this.navPane = (NavigationPane) null;
        }
        if (this.tabSwitcher != null)
        {
          this.tabSwitcher.Dispose();
          this.tabSwitcher = (TabSwitchForm) null;
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        if (this.explorerWindowSubclass != null)
        {
          this.explorerWindowSubclass.ReleaseHandle();
          this.explorerWindowSubclass = (WindowSubclass) null;
        }
        if (this.shellTabWindowSubclass != null)
        {
          this.shellTabWindowSubclass.ReleaseHandle();
          this.shellTabWindowSubclass = (WindowSubclass) null;
        }
        if (this.rebarWindowSubclass != null)
        {
          this.rebarWindowSubclass.ReleaseHandle();
          this.rebarWindowSubclass = (WindowSubclass) null;
          if (this.toolbarImageHelper != null)
          {
            this.toolbarImageHelper.Dispose();
            this.toolbarImageHelper = (ToolbarImageHelper) null;
          }
        }
        if (this.addressBandRootWindowSubclass != null)
        {
          this.addressBandRootWindowSubclass.ReleaseHandle();
          this.addressBandRootWindowSubclass = (WindowSubclass) null;
        }
        if (this.rebarParentWindowSubclass != null)
        {
          this.rebarParentWindowSubclass.ReleaseHandle();
          this.rebarParentWindowSubclass = (WindowSubclass) null;
        }
        if (this.hHook_Key != IntPtr.Zero)
        {
          PInvoke.UnhookWindowsHookEx(this.hHook_Key);
          this.hHook_Key = IntPtr.Zero;
        }
        if (this.hHook_Mouse != IntPtr.Zero)
        {
          PInvoke.UnhookWindowsHookEx(this.hHook_Mouse);
          this.hHook_Mouse = IntPtr.Zero;
        }
        if (this.hHook_GetMsg != IntPtr.Zero)
        {
          PInvoke.UnhookWindowsHookEx(this.hHook_GetMsg);
          this.hHook_GetMsg = IntPtr.Zero;
        }
        if (this.hHook_CallWndProc != IntPtr.Zero)
        {
          PInvoke.UnhookWindowsHookEx(this.hHook_CallWndProc);
          this.hHook_CallWndProc = IntPtr.Zero;
        }
        if (this.explorer != null)
        {
          Marshal.FinalReleaseComObject((object) this.explorer);
          this.explorer = (SHDocVw.WebBrowser) null;
        }
        if (this.shellBrowser != null)
        {
          Marshal.ReleaseComObject((object) this.shellBrowser);
          this.shellBrowser = (IShellBrowser) null;
        }
        if (this.shellFolderView != null)
        {
          Marshal.FinalReleaseComObject((object) this.shellFolderView);
          this.shellFolderView = (ShellFolderView) null;
        }
      }
      catch (Exception ex)
      {
      }
      this.bandSite = (IBandSite) null;
      ExplorerManager.ThreadInstance = (ExplorerManager) null;
      this.DestroyHandle();
    }

    public bool IsClosing { this.fNowClosing;

    public bool IsDisposed { this.disposed;

    public bool IsAlive { !this.IsClosing && !this.IsDisposed && this.Handle != IntPtr.Zero;

    private void InstallHooks()
    {
      this.explorerWindowSubclass = new WindowSubclass(this.hwndExplorer, new WindowSubclass.SubclassingProcedure(this.explorerSubclassProc));
      this.hwndShellTab = WindowUtil.GetShellTabWindowClass(this.shellBrowser);
      if (this.hwndShellTab != IntPtr.Zero)
        this.shellTabWindowSubclass = new WindowSubclass(this.hwndShellTab, new WindowSubclass.SubclassingProcedure(this.shellTabWindowSubclassProc));
      this.hwndAddressBandRoot = WindowUtil.GetAddressBandRoot(this.hwndExplorer);
      if (this.hwndAddressBandRoot != IntPtr.Zero)
        this.addressBandRootWindowSubclass = new WindowSubclass(this.hwndAddressBandRoot, new WindowSubclass.SubclassingProcedure(this.addressBandRootSubclassProc));
      this.hwndBreadcrumbParent = WindowUtil.GetBreadcrumbParent(this.hwndExplorer);
      this.hwndBreadcrumbToolbarWindow = PInvoke.FindWindowEx(this.hwndBreadcrumbParent, IntPtr.Zero, (string) null, (string) null);
      this.hookProc_Key = new HookProc(this.KeyboardProc);
      this.hHook_Key = PInvoke.SetWindowsHookEx(2, this.hookProc_Key, IntPtr.Zero, this.ThreadID);
      this.hookProc_Mouse = new HookProc(this.MouseProc);
      this.hHook_Mouse = PInvoke.SetWindowsHookEx(7, this.hookProc_Mouse, IntPtr.Zero, this.ThreadID);
      this.hookProc_GetMsg = new HookProc(this.GetMsgProc);
      this.hHook_GetMsg = PInvoke.SetWindowsHookEx(3, this.hookProc_GetMsg, IntPtr.Zero, this.ThreadID);
      this.hookProc_CallWndProc = new HookProc(this.CallWndProc);
      this.hHook_CallWndProc = PInvoke.SetWindowsHookEx(4, this.hookProc_CallWndProc, IntPtr.Zero, this.ThreadID);
    }

    private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (!this.InModal)
      {
        if (nCode >= 0)
        {
          try
          {
            int num = WM.KeyMessageKeyPressed(lParam) ? 1 : 0;
            bool fRepeat = WM.KeyMessageIsRepeat(lParam);
            if (num != 0)
            {
              if (this.HandleKEYDOWN((int) (long) wParam, fRepeat))
                return new IntPtr(1);
            }
            else
            {
              if (MessageListenerWindow.IsKeyFinderVisible)
                PInvoke.PostMessage(MessageListenerWindow.WindowHandle, 32785, wParam, (IntPtr) 1);
              if (Config.Get(Scts.SubDirTip) == 2 && this.View.IsSubFolderTipAvailable && !this.View.IsSubFolderMenuVisible)
                this.View.HideSubDirTip(false);
              switch ((Keys) (long) wParam)
              {
                case Keys.Tab:
                  if (Config.Bool(Scts.ShowTabSwitcher) && this.tabSwitcher != null && this.tabSwitcher.Visible && this.tabSwitcher.TabBar != null)
                  {
                    this.tabSwitcher.TabBar.TabManager.SetPseudoHotIndex(this.tabSwitcher.SelectedIndex);
                    break;
                  }
                  break;
                case Keys.ControlKey:
                  if (Config.Bool(Scts.ShowTabSwitcher))
                  {
                    this.HideTabSwitcher(true);
                    break;
                  }
                  break;
              }
              if (this.KeyUp != null)
                this.KeyUp((object) this, new KeyHookEventArgs((Keys) (long) wParam, fRepeat));
            }
          }
          catch (Exception ex)
          {
            DebugUtil.AppendToCriticalExceptionLog(ex);
          }
        }
      }
      return PInvoke.CallNextHookEx(this.hHook_Key, nCode, wParam, lParam);
    }

    private IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0 && !this.InModal)
      {
        if (this.MouseProcGlobal(wParam, lParam))
          return (IntPtr) 1;
        if (this.MouseHookProc != null)
        {
          foreach (HookProc invocation in this.MouseHookProc.GetInvocationList())
          {
            if (IntPtr.Zero != invocation(nCode, wParam, lParam))
              return (IntPtr) 1;
          }
        }
        switch ((int) wParam)
        {
          case 517:
            if (this.fSuppressRightButtonUp)
            {
              this.fSuppressRightButtonUp = false;
              return new IntPtr(1);
            }
            if (QEnvironment.IsWindows7 && !MenuUtility.InMenuLoop && !MenuUtility.InMenuMode && WindowUtil.MouseCursorIsInFolderBand(this.hwndShellTab, this.hwndListView, this.Toolbars))
            {
              this.OnFolderBandRightClick();
              break;
            }
            break;
          case 522:
            if (this.HandleMOUSEWHEEL(lParam))
              return new IntPtr(1);
            break;
          case 523:
            if (Config.Bool(Scts.MouseXButtonsNavigation))
              return new IntPtr(1);
            break;
          case 524:
            if (Config.Bool(Scts.MouseXButtonsNavigation))
            {
              this.HandleXBUTTON();
              return new IntPtr(1);
            }
            break;
        }
      }
      return PInvoke.CallNextHookEx(this.hHook_Mouse, nCode, wParam, lParam);
    }

    private bool MouseProcGlobal(IntPtr wParam, IntPtr lParam) { (int) wParam == 522 && this.HandleWheelWithRight(lParam);

    private unsafe IntPtr GetMsgProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0 && (int) wParam == 1)
      {
        QTTabBarLib.Interop.MSG* msgPtr = (QTTabBarLib.Interop.MSG*) (void*) lParam;
        if (this.DispatchKeyToExtraViews(msgPtr))
        {
          PInvoke.EatHookedMessage(msgPtr);
        }
        else
        {
          if (256 <= msgPtr->message && msgPtr->message <= 265 && !MenuUtility.InMenuLoop && this.fileRenameDialog != null && this.fileRenameDialog.IsHandleCreated && this.fileRenameDialog.Handle == PInvoke.GetForegroundWindow())
          {
            Message message = msgPtr->ToMessage();
            if (this.fileRenameDialog.PreProcessControlMessage(ref message) == PreProcessControlState.MessageProcessed)
            {
              PInvoke.EatHookedMessage(msgPtr);
              return PInvoke.CallNextHookEx(this.hHook_GetMsg, nCode, wParam, lParam);
            }
          }
          switch (msgPtr->message)
          {
            case 16:
              if (this.CloseMessagePosted != null)
              {
                bool handled = false;
                try
                {
                  this.CloseMessagePosted(ref *msgPtr, ref handled);
                }
                catch (Exception ex)
                {
                }
                if (handled)
                {
                  PInvoke.EatHookedMessage(msgPtr);
                  break;
                }
                break;
              }
              break;
            case 273:
              if (((int) (long) msgPtr->wParam & (int) ushort.MaxValue) == 41484)
              {
                this.SaveCurrentBandLayout();
                break;
              }
              break;
            case 274:
              if (((int) (long) msgPtr->wParam & 65520) == 61536)
              {
                int interactiveValue = UserEvents.GetInteractiveValue(UserEvents.JumpListCommand);
                if (UserEvents.IsIndexPersistentID(interactiveValue))
                {
                  UserEvents.Fire(interactiveValue, (ICommandInvokerWindow) this, (object) this.hwndExplorer, EventSource.JumpListCommand);
                  PInvoke.EatHookedMessage(msgPtr);
                  break;
                }
                switch (interactiveValue)
                {
                  case 0:
                    this.InvokeCommand(new CommandInfo(BarCommand.CloseTab, BarCommandArg.Current));
                    break;
                  case 1:
                    this.InvokeCommand(BarCommand.CloseWindow);
                    break;
                  case 2:
                    this.InvokeCommand(BarCommand.MinimizeToTasktray);
                    break;
                  case 3:
                    this.InvokeCommand(new CommandInfo(BarCommand.CloseTab, BarCommandArg.AllButCurrent));
                    break;
                }
                PInvoke.EatHookedMessage(msgPtr);
                break;
              }
              break;
            case 512:
            case 513:
            case 514:
            case 515:
            case 516:
            case 517:
            case 519:
            case 520:
            case 522:
            case 523:
            case 524:
            case 673:
            case 675:
              this.DispatchMouseMessage(msgPtr);
              break;
          }
        }
      }
      return PInvoke.CallNextHookEx(this.hHook_GetMsg, nCode, wParam, lParam);
    }

    private unsafe IntPtr CallWndProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0)
      {
        switch (((CWPSTRUCT*) (void*) lParam)->message)
        {
          case 529:
            MenuUtility.InMenuLoop = true;
            break;
          case 530:
            MenuUtility.InMenuLoop = false;
            break;
          case 4363:
            if (this.fTreeViewColorPendingOnStartUp)
            {
              this.TrySetTreeColors(Config.Bool(Scts.FolderTreeForeColorEnabled), Config.Bool(Scts.FolderTreeBackColorEnabled));
              break;
            }
            break;
        }
      }
      return PInvoke.CallNextHookEx(this.hHook_CallWndProc, nCode, wParam, lParam);
    }

    private bool HandleKEYDOWN(int rawKey, bool fRepeat)
    {
      Keys modifierKeys = Control.ModifierKeys;
      int num = (int) ((Keys) rawKey | modifierKeys);
      if (!MenuUtility.InMenuLoop && PreviewTip.IsFullScreen && PreviewTip.IsVisible)
      {
        switch (PreviewTip.HandleFullScreenPreviewKeys(num, fRepeat))
        {
          case KeyProcResult.Handled:
            return true;
          case KeyProcResult.Unhandle:
            return false;
        }
      }
      if (this.ModelessWindowProcessKey() || this.IMEWindowProcessKey())
        return false;
      if (fRepeat)
        this.StopSelectionChangeTimer();
      switch ((Keys) rawKey)
      {
        case Keys.Tab:
          if (Config.Bool(Scts.ShowTabSwitcher) && modifierKeys.HasFlag((Enum) Keys.Control))
          {
            this.fSuppressFocusing41008 = true;
            this.HideAllSubFolderTips();
            return this.ShowTabSwitcher(modifierKeys.HasFlag((Enum) Keys.Shift), fRepeat);
          }
          break;
        case Keys.Return:
          if (!fRepeat)
          {
            if (WindowUtil.AddressBarHasFocus())
            {
              this.NotifyBrowserRefresh(false);
            }
            else
            {
              NavigationPane navigationPane;
              if (!MenuUtility.InMenuLoop && this.IsNavigationWindowHasFocus(out navigationPane))
              {
                if (this.OnFolderTreeEnterKeyDown(navigationPane))
                  return true;
              }
              else if (modifierKeys.HasFlag((Enum) Keys.Alt))
              {
                TargetView focusedTargetView = this.GetFocusedTargetView(true);
                switch (focusedTargetView)
                {
                  case TargetView.None:
                    break;
                  case TargetView.Default:
                    if (this.OnViewEnterKeyDown())
                      return true;
                    break;
                  default:
                    if (this.Toolbars.IsExtraViewVisible(focusedTargetView) && this.Toolbars.GetExtraViewBar(focusedTargetView).OnViewEnterKeyDown())
                      return true;
                    break;
                }
              }
            }
          }
          return false;
        case Keys.ShiftKey:
          if (!fRepeat)
          {
            if (MessageListenerWindow.IsKeyFinderVisible)
              PInvoke.PostMessage(MessageListenerWindow.WindowHandle, 32785, (IntPtr) (int) modifierKeys, IntPtr.Zero);
            if (PreviewTip.IsEnabled && !Config.Bool(Scts.PreviewWithShiftKeyDown))
              this.HideAllPreviewTips();
            if (Config.Get(Scts.SubDirTip) == 1)
              this.HideAllSubFolderTips();
          }
          return false;
        case Keys.ControlKey:
        case Keys.Menu:
          if (!fRepeat && MessageListenerWindow.IsKeyFinderVisible)
          {
            PInvoke.PostMessage(MessageListenerWindow.WindowHandle, 32785, (IntPtr) (int) modifierKeys, IntPtr.Zero);
            break;
          }
          break;
        case Keys.Escape:
          if (!fRepeat)
          {
            if (MessageListenerWindow.IsKeyFinderVisible)
              PInvoke.PostMessage(MessageListenerWindow.WindowHandle, 32786, IntPtr.Zero, IntPtr.Zero);
            MessageForm.CloseOne();
          }
          return false;
        case Keys.Delete:
          if (!fRepeat && !MenuUtility.InMenuMode)
          {
            CustomViewBase focusedView = this.GetFocusedView();
            if (focusedView != null)
            {
              if (PreviewTip.IsEnabled)
                focusedView.HidePreviewTip();
              if (Config.Get(Scts.SubDirTip) > 0 && focusedView.IsSubFolderTipVisible && !focusedView.IsViewSubFolderMenuVisible)
                focusedView.HideSubDirTip();
              if (focusedView.VistaLayout)
                focusedView.MoveFocusToNextItem();
              if (!ShellMethods.IsTrashBin((byte[]) focusedView.ItemIDList))
                return FileOperationSnatcher.OnKeyDown((Keys) num, focusedView, OperationLogger.LogSite.Folder);
            }
          }
          return false;
        case Keys.Apps:
          if (MenuUtility.InMenuMode)
            return false;
          break;
        default:
          if (!fRepeat && MessageListenerWindow.IsKeyFinderVisible)
          {
            PInvoke.PostMessage(MessageListenerWindow.WindowHandle, 32785, (IntPtr) num, IntPtr.Zero);
            break;
          }
          break;
      }
      switch ((Keys) num)
      {
        case Keys.Back:
          if (!(this.hwndListView == PInvoke.GetFocus()))
            return false;
          if (!fRepeat)
          {
            if (Config.Bool(Scts.ViewBSKeyGoUp))
              this.InvokeCommand(BarCommand.GoUpOneLevel);
            else
              this.InvokeCommand(BarCommand.GoBack);
          }
          return true;
        case Keys.Tab:
          if (this.OnTabKeyWhileRenaming(false) || this.OnTabKeyOnEdit() || Config.Bool(Scts.FocusNextViewByTabIfFocused) && this.FocusNextView())
            return true;
          break;
        case Keys.Left:
        case Keys.Up:
        case Keys.Right:
        case Keys.Down:
          if (!fRepeat)
          {
            this.StopSelectionChangeTimer();
            if (!PreviewTip.IsFullScreen)
            {
              this.HideAllPreviewTips(true);
              break;
            }
            break;
          }
          break;
        case Keys.F2:
          if (Config.Bool(Scts.ViewF2ChangeTextSelection) && !ExplorerManager.HandleF2(this.View.ListViewHandle, this.View.VistaLayout) && (!this.Toolbars.IsSecondViewBarTabAvailable || !ExplorerManager.HandleF2(this.Toolbars.SecondViewBar.ActiveTab.View.ListViewHandle, this.Toolbars.SecondViewBar.ActiveTab.View.VistaLayout)) && this.Toolbars.IsThirdViewBarTabAvailable)
            ExplorerManager.HandleF2(this.Toolbars.ThirdViewBar.ActiveTab.View.ListViewHandle, this.Toolbars.ThirdViewBar.ActiveTab.View.VistaLayout);
          return false;
        case Keys.F5:
          if (!KeyboardShortcutManager.IsAssigned(num) && this.DispatchCommandKeyToExtraView(Keys.F5))
            return true;
          break;
        case Keys.Tab | Keys.Shift:
          if (this.OnTabKeyWhileRenaming(true))
            return true;
          break;
        case Keys.C | Keys.Control:
        case Keys.X | Keys.Control:
          if (!fRepeat)
          {
            CustomViewBase focusedView = this.GetFocusedView();
            if (focusedView != null)
              FileOperationSnatcher.OnKeyDown((Keys) num, focusedView);
          }
          return false;
        case Keys.V | Keys.Control:
          if (!fRepeat)
          {
            CustomViewBase focusedView = this.GetFocusedView();
            if (focusedView != null)
              return FileOperationSnatcher.OnKeyDown((Keys) num, focusedView, OperationLogger.LogSite.Folder);
          }
          return false;
        case Keys.F5 | Keys.Control:
          if (!KeyboardShortcutManager.IsAssigned(num))
          {
            this.DispatchCommandKeyToExtraView(Keys.F5);
            break;
          }
          break;
        case Keys.Left | Keys.Alt:
        case Keys.Right | Keys.Alt:
          return this.HandleGlobalKeys(num, fRepeat);
      }
      if (this.KeyDown != null)
      {
        KeyHookEventArgs e = new KeyHookEventArgs((Keys) num, fRepeat);
        this.KeyDown((object) this, e);
        if (e.Handled)
          return true;
      }
      if (rawKey == num)
      {
        if (!this.AnyViewHasFocus || MenuUtility.Visible || this.AnyViewRenaming)
          return false;
        if (!Config.Bool(Scts.AllowShortcutsWithoutModKeys))
          return PreviewTip.IsVisible && PreviewTip.HandleBuiltinKeyDown(num, fRepeat);
      }
      KeyboardShortcutItem ksi;
      if (KeyboardShortcutManager.TryGetItem(num, out ksi))
      {
        if (KeyboardShortcutManager.EditControlShouldProcessKey(ksi, true))
          return false;
        switch (ksi.Type)
        {
          case KeyboardShortcutType.Default:
            return this.PerformDefaultKey(num, ksi, fRepeat);
          case KeyboardShortcutType.Plugin:
            this.PerformPluginKey(ksi, fRepeat);
            break;
          case KeyboardShortcutType.Group:
            if (!fRepeat)
            {
              CommandInfo info = new CommandInfo(BarCommand.Group, ksi.GroupItemInfo.Name, BarCommandOptions.IgnoreModKeys);
              info.TargetView = this.GetFocusedTargetView();
              this.InvokeCommand(info);
              break;
            }
            break;
          case KeyboardShortcutType.AppLauncher:
            if (!fRepeat)
            {
              CommandInfo info = new CommandInfo(BarCommand.LaunchApplicationByMia, ksi.MenuItemArguments);
              info.TargetView = this.GetFocusedTargetView();
              this.InvokeCommand(info);
              break;
            }
            break;
          case KeyboardShortcutType.CommandButton:
            if (!this.PerformNoOpDropDown(ksi))
            {
              this.PerformCommandButtonKey(ksi, fRepeat);
              break;
            }
            break;
        }
        return true;
      }
      if (PreviewTip.IsVisible && PreviewTip.HandleBuiltinKeyDown(num, fRepeat))
        return true;
      switch (num)
      {
        case 131142:
          return (Config.ShortcutKeys[37] & 2097152) == 0;
        case 131159:
          return (Config.ShortcutKeys[8] & 2097152) == 0;
        case 196686:
          TargetView focusedTargetView1 = this.GetFocusedTargetView();
          switch (focusedTargetView1)
          {
            case TargetView.Default:
              if (!this.View.VistaLayout || this.View.ViewMode != FOLDERVIEWMODE.FVM_DETAILS)
                break;
              goto case TargetView.Second;
            case TargetView.Second:
            case TargetView.Third:
              if (!fRepeat && !MenuUtility.InMenuLoop && !MenuUtility.InMenuMode)
                this.InvokeCommand(BarCommand.CreateNewFolder, focusedTargetView1);
              return true;
          }
          break;
        case 262182:
          TargetView targetView;
          this.GetFocusedView(out targetView);
          switch (targetView)
          {
            case TargetView.None:
            case TargetView.Default:
              if (this.Toolbars.IsTabBarAvailable)
              {
                this.Toolbars.TabBar.GoUpOneLevel();
                return true;
              }
              break;
            case TargetView.Second:
              this.Toolbars.SecondViewBar.GoUpOneLevel();
              return true;
            case TargetView.Third:
              this.Toolbars.ThirdViewBar.GoUpOneLevel();
              return true;
          }
          break;
        case 262212:
          if (WindowUtil.AddressBarHasFocus())
          {
            if (this.Toolbars.IsPriorityExtraViewBarVisible && this.Toolbars.PriorityExtraViewBar.SetFocusToBreadcrums() || this.Toolbars.IsNonPriorityExtraViewBarVisible && this.Toolbars.NonPriorityExtraViewBar.SetFocusToBreadcrums())
              return true;
            break;
          }
          if (this.Toolbars.IsPriorityExtraViewBarVisible && this.Toolbars.PriorityExtraViewBar.BreadcrumsHasFocus && this.Toolbars.IsNonPriorityExtraViewBarVisible && this.Toolbars.NonPriorityExtraViewBar.SetFocusToBreadcrums())
            return true;
          break;
      }
      return false;
    }

    private bool PerformNoOpDropDown(KeyboardShortcutItem ksi)
    {
      CommandButtonInfo commandButtonInfo = ksi.CommandButtonInfo;
      if ((commandButtonInfo != null ? (commandButtonInfo.ScriptingCommand == ScriptingCommand.NoOp ? 1 : 0) : 0) != 0)
      {
        foreach (ICommandControlOwner commandControlOwner in this.Toolbars.AvailableCommandControlOwners)
        {
          if (commandControlOwner.CommandControl.GetItemFromID(ksi.CommandButtonInfo.ID) is ToolStripDropDownButtonSB itemFromId)
          {
            itemFromId.ShowDropDown();
            return true;
          }
        }
      }
      return false;
    }

    private bool HandleGlobalKeys(int key, bool fRepeat)
    {
      switch ((Keys) key)
      {
        case Keys.Left | Keys.Alt:
        case Keys.Right | Keys.Alt:
          TargetView targetView;
          this.GetFocusedView(out targetView);
          if (targetView == TargetView.None || targetView == TargetView.Default)
          {
            if (this.KeyDown != null)
            {
              KeyHookEventArgs e = new KeyHookEventArgs((Keys) key, fRepeat);
              this.KeyDown((object) this, e);
              return e.Handled;
            }
            if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
            {
              this.ViewSyncManager.OnBrowsingBack((CustomViewBase) this.View, key == 262181);
              break;
            }
            break;
          }
          bool fBack = key == 262181;
          if (targetView == TargetView.Second)
          {
            if (this.Toolbars.SecondViewBar.ActiveTab.NavigationLog.CanTravel(fBack))
              this.Toolbars.SecondViewBar.ActiveTab.Go(fBack);
          }
          else if (targetView == TargetView.Third && this.Toolbars.ThirdViewBar.ActiveTab.NavigationLog.CanTravel(fBack))
            this.Toolbars.ThirdViewBar.ActiveTab.Go(fBack);
          return true;
      }
      return false;
    }

    private void HideAllPreviewTips(bool fForce = false)
    {
      this.View.HidePreviewTip(fForce);
      if (this.Toolbars.IsSecondViewBarAvailable && this.Toolbars.SecondViewBar.IsActiveTabAvailable)
        this.Toolbars.SecondViewBar.ActiveTab.View.HidePreviewTip(fForce);
      if (!this.Toolbars.IsThirdViewBarAvailable || !this.Toolbars.ThirdViewBar.IsActiveTabAvailable)
        return;
      this.Toolbars.ThirdViewBar.ActiveTab.View.HidePreviewTip(fForce);
    }

    private void HideAllSubFolderTips(bool fForce = true)
    {
      if (this.View.IsSubFolderTipVisible && !this.View.IsSubFolderMenuVisible)
        this.View.HideSubDirTip(fForce);
      if (this.Toolbars.IsSecondViewBarAvailable && this.Toolbars.SecondViewBar.IsActiveTabAvailable && !this.Toolbars.SecondViewBar.ActiveTab.View.IsSubFolderMenuVisible)
        this.Toolbars.SecondViewBar.ActiveTab.View.HideSubDirTip(fForce);
      if (!this.Toolbars.IsThirdViewBarAvailable || !this.Toolbars.ThirdViewBar.IsActiveTabAvailable || this.Toolbars.ThirdViewBar.ActiveTab.View.IsSubFolderMenuVisible)
        return;
      this.Toolbars.ThirdViewBar.ActiveTab.View.HideSubDirTip(fForce);
    }

    private void StopSelectionChangeTimer()
    {
      this.View.StopSelectionChangeTimer();
      if (this.Toolbars.IsSecondViewBarTabAvailable)
        this.Toolbars.SecondViewBar.ActiveTab.View.StopSelectionChangeTimer();
      if (!this.Toolbars.IsThirdViewBarTabAvailable)
        return;
      this.Toolbars.ThirdViewBar.ActiveTab.View.StopSelectionChangeTimer();
    }

    private bool PerformDefaultKey(int key, KeyboardShortcutItem ksi, bool fRepeat)
    {
      if (PreviewTip.IsVisible)
      {
        if (!this.AnyViewRenaming && PreviewTip.HandleUserKeyDown(key, fRepeat))
          return true;
      }
      else if (ksi.IsPreviewKey || MenuUtility.InMenuMode && this.PerformMenuKey(key, ksi, fRepeat))
        return false;
      if (this.PerformPaneDefaultKey(key, ksi, fRepeat))
        return true;
      bool fShouldReturnFalse;
      if (this.PerformFocusDependentDefaultKey(key, ksi, fRepeat, out fShouldReturnFalse))
        return !fShouldReturnFalse;
      switch (ksi.Index)
      {
        case 0:
          this.InvokeCommand(BarCommand.GoBack);
          break;
        case 1:
          this.InvokeCommand(BarCommand.GoForward);
          break;
        case 2:
          if (!fRepeat)
          {
            this.InvokeCommand(BarCommand.GoBackToFirst);
            break;
          }
          break;
        case 3:
          if (!fRepeat)
          {
            this.InvokeCommand(BarCommand.GoForwardToLast);
            break;
          }
          break;
        case 12:
          if (!fRepeat)
          {
            WindowUtil.CloseExplorer(this.hwndExplorer, WMCLOSEARG.Normal);
            break;
          }
          break;
        case 15:
          if (!fRepeat)
          {
            this.InvokeCommand(BarCommand.NewWindow);
            break;
          }
          break;
        case 18:
          if (!fRepeat)
          {
            this.BrowseFolder((byte[]) null);
            break;
          }
          break;
        case 20:
          if (!fRepeat)
          {
            this.OpenOptionDialog();
            break;
          }
          break;
        case 23:
          if (!fRepeat)
          {
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.PerformClickBuiltInItem(BuiltinCommandID.Group)));
            break;
          }
          break;
        case 24:
          if (!fRepeat)
          {
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.PerformClickBuiltInItem(BuiltinCommandID.RecentTab)));
            break;
          }
          break;
        case 25:
          if (!fRepeat)
          {
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.PerformClickBuiltInItem(BuiltinCommandID.AppLauncher)));
            break;
          }
          break;
        case 33:
          if (!fRepeat)
          {
            this.InvokeCommand(BarCommand.ToggleTopmost);
            break;
          }
          break;
        case 34:
          this.SetWindowTransparency(true);
          break;
        case 35:
          this.SetWindowTransparency(false);
          break;
        case 36:
          if (!fRepeat)
          {
            this.FocusListView();
            break;
          }
          break;
        case 37:
          if (!fRepeat)
          {
            PInvoke.SetFocus(WindowUtil.GetSearchBox(this.hwndExplorer));
            break;
          }
          break;
        case 38:
          if (!fRepeat)
          {
            this.FocusFolderTree();
            break;
          }
          break;
        case 39:
          if (!fRepeat && this.Toolbars.IsTabBarAvailable)
          {
            this.Toolbars.TabBar.FocusBar();
            break;
          }
          break;
        case 40:
          if (!fRepeat)
          {
            if (this.Toolbars.IsCommandBarVrtVisible)
            {
              this.ExpandToolbar(Toolbar.CommandBarVertical);
              PInvoke.SendMessage(this.Toolbars.CommandBarVrtHandle, 32771, IntPtr.Zero, IntPtr.Zero);
              break;
            }
            if (this.Toolbars.IsThirdViewBarVisible && Config.Get(Scts.ExtraViewVisibleMode3rd) == 2)
            {
              this.Toolbars.ThirdViewBar.FocusCommandBar();
              break;
            }
            break;
          }
          break;
        case 43:
          if (!fRepeat)
          {
            this.SendToTasktray();
            break;
          }
          break;
        case 74:
          if (!fRepeat)
          {
            this.Toolbars.Toggle(Toolbar.CommandBar1);
            break;
          }
          break;
        case 75:
          if (!fRepeat)
          {
            this.Toolbars.Toggle(Toolbar.CommandBar2);
            break;
          }
          break;
        case 76:
          if (!fRepeat)
          {
            if (Config.Bool(Scts.AutoHideCommandBarV) && this.Toolbars.IsCommandBarVrtVisible)
              this.VerticalBarShrunken = true;
            this.Toolbars.Toggle(Toolbar.CommandBarVertical);
            break;
          }
          break;
        case 80:
          if (!fRepeat)
          {
            KeyboardShortcutFinder.ShowFinder();
            break;
          }
          break;
        case 81:
          if (!fRepeat)
          {
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.PerformClickBuiltInItem(BuiltinCommandID.RecentFile)));
            break;
          }
          break;
        case 100:
          if (!fRepeat)
          {
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.FocusFilterBox()));
            break;
          }
          break;
        case 101:
          if (!fRepeat)
          {
            this.MaximizeHorizontally((Screen) null);
            break;
          }
          break;
        case 102:
          if (!fRepeat)
          {
            this.InvokeCommand(new CommandInfo(BarCommand.GlobalOption, 13));
            break;
          }
          break;
        case 107:
          if (!fRepeat)
          {
            this.ShrinkHorizontalToolbars(true);
            break;
          }
          break;
        case 108:
          if (!fRepeat)
          {
            this.ExpandHorizontalToolbars();
            break;
          }
          break;
        case 109:
          if (!fRepeat)
          {
            if (this.ToolbarsAreShrunken)
            {
              this.ExpandHorizontalToolbars();
              break;
            }
            this.ShrinkHorizontalToolbars(true);
            break;
          }
          break;
        case 110:
          if (!fRepeat)
          {
            if (Config.Bool(Scts.BottomTabBarEnabled))
            {
              this.Toolbars.Show(Toolbar.TabBar, true, true);
              break;
            }
            this.Toolbars.Toggle(Toolbar.TabBar);
            break;
          }
          break;
        case 111:
          if (!fRepeat)
          {
            if (Config.Bool(Scts.BottomTabBarEnabled))
            {
              this.Toolbars.Toggle(Toolbar.BottomTabBar);
              break;
            }
            this.Toolbars.Show(Toolbar.BottomTabBar, true);
            break;
          }
          break;
        case 114:
          if (!fRepeat && this.Toolbars.IsTabBarVisible)
          {
            this.Toolbars.Show(this.Toolbars.TabBar.IsBottomBar ? Toolbar.TabBar : Toolbar.BottomTabBar, true);
            break;
          }
          break;
        case 115:
          if (!fRepeat)
          {
            this.ShowCurrentHistoryMenu();
            break;
          }
          break;
        case 116:
          if (!fRepeat)
          {
            CommandInfo info = new CommandInfo(BarCommand.RestoreTabs);
            using (IEnumerator<TabBarBase> enumerator = this.Toolbars.TabBars.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                enumerator.Current.InvokeCommand(info);
                if (-1 != info.ReturnInt32)
                  info.Int = 1;
              }
              break;
            }
          }
          else
            break;
        case 117:
          if (!fRepeat)
          {
            RecentTabManager.Clear();
            break;
          }
          break;
        case 118:
          if (!fRepeat)
          {
            RecentFileManager.Clear();
            break;
          }
          break;
        case 138:
          if (!fRepeat)
          {
            this.Toolbars.Toggle(Toolbar.SecondViewBar);
            break;
          }
          break;
        case 139:
          if (!fRepeat)
          {
            this.Toolbars.Toggle(Toolbar.ThirdViewBar);
            break;
          }
          break;
        case 140:
          if (!fRepeat)
          {
            this.SetFocusTo(FocusableWindow.SecondView);
            break;
          }
          break;
        case 141:
          if (!fRepeat)
          {
            this.SetFocusTo(FocusableWindow.ThirdView);
            break;
          }
          break;
        case 142:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Second, FileOperationVerb.Copy);
            break;
          }
          break;
        case 143:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Third, FileOperationVerb.Copy);
            break;
          }
          break;
        case 144:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Default, FileOperationVerb.Copy);
            break;
          }
          break;
        case 145:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Second, FileOperationVerb.Move);
            break;
          }
          break;
        case 146:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Third, FileOperationVerb.Move);
            break;
          }
          break;
        case 147:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Default, FileOperationVerb.Move);
            break;
          }
          break;
        case 148:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Second, FileOperationVerb.Link);
            break;
          }
          break;
        case 149:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Third, FileOperationVerb.Link);
            break;
          }
          break;
        case 150:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Default, FileOperationVerb.Link);
            break;
          }
          break;
        case 151:
          if (!fRepeat)
          {
            if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Scroll))
            {
              this.ViewSyncManager.SynchronizationLevel &= ~ViewSynchronizationLevel.Scroll;
              break;
            }
            this.ViewSyncManager.SynchronizationLevel |= ViewSynchronizationLevel.Scroll;
            break;
          }
          break;
        case 152:
          if (!fRepeat)
          {
            if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
            {
              this.ViewSyncManager.SynchronizationLevel &= ~ViewSynchronizationLevel.Navigation;
              break;
            }
            this.ViewSyncManager.SynchronizationLevel |= ViewSynchronizationLevel.Navigation;
            break;
          }
          break;
        case 153:
          if (!fRepeat)
          {
            this.ViewSyncManager.SyncLocation(0);
            break;
          }
          break;
        case 154:
          if (!fRepeat)
          {
            this.ViewSyncManager.SyncLocation(2);
            break;
          }
          break;
        case 155:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Other, FileOperationVerb.Copy);
            break;
          }
          break;
        case 156:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Other, FileOperationVerb.Move);
            break;
          }
          break;
        case 157:
          if (!fRepeat)
          {
            this.CopySelectionFromFocusedViewTo(TargetView.Other, FileOperationVerb.Move);
            break;
          }
          break;
        case 158:
          this.FocusNextView();
          break;
        case 159:
          if (!fRepeat)
          {
            switch (this.GetFocusedTargetView(true))
            {
              case TargetView.Default:
                this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.FocusFilterBox()));
                break;
              case TargetView.Second:
                if (this.Toolbars.IsSecondViewBarTabAvailable)
                {
                  this.Toolbars.SecondViewBar.FocusFilterBox();
                  break;
                }
                break;
              case TargetView.Third:
                if (this.Toolbars.IsThirdViewBarTabAvailable)
                {
                  this.Toolbars.ThirdViewBar.FocusFilterBox();
                  break;
                }
                break;
            }
          }
          else
            break;
          break;
        case 164:
          if (!fRepeat)
          {
            this.ViewSyncManager.SyncLocation(1);
            break;
          }
          break;
      }
      return true;
    }

    private bool PerformMenuKey(int key, KeyboardShortcutItem ksi, bool fRepeat)
    {
      switch (ksi.Index)
      {
        case 27:
        case 28:
        case 29:
        case 30:
        case 175:
        case 176:
        case 177:
        case 181:
        case 182:
          return true;
        default:
          return false;
      }
    }

    private bool PerformPaneDefaultKey(int key, KeyboardShortcutItem ksi, bool fRepeat)
    {
      NavigationPane navigationPane;
      return this.IsNavigationWindowHasFocus(out navigationPane) && this.OnFolderTreeKeyShortcut(ksi.Index, fRepeat, navigationPane);
    }

    private bool PerformFocusDependentDefaultKey(
      int key,
      KeyboardShortcutItem ksi,
      bool fRepeat,
      out bool fShouldReturnFalse)
    {
      fShouldReturnFalse = false;
      TargetView targetView1;
      CustomViewBase focusedView = this.GetFocusedView(out targetView1);
      if (targetView1 == TargetView.None)
        return false;
      if (this.PerformDefaultKeyOnFocusedView(key, ksi, fRepeat, focusedView))
        return true;
      if (targetView1 != TargetView.Default || this.Toolbars.IsTabBarAvailable)
        return this.PerformDefaultKeyOnFocusedTabBar(key, ksi, fRepeat, targetView1, out fShouldReturnFalse);
      switch (ksi.Index)
      {
        case 27:
        case 181:
          if (!fRepeat)
          {
            if (!this.View.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(new CommandInfo(ksi.Index == 27 ? BarCommand.ClipboardSelectionPaths : BarCommand.ClipboardSelectionPathsUNC, TargetView.Default));
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 28:
          if (!fRepeat)
          {
            if (!this.View.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(new CommandInfo(BarCommand.ClipboardSelectionNames, TargetView.Default));
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 29:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.ClipboardCurrentPath);
          return true;
        case 30:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.ClipboardCurrentName);
          return true;
        case 31:
          if (!fRepeat)
          {
            if (!this.View.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(BarCommand.ShowHashWindowForSelection);
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 32:
          if (!fRepeat)
          {
            if (!this.View.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(BarCommand.ClipboardSelectionHashes);
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 41:
          if (!fRepeat && this.hwndListView == PInvoke.GetFocus() && Config.Get(Scts.SubDirTip) > 0)
            this.View.ShowAndClickSubDirTip();
          return true;
        case 42:
          if (!fRepeat && this.hwndListView == PInvoke.GetFocus())
            this.View.ShowPreviewTipForSelectedItem(true);
          return true;
        case 44:
          if (!fRepeat)
            this.DeleteSelection(false, false);
          return true;
        case 45:
          if (!fRepeat)
            this.DeleteSelection(false, true);
          return true;
        case 46:
          if (!fRepeat)
            this.InvokeMenuCommand(ExplorerMenuCommand.InvertSelection);
          return true;
        case 47:
          if (!fRepeat)
            this.DeleteSelection(true, false);
          return true;
        case 48:
          if (!fRepeat)
          {
            using (ShellItem shellItem = this.View.ShellItem)
              shellItem.SetToClipboard(false, this.hwndExplorer);
          }
          return true;
        case 49:
          if (!fRepeat)
          {
            using (ShellItem shellItem = this.View.ShellItem)
              shellItem.SetToClipboard(true, this.hwndExplorer);
          }
          return true;
        case 50:
          if (!fRepeat)
          {
            using (ShellItem shellItem = new ShellItem(this.currentIDL, false))
            {
              if (shellItem.Available && !shellItem.IsReadOnly && ShellMethods.ClipboardContainsFileDropList(false))
                this.InvokeMenuCommand(ExplorerMenuCommand.PasteShortcut);
              else
                SystemSounds.Beep.Play2();
            }
          }
          return true;
        case 51:
          if (!fRepeat && !MenuUtility.InMenuLoop && !MenuUtility.InMenuMode)
            this.InvokeCommand(BarCommand.CreateNewFolder);
          return true;
        case 52:
          if (!fRepeat && !MenuUtility.InMenuLoop && !MenuUtility.InMenuMode)
            this.InvokeCommand(BarCommand.CreateNewTxtFile);
          return true;
        case 53:
          if (!fRepeat)
          {
            if (this.View.SelectedItemCount > 0)
              this.InvokeMenuCommand(ExplorerMenuCommand.CreateShortcut);
            else
              SystemSounds.Beep.Play2();
          }
          return true;
        case 54:
          if (!fRepeat)
          {
            if (this.View.SelectedItemCount > 0)
              this.InvokeMenuCommand(ExplorerMenuCommand.CopyToFolder);
            else
              SystemSounds.Beep.Play2();
          }
          return true;
        case 55:
          if (!fRepeat)
          {
            if (this.View.SelectedItemCount > 0)
              this.InvokeMenuCommand(ExplorerMenuCommand.MoveToFolder);
            else
              SystemSounds.Beep.Play2();
          }
          return true;
        case 65:
          if (!fRepeat)
            this.CopyFrom(FileOperationVerb.Copy);
          return true;
        case 66:
          if (!fRepeat)
            this.CopyFrom(FileOperationVerb.Move);
          return true;
        case 67:
          if (!fRepeat)
            this.CopyFrom(FileOperationVerb.Link);
          return true;
        case 83:
          if (!fRepeat)
            this.InvokeCommand(new CommandInfo(BarCommand.CreateNewFolder, 1));
          return true;
        case 99:
          if (!fRepeat)
            ShellMethods.PromptRename(this.currentIDL, this.hwndExplorer, StringResources.Dialogs._Dialog[2]);
          return true;
        case 103:
          if (this.hwndListView == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Left);
          return true;
        case 104:
          if (this.hwndListView == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Up);
          return true;
        case 105:
          if (this.hwndListView == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Right);
          return true;
        case 106:
          if (this.hwndListView == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Down);
          return true;
        case 124:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.RenameDialogAll);
          return true;
        case 125:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.ExtraLargeIcon);
          return true;
        case 126:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.LargeIcon);
          return true;
        case (int) sbyte.MaxValue:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.Icon);
          return true;
        case 128:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.SmallIcon);
          return true;
        case 129:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.List);
          return true;
        case 130:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.Details);
          return true;
        case 131:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.Tiles);
          return true;
        case 132:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.Contents);
          return true;
        case 133:
          if (!fRepeat)
            this.View.Sort("System.ItemNameDisplay");
          return true;
        case 134:
          if (!fRepeat)
            this.View.Sort("System.Size");
          return true;
        case 135:
          if (!fRepeat)
            this.View.Sort("System.ItemTypeText");
          return true;
        case 136:
          if (!fRepeat)
            this.View.Sort("System.DateModified");
          return true;
        case 137:
          if (!fRepeat)
            this.View.PasteIntoSelection();
          return true;
        case 161:
        case 162:
        case 163:
          if (!fRepeat)
          {
            TargetView targetView2 = TargetView.Second;
            if (ksi.Index == 162)
              targetView2 = TargetView.Third;
            else if (ksi.Index == 163)
              targetView2 = this.Toolbars.GetTheOtherView(TargetView.Default);
            if (this.Toolbars.IsExtraViewVisible(targetView2))
            {
              CommandInfo info = new CommandInfo(BarCommand.NewTab);
              info.IDL = this.currentIDL;
              info.TargetView = targetView2;
              info.Options = BarCommandOptions.IgnoreModKeys;
              this.InvokeCommand(info);
            }
            else
            {
              this.PendingFoldersExtraView = (IList<ItemIDList>) this.View.ItemIDList.AsArray<ItemIDList>();
              this.Toolbars.ShowExtraView(targetView2, true);
            }
          }
          return true;
        case 182:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.ClipboardCurrentPathUNC);
          return true;
        default:
          return false;
      }
    }

    private bool PerformDefaultKeyOnFocusedView(
      int key,
      KeyboardShortcutItem ksi,
      bool fRepeat,
      CustomViewBase view)
    {
      if (WindowUtil.EditHasFocus())
        return false;
      switch (ksi.Index)
      {
        case 175:
        case 176:
          view?.SelectIncrementallyPrevNext(ksi.Index == 175);
          return true;
        case 177:
          view?.SelectIncrementallyAll();
          return true;
        case 178:
          if (view != null)
          {
            CommandInfo info = new CommandInfo(BarCommand.AnalyzeFolder);
            info.TargetView = view.TargetView;
            this.InvokeCommand(info);
          }
          return true;
        case 179:
          if (view != null)
          {
            CommandInfo info = new CommandInfo(BarCommand.EditAttributes);
            info.TargetView = view.TargetView;
            this.InvokeCommand(info);
          }
          return true;
        case 180:
          if (view != null)
          {
            CommandInfo info = new CommandInfo(BarCommand.CreateSymbolicLink);
            info.TargetView = view.TargetView;
            this.InvokeCommand(info);
          }
          return true;
        default:
          return false;
      }
    }

    private bool PerformDefaultKeyOnFocusedTabBar(
      int key,
      KeyboardShortcutItem ksi,
      bool fRepeat,
      TargetView targetView,
      out bool fShouldReturnFalse)
    {
      fShouldReturnFalse = false;
      TabBarBase targetTabBar = this.GetTargetTabBar(targetView);
      QTabItem activeTab = targetTabBar.TabManager.ActiveTab;
      if (activeTab == null)
        return true;
      switch (ksi.Index)
      {
        case 0:
          targetTabBar.InvokeCommand(BarCommand.GoBack);
          return true;
        case 1:
          targetTabBar.InvokeCommand(BarCommand.GoForward);
          return true;
        case 2:
          if (!fRepeat)
            targetTabBar.InvokeCommand(BarCommand.GoBackToFirst);
          return true;
        case 3:
          if (!fRepeat)
            targetTabBar.InvokeCommand(BarCommand.GoForwardToLast);
          return true;
        case 4:
          if (!fRepeat)
            targetTabBar.ActivateTab(65536);
          return true;
        case 5:
          if (!fRepeat)
            targetTabBar.ActivateTab(65537);
          return true;
        case 6:
          if (!fRepeat)
            targetTabBar.ActivateTab(65538);
          return true;
        case 7:
          if (!fRepeat)
            targetTabBar.ActivateTab(65539);
          return true;
        case 8:
          if (!fRepeat)
            targetTabBar.InvokeCommand(new CommandInfo(BarCommand.CloseTab, BarCommandArg.Current));
          return true;
        case 9:
          if (!fRepeat)
            targetTabBar.CloseAllButCurrentTab();
          return true;
        case 10:
          if (!fRepeat)
            targetTabBar.CloseLeftRight(true, -1);
          return true;
        case 11:
          if (!fRepeat)
            targetTabBar.CloseLeftRight(false, -1);
          return true;
        case 13:
          if (!fRepeat)
            targetTabBar.ReopenLastClosedTabSet();
          return true;
        case 14:
          if (!fRepeat)
            targetTabBar.TabManager.Clone(activeTab, fSelect: Config.Bool(Scts.ActivateNewTab));
          return true;
        case 16:
          if (!fRepeat)
            activeTab.Locked = !activeTab.Locked;
          return true;
        case 17:
          if (!fRepeat)
            targetTabBar.ToggleTabLock();
          return true;
        case 18:
          if (!fRepeat)
            targetTabBar.BrowseFolder((byte[]) null);
          return true;
        case 19:
          if (!fRepeat)
            targetTabBar.CreateGroup(activeTab);
          return true;
        case 21:
          if (!fRepeat)
            targetTabBar.ShowBarMenu();
          return true;
        case 22:
          if (!fRepeat)
            targetTabBar.ShowActiveTabMenu();
          return true;
        case 27:
        case 181:
          if (!fRepeat)
          {
            if (!activeTab.AssociatedView.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(new CommandInfo(ksi.Index == 27 ? BarCommand.ClipboardSelectionPaths : BarCommand.ClipboardSelectionPathsUNC, targetView));
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 28:
          if (!fRepeat)
          {
            if (!activeTab.AssociatedView.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(new CommandInfo(BarCommand.ClipboardSelectionNames, targetView));
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 29:
          if (!fRepeat)
            this.InvokeCommand(new CommandInfo(BarCommand.ClipboardCurrentPath, targetView));
          return true;
        case 30:
          if (!fRepeat)
            this.InvokeCommand(new CommandInfo(BarCommand.ClipboardCurrentName, targetView));
          return true;
        case 31:
          if (!fRepeat)
          {
            if (!activeTab.AssociatedView.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(new CommandInfo(BarCommand.ShowHashWindowForSelection, targetView));
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 32:
          if (!fRepeat)
          {
            if (!activeTab.AssociatedView.IsSubFolderMenuVisible && !this.CommandBarKeyTargetExist)
              this.InvokeCommand(new CommandInfo(BarCommand.ClipboardSelectionHashes, targetView));
            else
              fShouldReturnFalse = true;
          }
          return true;
        case 41:
          if (!fRepeat && activeTab.AssociatedView.ListViewHandle == PInvoke.GetFocus() && Config.Get(Scts.SubDirTip) > 0)
            activeTab.AssociatedView.ShowAndClickSubDirTip();
          return true;
        case 42:
          if (!fRepeat && activeTab.AssociatedView.ListViewHandle == PInvoke.GetFocus())
            activeTab.AssociatedView.ShowPreviewTipForSelectedItem(true);
          return true;
        case 44:
          if (!fRepeat)
            this.DeleteSelection(false, false, targetView);
          return true;
        case 45:
          if (!fRepeat)
            this.DeleteSelection(false, true, targetView);
          return true;
        case 46:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.InvertSelection);
          return true;
        case 47:
          if (!fRepeat)
            targetTabBar.DeleteTab(activeTab, false);
          return true;
        case 48:
          if (!fRepeat)
          {
            targetTabBar.ClipboardTab(activeTab, false);
            break;
          }
          break;
        case 49:
          if (!fRepeat)
          {
            targetTabBar.ClipboardTab(activeTab, true);
            break;
          }
          break;
        case 50:
          if (!fRepeat)
          {
            using (ShellItem shellItem = new ShellItem((byte[]) activeTab.AssociatedView.ItemIDList, false))
            {
              if (shellItem.Available && !shellItem.IsReadOnly && ShellMethods.ClipboardContainsFileDropList(false))
                activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.PasteShortcut);
              else
                SystemSounds.Beep.Play2();
            }
          }
          return true;
        case 51:
          if (!fRepeat && !MenuUtility.InMenuLoop && !MenuUtility.InMenuMode)
            this.InvokeCommand(BarCommand.CreateNewFolder, targetView);
          return true;
        case 52:
          if (!fRepeat && !MenuUtility.InMenuLoop && !MenuUtility.InMenuMode)
            this.InvokeCommand(BarCommand.CreateNewTxtFile, targetView);
          return true;
        case 53:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.CreateShortcut);
          return true;
        case 54:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.CopyToFolder);
          return true;
        case 55:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.MoveToFolder);
          return true;
        case 56:
          if (!fRepeat)
          {
            targetTabBar.CopyToTab(DragDropEffects.Copy);
            break;
          }
          break;
        case 57:
          if (!fRepeat)
          {
            targetTabBar.CopyToTab(DragDropEffects.Move);
            break;
          }
          break;
        case 58:
          if (!fRepeat)
          {
            targetTabBar.CopyToTab(DragDropEffects.Link);
            break;
          }
          break;
        case 59:
          if (!fRepeat)
          {
            targetTabBar.CopyToNextTab(false, DragDropEffects.Copy);
            break;
          }
          break;
        case 60:
          if (!fRepeat)
          {
            targetTabBar.CopyToNextTab(false, DragDropEffects.Move);
            break;
          }
          break;
        case 61:
          if (!fRepeat)
          {
            targetTabBar.CopyToNextTab(false, DragDropEffects.Link);
            break;
          }
          break;
        case 62:
          if (!fRepeat)
          {
            targetTabBar.CopyToNextTab(true, DragDropEffects.Copy);
            break;
          }
          break;
        case 63:
          if (!fRepeat)
          {
            targetTabBar.CopyToNextTab(true, DragDropEffects.Move);
            break;
          }
          break;
        case 64:
          if (!fRepeat)
          {
            targetTabBar.CopyToNextTab(true, DragDropEffects.Link);
            break;
          }
          break;
        case 65:
          if (!fRepeat)
            this.CopyFrom(FileOperationVerb.Copy, targetView: targetView);
          return true;
        case 66:
          if (!fRepeat)
            this.CopyFrom(FileOperationVerb.Move, targetView: targetView);
          return true;
        case 67:
          if (!fRepeat)
            this.CopyFrom(FileOperationVerb.Link, targetView: targetView);
          return true;
        case 68:
          if (!fRepeat)
          {
            targetTabBar.CopyFrom(Direction.Left, FileOperationVerb.Copy);
            break;
          }
          break;
        case 69:
          if (!fRepeat)
          {
            targetTabBar.CopyFrom(Direction.Left, FileOperationVerb.Move);
            break;
          }
          break;
        case 70:
          if (!fRepeat)
          {
            targetTabBar.CopyFrom(Direction.Left, FileOperationVerb.Link);
            break;
          }
          break;
        case 71:
          if (!fRepeat)
          {
            targetTabBar.CopyFrom(Direction.Right, FileOperationVerb.Copy);
            break;
          }
          break;
        case 72:
          if (!fRepeat)
          {
            targetTabBar.CopyFrom(Direction.Right, FileOperationVerb.Move);
            break;
          }
          break;
        case 73:
          if (!fRepeat)
          {
            targetTabBar.CopyFrom(Direction.Right, FileOperationVerb.Link);
            break;
          }
          break;
        case 78:
          if (!fRepeat)
          {
            this.MergeAll();
            break;
          }
          break;
        case 79:
          if (!fRepeat)
          {
            using (ShellItem shellItem = new ShellItem(activeTab.CurrentIDL, false))
            {
              if (shellItem.IsFileSystem)
              {
                targetTabBar.CreateNewLibrary(activeTab);
                break;
              }
              SystemSounds.Beep.Play2();
              break;
            }
          }
          else
            break;
        case 82:
          if (!fRepeat)
          {
            targetTabBar.ActivateLast();
            break;
          }
          break;
        case 83:
          if (!fRepeat)
          {
            CommandInfo info = new CommandInfo(BarCommand.CreateNewFolder, 1);
            info.TargetView = targetView;
            this.InvokeCommand(info);
          }
          return true;
        case 99:
          if (!fRepeat)
            ShellMethods.PromptRename(activeTab.CurrentIDL, this.hwndExplorer, StringResources.Dialogs._Dialog[2]);
          return true;
        case 103:
          if (activeTab.AssociatedView.ListViewHandle == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Left);
          return true;
        case 104:
          if (activeTab.AssociatedView.ListViewHandle == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Up);
          return true;
        case 105:
          if (activeTab.AssociatedView.ListViewHandle == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Right);
          return true;
        case 106:
          if (activeTab.AssociatedView.ListViewHandle == PInvoke.GetFocus())
            WindowUtil.SendKey(Keys.Down);
          return true;
        case 112:
          targetTabBar.TabManager.SwapCurrentWithNext(true);
          break;
        case 113:
          targetTabBar.TabManager.SwapCurrentWithNext(false);
          break;
        case 124:
          if (!fRepeat)
            this.InvokeCommand(BarCommand.RenameDialogSelection, targetView);
          return true;
        case 125:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.ExtraLargeIcon);
          return true;
        case 126:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.LargeIcon);
          return true;
        case (int) sbyte.MaxValue:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.Icon);
          return true;
        case 128:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.SmallIcon);
          return true;
        case 129:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.List);
          return true;
        case 130:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.Details);
          return true;
        case 131:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.Tiles);
          return true;
        case 132:
          if (!fRepeat)
            activeTab.AssociatedView.InvokeMenuCommand(ExplorerMenuCommand.Contents);
          return true;
        case 133:
          if (!fRepeat)
            activeTab.AssociatedView.Sort("System.ItemNameDisplay");
          return true;
        case 134:
          if (!fRepeat)
            activeTab.AssociatedView.Sort("System.Size");
          return true;
        case 135:
          if (!fRepeat)
            activeTab.AssociatedView.Sort("System.ItemTypeText");
          return true;
        case 136:
          if (!fRepeat)
            activeTab.AssociatedView.Sort("System.DateModified");
          return true;
        case 137:
          if (!fRepeat)
            activeTab.AssociatedView.PasteIntoSelection();
          return true;
        case 160:
          if (!fRepeat && targetView != TargetView.Default)
          {
            CommandInfo info = new CommandInfo(BarCommand.MoveTabToView);
            info.Int = activeTab.ID;
            info.TargetView = TargetView.Default;
            this.InvokeCommand(info);
          }
          return true;
        case 161:
          if (!fRepeat && targetView != TargetView.Second)
          {
            CommandInfo info = new CommandInfo(BarCommand.MoveTabToView);
            info.Int = activeTab.ID;
            info.TargetView = TargetView.Second;
            this.InvokeCommand(info);
          }
          return true;
        case 162:
          if (!fRepeat && targetView != TargetView.Third)
          {
            CommandInfo info = new CommandInfo(BarCommand.MoveTabToView);
            info.Int = activeTab.ID;
            info.TargetView = TargetView.Third;
            this.InvokeCommand(info);
          }
          return true;
        case 163:
          if (!fRepeat)
          {
            CommandInfo info = new CommandInfo(BarCommand.MoveTabToView);
            info.Int = activeTab.ID;
            info.TargetView = this.Toolbars.GetTheOtherView(targetView);
            this.InvokeCommand(info);
          }
          return true;
        case 165:
          if (targetTabBar.TabManager.Count > 0)
            targetTabBar.tabControl.SelectTab(0);
          return true;
        case 166:
          if (targetTabBar.TabManager.Count > 1)
            targetTabBar.tabControl.SelectTab(1);
          return true;
        case 167:
          if (targetTabBar.TabManager.Count > 2)
            targetTabBar.tabControl.SelectTab(2);
          return true;
        case 168:
          if (targetTabBar.TabManager.Count > 3)
            targetTabBar.tabControl.SelectTab(3);
          return true;
        case 169:
          if (targetTabBar.TabManager.Count > 4)
            targetTabBar.tabControl.SelectTab(4);
          return true;
        case 170:
          if (targetTabBar.TabManager.Count > 5)
            targetTabBar.tabControl.SelectTab(5);
          return true;
        case 171:
          if (targetTabBar.TabManager.Count > 6)
            targetTabBar.tabControl.SelectTab(6);
          return true;
        case 172:
          if (targetTabBar.TabManager.Count > 7)
            targetTabBar.tabControl.SelectTab(7);
          return true;
        case 173:
          if (targetTabBar.TabManager.Count > 8)
            targetTabBar.tabControl.SelectTab(8);
          return true;
        case 174:
          if (targetTabBar.TabManager.Count > 9)
            targetTabBar.tabControl.SelectTab(9);
          return true;
        case 182:
          if (!fRepeat)
            this.InvokeCommand(new CommandInfo(BarCommand.ClipboardCurrentPathUNC, targetView));
          return true;
      }
      return false;
    }

    private void PerformPluginKey(KeyboardShortcutItem ksi, bool fRepeat)
    {
      Plugin plugin;
      if (!this.pluginManager.TryGetPlugin(ksi.PluginID, out plugin))
        return;
      try
      {
        if (plugin.PluginInformation.ShortcutKeyActions == null || plugin.PluginInformation.ShortcutKeyActions.Length <= ksi.Index)
          return;
        plugin.Instance.OnShortcutKeyPressed(ksi.Index);
        this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.ShortcutKeyPressed, (Keys) (ksi.Value & -1048577), ksi.Index, fRepeat, plugin.Instance));
      }
      catch (Exception ex)
      {
        IntPtr hwndExplorer = this.hwndExplorer;
        string name = plugin.PluginInformation.Name;
        string strCase = "On shortcut key pressed. Index is " + ksi.Index.ToString();
        PluginManager.HandlePluginException(ex, hwndExplorer, name, strCase);
      }
    }

    private void PerformCommandButtonKey(KeyboardShortcutItem ksi, bool fRepeat)
    {
      if (fRepeat)
        return;
      if (ksi.CommandButtonInfo.Type == CommandType.PlugIn)
        this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.PerformClickFromID(ksi.CommandButtonInfo.ID)));
      else
        new CommandInvoker((ICommandInvokerWindow) this, ksi.CommandButtonInfo.TargetView, this.GetFocusedTargetView()).Perform(ksi.CommandButtonInfo, true);
    }

    internal static bool HandleF2(IntPtr hwndListView, bool fVistaLayout)
    {
      if (hwndListView != IntPtr.Zero)
      {
        IntPtr num1 = fVistaLayout ? PInvoke.SendMessage(hwndListView, 4120, IntPtr.Zero, IntPtr.Zero) : PInvoke.GetFocus();
        if (num1 != IntPtr.Zero && (fVistaLayout || !("Edit" != PInvoke.GetClassName(num1)) && !("SHELLDLL_DefView" != PInvoke.GetClassName(PInvoke.GetWindowLongPtr(PInvoke.GetWindowLongPtr(PInvoke.GetWindowLongPtr(num1, GWL.HWNDPARENT), GWL.HWNDPARENT), GWL.HWNDPARENT)))))
        {
          int num2 = (int) PInvoke.SendMessage(num1, 14, 0, 0);
          if (num2 > 0)
          {
            IntPtr num3 = PInvoke.ZeroAlloc(num2 * 2 + 2);
            if (0 < (int) PInvoke.SendMessage(num1, 13, (IntPtr) (num2 + 1), num3))
            {
              string stringUni = Marshal.PtrToStringUni(num3);
              if (stringUni.Length > 2)
              {
                IntPtr lParam1 = PInvoke.SendMessage(num1, 176, IntPtr.Zero, IntPtr.Zero);
                int xLparam = MCR.GET_X_LPARAM(lParam1);
                int yLparam = MCR.GET_Y_LPARAM(lParam1);
                int num4 = stringUni.LastIndexOf(".");
                if (num4 == -1)
                {
                  int wParam;
                  int lParam2;
                  if (xLparam == 0 && yLparam == stringUni.Length)
                  {
                    wParam = stringUni.Length;
                    lParam2 = stringUni.Length;
                  }
                  else if (xLparam == stringUni.Length && yLparam == stringUni.Length)
                  {
                    wParam = 0;
                    lParam2 = 0;
                  }
                  else
                  {
                    wParam = 0;
                    lParam2 = -1;
                  }
                  PInvoke.SendMessage(num1, 177, (IntPtr) wParam, (IntPtr) lParam2);
                }
                else if (yLparam - xLparam >= 0)
                {
                  int lParam3;
                  int wParam;
                  if (xLparam == 0 && yLparam == num4)
                    wParam = lParam3 = num4;
                  else if (xLparam == yLparam && yLparam == num4)
                  {
                    wParam = num4 + 1;
                    lParam3 = stringUni.Length;
                  }
                  else if (xLparam == num4 + 1 && yLparam == stringUni.Length)
                  {
                    wParam = 0;
                    lParam3 = -1;
                  }
                  else if (xLparam == 0 && yLparam == stringUni.Length)
                  {
                    wParam = 0;
                    lParam3 = 0;
                  }
                  else
                  {
                    wParam = 0;
                    lParam3 = num4;
                  }
                  PInvoke.SendMessage(num1, 177, (IntPtr) wParam, (IntPtr) lParam3);
                }
              }
            }
            Marshal.FreeCoTaskMem(num3);
            return true;
          }
        }
      }
      return false;
    }

    private unsafe bool DispatchKeyToExtraViews(QTTabBarLib.Interop.MSG* msg) { !MenuUtility.InMenuLoop && 256 <= msg->message && msg->message <= 265 && (this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.ProcessViewKey(msg) || this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.ProcessViewKey(msg));

    private bool DispatchCommandKeyToExtraView(Keys keys)
    {
      bool flag = false;
      foreach (CustomViewBase extraView in this.Toolbars.ExtraViews)
      {
        if (extraView.Filtered)
        {
          extraView.InvokeMenuCommand(ExplorerMenuCommand.Refresh1);
          flag = true;
        }
      }
      if (flag)
        return false;
      TargetView targetView;
      CustomViewBase focusedView = this.GetFocusedView(out targetView);
      if (focusedView == null || targetView == TargetView.Default || keys != Keys.F5)
        return false;
      focusedView.InvokeMenuCommand(ExplorerMenuCommand.Refresh1);
      return true;
    }

    private bool OnViewDoubleClick(CustomViewEventArgs e)
    {
      if (e.IsControlPanel)
        return this.OnViewBackgroundClick(EventClick.LeftDouble);
      bool fMargin;
      using (ShellItem shellItem = new ShellItem(this.GetPIDLFromMouseEvent(e.Message, out fMargin)))
      {
        if (fMargin)
          return this.OnViewBackgroundClick(EventClick.LeftDouble);
        if (Control.ModifierKeys != Keys.None)
        {
          if (shellItem.IsFolder)
            return this.OnViewItemClick(shellItem, EventClick.LeftDouble);
        }
        else if (!QEnvironment.SingleClickMode)
          this.View.UpdateRecentFileFromSelection();
      }
      return false;
    }

    private unsafe void OnViewSingleClick(QTTabBarLib.Interop.MSG* msg)
    {
      bool fMargin;
      using (new ShellItem(this.GetPIDLFromMouseEvent(msg->ToMessage(), out fMargin)))
      {
        if (fMargin)
          return;
        this.View.UpdateRecentFileFromSelection();
      }
    }

    private bool OnViewMiddleClick(CustomViewEventArgs e)
    {
      if (e.IsControlPanel)
        return this.OnViewBackgroundClick(EventClick.Middle);
      bool fMargin;
      using (ShellItem shellItem = new ShellItem(this.GetPIDLFromMouseEvent(e.Message, out fMargin)))
      {
        if (fMargin || !shellItem.Available)
          return this.OnViewBackgroundClick(EventClick.Middle);
        if (shellItem.Available)
        {
          bool canCreateTab = shellItem.CanCreateTab;
          return !canCreateTab && UserEvents.GetInteractiveValue(UserEvents.ViewFileIconMiddleClick) == 4 || canCreateTab && UserEvents.GetInteractiveValue(UserEvents.ViewIconMiddleClick) == 18 ? this.OnViewBackgroundClick(EventClick.Middle) : (!canCreateTab ? this.OnViewFileItemClick(shellItem, EventClick.Middle) : this.OnViewItemClick(shellItem, EventClick.Middle));
        }
      }
      return false;
    }

    private bool OnViewEnterKeyDown()
    {
      if (Control.ModifierKeys != Keys.None)
      {
        IList<ItemIDList> selectionItemIdList = this.View.GetSelectionItemIDList(predicateAbs: ((Predicate<ShellItem>) (si { si.IsFolder)));
        if (selectionItemIdList != null && selectionItemIdList.Count > 0)
        {
          using (ShellItem shellItem = new ShellItem((byte[]) selectionItemIdList[0], false))
            return this.OnViewItemClick(shellItem, EventClick.Enter);
        }
      }
      this.View.UpdateRecentFileFromSelection();
      return false;
    }

    private bool OnViewItemClick(ShellItem shellItem, EventClick click)
    {
      int interactiveValueFromClick = UserEvents.GetInteractiveValueFromClick(click, (int[]) null, UserEvents.ViewIconMiddleClick, valueDoubleClick: UserEvents.ViewIconDoubleClick, valueEnterKey: UserEvents.ViewEnterKey);
      bool flag = UserEvents.IncludeSelectionForViewItemClick(click);
      if (UserEvents.IsIndexPersistentID(interactiveValueFromClick))
      {
        UserEvents.Fire(interactiveValueFromClick, (ICommandInvokerWindow) this, (object) ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null), UserEvents.MakeViewIconEventSource(click));
        return true;
      }
      TargetView view = this.makeTargetViewFromConfig(interactiveValueFromClick);
      switch (interactiveValueFromClick)
      {
        case 0:
        case 1:
        case 2:
          if (this.Toolbars.IsTabBarVisible)
            this.Toolbars.TabBar.OnViewItemUserEvent((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null, UserEvents.OptionFromConfig(interactiveValueFromClick));
          else if (shellItem.IsFolder)
            this.InvokeCommand(new CommandInfo(BarCommand.OpenInPlace, shellItem.IDL));
          return true;
        case 3:
        case 4:
        case 5:
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
          QSecondViewBar bar;
          if (this.Toolbars.TryGetAvailableExtraViewBar(view, out bar))
            bar.OnViewItemUserEvent((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null, UserEvents.OptionFromConfig(interactiveValueFromClick));
          else
            this.PendingFoldersExtraView = ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null);
          this.Toolbars.ShowExtraView(view, true);
          return true;
        case 12:
          if (this.Toolbars.IsTabBarVisible)
            this.Toolbars.TabBar.OnViewItemUserEvent((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null, UserEvents.OptionFromConfig(interactiveValueFromClick));
          else if (shellItem.IsFolder)
            this.InvokeCommand(new CommandInfo(BarCommand.NewWindow, shellItem.IDL));
          return true;
        case 13:
        case 14:
          bool fCut = interactiveValueFromClick == 14;
          if (ShellMethods.SetClipboardFileDropPaths(ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null), fCut, this.hwndExplorer))
          {
            FileOperationSound.Play(fCut ? OperationSound.Cut : OperationSound.Copy);
            break;
          }
          break;
        case 15:
          if (shellItem.IsFolder && ShellMethods.ClipboardContainsFileDropList())
          {
            FileOperationSound.Play(OperationSound.Paste);
            shellItem.PasteInto();
            break;
          }
          break;
        case 16:
          ShellMethods.ShowProperties(ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null), this.hwndExplorer);
          break;
        case 17:
          ShellMethods.SetStringClipboard(ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null).Select<ItemIDList, string>((Func<ItemIDList, string>) (idl {
          {
            using (ShellItem shellItem1 = new ShellItem((byte[]) idl, false))
              return shellItem1.Path;
          })).Where<string>((Func<string, bool>) (s { !string.IsNullOrEmpty(s))));
          break;
      }
      return click != EventClick.Middle;
    }

    private bool OnViewFileItemClick(ShellItem shellItem, EventClick click)
    {
      int interactiveValueFromClick = UserEvents.GetInteractiveValueFromClick(click, (int[]) null, UserEvents.ViewFileIconMiddleClick);
      bool flag = true;
      if (UserEvents.IsIndexPersistentID(interactiveValueFromClick))
      {
        UserEvents.Fire(interactiveValueFromClick, (ICommandInvokerWindow) this, (object) ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null), UserEvents.MakeViewFileIconEventSource(click));
        return true;
      }
      switch (interactiveValueFromClick)
      {
        case 0:
        case 1:
          bool fCut = interactiveValueFromClick == 1;
          if (ShellMethods.SetClipboardFileDropPaths(ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null), fCut, this.hwndExplorer))
          {
            FileOperationSound.Play(fCut ? OperationSound.Cut : OperationSound.Copy);
            break;
          }
          break;
        case 2:
          ShellMethods.ShowProperties(ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null), this.hwndExplorer);
          break;
        case 3:
          ShellMethods.SetStringClipboard(ShellUtils.AggregateIDLs((ItemIDList) shellItem, flag ? this.View.GetSelectionItemIDList() : (IList<ItemIDList>) null).Select<ItemIDList, string>((Func<ItemIDList, string>) (idl {
          {
            using (ShellItem shellItem1 = new ShellItem((byte[]) idl, false))
              return shellItem1.Path;
          })).Where<string>((Func<string, bool>) (s { !string.IsNullOrEmpty(s))));
          break;
      }
      return click != EventClick.Middle;
    }

    private bool OnViewBackgroundClick(EventClick click)
    {
      int interactiveValueFromClick = UserEvents.GetInteractiveValueFromClick(click, (int[]) null, UserEvents.ViewBackgroundMiddleClick, valueDoubleClick: UserEvents.ViewBackgroundDoubleClick);
      if (UserEvents.IsIndexPersistentID(interactiveValueFromClick))
      {
        UserEvents.Fire(interactiveValueFromClick, (ICommandInvokerWindow) this, (object) this.currentIDL, click == EventClick.Middle ? EventSource.ViewBackgroundMiddleClick : EventSource.ViewBackgroundDoubleClick);
        return true;
      }
      switch (interactiveValueFromClick)
      {
        case 0:
          this.InvokeCommand(BarCommand.GoUpOneLevel);
          return true;
        case 1:
          this.BrowseFolder((byte[]) null);
          return true;
        case 2:
          using (ShellItem shellItem = this.View.ShellItem)
            this.OpenNewWindow((IntPtr) shellItem);
          return true;
        case 3:
          this.InvokeCommand(new CommandInfo(BarCommand.CloneTab, BarCommandArg.Current, BarCommandOptions.IgnoreModKeys));
          break;
        case 4:
          using (ShellItem shellItem = this.View.ShellItem)
          {
            if (shellItem.HasPath)
              ShellMethods.SetStringClipboard(shellItem.Path);
          }
          return true;
        case 5:
          using (ShellItem shellItem = this.View.ShellItem)
            ShellMethods.SetStringClipboard(shellItem.DisplayName);
          return true;
        case 6:
          using (ShellItem shellItem = this.View.ShellItem)
            ShellMethods.ShowProperties(shellItem.IDL, this.hwndExplorer);
          return true;
        case 7:
          WindowUtil.CloseExplorer(this.hwndExplorer, WMCLOSEARG.Normal);
          return true;
        case 8:
          this.InvokeCommand(new CommandInfo(BarCommand.CloseTab, BarCommandArg.Current));
          return true;
        case 9:
          this.InvokeMenuCommand(ExplorerMenuCommand.SelectAll);
          return true;
        case 10:
          this.InvokeMenuCommand(ExplorerMenuCommand.InvertSelection);
          return true;
        case 11:
          this.InvokeCommand(BarCommand.CreateNewFolder);
          return true;
        case 12:
          this.InvokeCommand(BarCommand.CreateNewTxtFile);
          return true;
        case 13:
          this.InvokeCommand(BarCommand.CopyFromFolder);
          return true;
        case 14:
          this.InvokeCommand(BarCommand.MoveFromFolder);
          return true;
        case 15:
          this.InvokeCommand(BarCommand.LinkFromFolder);
          return true;
        case 16:
          this.InvokeMenuCommand(ExplorerMenuCommand.Paste);
          return true;
      }
      return false;
    }

    private void HandleXBUTTON()
    {
      if (MenuUtility.Visible)
        return;
      MouseButtons mouseButtons = Control.MouseButtons;
      Keys modifierKeys = Control.ModifierKeys;
      if (mouseButtons != MouseButtons.XButton1 && mouseButtons != MouseButtons.XButton2)
        return;
      bool flag = mouseButtons == MouseButtons.XButton1 ^ Config.Bool(Scts.SwapMouseXButtons);
      if (modifierKeys.HasFlag((Enum) Keys.Shift) && !Config.Bool(Scts.ViewXBtnNoNavInAllViewShft))
      {
        BarCommand barCommand = modifierKeys.HasFlag((Enum) Keys.Control) ? (flag ? BarCommand.GoBackToFirst : BarCommand.GoForwardToLast) : (flag ? BarCommand.GoBack : BarCommand.GoForward);
        this.InvokeCommand(barCommand);
        if (this.Toolbars.IsSecondViewBarTabAvailable)
          this.Toolbars.SecondViewBar.InvokeCommand(barCommand);
        if (!this.Toolbars.IsThirdViewBarTabAvailable)
          return;
        this.Toolbars.ThirdViewBar.InvokeCommand(barCommand);
      }
      else
      {
        BarCommand barCommand = modifierKeys == Keys.Control ? (flag ? BarCommand.GoBackToFirst : BarCommand.GoForwardToLast) : (flag ? BarCommand.GoBack : BarCommand.GoForward);
        if (this.Toolbars.SecondViewHasFocus)
          this.Toolbars.SecondViewBar.InvokeCommand(barCommand);
        else if (this.Toolbars.ThirdViewHasFocus)
          this.Toolbars.ThirdViewBar.InvokeCommand(barCommand);
        else
          this.InvokeCommand(barCommand);
      }
    }

    private TargetView makeTargetViewFromConfig(int config, TargetView defaultView = TargetView.Default)
    {
      TargetView targetView = defaultView;
      switch (config)
      {
        case 3:
        case 4:
        case 5:
          targetView = TargetView.Second;
          break;
        case 6:
        case 7:
        case 8:
          targetView = TargetView.Third;
          break;
        case 9:
        case 10:
        case 11:
          targetView = this.Toolbars.GetTheOtherView(TargetView.Default);
          break;
      }
      return targetView;
    }

    public unsafe bool HandleMOUSEWHEEL(IntPtr lParam)
    {
      MOUSEHOOKSTRUCTEX* mousehookstructexPtr = (MOUSEHOOKSTRUCTEX*) (void*) lParam;
      int h = MCR.HIWORD(mousehookstructexPtr->mouseData);
      IntPtr num = PInvoke.WindowFromPoint(new Point(mousehookstructexPtr->mhs.pt.x, mousehookstructexPtr->mhs.pt.y));
      Control control = Control.FromHandle(num);
      switch (control)
      {
        case null:
          CustomViewBase view = (CustomViewBase) null;
          if (num == this.hwndListView)
            view = (CustomViewBase) this.View;
          else if (this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.ListViewHandle == num)
            view = (CustomViewBase) this.Toolbars.SecondViewBar.ActiveTab.View;
          else if (this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.ListViewHandle == num)
            view = (CustomViewBase) this.Toolbars.ThirdViewBar.ActiveTab.View;
          if (view != null)
          {
            this.ScrollNotified(view);
            if (PInvoke.HasFocus(num) && Config.Bool(Scts.ViewHorizontalScroll) && Control.ModifierKeys == Keys.Shift)
            {
              view.Scroll(h < 0, false);
              return true;
            }
          }
          return false;
        case DropDownMenuEx dropDownMenuEx:
          if (dropDownMenuEx.CanScroll)
            PInvoke.SendMessage(num, 522, MCR.MAKELPARAM(0, h), MCR.MAKELPARAM(mousehookstructexPtr->mhs.pt.x, mousehookstructexPtr->mhs.pt.y));
          return true;
        case ToolStripEx parent:
label_5:
          ToolStripEx toolStripEx = parent;
          if (toolStripEx != null)
          {
            if (toolStripEx.NotifyMouseWheel)
            {
              PInvoke.SendMessage(num, 522, MCR.MAKELPARAM(0, h), MCR.MAKELPARAM(mousehookstructexPtr->mhs.pt.x, mousehookstructexPtr->mhs.pt.y));
              return true;
            }
            goto case null;
          }
          else if (control is SubDirTip subDirTip)
          {
            PInvoke.PostMessage(num, 522, MCR.MAKELPARAM(0, h), MCR.MAKELPARAM(mousehookstructexPtr->mhs.pt.x, mousehookstructexPtr->mhs.pt.y));
            if (subDirTip.OwnerListViewWindow == IntPtr.Zero)
              return subDirTip.NotifyMouseWheel;
            num = subDirTip.OwnerListViewWindow;
            goto case null;
          }
          else
            goto case null;
        default:
          parent = control.Parent as ToolStripEx;
          goto label_5;
      }
    }

    private unsafe bool HandleWheelWithRight(IntPtr lParam)
    {
      if (Control.MouseButtons == MouseButtons.Right && Config.Bool(Scts.TabWheelWithRightBtn))
      {
        MOUSEHOOKSTRUCTEX* mousehookstructexPtr = (MOUSEHOOKSTRUCTEX*) (void*) lParam;
        int num = MCR.HIWORD(mousehookstructexPtr->mouseData);
        Point point = mousehookstructexPtr->mhs.pt.ToPoint();
        TabBarBase tabBarBase = (TabBarBase) null;
        bool flag = false;
        Rectangle viewBarBounds;
        if (this.Toolbars.IsSecondViewBarTabAvailable)
        {
          viewBarBounds = this.Toolbars.SecondViewBar.ViewBarBounds;
          if (viewBarBounds.Contains(point))
          {
            tabBarBase = (TabBarBase) this.Toolbars.SecondViewBar;
            flag = true;
            goto label_9;
          }
        }
        if (this.Toolbars.IsThirdViewBarTabAvailable)
        {
          viewBarBounds = this.Toolbars.ThirdViewBar.ViewBarBounds;
          if (viewBarBounds.Contains(point))
          {
            tabBarBase = (TabBarBase) this.Toolbars.ThirdViewBar;
            flag = true;
            goto label_9;
          }
        }
        if (this.Toolbars.IsTabBarAvailable)
          tabBarBase = (TabBarBase) this.Toolbars.TabBar;
label_9:
        if (tabBarBase != null)
        {
          if (flag)
          {
            if (((QSecondViewBar) tabBarBase).ActiveTab.View.MouseRightButtonPressed)
              CustomViewBase.SuppressContextMenu = true;
            PInvoke.ReleaseCapture();
          }
          tabBarBase.OnTabWheel(num < 0);
          this.fSuppressRightButtonUp = true;
          return true;
        }
      }
      return false;
    }

    private void OnFolderBandRightClick()
    {
      if (this.contextMenFolderBand == null)
      {
        this.contextMenFolderBand = new ContextMenuStripEx(this.components);
        this.contextMenFolderBand.ImageList = Graphic.ImageList;
        this.contextMenFolderBand.Items.AddRange(MenuUtility.CreateToolbarMenuItems(true, false));
        this.contextMenFolderBand.Opening += new CancelEventHandler(this.contextMenFolderBand_Opening);
        this.contextMenFolderBand.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenFolderBand_ItemClicked);
      }
      this.contextMenFolderBand.Show(Control.MousePosition);
    }

    private bool OnBreadcrumbsClick(QTTabBarLib.Interop.MSG msg)
    {
      int num1 = msg.message == 519 ? 1 : 0;
      EventSource source = num1 != 0 ? EventSource.BreadcrumbsMiddleClick : EventSource.BreadcrumbsClick;
      int num2 = UserEvents.GetInteractiveValueFrom(num1 != 0 ? MouseButtons.Middle : MouseButtons.Left, UserEvents.BreadcrumbsClick, UserEvents.BreadcrumbsMiddleClick);
      TargetView targetView = this.makeTargetViewFromConfig(num2);
      switch (num2)
      {
        case 3:
        case 6:
        case 9:
          num2 = 0;
          break;
        case 4:
        case 7:
        case 10:
          num2 = 1;
          break;
        case 5:
        case 8:
        case 11:
          num2 = 2;
          break;
      }
      int index1 = QUIAutomation.HitTestOnBreadcrumbsBar(this.hwndBreadcrumbParent, Control.MousePosition);
      switch (index1)
      {
        case -3:
        case -1:
          return false;
        default:
          using (ShellItem si1 = new ShellItem(this.currentIDL, false))
          {
            if (si1.Available)
            {
              if (index1 == -2)
              {
                if (si1.IsDesktop)
                {
                  if (!UserEvents.IsIndexPersistentID(num2))
                    return this.OnBreadcrumbsClickCore(num2, targetView, si1, index1, true);
                  UserEvents.Fire(num2, (ICommandInvokerWindow) this, (object) si1.Path, source);
                  return true;
                }
              }
              else
              {
                IntPtr num3 = PInvoke.ILClone(si1.PIDL);
                for (int index2 = 0; index2 < index1; ++index2)
                {
                  IntPtr parentIdl = ShellMethods.GetParentIDL(num3);
                  if (num3 != IntPtr.Zero)
                    PInvoke.CoTaskMemFree(num3);
                  num3 = parentIdl;
                  if (num3 == IntPtr.Zero)
                    break;
                }
                if (num3 != IntPtr.Zero)
                {
                  using (ShellItem si2 = new ShellItem(num3))
                  {
                    if (!UserEvents.IsIndexPersistentID(num2))
                      return this.OnBreadcrumbsClickCore(num2, targetView, si2, index1, false);
                    UserEvents.Fire(num2, (ICommandInvokerWindow) this, (object) si2.Path, source);
                    return true;
                  }
                }
              }
            }
          }
          return true;
      }
    }

    private bool OnBreadcrumbsClickCore(
      int config,
      TargetView targetView,
      ShellItem si,
      int index,
      bool fDesktop)
    {
      int num;
      switch (config)
      {
        case 0:
          num = 0;
          break;
        case 1:
          num = 2;
          break;
        case 2:
          if (fDesktop)
          {
            if (targetView == TargetView.Default)
              return false;
            CommandInfo info = new CommandInfo(BarCommand.OpenInPlace, si.IDL, BarCommandOptions.IgnoreModKeys);
            info.TargetView = targetView;
            this.InvokeCommand(info);
            goto default;
          }
          else
          {
            if (index == 0 && targetView == TargetView.Default)
              return false;
            CommandInfo info = new CommandInfo(BarCommand.OpenInPlace, si.IDL, BarCommandOptions.IgnoreModKeys);
            info.TargetView = targetView;
            this.InvokeCommand(info);
            goto default;
          }
        case 12:
          this.InvokeCommand(new CommandInfo(BarCommand.NewWindow, si.IDL));
          goto default;
        case 13:
          this.InvokeCommand(new CommandInfo(BarCommand.ShowProperties, si.IDL));
          goto default;
        case 14:
          this.InvokeCommand(new CommandInfo(BarCommand.ClipboardPath, si.IDL));
          goto default;
        default:
label_14:
          return true;
      }
      BarCommandOptions options = (BarCommandOptions) (num | 16);
      CommandInfo info1 = new CommandInfo(BarCommand.NewTab, si.IDL, options);
      info1.TargetView = targetView;
      this.InvokeCommand(info1);
      goto label_14;
    }

    private bool OnFolderTreeMouseEvent(QTTabBarLib.Interop.MSG msg, NavigationPane navigationPane)
    {
      this.navigationPaneMouseEvent = (NavigationPane) null;
      bool flag1 = msg.message == 513 || msg.message == 519;
      bool flag2 = msg.message == 520 || msg.message == 519;
      int num = UserEvents.GetInteractiveValueFrom(flag2 ?
        MouseButtons.Middle : MouseButtons.Left, UserEvents.NavPaneClick, UserEvents.NavPaneMiddleClick);
      TargetView targetView = this.makeTargetViewFromConfig(num, navigationPane.TargetView);
      if (!flag2 && num == 2)
        return false;
      int flags;
      IntPtr hitem = navigationPane.HitTest(msg.lParam, out flags);
      bool flag3 = (flags & 16) != 0;
      ShellItem shellItem = (ShellItem) null;
      if (hitem != IntPtr.Zero && !flag3)
        shellItem = navigationPane.GetShellItemFromNode(hitem);
      using (shellItem)
      {
        if (shellItem != null && shellItem.Available && !shellItem.IsFolder)
          return false;
        if (flag1)
        {
          this.navigationPaneMouseEvent = navigationPane;
          if (!flag2)
          {
            navigationPane.DragStartPoint = MCR.GET_POINT_LPARAM(msg.lParam);
            navigationPane.Focus();
          }
          navigationPane.PreviousMouseDownItemHandle = hitem;
          return !flag3;
        }
        navigationPane.DragStartPoint = Point.Empty;
        if (navigationPane.PreviousMouseDownItemHandle != hitem)
        {
          navigationPane.PreviousMouseDownItemHandle = IntPtr.Zero;
          return true;
        }
        navigationPane.PreviousMouseDownItemHandle = IntPtr.Zero;
        if (shellItem != null)
        {
          if (shellItem.Available)
          {
            if (UserEvents.IsIndexPersistentID(num))
            {
              UserEvents.Fire(num, (ICommandInvokerWindow) this, (object) shellItem.Path, flag2 ?
                EventSource.NavPaneMiddleClick : EventSource.NavPaneClick);
              return true;
            }
            switch (num - 3)
            {
              case 0:
              case 3:
              case 6:
                num = 0;
                break;
              case 1:
              case 4:
              case 7:
                num = 1;
                break;
              case 2:
              case 5:
              case 8:
                num = 2;
                break;
            }
            if ((uint) num > 2U)
            {
              switch (num - 12)
              {
                case 0:
                  CommandInfo info1 = new CommandInfo(BarCommand.NewWindow, shellItem.IDL);
                  info1.TargetView = targetView;
                  this.InvokeCommand(info1);
                  break;
                case 1:
                  if (ShellMethods.ClipboardContainsFileDropList())
                  {
                    FileOperationSound.Play(OperationSound.Paste);
                    shellItem.PasteInto();
                    break;
                  }
                  break;
                case 2:
                  this.InvokeCommand(new CommandInfo(BarCommand.ShowProperties, shellItem.IDL));
                  break;
                case 3:
                  this.InvokeCommand(new CommandInfo(BarCommand.ClipboardPath, shellItem.IDL));
                  break;
                case 4:
                  navigationPane.StartRename(hitem);
                  return true;
              }
            }
            else
            {
              BarCommand command = num == 2 ? BarCommand.OpenInPlace : BarCommand.NewTab;
              BarCommandOptions options = (BarCommandOptions) ((num == 1 ? 2 : 0) | 16);
              CommandInfo info2 = new CommandInfo(command, shellItem.IDL, options);
              info2.TargetView = targetView;
              this.InvokeCommand(info2);
            }
            if (navigationPane == this.NavigationPane)
            {
              navigationPane.SelectedNode = hitem;
              navigationPane.Focus();
            }
            return true;
          }
        }
      }
      return false;
    }

    private bool OnFolderTreeEnterKeyDown(NavigationPane navigationPane)
    {
      IntPtr selectedNode = navigationPane.SelectedNode;
      using (ShellItem shellItemFromNode = navigationPane.GetShellItemFromNode(selectedNode))
      {
        if (shellItemFromNode.Available)
        {
          int num = UserEvents.GetInteractiveValue(UserEvents.NavPaneEnter);
          if (UserEvents.IsIndexPersistentID(num))
          {
            UserEvents.Fire(num, (ICommandInvokerWindow) this, (object) shellItemFromNode.Path, EventSource.NavPaneEnter);
            return true;
          }
          TargetView targetView = this.makeTargetViewFromConfig(num, navigationPane.TargetView);
          switch (num - 3)
          {
            case 0:
            case 3:
            case 6:
              num = 0;
              break;
            case 1:
            case 4:
            case 7:
              num = 1;
              break;
            case 2:
            case 5:
            case 8:
              num = 2;
              break;
          }
          if ((uint) num > 2U)
          {
            switch (num - 12)
            {
              case 0:
                CommandInfo info1 = new CommandInfo(BarCommand.NewWindow, shellItemFromNode.IDL);
                info1.TargetView = targetView;
                this.InvokeCommand(info1);
                break;
              case 1:
                if (ShellMethods.ClipboardContainsFileDropList())
                {
                  FileOperationSound.Play(OperationSound.Paste);
                  shellItemFromNode.PasteInto();
                  break;
                }
                break;
              case 2:
                this.InvokeCommand(new CommandInfo(BarCommand.ShowProperties, shellItemFromNode.IDL));
                break;
              case 3:
                this.InvokeCommand(new CommandInfo(BarCommand.ClipboardPath, shellItemFromNode.IDL));
                navigationPane.SelectedNode = selectedNode;
                break;
              case 4:
                navigationPane.StartRename(selectedNode);
                return true;
              default:
                navigationPane.SelectedNode = selectedNode;
                break;
            }
          }
          else
          {
            BarCommand command = num == 2 ? BarCommand.OpenInPlace : BarCommand.NewTab;
            BarCommandOptions options = (BarCommandOptions) ((num == 1 ? 2 : 0) | 16);
            CommandInfo info2 = new CommandInfo(command, shellItemFromNode.IDL, options);
            info2.TargetView = targetView;
            this.InvokeCommand(info2);
          }
          return true;
        }
      }
      return false;
    }

    private void OnFolderTreeMouseMove(QTTabBarLib.Interop.MSG msg, NavigationPane navigationPane)
    {
      if (this.navigationPaneMouseEvent == null || this.navigationPaneMouseEvent != navigationPane || !(this.navigationPaneMouseEvent.DragStartPoint != Point.Empty) || !(this.navigationPaneMouseEvent.PreviousMouseDownItemHandle != IntPtr.Zero) || !DragDropUtils.OutOfSystemDragSize(this.navigationPaneMouseEvent.DragStartPoint, MCR.GET_POINT_LPARAM(msg.lParam), ExplorerManager.WindowScaling))
        return;
      ShellMethods.DoDragDrop(this.navigationPaneMouseEvent.GetShellItemFromNode(this.navigationPaneMouseEvent.PreviousMouseDownItemHandle));
      this.navigationPaneMouseEvent.DragStartPoint = Point.Empty;
      this.navigationPaneMouseEvent.PreviousMouseDownItemHandle = IntPtr.Zero;
      this.navigationPaneMouseEvent = (NavigationPane) null;
    }

    private bool OnFolderTreeKeyShortcut(
      int keyShortcut,
      bool fRepeat,
      NavigationPane navigationPane)
    {
      switch (keyShortcut)
      {
        case 27:
        case 28:
        case 181:
          if (!fRepeat)
          {
            IntPtr selectedNode = navigationPane.SelectedNode;
            if (selectedNode != IntPtr.Zero)
            {
              using (ShellItem shellItemFromNode = navigationPane.GetShellItemFromNode(selectedNode))
              {
                if (shellItemFromNode.Available)
                {
                  string str;
                  switch (keyShortcut)
                  {
                    case 27:
                      str = shellItemFromNode.Path;
                      break;
                    case 181:
                      str = PathString.GetUniversalName(shellItemFromNode.Path);
                      break;
                    default:
                      str = shellItemFromNode.DisplayName;
                      break;
                  }
                  ShellMethods.SetStringClipboard(str);
                }
              }
            }
          }
          return true;
        default:
          return false;
      }
    }

    private unsafe bool explorerSubclassProc(ref Message msg)
    {
      switch (msg.Msg)
      {
        case 0:
          return false;
        case 3:
          this.View.HidePreviewTip();
          this.View.HideSubDirTip(false);
          break;
        case 5:
          this.View.HidePreviewTip();
          this.View.HideSubDirTip(false);
          if (msg.WParam == (IntPtr) 2)
          {
            this.ExpandToolbarsOnMaximized();
            break;
          }
          break;
        case 6:
          if (!this.fNowClosing)
          {
            int num = (int) (long) msg.WParam & (int) ushort.MaxValue;
            if (num > 0)
            {
              this.OnExplorerActivating();
              break;
            }
            if (num == 0)
            {
              this.OnExplorerDeactivating(msg);
              break;
            }
            break;
          }
          break;
        case 70:
          this.OnWindowPosChanging((WINDOWPOS*) (void*) msg.LParam);
          break;
        case 161:
        case 164:
        case 167:
        case 171:
          this.HideTabSwitcher(false);
          break;
        case 163:
          switch ((int) (long) msg.WParam)
          {
            case 2:
              if (this.fHorizontalyMaximized)
              {
                this.fHorizontalyMaximized = false;
                PInvoke.SetWindowPos(this.hwndExplorer, IntPtr.Zero, this.rctHorizontalyMaximizing.X, this.rctHorizontalyMaximizing.Y, this.rctHorizontalyMaximizing.Width, this.rctHorizontalyMaximizing.Height, SWP.NOZORDER);
                return true;
              }
              break;
            case 10:
            case 11:
              if (!Config.Bool(Scts.DisableWindowHorizontalMaximize))
              {
                this.MaximizeHorizontally(Screen.FromPoint(MCR.GET_POINT_LPARAM(msg.LParam)));
                break;
              }
              break;
            case 12:
            case 15:
              if (this.fHorizontalyMaximized)
              {
                this.fHorizontalyMaximized = false;
                PInvoke.SetWindowPos(this.hwndExplorer, IntPtr.Zero, this.rctHorizontalyMaximizing.X, this.rctHorizontalyMaximizing.Y, this.rctHorizontalyMaximizing.Width, this.rctHorizontalyMaximizing.Height, SWP.NOZORDER);
                PInvoke.ShowWindow(this.hwndExplorer, SHOWWINDOW.SHOWMAXIMIZED);
                return true;
              }
              break;
          }
          break;
        case 274:
          switch ((int) (long) msg.WParam & 65520)
          {
            case 61456:
              if (this.fHorizontalyMaximized)
              {
                this.pntWndDragStartHMaximized = MCR.GET_POINT_LPARAM(msg.LParam);
                break;
              }
              break;
            case 61472:
              this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.ExplorerStateChanged, ExplorerWindowActions.Minimized));
              if (Config.Bool(Scts.SendWindowToTrayOnMinimize))
              {
                this.SendToTasktray();
                return true;
              }
              break;
            case 61488:
              if (this.fHorizontalyMaximized)
              {
                if (PInvoke.IsIconic(this.hwndExplorer))
                  PInvoke.ShowWindow(this.hwndExplorer, SHOWWINDOW.RESTORE);
                this.fHorizontalyMaximized = false;
                PInvoke.SetWindowPos(this.hwndExplorer, IntPtr.Zero, this.rctHorizontalyMaximizing.X, this.rctHorizontalyMaximizing.Y, this.rctHorizontalyMaximizing.Width, this.rctHorizontalyMaximizing.Height, SWP.NOZORDER);
              }
              this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.ExplorerStateChanged, ExplorerWindowActions.Maximized));
              break;
            case 61536:
              int yLparam = MCR.GET_Y_LPARAM(msg.LParam);
              bool flag1 = yLparam != 0 && yLparam != -1;
              int interactiveValue = UserEvents.GetInteractiveValue(flag1 ? UserEvents.WindowCloseButton : UserEvents.JumpListCommand);
              if (UserEvents.IsIndexPersistentID(interactiveValue))
              {
                UserEvents.Fire(interactiveValue, (ICommandInvokerWindow) this, (object) this.hwndExplorer, flag1 ? EventSource.WindowCloseButton : EventSource.JumpListCommand);
                return true;
              }
              switch (interactiveValue)
              {
                case 0:
                  this.InvokeCommand(new CommandInfo(BarCommand.CloseTab, BarCommandArg.Current));
                  break;
                case 1:
                  this.InvokeCommand(BarCommand.CloseWindow);
                  break;
                case 2:
                  this.InvokeCommand(BarCommand.MinimizeToTasktray);
                  break;
                case 3:
                  this.InvokeCommand(new CommandInfo(BarCommand.CloseTab, BarCommandArg.AllButCurrent));
                  break;
              }
              return true;
            case 61728:
              this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.ExplorerStateChanged, ExplorerWindowActions.Restored));
              break;
          }
          break;
        case 295:
        case 296:
          if (this.View != null && this.View.VistaLayout)
          {
            CustomViewBase.HandleUIStateMessages(ref msg);
            break;
          }
          break;
        case 297:
          if (this.View != null && this.View.VistaLayout && !Config.Bool(Scts.NoOriginalFocusRectangle))
          {
            msg.Result = (IntPtr) 3;
            return true;
          }
          break;
        case 528:
          switch ((int) (long) msg.WParam & (int) ushort.MaxValue)
          {
            case 513:
            case 516:
            case 519:
            case 523:
              this.HideTabSwitcher(false);
              break;
          }
          break;
        case 532:
          if (this.fHorizontalyMaximized)
          {
            RECT rect = new RECT(this.rctHorizontalyMaximizing);
            RECT* lparam = (RECT*) (void*) msg.LParam;
            switch ((long) msg.WParam)
            {
              case 1:
              case 4:
              case 7:
                rect.left = lparam->left;
                *lparam = rect;
                msg.Result = (IntPtr) 1;
                this.fHorizontalyMaximized = false;
                return true;
              case 2:
              case 5:
              case 8:
                rect.right = lparam->right;
                *lparam = rect;
                msg.Result = (IntPtr) 1;
                this.fHorizontalyMaximized = false;
                return true;
            }
          }
          else
            break;
          break;
        case 534:
          if (this.fHorizontalyMaximized)
          {
            RECT* lparam = (RECT*) (void*) msg.LParam;
            Point mousePosition = Control.MousePosition;
            int num1 = (int) (48.0 * (double) ExplorerManager.WindowScaling);
            if (Math.Abs(this.pntWndDragStartHMaximized.X - mousePosition.X) < num1)
            {
              lparam->left = this.rctHorizontalyMaximized.Left;
              lparam->right = this.rctHorizontalyMaximized.Right;
              this.rctHorizontalyMaximized.Y = lparam->top;
              msg.Result = (IntPtr) 1;
              return true;
            }
            this.fHorizontalyMaximized = false;
            if (lparam->left < mousePosition.X && mousePosition.X < lparam->right && lparam->Width > 0)
            {
              double num2 = (double) (mousePosition.X - lparam->left) / (double) lparam->Width;
              int num3 = (int) ((double) mousePosition.X - (double) this.rctHorizontalyMaximizing.Width * num2);
              lparam->left = num3;
              lparam->right = num3 + this.rctHorizontalyMaximizing.Width;
            }
            else
            {
              lparam->left = mousePosition.X - 4;
              lparam->top = mousePosition.Y - 4;
              lparam->right = mousePosition.X - 4 + this.rctHorizontalyMaximizing.Width;
              lparam->bottom = mousePosition.Y - 4 + this.rctHorizontalyMaximizing.Height;
            }
            msg.Result = (IntPtr) 1;
            return true;
          }
          break;
        case 736:
          Rectangle bounds = this.Bounds;
          float num4 = 0.0f;
          float num5 = 0.0f;
          if (this.Toolbars.IsSecondViewBarVisible)
            num4 = (float) this.Toolbars.SecondViewBar.BaseBarSize / (float) bounds.Height;
          if (this.Toolbars.IsThirdViewBarVisible)
            num5 = (float) this.Toolbars.ThirdViewBar.BaseBarSize / (float) bounds.Width;
          WindowUtil.SetRedraw(this.ExplorerHandle, false);
          try
          {
            int windowDpi = ExplorerManager.WindowDpi;
            this.Dpi = ExplorerManager.windowDpi = (int) (long) msg.WParam & (int) ushort.MaxValue;
            this.NotifyDpiChange(windowDpi);
            this.explorerWindowSubclass.DefaultWindowProcedure(ref msg);
            this.fFirstDpiChangeNotified = true;
          }
          finally
          {
            WindowUtil.SetRedraw(this.ExplorerHandle, true);
            PInvoke.InvalidateRect(this.ExplorerHandle, IntPtr.Zero, true);
            if ((double) num4 != 0.0)
            {
              CommandInfo info = new CommandInfo(BarCommand.SetExtraviewSize);
              info.Ints = new int[3]
              {
                QMath.Round(num4 * 100f),
                1,
                1
              };
              this.InvokeCommand(info);
            }
            if ((double) num5 != 0.0)
            {
              CommandInfo info = new CommandInfo(BarCommand.SetExtraviewSize);
              info.Ints = new int[3]
              {
                QMath.Round(num5 * 100f),
                0,
                1
              };
              this.InvokeCommand(info);
            }
          }
          return true;
        case 800:
          if (!this.fNoMoreDWMCOLORIZATIONCOLORCHANGED)
          {
            this.fNoMoreDWMCOLORIZATIONCOLORCHANGED = true;
            ActionDelayer.Add((Action) (() {
            {
              this.Toolbars.Views.ForEach<CustomViewBase>((Action<CustomViewBase>) (v {
              {
                if (v == null || !v.VistaLayout)
                  return;
                PInvoke.InvalidateRect(v.ListViewHandle, IntPtr.Zero, true);
              }));
              WindowUtil.RedrawNonClientArea(this.ExplorerHandle);
              this.fNoMoreDWMCOLORIZATIONCOLORCHANGED = false;
            }), 1500);
            break;
          }
          break;
      }
      if (msg.Msg == RegisteredMessage.SetModal)
      {
        this.InModal = msg.WParam != IntPtr.Zero;
        return true;
      }
      if (msg.Msg == RegisteredMessage.GetExplorerManagerWindow)
      {
        msg.Result = this.Handle;
        return true;
      }
      if (msg.Msg == RegisteredMessage.DebugButtonMessage)
        return true;
      bool flag2 = false;
      if (this.ExplorerMessageCaptured != null)
        flag2 = this.ExplorerMessageCaptured(ref msg);
      if (!flag2 && msg.Msg == 16)
      {
        this.OnExplorerClosing();
        msg.LParam = IntPtr.Zero;
      }
      return flag2;
    }

    private unsafe bool rebarSubclassProc(ref Message msg)
    {
      switch (msg.Msg)
      {
        case 20:
          if (Config.Bool(Scts.RebarBackgroundColor) || Config.Bool(Scts.RebarBackgroundImage) || QEnvironment.LaterThan7)
          {
            bool flag = false;
            Rectangle rectangle = new Rectangle(Point.Empty, PInvoke.GetWindowRect(msg.HWnd).Size);
            using (Graphics g = Graphics.FromHdc(msg.WParam))
            {
              System.Drawing.Color backColor = Config.Bool(Scts.RebarBackgroundColor) ? Config.RebarBackgroundColor : (QEnvironment.IsWindows8 ? SystemColors.Window : System.Drawing.Color.Transparent);
              if (Config.Bool(Scts.RebarBackgroundImage))
              {
                if (Config.Path_RebarImage.Length > 0)
                {
                  if (this.toolbarImageHelper == null)
                    this.toolbarImageHelper = new ToolbarImageHelper();
                  flag = this.toolbarImageHelper.Draw(g, rectangle, ref msg, new DefWndProc(this.rebarWindowSubclass.DefaultWindowProcedure), backColor);
                }
              }
            }
            if (!flag && Config.Bool(Scts.RebarBackgroundColor))
            {
              Graphic.FillRectangleRTL(msg.WParam, Config.RebarBackgroundColor, rectangle, QEnvironment.RightToLeft);
              flag = true;
            }
            if (!flag && QEnvironment.LaterThan7)
            {
              Graphic.FillRectangleRTL(msg.WParam, ShellColors.Default, rectangle, QEnvironment.RightToLeft);
              flag = true;
            }
            if (flag)
            {
              msg.Result = (IntPtr) 1;
              return true;
            }
            break;
          }
          break;
        case 1035:
          if (!this.fNowClosing)
          {
            REBARBANDINFO* lparam = (REBARBANDINFO*) (void*) msg.LParam;
            if ((lparam->fMask & 1) != 0)
            {
              int bandLayoutFromId = this.GetBandLayoutFromID(lparam->wID);
              if (bandLayoutFromId != -1)
              {
                if ((bandLayoutFromId & (int) ushort.MaxValue) == 0)
                {
                  lparam->fStyle |= 1;
                  break;
                }
                lparam->fStyle &= -2;
                break;
              }
              lparam->fStyle &= -2;
              break;
            }
            break;
          }
          break;
      }
      return false;
    }

    private bool shellTabWindowSubclassProc(ref Message msg)
    {
      switch (msg.Msg)
      {
        case 273:
          switch ((ExplorerMenuCommand) ((int) (long) msg.WParam & (int) ushort.MaxValue))
          {
            case ExplorerMenuCommand.CopyToFolder:
            case ExplorerMenuCommand.MoveToFolder:
            case ExplorerMenuCommand.ChooseDetails:
            case ExplorerMenuCommand.OSVersion:
            case ExplorerMenuCommand.FolderOptions:
              this.InModal = true;
              this.shellTabWindowSubclass.DefaultWindowProcedure(ref msg);
              this.InModal = false;
              return true;
            case ExplorerMenuCommand.GoUpOneLevel:
              if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
              {
                this.ViewSyncManager.OnGoingUp();
                break;
              }
              break;
            case ExplorerMenuCommand.Unknown01:
              if (this.fSuppressFocusing41008)
              {
                this.fSuppressFocusing41008 = false;
                return true;
              }
              break;
            case ExplorerMenuCommand.ToggleToolbarLock:
              this.SaveCurrentBandLayout();
              break;
            case ExplorerMenuCommand.Refresh2:
              this.NotifyBrowserRefresh(true);
              break;
          }
          break;
      }
      if (this.View != null && this.View.VistaLayout)
      {
        switch (msg.Msg)
        {
          case 295:
          case 296:
            CustomViewBase.HandleUIStateMessages(ref msg);
            break;
          case 297:
            if (!Config.Bool(Scts.NoOriginalFocusRectangle))
            {
              msg.Result = (IntPtr) 3;
              return true;
            }
            break;
        }
      }
      if (msg.Msg == RegisteredMessage.ShellContextmenuHandler)
      {
        this.OnDynamicVerbInvoked(msg, TargetView.Default);
        return true;
      }
      bool flag = false;
      if (this.ShellTabMessageCaptured != null)
        flag = this.ShellTabMessageCaptured(ref msg);
      return flag;
    }

    private bool addressBandRootSubclassProc(ref Message msg)
    {
      if (msg.Msg == 273)
      {
        int num1 = MCR.HIWORD(msg.WParam);
        int num2 = MCR.LOWORD(msg.WParam);
        if (num1 == 0 && num2 == 102)
          this.NotifyBrowserRefresh(false);
      }
      return false;
    }

    private unsafe bool rebarParentSubclassProc(ref Message msg)
    {
      if (msg.Msg == 78)
      {
        switch (((NMHDR*) (void*) msg.LParam)->code)
        {
          case -839:
            if (this.Toolbars.IsTabBarAvailable && !this.Toolbars.TabBar.IsBottomBar)
            {
              NMREBARCHILDSIZE* lparam = (NMREBARCHILDSIZE*) (void*) msg.LParam;
              if (this.Toolbars.TabBar.BandID == lparam->wID)
              {
                RECT rcBand = lparam->rcBand;
                RECT rcChild = lparam->rcChild;
                if (rcBand.Height > rcChild.Height)
                {
                  lparam->rcChild = new RECT()
                  {
                    left = rcChild.left,
                    top = rcBand.top + (rcBand.Height - rcChild.Height),
                    right = rcChild.right,
                    bottom = rcBand.bottom
                  };
                  break;
                }
                break;
              }
              break;
            }
            break;
          case -836:
            this.SaveCurrentBandLayout();
            if (this.Toolbars.IsTopTabBarVisible)
            {
              this.Toolbars.TabBar.RebarBeginDrag(false);
              break;
            }
            break;
          case -835:
            if (this.Toolbars.IsTopTabBarVisible)
            {
              this.Toolbars.TabBar.RebarBeginDrag(true);
              break;
            }
            break;
        }
      }
      return false;
    }

    private IntPtr GetPIDLFromIndex(int index)
    {
      IShellView ppshv = (IShellView) null;
      try
      {
        if (this.shellBrowser.QueryActiveShellView(out ppshv) == 0)
        {
          if (ppshv is IFolderView folderView)
          {
            IntPtr pidl = IntPtr.Zero;
            IntPtr ppidl = IntPtr.Zero;
            try
            {
              if (folderView.GetCurFolder(out pidl) == 0)
              {
                if (folderView.Item(index, out ppidl) == 0)
                  return PInvoke.ILCombine(pidl, ppidl);
              }
            }
            finally
            {
              if (pidl != IntPtr.Zero)
                PInvoke.CoTaskMemFree(pidl);
              if (ppidl != IntPtr.Zero)
                PInvoke.CoTaskMemFree(ppidl);
            }
          }
        }
      }
      finally
      {
        if (ppshv != null)
          Marshal.ReleaseComObject((object) ppshv);
      }
      return IntPtr.Zero;
    }

    private IntPtr GetPIDLFromMouseEvent(Message msg, out bool fMargin)
    {
      fMargin = false;
      Point pointLparam = MCR.GET_POINT_LPARAM(msg.LParam);
      PInvoke.MapWindowPoints(msg.HWnd, IntPtr.Zero, ref pointLparam, 1);
      QHitTestInfo hitTestInfo = new QHitTestInfo(msg.HWnd, this.shellBrowser.GetCurrentViewMode(), pointLparam, false, false, this.View.VistaLayout);
      if (QUIAutomation.HitTestOnView(hitTestInfo))
      {
        if (hitTestInfo.index != -1)
          return this.GetPIDLFromIndex(hitTestInfo.index);
        fMargin = true;
      }
      return IntPtr.Zero;
    }

    private void CustomizeListView()
    {
      this.View.OnViewCreated((IShellView) null);
      this.hwndListView = this.View.ListViewHandle;
    }

    private bool ModelessWindowProcessKey()
    {
      if (this.fileRenameDialog != null)
      {
        try
        {
          return this.fileRenameDialog.IsHandleCreated && this.fileRenameDialog.Handle == PInvoke.GetForegroundWindow();
        }
        catch
        {
        }
      }
      return false;
    }

    private bool IMEWindowProcessKey()
    {
      IntPtr focus = PInvoke.GetFocus();
      if (focus != IntPtr.Zero)
      {
        using (IMEDetector imeDetector = new IMEDetector(focus))
        {
          if (imeDetector.IsOpen)
            return true;
        }
      }
      return false;
    }

    private bool TrySetTreeColors(bool fFore, bool fBack)
    {
      if (!this.fTreeViewColorPending || !this.NavigationPane.Available)
        return false;
      this.NavigationPane.UpdateColors(fFore, fBack);
      this.fTreeViewColorPending = false;
      this.fTreeViewColorPendingOnStartUp = false;
      return true;
    }

    private unsafe void DispatchMouseMessage(QTTabBarLib.Interop.MSG* msg)
    {
      switch (msg->message)
      {
        case 512:
          NavigationPane navigationPane1 = (NavigationPane) null;
          if (((int) (long) msg->wParam & 1) != 0 && this.IsNavigationWindow(msg->hwnd, out navigationPane1))
          {
            this.OnFolderTreeMouseMove(*msg, navigationPane1);
            break;
          }
          CustomViewBase viewFromHwnd1 = this.GetViewFromHwnd(msg->hwnd);
          if (viewFromHwnd1 == null || !viewFromHwnd1.DispatchMouseMessage(msg))
            break;
          PInvoke.EatHookedMessage(msg);
          break;
        case 513:
        case 514:
        case 519:
        case 520:
          if (MenuUtility.InMenuLoop)
            break;
          NavigationPane navigationPane2;
          if (this.IsNavigationWindow(msg->hwnd, out navigationPane2))
          {
            if (!this.OnFolderTreeMouseEvent(*msg, navigationPane2))
              break;
            PInvoke.EatHookedMessage(msg);
            break;
          }
          if ((msg->message == 513 || msg->message == 519) && msg->hwnd == this.hwndBreadcrumbToolbarWindow)
          {
            if (!this.OnBreadcrumbsClick(*msg))
              break;
            PInvoke.EatHookedMessage(msg);
            break;
          }
          if (msg->message == 514 && QEnvironment.SingleClickMode && msg->hwnd == this.hwndListView)
          {
            if (this.View.SuppressMouseUpAfterDoubleClickOnSingleClickMode)
            {
              this.View.SuppressMouseUpAfterDoubleClickOnSingleClickMode = false;
              PInvoke.EatHookedMessage(msg);
              break;
            }
            this.View.StopSelectionChangeTimer();
            if (this.ExplorerManagerEvent != null)
              this.ExplorerManagerEvent((object) this, new ExplorerManagerEventArgs(ExplorerManagerEventKind.SingleClickOnListView, msg->ToMessage()));
            this.OnViewSingleClick(msg);
            break;
          }
          CustomViewBase viewFromHwnd2 = this.GetViewFromHwnd(msg->hwnd);
          if (viewFromHwnd2 == null || !viewFromHwnd2.DispatchMouseMessage(msg))
            break;
          PInvoke.EatHookedMessage(msg);
          break;
        case 515:
        case 516:
        case 517:
        case 522:
        case 523:
        case 524:
        case 673:
        case 675:
          CustomViewBase viewFromHwnd3 = this.GetViewFromHwnd(msg->hwnd);
          if (viewFromHwnd3 == null || !viewFromHwnd3.DispatchMouseMessage(msg))
            break;
          PInvoke.EatHookedMessage(msg);
          break;
      }
    }

    public void OnVerbInvoked(IList<ItemIDList> idls, string verb)
    {
      if (verb == "QTTabBar.openInView")
      {
        TargetView theOtherView = this.Toolbars.GetTheOtherView(TargetView.Default);
        if (this.Toolbars.IsExtraViewVisible(theOtherView))
        {
          this.Toolbars.GetExtraViewBar(theOtherView).OnViewItemUserEvent((ItemIDList) null, idls, UserEventOption.NewTab);
        }
        else
        {
          this.PendingFoldersExtraView = idls;
          this.Toolbars.ShowExtraView(theOtherView, true);
        }
      }
      else if (this.Toolbars.IsTabBarAvailable)
        this.Toolbars.TabBar.OnVerbInvoked(idls, verb);
      else if (idls.Count == 1)
      {
        this.InvokeCommand(new CommandInfo(BarCommand.OpenInPlace, (byte[]) idls[0]));
      }
      else
      {
        if (idls.Count <= 1)
          return;
        this.PendingFoldersDefault = idls;
        this.Toolbars.Show(Config.Bool(Scts.TabBarPriority) ? Toolbar.BottomTabBar : Toolbar.TabBar, true);
      }
    }

    public static bool TryMakeSubFolderTipPath(
      ref string path,
      ref byte[] idl,
      bool fTab,
      bool fExplicit,
      out bool fQTG,
      IntPtr hwndExplorer)
    {
      fQTG = false;
      if (string.IsNullOrEmpty(path) || ShellMethods.IsIDLNullOrEmpty(idl))
        return false;
      if (PathString.IsFloppyDrive(path) || PathString.IsGodModesRoot(path))
        return true;
      ShellItem shellItem1 = fExplicit || !PathString.IsNetwork(path) ? new ShellItem(idl, hwndExplorer, ResolveOption.UI, Config.ResolverLongTimeout) : new ShellItem(idl, IntPtr.Zero, ResolveOption.Silent, 200);
      if (PathString.IsNetworkServer(path))
        return shellItem1.Available;
      using (shellItem1)
      {
        if (!shellItem1.Available)
          return false;
        if (shellItem1.IsLink)
        {
          using (ShellItem shellItem2 = shellItem1.ResolveLink(IntPtr.Zero, ResolveOption.Silent, fExplicit ? 3000 : 150))
          {
            if (!shellItem2.Available)
              return false;
            if (shellItem2.IsFolder)
            {
              path = shellItem2.Path;
              idl = shellItem2.IDL;
              if (PathString.IsNetworkServer(path))
                return true;
              if (PathString.IsFloppyDrive(path))
                return false;
              if (PathString.IsNamespace(path) || string.Equals(PathString.GetExtension(path), ".library-ms", StringComparison.OrdinalIgnoreCase))
                return true;
              if (PathString.IsGodModesRoot(path))
                return true;
            }
            else
            {
              if (!shellItem2.HasPath || !string.Equals(PathString.GetExtension(shellItem2.Path), ".QTTabGroup", StringComparison.OrdinalIgnoreCase))
                return false;
              fQTG = true;
              return true;
            }
          }
        }
        else
        {
          if (!shellItem1.IsFolder)
          {
            if (string.IsNullOrEmpty(path) || !string.Equals(PathString.GetExtension(path), ".QTTabGroup", StringComparison.OrdinalIgnoreCase))
              return false;
            fQTG = true;
            return true;
          }
          if (!PathString.IsNamespace(path))
          {
            if (!string.Equals(PathString.GetExtension(path), ".library-ms", StringComparison.OrdinalIgnoreCase))
              goto label_38;
          }
          return true;
        }
      }
label_38:
      if (PathString.IsCompressedFileOrInCompressedFolder(path, true))
        return Config.Bool(Scts.SubDirTipAndMenuForCompressed);
      return fTab || SubDirTip.DirectoryHasSubItems(path, Config.Bool(Scts.SubDirTipMenuIncludeHiddens), Config.Bool(Scts.SubDirTipMenuIncludeSystems), Config.Bool(Scts.SubDirTipMenuIncludeFiles));
    }

    private void ScrollNotified(CustomViewBase view)
    {
      view.HideSubDirTip(fClearRct: true);
      view.HidePreviewTip();
    }

    internal void OnSubFolderMenuItemClicked(TipControlEventArgs tce)
    {
      SubFolderTipSource source = tce.Source;
      ItemClickedEventArgs innerEventArgs = (ItemClickedEventArgs) tce.InnerEventArgs;
      TargetView targetView = innerEventArgs.TargetView;
      int config = UserEvents.GetInteractiveValueFrom(innerEventArgs.Button, UserEvents.SubFolderMenuClick, UserEvents.SubFolderMenuMiddleClick);
      switch (config)
      {
        case 2:
          if (SubDirTip.SourceIsTabBar(source))
          {
            config = 0;
            if (targetView == TargetView.Default)
            {
              targetView = new ValueSelector((Enum) source, new Enum[3]
              {
                (Enum) SubFolderTipSource.TabBar,
                (Enum) SubFolderTipSource.SecondViewBarMenu,
                (Enum) SubFolderTipSource.ThirdViewBarMenu
              }).Select<TargetView>(TargetView.Default, TargetView.Second, TargetView.Third);
              break;
            }
            break;
          }
          if (SubDirTip.SourceIsCommandBar(source))
          {
            config = 0;
            break;
          }
          break;
        case 3:
        case 4:
        case 5:
          targetView = TargetView.Second;
          break;
        case 6:
        case 7:
        case 8:
          targetView = TargetView.Third;
          break;
        case 9:
        case 10:
        case 11:
          targetView = this.Toolbars.GetTheOtherView(targetView);
          break;
      }
      if (!innerEventArgs.MultipleItemClicked)
      {
        QMenuItem clickedItem = (QMenuItem) innerEventArgs.ClickedItem;
        if (clickedItem.Genre == MenuGenre.SubDirTip_QTGRootItem)
        {
          if (config == 17)
          {
            GroupManager.SendToClipboardPaths((IEnumerable<string>) innerEventArgs.ClickedItem.Name.AsArray<string>());
          }
          else
          {
            if (config == 13 || config == 14 || config == 15 || config == 16 || clickedItem.InTrashBin)
              return;
            BarCommandOptions barCommandOptions = ~BarCommandOptions.DoNothing;
            if (config == 1 || config == 4 || config == 7 || config == 10)
              barCommandOptions = BarCommandOptions.NoActivate;
            BarCommandOptions options = barCommandOptions | BarCommandOptions.IgnoreModKeys;
            CommandInfo info1 = new CommandInfo(BarCommand.FocusListView);
            info1.TargetView = targetView;
            this.InvokeCommand(info1);
            CommandInfo info2 = new CommandInfo(BarCommand.Group, innerEventArgs.ClickedItem.Name, options);
            info2.TargetView = targetView;
            this.InvokeCommand(info2);
          }
        }
        else if (clickedItem.Genre == MenuGenre.SubDirTip_QTG)
        {
          if (clickedItem.InTrashBin && config != 16)
            return;
          BarCommandOptions opt = BarCommandOptions.IgnoreModKeys;
          switch (config)
          {
            case 1:
            case 4:
            case 7:
            case 10:
              opt |= BarCommandOptions.NoActivate;
              break;
            case 12:
              targetView = innerEventArgs.TargetView;
              opt |= BarCommandOptions.Window;
              break;
            case 13:
            case 14:
              bool fCut = config == 14;
              using (ShellItem shellItem = new ShellItem(clickedItem.IDL))
              {
                shellItem.SetToClipboard(fCut, this.hwndExplorer);
                return;
              }
            case 15:
              return;
            case 16:
              ShellMethods.ShowProperties(clickedItem.IDL, this.hwndExplorer);
              return;
            case 17:
              using (ShellItem shellItem = new ShellItem(clickedItem.IDL))
              {
                ShellMethods.SetStringClipboard(shellItem.Path);
                return;
              }
          }
          QGroupOpener.OpenGroupFiles((IList<string>) clickedItem.Path.AsArray<string>(), this.hwndExplorer, targetView, opt);
        }
        else if (clickedItem.Target == MenuTarget.Folder)
        {
          if (clickedItem.InTrashBin && config != 16 && config != 17)
            return;
          switch (config)
          {
            case 0:
            case 3:
            case 6:
            case 9:
              CommandInfo info3 = new CommandInfo(BarCommand.FocusListView);
              info3.TargetView = targetView;
              this.InvokeCommand(info3);
              CommandInfo info4 = new CommandInfo(BarCommand.NewTab, clickedItem.IDL, BarCommandOptions.Activate | BarCommandOptions.IgnoreModKeys);
              info4.TargetView = targetView;
              this.InvokeCommand(info4);
              break;
            case 1:
            case 4:
            case 7:
            case 10:
              CommandInfo info5 = new CommandInfo(BarCommand.FocusListView);
              info5.TargetView = targetView;
              this.InvokeCommand(info5);
              CommandInfo info6 = new CommandInfo(BarCommand.NewTab, clickedItem.IDL, BarCommandOptions.NoActivate | BarCommandOptions.IgnoreModKeys);
              info6.TargetView = targetView;
              this.InvokeCommand(info6);
              break;
            case 2:
            case 5:
            case 8:
            case 11:
              if (config == 2 && SubDirTip.SourceIsTab(source))
              {
                if (this.SubFolderMenuEvents == null)
                  break;
                this.SubFolderMenuEvents((object) this, new ExplorerManagerEventArgs(SubFolderTipEventKind.MenuItemClickedOpenInPlace, source)
                {
                  IDL = clickedItem.IDL,
                  TargetPath = clickedItem.TargetPath,
                  TipControlEventArgs = tce
                });
                break;
              }
              CommandInfo info7 = new CommandInfo(BarCommand.FocusListView);
              info7.TargetView = targetView;
              this.InvokeCommand(info7);
              CommandInfo info8 = new CommandInfo(BarCommand.OpenInPlace, clickedItem.IDL);
              info8.TargetView = targetView;
              this.InvokeCommand(info8);
              break;
            case 12:
              this.InvokeCommand(new CommandInfo(BarCommand.NewWindow, clickedItem.IDL));
              break;
            case 13:
            case 14:
              bool fCut = config == 14;
              using (ShellItem shellItem = new ShellItem(clickedItem.IDL))
              {
                shellItem.SetToClipboard(fCut, this.hwndExplorer);
                break;
              }
            case 15:
              if (!ShellMethods.ClipboardContainsFileDropList())
                break;
              using (ShellItem shellItem = new ShellItem(clickedItem.IDL))
              {
                FileOperationSound.Play(OperationSound.Paste);
                shellItem.PasteInto();
                break;
              }
            case 16:
              ShellMethods.ShowProperties(clickedItem.IDL, this.hwndExplorer);
              break;
            case 17:
              using (ShellItem shellItem = new ShellItem(clickedItem.IDL))
              {
                ShellMethods.SetStringClipboard(shellItem.Path);
                break;
              }
          }
        }
        else
        {
          using (ShellItem shellItem = new ShellItem(clickedItem.IDL))
          {
            if (!shellItem.IsDeadLink(this.hwndExplorer, ResolveOption.UI))
            {
              SHELLEXECUTEINFO lpExecInfo = new SHELLEXECUTEINFO();
              lpExecInfo.cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(lpExecInfo);
              lpExecInfo.nShow = SHOWWINDOW.SHOWNORMAL;
              lpExecInfo.fMask = SEEMASK.IDLIST | SEEMASK.ASYNCOK;
              lpExecInfo.lpIDList = shellItem.PIDL;
              lpExecInfo.hwnd = this.hwndExplorer;
              if (!string.IsNullOrEmpty(clickedItem.Path))
              {
                string s = PathString.MakeDefaultWorkingDirectoryStr(clickedItem.Path);
                if (s.Length > 0)
                  lpExecInfo.lpDirectory = Marshal.StringToCoTaskMemUni(s);
              }
              try
              {
                if (!PInvoke.ShellExecuteEx(ref lpExecInfo))
                  return;
                RecentFileManager.Add((IEnumerable<string[]>) new string[1][]
                {
                  new string[1]{ clickedItem.Path }
                });
              }
              finally
              {
                if (lpExecInfo.lpDirectory != IntPtr.Zero)
                  Marshal.FreeCoTaskMem(lpExecInfo.lpDirectory);
              }
            }
            else
              ErrorDialog.Show(ErrorKind.DeadLink, this.hwndExplorer, "'" + clickedItem.Text + "'", "QTTabBar", shellItem.TargetPath);
          }
        }
      }
      else
        this.OnSubFolderMenuMultipleItemClicked(tce, config, targetView);
    }

    private void OnSubFolderMenuMultipleItemClicked(
      TipControlEventArgs tce,
      int config,
      TargetView targetView)
    {
      BarCommandOptions options = BarCommandOptions.IgnoreModKeys;
      switch (config)
      {
        case 1:
        case 4:
        case 7:
        case 10:
          options |= BarCommandOptions.NoActivate;
          break;
        case 12:
          if (tce.ExecutedIDLs.Count > 0)
          {
            ExplorerOpener.Perform(tce.ExecutedIDLs.ToItemIDList(), targetView, tce.ExecutedGroups);
            return;
          }
          if (tce.ExecutedGroups.Count <= 0)
            return;
          ExplorerOpener.Perform(tce.ExecutedGroups, targetView);
          return;
        case 13:
        case 14:
          List<string> lstPaths = new List<string>();
          foreach (byte[] executedIdL in tce.ExecutedIDLs)
          {
            using (ShellItem shellItem = new ShellItem(executedIdL, false))
            {
              if (shellItem.HasPath)
                lstPaths.Add(shellItem.Path);
            }
          }
          if (lstPaths.Count <= 0)
            return;
          ShellMethods.SetClipboardFileDropPaths(lstPaths, config == 14, this.hwndExplorer);
          return;
        case 15:
          ShellUtils.PasteInto((IEnumerable<ItemIDList>) tce.ExecutedIDLs.ToItemIDList(), this.hwndExplorer);
          return;
        case 16:
          return;
        case 17:
          string str = string.Empty;
          foreach (byte[] executedIdL in tce.ExecutedIDLs)
          {
            using (ShellItem shellItem = new ShellItem(executedIdL, false))
            {
              if (shellItem.HasPath)
                str = str + shellItem.Path + "\r\n";
            }
          }
          ShellMethods.SetStringClipboard(str + GroupManager.StringFromGroups((IEnumerable<string>) tce.ExecutedGroups));
          return;
      }
      if (tce.ExecutedGroups.Count > 0)
      {
        CommandInfo info1 = new CommandInfo(BarCommand.FocusListView);
        info1.TargetView = targetView;
        this.InvokeCommand(info1);
        CommandInfo info2 = new CommandInfo(BarCommand.Group, tce.ExecutedGroups.ToArray<string>(), options);
        info2.TargetView = targetView;
        this.InvokeCommand(info2);
      }
      if (tce.ExecutedIDLs.Count <= 0)
        return;
      CommandInfo info3 = new CommandInfo(BarCommand.FocusListView);
      info3.TargetView = targetView;
      this.InvokeCommand(info3);
      CommandInfo info4 = new CommandInfo(BarCommand.NewTabs, (IList<byte[]>) tce.ExecutedIDLs, options);
      info4.TargetView = targetView;
      this.InvokeCommand(info4);
    }

    private void OnSubFolderMenuItemRightClicked(TipControlEventArgs tce)
    {
      ItemRightClickedEventArgs innerEventArgs = (ItemRightClickedEventArgs) tce.InnerEventArgs;
      Point pnt = innerEventArgs.MakeRightClickedLocation();
      if (!innerEventArgs.MultipleItemClicked)
      {
        if (!(innerEventArgs.ClickedItem is QMenuItem clickedItem))
          return;
        using (ShellItem shellItem = new ShellItem(clickedItem.IDL, false))
        {
          if (!shellItem.Available)
            return;
          switch (ShellMethods.PopUpContextMenu(shellItem, pnt, ref innerEventArgs.MessageHandlerObject.ContextMenu2, innerEventArgs.MessageHandlerObject.Hwnd, clickedItem.InTrashBin ? (PopupMenuCommand[]) null : SubDirTip.GetContextMenuCommands(TargetView.Default)))
          {
            case PopupMenuCommand.OpenInPlace:
              CommandInfo info1 = new CommandInfo(BarCommand.FocusListView);
              info1.TargetView = tce.TargetView;
              this.InvokeCommand(info1);
              CommandInfo info2 = new CommandInfo(BarCommand.OpenInPlace, shellItem.IDL);
              info2.TargetView = tce.TargetView;
              this.InvokeCommand(info2);
              innerEventArgs.PostProcess = RightClickedPostProcess.CloseAll;
              break;
            case PopupMenuCommand.OpenParent:
              using (ShellItem parent = shellItem.Parent)
              {
                CommandInfo info3 = new CommandInfo(BarCommand.FocusListView);
                info3.TargetView = tce.TargetView;
                this.InvokeCommand(info3);
                CommandInfo info4 = new CommandInfo(BarCommand.NewTab, parent.IDL);
                info4.TargetView = tce.TargetView;
                this.InvokeCommand(info4);
              }
              innerEventArgs.PostProcess = RightClickedPostProcess.CloseAll;
              break;
            case PopupMenuCommand.RenameItem:
              if (!(clickedItem.Owner is DropDownMenuEx owner))
                break;
              owner.RenameItem(clickedItem);
              break;
            case PopupMenuCommand.OpenInNewTab2nd:
              CommandInfo info5 = new CommandInfo(BarCommand.NewTab, shellItem.IDL);
              info5.TargetView = TargetView.Second;
              this.InvokeCommand(info5);
              innerEventArgs.PostProcess = RightClickedPostProcess.CloseAll;
              break;
            case PopupMenuCommand.OpenInNewTab3rd:
              CommandInfo info6 = new CommandInfo(BarCommand.NewTab, shellItem.IDL);
              info6.TargetView = TargetView.Third;
              this.InvokeCommand(info6);
              innerEventArgs.PostProcess = RightClickedPostProcess.CloseAll;
              break;
            case PopupMenuCommand.ShellContextMenuInvoked:
              innerEventArgs.PostProcess = RightClickedPostProcess.CloseAll;
              break;
          }
        }
      }
      else
      {
        if (PopupMenuCommand.ShellContextMenuInvoked != ShellMethods.PopUpContextMenu(tce.ExecutedIDLs, pnt, ref innerEventArgs.MessageHandlerObject.ContextMenu2, innerEventArgs.MessageHandlerObject.Hwnd))
          return;
        innerEventArgs.PostProcess = RightClickedPostProcess.CloseAll;
      }
    }

    private void OnSubFolderMenuClosed(SubFolderTipSource source)
    {
      if (this.SubFolderMenuEvents == null)
        return;
      this.SubFolderMenuEvents((object) this, new ExplorerManagerEventArgs(SubFolderTipEventKind.MenuClosed, source));
    }

    private void OnSubFolderMenuMouseLeaving(SubFolderTipSource source)
    {
      if (this.SubFolderMenuEvents == null)
        return;
      this.SubFolderMenuEvents((object) this, new ExplorerManagerEventArgs(SubFolderTipEventKind.MouseLeaving, source));
    }

    private void OnSubFolderMenuMouseWheelEx(SubFolderTipSource source, MouseEventArgs e)
    {
      if (this.SubFolderMenuEvents == null)
        return;
      this.SubFolderMenuEvents((object) this, new ExplorerManagerEventArgs(SubFolderTipEventKind.MouseWheelEx, source)
      {
        MouseEventArgs = e
      });
    }

    private void View_NavigationComplete(object sender, ExplorerBrowserNavigationEventArgs ebne)
    {
      try
      {
        ShellItem shellItem = ebne.ShellItem;
        string url = ebne.URL;
        this.currentIDL = shellItem.IDL;
        if (!this.fFirstNavigationComplete)
        {
          if (ExplorerOpener.IsDirty)
            this.ProcessStartupsFallback();
          try
          {
            if (Config.Bool(Scts.SaveExplorerTopMost))
            {
              if (Config.Bool(Scts.ExplorerTopMost))
                this.InvokeCommand(BarCommand.ToggleTopmost);
            }
          }
          catch (Exception ex)
          {
            DebugUtil.AppendToCriticalExceptionLog(ex);
          }
        }
        MenuUtility.ExitMenuMode();
        this.CustomizeListView();
        try
        {
          if (this.shellFolderView != null)
          {
            // ISSUE: method pointer
            // ISSUE: object of a compiler-generated type is created
            new ComAwareEventInfo(typeof (DShellFolderViewEvents_Event), "SelectionChanged").RemoveEventHandler((object) this.shellFolderView, (Delegate) new DShellFolderViewEvents_SelectionChangedEventHandler((object) this, (UIntPtr) __methodptr(shellFolderView_SelectionChanged)));
            // ISSUE: method pointer
            // ISSUE: object of a compiler-generated type is created
            new ComAwareEventInfo(typeof (DShellFolderViewEvents_Event), "BeginDrag").RemoveEventHandler((object) this.shellFolderView, (Delegate) new DShellFolderViewEvents_BeginDragEventHandler((object) this, (UIntPtr) __methodptr(shellFolderView_BeginDrag)));
            // ISSUE: method pointer
            // ISSUE: object of a compiler-generated type is created
            new ComAwareEventInfo(typeof (DShellFolderViewEvents_Event), "EnumDone").RemoveEventHandler((object) this.shellFolderView, (Delegate) new DShellFolderViewEvents_EnumDoneEventHandler((object) this, (UIntPtr) __methodptr(shellFolderView_EnumDone)));
            Marshal.FinalReleaseComObject((object) this.shellFolderView);
            this.shellFolderView = (ShellFolderView) null;
          }
          try
          {
            this.shellFolderView = this.Explorer.Document as ShellFolderView;
          }
          catch (NotImplementedException ex)
          {
          }
          if (this.shellFolderView != null)
          {
            // ISSUE: method pointer
            // ISSUE: object of a compiler-generated type is created
            new ComAwareEventInfo(typeof (DShellFolderViewEvents_Event), "SelectionChanged").AddEventHandler((object) this.shellFolderView, (Delegate) new DShellFolderViewEvents_SelectionChangedEventHandler((object) this, (UIntPtr) __methodptr(shellFolderView_SelectionChanged)));
            // ISSUE: method pointer
            // ISSUE: object of a compiler-generated type is created
            new ComAwareEventInfo(typeof (DShellFolderViewEvents_Event), "BeginDrag").AddEventHandler((object) this.shellFolderView, (Delegate) new DShellFolderViewEvents_BeginDragEventHandler((object) this, (UIntPtr) __methodptr(shellFolderView_BeginDrag)));
            // ISSUE: method pointer
            // ISSUE: object of a compiler-generated type is created
            new ComAwareEventInfo(typeof (DShellFolderViewEvents_Event), "EnumDone").AddEventHandler((object) this.shellFolderView, (Delegate) new DShellFolderViewEvents_EnumDoneEventHandler((object) this, (UIntPtr) __methodptr(shellFolderView_EnumDone)));
            if (this.View.VistaLayout)
            {
              if (Config.Bool(Scts.AlwaysColumnHeader))
                this.shellFolderView.FolderFlags &= 4269801471U;
            }
          }
        }
        catch (Exception ex)
        {
          DebugUtil.AppendToCriticalExceptionLog(ex);
        }
        bool flag = false;
        try
        {
          if (this.NavigateComplete != null)
          {
            ExplorerManagerEventArgs e = new ExplorerManagerEventArgs(url, this.currentIDL);
            this.NavigateComplete((object) this, e);
            flag = e.SuppressUserNavCompEvent;
          }
          if (!this.fNowInitializingInstallation)
            this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.NavigationComplete, 0, this.currentIDL, url));
        }
        catch (Exception ex)
        {
        }
        if (this.fFirstNavigationComplete)
        {
          try
          {
            this.NotifyBrowserRefresh(true);
          }
          catch (Exception ex)
          {
          }
          this.NotifyNavigationToFileRenamer(this.currentIDL, TargetView.Default);
          try
          {
            if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
              this.ViewSyncManager.OnNavigationComplete((CustomViewBase) this.View, shellItem);
          }
          catch (Exception ex)
          {
          }
          try
          {
            if (Config.Bool(Scts.SaveSettingsAuto))
            {
              if (!this.Initializing)
                SettingSaver.Save();
            }
          }
          catch (Exception ex)
          {
          }
        }
        try
        {
          if (!this.fFirstNavigationComplete)
            this.SetFocusOnOpening();
        }
        catch (Exception ex)
        {
        }
        try
        {
          switch (ExplorerProcessCaptor.ReadSelectionView())
          {
            case TargetView.Invalid:
            case TargetView.Default:
              string path = ExplorerProcessCaptor.ReadSelection();
              if (!string.IsNullOrEmpty(path))
              {
                this.TrySetSelection(new Address(path).AsArray<Address>(), (string) null, true);
                break;
              }
              break;
          }
        }
        catch (Exception ex)
        {
        }
        try
        {
          if (!this.Toolbars.IsTabBarAvailable)
          {
            if (!this.fFirstNavigationComplete && (Config.Get(Scts.SaveRecentTabWhen) & 1) != 0)
              RecentTabManager.AddOnOpen((IEnumerable<LogData>) new LogData[1]
              {
                new LogData(this.currentIDL)
              });
            else
              RecentTabManager.AddOnNavigation(new LogData(this.currentIDL));
          }
        }
        catch (Exception ex)
        {
          DebugUtil.AppendToCriticalExceptionLog(ex);
        }
        try
        {
          if (this.navPane != null)
          {
            this.fTreeViewColorPending = true;
            this.navPane.InitializeHandle();
          }
          bool fFore = Config.Bool(Scts.FolderTreeForeColorEnabled);
          bool fBack = Config.Bool(Scts.FolderTreeBackColorEnabled);
          if (fBack | fFore)
          {
            if (!this.TrySetTreeColors(fFore, fBack))
            {
              if (!this.fFirstNavigationComplete)
                this.fTreeViewColorPendingOnStartUp = true;
            }
          }
        }
        catch (Exception ex)
        {
          DebugUtil.AppendToCriticalExceptionLog(ex);
        }
        if (!this.fFirstNavigationComplete)
        {
          try
          {
            if (Config.Bool(Scts.CheckUpdate))
              UpdateChecker.Check(false, Config.Bool(Scts.CheckUpdateNoBeta));
          }
          catch
          {
          }
        }
        try
        {
          if (!flag)
          {
            if (UserEvents.NavigationComplete != 0)
              UserEvents.Fire(UserEvents.NavigationComplete, (ICommandInvokerWindow) this, (object) url, EventSource.NavigationComplete);
          }
        }
        catch
        {
        }
        if (this.Toolbars.IsTabBarAvailable)
          return;
        this.SyncTabBarAndCommandBars(BuiltinButtonRefreshes.DisableGoUpOneLevel, BuiltinButtonRefreshSource.Default);
      }
      finally
      {
        this.fFirstNavigationComplete = true;
        this.PendingTabDefaultView = (QTabItem) null;
        this.PendingTabExtraView = (QTabItem) null;
        this.PendingModifyTabExtraView2nd = (QTabItem) null;
        this.PendingModifyTabExtraView3rd = (QTabItem) null;
      }
    }

    private void View_NavigationFailed(object sender, ExplorerBrowserNavigationEventArgs e)
    {
      if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
        this.ViewSyncManager.OnNavigationFailed((CustomViewBase) this.View);
      if (this.NavigateError != null)
        this.NavigateError((object) this, new ExplorerManagerEventArgs(e.URL, (byte[]) null)
        {
          StatusCode = e.StatusCode
        });
      try
      {
        if (UserEvents.NavigationFailed == 0)
          return;
        UserEvents.Fire(UserEvents.NavigationFailed, (ICommandInvokerWindow) this, (object) e.URL, EventSource.NavigationFailed);
      }
      catch
      {
      }
    }

    private void shellFolderView_SelectionChanged() { this.OnSelectionChanged();

    private bool shellFolderView_BeginDrag()
    {
      this.View.OnBeginDrag();
      return true;
    }

    private void shellFolderView_EnumDone()
    {
      ExplorerManager.StartUpProcessComplete = true;
      if (!this.fFirstDpiChangeNotified)
      {
        this.fFirstDpiChangeNotified = true;
        this.NotifyDpiChange(DpiManager.DefaultDpi);
      }
      if (this.fFirstEnumDone)
        return;
      this.fFirstEnumDone = true;
      try
      {
        if (UserEvents.WindowOpened == 0)
          return;
        UserEvents.Fire(UserEvents.WindowOpened, (ICommandInvokerWindow) this, (object) this.explorer.LocationURL, EventSource.WindowOpened);
      }
      catch
      {
      }
    }

    private void Application_EnterThreadModal(object sender, EventArgs e) { this.InModal = true;

    private void Application_LeaveThreadModal(object sender, EventArgs e) { this.InModal = false;

    private void contextMenFolderBand_Opening(object sender, CancelEventArgs e)
    {
      this.contextMenFolderBand.Alpha = Config.MenuOpacity;
      this.OnToolbarMenuOpening(this.contextMenFolderBand.Items);
    }

    private void contextMenFolderBand_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { this.OnToolbarMenuItemClicked(e.ClickedItem);

    public void OnToolbarMenuOpening(ToolStripItemCollection items)
    {
      foreach (ToolStripItem toolStripItem in (ArrangedElementCollection) items)
      {
        Toolbar result;
        if (toolStripItem is ToolStripMenuItem toolStripMenuItem && Enum.TryParse<Toolbar>(toolStripMenuItem.Name, out result))
        {
          switch (result)
          {
            case Toolbar.TabBar:
              toolStripMenuItem.CheckState = this.Toolbars.IsTopTabBarVisible ? CheckState.Indeterminate : CheckState.Unchecked;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[110]);
              continue;
            case Toolbar.CommandBar1:
              toolStripMenuItem.Checked = this.Toolbars.IsCommandBar1stVisible;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[74]);
              continue;
            case Toolbar.CommandBar2:
              toolStripMenuItem.Checked = this.Toolbars.IsCommandBar2ndVisible;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[75]);
              continue;
            case Toolbar.CommandBarVertical:
              toolStripMenuItem.Checked = this.VerticalBarShrunken || this.Toolbars.IsCommandBarVrtVisible || this.Toolbars.IsVerticalCommandBarEnabledInThirdView;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[76]);
              continue;
            case Toolbar.BottomTabBar:
              toolStripMenuItem.CheckState = this.Toolbars.IsBottomTabBarVisible ? CheckState.Indeterminate : CheckState.Unchecked;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[111]);
              continue;
            case Toolbar.SecondViewBar:
              toolStripMenuItem.Checked = this.Toolbars.IsSecondViewBarVisible;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[138]);
              continue;
            case Toolbar.ThirdViewBar:
              toolStripMenuItem.Checked = this.Toolbars.IsThirdViewBarVisible;
              toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[139]);
              continue;
            default:
              continue;
          }
        }
        else if (toolStripItem.Name == "buttonBarOption")
        {
          toolStripMenuItem.Enabled = this.Toolbars.IsCommandBar1stVisible;
          toolStripMenuItem.ShortcutKeyDisplayString = KeyShortcuts.ToMenuString(Config.ShortcutKeys[102]);
        }
      }
    }

    public void OnToolbarMenuItemClicked(ToolStripItem clickedItem)
    {
      Toolbar result;
      if (clickedItem is ToolStripMenuItem toolStripMenuItem && Enum.TryParse<Toolbar>(toolStripMenuItem.Name, out result))
      {
        if (result == Toolbar.TabBar || result == Toolbar.BottomTabBar)
          Config.Set(Scts.ExtraViewVisibleMode2nd, this.Toolbars.IsSecondViewBarVisible ? 1 : 0);
        switch (result)
        {
          case Toolbar.TabBar:
          case Toolbar.CommandBar1:
          case Toolbar.CommandBar2:
            this.Toolbars.Show(result, !toolStripMenuItem.Checked, true);
            break;
          case Toolbar.CommandBarVertical:
            if (this.VerticalBarShrunken)
            {
              Config.Set(Scts.CommandBar_VerticalEnabled, false);
              Config.Sync();
              this.VerticalBarShrunken = false;
              this.Toolbars.CommandBarVrtHandle = IntPtr.Zero;
              break;
            }
            this.Toolbars.Show(result, !toolStripMenuItem.Checked);
            break;
          case Toolbar.BottomTabBar:
            this.Toolbars.Show(result, !toolStripMenuItem.Checked);
            break;
          case Toolbar.SecondViewBar:
            this.Toolbars.ShowSecondView(!toolStripMenuItem.Checked);
            break;
          case Toolbar.ThirdViewBar:
            this.Toolbars.ShowThirdView(!toolStripMenuItem.Checked);
            break;
        }
      }
      else if (clickedItem.Name == "optionItem")
      {
        this.OpenOptionDialog();
      }
      else
      {
        if (!(clickedItem.Name == "buttonBarOption"))
          return;
        this.InvokeCommand(new CommandInfo(BarCommand.GlobalOption, 13));
      }
    }

    public bool InvokeCommand(CommandInfo info)
    {
      if (this.InvokeRequired)
        throw new InvalidOperationException("do not call from other thread than UI");
      CustomViewBase view = this.GetView(info.TargetView);
      bool flag1 = ExplorerManager.CommandWantView(info);
      if (info.Command > BarCommand.MENUCOMMAND_FIRST)
      {
        view?.InvokeMenuCommand((ExplorerMenuCommand) info.Command);
        return true;
      }
      switch (info.Command)
      {
        case BarCommand.NavigateTabTo:
        case BarCommand.DuplicateTabByID:
        case BarCommand.LockTabByID:
        case BarCommand.IsTabLocked:
        case BarCommand.CloseTabByID:
        case BarCommand.CloseAllButOneByID:
        case BarCommand.CreateNewGroupFromTab:
        case BarCommand.CreateNewLibraryFromTab:
        case BarCommand.DeleteTab:
        case BarCommand.ClipboardTab:
        case BarCommand.CopyToTab:
        case BarCommand.MoveToTab:
        case BarCommand.CreateLinkInTab:
        case BarCommand.RenameTab:
        case BarCommand.GoBackTab:
        case BarCommand.GoForwardTab:
        case BarCommand.GetTabViewItemCount:
          if (info.Int == -2)
          {
            switch (info.Command)
            {
              case BarCommand.NavigateTabTo:
                using (ShellItem shellItem = new ShellItem(info.IDL, false))
                {
                  if (shellItem.Available)
                  {
                    this.InvokeCommand(new CommandInfo(BarCommand.OpenInPlace, shellItem.IDL));
                    info.ReturnBoolean = true;
                    break;
                  }
                  break;
                }
              case BarCommand.CloseTabByID:
                this.InvokeCommand(BarCommand.CloseWindow);
                break;
              case BarCommand.CreateNewGroupFromTab:
                using (ShellItem shellItem = this.View.ShellItem)
                {
                  info.ReturnString = NewGroupOrLibraryForm.Create(info.Str ?? shellItem.DisplayName, (IEnumerable<byte[]>) new List<byte[]>()
                  {
                    this.currentIDL
                  });
                  break;
                }
              case BarCommand.CreateNewLibraryFromTab:
                string str = info.Str;
                using (ShellItem shellItem = this.View.ShellItem)
                {
                  if (!shellItem.IsFileSystem)
                  {
                    SystemSounds.Beep.Play2();
                    return true;
                  }
                  if (string.IsNullOrEmpty(str))
                    str = shellItem.DisplayName;
                }
                string name = PathString.SanitizeNameString(PathString.LimitLength(str, (int) byte.MaxValue));
                if (name.Length > 0)
                {
                  List<byte[]> idls = new List<byte[]>()
                  {
                    this.currentIDL
                  };
                  using (Library library = new Library(name, FileMode.CreateNew))
                  {
                    library.Add((IEnumerable<byte[]>) idls);
                    info.ReturnString = library.Name;
                    break;
                  }
                }
                else
                  break;
              case BarCommand.RenameTab:
                ShellMethods.PromptRename(this.currentIDL, this.hwndExplorer, StringResources.Dialogs._Dialog[2]);
                break;
              case BarCommand.GoBackTab:
                this.InvokeCommand(BarCommand.GoBack);
                info.ReturnBoolean = true;
                break;
              case BarCommand.GoForwardTab:
                this.InvokeCommand(BarCommand.GoForward);
                info.ReturnBoolean = true;
                break;
            }
            return true;
          }
          if (info.Int == -1)
          {
            if (view == null)
              return true;
            info.TargetView = view.TargetView;
            break;
          }
          bool flag2 = false;
          foreach (TabBarBase tabBar in this.Toolbars.TabBars)
          {
            if (tabBar.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == info.Int)) != null)
            {
              flag2 = true;
              info.TargetView = tabBar.TargetView;
              break;
            }
          }
          if (!flag2)
          {
            if (info.Scripting)
              throw new InvalidOperationException("The tab was not found in any tabbar.");
            return true;
          }
          break;
        case BarCommand.BrowseFolder:
          info.ReturnInt32 = this.BrowseFolder(info.IDL, info.TargetView, info.Options.HasFlag((Enum) BarCommandOptions.NoActivate));
          return true;
        case BarCommand.ActivateTabByID:
          foreach (TabBarBase tabBar in this.Toolbars.TabBars)
          {
            QTabItem qtabItem = tabBar.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == info.Int));
            if (qtabItem != null)
            {
              qtabItem.Activate();
              info.ReturnInt32 = qtabItem.ID;
              return true;
            }
          }
          return true;
        case BarCommand.OpenSearchFolder:
          using (ShellItem shellItem = new ShellItem(COMGUIDS.FOLDERID_SearchHome))
          {
            if (shellItem.Available)
            {
              CommandInfo info1 = new CommandInfo(info.Options.HasFlag((Enum) BarCommandOptions.Window) ? BarCommand.NewWindow : BarCommand.NewTab, shellItem.IDL);
              info1.TargetView = info.TargetView;
              int num = this.InvokeCommand(info1) ? 1 : 0;
              info.ReturnInt32 = info1.ReturnInt32;
              return num != 0;
            }
            break;
          }
        case BarCommand.CreateNewGroup:
          if (info.Int == 65540)
          {
            TabBarBase focusedTabBar = this.GetFocusedTabBar();
            if (focusedTabBar == null)
              NewGroupOrLibraryForm.Create(this.Explorer.LocationName, (IEnumerable<byte[]>) this.currentIDL.AsArray<byte[]>());
            else
              NewGroupOrLibraryForm.Create(focusedTabBar.TabManager.ActiveTab.Text, focusedTabBar.TabManager.ActiveTab, (IEnumerable<QTabItem>) null);
          }
          else if (info.Int == 65541)
            NewGroupOrLibraryForm.Create("New group", (QTabItem) null, this.Toolbars.TabBars.SelectMany<TabBarBase, QTabItem>((Func<TabBarBase, IEnumerable<QTabItem>>) (tb { tb.TabManager.Items)));
          else
            info.ReturnString = NewGroupOrLibraryForm.CreateNew(this.hwndExplorer);
          return true;
        case BarCommand.CreateNewGroupFromPath:
          if (CommandInvoker.InvokeNeutral((CommandInfoBase) info, this.hwndExplorer))
            return true;
          break;
        case BarCommand.CreateNewLibraryFromPath:
          if (info.IDLs != null && info.IDLs.Count > 0)
          {
            string str = info.Str;
            if (string.IsNullOrWhiteSpace(str))
            {
              using (NewGroupOrLibraryForm newLibraryForm = NewGroupOrLibraryForm.CreateNewLibraryForm(string.Empty, false))
              {
                if (DialogResult.OK != newLibraryForm.ShowDialog())
                  return true;
                str = newLibraryForm.LibraryName;
              }
            }
            else
              str = PathString.SanitizeNameString(PathString.LimitLength(str, (int) byte.MaxValue));
            if (!string.IsNullOrWhiteSpace(str))
            {
              List<byte[]> idls = new List<byte[]>();
              foreach (byte[] idL in info.IDLs)
              {
                using (ShellItem shellItem = new ShellItem(idL))
                {
                  if (shellItem.IsFileSystem)
                    idls.Add(idL);
                }
              }
              if (idls.Count > 0)
              {
                using (Library library = new Library(str, info.Bool ? FileMode.Append : FileMode.CreateNew))
                {
                  library.Add((IEnumerable<byte[]>) idls);
                  info.ReturnString = library.Name;
                }
              }
            }
          }
          return true;
        case BarCommand.CreateNewApplicationLauncher:
        case BarCommand.GetApplicationLauncher:
        case BarCommand.DeleteApplicationLauncher:
        case BarCommand.ClipboardPath:
        case BarCommand.ClipboardName:
        case BarCommand.SetFolderOptions:
        case BarCommand.GetFolderOptions:
        case BarCommand.ShowKeyFinder:
        case BarCommand.Alert:
        case BarCommand.MessageBox:
        case BarCommand.InputBox:
        case BarCommand.PlaySound:
        case BarCommand.RefreshIconCache:
        case BarCommand.RefreshFolderSettings:
        case BarCommand.DeleteMRU:
        case BarCommand.RenameItem:
        case BarCommand.OpenFileDialog:
        case BarCommand.SaveFileDialog:
        case BarCommand.FolderBrowserDialog:
        case BarCommand.RenameMultiple:
        case BarCommand.Export:
        case BarCommand.ShowMessageForm:
        case BarCommand.GetClipboardString:
        case BarCommand.SetClipboardString:
          CommandInvoker.InvokeNeutral((CommandInfoBase) info, this.hwndExplorer);
          return true;
        case BarCommand.LaunchApplication:
          try
          {
            if (!string.IsNullOrEmpty(info.Str))
            {
              if (view != null)
                AppLauncher.Execute(info.Str, this.hwndExplorer, new TokenReplacer(new Func<IList<ShellItem>>(view.GetSelection), new Func<IntPtr>(view.GetCurrentFolder)));
            }
          }
          catch
          {
          }
          return true;
        case BarCommand.LaunchApplicationByMia:
          try
          {
            if (view != null)
            {
              TokenReplacer tokenReplacer = new TokenReplacer(new Func<IList<ShellItem>>(view.GetSelection), new Func<IntPtr>(view.GetCurrentFolder));
              if (info.Mia != null)
                AppLauncher.Execute(info.Mia, this.hwndExplorer, tokenReplacer);
              else if (info.Strs != null)
              {
                if (info.Strs.Length == 3)
                  AppLauncher.Execute(new MenuItemArguments(info.Strs[0], info.Strs[1], info.Strs[2], 0, MenuGenre.None), this.hwndExplorer, tokenReplacer);
              }
            }
          }
          catch (Exception ex)
          {
          }
          return true;
        case BarCommand.LaunchApplicationByIDL:
          try
          {
            if (!ShellMethods.IsIDLNullOrEmpty(info.IDL))
            {
              string str1 = (string) null;
              string str2 = (string) null;
              if (info.Strs != null)
              {
                if (info.Strs.Length != 0)
                  str1 = info.Strs[0];
                if (info.Strs.Length > 1)
                  str2 = PathString.Unquote(info.Strs[1]);
              }
              using (ShellItem shellItem = new ShellItem(info.IDL, false))
              {
                if (AppLauncher.Execute(shellItem, this.hwndExplorer, str1, str2, info.Str, (SHOWWINDOW) info.Int))
                {
                  if (shellItem.HasPath)
                    RecentFileManager.Add((IEnumerable<string[]>) new string[5]
                    {
                      shellItem.Path,
                      str1.Ensure(),
                      str2.Ensure(),
                      string.Empty,
                      DateTime.Now.Ticks.ToString()
                    }.AsList<string[]>());
                }
              }
            }
          }
          catch (Exception ex)
          {
          }
          return true;
        case BarCommand.DeleteSelectedFiles:
          this.DeleteSelection(false, info.Bool || info.Options.HasFlag((Enum) BarCommandOptions.Nuke), info.TargetView);
          return true;
        case BarCommand.SetTopmost:
          this.SetTopMost(!info.Bool);
          this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco { cco.RefreshBarState(!info.Bool ? BuiltinButtonRefreshes.Topmost : (BuiltinButtonRefreshes) 0, BuiltinButtonRefreshes.Topmost)));
          return true;
        case BarCommand.ToggleTopmost:
          bool fTop = this.ToggleTopMost();
          this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco { cco.RefreshBarState(fTop ? BuiltinButtonRefreshes.Topmost : (BuiltinButtonRefreshes) 0, BuiltinButtonRefreshes.Topmost)));
          info.ReturnBoolean = fTop;
          return true;
        case BarCommand.IsTopmost:
          info.ReturnBoolean = this.IsTopMost;
          return true;
        case BarCommand.MinimizeToTasktray:
          if (info.Bool)
            this.RestoreFromTray();
          else
            this.SendToTasktray();
          return true;
        case BarCommand.RestoreFromTasktray:
          this.RestoreFromTray();
          return true;
        case BarCommand.IsWindowInTasktray:
          info.ReturnBoolean = this.IsInTray();
          return true;
        case BarCommand.MaximizeHorizontaly:
          this.MaximizeHorizontally((Screen) null);
          return true;
        case BarCommand.Maximize:
          if (this.IsInTray())
            this.RestoreFromTray();
          this.ShowWindow(SHOWWINDOW.SHOWMAXIMIZED);
          return true;
        case BarCommand.Minimize:
          if (!this.IsInTray())
            this.ShowWindow(SHOWWINDOW.MINIMIZE);
          return true;
        case BarCommand.WindowState:
          info.ReturnInt32 = (int) this.WindowState;
          return true;
        case BarCommand.Restore:
          if (this.IsInTray())
            this.RestoreFromTray();
          this.ShowWindow(SHOWWINDOW.RESTORE);
          return true;
        case BarCommand.Transparency:
          this.SetWindowTransparency(info.Int >= 0);
          return true;
        case BarCommand.TransparencyTo:
          byte alpha = WindowUtil.SetTransparency(this.hwndExplorer, (byte) info.Int);
          this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco {
          {
            if (!(cco is BandObject bandObject2))
              return;
            PInvoke.SendMessage(bandObject2.Handle, 32770, (IntPtr) (int) alpha, IntPtr.Zero);
          }));
          return true;
        case BarCommand.SetFocusTo:
          if (Enum.IsDefined(typeof (FocusableWindow), (object) info.Int))
            this.SetFocusTo((FocusableWindow) info.Int);
          return true;
        case BarCommand.FocusListView:
          this.FocusListView(info.TargetView);
          return true;
        case BarCommand.SetExtraviewSize:
          if (info.Ints != null && info.Ints.Length > 2)
          {
            int num = info.Ints[0];
            bool flag3 = info.Ints[1] == 0;
            bool flag4 = info.Ints[2] == 0;
            QSecondViewBar qsecondViewBar = flag3 ? (this.Toolbars.IsThirdViewBarVisible ? (QSecondViewBar) this.Toolbars.ThirdViewBar : (QSecondViewBar) null) : (this.Toolbars.IsSecondViewBarVisible ? this.Toolbars.SecondViewBar : (QSecondViewBar) null);
            if (qsecondViewBar != null)
            {
              int val2 = num;
              if (!flag4)
                val2 = QMath.Round(flag3 ? (float) (this.Bounds.Width * num) / 100f : (float) (this.Bounds.Height * num) / 100f);
              qsecondViewBar.BaseBarSize = (int) Math.Min((float) Math.Max(64, val2), (float) ((flag3 ? (double) this.Bounds.Width : (double) this.Bounds.Height) * 0.699999988079071));
            }
          }
          return true;
        case BarCommand.GetSystemDpi:
          info.ReturnInt32 = DpiManager.DefaultDpi;
          return true;
        case BarCommand.GetWindowDpi:
          if (info.Object != null && info.Object is IntPtr)
          {
            ExplorerManager explorerManager = InstanceManager.ExplorerManagerFromHandle((IntPtr) info.Object);
            if (explorerManager != null)
              info.ReturnInt32 = explorerManager.Dpi;
          }
          else
            info.ReturnInt32 = this.Dpi;
          return true;
        case BarCommand.GetDpiFromPoint:
          if (info.Ints != null && info.Ints.Length > 1)
            info.ReturnInt32 = DpiManager.GetDpiFromPoint(new Point(info.Ints[0], info.Ints[1]));
          return true;
        case BarCommand.GetWindowText:
          info.ReturnString = this.explorer.LocationName;
          return true;
        case BarCommand.GetCurrentPath:
          if (view != null)
          {
            using (ShellItem shellItem = view.ShellItem)
              info.ReturnString = shellItem.Path;
          }
          return true;
        case BarCommand.GetTabText:
        case BarCommand.GetTabPath:
          if (info.Int == -2)
          {
            using (ShellItem shellItem = this.View.ShellItem)
              info.ReturnString = info.Command == BarCommand.GetTabText ? shellItem.DisplayName : shellItem.Path;
          }
          else
          {
            foreach (TabBarBase tabBar in this.Toolbars.TabBars)
            {
              QTabItem qtabItem = tabBar.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == info.Int));
              if (qtabItem != null)
              {
                info.ReturnString = info.Command == BarCommand.GetTabText ? qtabItem.Text : qtabItem.CurrentPath;
                return true;
              }
            }
            info.ReturnString = (string) null;
          }
          return true;
        case BarCommand.ShowToolbar:
          if (Enum.IsDefined(typeof (Toolbar), (object) info.Int))
          {
            if (info.Int == 8)
              this.Toolbars.ShowSecondView(info.Bool);
            else if (info.Int == 9)
              this.Toolbars.ShowThirdView(info.Bool);
            else
              this.Toolbars.Show((Toolbar) info.Int, info.Bool, true);
            info.ReturnBoolean = this.IsToolbarVisible((Toolbar) info.Int);
          }
          return true;
        case BarCommand.ToggleToolbar:
          if (Enum.IsDefined(typeof (Toolbar), (object) info.Int))
          {
            this.Toolbars.Toggle((Toolbar) info.Int);
            info.ReturnBoolean = this.IsToolbarVisible((Toolbar) info.Int);
          }
          return true;
        case BarCommand.ShrinkToolbar:
          if (Enum.IsDefined(typeof (Toolbar), (object) info.Int))
            this.ShrinkToolbar((Toolbar) info.Int);
          return true;
        case BarCommand.ExpandToolbar:
          if (Enum.IsDefined(typeof (Toolbar), (object) info.Int))
            this.ExpandToolbar((Toolbar) info.Int);
          return true;
        case BarCommand.IsToolbarVisible:
          if (Enum.IsDefined(typeof (Toolbar), (object) info.Int))
            info.ReturnBoolean = this.IsToolbarVisible((Toolbar) info.Int);
          return true;
        case BarCommand.ViewSearch:
          this.SearchBoxText = info.Str;
          return true;
        case BarCommand.GetViewSearchStr:
          info.ReturnString = this.SearchBoxText;
          return true;
        case BarCommand.ViewFilter:
          if (info.TargetView == TargetView.Default || info.TargetView == TargetView.None)
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.TrySetFilterBoxText(info.Str)));
          if ((info.TargetView == TargetView.Second || info.TargetView == TargetView.None) && this.Toolbars.IsSecondViewBarTabAvailable)
            this.Toolbars.SecondViewBar.Filter(info.Str);
          if ((info.TargetView == TargetView.Third || info.TargetView == TargetView.None) && this.Toolbars.IsThirdViewBarTabAvailable)
            this.Toolbars.ThirdViewBar.Filter(info.Str);
          return true;
        case BarCommand.ViewRefresh:
          if (info.TargetView == TargetView.Default)
          {
            // ISSUE: reference to a compiler-generated method
            this.explorer.Refresh();
            this.NotifyBrowserRefresh(false);
            return true;
          }
          view?.Refresh();
          return true;
        case BarCommand.ViewRedraw:
          if (view != null)
            PInvoke.InvalidateRect(view.ListViewHandle, IntPtr.Zero, true);
          return true;
        case BarCommand.ClipboardSelectionPaths:
        case BarCommand.ClipboardSelectionPathsUNC:
        case BarCommand.ClipboardSelectionNames:
        case BarCommand.ClipboardCurrentPath:
        case BarCommand.ClipboardCurrentPathUNC:
        case BarCommand.ClipboardCurrentName:
        case BarCommand.ClipboardSelectionHashes:
        case BarCommand.ShowHashWindowForSelection:
          return this.DoFileTools(info);
        case BarCommand.ClipboardAllTabPaths:
        case BarCommand.ClipboardAllTabPathsUNC:
          if (info.TargetView == TargetView.None)
          {
            ShellMethods.SetStringClipboard(this.Toolbars.TabBars.SelectMany<TabBarBase, string>((Func<TabBarBase, IEnumerable<string>>) (tabBar { tabBar.TabManager.Items.Select<QTabItem, string>((Func<QTabItem, string>) (tab { info.Command != BarCommand.ClipboardAllTabPathsUNC ? tab.CurrentPath : PathString.GetUniversalName(tab.CurrentPath))))));
            return true;
          }
          break;
        case BarCommand.ViewLeft:
          this.FocusListView(info.TargetView);
          WindowUtil.SendKey(Keys.Left);
          return true;
        case BarCommand.ViewUp:
          this.FocusListView(info.TargetView);
          WindowUtil.SendKey(Keys.Up);
          return true;
        case BarCommand.ViewRight:
          this.FocusListView(info.TargetView);
          WindowUtil.SendKey(Keys.Right);
          return true;
        case BarCommand.ViewDown:
          this.FocusListView(info.TargetView);
          WindowUtil.SendKey(Keys.Down);
          return true;
        case BarCommand.GetCurrentView:
          if (view != null)
            info.ReturnInt32 = (int) view.ExactViewMode;
          else
            info.ReturnInt32 = -1;
          return true;
        case BarCommand.ViewSelectFromCursor:
          if (view != null)
          {
            this.FocusListView(info.TargetView);
            view.SelectAboveCursor(info.Bool);
          }
          return true;
        case BarCommand.ViewSort:
          view?.Sort(info.Str, info.Int);
          return true;
        case BarCommand.ViewPasteInto:
          view?.PasteIntoSelection();
          return true;
        case BarCommand.GetViewSort:
          if (view != null)
            info.ReturnString = view.GetSortString(info.Int);
          return true;
        case BarCommand.GetViewSortDirection:
          if (view != null)
            info.ReturnInt32 = view.GetSortDirection();
          return true;
        case BarCommand.ViewSetItemSelection:
          if (view != null && info.Ints != null && info.Ints.Length > 1)
            view.SelectItem(info.Ints[0], (SVSIF) info.Ints[1]);
          return true;
        case BarCommand.GetColumnWidth:
          if (view != null)
            info.ReturnInt32 = view.GetColumnWidth(info.Str);
          return true;
        case BarCommand.SetColumnWidth:
          view?.SetColumnWidth(info.Str, info.Int);
          return true;
        case BarCommand.GetFocusedView:
          TargetView targetView1;
          this.GetFocusedView(out targetView1);
          info.ReturnInt32 = (int) targetView1;
          return true;
        case BarCommand.GetOtherView:
          TargetView targetView2;
          if (this.GetFocusedView(out targetView2) == null)
            info.ReturnInt32 = -1;
          else
            info.ReturnInt32 = (int) this.Toolbars.GetTheOtherView(targetView2);
          return true;
        case BarCommand.GetFocusedViewPath:
          CustomViewBase focusedView = this.GetFocusedView(out TargetView _);
          if (focusedView != null)
          {
            using (ShellItem shellItem = focusedView.ShellItem)
              info.ReturnString = shellItem.Path;
          }
          return true;
        case BarCommand.GetOtherViewPath:
          TargetView targetView3;
          if (this.GetFocusedView(out targetView3) != null)
          {
            TargetView theOtherView = this.Toolbars.GetTheOtherView(targetView3);
            if (theOtherView == TargetView.Default)
            {
              using (ShellItem shellItem = this.View.ShellItem)
                info.ReturnString = shellItem.Path;
            }
            else if (this.Toolbars.IsExtraViewVisible(theOtherView))
            {
              QSecondViewBar extraViewBar = this.Toolbars.GetExtraViewBar(theOtherView);
              if (extraViewBar.IsActiveTabAvailable)
              {
                using (ShellItem shellItem = extraViewBar.ActiveTab.View.ShellItem)
                  info.ReturnString = shellItem.Path;
              }
            }
          }
          return true;
        case BarCommand.GetPathFromView:
          TargetView targetView4 = (TargetView) info.Int;
          if (targetView4 == TargetView.Default)
          {
            using (ShellItem shellItem = this.View.ShellItem)
              info.ReturnString = shellItem.Path;
          }
          else if (this.Toolbars.IsExtraViewVisible(targetView4))
          {
            QSecondViewBar extraViewBar = this.Toolbars.GetExtraViewBar(targetView4);
            if (extraViewBar.IsActiveTabAvailable)
            {
              using (ShellItem shellItem = extraViewBar.ActiveTab.View.ShellItem)
                info.ReturnString = shellItem.Path;
            }
          }
          return true;
        case BarCommand.MoveTabToView:
          if (info.Int == -1)
          {
            TabBarBase focusedTabBar = this.GetFocusedTabBar();
            if (focusedTabBar == null || !focusedTabBar.TabManager.IsActiveTabAvailable)
              return true;
            info.Int = focusedTabBar.TabManager.ActiveTab.ID;
          }
          foreach (TabBarBase tabBar1 in this.Toolbars.TabBars)
          {
            QTabItem tab = tabBar1.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == info.Int));
            if (tab != null)
            {
              TargetView targetView5 = tabBar1.TargetView;
              if (targetView5 != info.TargetView)
              {
                if (info.TargetView == TargetView.Other)
                  info.TargetView = this.Toolbars.GetTheOtherView(targetView5);
                else if (info.TargetView == TargetView.Priority)
                  info.TargetView = this.Toolbars.GetPriorityView();
                else if (info.TargetView < TargetView.Default)
                  info.TargetView = TargetView.Default;
                TabBarBase tabBar2 = this.Toolbars.GetTabBar(info.TargetView, true);
                if (tabBar2 != null)
                  tabBar2.ReparentTab(tab);
                else if (info.TargetView == TargetView.Default)
                {
                  this.PendingTabDefaultView = tab;
                  this.Toolbars.Show(Config.Bool(Scts.TabBarPriority) ? Toolbar.BottomTabBar : Toolbar.TabBar, true);
                }
                else
                {
                  this.PendingTabExtraView = tab;
                  this.Toolbars.ShowExtraView(info.TargetView, true);
                }
                if (!info.Bool && (tabBar1.TargetView != TargetView.Default || tabBar1.TabManager.Count > 1))
                  tab.Close();
                this.FocusListView(info.TargetView);
              }
              return true;
            }
          }
          if (info.Scripting)
            throw new InvalidOperationException("The tab was not found in any tabbar.");
          return true;
        case BarCommand.GetIconSize:
          if (view != null)
            info.ReturnInt32 = view.IconSizeInPixel;
          return true;
        case BarCommand.SetIconSize:
          if (view != null)
          {
            view.IconSizeInPixel = info.Int;
            info.ReturnInt32 = info.Int;
          }
          return true;
        case BarCommand.ScrollView:
          if (view != null)
          {
            bool flag5 = info.Ints != null && info.Ints.Length != 0 && info.Ints[0] != 0;
            bool flag6 = info.Ints != null && info.Ints.Length > 1 && info.Ints[1] != 0;
            info.ReturnBoolean = view.Scroll(!flag5, !flag6);
          }
          return true;
        case BarCommand.ShowPreview:
          if (view != null)
          {
            this.FocusListView(info.TargetView);
            view.ShowPreviewTipForSelectedItem(true);
          }
          return true;
        case BarCommand.PreviewVolumeUp:
          PreviewTip.HandleKeyDown(107, false);
          return true;
        case BarCommand.PreviewVolumeDown:
          PreviewTip.HandleKeyDown(109, false);
          return true;
        case BarCommand.PreviewMute:
          PreviewTip.HandleKeyDown(96, false);
          return true;
        case BarCommand.PreviewSlower:
          PreviewTip.HandleKeyDown(97, false);
          return true;
        case BarCommand.PreviewResetSpeed:
          PreviewTip.HandleKeyDown(98, false);
          return true;
        case BarCommand.PreviewFaster:
          PreviewTip.HandleKeyDown(99, false);
          return true;
        case BarCommand.PreviewBack:
          PreviewTip.HandleKeyDown(100, false);
          return true;
        case BarCommand.PreviewPause:
          PreviewTip.HandleKeyDown(101, false);
          return true;
        case BarCommand.PreviewFwrd:
          PreviewTip.HandleKeyDown(102, false);
          return true;
        case BarCommand.PreviewShrink:
          PreviewTip.HandleKeyDown(103, false);
          return true;
        case BarCommand.PreviewFullSize:
          PreviewTip.HandleKeyDown(104, false);
          return true;
        case BarCommand.PreviewWiden:
          PreviewTip.HandleKeyDown(105, false);
          return true;
        case BarCommand.PreviewFullSzMode:
          PreviewTip.HandleKeyDown(111, false);
          return true;
        case BarCommand.PreviewCornerMode:
          PreviewTip.HandleKeyDown(106, false);
          return true;
        case BarCommand.PreviewFullScreen:
          PreviewTip.HandleKeyDown(110, false);
          return true;
        case BarCommand.PreviewStopFor:
          PreviewSource source = (PreviewSource) info.Int;
          switch (source)
          {
            case PreviewSource.None:
              Config.Set(Scts.PreviewStop, info.Bool);
              Config.Save();
              InstanceManager.SyncCommandBarThreads(CommandBarSyncStaticCommand.RefreshMediaButton, info.Hwnd);
              InstanceManager.SyncProcesses(32787);
              break;
            case PreviewSource.Picture:
              Config.Set(Scts.PreviewStopPicture, info.Bool);
              break;
            case PreviewSource.AudioVideo:
              Config.Set(Scts.PreviewStopMedia, info.Bool);
              break;
            case PreviewSource.Text:
              Config.Set(Scts.PreviewStopText, info.Bool);
              break;
          }
          if (info.Bool)
            PreviewTip.HideAll(source);
          if (source != PreviewSource.None)
          {
            Config.Sync();
            break;
          }
          break;
        case BarCommand.ClearPreviewCache:
          PreviewTip.ClearChache();
          return true;
        case BarCommand.ShowMenuFor:
          if (Enum.IsDefined(typeof (MenuKind), (object) info.Int))
          {
            BuiltinCommandID id = BuiltinCommandID.Group;
            switch (info.Int)
            {
              case 2:
                id = BuiltinCommandID.Group;
                break;
              case 3:
                id = BuiltinCommandID.RecentTab;
                break;
              case 4:
                id = BuiltinCommandID.RecentFile;
                break;
              case 5:
                id = BuiltinCommandID.AppLauncher;
                break;
              default:
                return true;
            }
            this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.PerformClickBuiltInItem(id)));
            break;
          }
          break;
        case BarCommand.ShowSubFolderMenu:
          if (Config.Get(Scts.SubDirTip) > 0 && view != null)
          {
            view.Focus();
            view.ShowAndClickSubDirTip();
          }
          return true;
        case BarCommand.ClearRecentTabHistory:
          RecentTabManager.Clear();
          return true;
        case BarCommand.ClearRecentFileHistory:
          RecentFileManager.Clear();
          return true;
        case BarCommand.GlobalOption:
          if (Enum.IsDefined(typeof (OptionDialogTab), (object) info.Int))
            this.OpenOptionDialog((OptionDialogTab) info.Int);
          return true;
        case BarCommand.ButtonBarOption:
          this.InvokeCommand(new CommandInfo(BarCommand.GlobalOption, 13));
          return true;
        case BarCommand.SaveDesktopIconPos:
          ShellUtils.SaveDesktopIconPositions((IWin32Window) new Win32Window(this.hwndExplorer), info.Str);
          return true;
        case BarCommand.LoadDesktopIconPos:
          ShellUtils.LoadDesktopIconPositions((IWin32Window) new Win32Window(this.hwndExplorer), info.Str);
          return true;
        case BarCommand.CreateNewFolder:
          if (info.Str != null && Path.IsPathRooted(info.Str))
          {
            DirectoryInfo directory = Directory.CreateDirectory(info.Str);
            if (directory.Exists)
            {
              if (info.Int == 1)
              {
                using (ShellItem shellItem = new ShellItem(directory.FullName))
                {
                  CommandInfo info2 = new CommandInfo(BarCommand.NewTab, shellItem.IDL, BarCommandOptions.Activate | BarCommandOptions.IgnoreModKeys);
                  info2.TargetView = info.TargetView;
                  this.InvokeCommand(info2);
                  info.ReturnInt32 = info2.ReturnInt32;
                }
              }
              else
                info.ReturnString = directory.FullName;
            }
            return true;
          }
          if (view != null)
          {
            using (ShellItem shellItem1 = view.ShellItem)
            {
              using (ShellItem shellItem2 = new ShellItem(ShellMethods.CreateNewItem(info.Int == 0 ? view.ShellBrowser : (IShellBrowser) null, shellItem1, true, this.hwndExplorer, info.Str)))
              {
                if (shellItem2.Available)
                {
                  if (info.Int == 1)
                  {
                    CommandInfo info3 = new CommandInfo(BarCommand.NewTab, shellItem2.IDL, BarCommandOptions.Activate | BarCommandOptions.IgnoreModKeys);
                    info3.TargetView = info.TargetView;
                    this.InvokeCommand(info3);
                    info.ReturnInt32 = info3.ReturnInt32;
                  }
                  else if (shellItem2.HasPath)
                    info.ReturnString = shellItem2.Path;
                }
              }
            }
          }
          return true;
        case BarCommand.CreateNewTxtFile:
          if (info.Str != null)
          {
            if (Path.IsPathRooted(info.Str))
            {
              info.ReturnString = FileOperationHelper.CreateNewFileFromPath((string) null, info.Str, info.Bool, info.Scripting);
              return true;
            }
            if (info.Str.Contains(Path.DirectorySeparatorChar.ToString()) && view != null)
            {
              info.ReturnString = FileOperationHelper.CreateNewFileFromPath(view.Path, info.Str, info.Bool, info.Scripting);
              return true;
            }
          }
          if (view != null)
          {
            using (ShellItem shellItem3 = view.ShellItem)
            {
              using (ShellItem shellItem4 = new ShellItem(ShellMethods.CreateNewItem(view.ShellBrowser, shellItem3, false, this.hwndExplorer, info.Str)))
              {
                if (shellItem4.HasPath)
                  info.ReturnString = shellItem4.Path;
              }
            }
          }
          return true;
        case BarCommand.ShowProperties:
          if (!ShellMethods.IsIDLNullOrEmpty(info.IDL))
          {
            ShellMethods.ShowProperties(info.IDL, this.hwndExplorer);
            return true;
          }
          if (!string.IsNullOrEmpty(info.Str))
          {
            using (ShellItem shellItem = new ShellItem(info.Str))
            {
              if (shellItem.Available)
                ShellMethods.ShowProperties(shellItem.IDL, this.hwndExplorer);
            }
            return true;
          }
          if (info.Int == -1)
          {
            ShellMethods.ShowProperties(this.currentIDL, this.hwndExplorer);
            return true;
          }
          bool flag7 = false;
          foreach (TabBarBase tabBar in this.Toolbars.TabBars)
          {
            if (tabBar.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == info.Int)) != null)
            {
              info.TargetView = tabBar.TargetView;
              flag7 = true;
              break;
            }
          }
          if (!flag7)
            return true;
          break;
        case BarCommand.RenameDialogSelection:
        case BarCommand.RenameDialogAll:
          if (view != null)
          {
            using (ShellItem shellItem = view.ShellItem)
            {
              if (shellItem.HasPath && shellItem.IsFileSystem)
              {
                if (this.fileRenameDialog == null)
                  this.fileRenameDialog = new FileRenameDialog(new FileOperationProxy(new Func<TargetView, IEnumerable<string>>(this.fileOp_GetSelection), new Action(this.fileOp_Undo), info.TargetView), this.hwndExplorer);
                this.fileRenameDialog.Show((IWin32Window) new Win32Window(this.hwndExplorer), shellItem.Path, info.Command == BarCommand.RenameDialogSelection, info.TargetView);
                break;
              }
              SystemSounds.Asterisk.Play2();
              break;
            }
          }
          else
            break;
        case BarCommand.AnalyzeFolder:
        case BarCommand.EditAttributes:
          string str3 = string.Empty;
          if (!string.IsNullOrEmpty(info.Str))
            str3 = info.Str;
          else if (view != null)
          {
            str3 = view.Path;
            if (view.SelectedItemCount > 0)
            {
              IShellItemArray selectionArray = view.GetSelectionArray();
              try
              {
                if (selectionArray != null)
                {
                  foreach (string path in selectionArray.GetPaths())
                  {
                    if (Directory.Exists(path))
                    {
                      str3 = path;
                      break;
                    }
                  }
                }
              }
              finally
              {
                if (selectionArray != null)
                  Marshal.ReleaseComObject((object) selectionArray);
              }
            }
            else
            {
              string path = view.Path;
              if (Directory.Exists(path))
                str3 = path;
            }
          }
          switch (info.Command)
          {
            case BarCommand.AnalyzeFolder:
              AppLauncher.ExecuteDirectoryAnalyzer(str3);
              break;
            case BarCommand.EditAttributes:
              AppLauncher.ExecuteAttributeEditor(str3);
              break;
          }
          return true;
        case BarCommand.CreateSymbolicLink:
          if (info.Strs != null && info.Strs.Length == 2)
            Link.Create(info.Strs[0], info.Strs[1], (LinkType) info.Int);
          else if (view != null && view.SelectedItemCount > 0)
          {
            string pathTarget = string.Empty;
            IShellItemArray selectionArray = view.GetSelectionArray();
            try
            {
              if (selectionArray != null)
              {
                using (List<string>.Enumerator enumerator = selectionArray.GetPaths().GetEnumerator())
                {
                  if (enumerator.MoveNext())
                    pathTarget = enumerator.Current;
                }
              }
            }
            finally
            {
              if (selectionArray != null)
                Marshal.ReleaseComObject((object) selectionArray);
            }
            Link.CreateFromPath(pathTarget, (IWin32Window) new Win32Window(this.ExplorerHandle));
          }
          return true;
        case BarCommand.SetViewSync:
          this.ViewSyncManager.SynchronizationLevel = (ViewSynchronizationLevel) info.Int;
          return true;
        case BarCommand.GetViewSync:
          info.ReturnInt32 = (int) this.ViewSyncManager.SynchronizationLevel;
          return true;
        case BarCommand.Import:
          if (CommandInvoker.InvokeNeutral((CommandInfoBase) info, this.hwndExplorer))
            this.odCallback_ConfigLoaded(new OptionDialogCallbackInfo(OptionCallbackKind.ConfigFileLoaded));
          return true;
        case BarCommand.GetTabs:
          List<int> intList = new List<int>();
          if ((info.TargetView == TargetView.None || info.TargetView == TargetView.Default) && this.Toolbars.IsTabBarAvailable)
          {
            this.Toolbars.TabBar.InvokeCommand(info);
            if (info.ReturnInt32Array != null)
              intList.AddRange((IEnumerable<int>) info.ReturnInt32Array);
          }
          info.ReturnInt32Array = (int[]) null;
          if ((info.TargetView == TargetView.None || info.TargetView == TargetView.Second) && this.Toolbars.IsSecondViewBarVisible)
          {
            this.Toolbars.SecondViewBar.InvokeCommand(info);
            if (info.ReturnInt32Array != null)
              intList.AddRange((IEnumerable<int>) info.ReturnInt32Array);
          }
          info.ReturnInt32Array = (int[]) null;
          if ((info.TargetView == TargetView.None || info.TargetView == TargetView.Third) && this.Toolbars.IsThirdViewBarVisible)
          {
            this.Toolbars.ThirdViewBar.InvokeCommand(info);
            if (info.ReturnInt32Array != null)
              intList.AddRange((IEnumerable<int>) info.ReturnInt32Array);
          }
          info.ReturnInt32Array = intList.ToArray();
          return true;
        case BarCommand.GetActiveView:
          info.ReturnInt32 = (int) this.GetFocusedTargetView();
          return true;
        case BarCommand.GetSelectionForTab:
          try
          {
            IList<byte[]> selectionForTabId = this.GetSelectionForTabID(info.Int, info.Scripting);
            if (selectionForTabId != null)
            {
              List<string> stringList = new List<string>();
              foreach (byte[] idl in (IEnumerable<byte[]>) selectionForTabId)
              {
                using (ShellItem shellItem = new ShellItem(idl, false))
                {
                  if (shellItem.HasPath)
                    stringList.Add(shellItem.Path);
                }
              }
              info.ReturnStringArray = stringList.ToArray();
            }
          }
          catch (Exception ex)
          {
          }
          return true;
        case BarCommand.SetSelectionForTab:
          try
          {
            List<string> lstItems = info.Strs == null ? new List<string>() : new List<string>((IEnumerable<string>) info.Strs);
            if (info.Int == -2)
            {
              info.ReturnBoolean = this.shellBrowser.TrySetSelection(ShellUtils.ParseSelectionData(lstItems, (byte[]) null, this.shellBrowser), (byte[]) null);
            }
            else
            {
              QTabItem qtabItem = (QTabItem) null;
              TabBarBase tabBarBase = (TabBarBase) null;
              foreach (TabBarBase tabBar in this.Toolbars.TabBars)
              {
                qtabItem = tabBar.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == info.Int));
                if (qtabItem != null)
                {
                  tabBarBase = tabBar;
                  break;
                }
              }
              if (qtabItem == null)
                return true;
              if (tabBarBase.TargetView == TargetView.Default)
              {
                if (qtabItem.IsActive)
                {
                  info.ReturnBoolean = this.shellBrowser.TrySetSelection(ShellUtils.ParseSelectionData(lstItems, (byte[]) null, this.shellBrowser), (byte[]) null);
                }
                else
                {
                  qtabItem.SetSelectedItems(ShellUtils.ParseSelectionData(lstItems, qtabItem.CurrentIDL, (IShellBrowser) null), (byte[]) null);
                  info.ReturnBoolean = true;
                }
              }
              else
              {
                qtabItem.SetSelectedItems(ShellUtils.ParseSelectionData(lstItems, qtabItem.CurrentIDL, (IShellBrowser) null), (byte[]) null);
                info.ReturnBoolean = true;
              }
            }
          }
          catch
          {
          }
          return true;
        case BarCommand.GetSelectionFoldersForTab:
          try
          {
            int id = info.Ints[0];
            bool flag8 = info.Ints[1] == 0;
            IList<byte[]> selectionForTabId = this.GetSelectionForTabID(id, info.Scripting);
            if (selectionForTabId != null)
            {
              List<string> stringList = new List<string>();
              foreach (byte[] idl in (IEnumerable<byte[]>) selectionForTabId)
              {
                using (ShellItem shellItem = new ShellItem(idl, false))
                {
                  if (shellItem.HasPath)
                  {
                    if (shellItem.IsFolder == flag8)
                      stringList.Add(shellItem.Path);
                  }
                }
              }
              info.ReturnStringArray = stringList.ToArray();
            }
          }
          catch
          {
          }
          return true;
        case BarCommand.GetExplorer:
          info.ReturnObject = (object) new object[1]
          {
            (object) this.hwndExplorer
          };
          return true;
        case BarCommand.GetDataInternal:
          Tuple<SlotDataType, object> slotdata = CommandDataSlot.GetSlotdata(info.Int);
          if (slotdata != null)
          {
            info.ReturnInt32 = (int) slotdata.Item1;
            info.ReturnObject = slotdata.Item2;
            info.ReturnBoolean = true;
          }
          return true;
        case BarCommand.SetDataInternal:
          if (info.Ints != null && info.Ints.Length == 2)
          {
            CommandDataSlot.SetSlotData(info.Ints[0], (SlotDataType) info.Ints[1], info.Object);
            info.ReturnBoolean = true;
          }
          return true;
        case BarCommand.GetViewFromID:
          if (info.Int == -2)
          {
            info.ReturnInt32 = 0;
          }
          else
          {
            foreach (TabBarBase tabBar in this.Toolbars.TabBars)
            {
              if (tabBar.TabManager.Items.Any<QTabItem>((Func<QTabItem, bool>) (tab { tab.ID == info.Int)))
              {
                info.ReturnInt32 = (int) tabBar.TargetView;
                return true;
              }
            }
            info.ReturnInt32 = -1;
          }
          return true;
        case BarCommand.MakeMia:
          if (info.Strs != null && info.Strs.Length != 0 && !string.IsNullOrWhiteSpace(info.Strs[0]))
          {
            MenuItemArguments mia = new MenuItemArguments(info.Strs[0], info.Strs[1] ?? string.Empty, info.Strs[2] ?? string.Empty, 0, MenuGenre.None);
            if (view != null)
            {
              int num1 = (int) new TokenReplacer(new Func<IList<ShellItem>>(view.GetSelection), new Func<IntPtr>(view.GetCurrentFolder)).Process(mia);
            }
            else
            {
              int num2 = (int) TokenReplacer.Null.Process(mia);
            }
            info.ReturnStringArray = new string[3]
            {
              mia.Path,
              mia.Argument,
              mia.WorkingDirectory
            };
          }
          return true;
        case BarCommand.GetEventData:
          Tuple<SlotDataType, object> userEventData = this.GetUserEventData(info.Int);
          if (userEventData != null)
          {
            info.ReturnInt32 = (int) userEventData.Item1;
            info.ReturnObject = userEventData.Item2;
            info.ReturnBoolean = true;
          }
          return true;
        case BarCommand.GetEventSource:
          info.ReturnInt32 = (int) this.GetUserEventSource(info.Int);
          return true;
        case BarCommand.GetEventSourceStr:
          info.ReturnString = this.GetUserEventSource(info.Int).ToString();
          return true;
        case BarCommand.GetEventKeyState:
          info.ReturnInt32 = this.GetUserEventKeyState(info.Int);
          return true;
        case BarCommand.GetEventTargetView:
          info.ReturnInt32 = (int) this.GetUserEventTargetView(info.Int);
          return true;
        case BarCommand.SetReturn:
          if (info.Ints != null && info.Ints.Length == 2)
            CommandDataSlot.SetReturn(info.Ints[0], (SlotDataType) info.Ints[1], info.Object);
          return true;
      }
      if (this.Toolbars.IsTabBarAvailable && (info.TargetView == TargetView.Default || info.TargetView == TargetView.None))
      {
        this.Toolbars.TabBar.InvokeCommand(info);
        return true;
      }
      if (info.TargetView == TargetView.Second)
      {
        if (this.Toolbars.IsSecondViewBarVisible)
          this.Toolbars.SecondViewBar.InvokeCommand(info);
        else if (flag1)
        {
          this.PendingCommandExtraView = info;
          this.Toolbars.ShowSecondView(true);
        }
        return true;
      }
      if (info.TargetView == TargetView.Third)
      {
        if (this.Toolbars.IsThirdViewBarVisible)
          this.Toolbars.ThirdViewBar.InvokeCommand(info);
        else if (flag1)
        {
          this.PendingCommandExtraView = info;
          this.Toolbars.ShowThirdView(true);
        }
        return true;
      }
      if (info.TargetView != TargetView.Priority)
        return this.PerformCommandFallback(info);
      if (Config.Bool(Scts.ExtraViewPriority))
      {
        if (this.Toolbars.IsThirdViewBarVisible)
          this.Toolbars.ThirdViewBar.InvokeCommand(info);
        else if (this.Toolbars.IsSecondViewBarVisible)
          this.Toolbars.SecondViewBar.InvokeCommand(info);
        else if (flag1)
        {
          this.PendingCommandExtraView = info;
          this.Toolbars.ShowThirdView(true);
        }
      }
      else if (this.Toolbars.IsSecondViewBarVisible)
        this.Toolbars.SecondViewBar.InvokeCommand(info);
      else if (this.Toolbars.IsThirdViewBarVisible)
        this.Toolbars.ThirdViewBar.InvokeCommand(info);
      else if (flag1)
      {
        this.PendingCommandExtraView = info;
        this.Toolbars.ShowSecondView(true);
      }
      return true;
    }

    private IList<byte[]> GetSelectionForTabID(int id, bool fScripting)
    {
      if (id == -2 || id == -1 || id == 0)
        return this.View.GetSelectionIDL();
      QTabItem tab = (QTabItem) null;
      TabBarBase tabBarBase = (TabBarBase) null;
      foreach (TabBarBase tabBar in this.Toolbars.TabBars)
      {
        tab = tabBar.TabManager.Items.FirstOrDefault<QTabItem>((Func<QTabItem, bool>) (t { t.ID == id));
        if (tab != null)
        {
          tabBarBase = tabBar;
          break;
        }
      }
      if (tab == null)
      {
        if (fScripting)
          throw new InvalidOperationException("The tab was not found in any tabbar.");
        return (IList<byte[]>) null;
      }
      if (tabBarBase.TargetView != TargetView.Default)
        return tab.AssociatedView.GetSelectionIDL();
      if (tab.IsActive)
        return this.View.GetSelectionIDL();
      List<byte[]> selectedItems = tab.GetSelectedItems(out byte[] _);
      return selectedItems != null ? (IList<byte[]>) selectedItems.Select<byte[], byte[]>((Func<byte[], byte[]>) (idl {
      {
        using (ShellItem shellItem = new ShellItem((ItemIDList) tab.CurrentIDL, (ItemIDList) idl))
          return shellItem.IDL;
      })).Where<byte[]>((Func<byte[], bool>) (idl { !ShellMethods.IsIDLNullOrEmpty(idl))).ToList<byte[]>() : (IList<byte[]>) null;
    }

    public bool InvokeCommand(BarCommand command, TargetView targetView = TargetView.Default) { this.InvokeCommand(new CommandInfo(command, targetView));

    private bool PerformCommandFallback(CommandInfo info)
    {
      switch (info.Command)
      {
        case BarCommand.GoBack:
          return this.shellBrowser.BrowseObject(IntPtr.Zero, SBSP.SAMEBROWSER | SBSP.NAVIGATEBACK) == 0;
        case BarCommand.GoForward:
          try
          {
            // ISSUE: reference to a compiler-generated method
            this.explorer.GoForward();
          }
          catch
          {
          }
          return true;
        case BarCommand.GoBackToFirst:
          do
            ;
          while (this.shellBrowser.BrowseObject(IntPtr.Zero, SBSP.SAMEBROWSER | SBSP.NAVIGATEBACK) == 0);
          return true;
        case BarCommand.GoForwardToLast:
          do
            ;
          while (this.shellBrowser.BrowseObject(IntPtr.Zero, SBSP.SAMEBROWSER | SBSP.NAVIGATEFORWARD) == 0);
          return true;
        case BarCommand.NewTab:
        case BarCommand.NewTabAt:
        case BarCommand.NewWindow:
        case BarCommand.OpenInPlace:
          using (ShellItem shellItem1 = new ShellItem(info.IDL))
          {
            if (shellItem1.Available)
            {
              if (info.Command == BarCommand.NewWindow)
              {
                this.OpenNewWindow(shellItem1.PIDL);
                return true;
              }
              SBSP wFlags = Config.Get(Scts.PlaySoundOnNavigation) != 2 ? SBSP.SAMEBROWSER : SBSP.SAMEBROWSER | SBSP.PLAYNOSOUND;
              if (info.Options.HasFlag((Enum) BarCommandOptions.NoFocusView))
                wFlags |= SBSP.ACTIVATE_NOFOCUS;
              this.shellBrowser.BrowseObject(shellItem1.PIDL, wFlags);
              this.BringToFront();
              return true;
            }
            if (info.Command == BarCommand.NewWindow)
            {
              if (info.IDL == null)
              {
                using (ShellItem shellItem2 = this.View.ShellItem)
                {
                  this.OpenNewWindow((IntPtr) shellItem2);
                  return true;
                }
              }
              else
                break;
            }
            else
              break;
          }
        case BarCommand.CloseTab:
        case BarCommand.CloseWindow:
          WindowUtil.CloseExplorer(this.hwndExplorer, WMCLOSEARG.Normal);
          return true;
        case BarCommand.GoUpOneLevel:
          this.OnGoingUp();
          this.shellBrowser.BrowseObject(IntPtr.Zero, (SBSP) ((Config.Get(Scts.PlaySoundOnNavigation) == 2 ? 2097152 : 0) | 8192 | 1));
          return true;
        case BarCommand.Group:
          if (info.TargetView == TargetView.Second)
          {
            this.PendingGroupsExtraView = !string.IsNullOrEmpty(info.Str) ? (IList<string>) info.Str.AsArray<string>() : (IList<string>) info.Strs;
            this.Toolbars.ShowSecondView(true);
            break;
          }
          if (info.TargetView == TargetView.Third)
          {
            this.PendingGroupsExtraView = !string.IsNullOrEmpty(info.Str) ? (IList<string>) info.Str.AsArray<string>() : (IList<string>) info.Strs;
            this.Toolbars.ShowThirdView(true);
            break;
          }
          int num = Config.Bool(Scts.ExtraViewPriority) ? 1 : 0;
          bool flag1 = num != 0 ? this.Toolbars.IsThirdViewBarVisible : this.Toolbars.IsSecondViewBarVisible;
          bool flag2 = num != 0 ? this.Toolbars.IsSecondViewBarVisible : this.Toolbars.IsThirdViewBarVisible;
          QSecondViewBar qsecondViewBar1 = num != 0 ? (QSecondViewBar) this.Toolbars.ThirdViewBar : this.Toolbars.SecondViewBar;
          QSecondViewBar qsecondViewBar2 = num != 0 ? this.Toolbars.SecondViewBar : (QSecondViewBar) this.Toolbars.ThirdViewBar;
          if (flag1)
          {
            qsecondViewBar1.InvokeCommand(info);
            break;
          }
          if (flag2)
          {
            qsecondViewBar2.InvokeCommand(info);
            break;
          }
          this.PendingGroupsDefault = !string.IsNullOrEmpty(info.Str) ? (IList<string>) info.Str.AsArray<string>() : (IList<string>) info.Strs;
          this.Toolbars.Show(Config.Bool(Scts.TabBarPriority) ? Toolbar.BottomTabBar : Toolbar.TabBar, true);
          break;
        case BarCommand.DeleteCurrentTab:
          this.DeleteSelection(true, info.Options.HasFlag((Enum) BarCommandOptions.Nuke));
          return true;
        case BarCommand.CopyFromFolder:
          this.CopyFrom(FileOperationVerb.Copy);
          return true;
        case BarCommand.MoveFromFolder:
          this.CopyFrom(FileOperationVerb.Move);
          return true;
        case BarCommand.LinkFromFolder:
          this.CopyFrom(FileOperationVerb.Link);
          return true;
        case BarCommand.GetActiveTab:
          info.ReturnInt32 = -2;
          return true;
      }
      return false;
    }

    public bool InvokeCommand(CommandInfoBase infoBase)
    {
      CommandInfo info = new CommandInfo(infoBase);
      int num = this.InvokeCommand(info) ? 1 : 0;
      info.CopyDataToBase(infoBase);
      return num != 0;
    }

    public void InvokeMenuCommand(ExplorerMenuCommand command) { this.View.InvokeMenuCommand(command);

    public void OnDynamicVerbInvoked(Message msg, TargetView targetView)
    {
      switch ((int) msg.WParam)
      {
        case 0:
          CommandInfo info1 = new CommandInfo(BarCommand.CopyToTab, -1);
          info1.TargetView = targetView;
          this.InvokeCommand(info1);
          break;
        case 1:
          CommandInfo info2 = new CommandInfo(BarCommand.MoveToTab, -1);
          info2.TargetView = targetView;
          this.InvokeCommand(info2);
          break;
        case 2:
          CommandInfo info3 = new CommandInfo(BarCommand.CreateLinkInTab, -1);
          info3.TargetView = targetView;
          this.InvokeCommand(info3);
          break;
        case 3:
          CommandInfo info4 = new CommandInfo(BarCommand.CopyToFolder);
          info4.TargetView = targetView;
          this.InvokeCommand(info4);
          break;
        case 4:
          CommandInfo info5 = new CommandInfo(BarCommand.MoveToFolder);
          info5.TargetView = targetView;
          this.InvokeCommand(info5);
          break;
        case 5:
          CommandInfo info6 = new CommandInfo(BarCommand.CopyFromFolder);
          info6.TargetView = targetView;
          this.InvokeCommand(info6);
          break;
        case 6:
          CommandInfo info7 = new CommandInfo(BarCommand.MoveFromFolder);
          info7.TargetView = targetView;
          this.InvokeCommand(info7);
          break;
        case 7:
          CommandInfo info8 = new CommandInfo(BarCommand.LinkFromFolder);
          info8.TargetView = targetView;
          this.InvokeCommand(info8);
          break;
        case 8:
          CustomViewBase view = this.GetView(targetView);
          if (view == null)
            break;
          IList<ShellItem> selection = view.GetSelection();
          if (selection.Count <= 0)
            break;
          CommandInfo info9 = new CommandInfo(BarCommand.CopyFolderSkelton);
          info9.ShellItems = new List<ShellItem>((IEnumerable<ShellItem>) selection);
          info9.TargetView = targetView;
          this.InvokeCommand(info9);
          selection.ForEach<ShellItem>((Action<ShellItem>) (si { si.Dispose()));
          break;
        case 10:
          CommandInfo info10 = new CommandInfo(MCR.HIWORD(msg.LParam) == 0 ? BarCommand.RenameDialogAll : BarCommand.RenameDialogSelection);
          info10.TargetView = targetView;
          this.InvokeCommand(info10);
          break;
        case 13:
          CommandInfo info11 = new CommandInfo(BarCommand.ShowHashWindowForSelection);
          info11.TargetView = targetView;
          this.InvokeCommand(info11);
          break;
        case 14:
          CommandInfo info12 = new CommandInfo(BarCommand.ClipboardSelectionHashes);
          info12.TargetView = targetView;
          this.InvokeCommand(info12);
          break;
        case 15:
          CommandInfo info13 = new CommandInfo(BarCommand.AnalyzeFolder);
          info13.TargetView = targetView;
          this.InvokeCommand(info13);
          break;
        case 16:
          CommandInfo info14 = new CommandInfo(BarCommand.EditAttributes);
          info14.TargetView = targetView;
          this.InvokeCommand(info14);
          break;
        case 17:
          CommandInfo info15 = new CommandInfo(BarCommand.CreateSymbolicLink);
          info15.TargetView = targetView;
          this.InvokeCommand(info15);
          break;
      }
    }

    public void OpenNewWindow(IntPtr pidl)
    {
      SHELLEXECUTEINFO lpExecInfo = new SHELLEXECUTEINFO();
      lpExecInfo.cbSize = Marshal.SizeOf(typeof (SHELLEXECUTEINFO));
      lpExecInfo.nShow = SHOWWINDOW.SHOWNORMAL;
      lpExecInfo.fMask = SEEMASK.IDLIST | SEEMASK.ASYNCOK;
      lpExecInfo.lpIDList = pidl;
      lpExecInfo.lpVerb = Marshal.StringToCoTaskMemUni("opennewwindow");
      try
      {
        this.InModal = true;
        PInvoke.ShellExecuteEx(ref lpExecInfo);
      }
      finally
      {
        if (lpExecInfo.lpVerb != IntPtr.Zero)
          Marshal.FreeCoTaskMem(lpExecInfo.lpVerb);
        this.InModal = false;
      }
    }

    private bool DoFileTools(CommandInfo info)
    {
      CustomViewBase view = this.GetView(info.TargetView);
      if (view != null)
      {
        IShellItemArray shellItemArray = (IShellItemArray) null;
        switch (info.Command)
        {
          case BarCommand.ClipboardSelectionPaths:
          case BarCommand.ClipboardSelectionPathsUNC:
          case BarCommand.ClipboardSelectionNames:
            try
            {
              shellItemArray = view.GetSelectionArray();
              if (shellItemArray != null)
              {
                IEnumerable<string> strings = (IEnumerable<string>) shellItemArray.GetPaths(info.Command == BarCommand.ClipboardSelectionNames);
                if (info.Command == BarCommand.ClipboardSelectionPathsUNC)
                  strings = strings.Select<string, string>((Func<string, string>) (s { PathString.GetUniversalName(s)));
                ShellMethods.SetStringClipboard(strings);
              }
            }
            finally
            {
              if (shellItemArray != null)
                Marshal.ReleaseComObject((object) shellItemArray);
            }
            return true;
          case BarCommand.ClipboardCurrentPath:
          case BarCommand.ClipboardCurrentPathUNC:
          case BarCommand.ClipboardCurrentName:
            using (ShellItem shellItem = view.ShellItem)
            {
              if (shellItem.Available)
                ShellMethods.SetStringClipboard(info.Command == BarCommand.ClipboardCurrentPath ? shellItem.Path : (info.Command == BarCommand.ClipboardCurrentPathUNC ? PathString.GetUniversalName(shellItem.Path) : shellItem.DisplayName));
            }
            return true;
          case BarCommand.ClipboardSelectionHashes:
          case BarCommand.ShowHashWindowForSelection:
            try
            {
              shellItemArray = view.GetSelectionArray();
              if (shellItemArray != null)
              {
                List<string> paths = shellItemArray.GetPaths();
                if (info.Command == BarCommand.ShowHashWindowForSelection)
                  FileHashComputer.ShowForm((IEnumerable<string>) paths);
                else if (info.Command == BarCommand.ClipboardSelectionHashes)
                  QHash.GetForPath((IEnumerable<string>) paths, this.hwndExplorer);
              }
            }
            finally
            {
              if (shellItemArray != null)
                Marshal.ReleaseComObject((object) shellItemArray);
            }
            return true;
        }
      }
      return false;
    }

    public int BrowseFolder(byte[] idlInitial, TargetView targetView = TargetView.Default, bool fBackground = false)
    {
      this.InModal = true;
      bool isTopMost = this.IsTopMost;
      if (isTopMost)
        this.ToggleTopMost();
      try
      {
        using (FolderBrowserDialogEx folderBrowserDialogEx = new FolderBrowserDialogEx())
        {
          using (ShellItem shellItem1 = new ShellItem(idlInitial ?? this.currentIDL, false))
          {
            folderBrowserDialogEx.InitialFolder = shellItem1.PIDL;
            if (DialogResult.OK == folderBrowserDialogEx.ShowDialog(this.hwndExplorer))
            {
              using (ShellItem shellItem2 = new ShellItem(folderBrowserDialogEx.PIDL))
              {
                CommandInfo commandInfo = new CommandInfo(BarCommand.NewTab, shellItem2.IDL);
                commandInfo.TargetView = targetView;
                commandInfo.Options = fBackground ? BarCommandOptions.NoActivate : ~BarCommandOptions.DoNothing;
                CommandInfo info = commandInfo;
                this.InvokeCommand(info);
                return info.ReturnInt32;
              }
            }
          }
        }
      }
      finally
      {
        this.InModal = false;
        if (isTopMost)
          this.ToggleTopMost();
      }
      return -1;
    }

    public void DeleteSelection(bool fCurrentFolder, bool fNuke, TargetView targetView = TargetView.Default)
    {
      FileOperationOptions opt = fNuke ? FileOperationOptions.Nuke : FileOperationOptions.Normal;
      CustomViewBase view = this.GetView(targetView);
      if (view == null)
        return;
      if (fCurrentFolder)
      {
        using (ShellItem shellItem = view.ShellItem)
        {
          if (shellItem.CanDelete)
          {
            if (!ShellFileOperation.Delete(shellItem.ShellItemArray, this.hwndExplorer, opt))
              return;
            FileOperationSound.Play(OperationSound.Delete);
          }
          else
            SystemSounds.Beep.Play2();
        }
      }
      else
      {
        IShellItemArray selectionArray = view.GetSelectionArray();
        try
        {
          if (selectionArray != null)
          {
            if (ShellFileOperation.Delete(selectionArray, this.hwndExplorer, opt))
            {
              FileOperationSound.Play(OperationSound.Delete);
              return;
            }
          }
        }
        finally
        {
          if (selectionArray != null)
            Marshal.ReleaseComObject((object) selectionArray);
        }
        SystemSounds.Beep.Play2();
      }
    }

    public void CopyFrom(
      FileOperationVerb effect,
      ShellItem shellItemInitial = null,
      TargetView targetView = TargetView.Default)
    {
      this.InModal = true;
      try
      {
        CustomViewBase view = this.GetView(targetView);
        if (view == null)
          return;
        using (ShellItem shellItem = view.ShellItem)
          ShellFileOperation.CopyFrom(shellItem, this.hwndExplorer, effect, shellItemInitial);
      }
      finally
      {
        this.InModal = false;
      }
    }

    private void NotifyBrowserRefresh(bool fFocus) { this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco { cco.ClearFilterBox(fFocus)));

    public void MoveWindow(Point pnt, int width = 0, int height = 0)
    {
      Rectangle workingArea = Screen.FromPoint(pnt).WorkingArea;
      Rectangle windowRect = PInvoke.GetWindowRect(this.hwndExplorer);
      if (pnt.X + windowRect.Width > workingArea.Right)
        pnt.X -= pnt.X + windowRect.Width - workingArea.Right;
      if (pnt.Y + windowRect.Height > workingArea.Bottom)
        pnt.Y -= pnt.Y + windowRect.Height - workingArea.Bottom;
      SWP uFlags = SWP.NOZORDER | SWP.NOACTIVATE;
      if (width == 0 && height == 0)
        uFlags |= SWP.NOSIZE;
      PInvoke.SetWindowPos(this.hwndExplorer, IntPtr.Zero, pnt.X, pnt.Y, width, height, uFlags);
    }

    private void MoveWindowToScreen(Rectangle preferredScreen)
    {
      Rectangle windowRect = PInvoke.GetWindowRect(this.hwndExplorer);
      if (preferredScreen.IntersectsWith(windowRect))
        return;
      Rectangle workingArea = Screen.FromPoint(windowRect.Location).WorkingArea;
      double num1 = Math.Max(0.0, (double) (windowRect.X - workingArea.X) / (double) workingArea.Width);
      double num2 = Math.Max(0.0, (double) (windowRect.Y - workingArea.Y) / (double) workingArea.Height);
      if (this.Dpi > DpiManager.GetDpiFromPoint(preferredScreen.Location))
      {
        Point point = new Point(preferredScreen.X + preferredScreen.Width / 2, preferredScreen.Y + preferredScreen.Height / 2);
        Rectangle rectangle = new Rectangle(point.X - windowRect.Width / 2, point.Y - windowRect.Height / 2, windowRect.Width, windowRect.Height);
        if (rectangle.Width > preferredScreen.Width)
          rectangle.Inflate(preferredScreen.Width - rectangle.Width, 0);
        if (rectangle.Height > preferredScreen.Height)
          rectangle.Inflate(0, preferredScreen.Height - rectangle.Height);
        this.MoveWindow(rectangle.Location, rectangle.Width, rectangle.Height);
      }
      else
        this.MoveWindow(new Point(preferredScreen.X + (int) ((double) preferredScreen.Width * num1), preferredScreen.Y + (int) ((double) preferredScreen.Height * num2)));
    }

    private void CopySelectionFromFocusedViewTo(TargetView targetView, FileOperationVerb verb)
    {
      TargetView targetView1;
      CustomViewBase focusedView = this.GetFocusedView(out targetView1);
      if (focusedView == null)
        return;
      if (targetView == TargetView.Other)
        targetView = this.Toolbars.GetTheOtherView(targetView1);
      if (targetView == targetView1)
        return;
      CustomViewBase view = this.GetView(targetView);
      if (view == null)
        return;
      focusedView.CopySelectionTo(view, verb);
    }

    private bool CaptureByExtraView(
      IList<byte[]> idls,
      TargetView targetView,
      BarCommandOptions opt)
    {
      if (idls == null || idls.Count <= 0 || !ExplorerManager.SanitizeForExtraView(ref idls))
        return false;
      if (this.Toolbars.IsExtraViewVisible(targetView))
      {
        if (opt.HasFlag((Enum) BarCommandOptions.Async))
        {
          Task.Factory.StartNew((Action) (() { Thread.Sleep(100))).ContinueWith((Action<Task>) (t {
          {
            this.Toolbars.GetExtraViewBar(targetView).InvokeCommand(new CommandInfo(BarCommand.NewTabs, idls, opt | BarCommandOptions.IgnoreModKeys));
            if (WindowUtil.BringExplorerToFront(this.hwndExplorer))
              Application.DoEvents();
            this.SetFocusTo(targetView == TargetView.Second ? FocusableWindow.SecondView : FocusableWindow.ThirdView);
          }), TaskScheduler.FromCurrentSynchronizationContext());
          return true;
        }
        this.Toolbars.GetExtraViewBar(targetView).InvokeCommand(new CommandInfo(BarCommand.NewTabs, idls, opt | BarCommandOptions.IgnoreModKeys));
      }
      else
      {
        if (opt.HasFlag((Enum) BarCommandOptions.StartUpSelection))
          this.StartUpSelectionPendingExtraView = true;
        this.PendingFoldersExtraView = (IList<ItemIDList>) idls.Select<byte[], ItemIDList>((Func<byte[], ItemIDList>) (idl { (ItemIDList) idl)).ToList<ItemIDList>();
        this.Toolbars.ShowExtraView(targetView, true);
      }
      if (WindowUtil.BringExplorerToFront(this.hwndExplorer))
        Application.DoEvents();
      this.SetFocusTo(targetView == TargetView.Second ? FocusableWindow.SecondView : FocusableWindow.ThirdView);
      return true;
    }

    private bool CaptureByExtraView(
      IList<string> groups,
      BarCommandOptions opt,
      TargetView targetView)
    {
      if (groups == null || groups.Count <= 0)
        return false;
      if (this.Toolbars.IsExtraViewVisible(targetView))
      {
        if (opt.HasFlag((Enum) BarCommandOptions.Async))
        {
          Task.Factory.StartNew((Action) (() { Thread.Sleep(100))).ContinueWith((Action<Task>) (t {
          {
            this.Toolbars.GetExtraViewBar(targetView).OpenCapturedGroups(groups, opt);
            if (WindowUtil.BringExplorerToFront(this.hwndExplorer))
              Application.DoEvents();
            this.SetFocusTo(targetView == TargetView.Second ? FocusableWindow.SecondView : FocusableWindow.ThirdView);
          }), TaskScheduler.FromCurrentSynchronizationContext());
          return true;
        }
        this.Toolbars.GetExtraViewBar(targetView).OpenCapturedGroups(groups, opt);
      }
      else
      {
        this.PendingGroupsExtraView = groups;
        this.Toolbars.ShowExtraView(targetView, true);
      }
      if (WindowUtil.BringExplorerToFront(this.hwndExplorer))
        Application.DoEvents();
      this.SetFocusTo(targetView == TargetView.Second ? FocusableWindow.SecondView : FocusableWindow.ThirdView);
      return true;
    }

    private static bool SanitizeForExtraView(ref IList<byte[]> idls)
    {
      List<byte[]> numArrayList = new List<byte[]>();
      for (int index = 0; index < idls.Count; ++index)
      {
        if (!QSecondViewBar.IsUnnavigatablePath((ItemIDList) idls[index]))
          numArrayList.Add(idls[index]);
      }
      idls = (IList<byte[]>) numArrayList;
      return idls.Count > 0;
    }

    private static bool CommandWantView(CommandInfo info) { info.Scripting || info.Command == BarCommand.Group || info.Command == BarCommand.NewTab || info.Command == BarCommand.NewTabAt || info.Command == BarCommand.ReplaceByGroup;

    private bool NavigateInPlaceExtraView(byte[] idl, BarCommandOptions opt, TargetView targetView)
    {
      if (!this.Toolbars.IsExtraViewVisible(targetView))
        return false;
      QSecondViewBar extraViewBar = this.Toolbars.GetExtraViewBar(targetView);
      if (extraViewBar.IsActiveTabNavigatable)
        return false;
      if (opt.HasFlag((Enum) BarCommandOptions.Async))
      {
        Task.Factory.StartNew((Action) (() { Thread.Sleep(100))).ContinueWith((Action<Task>) (t {
        {
          QSecondViewBar qsecondViewBar = extraViewBar;
          qsecondViewBar.InvokeCommand(new CommandInfo(BarCommand.CloneTab, BarCommandArg.Current, BarCommandOptions.Activate | BarCommandOptions.IgnoreModKeys)
          {
            IDL = idl
          });
          this.SetFocusTo(targetView == TargetView.Second ? FocusableWindow.SecondView : FocusableWindow.ThirdView);
        }), TaskScheduler.FromCurrentSynchronizationContext());
      }
      else
      {
        QSecondViewBar qsecondViewBar = extraViewBar;
        CommandInfo info = new CommandInfo(BarCommand.CloneTab, BarCommandArg.Current, BarCommandOptions.Activate | BarCommandOptions.IgnoreModKeys);
        info.IDL = idl;
        qsecondViewBar.InvokeCommand(info);
      }
      return true;
    }

    IntPtr ICommandInvokerWindow.CommandWindowHandle { this.ExplorerHandle;

    public bool ShowWindow(SHOWWINDOW showWindow) { PInvoke.ShowWindow(this.hwndExplorer, showWindow);

    public void BringToFront()
    {
      if (!this.fFirstNavigationComplete)
        return;
      WindowUtil.BringExplorerToFront(this.hwndExplorer);
    }

    public ExplorerWindowState WindowState
    {
      get
      {
        if (PInvoke.IsZoomed(this.hwndExplorer))
          return ExplorerWindowState.Maximized;
        if (PInvoke.IsIconic(this.hwndExplorer))
          return ExplorerWindowState.Minimized;
        if (this.IsInTray())
          return ExplorerWindowState.MinimizedToTaskTray;
        return this.fHorizontalyMaximized ? ExplorerWindowState.HorizontallyMaximized : ExplorerWindowState.Normal;
      }
    }

    public Rectangle Bounds { PInvoke.GetWindowRect(this.hwndExplorer);

    private void SetTopMost(bool fTopMost) { PInvoke.SetWindowPos(this.hwndExplorer, fTopMost ? W32.HWND_TOPMOST : W32.HWND_NOTOPMOST, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

    internal bool ToggleTopMost()
    {
      bool isTopMost = this.IsTopMost;
      this.SetTopMost(!isTopMost);
      Config.Set(Scts.ExplorerTopMost, !isTopMost);
      if (Config.Bool(Scts.SaveExplorerTopMost))
        Config.Save();
      return !isTopMost;
    }

    private void MaximizeHorizontally(Screen screen)
    {
      if (screen == null)
        screen = Screen.FromHandle(this.hwndExplorer);
      Rectangle windowRect = PInvoke.GetWindowRect(this.hwndExplorer);
      Rectangle workingArea = screen.WorkingArea;
      if (!this.fHorizontalyMaximized)
      {
        if (windowRect.Y == workingArea.Y && windowRect.Bottom == workingArea.Bottom && !PInvoke.IsZoomed(this.hwndExplorer))
        {
          PInvoke.ShowWindow(this.hwndExplorer, SHOWWINDOW.SHOWMAXIMIZED);
        }
        else
        {
          this.rctHorizontalyMaximizing = windowRect;
          this.rctHorizontalyMaximized = new Rectangle(workingArea.X, windowRect.Y, workingArea.Width, windowRect.Height);
          PInvoke.SetWindowPos(this.hwndExplorer, IntPtr.Zero, workingArea.X, windowRect.Y, workingArea.Width, windowRect.Height, SWP.NOZORDER);
          this.fHorizontalyMaximized = true;
        }
      }
      else
      {
        this.fHorizontalyMaximized = false;
        PInvoke.SetWindowPos(this.hwndExplorer, IntPtr.Zero, this.rctHorizontalyMaximizing.X, this.rctHorizontalyMaximizing.Y, this.rctHorizontalyMaximizing.Width, this.rctHorizontalyMaximizing.Height, SWP.NOZORDER);
      }
    }

    public bool IsTopMost { WindowUtil.IsTopmost(this.hwndExplorer);

    public bool FocusListView(TargetView targetView = TargetView.Default)
    {
      CustomViewBase view = this.GetView(targetView);
      return view != null && IntPtr.Zero != PInvoke.SetFocus(WindowUtil.GetListView(view.ShellBrowser));
    }

    public bool FocusFolderTree()
    {
      IntPtr treeView = WindowUtil.GetTreeView(this.shellBrowser);
      return treeView != IntPtr.Zero && IntPtr.Zero != PInvoke.SetFocus(treeView);
    }

    public bool SetFocusTo(FocusableWindow window)
    {
      switch (window)
      {
        case FocusableWindow.FolderView:
          return this.FocusListView();
        case FocusableWindow.Tree:
          return this.FocusFolderTree();
        case FocusableWindow.Tab:
          if (this.Toolbars.IsTabBarAvailable)
          {
            this.Toolbars.TabBar.FocusBar();
            return true;
          }
          break;
        case FocusableWindow.VerticalBar:
          if (this.Toolbars.CommandBarVrtHandle != IntPtr.Zero)
          {
            this.ExpandToolbar(Toolbar.CommandBarVertical);
            PInvoke.SendMessage(this.Toolbars.CommandBarVrtHandle, 32771, IntPtr.Zero, IntPtr.Zero);
            return true;
          }
          break;
        case FocusableWindow.SearchBar:
          return IntPtr.Zero != PInvoke.SetFocus(WindowUtil.GetSearchBox(this.hwndExplorer));
        case FocusableWindow.FilterBox:
          return this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.FocusFilterBox()));
        case FocusableWindow.AddressBar:
          PInvoke.PostMessage(this.hwndListView, 260, 68, 538968065);
          return true;
        case FocusableWindow.SecondView:
          if (this.Toolbars.IsSecondViewBarAvailable && this.Toolbars.SecondViewBar.IsActiveTabAvailable)
            this.Toolbars.SecondViewBar.ActiveTab.View.Focus();
          return true;
        case FocusableWindow.ThirdView:
          if (this.Toolbars.IsThirdViewBarAvailable && this.Toolbars.ThirdViewBar.IsActiveTabAvailable)
            this.Toolbars.ThirdViewBar.ActiveTab.View.Focus();
          return true;
      }
      return false;
    }

    private bool FocusNextView()
    {
      if (this.View.Focused)
      {
        if (WindowUtil.EditInViewHasFocus(this.hwndListView, this.View.VistaLayout))
          return false;
        if (this.Toolbars.IsSecondViewBarTabAvailable)
        {
          this.Toolbars.SecondViewBar.ActiveTab.View.Focus();
          return true;
        }
        if (this.Toolbars.IsThirdViewBarTabAvailable)
        {
          this.Toolbars.ThirdViewBar.ActiveTab.View.Focus();
          return true;
        }
      }
      else
      {
        if (this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.Focused)
        {
          if (WindowUtil.EditInViewHasFocus(this.Toolbars.SecondViewBar.ActiveTab.View.ListViewHandle, this.Toolbars.SecondViewBar.ActiveTab.View.VistaLayout))
            return false;
          if (this.Toolbars.IsThirdViewBarTabAvailable)
          {
            this.Toolbars.ThirdViewBar.ActiveTab.View.Focus();
            return true;
          }
          this.View.Focus();
          return true;
        }
        if (this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.Focused && !WindowUtil.EditInViewHasFocus(this.Toolbars.ThirdViewBar.ActiveTab.View.ListViewHandle, this.Toolbars.ThirdViewBar.ActiveTab.View.VistaLayout))
        {
          this.View.Focus();
          return true;
        }
      }
      return false;
    }

    public bool InModal
    {
      get { this.modalCounter > 0;
      set
      {
        if (value)
        {
          ++this.modalCounter;
        }
        else
        {
          --this.modalCounter;
          if (this.modalCounter >= 0)
            return;
          this.modalCounter = 0;
        }
      }
    }

    private void InitializeWindow()
    {
      if (Config.Bool(Scts.SaveWindowTransparency) && Config.WindowAlpha < byte.MaxValue)
      {
        PInvoke.SetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE), 524288));
        PInvoke.SetLayeredWindowAttributes(this.hwndExplorer, 0, Config.WindowAlpha, 2);
      }
      if (Config.Bool(Scts.LockWindowSize))
        PInvoke.SetWindowLongPtr(this.hwndExplorer, GWL.STYLE, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.STYLE), 4294705151U));
      try
      {
        using (RegistryKey registryKey = RegistryUtil.OpenUserRoot(true))
        {
          if (registryKey != null)
          {
            if (registryKey.GetValue("TabDroppedLocation") is byte[] input2 && input2.Length == 8)
            {
              int[] int32 = RegistryUtil.ByteToInt32(input2);
              if (int32 != null && int32.Length == 2)
                this.MoveWindow(new Point(int32[0], int32[1]));
            }
            else if (registryKey.GetValue("PreferredScreenBounds") is byte[] input1 && input1.Length == 16)
            {
              int[] int32 = RegistryUtil.ByteToInt32(input1);
              if (int32 != null && int32.Length == 4)
                this.MoveWindowToScreen(new Rectangle(int32[0], int32[1], int32[2], int32[3]));
            }
            registryKey.SetValue("PreferredScreenBounds", (object) new byte[0]);
            registryKey.SetValue("TabDroppedLocation", (object) new byte[0]);
          }
        }
      }
      catch (Exception ex)
      {
      }
      this.ResetShrinkTimers(ShrinkTimerModes.None);
    }

    public bool FirstNavigationComplete { this.fFirstNavigationComplete;

    public void SetWindowTransparency(bool fPlus)
    {
      if (IntPtr.Zero == PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE), 524288))
      {
        if (fPlus)
          return;
        PInvoke.SetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE), 524288));
        PInvoke.SetLayeredWindowAttributes(this.hwndExplorer, 0, byte.MaxValue, 2);
      }
      byte bAlpha;
      if (!PInvoke.GetLayeredWindowAttributes(this.hwndExplorer, out int _, out bAlpha, out int _))
        return;
      bAlpha = !fPlus ? (byte) Math.Max(20, (int) bAlpha - 12) : (byte) Math.Min((int) byte.MaxValue, (int) bAlpha + 12);
      PInvoke.SetLayeredWindowAttributes(this.hwndExplorer, 0, bAlpha, 2);
      this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco {
      {
        if (!(cco is BandObject bandObject2))
          return;
        PInvoke.SendMessage(bandObject2.Handle, 32770, (IntPtr) (int) bAlpha, IntPtr.Zero);
      }));
      if (bAlpha != byte.MaxValue)
        return;
      PInvoke.SetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.EXSTYLE), -524289));
    }

    private bool SetFocusOnOpening()
    {
      int val = Config.Get(Scts.FocusOnStartUp);
      int num = Config.Get(Scts.CapturingView);
      if (num > 0 && val == 0)
        val = num == 1 ? 4 : 5;
      if (val == 0)
        return false;
      bool fResult = true;
      Task.Factory.StartNew((Action) (() { Thread.Sleep(250))).ContinueWith((Action<Task>) (t {
      {
        try
        {
          switch (val)
          {
            case 1:
              fResult = this.SetFocusTo(FocusableWindow.Tree);
              break;
            case 2:
              fResult = this.SetFocusTo(FocusableWindow.AddressBar);
              break;
            case 3:
              fResult = this.SetFocusTo(FocusableWindow.SearchBar);
              break;
            case 4:
              fResult = this.SetFocusTo(FocusableWindow.SecondView);
              break;
            case 5:
              fResult = this.SetFocusTo(FocusableWindow.ThirdView);
              break;
          }
        }
        catch (Exception ex)
        {
        }
      }), TaskScheduler.FromCurrentSynchronizationContext());
      return fResult;
    }

    public void MergeAll()
    {
      this.RestoreFromTray();
      ExplorerManager.fMergingAllWindow = true;
      foreach (ExplorerManager explorerManagerInstance in (IEnumerable<ExplorerManager>) InstanceManager.ExplorerManagerInstances)
      {
        if (explorerManagerInstance != this)
        {
          if (this.Toolbars.IsTabBarAvailable && explorerManagerInstance.Toolbars.IsTabBarAvailable)
            this.Toolbars.TabBar.MergeFrom((TabBarBase) explorerManagerInstance.Toolbars.TabBar);
          if (this.Toolbars.IsSecondViewBarTabAvailable && explorerManagerInstance.Toolbars.IsSecondViewBarTabAvailable)
            this.Toolbars.SecondViewBar.MergeFrom((TabBarBase) explorerManagerInstance.Toolbars.SecondViewBar);
          if (this.Toolbars.IsThirdViewBarTabAvailable && explorerManagerInstance.Toolbars.IsThirdViewBarTabAvailable)
            this.Toolbars.ThirdViewBar.MergeFrom((TabBarBase) explorerManagerInstance.Toolbars.ThirdViewBar);
          WindowUtil.CloseExplorer(explorerManagerInstance.ExplorerHandle, WMCLOSEARG.CloseWindowWithoutSaving);
        }
      }
    }

    private void ShowCurrentHistoryMenu()
    {
      if (this.Toolbars.IsTabBarAvailable)
      {
        if (!this.Toolbars.TabBar.TabManager.IsActiveTabAvailable)
          return;
        this.Toolbars.TabBar.ShowCurrentHistoryMenu();
      }
      else
        QUIAutomation.PressToolbarButton(WindowUtil.GetTravelToolbar(this.hwndExplorer), "Item 258");
    }

    private bool OnTabKeyWhileRenaming(bool fShift)
    {
      IntPtr focus = PInvoke.GetFocus();
      if (focus != IntPtr.Zero)
      {
        using (IMEDetector imeDetector = new IMEDetector(focus))
        {
          if (imeDetector.IsOpen)
            return false;
        }
        if (this.View.OnTabKeyWhileRenaming(focus, fShift) || this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.OnTabKeyWhileRenaming(focus, fShift) || this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.OnTabKeyWhileRenaming(focus, fShift))
          return true;
      }
      return false;
    }

    private bool OnTabKeyOnEdit()
    {
      IntPtr focus = PInvoke.GetFocus();
      if (!(focus != IntPtr.Zero) || !(Control.FromHandle(focus) is TextBox textBox) || !textBox.AcceptsTab)
        return false;
      textBox.SelectedText = '\t'.ToString();
      return true;
    }

    private unsafe void OnWindowPosChanging(WINDOWPOS* wp)
    {
      if (wp->flags.HasFlag((Enum) SWP.NOSIZE) || wp->x <= -32000)
        return;
      Rectangle bounds = this.Bounds;
      if (bounds.X <= -32000 || bounds.Width == wp->cx && bounds.Height == wp->cy)
        return;
      this.extraViewResizer.OnExplorerResizing(new Size(wp->cx, wp->cy), bounds.Size, wp->flags.HasFlag((Enum) SWP.STATECHANGED), wp->flags.HasFlag((Enum) SWP.SHOWWINDOW));
    }

    private void OnExplorerActivating()
    {
      this.RegisterPrimaryExplorerManager(false);
      this.RestoreFromTray();
    }

    private void OnExplorerDeactivating(Message msg)
    {
      this.View.HidePreviewTip();
      PreviewTip.Suppress();
      this.View.HideSubDirTip(false, fExplorerInactivated: true);
      this.HideTabSwitcher(false);
      if (this.ExplorerManagerEvent == null)
        return;
      this.ExplorerManagerEvent((object) this, new ExplorerManagerEventArgs(ExplorerManagerEventKind.WindowDeactivated, msg));
    }

    private TargetView PreviousActiveTargetView { get; set; }

    public int Dpi { get; private set; }

    private void NotifyDpiChange(int oldDpi) { this.Toolbars.AllBandObjects.ForEach<BandObject>((Action<BandObject>) (bo { bo.NotifyDpiChanged(oldDpi, ExplorerManager.WindowDpi)));

    public Rebar Rebar
    {
      get
      {
        if (this.rebar == null)
          this.rebar = new Rebar(this.bandSite, this.hwndRebar);
        return this.rebar;
      }
    }

    public bool IsFolderTreeVisible
    {
      get
      {
        IntPtr phwnd;
        return this.shellBrowser.GetControlWindow(3U, out phwnd) == 0 && PInvoke.IsWindowVisible(phwnd);
      }
    }

    public bool NowTogglingTabBar { get; set; }

    public bool IsToolbarVisible(Toolbar toolbar)
    {
      switch (toolbar)
      {
        case Toolbar.TabBar:
          return this.Toolbars.IsTabBarVisible;
        case Toolbar.CommandBar1:
          return this.Toolbars.IsCommandBar1stVisible;
        case Toolbar.CommandBar2:
          return this.Toolbars.IsCommandBar2ndVisible;
        case Toolbar.CommandBarVertical:
          return this.Toolbars.IsCommandBarVrtVisible;
        case Toolbar.BottomTabBar:
          return this.Toolbars.IsBottomTabBarVisible;
        case Toolbar.SecondViewBar:
          return this.Toolbars.IsSecondViewBarVisible;
        case Toolbar.ThirdViewBar:
          return this.Toolbars.IsThirdViewBarVisible;
        default:
          return false;
      }
    }

    private bool CommandBarKeyTargetExist { this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.DropdownHasKeyFocus));

    public static bool TabBarEnabled
    {
      get
      {
        using (RegistryKey rk = RegistryUtil.OpenUserRoot())
        {
          if (rk != null)
            return RegistryUtil.GetValueSafe<int>(rk, nameof (TabBarEnabled), 0) != 0;
        }
        return false;
      }
      set
      {
        using (RegistryKey registryKey = RegistryUtil.OpenUserRoot(true))
          registryKey?.SetValue(nameof (TabBarEnabled), (object) (value ? 1 : 0));
      }
    }

    public IntPtr RebarWindow { this.hwndRebar;

    public void OnTabBarEnabledChanged()
    {
      ExplorerManager.RefreshStaticVerbs();
      QGroupOpener.InitializeUser();
    }

    public void OnCommandBarItemsCreated(ICommandControlOwner commandControlOwner) { this.SyncTabBarAndCommandBars(BuiltinButtonRefreshes.CloseButtons | BuiltinButtonRefreshes.TravelButtons | BuiltinButtonRefreshes.Topmost | BuiltinButtonRefreshes.DisableGoUpOneLevel, commandBarSpec: commandControlOwner);

    public void SyncTabBarAndCommandBars(
      BuiltinButtonRefreshes mask,
      BuiltinButtonRefreshSource source = BuiltinButtonRefreshSource.All,
      ICommandControlOwner commandBarSpec = null)
    {
      if (source.HasFlag((Enum) BuiltinButtonRefreshSource.Default))
      {
        BuiltinButtonRefreshes flags;
        if (this.Toolbars.IsTabBarAvailable)
        {
          flags = this.Toolbars.TabBar.MakeSyncFlagsFromMask(mask);
        }
        else
        {
          mask |= BuiltinButtonRefreshes.NoTabBar;
          flags = this.MakeSyncFlagsFromMaskWithoutTabBar(mask);
        }
        this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco {
        {
          if (commandBarSpec != null && commandBarSpec != cco)
            return;
          cco.RefreshBarState(flags, mask, BuiltinButtonRefreshSource.Default);
        }));
      }
      if (source.HasFlag((Enum) BuiltinButtonRefreshSource.Second) && this.Toolbars.IsSecondViewBarAvailable)
      {
        BuiltinButtonRefreshes flags = this.Toolbars.SecondViewBar.MakeSyncFlagsFromMask(mask);
        this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco {
        {
          if (commandBarSpec != null && commandBarSpec != cco)
            return;
          cco.RefreshBarState(flags, mask, BuiltinButtonRefreshSource.Second);
        }));
      }
      if (!source.HasFlag((Enum) BuiltinButtonRefreshSource.Third) || !this.Toolbars.IsThirdViewBarAvailable)
        return;
      BuiltinButtonRefreshes flags1 = this.Toolbars.ThirdViewBar.MakeSyncFlagsFromMask(mask);
      this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco {
      {
        if (commandBarSpec != null && commandBarSpec != cco)
          return;
        cco.RefreshBarState(flags1, mask, BuiltinButtonRefreshSource.Third);
      }));
    }

    private BuiltinButtonRefreshes MakeSyncFlagsFromMaskWithoutTabBar(
      BuiltinButtonRefreshes mask)
    {
      BuiltinButtonRefreshes builtinButtonRefreshes = (BuiltinButtonRefreshes) (0 | 32);
      if (mask.HasFlag((Enum) BuiltinButtonRefreshes.CloseWindow))
        builtinButtonRefreshes |= BuiltinButtonRefreshes.CloseWindow;
      if (this.IsTopMost)
        builtinButtonRefreshes |= BuiltinButtonRefreshes.Topmost;
      if (mask.HasFlag((Enum) BuiltinButtonRefreshes.DisableGoUpOneLevel) && this.View.ItemIDList.IsRoot)
        builtinButtonRefreshes |= BuiltinButtonRefreshes.DisableGoUpOneLevel;
      return builtinButtonRefreshes;
    }

    public void OpenInTabBar(IList<byte[]> idls, TargetView targetView, BarCommandOptions opt)
    {
      if (targetView == TargetView.Default)
      {
        if (this.Toolbars.IsTabBarVisible)
        {
          if (opt.HasFlag((Enum) BarCommandOptions.Async))
          {
            Task.Factory.StartNew((Action) (() { Thread.Sleep(100))).ContinueWith((Action<Task>) (t {
            {
              this.InvokeCommand(new CommandInfo(BarCommand.NewTabs, idls, opt | BarCommandOptions.IgnoreModKeys));
              this.BringToFront();
            }), TaskScheduler.FromCurrentSynchronizationContext());
            return;
          }
          this.InvokeCommand(new CommandInfo(BarCommand.NewTabs, idls, opt | BarCommandOptions.IgnoreModKeys));
        }
        else
        {
          this.PendingFoldersDefault = (IList<ItemIDList>) idls.ToItemIDList().ToList<ItemIDList>();
          this.Toolbars.Show(Config.Bool(Scts.TabBarPriority) ? Toolbar.BottomTabBar : Toolbar.TabBar, true);
        }
        this.BringToFront();
      }
      else
        this.CaptureByExtraView(idls, targetView, opt);
    }

    private void OpenGroupsInTabBar(
      string[] groups,
      BarCommandOptions options,
      TargetView targetView)
    {
      if (targetView == TargetView.Default)
      {
        if (this.Toolbars.IsTabBarVisible)
        {
          if (options.HasFlag((Enum) BarCommandOptions.Async))
          {
            Task.Factory.StartNew((Action) (() { Thread.Sleep(100))).ContinueWith((Action<Task>) (t {
            {
              this.InvokeCommand(new CommandInfo(BarCommand.Group, groups, BarCommandOptions.IgnoreModKeys));
              this.BringToFront();
            }), TaskScheduler.FromCurrentSynchronizationContext());
            return;
          }
          this.InvokeCommand(new CommandInfo(BarCommand.Group, groups, BarCommandOptions.IgnoreModKeys));
        }
        else
        {
          this.PendingGroupsDefault = (IList<string>) groups;
          this.Toolbars.Show(Config.Bool(Scts.TabBarPriority) ? Toolbar.BottomTabBar : Toolbar.TabBar, true);
        }
        this.BringToFront();
      }
      else
        this.CaptureByExtraView((IList<string>) groups, options, targetView);
    }

    public string SearchBoxText
    {
      get { QUIAutomation.GetSearchBoxText(WindowUtil.GetSearchBox(this.hwndExplorer));
      set { QUIAutomation.SetSearchBoxText(WindowUtil.GetSearchBox(this.hwndExplorer), value);
    }

    public IEnumerable<QTabItem> AllTabs(TargetView tvFirst = TargetView.Default)
    {
      bool fDefaultComplete = false;
      switch (tvFirst)
      {
        case TargetView.Second:
          foreach (QTabItem enumTab in this.enumTabs(TargetView.Second))
            yield return enumTab;
          break;
        case TargetView.Third:
          foreach (QTabItem enumTab in this.enumTabs(TargetView.Third))
            yield return enumTab;
          break;
        default:
          foreach (QTabItem enumTab in this.enumTabs(TargetView.Default))
            yield return enumTab;
          fDefaultComplete = true;
          break;
      }
      if (!fDefaultComplete)
      {
        foreach (QTabItem enumTab in this.enumTabs(TargetView.Default))
          yield return enumTab;
      }
      if (tvFirst != TargetView.Second)
      {
        foreach (QTabItem enumTab in this.enumTabs(TargetView.Second))
          yield return enumTab;
      }
      if (tvFirst != TargetView.Third)
      {
        foreach (QTabItem enumTab in this.enumTabs(TargetView.Third))
          yield return enumTab;
      }
    }

    private IEnumerable<QTabItem> enumTabs(TargetView targetView)
    {
      TabBarBase tabBar = this.Toolbars.GetTabBar(targetView);
      return tabBar != null ? tabBar.TabManager.Items : Enumerable.Empty<QTabItem>();
    }

    private bool IsNavigationWindow(IntPtr hwnd, out NavigationPane navigationPane)
    {
      navigationPane = (NavigationPane) null;
      if (hwnd != IntPtr.Zero)
      {
        if (hwnd == this.NavigationPane.Handle)
        {
          navigationPane = this.NavigationPane;
          return true;
        }
        IntPtr num1 = hwnd;
        IntPtr? handle1 = this.toolbarManager.SecondViewBar?.ActiveView?.NavigationPane?.Handle;
        if ((handle1.HasValue ? (num1 == handle1.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        {
          navigationPane = this.toolbarManager.SecondViewBar.ActiveView.NavigationPane;
          return true;
        }
        IntPtr num2 = hwnd;
        IntPtr? handle2 = this.toolbarManager.ThirdViewBar?.ActiveView?.NavigationPane?.Handle;
        if ((handle2.HasValue ? (num2 == handle2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        {
          navigationPane = this.toolbarManager.ThirdViewBar.ActiveView.NavigationPane;
          return true;
        }
      }
      return false;
    }

    private bool IsNavigationWindowHasFocus(out NavigationPane navigationPane)
    {
      navigationPane = (NavigationPane) null;
      if (this.NavigationPane.Focused)
      {
        navigationPane = this.NavigationPane;
        return true;
      }
      bool? focused1 = this.toolbarManager.SecondViewBar?.ActiveView?.NavigationPane?.Focused;
      if (1 == (focused1.GetValueOrDefault() ? 1 : 0) & focused1.HasValue)
      {
        navigationPane = this.toolbarManager.SecondViewBar.ActiveView.NavigationPane;
        return true;
      }
      bool? focused2 = this.toolbarManager.ThirdViewBar?.ActiveView?.NavigationPane?.Focused;
      if (!(1 == (focused2.GetValueOrDefault() ? 1 : 0) & focused2.HasValue))
        return false;
      navigationPane = this.toolbarManager.ThirdViewBar.ActiveView.NavigationPane;
      return true;
    }

    private void SaveCurrentBandLayout()
    {
      try
      {
        if (this.bandSite == null || this.AnyToolbarShrunken)
          return;
        using (RegistryKey subKey = Registry.CurrentUser.CreateSubKey("Software\\Quizo\\QTTabBar\\BandLayout"))
        {
          Dictionary<int, Dictionary<int, REBARBANDINFO>> bandInfoDic = ExplorerManager.createBandInfoDic(this.hwndRebar);
          int[] array1 = new int[bandInfoDic.Keys.Count];
          bandInfoDic.Keys.CopyTo(array1, 0);
          Array.Sort<int>(array1);
          for (int row = 0; row < array1.Length; ++row)
          {
            Dictionary<int, REBARBANDINFO> dictionary = bandInfoDic[array1[row]];
            int[] array2 = new int[dictionary.Keys.Count];
            dictionary.Keys.CopyTo(array2, 0);
            Array.Sort<int>(array2);
            for (int index = 0; index < array2.Length; ++index)
            {
              REBARBANDINFO rebarbandinfo = dictionary[array2[index]];
              object ppv = (object) null;
              try
              {
                if (this.bandSite.GetBandObject(rebarbandinfo.wID, COMGUIDS.IID_IDeskBand, out ppv) == 0)
                {
                  if (ppv is BandObject bandObject)
                  {
                    if (!bandObject.Visible)
                      continue;
                  }
                  if (ppv is IPersistStream persistStream)
                  {
                    Guid pClassID;
                    if (persistStream.GetClassID(out pClassID) == 0)
                      ExplorerManager.SaveBandLayout(subKey, pClassID.ToString("B"), row, index);
                  }
                }
              }
              catch (Exception ex)
              {
              }
              finally
              {
                QMarshal.ReleaseComObject(ppv);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private int GetBandLayoutFromID(int id)
    {
      if (this.bandSite != null)
      {
        object ppv = (object) null;
        try
        {
          using (RegistryKey rk = RegistryUtil.OpenUserRoot("BandLayout", fCreate: true))
          {
            if (rk != null)
            {
              if (this.bandSite.GetBandObject(id, COMGUIDS.IID_IMenuBand, out ppv) == 0)
                return RegistryUtil.GetValueSafe<int>(rk, "Menu Bar", 0);
              if (this.bandSite.GetBandObject(id, COMGUIDS.IID_IDeskBand, out ppv) == 0)
              {
                if (ppv is IPersistStream persistStream)
                {
                  Guid pClassID;
                  if (persistStream.GetClassID(out pClassID) == 0)
                    return RegistryUtil.GetValueSafe<int>(rk, pClassID.ToString("B"), 0);
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
        }
        finally
        {
          QMarshal.ReleaseComObject(ppv);
        }
      }
      return -1;
    }

    [Conditional("DBUG")]
    public void ClearBandLayout(Guid guid)
    {
    }

    private static void SaveBandLayout(RegistryKey rk, string keyName, int row, int index) { rk?.SetValue(keyName, (object) (row << 16 | index & (int) ushort.MaxValue));

    private static unsafe Dictionary<int, Dictionary<int, REBARBANDINFO>> createBandInfoDic(
      IntPtr hwndRebar)
    {
      Dictionary<int, Dictionary<int, REBARBANDINFO>> bandInfoDic = new Dictionary<int, Dictionary<int, REBARBANDINFO>>();
      REBARBANDINFO* lParam1 = stackalloc REBARBANDINFO[1];
      lParam1->cbSize = sizeof (REBARBANDINFO);
      lParam1->fMask = 272;
      RECT* lParam2 = stackalloc RECT[1];
      int num = (int) PInvoke.SendMessage(hwndRebar, 1036, IntPtr.Zero, IntPtr.Zero);
      for (int wParam = 0; wParam < num; ++wParam)
      {
        PInvoke.SendMessage(hwndRebar, 1052, (void*) wParam, (void*) lParam1);
        PInvoke.SendMessage(hwndRebar, 1033, (void*) wParam, (void*) lParam2);
        if (!bandInfoDic.ContainsKey(lParam2->top))
          bandInfoDic.Add(lParam2->top, new Dictionary<int, REBARBANDINFO>());
        bandInfoDic[lParam2->top][lParam2->left] = lParam1[0];
      }
      return bandInfoDic;
    }

    public int GetCurrentLayoutFromID(int id)
    {
      this.SaveCurrentBandLayout();
      return this.GetBandLayoutFromID(id);
    }

    private void SaveExtraBarRatios()
    {
      bool secondViewBarVisible = this.Toolbars.IsSecondViewBarVisible;
      bool thirdViewBarVisible = this.Toolbars.IsThirdViewBarVisible;
      if (!(secondViewBarVisible | thirdViewBarVisible))
        return;
      using (RegistryKey registryKey = RegistryUtil.OpenUserRoot("Volatile", true, true))
      {
        if (secondViewBarVisible)
        {
          float f = Math.Max(0.1f, (float) this.Toolbars.SecondViewBar.BaseBarSize / (float) this.Bounds.Height);
          registryKey.SetValue("ExtraViewWidthRatio2nd", (object) Config.SingleToStrng(f));
        }
        if (!thirdViewBarVisible)
          return;
        float f1 = Math.Max(0.1f, (float) this.Toolbars.ThirdViewBar.BaseBarSize / (float) this.Bounds.Width);
        registryKey.SetValue("ExtraViewWidthRatio3rd", (object) Config.SingleToStrng(f1));
      }
    }

    public void ResetShrinkTimers(ShrinkTimerModes mode)
    {
      if (mode.HasFlag((Enum) ShrinkTimerModes.Horizontal) && this.timerShrinkStart != null)
        this.timerShrinkStart.Enabled = false;
      if (mode.HasFlag((Enum) ShrinkTimerModes.Vertical) && this.timerVerticalBarShrinkStart != null)
        this.timerVerticalBarShrinkStart.Enabled = false;
      if (mode.HasFlag((Enum) ShrinkTimerModes.Bottom) && this.timerBottomBarShrinkStart != null)
        this.timerBottomBarShrinkStart.Enabled = false;
      if (ExplorerManager.AnyToolbarAutoHide || ExplorerManager.VerticalBarAutoHide || ExplorerManager.BottomBarAutoHide)
      {
        if (this.timerPollingShrinkToolbar == null)
        {
          this.timerPollingShrinkToolbar = new System.Windows.Forms.Timer(this.components);
          this.timerPollingShrinkToolbar.Interval = 333;
          this.timerPollingShrinkToolbar.Tick += new EventHandler(this.timerPollingShrinkToolbar_Tick);
        }
        this.timerPollingShrinkToolbar.Enabled = true;
      }
      else
      {
        if (this.timerPollingShrinkToolbar == null)
          return;
        this.timerPollingShrinkToolbar.Enabled = false;
      }
    }

    public void ShrinkHorizontalToolbars(bool fManual)
    {
      this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
      if (!this.ToolbarsAreShrunken)
      {
        this.ToolbarsAreShrunken = true;
        this.HideTipsForShrinking();
        bool fTabBar;
        bool fButtonBar;
        bool fHVersatileBar;
        this.GetUnshrinkableToolbars(out fTabBar, out fButtonBar, out fHVersatileBar);
        bool anyToolbarAutoHide = ExplorerManager.AnyToolbarAutoHide;
        if (this.Toolbars.IsTabBarAvailable && !this.Toolbars.TabBar.IsBottomBar && (anyToolbarAutoHide ? (fTabBar || !Config.Bool(Scts.AutoHideTabBar) ? 0 : (!Config.Bool(Scts.ShrinkTabBarOnlyWhenSoleTab) ? 1 : (this.Toolbars.TabBar.TabItems.Count == 1 ? 1 : 0))) : (fManual ? 1 : 0)) != 0)
          this.Toolbars.TabBar.Shrink();
        if (this.Toolbars.IsCommandBar1stAvailable && (anyToolbarAutoHide ? (fButtonBar ? 0 : (Config.Bool(Scts.AutoHideCommandBar1st) ? 1 : 0)) : (fManual ? 1 : 0)) != 0)
          this.Toolbars.CommandBar1st.Shrink();
        if (this.Toolbars.IsCommandBar2ndAvailable && (anyToolbarAutoHide ? (fHVersatileBar ? 0 : (Config.Bool(Scts.AutoHideCommandBar2nd) ? 1 : 0)) : (fManual ? 1 : 0)) != 0)
          this.Toolbars.CommandBar2nd.Shrink();
      }
      this.fHorizontalToolbarShrinkPending = false;
    }

    public void ExpandHorizontalToolbars()
    {
      this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
      if (!this.ToolbarsAreShrunken)
        return;
      this.ToolbarsAreShrunken = false;
      this.HideTipsForShrinking();
      if (this.Toolbars.IsTabBarAvailable && !this.Toolbars.TabBar.IsBottomBar)
        this.Toolbars.TabBar.Expand();
      if (this.Toolbars.IsCommandBar1stAvailable)
        this.Toolbars.CommandBar1st.Expand();
      if (!this.Toolbars.IsCommandBar2ndAvailable)
        return;
      this.Toolbars.CommandBar2nd.Expand();
    }

    public void ShrinkToolbar(Toolbar toolbar)
    {
      switch (toolbar)
      {
        case Toolbar.CommandBarVertical:
          this.ResetShrinkTimers(ShrinkTimerModes.Vertical);
          if (!this.VerticalBarShrunken)
          {
            this.VerticalBarShrunken = true;
            this.Toolbars.Show(Toolbar.CommandBarVertical, false);
          }
          this.fVerticalBarShrinkPending = false;
          break;
        case Toolbar.BottomTabBar:
          this.ResetShrinkTimers(ShrinkTimerModes.Bottom);
          this.HideTipsForShrinking();
          if (!this.BottomBarShrunken)
          {
            this.BottomBarShrunken = true;
            if (this.Toolbars.IsTabBarAvailable && this.Toolbars.TabBar.IsBottomBar)
              this.Toolbars.TabBar.Shrink();
          }
          this.fBottomBarShrinkPending = false;
          break;
        default:
          bool fTabBar = false;
          bool fButtonBar = false;
          bool fHVersatileBar = false;
          this.GetUnshrinkableToolbars(out fTabBar, out fButtonBar, out fHVersatileBar);
          switch (toolbar)
          {
            case Toolbar.TabBar:
              if (!fTabBar && this.Toolbars.IsTabBarAvailable && !this.Toolbars.TabBar.IsBottomBar)
              {
                this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
                this.Toolbars.TabBar.Shrink();
                break;
              }
              break;
            case Toolbar.CommandBar1:
              if (!fButtonBar && this.Toolbars.IsCommandBar1stAvailable)
              {
                this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
                this.Toolbars.CommandBar1st.Shrink();
                break;
              }
              break;
            case Toolbar.CommandBar2:
              if (!fHVersatileBar && this.Toolbars.IsCommandBar2ndAvailable)
              {
                this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
                this.Toolbars.CommandBar2nd.Shrink();
                break;
              }
              break;
          }
          this.ToolbarsAreShrunken = this.AnyToolbarShrunken;
          break;
      }
    }

    public void ShrinkBottomTabBarAuto()
    {
      if (Config.Bool(Scts.ShrinkBtmBarOnlyWhenSoleTab) && (!this.Toolbars.IsTabBarAvailable || !this.Toolbars.TabBar.IsBottomBar || this.Toolbars.TabBar.TabItems.Count != 1))
        return;
      this.ShrinkToolbar(Toolbar.BottomTabBar);
    }

    public void ExpandToolbar(Toolbar toolbar)
    {
      switch (toolbar)
      {
        case Toolbar.CommandBarVertical:
          this.ResetShrinkTimers(ShrinkTimerModes.Vertical);
          if (!this.VerticalBarShrunken)
            break;
          this.VerticalBarShrunken = false;
          this.HideTipsForShrinking();
          this.Toolbars.Show(Toolbar.CommandBarVertical, true);
          break;
        case Toolbar.BottomTabBar:
          this.ResetShrinkTimers(ShrinkTimerModes.Bottom);
          if (!this.BottomBarShrunken)
            break;
          this.BottomBarShrunken = false;
          this.HideTipsForShrinking();
          if (!this.Toolbars.IsTabBarAvailable || !this.Toolbars.TabBar.IsBottomBar)
            break;
          this.Toolbars.TabBar.Expand();
          break;
        default:
          this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
          this.HideTipsForShrinking();
          switch (toolbar)
          {
            case Toolbar.TabBar:
              if (this.Toolbars.IsTabBarAvailable && !this.Toolbars.TabBar.IsBottomBar)
              {
                this.Toolbars.TabBar.Expand();
                break;
              }
              break;
            case Toolbar.CommandBar1:
              if (this.Toolbars.IsCommandBar1stAvailable)
              {
                this.Toolbars.CommandBar1st.Expand();
                break;
              }
              break;
            case Toolbar.CommandBar2:
              if (this.Toolbars.IsCommandBar2ndAvailable)
              {
                this.Toolbars.CommandBar2nd.Expand();
                break;
              }
              break;
          }
          this.ToolbarsAreShrunken = this.AnyToolbarShrunken;
          break;
      }
    }

    private void ExpandToolbarsOnMaximized()
    {
      if (!Config.Bool(Scts.ExpandToolbarsMaximizedWnd))
        return;
      Action expandToolbarsOnMaximizedCore = (Action) (() {
      {
        if (this.ToolbarsAreShrunken)
          this.ExpandHorizontalToolbars();
        if (this.VerticalBarShrunken)
          this.ExpandToolbar(Toolbar.CommandBarVertical);
        if (!this.BottomBarShrunken)
          return;
        this.ExpandToolbar(Toolbar.BottomTabBar);
      });
      if (!Config.Bool(Scts.ExtraViewNoKeepViewRatio))
      {
        if (this.timerExpandTimerOnMaximized == null)
        {
          this.timerExpandTimerOnMaximized = new System.Windows.Forms.Timer(this.components);
          this.timerExpandTimerOnMaximized.Interval = 750;
          this.timerExpandTimerOnMaximized.Tick += (EventHandler) ((e, sender) {
          {
            this.timerExpandTimerOnMaximized.Stop();
            expandToolbarsOnMaximizedCore();
          });
        }
        this.timerExpandTimerOnMaximized.Start();
      }
      else
        expandToolbarsOnMaximizedCore();
    }

    public void UpdateToolbarShrink(Toolbar toolbar, bool fShrink)
    {
      if (fShrink)
        this.ShrinkToolbar(toolbar);
      else
        this.ExpandToolbar(toolbar);
    }

    private void GetUnshrinkableToolbars(
      out bool fTabBar,
      out bool fButtonBar,
      out bool fHVersatileBar)
    {
      fTabBar = false;
      fButtonBar = false;
      fHVersatileBar = false;
      Type[] typeArray = new Type[3]
      {
        typeof (QTTabBarClass),
        typeof (QCommandBar),
        typeof (QCommandBar2nd)
      };
      int[] numArray = new int[3]{ -1, -1, -1 };
      bool[] flagArray = new bool[3]
      {
        Config.Bool(Scts.AutoHideTabBar),
        Config.Bool(Scts.AutoHideCommandBar1st),
        Config.Bool(Scts.AutoHideCommandBar2nd)
      };
      using (RegistryKey rk = RegistryUtil.OpenUserRoot("BandLayout"))
      {
        if (rk != null)
        {
          for (int index = 0; index < typeArray.Length; ++index)
            numArray[index] = RegistryUtil.GetValueSafe<int>(rk, typeArray[index].GUID.ToString("B"), -1);
        }
      }
      fTabBar = numArray[1] != -1 && !flagArray[1] && numArray[1] >> 16 == numArray[0] >> 16 || numArray[2] != -1 && !flagArray[2] && numArray[2] >> 16 == numArray[0] >> 16;
      fButtonBar = numArray[0] != -1 && !flagArray[0] && numArray[0] >> 16 == numArray[1] >> 16 || numArray[2] != -1 && !flagArray[2] && numArray[2] >> 16 == numArray[1] >> 16;
      fHVersatileBar = numArray[0] != -1 && !flagArray[0] && numArray[0] >> 16 == numArray[2] >> 16 || numArray[1] != -1 && !flagArray[1] && numArray[1] >> 16 == numArray[2] >> 16;
    }

    private void HideTipsForShrinking()
    {
      this.View.HideSubDirTip();
      this.View.HideGhostTip();
      this.View.HidePreviewTip();
    }

    public bool ToolbarsAreShrunken { get; private set; }

    public bool VerticalBarShrunken { get; set; }

    public bool BottomBarShrunken { get; set; }

    public static bool AnyToolbarAutoHide { Config.Bool(Scts.AutoHideTabBar) || Config.Bool(Scts.AutoHideCommandBar1st) || Config.Bool(Scts.AutoHideCommandBar2nd);

    private static bool VerticalBarAutoHide { Config.Bool(Scts.CommandBar_VerticalEnabled) && Config.Bool(Scts.AutoHideCommandBarV);

    private static bool BottomBarAutoHide { Config.Bool(Scts.BottomTabBarEnabled) && Config.Bool(Scts.AutoHideBottomTabBar);

    private bool AnyToolbarShrunken
    {
      get
      {
        if (this.Toolbars.IsTabBarAvailable && !this.Toolbars.IsBottomTabBarAvailable && this.Toolbars.TabBar.Shrunken || this.Toolbars.IsCommandBar1stAvailable && this.Toolbars.CommandBar1st.Shrunken)
          return true;
        return this.Toolbars.IsCommandBar2ndAvailable && this.Toolbars.CommandBar2nd.Shrunken;
      }
    }

    private bool AllToolbarShrunken
    {
      get
      {
        if (this.Toolbars.IsTabBarAvailable && !this.Toolbars.IsBottomTabBarAvailable && Config.Bool(Scts.AutoHideTabBar) && !this.Toolbars.TabBar.Shrunken || this.Toolbars.IsCommandBar1stAvailable && Config.Bool(Scts.AutoHideCommandBar1st) && !this.Toolbars.CommandBar1st.Shrunken)
          return false;
        return !this.Toolbars.IsCommandBar2ndAvailable || !Config.Bool(Scts.AutoHideCommandBar2nd) || this.Toolbars.CommandBar2nd.Shrunken;
      }
    }

    public void ToggleAutoHideMode()
    {
      bool flag = !ExplorerManager.AnyToolbarAutoHide;
      Config.Set(Scts.AutoHideTabBar, flag);
      Config.Set(Scts.AutoHideCommandBar1st, flag);
      Config.Set(Scts.AutoHideCommandBar2nd, flag);
      Config.Sync();
      this.ResetShrinkTimers(ShrinkTimerModes.Horizontal);
      if (flag)
        return;
      this.ExpandHorizontalToolbars();
    }

    public void ToggleVerticalAutoHideMode()
    {
      if (!Config.Bool(Scts.CommandBar_VerticalEnabled))
        return;
      Config.Set(Scts.AutoHideCommandBarV, !Config.Bool(Scts.AutoHideCommandBarV));
      Config.Sync();
      this.ResetShrinkTimers(ShrinkTimerModes.Vertical);
    }

    public void ToggleBottomAutoHideMode()
    {
      if (!Config.Bool(Scts.BottomTabBarEnabled))
        return;
      Config.Set(Scts.AutoHideBottomTabBar, !Config.Bool(Scts.AutoHideBottomTabBar));
      Config.Sync();
      this.ResetShrinkTimers(ShrinkTimerModes.Bottom);
    }

    private void timerPollingShrinkToolbar_Tick(object sender, EventArgs e)
    {
      Point mousePosition = Control.MousePosition;
      if ((this.timerShrinkStart == null || !this.timerShrinkStart.Enabled) && ExplorerManager.AnyToolbarAutoHide)
      {
        if (!this.ToolbarsAreShrunken)
        {
          if (!this.NoShrink && !this.FocusKeepsToolbarVisible(ShrinkTimerModes.Horizontal) && !this.MouseCursorKeepsToolbarVisible(mousePosition))
            this.StartWaitForShrinking(true);
        }
        else if (!this.NoExpand && this.MouseCursorExpandsToolbar(mousePosition))
          this.StartWaitForShrinking(false);
      }
      if ((this.timerVerticalBarShrinkStart == null || !this.timerVerticalBarShrinkStart.Enabled) && ExplorerManager.VerticalBarAutoHide)
      {
        if (!this.VerticalBarShrunken)
        {
          if (!this.NoShrink && !this.FocusKeepsToolbarVisible(ShrinkTimerModes.Vertical) && !this.MouseCursorKeepsVerticalBarVisible(mousePosition))
            this.StartWaitForVerticalbarShrinking(true);
        }
        else if (!this.NoExpand && this.MouseCursorExpandsVerticalBar(mousePosition))
          this.StartWaitForVerticalbarShrinking(false);
      }
      if (this.timerBottomBarShrinkStart != null && this.timerBottomBarShrinkStart.Enabled || !ExplorerManager.BottomBarAutoHide)
        return;
      if (!this.BottomBarShrunken)
      {
        if (this.NoShrink || this.FocusKeepsToolbarVisible(ShrinkTimerModes.Bottom) || this.MouseCursorKeepsBottomBarVisible(mousePosition))
          return;
        this.StartWaitForBottomBarShrinking(true);
      }
      else
      {
        if (this.NoExpand || !this.MouseCursorExpandsBottomBar(mousePosition))
          return;
        this.StartWaitForBottomBarShrinking(false);
      }
    }

    private void StartWaitForShrinking(bool fShrink)
    {
      if (this.timerShrinkStart == null)
      {
        this.timerShrinkStart = new System.Windows.Forms.Timer(this.components);
        this.timerShrinkStart.Tick += new EventHandler(this.timerShrinkStart_Tick);
      }
      this.timerShrinkStart.Interval = fShrink ? Config.AutoHideToolbarDelay : Config.AutoShowToolbarDelay;
      this.timerShrinkStart.Restart();
    }

    private void StartWaitForVerticalbarShrinking(bool fShrink)
    {
      if (this.timerVerticalBarShrinkStart == null)
      {
        this.timerVerticalBarShrinkStart = new System.Windows.Forms.Timer(this.components);
        this.timerVerticalBarShrinkStart.Tick += new EventHandler(this.timerVerticalBarShrinkStart_Tick);
      }
      this.timerVerticalBarShrinkStart.Interval = fShrink ? Config.AutoHideToolbarDelay : Config.AutoShowToolbarDelay;
      this.timerVerticalBarShrinkStart.Enabled = false;
      this.timerVerticalBarShrinkStart.Enabled = true;
    }

    private void StartWaitForBottomBarShrinking(bool fShrink)
    {
      if (this.timerBottomBarShrinkStart == null)
      {
        this.timerBottomBarShrinkStart = new System.Windows.Forms.Timer(this.components);
        this.timerBottomBarShrinkStart.Tick += new EventHandler(this.timerBottomBarShrinkStart_Tick);
      }
      this.timerBottomBarShrinkStart.Interval = fShrink ? Config.AutoHideToolbarDelay : Config.AutoShowToolbarDelay;
      this.timerBottomBarShrinkStart.Enabled = false;
      this.timerBottomBarShrinkStart.Enabled = true;
    }

    private void timerShrinkStart_Tick(object sender, EventArgs e)
    {
      this.timerShrinkStart.Enabled = false;
      ExplorerManager.FOLDERBAND_HEIGHT = WindowUtil.GetFolderBandHeight(this.hwndShellTab);
      Point mousePosition = Control.MousePosition;
      if (!this.ToolbarsAreShrunken)
      {
        if (this.NoShrink || this.FocusKeepsToolbarVisible(ShrinkTimerModes.Horizontal) || this.MouseCursorKeepsToolbarVisible(mousePosition))
          return;
        bool flag1 = ExplorerManager.VerticalBarAutoHide && !this.VerticalBarShrunken;
        bool flag2 = ExplorerManager.BottomBarAutoHide && !this.BottomBarShrunken;
        if (flag1 & flag2 && (!this.fVerticalBarShrinkPending || !this.fBottomBarShrinkPending) || flag1 && !flag2 && !this.fVerticalBarShrinkPending || !flag1 & flag2 && !this.fBottomBarShrinkPending)
        {
          this.fHorizontalToolbarShrinkPending = true;
        }
        else
        {
          this.ShrinkHorizontalToolbars(false);
          if (this.fVerticalBarShrinkPending)
            this.ShrinkToolbar(Toolbar.CommandBarVertical);
          if (!this.fBottomBarShrinkPending)
            return;
          this.ShrinkBottomTabBarAuto();
        }
      }
      else
      {
        if (this.NoExpand || !this.MouseCursorExpandsToolbar(mousePosition))
          return;
        this.ExpandHorizontalToolbars();
      }
    }

    private void timerVerticalBarShrinkStart_Tick(object sender, EventArgs e)
    {
      this.timerVerticalBarShrinkStart.Enabled = false;
      Point mousePosition = Control.MousePosition;
      if (!this.VerticalBarShrunken)
      {
        if (this.NoShrink || this.FocusKeepsToolbarVisible(ShrinkTimerModes.Vertical) || this.MouseCursorKeepsVerticalBarVisible(mousePosition))
          return;
        bool flag1 = ExplorerManager.AnyToolbarAutoHide && !this.ToolbarsAreShrunken;
        bool flag2 = ExplorerManager.BottomBarAutoHide && !this.BottomBarShrunken;
        if (flag1 & flag2 && (!this.fHorizontalToolbarShrinkPending || !this.fBottomBarShrinkPending) || flag1 && !flag2 && !this.fHorizontalToolbarShrinkPending || !flag1 & flag2 && !this.fBottomBarShrinkPending)
        {
          this.fVerticalBarShrinkPending = true;
        }
        else
        {
          this.ShrinkToolbar(Toolbar.CommandBarVertical);
          if (this.fHorizontalToolbarShrinkPending)
            this.ShrinkHorizontalToolbars(false);
          if (!this.fBottomBarShrinkPending)
            return;
          this.ShrinkBottomTabBarAuto();
        }
      }
      else
      {
        if (this.NoExpand || !this.MouseCursorExpandsVerticalBar(mousePosition))
          return;
        this.ExpandToolbar(Toolbar.CommandBarVertical);
      }
    }

    private void timerBottomBarShrinkStart_Tick(object sender, EventArgs e)
    {
      this.timerBottomBarShrinkStart.Enabled = false;
      Point mousePosition = Control.MousePosition;
      if (!this.BottomBarShrunken)
      {
        if (this.NoShrink || this.FocusKeepsToolbarVisible(ShrinkTimerModes.Bottom) || this.MouseCursorKeepsBottomBarVisible(mousePosition))
          return;
        bool flag1 = ExplorerManager.AnyToolbarAutoHide && !this.ToolbarsAreShrunken;
        bool flag2 = ExplorerManager.VerticalBarAutoHide && !this.VerticalBarShrunken;
        if (flag1 & flag2 && (!this.fHorizontalToolbarShrinkPending || !this.fVerticalBarShrinkPending) || flag1 && !flag2 && !this.fHorizontalToolbarShrinkPending || !flag1 & flag2 && !this.fVerticalBarShrinkPending)
        {
          this.fBottomBarShrinkPending = true;
        }
        else
        {
          this.ShrinkBottomTabBarAuto();
          if (this.fHorizontalToolbarShrinkPending)
            this.ShrinkHorizontalToolbars(false);
          if (!this.fVerticalBarShrinkPending)
            return;
          this.ShrinkToolbar(Toolbar.CommandBarVertical);
        }
      }
      else
      {
        if (this.NoExpand || !this.MouseCursorExpandsBottomBar(mousePosition))
          return;
        this.ExpandToolbar(Toolbar.BottomTabBar);
      }
    }

    private bool NoShrink { this.StopShrinkToolbars || Config.Bool(Scts.ExpandToolbarsMaximizedWnd) && PInvoke.IsZoomed(this.hwndExplorer) || PInvoke.GetCapture() != IntPtr.Zero || Control.MouseButtons != MouseButtons.None || MenuUtility.Visible;

    private bool NoExpand
    {
      get
      {
        if (Config.Bool(Scts.NoAutoShowToolbars))
          return true;
        return Control.MouseButtons == MouseButtons.None && MenuUtility.Visible;
      }
    }

    public bool StopShrinkToolbars { get; set; }

    private bool FocusKeepsToolbarVisible(ShrinkTimerModes mode)
    {
      switch (mode)
      {
        case ShrinkTimerModes.Horizontal:
          if (Config.Bool(Scts.AutoHideTabBar) && this.Toolbars.IsTabBarAvailable && this.Toolbars.TabBar.ContainsFocus || Config.Bool(Scts.AutoHideCommandBar1st) && this.Toolbars.IsCommandBar1stAvailable && this.Toolbars.CommandBar1st.ContainsFocus)
            return true;
          return Config.Bool(Scts.AutoHideCommandBar2nd) && this.Toolbars.IsCommandBar2ndAvailable && this.Toolbars.CommandBar2nd.ContainsFocus;
        case ShrinkTimerModes.Vertical:
          return Config.Bool(Scts.AutoHideCommandBarV) && this.Toolbars.IsCommandBarVrtAvailable && this.Toolbars.CommandBarVrt.ContainsFocus;
        case ShrinkTimerModes.Bottom:
          return Config.Bool(Scts.AutoHideBottomTabBar) && this.Toolbars.IsBottomTabBarAvailable && this.Toolbars.TabBar.ContainsFocus;
        default:
          return false;
      }
    }

    private bool MouseCursorKeepsToolbarVisible(Point pntMouse)
    {
      Rectangle windowRect1 = PInvoke.GetWindowRect(this.hwndShellTab);
      Rectangle windowRect2 = PInvoke.GetWindowRect(this.hwndExplorer);
      Rectangle windowRect3 = PInvoke.GetWindowRect(this.hwndRebar);
      return QEnvironment.LaterThan7 ? windowRect1.Left < pntMouse.X && pntMouse.X < windowRect1.Right && windowRect2.Top < pntMouse.Y && pntMouse.Y < windowRect3.Bottom && this.MouseCursorIsOnThreadWindow(pntMouse) : windowRect1.Left < pntMouse.X && pntMouse.X < windowRect1.Right && windowRect2.Top < pntMouse.Y && pntMouse.Y < windowRect1.Top + windowRect3.Height + ExplorerManager.FOLDERBAND_HEIGHT && this.MouseCursorIsOnThreadWindow(pntMouse);
    }

    private bool MouseCursorExpandsToolbar(Point pntMouse)
    {
      Rectangle windowRect1 = PInvoke.GetWindowRect(this.hwndShellTab);
      Rectangle windowRect2 = PInvoke.GetWindowRect(this.hwndRebar);
      if (this.Toolbars.IsCommandBarVrtAvailable && this.Toolbars.CommandBarVrt.RebarRect.Contains(pntMouse))
        return false;
      if (QEnvironment.LaterThan7)
      {
        Rectangle windowRect3 = PInvoke.GetWindowRect(this.hwndAddressBandRoot);
        return windowRect1.Left < pntMouse.X && pntMouse.X < windowRect1.Right && windowRect3.Top < pntMouse.Y && pntMouse.Y < windowRect2.Bottom && this.MouseCursorIsOnThreadWindow(pntMouse) && !this.MouseCursorIsOnHorizontalToolbarPrivateArea(pntMouse);
      }
      return windowRect1.Left < pntMouse.X && pntMouse.X < windowRect1.Right && windowRect1.Top < pntMouse.Y && pntMouse.Y < windowRect1.Top + windowRect2.Height + ExplorerManager.FOLDERBAND_HEIGHT && this.MouseCursorIsOnThreadWindow(pntMouse) && !this.MouseCursorIsOnHorizontalToolbarPrivateArea(pntMouse);
    }

    private bool MouseCursorKeepsVerticalBarVisible(Point pntMouse) { (WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, QEnvironment.RightToLeft ? 11 : 10, pntMouse) || WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, QEnvironment.RightToLeft ? 17 : 16, pntMouse) || this.Toolbars.IsCommandBarVrtAvailable && this.Toolbars.CommandBarVrt.RebarRect.InflateBy(0, 0, Graphic.ScaleBy(ExplorerManager.WindowScaling, 8), 0).Contains(pntMouse)) && this.MouseCursorIsOnThreadWindow(pntMouse);

    private bool MouseCursorExpandsVerticalBar(Point pntMouse)
    {
      if (PInvoke.IsZoomed(this.hwndExplorer))
      {
        Point lpPoints = pntMouse;
        PInvoke.MapWindowPoints(IntPtr.Zero, this.hwndShellTab, ref lpPoints, 1);
        Rectangle windowRect = PInvoke.GetWindowRect(this.hwndShellTab);
        return (QEnvironment.RightToLeft ? (windowRect.Right - 4 >= lpPoints.X ? 0 : (lpPoints.X < windowRect.Right ? 1 : 0)) : (0 > lpPoints.X ? 0 : (lpPoints.X < 4 ? 1 : 0))) != 0 && 0 <= lpPoints.Y && lpPoints.Y < windowRect.Height && this.MouseCursorIsOnThreadWindow(pntMouse);
      }
      return (WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, QEnvironment.RightToLeft ? 11 : 10, pntMouse) || WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, QEnvironment.RightToLeft ? 17 : 16, pntMouse)) && this.MouseCursorIsOnThreadWindow(pntMouse);
    }

    private bool MouseCursorKeepsBottomBarVisible(Point pntMouse)
    {
      if (!WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, 15, pntMouse) && !WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, 16, pntMouse) && !WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, 17, pntMouse))
      {
        if (this.Toolbars.IsBottomTabBarAvailable)
        {
          Rectangle rectangle = this.Toolbars.TabBar.RebarRect;
          if (!rectangle.Contains(pntMouse))
          {
            rectangle = this.Toolbars.TabBar.BasebarRect;
            if (rectangle.Contains(pntMouse) || this.MouseCursorIsOnBottomRegionButNotInListView(pntMouse))
              goto label_5;
          }
          else
            goto label_5;
        }
        if (!this.MouseCursorIsOnStatusBar(pntMouse))
          return false;
      }
label_5:
      return this.MouseCursorIsOnThreadWindow(pntMouse);
    }

    private bool MouseCursorExpandsBottomBar(Point pntMouse)
    {
      if (PInvoke.IsZoomed(this.hwndExplorer))
      {
        Rectangle windowRect = PInvoke.GetWindowRect(this.hwndExplorer);
        Rectangle clientRect = PInvoke.GetClientRect(this.hwndExplorer);
        return (windowRect.Left <= pntMouse.X && pntMouse.X < windowRect.Right && clientRect.Bottom + windowRect.Top - Graphic.ScaleBy(ExplorerManager.WindowScaling, 16) <= pntMouse.Y && pntMouse.Y < clientRect.Bottom + windowRect.Top || this.MouseCursorIsOnStatusBar(pntMouse)) && this.MouseCursorIsOnThreadWindow(pntMouse);
      }
      return (WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, 15, pntMouse) || WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, 16, pntMouse) || WindowUtil.MouseCursorIsOnTheBorder(this.hwndExplorer, 17, pntMouse) || this.MouseCursorIsOnStatusBar(pntMouse) || this.Toolbars.IsBottomTabBarAvailable && this.Toolbars.TabBar.BasebarRect.Contains(pntMouse)) && this.MouseCursorIsOnThreadWindow(pntMouse);
    }

    private bool MouseCursorIsOnThreadWindow(Point pntMouse)
    {
      int lpdwProcessId;
      return PInvoke.GetWindowThreadProcessId(PInvoke.WindowFromPoint(pntMouse), out lpdwProcessId) == this.ThreadID && lpdwProcessId == this.ProcessID;
    }

    private bool MouseCursorIsOnStatusBar(Point pntMouse)
    {
      if (QEnvironment.IsWindows7)
      {
        IntPtr phwnd;
        return this.shellBrowser.GetControlWindow(1U, out phwnd) == 0 && PInvoke.GetWindowRect(phwnd).Contains(pntMouse);
      }
      Rectangle windowRect = PInvoke.GetWindowRect(WindowUtil.GetDUIViewWndClassName(this.shellBrowser));
      return windowRect.Left <= pntMouse.X && pntMouse.X <= windowRect.Right && windowRect.Bottom - Graphic.ScaleBy(ExplorerManager.WindowScaling, 24) <= pntMouse.Y && pntMouse.Y <= windowRect.Bottom;
    }

    private bool MouseCursorIsOnBottomRegionButNotInListView(Point pntMouse)
    {
      Rectangle windowRect = PInvoke.GetWindowRect(this.Toolbars.TabBarHandle);
      return this.hwndListView != IntPtr.Zero && windowRect.Left < pntMouse.X && pntMouse.X < windowRect.Right && PInvoke.GetWindowRect(this.hwndListView).Bottom < pntMouse.Y && pntMouse.Y < windowRect.Top;
    }

    private bool MouseCursorIsOnHorizontalToolbarPrivateArea(Point pntMouse)
    {
      IntPtr handle = PInvoke.WindowFromPoint(pntMouse);
      if (handle != IntPtr.Zero)
      {
        Control control = Control.FromHandle(handle);
        if (control != null)
        {
          if (!(control is BandObject bandObject))
            bandObject = control.Parent as BandObject;
          if (bandObject != null)
            return bandObject.KeepOtherToolbarShrunken;
        }
      }
      return false;
    }

    public void InitializeNavigationStatus(bool fForce)
    {
      if (!(!this.fFirstNavigationComplete | fForce))
        return;
      this.fNowInitializingInstallation = true;
      using (ShellItem shellItem = this.View.ShellItem)
      {
        string url = shellItem.Path;
        if (string.IsNullOrEmpty(url))
          url = shellItem.DisplayName;
        this.View_NavigationComplete((object) this.View, new ExplorerBrowserNavigationEventArgs(shellItem, url: url));
      }
      this.fNowInitializingInstallation = false;
    }

    public bool Initializing { this.fNowInitializingInstallation;

    public bool ExplorerIsComplete { this.explorer != null && this.explorer.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE;

    public ToolStripItem CommandBarDragStartItem { get; set; }

    public ToolStripItem CommandBarDragStartMenuItem { get; set; }

    public CommandBarType CommandBarDragStartSource { get; set; }

    public CommandBarType CommandBarDragStartMenuSource { get; set; }

    public bool ProcessingPendings { this.fProcessingPendings;

    public void OpenOptionDialog(OptionDialogTab index = OptionDialogTab.NotSpecified)
    {
      if (ExplorerManager.fNowOptionDialogOpening)
        return;
      if (index != OptionDialogTab.NotSpecified)
      {
        OptionDialog.indexOptionsDialogTab = (int) index;
        OptionDialog.NoInnerTabIndex = true;
      }
      if (ExplorerManager.optionsDialog == null)
      {
        ExplorerManager.fNowOptionDialogOpening = true;
        this.fOptionDialogCreated = true;
        Thread thread = new Thread(new ThreadStart(this.openOptionDialogCore));
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
      }
      else
      {
        if (ExplorerManager.optionsDialog.Disposing || ExplorerManager.optionsDialog.IsDisposed || !ExplorerManager.optionsDialog.IsHandleCreated)
          return;
        ExplorerManager.optionsDialog.Invoke((Delegate) new Action<OptionDialogTab>(this.odCallback_Activate), (object) index);
        OptionDialog.NoInnerTabIndex = false;
      }
    }

    private void openOptionDialogCore()
    {
      try
      {
        using (ExplorerManager.optionsDialog = new OptionDialog(this.pluginManager, new Action<OptionDialogCallbackInfo>(this.optionDialogCallback)))
        {
          ExplorerManager.fNowOptionDialogOpening = false;
          int num = (int) ExplorerManager.optionsDialog.ShowDialog();
        }
        ExplorerManager.optionsDialog = (OptionDialog) null;
        this.fOptionDialogCreated = false;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex?.ToString() ?? "");
      }
    }

    private void optionDialogCallback(OptionDialogCallbackInfo info)
    {
      switch (info.CallBackKind)
      {
        case OptionCallbackKind.Apply:
          this.Invoke((Delegate) new Action<OptionDialogCallbackInfo>(this.odCallback_RefreshOptions), new object[1]
          {
            (object) info
          });
          break;
        case OptionCallbackKind.OK:
          ExplorerManager.optionsDialog = (OptionDialog) null;
          this.Invoke((Delegate) new Action<OptionDialogCallbackInfo>(this.odCallback_RefreshOptions), new object[1]
          {
            (object) info
          });
          break;
        case OptionCallbackKind.PluginOption:
          this.BeginInvoke((Delegate) new Action<OptionDialogCallbackInfo>(this.odCallback_PluginOption), new object[1]
          {
            (object) info
          });
          break;
        case OptionCallbackKind.AutoCompleteSourceCleared:
          this.BeginInvoke((Delegate) new Action(this.odCallback_AutoCompleteSourcesCleared));
          break;
        case OptionCallbackKind.ConfigFileLoaded:
          ExplorerManager.optionsDialog = (OptionDialog) null;
          this.Invoke((Delegate) new Action<OptionDialogCallbackInfo>(this.odCallback_ConfigLoaded), new object[1]
          {
            (object) info
          });
          break;
      }
    }

    private void odCallback_PluginOption(OptionDialogCallbackInfo info)
    {
      if (!this.InModal)
      {
        WindowUtil.BringExplorerToFront(this.hwndExplorer);
        Plugin plugin;
        if (!this.pluginManager.TryGetPlugin(info.PluginID, out plugin) && !PluginManager.TryGetStaticPlugin(info.PluginID, out plugin) || plugin == null)
          return;
        if (plugin.Instance == null)
          return;
        try
        {
          plugin.Instance.OnOption();
        }
        catch (Exception ex)
        {
          IntPtr hwndExplorer = this.hwndExplorer;
          string pluginId = info.PluginID;
          PluginManager.HandlePluginException(ex, hwndExplorer, pluginId, "Open plugin option.");
          string optional = "Error at Plugin: " + info.PluginID;
          DebugUtil.AppendToCriticalExceptionLog(ex, optional);
        }
      }
      else
        SystemSounds.Beep.Play();
    }

    private void odCallback_RefreshOptions(OptionDialogCallbackInfo info)
    {
      ExplorerManager.RefreshProcess(true, info.ReBarBackgroundColorChanged, info.LanguageFileChanged);
      InstanceManager.SyncProcesses(32772, IntPtr.Zero, IntPtr.Zero);
      InstanceManager.SyncProcesses(32774, IntPtr.Zero, IntPtr.Zero);
      InstanceManager.SyncProcesses(32775, IntPtr.Zero, IntPtr.Zero, false);
      InstanceManager.SyncProcesses(32769, info.ReBarBackgroundColorChanged ? (IntPtr) 1 : IntPtr.Zero, info.LanguageFileChanged ? (IntPtr) 1 : IntPtr.Zero);
      IntPtr desktopToolWindow = WindowUtil.GetDesktopToolWindow();
      if (desktopToolWindow != IntPtr.Zero)
        PInvoke.PostMessage(desktopToolWindow, 32768, IntPtr.Zero, info.LanguageFileChanged ? (IntPtr) 1 : IntPtr.Zero);
      if (MessageListenerWindow.IsKeyFinderVisible)
        PInvoke.PostMessage(MessageListenerWindow.WindowHandle, 32786, IntPtr.Zero, IntPtr.Zero);
      QGroupOpener.InitializeUser();
      ExplorerProcessCaptor.InitializeUser();
      CommandButtonFile.InitializeUser();
    }

    private void odCallback_ConfigLoaded(OptionDialogCallbackInfo info)
    {
      info.ReBarBackgroundColorChanged = info.LanguageFileChanged = true;
      this.odCallback_RefreshOptions(info);
      InstanceManager.SyncProcesses(32797, IntPtr.Zero, IntPtr.Zero, false);
    }

    private void odCallback_AutoCompleteSourcesCleared()
    {
      this.Toolbars.FirstVisibleCommandControlOwner((Func<ICommandControlOwner, bool>) (cco { cco.ClearFilterBoxAutoCompleteSource()));
      new QAutoCompleteSource("FileRenamerFind", 0).Clear();
      new QAutoCompleteSource("FileRenamerReplace", 0).Clear();
    }

    private static void RefreshProcess(
      bool fPrimaryProcess,
      bool fReBarBackgroundColorTurnedOff,
      bool fLanguageFileChanged)
    {
      ExplorerManager.refreshProcessInfo = new RefreshProcessInfo(fReBarBackgroundColorTurnedOff);
      if (fLanguageFileChanged)
        StringResources.Initialize();
      RecentTabManager.RefreshAllowDuplication();
      if (fReBarBackgroundColorTurnedOff)
        ExplorerManager.DefaultRebarCOLORREF = -1;
      ViewColorManager.Initialize();
      if (fPrimaryProcess)
      {
        ShellExtHelper.SaveStringResources();
        ExplorerManager.RefreshStaticVerbs();
        ViewColorManager.InstallShellExtension(Config.Bool(Scts.ViewVistaLayout) && !Config.Bool(Scts.ViewDisableExtendedColor) && Config.Bool(Scts.ViewEnableColorShellExt));
        IntPtr desktopToolWindow = WindowUtil.GetDesktopToolWindow();
        if (desktopToolWindow != IntPtr.Zero)
          PInvoke.SendMessage(desktopToolWindow, 32770, 0, 0);
      }
      else
      {
        UserEvents.Initialize();
        WindowCaptureManager.Initialize(true);
      }
      if (RendererProvider.Initialize())
      {
        ExplorerManager.refreshProcessInfo.MenuRendererChanged = true;
        if (fPrimaryProcess)
        {
          IntPtr desktopHwnd = WindowUtil.GetDesktopHwnd();
          if (desktopHwnd != IntPtr.Zero)
            PInvoke.PostMessage(desktopHwnd, RegisteredMessage.InvokeInDesktopThread, 2, 0);
        }
      }
      foreach (IntPtr explorerManager in (IEnumerable<IntPtr>) InstanceManager.ExplorerManagers())
        PInvoke.SendNotifyMessage(explorerManager, 48647, IntPtr.Zero, IntPtr.Zero);
    }

    private void RefreshThread()
    {
      ExplorerManager.watermarkImageCache.Clear();
      if (ExplorerManager.refreshProcessInfo.ReBarBackgroundColorTurnedOff && ExplorerManager.DefaultRebarCOLORREF != -1)
        PInvoke.SendMessage(this.hwndRebar, 1043, IntPtr.Zero, (IntPtr) ExplorerManager.DefaultRebarCOLORREF);
      else if (Config.Bool(Scts.RebarBackgroundColor))
      {
        if (ExplorerManager.DefaultRebarCOLORREF == -1)
          ExplorerManager.DefaultRebarCOLORREF = (int) (long) PInvoke.SendMessage(this.hwndRebar, 1044, IntPtr.Zero, IntPtr.Zero);
        PInvoke.SendMessage(this.hwndRebar, 1043, IntPtr.Zero, (IntPtr) MCR.MakeCOLORREF(Config.RebarBackgroundColor));
      }
      if (Config.Bool(Scts.RebarBackgroundImage) && this.toolbarImageHelper != null)
        this.toolbarImageHelper.CreateRebarImage();
      if ((Config.Bool(Scts.RebarBackgroundColor) || Config.Bool(Scts.RebarBackgroundImage)) && this.rebarWindowSubclass == null)
        this.rebarWindowSubclass = new WindowSubclass(this.hwndRebar, new WindowSubclass.SubclassingProcedure(this.rebarSubclassProc));
      IntPtr windowLongPtr = PInvoke.GetWindowLongPtr(this.hwndRebar, GWL.HWNDPARENT);
      if (windowLongPtr != IntPtr.Zero)
        PInvoke.RedrawWindow(windowLongPtr, IntPtr.Zero, IntPtr.Zero, RDW.INVALIDATE | RDW.VALIDATE | RDW.ALLCHILDREN | RDW.ERASENOW);
      this.ResetShrinkTimers(ShrinkTimerModes.All);
      if (this.Toolbars.IsTabBarAvailable)
      {
        this.Toolbars.TabBar.RefreshTabBar();
        if (QEnvironment.LaterThan7)
          this.Toolbars.TabBar.RefreshThemeColors();
        if (this.Toolbars.IsBottomTabBarAvailable)
          this.UpdateToolbarShrink(Toolbar.BottomTabBar, Config.Bool(Scts.AutoHideBottomTabBar) && (!Config.Bool(Scts.ShrinkBtmBarOnlyWhenSoleTab) || this.Toolbars.TabBar.TabItems.Count == 1));
        else
          this.UpdateToolbarShrink(Toolbar.TabBar, Config.Bool(Scts.AutoHideTabBar) && (!Config.Bool(Scts.ShrinkTabBarOnlyWhenSoleTab) || this.Toolbars.TabBar.TabItems.Count == 1));
      }
      if (this.Toolbars.IsCommandBar1stAvailable)
      {
        this.UpdateToolbarShrink(Toolbar.CommandBar1, Config.Bool(Scts.AutoHideCommandBar1st));
        if (QEnvironment.LaterThan7)
          this.Toolbars.CommandBar1st.RefreshThemeColors();
      }
      if (this.Toolbars.IsCommandBar2ndAvailable)
      {
        this.UpdateToolbarShrink(Toolbar.CommandBar2, Config.Bool(Scts.AutoHideCommandBar2nd));
        if (QEnvironment.LaterThan7)
          this.Toolbars.CommandBar2nd.RefreshThemeColors();
      }
      if (this.Toolbars.IsSecondViewBarAvailable)
      {
        this.Toolbars.SecondViewBar.RefreshOptions();
        if (QEnvironment.LaterThan7)
          this.Toolbars.SecondViewBar.RefreshThemeColors();
      }
      if (this.Toolbars.IsThirdViewBarAvailable)
      {
        this.Toolbars.ThirdViewBar.RefreshOptions();
        if (QEnvironment.LaterThan7)
          this.Toolbars.ThirdViewBar.RefreshThemeColors();
      }
      if (Config.Bool(Scts.CommandBar_VerticalEnabled))
      {
        if (this.Toolbars.IsCommandBarVrtAvailable)
          ((ICommandControlOwner) this.Toolbars.CommandBarVrt).RefreshVerticalBar();
        if (this.Toolbars.IsThirdViewBarAvailable && Config.Get(Scts.ExtraViewVisibleMode3rd) == 2)
          ((ICommandControlOwner) this.Toolbars.ThirdViewBar).RefreshVerticalBar();
        if (this.Toolbars.IsCommandBarVrtVisible)
          this.UpdateToolbarShrink(Toolbar.CommandBarVertical, Config.Bool(Scts.AutoHideCommandBarV));
        else if (Config.Bool(Scts.AutoHideCommandBarV))
        {
          this.VerticalBarShrunken = true;
        }
        else
        {
          this.VerticalBarShrunken = false;
          this.Toolbars.Show(Toolbar.CommandBarVertical, true);
        }
      }
      if (Config.Bool(Scts.LockWindowSize))
        PInvoke.SetWindowLongPtr(this.hwndExplorer, GWL.STYLE, PInvoke.Ptr_OP_AND(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.STYLE), 4294705151U));
      else
        PInvoke.SetWindowLongPtr(this.hwndExplorer, GWL.STYLE, PInvoke.Ptr_OP_OR(PInvoke.GetWindowLongPtr(this.hwndExplorer, GWL.STYLE), 262144U));
      if (this.shellFolderView != null)
      {
        if (this.shellBrowser is IFolderViewOptions shellBrowser)
        {
          int fvoFlags = Config.Bool(Scts.ViewVistaLayout) ? 1 : 0;
          shellBrowser.SetFolderViewOptions(FOLDERVIEWOPTIONS.FVO_VISTALAYOUT, (FOLDERVIEWOPTIONS) fvoFlags);
        }
        bool flag = ((int) this.shellFolderView.FolderFlags & 25165824) == 0;
        if (((flag ? 1 : 0) ^ (!Config.Bool(Scts.ViewVistaLayout) ? 0 : (Config.Bool(Scts.AlwaysColumnHeader) ? 1 : 0))) != 0)
        {
          if (!flag)
            this.shellFolderView.FolderFlags &= 4269801471U;
          else
            this.shellFolderView.FolderFlags |= 16777216U;
        }
        if (this.View.VistaLayout && Config.Bool(Scts.ViewVistaLayout))
        {
          this.AllViews((Action<CustomViewBase>) (v {
          {
            v.FullRowSelect = !Config.Bool(Scts.ViewNoFullRowSelect);
            v.RefreshColumnWidth(true);
          }));
          this.RefreshAllWatermarsk(IntPtr.Zero);
          this.InvokeCommand(BarCommand.Refresh1);
        }
      }
      this.Toolbars.AllAvailableCommandControlOwner((Action<ICommandControlOwner>) (cco { cco.RefreshInstance(CommandBarSyncGenres.Items | CommandBarSyncGenres.Plugins)));
      if (this.tabSwitcher != null)
      {
        this.tabSwitcher.Dispose();
        this.tabSwitcher = (TabSwitchForm) null;
      }
      if (ExplorerManager.refreshProcessInfo.MenuRendererChanged)
        this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.MenuRendererChanged));
      this.pluginManager.FireEvent(new PluginEventInfo(PluginEvents.SettingsChanged, 0, (byte[]) null, (string) null));
      this.fTreeViewColorPending = true;
      this.TrySetTreeColors(Config.Bool(Scts.FolderTreeForeColorEnabled), Config.Bool(Scts.FolderTreeBackColorEnabled));
      this.AllViews((Action<CustomViewBase>) (v { v.RefreshColors()));
    }

    private void odCallback_Activate(OptionDialogTab index)
    {
      if (ExplorerManager.optionsDialog.WindowState == FormWindowState.Minimized)
        ExplorerManager.optionsDialog.WindowState = FormWindowState.Normal;
      else
        ExplorerManager.optionsDialog.Activate();
      if (index == OptionDialogTab.NotSpecified)
        return;
      ExplorerManager.optionsDialog.SelectTab(OptionDialog.indexOptionsDialogTab);
    }

    private void odCallback_Close() { ExplorerManager.optionsDialog.OwnerWindowClosing();

    private void odCallback_NotifyViewColor() { ExplorerManager.optionsDialog.InitializeViewExtColors();

    private void RefreshAllWatermarsk(IntPtr pidl)
    {
      IShellFolder sf = (IShellFolder) null;
      try
      {
        PInvoke.SHGetDesktopFolder(out sf);
        this.AllViews((Action<CustomViewBase>) (v {
        {
          using (ShellItem shellItem = v.ShellItem)
          {
            if (!(pidl == IntPtr.Zero) && !ShellMethods.CompareIDs(shellItem.PIDL, pidl, sf))
              return;
            v.RefreshViewWatermark(false);
          }
        }));
      }
      finally
      {
        if (sf != null)
          Marshal.ReleaseComObject((object) sf);
      }
    }

    private static void RefreshStaticVerbs()
    {
      if (Config.Bool(Scts.NoFolderVerbs))
      {
        ShellExtHelper.InstallFolderStaticVerbs(false);
        ShellExtHelper.ActivateFolderStaticVerbs(false);
      }
      else
      {
        ShellExtHelper.InstallFolderStaticVerbs(true);
        ShellExtHelper.ActivateFolderStaticVerbs(!ExplorerManager.ToolbarManager.AllTabBarDisabled && Config.Bool(Scts.CaptureWindow));
      }
    }

    public void SendToTasktray() { ExplorerManager.sendToTaskTrayCore(this, false);

    public void RestoreFromTray()
    {
      if (ExplorerManager.dicNotifyIcon == null || !ExplorerManager.dicNotifyIcon.ContainsKey(this))
        return;
      ExplorerManager.sendToTaskTrayCore(this, true);
    }

    public bool IsInTray() { ExplorerManager.dicNotifyIcon != null && ExplorerManager.dicNotifyIcon.ContainsKey(this);

    public static void RestoreAllWindowFromTray()
    {
      if (ExplorerManager.dicNotifyIcon == null)
        return;
      foreach (ExplorerManager key in new Dictionary<ExplorerManager, SHOWWINDOW>((IDictionary<ExplorerManager, SHOWWINDOW>) ExplorerManager.dicNotifyIcon).Keys)
        ExplorerManager.sendToTaskTrayCore(key, true);
    }

    private static void CreateNotifyIcon(object o)
    {
      ExplorerManager.contextMenuNotifyIcon = new ContextMenuStripEx((IContainer) null);
      ExplorerManager.contextMenuNotifyIcon.Items.Add("dummy");
      ExplorerManager.contextMenuNotifyIcon.ImageList = Graphic.ImageList;
      ExplorerManager.contextMenuNotifyIcon.Opening += new CancelEventHandler(ExplorerManager.contextMenuNotifyIcon_Opening);
      ExplorerManager.contextMenuNotifyIcon.ItemClicked += new ToolStripItemClickedEventHandler(ExplorerManager.contextMenuNotifyIcon_ItemClicked);
      ExplorerManager.contextMenuNotifyIcon.CreateControl();
      ExplorerManager.fNotifyIconContextMenued = false;
      if (ExplorerManager.icoNotify == null)
        ExplorerManager.icoNotify = Graphic.GetIcon(string.Empty, false);
      ExplorerManager.notifyIcon = new NotifyIcon();
      ExplorerManager.notifyIcon.Icon = ExplorerManager.icoNotify;
      ExplorerManager.notifyIcon.ContextMenuStrip = (ContextMenuStrip) ExplorerManager.contextMenuNotifyIcon;
      ExplorerManager.notifyIcon.MouseClick += new MouseEventHandler(ExplorerManager.notifyIcon_MouseClick);
      ExplorerManager.notifyIcon.MouseDoubleClick += new MouseEventHandler(ExplorerManager.notifyIcon_MouseDoubleClick);
      ExplorerManager.explorerManager_NotifyIconOwner = o as ExplorerManager;
      ExplorerManager.fMergingAllWindow = false;
      if (ExplorerManager.dicNotifyIcon == null || ExplorerManager.dicNotifyIcon.Count <= 0)
        return;
      ExplorerManager.notifyIcon.Visible = true;
      ExplorerManager.CreateContextMenuItemsForNotifyIcon();
      ExplorerManager.RefreshNotifyIconText();
    }

    private static void sendToTaskTrayCore(ExplorerManager explorerManager, bool fRestoreWindow)
    {
      ExplorerManager.fNotifyIconChanged = true;
      if (ExplorerManager.dicNotifyIcon == null)
        ExplorerManager.dicNotifyIcon = new ConcurrentDictionary<ExplorerManager, SHOWWINDOW>();
      if (ExplorerManager.notifyIcon == null)
        ExplorerManager.CreateNotifyIcon((object) explorerManager);
      o = (ITaskbarList) null;
      try
      {
        object ppv;
        PInvoke.CoCreateInstance(COMGUIDS.CLSID_TaskbarList, IntPtr.Zero, CLSCTX.INPROC_SERVER, COMGUIDS.IID_ITaskbarList, out ppv);
        if (!(ppv is ITaskbarList o) || !MCR.SUCCEEDED(o.HrInit()))
          return;
        if (fRestoreWindow)
        {
          SHOWWINDOW nCmdShow;
          if (!ExplorerManager.dicNotifyIcon.TryRemove(explorerManager, out nCmdShow))
            nCmdShow = SHOWWINDOW.SHOWNORMAL;
          o.AddTab(explorerManager.hwndExplorer);
          PInvoke.ShowWindow(explorerManager.hwndExplorer, nCmdShow);
          PInvoke.SetForegroundWindow(explorerManager.hwndExplorer);
          if (ExplorerManager.dicNotifyIcon.Count == 0)
            ExplorerManager.notifyIcon.Visible = false;
        }
        else
        {
          PInvoke.ShowWindow(explorerManager.hwndExplorer, SHOWWINDOW.HIDE);
          o.DeleteTab(explorerManager.hwndExplorer);
          ExplorerManager.notifyIcon.Visible = true;
          ExplorerManager.dicNotifyIcon[explorerManager] = PInvoke.IsZoomed(explorerManager.hwndExplorer) ? SHOWWINDOW.SHOWMAXIMIZED : SHOWWINDOW.SHOWNORMAL;
        }
        if (!ExplorerManager.notifyIcon.Visible)
          return;
        if (ExplorerManager.explorerManager_NotifyIconOwner == explorerManager && !ExplorerManager.fNotifyIconContextMenued)
          ExplorerManager.CreateContextMenuItemsForNotifyIcon();
        ExplorerManager.RefreshNotifyIconText();
      }
      finally
      {
        if (o != null)
          Marshal.ReleaseComObject((object) o);
      }
    }

    private static void RefreshNotifyIconText()
    {
      int count = ExplorerManager.dicNotifyIcon.Count;
      string str = count.ToString() + StringResources.Pluralize(StringResources.Global.NotifyIcon[2], count);
      if (str.Length > 63)
        str = str.Substring(0, 60) + "...";
      ExplorerManager.notifyIcon.Text = str;
    }

    private static void CreateContextMenuItemsForNotifyIcon()
    {
      if (!ExplorerManager.fNotifyIconChanged && ExplorerManager.contextMenuNotifyIcon.Items.Count != 1)
        return;
      ExplorerManager.fNotifyIconChanged = false;
      ExplorerManager.contextMenuNotifyIcon.SuspendLayout();
      while (ExplorerManager.contextMenuNotifyIcon.Items.Count > 0)
        ExplorerManager.contextMenuNotifyIcon.Items[0].Dispose();
      foreach (ExplorerManager key in (IEnumerable<ExplorerManager>) ExplorerManager.dicNotifyIcon.Keys)
      {
        string windowText = PInvoke.GetWindowText(key.hwndExplorer);
        if (!string.IsNullOrEmpty(windowText))
        {
          ToolStripMenuItem toolStripMenuItem1 = (ToolStripMenuItem) new QMenuItem(windowText, MenuGenre.None);
          toolStripMenuItem1.Tag = (object) key;
          QTTabBarClass tabBar = key.Toolbars.TabBar;
          if (tabBar != null)
          {
            toolStripMenuItem1.ToolTipText = tabBar.SelectedTabItem.ToolTipText;
            toolStripMenuItem1.ImageKey = Graphic.GetImageKey(tabBar.SelectedTabItem.CurrentPath, tabBar.SelectedTabItem.CurrentIDL, (string) null, (string) null, Graphic.ImageList, false);
            if (tabBar.TabItems.Count > 1)
            {
              for (int index = 0; index < tabBar.TabItems.Count; ++index)
              {
                QTabItem tabItem = tabBar.TabItems[index];
                ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(tabItem.Text);
                toolStripMenuItem2.Tag = (object) key;
                toolStripMenuItem2.ToolTipText = PathString.MakeTooltipText(tabItem.CurrentPath, tabItem.CurrentIDL);
                toolStripMenuItem2.ImageKey = Graphic.GetImageKey(tabItem.CurrentPath, tabItem.CurrentIDL, (string) null, (string) null, Graphic.ImageList, false);
                toolStripMenuItem1.DropDownItems.Add((ToolStripItem) toolStripMenuItem2);
              }
              if (toolStripMenuItem1.DropDownItems.Count > 0)
              {
                toolStripMenuItem1.DropDownItemClicked += new ToolStripItemClickedEventHandler(ExplorerManager.contextMenuNotifyIcon_SubItems_DropDownItemClicked);
                toolStripMenuItem1.DropDown.ImageList = Graphic.ImageList;
              }
            }
          }
          ExplorerManager.contextMenuNotifyIcon.Items.Add((ToolStripItem) toolStripMenuItem1);
        }
      }
      ExplorerManager.contextMenuNotifyIcon.Items.Add((ToolStripItem) new ToolStripSeparatorEx());
      ExplorerManager.contextMenuNotifyIcon.Items.Add(StringResources.Global.NotifyIcon[0]);
      ExplorerManager.contextMenuNotifyIcon.Items.Add(StringResources.Global.NotifyIcon[1]);
      ExplorerManager.contextMenuNotifyIcon.ResumeLayout();
    }

    private static void notifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      ExplorerManager.RestoreAllWindowFromTray();
    }

    private static void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      ExplorerManager.RestoreAllWindowFromTray();
    }

    private static void contextMenuNotifyIcon_Opening(object sender, CancelEventArgs e)
    {
      ExplorerManager.fNotifyIconContextMenued = true;
      ExplorerManager.CreateContextMenuItemsForNotifyIcon();
    }

    private static void contextMenuNotifyIcon_ItemClicked(
      object sender,
      ToolStripItemClickedEventArgs e)
    {
      if (e.ClickedItem is ToolStripSeparator)
        return;
      if (e.ClickedItem.Tag == null)
      {
        switch (ExplorerManager.contextMenuNotifyIcon.Items.Count - ExplorerManager.contextMenuNotifyIcon.Items.IndexOf(e.ClickedItem))
        {
          case 1:
            ExplorerManager.notifyIcon.Visible = false;
            ExplorerManager.contextMenuNotifyIcon.Hide();
            bool flag = false;
            foreach (ExplorerManager key in new Dictionary<ExplorerManager, SHOWWINDOW>((IDictionary<ExplorerManager, SHOWWINDOW>) ExplorerManager.dicNotifyIcon).Keys)
            {
              if (key != ExplorerManager.explorerManager_NotifyIconOwner)
              {
                WindowUtil.CloseExplorer(key.hwndExplorer, WMCLOSEARG.CloseWindow);
                Thread.Sleep(100);
              }
              else
                flag = true;
            }
            if (!flag)
              break;
            WindowUtil.CloseExplorer(ExplorerManager.explorerManager_NotifyIconOwner.hwndExplorer, WMCLOSEARG.CloseWindow);
            break;
          case 2:
            ExplorerManager.RestoreAllWindowFromTray();
            break;
        }
      }
      else
      {
        if (!(e.ClickedItem.Tag is ExplorerManager tag))
          return;
        ExplorerManager.sendToTaskTrayCore(tag, true);
      }
    }

    private static void contextMenuNotifyIcon_SubItems_DropDownItemClicked(
      object sender,
      ToolStripItemClickedEventArgs e)
    {
      ExplorerManager tag = (ExplorerManager) e.ClickedItem.Tag;
      int wParam = ((ToolStripDropDownItem) sender).DropDownItems.IndexOf(e.ClickedItem);
      if (wParam <= -1)
        return;
      ExplorerManager.sendToTaskTrayCore(tag, true);
      if (!(tag.Toolbars.TabBarHandle != IntPtr.Zero))
        return;
      PInvoke.SendMessage(tag.Toolbars.TabBarHandle, 32770, (IntPtr) wParam, IntPtr.Zero);
    }

    private bool ShowTabSwitcher(bool fShift, bool fRepeat)
    {
      TabBarBase tabBarBase;
      if (this.tabSwitcher != null && this.tabSwitcher.Visible)
      {
        tabBarBase = this.tabSwitcher.TabBar;
      }
      else
      {
        TargetView targetView;
        this.GetFocusedView(out targetView);
        if (targetView == TargetView.None)
          targetView = TargetView.Default;
        tabBarBase = this.GetTargetTabBar(targetView);
      }
      if (tabBarBase != null && tabBarBase.TabManager.IsActiveTabAvailable && tabBarBase.TabManager.Count > 1)
      {
        if (this.tabSwitcher == null)
        {
          this.tabSwitcher = new TabSwitchForm();
          this.tabSwitcher.Switched += new ItemCheckEventHandler(this.tabSwitcher_Switched);
          this.tabSwitcher.VisibleChanged += new EventHandler(this.tabSwitcher_VisibleChanged);
        }
        if (!this.tabSwitcher.Visible)
        {
          List<PathData> lstPaths = new List<PathData>();
          string str = Config.Bool(Scts.TabAutoSuffix) ? " @ " : " : ";
          foreach (QTabItem qtabItem in tabBarBase.TabManager.Items)
          {
            string strDisplay = string.IsNullOrEmpty(qtabItem.Comment) ? qtabItem.Text : qtabItem.Text + str + qtabItem.Comment;
            lstPaths.Add(new PathData(strDisplay, qtabItem.CurrentPath, qtabItem.CurrentIDL, qtabItem.ImageKey));
          }
          this.tabSwitcher.TabBar = tabBarBase;
          this.tabSwitcher.ShowSwitcher(this.hwndExplorer, tabBarBase.TabManager.SelectedIndex, lstPaths);
        }
        int index = this.tabSwitcher.Switch(fShift);
        if (!fRepeat || tabBarBase.TabManager.Count < 13)
          tabBarBase.TabManager.SetPseudoHotIndex(index);
      }
      return false;
    }

    private void HideTabSwitcher(bool fSwitch)
    {
      if (this.tabSwitcher == null || !this.tabSwitcher.Visible)
        return;
      this.tabSwitcher.HideSwitcher(fSwitch);
    }

    private void tabSwitcher_Switched(object sender, ItemCheckEventArgs e)
    {
      if (this.tabSwitcher.TabBar == null)
        return;
      this.tabSwitcher.TabBar.TabManager.SelectedIndex = e.Index;
    }

    private void tabSwitcher_VisibleChanged(object sender, EventArgs e)
    {
      if (this.tabSwitcher.Visible)
        return;
      foreach (TabBarBase tabBar in this.Toolbars.TabBars)
        tabBar.TabManager.SetPseudoHotIndex(-1);
    }

    private void OnSelectionChanged()
    {
      bool flag = false;
      if (this.ExplorerManagerEvent != null)
      {
        try
        {
          ExplorerManagerEventArgs e = new ExplorerManagerEventArgs(ExplorerManagerEventKind.SelectionChanged, new Message());
          this.ExplorerManagerEvent((object) this, e);
          flag = e.Handled;
        }
        catch
        {
        }
      }
      if (!flag)
      {
        try
        {
          using (ShellItem shellItem = new ShellItem(this.currentIDL, false))
            this.PluginManager.FireEvent(new PluginEventInfo(PluginEvents.SelectionChanged, -1, this.currentIDL, shellItem.Path));
        }
        catch
        {
        }
      }
      this.NotifySelectionChangeToFileRenamer(TargetView.Default);
    }

    public bool TrySetSelection(Address[] addresses, string pathToFocus, bool fDeselectOthers)
    {
      if (addresses == null)
        addresses = new Address[0];
      IShellView ppshv = (IShellView) null;
      try
      {
        if (this.shellBrowser.QueryActiveShellView(out ppshv) == 0)
        {
          bool flag1 = true;
          bool flag2 = false;
          bool flag3 = !string.IsNullOrEmpty(pathToFocus);
          if (fDeselectOthers && addresses.Length == 0 && ppshv is IFolderView folderView)
            folderView.SelectItem(0, SVSIF.DESELECTOTHERS);
          foreach (Address address in addresses)
          {
            using (ShellItem idlw = PluginUtils.AddressToIDLW(address))
            {
              if (idlw.Available)
              {
                SVSIF uFlags = SVSIF.SELECT;
                if (flag1)
                {
                  uFlags |= SVSIF.ENSUREVISIBLE;
                  if (!flag3)
                  {
                    flag2 = true;
                    uFlags |= SVSIF.FOCUSED;
                  }
                  if (fDeselectOthers)
                    uFlags |= SVSIF.DESELECTOTHERS;
                  flag1 = false;
                }
                if (!flag2 & flag3 && string.Equals(address.Path, pathToFocus, StringComparison.OrdinalIgnoreCase))
                {
                  flag2 = true;
                  uFlags |= SVSIF.FOCUSED;
                }
                ppshv.SelectItem(idlw.LastPIDL, uFlags);
              }
            }
          }
          if (!flag2 & flag3)
          {
            using (ShellItem shellItem = new ShellItem(pathToFocus))
            {
              if (shellItem.Available)
                ppshv.SelectItem(shellItem.LastPIDL, SVSIF.ENSUREVISIBLE | SVSIF.FOCUSED);
            }
          }
          return true;
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
        if (ppshv != null)
          Marshal.ReleaseComObject((object) ppshv);
      }
      return false;
    }

    public ItemIDList PreviousMarkedItem { get; set; }

    private IEnumerable<string> fileOp_GetSelection(TargetView targetView) { (IEnumerable<string>) this.Invoke((Delegate) (() {
    {
      List<string> selection = new List<string>();
      CustomViewBase view = this.GetView(targetView);
      if (view != null)
      {
        foreach (ShellItem shellItem in (IEnumerable<ShellItem>) view.GetSelection())
        {
          try
          {
            using (shellItem)
            {
              if (shellItem.IsFileSystem)
              {
                if (shellItem.HasPath)
                  selection.Add(shellItem.Path);
              }
            }
          }
          catch
          {
          }
        }
      }
      return (IEnumerable<string>) selection;
    }));

    private void fileOp_Undo() { this.InvokeMenuCommand(ExplorerMenuCommand.Undo);

    public static Bitmap GetWatermarkImage(BmpCacheKey key) { ExplorerManager.watermarkImageCache[key];

    public void ViewFocusChanged(bool fGotFocus, IntPtr hwnd, TargetView sourceView)
    {
      if (fGotFocus)
      {
        BuiltinButtonRefreshes mask = BuiltinButtonRefreshes.TabCloseButtons | BuiltinButtonRefreshes.TravelButtons | BuiltinButtonRefreshes.DisableGoUpOneLevel | BuiltinButtonRefreshes.ActiveView;
        int num;
        switch (sourceView)
        {
          case TargetView.Second:
            num = 2;
            break;
          case TargetView.Third:
            num = 4;
            break;
          default:
            num = 1;
            break;
        }
        BuiltinButtonRefreshSource source = (BuiltinButtonRefreshSource) num;
        this.SyncTabBarAndCommandBars(mask, source);
        this.PluginManager.FireEvent(new PluginEventInfo(PluginEvents.ActiveViewChanged)
        {
          TargetView = sourceView
        });
        this.PreviousActiveTargetView = sourceView;
      }
      else
      {
        TargetView targetView = TargetView.None;
        foreach (CustomViewBase view in this.Toolbars.Views)
        {
          if (hwnd == view.ListViewHandle)
          {
            targetView = view.TargetView;
            break;
          }
        }
        if (targetView != TargetView.None)
          return;
        this.PluginManager.FireEvent(new PluginEventInfo(PluginEvents.ActiveViewChanged)
        {
          TargetView = targetView
        });
      }
    }

    private void AllViews(Action<CustomViewBase> action)
    {
      action((CustomViewBase) this.View);
      if (this.Toolbars.IsSecondViewBarAvailable)
      {
        foreach (QTabItem qtabItem in this.Toolbars.SecondViewBar.TabManager.Items)
          action(qtabItem.AssociatedView);
      }
      if (!this.Toolbars.IsThirdViewBarAvailable)
        return;
      foreach (QTabItem qtabItem in this.Toolbars.ThirdViewBar.TabManager.Items)
        action(qtabItem.AssociatedView);
    }

    private void View_CustomViewEvent(object sender, CustomViewEventArgs e)
    {
      switch (e.Kind)
      {
        case CustomViewEventKind.EnterKeyDown:
          if (this.ExplorerManagerEvent != null)
            this.ExplorerManagerEvent((object) this, new ExplorerManagerEventArgs(ExplorerManagerEventKind.EnterKeyDownOnListView, e.Message));
          e.Handled = this.OnViewEnterKeyDown();
          break;
        case CustomViewEventKind.EnterModal:
          this.InModal = true;
          break;
        case CustomViewEventKind.ExitModal:
          this.InModal = false;
          break;
        case CustomViewEventKind.BrowserRefresh:
          this.NotifyBrowserRefresh(false);
          break;
        case CustomViewEventKind.XButtonDown:
          if (this.ExplorerManagerEvent == null)
            break;
          ExplorerManagerEventArgs e1 = new ExplorerManagerEventArgs(ExplorerManagerEventKind.XButtonDown, e.Message);
          this.ExplorerManagerEvent((object) this, e1);
          e.Handled = e1.Handled;
          break;
        case CustomViewEventKind.XButtonUp:
          if (this.ExplorerManagerEvent == null)
            break;
          ExplorerManagerEventArgs e2 = new ExplorerManagerEventArgs(ExplorerManagerEventKind.XButtonUp, e.Message);
          this.ExplorerManagerEvent((object) this, e2);
          e.Handled = e2.Handled;
          break;
        case CustomViewEventKind.ViewDoubleClicked:
          if (this.ExplorerManagerEvent != null)
            this.ExplorerManagerEvent((object) this, new ExplorerManagerEventArgs(ExplorerManagerEventKind.DoubleClickOnListView, e.Message));
          e.Handled = this.OnViewDoubleClick(e);
          break;
        case CustomViewEventKind.ViewMiddleClicked:
          e.Handled = this.OnViewMiddleClick(e);
          break;
        case CustomViewEventKind.BrowsingBack:
        case CustomViewEventKind.BrowsingForward:
          if (!this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
            break;
          this.ViewSyncManager.OnBrowsingBack((CustomViewBase) this.View, e.Kind == CustomViewEventKind.BrowsingBack);
          break;
        case CustomViewEventKind.BrowsingBackPending:
          CustomViewEventArgs customViewEventArgs1 = e;
          CommandInfo info1 = new CommandInfo(BarCommand.GoBack);
          info1.Options = BarCommandOptions.NoFocusView;
          int num1 = this.InvokeCommand(info1) ? 1 : 0;
          customViewEventArgs1.Return = num1 != 0;
          break;
        case CustomViewEventKind.BrowsingForwardPending:
          CustomViewEventArgs customViewEventArgs2 = e;
          CommandInfo info2 = new CommandInfo(BarCommand.GoForward);
          info2.Options = BarCommandOptions.NoFocusView;
          int num2 = this.InvokeCommand(info2) ? 1 : 0;
          customViewEventArgs2.Return = num2 != 0;
          break;
      }
    }

    private void View_TipControlEvent(object sender, TipControlEventArgs e)
    {
      switch (e.Kind)
      {
        case TipControlEventKind.MenuItemClicked:
          this.OnSubFolderMenuItemClicked(e);
          break;
        case TipControlEventKind.MenuItemRightClicked:
          this.OnSubFolderMenuItemRightClicked(e);
          break;
        case TipControlEventKind.MenuClosed:
          this.OnSubFolderMenuClosed(e.Source);
          break;
        case TipControlEventKind.MouseLeaving:
          this.OnSubFolderMenuMouseLeaving(e.Source);
          break;
        case TipControlEventKind.MouseWheelOnTip:
          this.OnSubFolderMenuMouseWheelEx(e.Source, (MouseEventArgs) e.InnerEventArgs);
          break;
      }
    }

    private void View_Scrolled(object sender, ViewScrollEventArgs e)
    {
      if (!this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Scroll))
        return;
      this.ViewSyncManager.OnScroll((CustomViewBase) this.View, e);
    }

    public CustomViewBase GetView(TargetView targetView)
    {
      switch (targetView)
      {
        case TargetView.Default:
          return (CustomViewBase) this.View;
        case TargetView.Second:
          if (this.Toolbars.IsSecondViewBarTabAvailable)
            return (CustomViewBase) this.Toolbars.SecondViewBar.ActiveTab.View;
          break;
        case TargetView.Third:
          if (this.Toolbars.IsThirdViewBarTabAvailable)
            return (CustomViewBase) this.Toolbars.ThirdViewBar.ActiveTab.View;
          break;
      }
      return (CustomViewBase) null;
    }

    public CustomViewBase GetFocusedView() { this.GetFocusedView(out TargetView _);

    public CustomViewBase GetFocusedView(out TargetView targetView)
    {
      if (PInvoke.GetForegroundWindow() != this.hwndExplorer)
      {
        CustomViewBase view = this.GetView(this.PreviousActiveTargetView);
        if (view != null)
        {
          targetView = this.PreviousActiveTargetView;
          return view;
        }
      }
      targetView = TargetView.None;
      if (PInvoke.GetFocus() != IntPtr.Zero)
      {
        if (this.View.Focused)
        {
          targetView = TargetView.Default;
          return (CustomViewBase) this.View;
        }
        if (this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.Focused)
        {
          targetView = TargetView.Second;
          return (CustomViewBase) this.Toolbars.SecondViewBar.ActiveTab.View;
        }
        if (this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.Focused)
        {
          targetView = TargetView.Third;
          return (CustomViewBase) this.Toolbars.ThirdViewBar.ActiveTab.View;
        }
      }
      return (CustomViewBase) null;
    }

    public CustomViewBase GetViewFromHwnd(IntPtr hwnd)
    {
      if (this.View != null && hwnd == this.View.ListViewHandle)
        return (CustomViewBase) this.View;
      if (this.Toolbars != null && this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.ListViewHandle == hwnd)
        return (CustomViewBase) this.Toolbars.SecondViewBar.ActiveTab.View;
      return this.Toolbars != null && this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.ListViewHandle == hwnd ? (CustomViewBase) this.Toolbars.ThirdViewBar.ActiveTab.View : (CustomViewBase) null;
    }

    public TabBarBase GetFocusedTabBar()
    {
      if (PInvoke.GetFocus() != IntPtr.Zero)
      {
        if (this.View.Focused)
          return !this.Toolbars.IsTabBarAvailable ? (TabBarBase) null : (TabBarBase) this.Toolbars.TabBar;
        if (this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.Focused)
          return (TabBarBase) this.Toolbars.SecondViewBar;
        if (this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.Focused)
          return (TabBarBase) this.Toolbars.ThirdViewBar;
      }
      return (TabBarBase) null;
    }

    public TargetView GetFocusedTargetView(bool fAllowNone = false)
    {
      TargetView targetView;
      this.GetFocusedView(out targetView);
      if (!fAllowNone && targetView == TargetView.None)
        targetView = TargetView.Default;
      return targetView;
    }

    private TabBarBase GetTargetTabBar(TargetView targetView)
    {
      switch (targetView)
      {
        case TargetView.Default:
          if (this.Toolbars.IsTabBarAvailable)
            return (TabBarBase) this.Toolbars.TabBar;
          break;
        case TargetView.Second:
          if (this.Toolbars.IsSecondViewBarAvailable)
            return (TabBarBase) this.Toolbars.SecondViewBar;
          break;
        case TargetView.Third:
          if (this.Toolbars.IsThirdViewBarAvailable)
            return (TabBarBase) this.Toolbars.ThirdViewBar;
          break;
      }
      return (TabBarBase) null;
    }

    public IEnumerable<ExtraView> GetExtraViews(TargetView targetView)
    {
      switch (targetView)
      {
        case TargetView.Second:
          if (this.Toolbars.IsSecondViewBarAvailable)
            return this.Toolbars.SecondViewBar.TabManager.Items.OfType<QTabItemWithView>().Select<QTabItemWithView, ExtraView>((Func<QTabItemWithView, ExtraView>) (t { t.View));
          break;
        case TargetView.Third:
          if (this.Toolbars.IsThirdViewBarAvailable)
            return this.Toolbars.ThirdViewBar.TabManager.Items.OfType<QTabItemWithView>().Select<QTabItemWithView, ExtraView>((Func<QTabItemWithView, ExtraView>) (t { t.View));
          break;
      }
      return Enumerable.Empty<ExtraView>();
    }

    public ExtraView GetFirstExtraView()
    {
      bool flag1 = Config.Bool(Scts.ExtraViewPriority);
      bool flag2 = flag1 ? this.Toolbars.IsThirdViewBarTabAvailable : this.Toolbars.IsSecondViewBarTabAvailable;
      bool flag3 = flag1 ? this.Toolbars.IsSecondViewBarTabAvailable : this.Toolbars.IsThirdViewBarTabAvailable;
      if (flag2)
        return !flag1 ? this.Toolbars.SecondViewBar.ActiveTab.View : this.Toolbars.ThirdViewBar.ActiveTab.View;
      if (!flag3)
        return (ExtraView) null;
      return !flag1 ? this.Toolbars.ThirdViewBar.ActiveTab.View : this.Toolbars.SecondViewBar.ActiveTab.View;
    }

    private bool AnyViewHasFocus { this.GetFocusedView() != null;

    private bool AnyViewRenaming
    {
      get
      {
        if (this.View.Renaming || this.Toolbars.IsSecondViewBarTabAvailable && this.Toolbars.SecondViewBar.ActiveTab.View.Renaming)
          return true;
        return this.Toolbars.IsThirdViewBarTabAvailable && this.Toolbars.ThirdViewBar.ActiveTab.View.Renaming;
      }
    }

    public ViewSyncManager ViewSyncManager { get; private set; }

    private void ViewSyncManager_SyncCanceled(object sender, EventArgs e)
    {
      if (this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
        MessageForm.Show(this.hwndExplorer, StringResources.Global.ViewSync[3], "QTTabBar", MessageBoxIcon.Asterisk, 3000);
      this.ViewSyncManager.SynchronizationLevel &= ~ViewSynchronizationLevel.Navigation;
    }

    public void OnGoingUp()
    {
      if (!this.ViewSyncManager.SynchronizationLevel.HasFlag((Enum) ViewSynchronizationLevel.Navigation))
        return;
      this.ViewSyncManager.OnGoingUp();
    }

    public ExplorerManager.ToolbarManager Toolbars { this.toolbarManager;

    public IShellBrowser ShellBrowser { this.shellBrowser;

    public SHDocVw.WebBrowser Explorer { this.explorer;

    public bool ExplorerIsBusy { !this.fNowClosing && this.explorer != null && this.explorer.Busy;

    public IntPtr ExplorerHandle { this.hwndExplorer;

    public PluginManager PluginManager { this.pluginManager;

    public int ThreadID { get; private set; }

    public int ProcessID { get; private set; }

    public IntPtr ListViewHandle { this.hwndListView;

    public IntPtr ShellTabWindow { this.hwndShellTab;

    public void SetUserEventData(
      int key,
      Tuple<SlotDataType, object> data,
      EventSource source,
      int keyState,
      TargetView targetView)
    {
      if (this.dicUserEventData == null)
        this.dicUserEventData = new Dictionary<int, EventData>();
      this.dicUserEventData[key] = new EventData(data, source, keyState, targetView);
    }

    private Tuple<SlotDataType, object> GetUserEventData(int key)
    {
      EventData eventData;
      return this.dicUserEventData != null && this.dicUserEventData.TryGetValue(key, out eventData) ? eventData.Data : (Tuple<SlotDataType, object>) null;
    }

    private EventSource GetUserEventSource(int key)
    {
      EventData eventData;
      return this.dicUserEventData != null && this.dicUserEventData.TryGetValue(key, out eventData) ? eventData.Source : EventSource.NotAvailable;
    }

    private int GetUserEventKeyState(int key)
    {
      EventData eventData;
      return this.dicUserEventData != null && this.dicUserEventData.TryGetValue(key, out eventData) ? eventData.KeyState : 0;
    }

    private TargetView GetUserEventTargetView(int key)
    {
      EventData eventData;
      return this.dicUserEventData != null && this.dicUserEventData.TryGetValue(key, out eventData) ? eventData.TargetView : TargetView.None;
    }

    public void NotifyNavigationToFileRenamer(byte[] idl, TargetView view)
    {
      try
      {
        if (this.fileRenameDialog == null || !this.fileRenameDialog.Visible)
          return;
        this.fileRenameDialog.NotifyNavigation(idl, view);
      }
      catch (Exception ex)
      {
      }
    }

    public void NotifySelectionChangeToFileRenamer(TargetView view)
    {
      try
      {
        if (this.fileRenameDialog == null || !this.fileRenameDialog.Visible)
          return;
        this.fileRenameDialog.NotifySelectionChange(view);
      }
      catch (Exception ex)
      {
      }
    }

    [Conditional("DEBUG")]
    public void DebugButtonPressed()
    {
      string str1 = "Desktop: \t" + PInvoke.GetDpiForWindow(WindowUtil.GetDesktopHwnd()).ToString() + "\r\nDefault: \t" + DpiManager.DefaultDpi.ToString() + "\r\nThis: \t" + PInvoke.GetDpiForWindow(this.ExplorerHandle).ToString() + "\r\nVer: \t" + QEnvironment.OSVersion?.ToString() + "\r\nIn Night mode: " + QEnvironment.InNightMode.ToString() + "\r\nSmallCaptionFont size: \t" + SystemFonts.SmallCaptionFont.SizeInPoints.ToString() + "\r\nIcon title Font size: \t" + SystemFonts.IconTitleFont.SizeInPoints.ToString() + "\r\nMenu Font size: \t" + SystemFonts.MenuFont.SizeInPoints.ToString() + "\r\nDefaultFont size: " + SystemFonts.DefaultFont.SizeInPoints.ToString();
      using (Graphics graphics = Graphics.FromHwnd(this.Toolbars.CommandBar1stHandle))
        str1 = str1 + "\r\nGraphics.DpiX value : \t" + graphics.DpiX.ToString();
      string str2 = str1 + "\r\nGetDpiForWindow value : \t" + DpiManager.GetDpiForWindow(this.Toolbars.CommandBar1stHandle).ToString() + "\r\n" + "\r\n .net framework : " + Environment.Version.ToString() + "\r\n OS ver, Major:" + QEnvironment.OSVersion.Major.ToString() + ", Minor:" + QEnvironment.OSVersion.Minor.ToString() + ", Build:" + QEnvironment.OSVersion.Build.ToString() + ", Revision:" + QEnvironment.OSVersion.Revision.ToString() + "\r\n Is Windows11 :" + QEnvironment.IsWindows11.ToString();
      IntPtr dc = PInvoke.GetDC(IntPtr.Zero);
      int num1 = 0;
      try
      {
        num1 = PInvoke.GetDeviceCaps(dc, 88);
      }
      finally
      {
        PInvoke.ReleaseDC(IntPtr.Zero, dc);
      }
      string str3 = str2 + "\r\nDPI By DeviceCaps : " + num1.ToString();
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      string version = ((AssemblyFileVersionAttribute) Attribute.GetCustomAttribute(executingAssembly, typeof (AssemblyFileVersionAttribute))).Version;
      int num2 = (int) MessageBox.Show(str3 + "\r\nVersion  : " + version + "\r\nLoaded from GAC  : " + executingAssembly.GlobalAssemblyCache.ToString() + "\r\nBounds: " + this.Toolbars.TabBar.SelectedTabItem.Bounds.ToString() + " --- Config.TabHeight:" + Config.TabHeight.ToString() + ", DPI: " + this.Toolbars.TabBar.tabControl.Dpi.ToString() + "\r\nProcess name: " + ScriptingHelper.IsExplorerProcess.ToString() + "\r\nProcessor Count: " + Environment.ProcessorCount.ToString() + "\r\nVertically maximized ?: " + WindowUtil.IsAtBottomOfScreen(this.hwndExplorer).ToString());
    }

    public sealed class ToolbarManager
    {
      private ExplorerManager explorerManager;
      private QTTabBarClass tabBar;
      private QTTabBarClass tabBarAnother;
      private QCommandBar commandBar1st;
      private QCommandBar2nd commandBar2nd;
      private QCommandBarVertical commandBarVrt;
      private QManagementBar managementBar;
      private QSecondViewBar secondViewBar;
      private QThirdViewBar thirdViewBar;
      private bool fNowHiding3rdViewBar;

      public ToolbarManager(ExplorerManager explorerManager) { this.explorerManager = explorerManager;

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
              str = typeof (QCommandBar).GUID.ToString("B");
              break;
            case Toolbar.CommandBar2:
              str = typeof (QCommandBar2nd).GUID.ToString("B");
              break;
            case Toolbar.CommandBarVertical:
              if (this.IsThirdViewBarVisible && !this.fNowHiding3rdViewBar)
              {
                if (fShow)
                {
                  this.ThirdViewBar.ShowVerticalCommandBar();
                  return;
                }
                this.ThirdViewBar.HideVerticalCommandBar();
                return;
              }
              this.save3rdViewBarWidth();
              fSaveBandLayout = false;
              str = typeof (QCommandBarVertical).GUID.ToString("B");
              break;
            case Toolbar.BottomTabBar:
              fSaveBandLayout = false;
              str = typeof (QTHorizontalExplorerBar).GUID.ToString("B");
              break;
            case Toolbar.ManagementBar:
              fSaveBandLayout = false;
              if (!fShow)
                this.NowHidingManagementBarByCode = true;
              str = typeof (QManagementBar).GUID.ToString("B");
              break;
            case Toolbar.SecondViewBar:
              fSaveBandLayout = false;
              str = typeof (QSecondViewBar).GUID.ToString("B");
              break;
            case Toolbar.ThirdViewBar:
              fSaveBandLayout = false;
              str = typeof (QThirdViewBar).GUID.ToString("B");
              break;
          }
          if (str != null)
          {
            object obj1 = (object) str;
            object obj2 = (object) fShow;
            try
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
            }
          }
          if (!fSaveBandLayout)
            return;
          this.explorerManager.SaveCurrentBandLayout();
        }
        finally
        {
          this.fNowHiding3rdViewBar = false;
        }
      }

      public void Toggle(Toolbar toolbar)
      {
        switch (toolbar)
        {
          case Toolbar.TabBar:
            this.Show(Toolbar.TabBar, !this.IsTabBarVisible);
            break;
          case Toolbar.CommandBar1:
            this.Show(toolbar, !this.IsCommandBar1stVisible);
            break;
          case Toolbar.CommandBar2:
            this.Show(toolbar, !this.IsCommandBar2ndVisible);
            break;
          case Toolbar.CommandBarVertical:
            this.Show(toolbar, !this.IsCommandBarVrtVisible && !this.IsVerticalCommandBarEnabledInThirdView);
            break;
          case Toolbar.BottomTabBar:
            this.Show(Toolbar.BottomTabBar, !this.IsBottomTabBarVisible);
            break;
          case Toolbar.SecondViewBar:
            this.ShowSecondView(!this.IsSecondViewBarVisible);
            break;
          case Toolbar.ThirdViewBar:
            this.ShowThirdView(!this.IsThirdViewBarVisible);
            break;
        }
      }

      public bool AllBarDisabled { !ExplorerManager.TabBarEnabled && !this.IsCommandBar1stAvailable && !this.IsCommandBar2ndAvailable && !Config.Bool(Scts.CommandBar_VerticalEnabled) && !Config.Bool(Scts.ExtraViewBarEnabled2nd) && !Config.Bool(Scts.ExtraViewBarEnabled3rd);

      public static bool AllTabBarDisabled { !ExplorerManager.TabBarEnabled && !Config.Bool(Scts.ExtraViewBarEnabled2nd) && !Config.Bool(Scts.ExtraViewBarEnabled3rd);

      public bool ManagementBarIsRequired { Config.Bool(Scts.BottomTabBarEnabled) ? !this.IsCommandBar1stVisible && !this.IsCommandBar2ndVisible : (Config.Bool(Scts.CommandBar_VerticalEnabled) || Config.Bool(Scts.ExtraViewBarEnabled2nd) || Config.Bool(Scts.ExtraViewBarEnabled3rd) ? !this.IsTabBarVisible && !this.IsCommandBar1stVisible && !this.IsCommandBar2ndVisible : (this.ManagementBar == null || !this.ManagementBar.NowUserHidManagementBar) && !this.IsTabBarVisible && !this.IsCommandBar1stVisible && !this.IsCommandBar2ndVisible);

      public bool NowHidingManagementBarByCode { get; set; }

      public bool AllToolbarReleased { this.tabBar == null && this.commandBar1st == null && this.commandBar2nd == null && this.commandBarVrt == null && this.managementBar == null && this.secondViewBar == null && this.thirdViewBar == null;

      private static bool IsToolBarAvailableCore(BandObject bo) { bo != null && bo.IsHandleCreated && !bo.IsDisposed;

      private static bool IsToolBarVisibleCore(BandObject bo) { ExplorerManager.ToolbarManager.IsToolBarAvailableCore(bo) && bo.Shown;

      public IEnumerable<BandObject> AllBandObjects
      {
        get
        {
          if (this.IsTabBarAvailable)
            yield return (BandObject) this.TabBar;
          if (this.IsCommandBar1stAvailable)
            yield return (BandObject) this.CommandBar1st;
          if (this.IsCommandBar2ndAvailable)
            yield return (BandObject) this.CommandBar2nd;
          if (this.IsCommandBarVrtAvailable)
            yield return (BandObject) this.CommandBarVrt;
          if (this.IsSecondViewBarAvailable)
            yield return (BandObject) this.SecondViewBar;
          if (this.IsThirdViewBarAvailable)
            yield return (BandObject) this.ThirdViewBar;
        }
      }

      public QTTabBarClass TabBar
      {
        get { this.tabBar;
        set
        {
          if (this.tabBar == value)
            return;
          if (this.tabBar != null)
            this.tabBarAnother = value != null ? this.tabBar : (QTTabBarClass) null;
          this.tabBar = value;
          if (value == null)
          {
            this.TabBarHandle = IntPtr.Zero;
            this.explorerManager.OnInstanceUnregistered(false);
          }
          else
            this.TabBarHandle = value.Handle;
        }
      }

      public QTTabBarClass AnotherTabBar { this.tabBarAnother;

      public IntPtr TabBarHandle { get; set; }

      public bool IsTabBarVisible { this.IsTabBarAvailable && this.TabBar.Shown;

      public bool IsTopTabBarAvailable { this.IsTabBarAvailable && !this.TabBar.IsBottomBar;

      public bool IsTopTabBarVisible { this.IsTabBarVisible && !(this.TabBar is QTHorizontalExplorerBar);

      public bool IsBottomTabBarVisible { this.IsTabBarVisible && this.TabBar is QTHorizontalExplorerBar;

      public bool IsTabBarAvailable { this.TabBar != null && this.TabBar.IsHandleCreated && !this.TabBar.IsDisposed && this.TabBarHandle != IntPtr.Zero;

      public bool IsBottomTabBarAvailable { this.IsTabBarAvailable && this.TabBar is QTHorizontalExplorerBar;

      public IEnumerable<TabBarBase> TabBars
      {
        get
        {
          if (this.IsTabBarAvailable)
            yield return (TabBarBase) this.TabBar;
          if (this.IsSecondViewBarVisible)
            yield return (TabBarBase) this.SecondViewBar;
          if (this.IsThirdViewBarVisible)
            yield return (TabBarBase) this.ThirdViewBar;
        }
      }

      public TabBarBase GetTabBar(TargetView tv, bool fAllowNoTab = false)
      {
        switch (tv)
        {
          case TargetView.Default:
            if (this.IsTabBarAvailable)
              return (TabBarBase) this.TabBar;
            break;
          case TargetView.Second:
            if (this.IsSecondViewBarTabAvailable || fAllowNoTab && this.IsSecondViewBarVisible)
              return (TabBarBase) this.SecondViewBar;
            break;
          case TargetView.Third:
            if (this.IsThirdViewBarTabAvailable || fAllowNoTab && this.IsThirdViewBarVisible)
              return (TabBarBase) this.ThirdViewBar;
            break;
        }
        return (TabBarBase) null;
      }

      public IEnumerable<QTabItem> AllTabs
      {
        get
        {
          if (this.IsTabBarAvailable)
          {
            foreach (QTabItem tabItem in this.TabBar.TabItems)
              yield return tabItem;
          }
          if (this.IsSecondViewBarVisible)
          {
            foreach (QTabItem qtabItem in this.SecondViewBar.TabManager.Items)
              yield return qtabItem;
          }
          if (this.IsThirdViewBarVisible)
          {
            foreach (QTabItem qtabItem in this.ThirdViewBar.TabManager.Items)
              yield return qtabItem;
          }
        }
      }

      public QCommandBar CommandBar1st
      {
        get { this.commandBar1st;
        set
        {
          if (this.commandBar1st == value)
            return;
          this.commandBar1st = value;
          if (value == null)
          {
            this.CommandBar1stHandle = IntPtr.Zero;
            this.explorerManager.OnInstanceUnregistered(true);
          }
          else
            this.CommandBar1stHandle = value.Handle;
        }
      }

      public QCommandBar2nd CommandBar2nd
      {
        get { this.commandBar2nd;
        set
        {
          if (this.commandBar2nd == value)
            return;
          this.commandBar2nd = value;
          if (value == null)
          {
            this.CommandBar2ndHandle = IntPtr.Zero;
            this.explorerManager.OnInstanceUnregistered(true);
          }
          else
            this.CommandBar2ndHandle = value.Handle;
        }
      }

      public QCommandBarVertical CommandBarVrt
      {
        get { this.commandBarVrt;
        set
        {
          if (this.commandBarVrt == value)
            return;
          this.commandBarVrt = value;
          if (value == null)
          {
            this.CommandBarVrtHandle = IntPtr.Zero;
            this.explorerManager.OnInstanceUnregistered(true);
          }
          else
            this.CommandBarVrtHandle = value.Handle;
        }
      }

      public IntPtr CommandBar1stHandle { get; set; }

      public IntPtr CommandBar2ndHandle { get; set; }

      public IntPtr CommandBarVrtHandle { get; set; }

      public bool IsCommandBar1stVisible { ExplorerManager.ToolbarManager.IsToolBarVisibleCore((BandObject) this.CommandBar1st);

      public bool IsCommandBar1stAvailable { ExplorerManager.ToolbarManager.IsToolBarAvailableCore((BandObject) this.CommandBar1st);

      public bool IsCommandBar2ndVisible { ExplorerManager.ToolbarManager.IsToolBarVisibleCore((BandObject) this.CommandBar2nd);

      public bool IsCommandBar2ndAvailable { ExplorerManager.ToolbarManager.IsToolBarAvailableCore((BandObject) this.CommandBar2nd);

      public bool IsCommandBarVrtVisible { ExplorerManager.ToolbarManager.IsToolBarVisibleCore((BandObject) this.CommandBarVrt);

      public bool IsCommandBarVrtAvailable { ExplorerManager.ToolbarManager.IsToolBarAvailableCore((BandObject) this.CommandBarVrt);

      public ICommandControlOwner[] CommandControlOwners { new ICommandControlOwner[4]
      {
        (ICommandControlOwner) this.CommandBar1st,
        (ICommandControlOwner) this.CommandBar2nd,
        (ICommandControlOwner) this.CommandBarVrt,
        (ICommandControlOwner) this.ThirdViewBar
      };

      public bool FirstVisibleCommandControlOwner(Func<ICommandControlOwner, bool> func = null)
      {
        foreach (ICommandControlOwner commandControlOwner in this.CommandControlOwners)
        {
          if (commandControlOwner is BandObject bo && ExplorerManager.ToolbarManager.IsToolBarVisibleCore(bo) && (func == null || func(commandControlOwner)))
            return true;
        }
        return false;
      }

      public void AllAvailableCommandControlOwner(Action<ICommandControlOwner> action)
      {
        foreach (ICommandControlOwner commandControlOwner in this.CommandControlOwners)
        {
          if (commandControlOwner is BandObject bo && ExplorerManager.ToolbarManager.IsToolBarAvailableCore(bo))
            action(commandControlOwner);
        }
      }

      public IEnumerable<ICommandControlOwner> AvailableCommandControlOwners
      {
        get
        {
          ICommandControlOwner[] commandControlOwnerArray = this.CommandControlOwners;
          for (int index = 0; index < commandControlOwnerArray.Length; ++index)
          {
            ICommandControlOwner commandControlOwner = commandControlOwnerArray[index];
            if (commandControlOwner is BandObject bo && ExplorerManager.ToolbarManager.IsToolBarVisibleCore(bo))
              yield return commandControlOwner;
          }
          commandControlOwnerArray = (ICommandControlOwner[]) null;
        }
      }

      public QManagementBar ManagementBar
      {
        get { this.managementBar;
        set
        {
          if (this.managementBar == value)
            return;
          this.managementBar = value;
          if (value != null)
            return;
          this.explorerManager.OnInstanceUnregistered(false);
        }
      }

      public QSecondViewBar SecondViewBar
      {
        get { this.secondViewBar;
        set
        {
          if (this.secondViewBar == value)
            return;
          this.secondViewBar = value;
          if (value == null)
          {
            this.SecondViewBarHandle = IntPtr.Zero;
            this.explorerManager.OnInstanceUnregistered(false);
          }
          else
            this.SecondViewBarHandle = value.Handle;
        }
      }

      public IntPtr SecondViewBarHandle { get; set; }

      public bool IsSecondViewBarVisible
      {
        get
        {
          if (!this.IsSecondViewBarAvailable)
            return false;
          return PInvoke.IsWindowVisible(this.SecondViewBarHandle) || this.explorerManager.IsInTray();
        }
      }

      public bool IsSecondViewBarAvailable { ExplorerManager.ToolbarManager.IsToolBarAvailableCore((BandObject) this.SecondViewBar);

      public bool IsSecondViewBarTabAvailable { this.IsSecondViewBarVisible && this.SecondViewBar.IsActiveTabAvailable;

      public bool SecondViewHasFocus { this.SecondViewBar != null && this.SecondViewBar.IsHandleCreated && this.SecondViewBar.Visible && this.SecondViewBar.HasFocus;

      public void ShowSecondView(bool fShow)
      {
        if (!(fShow ^ this.IsSecondViewBarVisible))
          return;
        if (fShow)
        {
          bool bottomTabBarVisible = this.IsBottomTabBarVisible;
          Config.Set(Scts.ExtraViewVisibleMode2nd, bottomTabBarVisible ? 2 : 1);
          this.Show(Toolbar.SecondViewBar, true);
          if (!bottomTabBarVisible || !this.IsSecondViewBarAvailable)
            return;
          this.Show(Toolbar.TabBar, true);
        }
        else
        {
          if (Config.Get(Scts.ExtraViewVisibleMode2nd) == 2)
            this.Show(Toolbar.BottomTabBar, true);
          else
            this.Show(Toolbar.SecondViewBar, false);
          Config.Set(Scts.ExtraViewVisibleMode2nd, 0);
        }
      }

      public bool IsPriorityExtraViewBarVisible { !Config.Bool(Scts.ExtraViewPriority) ? this.IsSecondViewBarVisible : this.IsThirdViewBarVisible;

      public bool IsNonPriorityExtraViewBarVisible { !Config.Bool(Scts.ExtraViewPriority) ? this.IsThirdViewBarVisible : this.IsSecondViewBarVisible;

      public QSecondViewBar PriorityExtraViewBar { !Config.Bool(Scts.ExtraViewPriority) ? this.SecondViewBar : (QSecondViewBar) this.ThirdViewBar;

      public QSecondViewBar NonPriorityExtraViewBar { !Config.Bool(Scts.ExtraViewPriority) ? (QSecondViewBar) this.ThirdViewBar : this.SecondViewBar;

      public QThirdViewBar ThirdViewBar
      {
        get { this.thirdViewBar;
        set
        {
          if (this.thirdViewBar == value)
            return;
          this.thirdViewBar = value;
          if (value == null)
          {
            this.ThirdViewBarHandle = IntPtr.Zero;
            this.explorerManager.OnInstanceUnregistered(false);
          }
          else
            this.ThirdViewBarHandle = value.Handle;
        }
      }

      public IntPtr ThirdViewBarHandle { get; set; }

      public bool IsThirdViewBarVisible
      {
        get
        {
          if (!this.IsThirdViewBarAvailable)
            return false;
          return PInvoke.IsWindowVisible(this.ThirdViewBarHandle) || this.explorerManager.IsInTray();
        }
      }

      public bool IsThirdViewBarAvailable { ExplorerManager.ToolbarManager.IsToolBarAvailableCore((BandObject) this.ThirdViewBar);

      public bool IsThirdViewBarTabAvailable { this.IsThirdViewBarVisible && this.ThirdViewBar.IsActiveTabAvailable;

      public bool ThirdViewHasFocus { this.ThirdViewBar != null && this.ThirdViewBar.IsHandleCreated && this.ThirdViewBar.Visible && this.ThirdViewBar.HasFocus;

      public void ShowThirdView(bool fShow)
      {
        if (!(fShow ^ this.IsThirdViewBarVisible))
          return;
        if (fShow)
        {
          bool commandBarVrtVisible = this.IsCommandBarVrtVisible;
          Config.Set(Scts.ExtraViewVisibleMode3rd, commandBarVrtVisible ? 2 : 1);
          this.Show(Toolbar.ThirdViewBar, true);
          if (!commandBarVrtVisible || !this.IsThirdViewBarAvailable)
            return;
          this.ThirdViewBar.ResetSize(Config.EnsureRange(this.ThirdViewBar.RestoreWidth, 1024, 372));
        }
        else
        {
          this.save3rdViewBarWidth();
          if (Config.Get(Scts.ExtraViewVisibleMode3rd) == 2)
          {
            this.fNowHiding3rdViewBar = true;
            this.Show(Toolbar.CommandBarVertical, true);
          }
          else
            this.Show(Toolbar.ThirdViewBar, false);
          Config.Set(Scts.ExtraViewVisibleMode3rd, 0);
        }
      }

      public bool IsVerticalCommandBarEnabledInThirdView { this.IsThirdViewBarVisible && Config.Get(Scts.ExtraViewVisibleMode3rd) == 2;

      private void save3rdViewBarWidth()
      {
        if (!this.IsThirdViewBarVisible)
          return;
        this.ThirdViewBar.RestoreWidth = this.ThirdViewBar.Width;
      }

      public bool IsExtraViewAvailable(TargetView targetView)
      {
        if (targetView == TargetView.Second)
          return this.IsSecondViewBarAvailable;
        return targetView == TargetView.Third && this.IsThirdViewBarAvailable;
      }

      public bool IsExtraViewVisible(TargetView targetView)
      {
        if (targetView == TargetView.Second)
          return this.IsSecondViewBarVisible;
        return targetView == TargetView.Third && this.IsThirdViewBarVisible;
      }

      public QSecondViewBar GetExtraViewBar(TargetView targetView)
      {
        if (targetView == TargetView.Second)
          return this.SecondViewBar;
        return targetView == TargetView.Third ? (QSecondViewBar) this.ThirdViewBar : (QSecondViewBar) null;
      }

      public TargetView GetTheOtherView(TargetView viewFrom)
      {
        if (viewFrom != TargetView.Default)
          return TargetView.Default;
        int num = Config.Bool(Scts.ExtraViewPriority) ? 1 : 0;
        TargetView targetView1 = num != 0 ? TargetView.Third : TargetView.Second;
        TargetView targetView2 = num != 0 ? TargetView.Second : TargetView.Third;
        bool flag1 = num != 0 ? this.IsThirdViewBarVisible : this.IsSecondViewBarVisible;
        bool flag2 = num != 0 ? this.IsSecondViewBarVisible : this.IsThirdViewBarVisible;
        return flag1 || !flag2 ? targetView1 : targetView2;
      }

      public TargetView GetPriorityView() { !Config.Bool(Scts.ExtraViewPriority) ? TargetView.Second : TargetView.Third;

      public bool TryGetAvailableExtraViewBar(TargetView view, out QSecondViewBar bar)
      {
        bar = (QSecondViewBar) null;
        if (view != TargetView.Second)
        {
          if (view != TargetView.Third)
            throw new ArgumentException();
          if (!this.IsThirdViewBarAvailable)
            return false;
          bar = (QSecondViewBar) this.ThirdViewBar;
          return true;
        }
        if (!this.IsSecondViewBarAvailable)
          return false;
        bar = this.SecondViewBar;
        return true;
      }

      public void AllTabAvailableExtraViewBar(Action<QSecondViewBar> action)
      {
        if (this.IsSecondViewBarTabAvailable)
          action(this.SecondViewBar);
        if (!this.IsThirdViewBarTabAvailable)
          return;
        action((QSecondViewBar) this.ThirdViewBar);
      }

      public void AllExtraCustomViews(Action<ExtraView> action) { this.AllTabAvailableExtraViewBar((Action<QSecondViewBar>) (bar { bar.TabManager.Items.OfType<QTabItemWithView>().ForEach<QTabItemWithView>((Action<QTabItemWithView>) (tab { action(tab.View)))));

      public void ShowExtraView(TargetView view, bool fShow)
      {
        if (view == TargetView.Second)
        {
          this.ShowSecondView(fShow);
        }
        else
        {
          if (view != TargetView.Third)
            return;
          this.ShowThirdView(fShow);
        }
      }

      public IEnumerable<CustomViewBase> Views
      {
        get
        {
          yield return (CustomViewBase) this.explorerManager.View;
          if (this.IsSecondViewBarTabAvailable)
            yield return (CustomViewBase) this.SecondViewBar.ActiveTab.View;
          if (this.IsThirdViewBarTabAvailable)
            yield return (CustomViewBase) this.ThirdViewBar.ActiveTab.View;
        }
      }

      public IEnumerable<CustomViewBase> ExtraViews
      {
        get
        {
          if (this.IsSecondViewBarTabAvailable)
            yield return (CustomViewBase) this.SecondViewBar.ActiveTab.View;
          if (this.IsThirdViewBarTabAvailable)
            yield return (CustomViewBase) this.ThirdViewBar.ActiveTab.View;
        }
      }
    }
    }
}

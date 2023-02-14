using QTTabBarLib.Interop;

namespace QTTabBarLib
{
  internal static class RegisteredMessage
  {
    private const string STR_SHELLFILEOPENED = "ShellFileOpened";
    private const string STR_EM_GETMANAGER = "EM_GETMANAGER_QTTABBAR";
    private const string STR_EM_SETMODAL = "EM_SETMODAL_QTTABBAR";
    private const string STR_QTDT_DESKTOPTHREAD = "QTDT_DESKTOPTHREAD";
    private const string STR_QTDT_SHOWMENU = "QTDT_SHOWMENU";
    private const string STR_QTDT_SHOWDESKTOPCONTENTS = "QTDT_SHOWDESKTOPCONTENTS";
    private const string STR_TMN_NOTIFY = "TMN_NOTIFY_QTTABBAR";
    private const string STR_SHELLEXT_MENUHANDLER = "SHELLEXT_MENUHANDLEER";
    private const string STR_UNSUBCLASS = "QTTabBar_UNSUBCLASS";
    private const string STR_AREYOUEXTRAVIEW = "QTTabBar_DETECTEXTRAVIEW";
    private const string STR_VIEW_SELECTPIDL = "QTTabBar_SelectItem";
    private static volatile int msgShellFileOpened;
    private static volatile int msgGetManager;
    private static volatile int msgSetModal;
    private static volatile int msgDesktopThread;
    private static volatile int msgShowMenu;
    private static volatile int msgShowDesktopContents;
    private static volatile int msgThumbnailNotify;
    private static volatile int msgCopyHandler;
    private static volatile int msgUnsubclass;
    private static volatile int msgDetectExtraView;
    private static volatile int msgViewSelectItem;
    private static volatile int debugButtonMsg;

    public static int ShellFileOpened
    {
      get
      {
        if (RegisteredMessage.msgShellFileOpened == 0)
            RegisteredMessage.msgShellFileOpened = PInvoke.RegisterWindowMessage("ShellFileOpened");
        return RegisteredMessage.msgShellFileOpened;
      }
    }

    public static int GetExplorerManagerWindow
    {
      get
      {
        if (RegisteredMessage.msgGetManager == 0)
          RegisteredMessage.msgGetManager = PInvoke.RegisterWindowMessage("EM_GETMANAGER_QTTABBAR");
        return RegisteredMessage.msgGetManager;
      }
    }

    public static int SetModal
    {
      get
      {
        if (RegisteredMessage.msgSetModal == 0)
          RegisteredMessage.msgSetModal = PInvoke.RegisterWindowMessage("EM_SETMODAL_QTTABBAR");
        return RegisteredMessage.msgSetModal;
      }
    }

    public static int ShowDesktopToolMenu
    {
      get
      {
        if (RegisteredMessage.msgShowMenu == 0)
          RegisteredMessage.msgShowMenu = PInvoke.RegisterWindowMessage("QTDT_SHOWMENU");
        return RegisteredMessage.msgShowMenu;
      }
    }

    public static int ShowDesktopContentsMenu
    {
      get
      {
        if (RegisteredMessage.msgShowDesktopContents == 0)
          RegisteredMessage.msgShowDesktopContents = PInvoke.RegisterWindowMessage("QTDT_SHOWDESKTOPCONTENTS");
        return RegisteredMessage.msgShowDesktopContents;
      }
    }

    public static int InvokeInDesktopThread
    {
      get
      {
        if (RegisteredMessage.msgDesktopThread == 0)
          RegisteredMessage.msgDesktopThread = PInvoke.RegisterWindowMessage("QTDT_DESKTOPTHREAD");
        return RegisteredMessage.msgDesktopThread;
      }
    }

    public static int NotifyPreviewTipEvent
    {
      get
      {
        if (RegisteredMessage.msgThumbnailNotify == 0)
          RegisteredMessage.msgThumbnailNotify = PInvoke.RegisterWindowMessage("TMN_NOTIFY_QTTABBAR");
        return RegisteredMessage.msgThumbnailNotify;
      }
    }

    public static int ShellContextmenuHandler
    {
      get
      {
        if (RegisteredMessage.msgCopyHandler == 0)
          RegisteredMessage.msgCopyHandler = PInvoke.RegisterWindowMessage("SHELLEXT_MENUHANDLEER");
        return RegisteredMessage.msgCopyHandler;
      }
    }

    public static int Unsubclass
    {
      get
      {
        if (RegisteredMessage.msgUnsubclass == 0)
          RegisteredMessage.msgUnsubclass = PInvoke.RegisterWindowMessage("QTTabBar_UNSUBCLASS");
        return RegisteredMessage.msgUnsubclass;
      }
    }

    public static int AreYouExtraView
    {
      get
      {
        if (RegisteredMessage.msgDetectExtraView == 0)
          RegisteredMessage.msgDetectExtraView = PInvoke.RegisterWindowMessage("QTTabBar_DETECTEXTRAVIEW");
        return RegisteredMessage.msgDetectExtraView;
      }
    }

    public static int SelectItem
    {
      get
      {
        if (RegisteredMessage.msgViewSelectItem == 0)
          RegisteredMessage.msgViewSelectItem = PInvoke.RegisterWindowMessage("QTTabBar_SelectItem");
        return RegisteredMessage.msgViewSelectItem;
      }
    }

    public static int DebugButtonMessage
    {
      get
      {
        if (RegisteredMessage.debugButtonMsg == 0)
          RegisteredMessage.debugButtonMsg = PInvoke.RegisterWindowMessage("QuizoDebugButtonPlugin");
        return RegisteredMessage.debugButtonMsg;
      }
    }
  }
}

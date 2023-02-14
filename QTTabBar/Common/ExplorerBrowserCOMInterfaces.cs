//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace QTTabBarLib.Common
{
    internal enum CommDlgBrowser2ViewFlags
    {
        ShowAllFiles = 0x00000001,
        IsFileSave = 0x00000002,
        AllowPreviewPane = 0x00000004,
        NoSelectVerb = 0x00000008,
        NoIncludeItem = 0x00000010,
        IsFolderPicker = 0x00000020
    }

    internal enum CommDlgBrowserNotifyType
    {
        Done = 1,
        Start = 2
    }

    internal enum CommDlgBrowserStateChange
    {
        SetFocus = 0,
        KillFocus = 1,
        SelectionChange = 2,
        Rename = 3,
        StateChange = 4
    }

    [Flags]
    internal enum ExplorerBrowserOptions
    {
        NavigateOnce = 0x00000001,
        ShowFrames = 0x00000002,
        AlwaysNavigate = 0x00000004,
        NoTravelLog = 0x00000008,
        NoWrapperWindow = 0x00000010,
        HtmlSharepointView = 0x00000020,
        NoBorder = 0x00000040,
        NoPersistViewState = 0x00000080,
    }

    internal enum ExplorerPaneState
    {
        DoNotCare = 0x00000000,
        DefaultOn = 0x00000001,
        DefaultOff = 0x00000002,
        StateMask = 0x0000ffff,
        InitialState = 0x00010000,
        Force = 0x00020000
    }

    [Flags]
    internal enum FolderOptions
    {
        AutoArrange = 0x00000001,
        AbbreviatedNames = 0x00000002,
        SnapToGrid = 0x00000004,
        OwnerData = 0x00000008,
        BestFitWindow = 0x00000010,
        Desktop = 0x00000020,
        SingleSelection = 0x00000040,
        NoSubfolders = 0x00000080,
        Transparent = 0x00000100,
        NoClientEdge = 0x00000200,
        NoScroll = 0x00000400,
        AlignLeft = 0x00000800,
        NoIcons = 0x00001000,
        ShowSelectionAlways = 0x00002000,
        NoVisible = 0x00004000,
        SingleClickActivate = 0x00008000,
        NoWebView = 0x00010000,
        HideFilenames = 0x00020000,
        CheckSelect = 0x00040000,
        NoEnumRefresh = 0x00080000,
        NoGrouping = 0x00100000,
        FullRowSelect = 0x00200000,
        NoFilters = 0x00400000,
        NoColumnHeaders = 0x00800000,
        NoHeaderInAllViews = 0x01000000,
        ExtendedTiles = 0x02000000,
        TriCheckSelect = 0x04000000,
        AutoCheckSelect = 0x08000000,
        NoBrowserViewState = 0x10000000,
        SubsetGroups = 0x20000000,
        UseSearchFolders = 0x40000000,
        AllowRightToLeftReading = unchecked((int)0x80000000)
    }

    internal enum FolderViewMode
    {
        Auto = -1,
        First = 1,
        Icon = 1,
        SmallIcon = 2,
        List = 3,
        Details = 4,
        Thumbnail = 5,
        Tile = 6,
        Thumbstrip = 7,
        Content = 8,
        Last = 8
    }

    internal enum ShellViewGetItemObject
    {
        Background = 0x00000000,
        Selection = 0x00000001,
        AllView = 0x00000002,
        Checked = 0x00000003,
        TypeMask = 0x0000000F,
        ViewOrderFlag = unchecked((int)0x80000000)
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.ICommDlgBrowser3),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICommDlgBrowser3
    {
        // dlg1
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnDefaultCommand(IntPtr ppshv);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnStateChange(
            IntPtr ppshv,
            CommDlgBrowserStateChange uChange);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult IncludeObject(
            IntPtr ppshv,
            IntPtr pidl);

        // dlg2
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetDefaultMenuText(
            IShellView shellView,
            IntPtr buffer, //A pointer to a buffer that is used by the Shell browser to return the default shortcut menu text.
            int bufferMaxLength); //should be max size = 260?

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetViewFlags(
            [Out] out uint pdwFlags); // CommDlgBrowser2ViewFlags

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Notify(
            IntPtr pshv, CommDlgBrowserNotifyType notifyType);

        // dlg3
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentFilter(
            StringBuilder pszFileSpec,
            int cchFileSpec);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnColumnClicked(
            IShellView ppshv,
            int iColumn);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnPreViewCreated(IShellView ppshv);
    }

    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid(ExplorerBrowserIIDGuid.IExplorerBrowser)]
    internal interface IExplorerBrowser
    {
        /// <summary>Prepares the browser to be navigated.</summary>
        /// <param name="hwndParent">A handle to the owner window or control.</param>
        /// <param name="prc">
        /// A pointer to a RECT containing the coordinates of the bounding rectangle the browser will occupy. The coordinates are relative to
        /// hwndParent. If this parameter is NULL, then method IExplorerBrowser::SetRect should subsequently be called.
        /// </param>
        /// <param name="pfs">
        /// A pointer to a FOLDERSETTINGS structure that determines how the folder will be displayed in the view. If this parameter is NULL,
        /// then method IExplorerBrowser::SetFolderSettings should be called, otherwise, the default view settings for the folder are used.
        /// </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Initialize(IntPtr hwndParent, [In] ref NativeRect prc, [In] FolderSettings pfs);

        /// <summary>Destroys the browser.</summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Destroy();

        /// <summary>Sets the size and position of the view windows created by the browser.</summary>
        /// <param name="phdwp">A pointer to a DeferWindowPos handle. This paramater can be NULL.</param>
        /// <param name="rcBrowser">The coordinates that the browser will occupy.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetRect([In, Out] ref IntPtr phdwp, NativeRect rcBrowser);

        /// <summary>Sets the name of the property bag.</summary>
        /// <param name="pszPropertyBag">
        /// A pointer to a constant, null-terminated, Unicode string that contains the name of the property bag. View state information that
        /// is specific to the application of the client is stored (persisted) using this name.
        /// </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetPropertyBag([MarshalAs(UnmanagedType.LPWStr)] string pszPropertyBag);

        /// <summary>Sets the default empty text.</summary>
        /// <param name="pszEmptyText">A pointer to a constant, null-terminated, Unicode string that contains the empty text.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetEmptyText([MarshalAs(UnmanagedType.LPWStr)] string pszEmptyText);

        /// <summary>Sets the folder settings for the current view.</summary>
        /// <param name="pfs">A pointer to a FOLDERSETTINGS structure that contains the folder settings to be applied.</param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SetFolderSettings(FolderSettings pfs);

        /// <summary>Initiates a connection with IExplorerBrowser for event callbacks.</summary>
        /// <param name="psbe">A pointer to the IExplorerBrowserEvents interface of the object to be advised of IExplorerBrowser events</param>
        /// <param name="pdwCookie">
        /// When this method returns, contains a token that uniquely identifies the event listener. This allows several event listeners to be
        /// subscribed at a time.
        /// </param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Advise(IntPtr psbe, out uint pdwCookie);

        /// <summary>Terminates an advisory connection.</summary>
        /// <param name="dwCookie">
        /// A connection token previously returned from IExplorerBrowser::Advise. Identifies the connection to be terminated.
        /// </param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Unadvise([In] uint dwCookie);

        /// <summary>Sets the current browser options.</summary>
        /// <param name="dwFlag">One or more EXPLORER_BROWSER_OPTIONS flags to be set.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetOptions([In]ExplorerBrowserOptions dwFlag);

        /// <summary>Gets the current browser options.</summary>
        /// <param name="pdwFlag">When this method returns, contains the current EXPLORER_BROWSER_OPTIONS for the browser.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetOptions(out ExplorerBrowserOptions pdwFlag);

        /// <summary>Browses to a pointer to an item identifier list (PIDL)</summary>
        /// <param name="pidl">
        /// A pointer to a const ITEMIDLIST (item identifier list) that specifies an object's location as the destination to navigate to.
        /// This parameter can be NULL.
        /// </param>
        /// <param name="uFlags">A flag that specifies the category of the pidl. This affects how navigation is accomplished</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void BrowseToIDList(IntPtr pidl, uint uFlags);

        /// <summary>Browse to an object</summary>
        /// <param name="punk">A pointer to an object to browse to. If the object cannot be browsed, an error value is returned.</param>
        /// <param name="uFlags">A flag that specifies the category of the pidl. This affects how navigation is accomplished.</param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult BrowseToObject([MarshalAs(UnmanagedType.IUnknown)] object punk, uint uFlags);

        /// <summary>Creates a results folder and fills it with items.</summary>
        /// <param name="punk">An interface pointer on the source object that will fill the IResultsFolder</param>
        /// <param name="dwFlags">One of the EXPLORER_BROWSER_FILL_FLAGS</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FillFromObject([MarshalAs(UnmanagedType.IUnknown)] object punk, int dwFlags);

        /// <summary>Removes all items from the results folder.</summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RemoveAll();

        /// <summary>Gets an interface for the current view of the browser.</summary>
        /// <param name="riid">A reference to the desired interface ID.</param>
        /// <param name="ppv">
        /// When this method returns, contains the interface pointer requested in riid. This will typically be IShellView or IShellView2.
        /// </param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentView(ref Guid riid, out IntPtr ppv);
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IExplorerBrowserEvents),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IExplorerBrowserEvents
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnNavigationPending(IntPtr pidlFolder);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnViewCreated([MarshalAs(UnmanagedType.IUnknown)]  object psv);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnNavigationComplete(IntPtr pidlFolder);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnNavigationFailed(IntPtr pidlFolder);
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IExplorerPaneVisibility),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IExplorerPaneVisibility
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetPaneState(ref Guid explorerPane, out ExplorerPaneState peps);
    };

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IFolderView),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFolderView
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCurrentViewMode([Out] out uint pViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCurrentViewMode(uint ViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFolder(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Item(int iItemIndex, out IntPtr ppidl);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ItemCount(uint uFlags, out int pcItems);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Items(uint uFlags, ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectionMarkedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFocusedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetItemPosition(IntPtr pidl, out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSpacing([Out] out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDefaultSpacing(out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetAutoArrange();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectItem(int iItem, uint dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectAndPositionItems(uint cidl, IntPtr apidl, ref NativePoint apt, uint dwFlags);
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IFolderView2),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFolderView2 : IFolderView
    {
        // IFolderView
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentViewMode(out uint pViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCurrentViewMode(uint ViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFolder(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Item(int iItemIndex, out IntPtr ppidl);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult ItemCount(uint uFlags, out int pcItems);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Items(uint uFlags, ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectionMarkedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFocusedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetItemPosition(IntPtr pidl, out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSpacing([Out] out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDefaultSpacing(out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetAutoArrange();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectItem(int iItem, uint dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectAndPositionItems(uint cidl, IntPtr apidl, ref NativePoint apt, uint dwFlags);

        // IFolderView2
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetGroupBy(IntPtr key, bool fAscending);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetGroupBy(ref IntPtr pkey, ref bool pfAscending);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetViewProperty(IntPtr pidl, IntPtr propkey, object propvar);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetViewProperty(IntPtr pidl, IntPtr propkey, out object ppropvar);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetExtendedTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetText(int iType, [MarshalAs(UnmanagedType.LPWStr)] string pwszText);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCurrentFolderFlags(uint dwMask, uint dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCurrentFolderFlags(out uint pdwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSortColumnCount(out int pcColumns);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetSortColumns(IntPtr rgSortColumns, int cColumns);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSortColumns(out IntPtr rgSortColumns, int cColumns);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetItem(int iItem, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetVisibleItem(int iStart, bool fPrevious, out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectedItem(int iStart, out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelection(bool fNoneImpliesFolder, out IShellItemArray ppsia);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectionState(IntPtr pidl, out uint pdwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void InvokeVerbOnSelection([In, MarshalAs(UnmanagedType.LPWStr)] string pszVerb);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SetViewModeAndIconSize(int uViewMode, int iImageSize);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetViewModeAndIconSize(out int puViewMode, out int piImageSize);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetGroupSubsetCount(uint cVisibleRows);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetGroupSubsetCount(out uint pcVisibleRows);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetRedraw(bool fRedrawOn);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsMoveInSameFolder();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void DoRename();
    }

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult Notify(
    //        IntPtr pshv,
    //        CommDlgBrowserNotifyType notifyType);
    //}
    [ComImport,
   Guid(ExplorerBrowserIIDGuid.IInputObject),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInputObject
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult UIActivateIO(bool fActivate, ref System.Windows.Forms.Message pMsg);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult HasFocusIO();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult TranslateAcceleratorIO(ref System.Windows.Forms.Message pMsg);
    };

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IServiceProvider),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IServiceProvider
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall)]
        HResult QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    };

    // [PreserveSig] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] HResult
    // GetViewFlags(CommDlgBrowser2ViewFlags pdwFlags);
    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IShellView),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellView
    {
        // IOleWindow
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetWindow(
            out IntPtr phwnd);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult ContextSensitiveHelp(
            bool fEnterMode);

        // IShellView
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult TranslateAccelerator(
            IntPtr pmsg);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult EnableModeless(
            bool fEnable);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult UIActivate(
            uint uState);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Refresh();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult CreateViewWindow(
            [MarshalAs(UnmanagedType.IUnknown)] object psvPrevious,
            IntPtr pfs,
            [MarshalAs(UnmanagedType.IUnknown)] object psb,
            IntPtr prcView,
            out IntPtr phWnd);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult DestroyViewWindow();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentInfo(
            out IntPtr pfs);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult AddPropertySheetPages(
            uint dwReserved,
            IntPtr pfn,
            uint lparam);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SaveViewState();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SelectItem(
            IntPtr pidlItem,
            uint uFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetItemObject(
            ShellViewGetItemObject uItem,
            ref Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }

    [ComImport,
     TypeLibType(TypeLibTypeFlags.FCanCreate),
     ClassInterface(ClassInterfaceType.None),
     Guid(ExplorerBrowserCLSIDGuid.ExplorerBrowser)]
    internal class ExplorerBrowserClass : IExplorerBrowser
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void Initialize(IntPtr hwndParent, [In]ref NativeRect prc, [In] FolderSettings pfs);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void Destroy();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void SetRect([In, Out] ref IntPtr phdwp, NativeRect rcBrowser);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void SetPropertyBag([MarshalAs(UnmanagedType.LPWStr)] string pszPropertyBag);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void SetEmptyText([MarshalAs(UnmanagedType.LPWStr)] string pszEmptyText);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual HResult SetFolderSettings(FolderSettings pfs);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual HResult Advise(IntPtr psbe, out uint pdwCookie);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual HResult Unadvise(uint dwCookie);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void SetOptions([In]ExplorerBrowserOptions dwFlag);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void GetOptions(out ExplorerBrowserOptions pdwFlag);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void BrowseToIDList(IntPtr pidl, uint uFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual HResult BrowseToObject([MarshalAs(UnmanagedType.IUnknown)] object punk, uint uFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void FillFromObject([MarshalAs(UnmanagedType.IUnknown)] object punk, int dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual void RemoveAll();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern virtual HResult GetCurrentView(ref Guid riid, out IntPtr ppv);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal class FolderSettings
    {
        public FolderViewMode ViewMode;
        public FolderOptions Options;
    }

    // Disable warning if a method declaration hides another inherited from a parent COM interface To successfully import a COM interface,
    // all inherited methods need to be declared again with the exception of those already declared in "IUnknown"
#pragma warning disable 108
    //[ComImport,
    // Guid(ExplorerBrowserIIDGuid.ICommDlgBrowser),
    // InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //internal interface ICommDlgBrowser
    //{
    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult OnDefaultCommand(IntPtr ppshv);

    // [PreserveSig] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] HResult OnStateChange( IntPtr
    // ppshv, CommDlgBrowserStateChange uChange);

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult IncludeObject(
    //        IntPtr ppshv,
    //        IntPtr pidl);
    //}

    //[ComImport,
    // Guid(ExplorerBrowserIIDGuid.ICommDlgBrowser2),
    // InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //internal interface ICommDlgBrowser2
    //{
    //    // dlg

    // [PreserveSig] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] HResult OnDefaultCommand(IntPtr ppshv);

    // [PreserveSig] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] HResult OnStateChange( IntPtr
    // ppshv, CommDlgBrowserStateChange uChange);

    // [PreserveSig] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] HResult IncludeObject( IntPtr
    // ppshv, IntPtr pidl);

    // // dlg2

    // [PreserveSig] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)] HResult GetDefaultMenuText( [In]
    // IShellView shellView, StringBuilder buffer, //A pointer to a buffer that is used by the Shell browser to return the default shortcut
    // menu text. [In] int bufferMaxLength); //should be max size = 260?
#pragma warning restore 108
}
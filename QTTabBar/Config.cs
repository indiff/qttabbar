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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using Padding = System.Windows.Forms.Padding;
using Key = System.Windows.Forms.Keys;

namespace QTTabBarLib {

     // Wrapper class to get around  Font serialization stupidity
    [Serializable]
    public class XmlSerializableFont {
        public string FontName { get; set; }
        public float FontSize { get; set; }
        public FontStyle FontStyle { get; set; }

        public static XmlSerializableFont FromFont(Font font) {
            return font == null ? null : new XmlSerializableFont
                    {FontName = font.Name, FontSize = font.Size, FontStyle = font.Style};
        }

        public Font ToFont() {
            return ToFont(this);
        }

        public static Font ToFont(XmlSerializableFont xmlSerializableFont) {
            return new Font(
                    xmlSerializableFont.FontName,
                    xmlSerializableFont.FontSize,
                    xmlSerializableFont.FontStyle);
        }
    }

    public enum TabPos {
        Rightmost,
        Right,
        Left,
        Leftmost,
        LastActive,
    }

    public enum StretchMode {
        Full,
        Real,
        Tile,
    }

    public enum MouseTarget {
        Anywhere,
        Tab,
        TabBarBackground,
        FolderLink,
        ExplorerItem,
        ExplorerBackground
    }

    [Flags]
    public enum MouseChord {
        None    =   0,
        Shift   =   1,
        Ctrl    =   2,
        Alt     =   4,
        Left    =   8,
        Right   =  16,
        Middle  =  32,
        Double  =  64,
        X1      = 128,
        X2      = 256,
    }

    // WARNING
    // reordering these will break existing settings.
    public enum BindAction
    {
        GoBack = 0,
        GoForward,
        GoFirst,
        GoLast,
        NextTab,
        PreviousTab,
        FirstTab,
        LastTab,
        SwitchToLastActivated,
        NewTab,
        NewWindow,
        MergeWindows,
        CloseCurrent,
        CloseAllButCurrent,
        CloseLeft,
        CloseRight,
        CloseWindow,
        RestoreLastClosed,
        CloneCurrent,
        TearOffCurrent,
        LockCurrent,
        LockAll,
        BrowseFolder,
        CreateNewGroup,
        ShowOptions,
        ShowToolbarMenu,
        ShowTabMenuCurrent,
        ShowGroupMenu,
        ShowUserAppsMenu,
        ShowRecentTabsMenu,
        ShowRecentFilesMenu,
        NewFile,
        NewFolder,
        CopySelectedPaths,
        CopySelectedNames,
        CopyCurrentFolderPath,
        CopyCurrentFolderName,
        ChecksumSelected,
        ToggleTopMost,
        TransparencyPlus,
        TransparencyMinus,
        FocusFileList,
        FocusSearchBarReal,
        FocusSearchBarBBar,
        ShowSDTSelected,
        SendToTray,
        FocusTabBar,
        SortTabsByName,
        SortTabsByPath,
        SortTabsByActive,

        KEYBOARD_ACTION_COUNT,
        // Mouse-only from here on down

        Nothing = QTUtility.FIRST_MOUSE_ONLY_ACTION,
        UpOneLevel,
        Refresh,
        Paste,
        Maximize,
        Minimize,

        // Item Actions
        ItemOpenInNewTab,
        ItemOpenInNewTabNoSel,
        ItemOpenInNewWindow,
        ItemCut,
        ItemCopy,
        ItemDelete,
        ItemProperties,
        CopyItemPath,
        CopyItemName,
        ChecksumItem,

        // Tab Actions
        CloseTab,
        CloseLeftTab,
        CloseRightTab,
        UpOneLevelTab, //hmm
        LockTab,
        ShowTabMenu,
        TearOffTab,
        CloneTab,
        CopyTabPath,
        TabProperties,
        ShowTabSubfolderMenu,
        CloseAllButThis,
        
        /******* add by qwop start *****/
        // add by qwop 2012 08 10
        OpenCmd
        ,ItemsOpenInNewTabNoSel //add bool qwop 2012 08 12
        /***** add by qwop end *****/

          /******* add by qwop start *****/
        // add by qwop 2019 12 16 19:27
        , SortTab 
        , TurnOffRepeat
        //add bool qwop 2019 12 16 19:27
        /***** add by qwop end *****/
    }

    [Serializable]
    public class Config {
/*插件管理*/
/*语言配置*/
/*关于信息*/       
		// Shortcuts to the loaded config, for convenience.
        public static _Window Window    { get { return ConfigManager.LoadedConfig.window; } }	/*窗口行为*/
        public static _Tabs Tabs        { get { return ConfigManager.LoadedConfig.tabs; } }		/*标签行为*/
        public static _Tweaks Tweaks    { get { return ConfigManager.LoadedConfig.tweaks; } }	/*调整工具*/
        public static _Tips Tips        { get { return ConfigManager.LoadedConfig.tips; } }		/*预览提示*/
        public static _Misc Misc        { get { return ConfigManager.LoadedConfig.misc; } }		/*常规选项*/
        public static _Skin Skin        { get { return ConfigManager.LoadedConfig.skin; } }		/*标签外观*/
        public static _BBar BBar        { get { return ConfigManager.LoadedConfig.bbar; } }		/*按钮选项*/
        public static _Mouse Mouse      { get { return ConfigManager.LoadedConfig.mouse; } }	/*鼠标操作*/
        public static _Keys Keys        { get { return ConfigManager.LoadedConfig.keys; } }		/*快捷操作*/
        public static _Plugin Plugin    { get { return ConfigManager.LoadedConfig.plugin; } }	/*插件管理*/
        public static _Lang Lang        { get { return ConfigManager.LoadedConfig.lang; } }		/*语言配置*/
        public static _Desktop Desktop  { get { return ConfigManager.LoadedConfig.desktop; } }

        public _Window window   { get; set; }
        public _Tabs tabs       { get; set; }
        public _Tweaks tweaks   { get; set; }
        public _Tips tips       { get; set; }
        public _Misc misc       { get; set; }
        public _Skin skin       { get; set; }
        public _BBar bbar       { get; set; }
        public _Mouse mouse     { get; set; }
        public _Keys keys       { get; set; }
        public _Plugin plugin   { get; set; }
        public _Lang lang       { get; set; }
        public _Desktop desktop { get; set; }

        public Config() {
            window = new _Window();
            tabs = new _Tabs();
            tweaks = new _Tweaks();
            tips = new _Tips();
            misc = new _Misc();
            skin = new _Skin();
            bbar = new _BBar();
            mouse = new _Mouse();
            keys = new _Keys();
            plugin = new _Plugin();
            lang = new _Lang();
            desktop = new _Desktop();
        }

        [Serializable]
        public class _Window {
            public bool CaptureNewWindows        { get; set; }
            public bool RestoreSession           { get; set; }
            public bool RestoreOnlyLocked        { get; set; }
            public bool CloseBtnClosesUnlocked   { get; set; }
            public bool CloseBtnClosesSingleTab  { get; set; }
            public bool TrayOnClose              { get; set; }
            public bool TrayOnMinimize           { get; set; }
            public byte[] DefaultLocation        { get; set; }

            public _Window() {
              /*  CaptureNewWindows = false;
                RestoreSession = false;
                RestoreOnlyLocked = false;
                CloseBtnClosesSingleTab = true;
                CloseBtnClosesUnlocked = false;
                TrayOnClose = false;
                TrayOnMinimize = false;*/

                /* qwop's default value. */
                CaptureNewWindows = true;
                RestoreSession = true;
                RestoreOnlyLocked = false;
                CloseBtnClosesUnlocked = false;
                CloseBtnClosesSingleTab = true;
                TrayOnClose = false;
                TrayOnMinimize = false;

  //              string idl = Environment.OSVersion.Version >= new Version(6, 1)
 //                       ? "::{031E4825-7B94-4DC3-B131-E946B44C8DD5}"  // Libraries
   //                     : "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // Computer
                string idl = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // 我的电脑， 默认打开
                using(IDLWrapper w = new IDLWrapper(idl)) {
                    DefaultLocation = w.IDL;
                }
            }
        }

        [Serializable]
        public class _Tabs {
            public TabPos NewTabPosition         { get; set; }
            public TabPos NextAfterClosed        { get; set; }
            public bool ActivateNewTab           { get; set; }
            public bool NeverOpenSame            { get; set; }
            public bool RenameAmbTabs            { get; set; }
            public bool DragOverTabOpensSDT      { get; set; }
            public bool ShowFolderIcon           { get; set; }
            public bool ShowSubDirTipOnTab       { get; set; }
            public bool ShowDriveLetters         { get; set; }
            public bool ShowCloseButtons         { get; set; }
            public bool CloseBtnsWithAlt         { get; set; }
            public bool CloseBtnsOnHover         { get; set; }
            public bool ShowNavButtons           { get; set; }
            public bool NavButtonsOnRight        { get; set; }
            public bool MultipleTabRows          { get; set; }
            public bool ActiveTabOnBottomRow     { get; set; }

            public _Tabs() {
               /* NewTabPosition = TabPos.Rightmost;
                NextAfterClosed = TabPos.LastActive;
                ActivateNewTab = true;
                NeverOpenSame = true;
                RenameAmbTabs = false;
                DragOverTabOpensSDT = false;
                ShowFolderIcon = true;
                ShowSubDirTipOnTab = true;
                ShowDriveLetters = false;
                ShowCloseButtons = false;
                CloseBtnsWithAlt = false;
                CloseBtnsOnHover = false;
                ShowNavButtons = false;
                MultipleTabRows = true;
                ActiveTabOnBottomRow = true;*/

                /* qwop's default's value.*/
                NewTabPosition = TabPos.Rightmost;  // 新标签在最右侧
                NextAfterClosed = TabPos.LastActive; // 关闭标签时候切换导
                ActivateNewTab = true;  
                NeverOpenSame = true;
                RenameAmbTabs = true;
                DragOverTabOpensSDT = false;
                ShowFolderIcon = true;  // 显示驱动器图标
                ShowSubDirTipOnTab = false;
                ShowDriveLetters = false; // 显示驱动器字符
                ShowCloseButtons = false;  // 显示关闭按钮
                CloseBtnsWithAlt = false; // 只有alt按住进行关闭
                CloseBtnsOnHover = false;  // 仅鼠标悬浮关闭
                ShowNavButtons = true;  // 在标签显示导航按钮
                NavButtonsOnRight = true; // 在最右边
                MultipleTabRows = true; // 允许多行标签
                ActiveTabOnBottomRow = true; // 始终将活动标签置于底部行
            }
        }

        [Serializable]
        public class _Tweaks {
            public bool AlwaysShowHeaders        { get; set; }
            public bool KillExtWhileRenaming     { get; set; }
            public bool RedirectLibraryFolders   { get; set; }
            public bool F2Selection              { get; set; }
            public bool WrapArrowKeySelection    { get; set; }
            public bool BackspaceUpLevel         { get; set; }
            public bool HorizontalScroll         { get; set; }
            public bool ForceSysListView         { get; set; }
            public bool ToggleFullRowSelect      { get; set; }
            public bool DetailsGridLines         { get; set; }
            public bool AlternateRowColors       { get; set; }
            public Color AltRowBackgroundColor   { get; set; }
            public Color AltRowForegroundColor   { get; set; }

            public _Tweaks() {
               /* AlwaysShowHeaders = !QTUtility.IsXP && !QTUtility.IsWin7;
                KillExtWhileRenaming = true;
                RedirectLibraryFolders = false;
                F2Selection = true;
                WrapArrowKeySelection = false;
                BackspaceUpLevel = QTUtility.IsXP;
                HorizontalScroll = true;
                ForceSysListView = false;
                ToggleFullRowSelect = false;
                DetailsGridLines = false;
                AlternateRowColors = false;
                AltRowForegroundColor = SystemColors.WindowText;
                AltRowBackgroundColor = QTUtility2.MakeColor(0xfaf5f1); */

                /* qwop's default value.*/
                AlwaysShowHeaders = true;
                KillExtWhileRenaming = true;
                RedirectLibraryFolders = true;
                F2Selection = false;
                WrapArrowKeySelection = true;
                BackspaceUpLevel = true;
                HorizontalScroll = true;
                ForceSysListView = true;
                ToggleFullRowSelect = QTUtility.IsXP;
                DetailsGridLines = true;
                AlternateRowColors = true;
                AltRowForegroundColor = SystemColors.WindowText;
                AltRowBackgroundColor = QTUtility2.MakeColor(0xfaf5f1);
            }
        }

        [Serializable]
        public class _Tips {
            public bool ShowSubDirTips           { get; set; }
            public bool SubDirTipsPreview        { get; set; }
            public bool SubDirTipsFiles          { get; set; }
            public bool SubDirTipsWithShift      { get; set; }
            public bool ShowTooltipPreviews      { get; set; }
            public bool ShowPreviewsWithShift    { get; set; }
            public bool ShowPreviewInfo          { get; set; }
            public int PreviewMaxWidth           { get; set; }
            public int PreviewMaxHeight          { get; set; }
            public Font PreviewFont              { get; set; }
            public List<string> TextExt          { get; set; }
            public List<string> ImageExt         { get; set; }
            
            public _Tips() {
                /*ShowSubDirTips = true;
                 SubDirTipsPreview = true;
                 SubDirTipsFiles = true;
                 SubDirTipsWithShift = false;
                 ShowTooltipPreviews = true;
                 ShowPreviewsWithShift = false;
                 ShowPreviewInfo = true;
                 PreviewMaxWidth = 512;
                 PreviewMaxHeight = 256;
                 PreviewFont = Control.DefaultFont;
                 TextExt = new List<string> {".txt", ".ini", ".inf" ,".cs", ".log", ".js", ".vbs"};
                 ImageExt = ThumbnailTooltipForm.MakeDefaultImgExts();*/

                ShowSubDirTips = true;
                SubDirTipsPreview = true;
                SubDirTipsFiles = true;
                SubDirTipsWithShift = true;
                ShowTooltipPreviews = true;
                ShowPreviewsWithShift = true;
                ShowPreviewInfo = true;

                PreviewMaxWidth = 512;
                PreviewMaxHeight = 256;
                PreviewFont = Control.DefaultFont;
                PreviewFont = new Font(new FontFamily("微软雅黑"), 9f);
                TextExt = new List<string> { 
                    ".txt",".rtf"
                    ,".ini", ".inf",".properties", ".ruleset", ".settings"
                    ,".cs", ".log"
                    ,".js", ".vbs", ".bat", ".cmd", ".sh"
                    ,".c",".cpp",".cc",".h", ".rc"
                    ,".xml"
                    ,".htm",".html",".mht",".mhtml", ".shtml", ".hta"
                    ,".HxT",".HxC",".hhc",".hhk", ".hhp"
                    ,".java"
                    ,".sql"
                    ,".csv"
                    ,".md" 
                    
                    ,".py", ".rb"
                    ,".jsp", ".asp", ".php",".aspx"
                    ,".resx",".xaml",  ".config", ".manifest", ".csproj", ".vbproj"
                };
                ImageExt = ThumbnailTooltipForm.MakeDefaultImgExts();
            }
        }

        [Serializable]
        public class _Misc {
            public bool TaskbarThumbnails        { get; set; }
            public bool KeepHistory              { get; set; }
            public int TabHistoryCount           { get; set; }
            public bool KeepRecentFiles          { get; set; }
            public int FileHistoryCount          { get; set; }
            public int NetworkTimeout            { get; set; }
            public bool AutoUpdate               { get; set; }

            public _Misc() {
                TaskbarThumbnails = false;
                KeepHistory = true;
                TabHistoryCount = 15;
                KeepRecentFiles = true;
                FileHistoryCount = 15;
                NetworkTimeout = 0;
                AutoUpdate = true;
            }
        }

        [Serializable]
        public class _Skin {
            public bool UseTabSkin               { get; set; }
            public string TabImageFile           { get; set; }
            public Padding TabSizeMargin         { get; set; }
            public Padding TabContentMargin      { get; set; }
            public int OverlapPixels             { get; set; }
            public bool HitTestTransparent       { get; set; }
            public int TabHeight                 { get; set; }
            public int TabMinWidth               { get; set; }
            public int TabMaxWidth               { get; set; }
            public bool FixedWidthTabs           { get; set; }
            public Font TabTextFont              { get; set; }
            public Color TabTextActiveColor      { get; set; }
            public Color TabTextInactiveColor    { get; set; }
            public Color TabTextHotColor         { get; set; }
            public Color TabShadActiveColor      { get; set; }
            public Color TabShadInactiveColor    { get; set; }
            public Color TabShadHotColor         { get; set; }
            public bool TabTitleShadows          { get; set; }
            public bool TabTextCentered          { get; set; }
            public bool UseRebarBGColor          { get; set; }
            public Color RebarColor              { get; set; }
            public bool UseRebarImage            { get; set; }
            public StretchMode RebarStretchMode  { get; set; }
            public string RebarImageFile         { get; set; }
            public bool RebarImageSeperateBars   { get; set; }
            public Padding RebarSizeMargin       { get; set; }
            public bool ActiveTabInBold          { get; set; }

            public _Skin() {
                /* UseTabSkin = false;
                 TabImageFile = "";
                 TabSizeMargin = Padding.Empty;
                 TabContentMargin = Padding.Empty;
                 OverlapPixels = 0;
                 HitTestTransparent = false;
                 TabHeight = 24;
                 TabMinWidth = 50;
                 TabMaxWidth = 200;
                 FixedWidthTabs = false;
                 TabTextFont = Control.DefaultFont;
                 TabTextActiveColor = Color.Black;
                 TabTextInactiveColor = Color.Black;
                 TabTextHotColor = Color.Black;
                 TabShadActiveColor = Color.Gray;
                 TabShadInactiveColor = Color.White;
                 TabShadHotColor = Color.White;
                 TabTitleShadows = false;
                 TabTextCentered = false;
                 UseRebarBGColor = false;
                 RebarColor = Color.Gray;
                 UseRebarImage = false;
                 RebarStretchMode = StretchMode.Full;
                 RebarImageFile = "";
                 RebarImageSeperateBars = false;
                 RebarSizeMargin = Padding.Empty;
                 ActiveTabInBold = false;*/

                /* qwop's default value. */
                UseTabSkin = false;
                TabImageFile = "";
                TabSizeMargin = Padding.Empty;
                TabContentMargin = Padding.Empty;
                OverlapPixels = 0;
                HitTestTransparent = false;
                TabHeight = 24;
                TabMinWidth = 50;
                TabMaxWidth = 200;
                FixedWidthTabs = false;
                TabTextFont = new Font(new FontFamily("微软雅黑"), 9f);
                TabTextActiveColor = Color.Black;
                TabTextInactiveColor = Color.Black;
                TabTextHotColor = Color.Black;
                TabShadActiveColor = Color.Gray;
                TabShadInactiveColor = Color.White;
                TabShadHotColor = Color.White;
                TabTitleShadows = true;
                TabTextCentered = false;
                UseRebarBGColor = true;
                RebarColor = Color.FromArgb(230,230,230);
                UseRebarImage = false;
                RebarStretchMode = StretchMode.Tile;
                RebarImageFile = "";
                RebarImageSeperateBars = false;
                RebarSizeMargin = Padding.Empty;
                ActiveTabInBold = false;
            }
        }

        [Serializable]
        public class _BBar {
            public int[] ButtonIndexes           { get; set; }
            public string[] ActivePluginIDs      { get; set; }
            public bool LargeButtons             { get; set; }
            public bool LockSearchBarWidth       { get; set; }
            public bool LockDropDownButtons      { get; set; }
            public bool ShowButtonLabels         { get; set; }
            public string ImageStripPath         { get; set; }
            
            public _BBar() {
                /* // the old 
                ButtonIndexes = QTUtility.IsXP 
                        ? new int[] {1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20} 
                        : new int[] {3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 9, 20};
                ActivePluginIDs = new string[0];
                LockDropDownButtons = false;
                LargeButtons = true;
                LockSearchBarWidth = false;
                ShowButtonLabels = false;
                ImageStripPath = ""; */

                /* qwop's default. */
                ButtonIndexes	=	QTUtility.IsXP
                        ? new int[] { 1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 21, 9, 20  }
                        : new System.Int32[] { 3, 4, 5, 0, 6, 7, 0, 17, 11, 12, 14, 15, 13, 0, 21, 9, 19, 10 };
                ActivePluginIDs = new string[0];
                LargeButtons	=	false;
                LockSearchBarWidth	=	true;
                LockDropDownButtons	=	true;
                ShowButtonLabels	=	true;
                ImageStripPath	=	"";
            }
        }

        [Serializable]
        public class _Mouse {
            public bool MouseScrollsHotWnd       { get; set; }
            public Dictionary<MouseChord, BindAction> GlobalMouseActions { get; set; }
            public Dictionary<MouseChord, BindAction> TabActions { get; set; }
            public Dictionary<MouseChord, BindAction> BarActions { get; set; }
            public Dictionary<MouseChord, BindAction> LinkActions { get; set; }
            public Dictionary<MouseChord, BindAction> ItemActions { get; set; }
            public Dictionary<MouseChord, BindAction> MarginActions { get; set; }

            public _Mouse() {
                /*MouseScrollsHotWnd = false;
                GlobalMouseActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.X1, BindAction.GoBack},
                    {MouseChord.X2, BindAction.GoForward},
                    {MouseChord.X1 | MouseChord.Ctrl, BindAction.GoFirst},
                    {MouseChord.X2 | MouseChord.Ctrl, BindAction.GoLast}
                };
                TabActions = new Dictionary<MouseChord, BindAction> { 
                    {MouseChord.Middle, BindAction.CloseTab},
                    {MouseChord.Ctrl | MouseChord.Left, BindAction.LockTab},
                    {MouseChord.Double, BindAction.UpOneLevelTab},
                };
                BarActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Double, BindAction.NewTab},
                    {MouseChord.Middle, BindAction.RestoreLastClosed}
                };
                LinkActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewWindow}
                };
                ItemActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewWindow}                        
                };
                MarginActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Double, BindAction.UpOneLevel}
                };*/

                /* qwop's default value. */
                MouseScrollsHotWnd = false;
                GlobalMouseActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.X1, BindAction.GoBack},
                    {MouseChord.X2, BindAction.GoForward},
                    {MouseChord.X1 | MouseChord.Ctrl, BindAction.GoFirst},
                    {MouseChord.X2 | MouseChord.Ctrl, BindAction.GoLast}
                };
                TabActions = new Dictionary<MouseChord, BindAction> { 
                    {MouseChord.Middle, BindAction.CloseTab},
                    {MouseChord.Ctrl | MouseChord.Left, BindAction.LockTab},
                    {MouseChord.Double, BindAction.UpOneLevelTab},
                };
                BarActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Double, BindAction.NewTab},
                    {MouseChord.Middle, BindAction.RestoreLastClosed},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.TearOffCurrent}
                };
                LinkActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewWindow}
                };
                ItemActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewTabNoSel}                        
                };
                MarginActions = new Dictionary<MouseChord, BindAction> {
                    { MouseChord.Double, BindAction.UpOneLevel}
                    // add by qwop //
                    ,{ MouseChord.Middle, BindAction.BrowseFolder}
                    ,{ ( MouseChord) 66, BindAction.OpenCmd } // ===  {MouseChord.Ctrl | MouseChord.Double, BindAction.OpenCmd}
                    ,{ MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemsOpenInNewTabNoSel}
                    // add by qwop //
                };
            }
        }

        [Serializable]
        public class _Keys {
            public int[] Shortcuts               { get; set; }
            public Dictionary<string, int[]> PluginShortcuts { get; set; } 
            public bool UseTabSwitcher           { get; set; }

            public _Keys() {
                var dict = new Dictionary<BindAction, Keys> {
                    {BindAction.GoBack,             Key.Left  | Key.Alt},
                    {BindAction.GoForward,          Key.Right | Key.Alt},
                    {BindAction.GoFirst,            Key.Left  | Key.Control | Key.Alt},
                    {BindAction.GoLast,             Key.Right | Key.Control | Key.Alt},
                    {BindAction.NextTab,            Key.Tab   | Key.Control},
                    {BindAction.PreviousTab,        Key.Tab   | Key.Control | Key.Shift},
                    {BindAction.NewTab,             Key.T     | Key.Control},
                    {BindAction.NewWindow,          Key.T     | Key.Control | Key.Shift},
                    {BindAction.CloseCurrent,       Key.W     | Key.Control},
                    {BindAction.CloseAllButCurrent, Key.W     | Key.Control | Key.Shift},
                    {BindAction.RestoreLastClosed,  Key.Z     | Key.Control | Key.Shift},
                    {BindAction.LockCurrent,        Key.L     | Key.Control},
                    {BindAction.LockAll,            Key.L     | Key.Control | Key.Shift},
                    {BindAction.BrowseFolder,       Key.O     | Key.Control},
                    {BindAction.ShowOptions,        Key.O     | Key.Alt},
                    {BindAction.ShowToolbarMenu,    Key.Oemcomma  | Key.Alt},
                    {BindAction.ShowTabMenuCurrent, Key.OemPeriod | Key.Alt},
                    {BindAction.ShowGroupMenu,      Key.G     | Key.Alt},
                    {BindAction.ShowUserAppsMenu,   Key.H     | Key.Alt},
                    {BindAction.ShowRecentTabsMenu, Key.U     | Key.Alt},
                    {BindAction.ShowRecentFilesMenu,Key.F     | Key.Alt},

                    // Bug fix 热键冲突， 调整 by qwop 
                    // {BindAction.NewFile,            Key.N     | Key.Control},
                    {BindAction.NewFile,            Key.N     | Key.Control | Key.Alt},
                    // {BindAction.NewFolder,          Key.N     | Key.Control | Key.Shift},
                   //  {BindAction.NewFolder,          Key.N     | Key.Shift }, // 系统默认自带
                };
                Shortcuts = new int[(int)BindAction.KEYBOARD_ACTION_COUNT];
                PluginShortcuts = new Dictionary<string, int[]>();
                foreach(var pair in dict) {
                    Shortcuts[(int)pair.Key] = (int)pair.Value | QTUtility.FLAG_KEYENABLED;
                }
                UseTabSwitcher = true;
            }
        }

        [Serializable]
        public class _Plugin {
            public string[] Enabled              { get; set; }

            public _Plugin() {
                Enabled = new string[0];
            }
        }

        [Serializable]
        public class _Lang {
            public string[] PluginLangFiles      { get; set; }
            public bool UseLangFile              { get; set; }
            public string LangFile { get; set; }
            public string BuiltInLang { get; set; }
            public int BuiltInLangSelectedIndex { get; set; }
            public _Lang() {
                UseLangFile = false;
                BuiltInLang = "English";
                LangFile = "";
                PluginLangFiles = new string[0];
                // WorkingConfig.lang.BuiltInLangSelectedIndex;
                // modify by qwop  at http://q.cnblogs.com/q/14857/
                if (System.Globalization.CultureInfo.InstalledUICulture.Name.Equals("zh-CN"))
                {
                    BuiltInLangSelectedIndex = 1;
                    // Console.WriteLine(System.Globalization.CultureInfo.InstalledUICulture.NativeName);
                }
                else {
                    BuiltInLangSelectedIndex = 0;
                }

              //  BuiltInLangSelectedIndex = 0;// English version
            }
        }

        [Serializable]
        public class _Desktop {
            public int FirstItem                 { get; set; }
            public int SecondItem                { get; set; }
            public int ThirdItem                 { get; set; }
            public int FourthItem                { get; set; }
            public bool GroupExpanded            { get; set; }
            public bool RecentTabExpanded        { get; set; }
            public bool ApplicationExpanded      { get; set; }
            public bool RecentFileExpanded       { get; set; }
            public bool TaskBarDblClickEnabled   { get; set; }
            public bool DesktopDblClickEnabled   { get; set; }
            public bool LockMenu                 { get; set; }
            public bool TitleBackground          { get; set; }
            public bool IncludeGroup             { get; set; }
            public bool IncludeRecentTab         { get; set; }
            public bool IncludeApplication       { get; set; }
            public bool IncludeRecentFile        { get; set; }
            public bool OneClickMenu             { get; set; }
            public bool EnableAppShortcuts       { get; set; }
            public int Width                     { get; set; }
            public int lstSelectedIndex          { get; set; } /*最后选中的菜单项.*/
            public _Desktop() {
                FirstItem = 0;
                SecondItem = 1;
                ThirdItem = 2;
                FourthItem = 3;
                GroupExpanded = false;
                RecentTabExpanded = false;
                ApplicationExpanded = false;
                RecentFileExpanded = false;
                TaskBarDblClickEnabled = true;
                DesktopDblClickEnabled = true;
                LockMenu = false;
                TitleBackground = false;
                IncludeApplication = true;
                IncludeRecentTab = true;
                IncludeApplication = true;
                IncludeRecentFile = true;
                OneClickMenu = false;
                EnableAppShortcuts = true;
                Width = 80;

                // qwop's default value.
                Width = 12;

                // add by qwop.
                lstSelectedIndex = 0;
            }
        }
    }

    public static class ConfigManager {
        public static Config LoadedConfig;

        public static void Initialize() {
            LoadedConfig = new Config();
            ReadConfig();

        }

        public static void UpdateConfig(bool fBroadcast = true) {
            QTUtility.TextResourcesDic = Config.Lang.UseLangFile && File.Exists(Config.Lang.LangFile)
                    ? QTUtility.ReadLanguageFile(Config.Lang.LangFile)
                    : null;
            QTUtility.ValidateTextResources();
            StaticReg.ClosedTabHistoryList.MaxCapacity = Config.Misc.TabHistoryCount;
            StaticReg.ExecutedPathsList.MaxCapacity = Config.Misc.FileHistoryCount;
            DropDownMenuBase.InitializeMenuRenderer();
            ContextMenuStripEx.InitializeMenuRenderer();
            PluginManager.RefreshPlugins();
            InstanceManager.LocalTabBroadcast(tabbar => tabbar.RefreshOptions());
            if(fBroadcast) {
                // SyncTaskBarMenu(); todo
                InstanceManager.StaticBroadcast(() => {
                    ReadConfig();
                    UpdateConfig(false);
                });
            }
        }

        public static void ReadConfig() {
            const string RegPath = RegConst.Root + RegConst.Config;

            var categories =
                from categoryProperty in typeof(Config).GetProperties()
                where categoryProperty.CanWrite
                let categoryType = categoryProperty.PropertyType
                let categoryObject = categoryProperty.GetValue(LoadedConfig, null)
                select new {
                    keyPath = RegPath + categoryType.Name.Substring(1),
                    categoryObject, 
                    settings = (
                        from settingProperty in categoryType.GetProperties()
                        select new {
                            name = settingProperty.Name,
                            type = settingProperty.PropertyType,
                            value = settingProperty.GetValue(categoryObject, null),
                            property = settingProperty
                        }
                    )
                };

            foreach(var category in categories) {
                using (var key=Registry.CurrentUser.CreateSubKey(category.keyPath)) {
                    foreach(var setting in category.settings) {
                        object value = key.GetValue(setting.name);
                        if (value == null) { continue;}

                        Type t = setting.type;
                        try {
                            if (t == typeof(bool)) {
                                value = (int)value != 0;
                            } else if (t.IsEnum) {
                                value = Enum.Parse(t, value.ToString());
                            } else if (t != typeof(int) && t != typeof(string)) {
                                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value.ToString()))) {
                                    if (t == typeof(Font)) {
                                        var ser = new DataContractJsonSerializer(typeof(XmlSerializableFont));
                                        var xsf = ser.ReadObject(stream) as XmlSerializableFont;
                                        value = xsf == null ? null : xsf.ToFont();
                                    } else {
                                        var ser = new DataContractJsonSerializer(t);
                                        value = ser.ReadObject(stream);
                                    }
                                }
                            }

                            setting.property.SetValue(category.categoryObject, value, null);
                        } catch {}
                    }
                }
            }

            using(IDLWrapper wrapper = new IDLWrapper(Config.Window.DefaultLocation)) {
                if(!wrapper.Available) {
                    Config.Window.DefaultLocation = new Config._Window().DefaultLocation;
                }
            }
            Config.Tips.PreviewFont = Config.Tips.PreviewFont ?? Control.DefaultFont;
            Config.Tips.PreviewMaxWidth = QTUtility.ValidateMinMax(Config.Tips.PreviewMaxWidth, 128, 1920);
            Config.Tips.PreviewMaxHeight = QTUtility.ValidateMinMax(Config.Tips.PreviewMaxHeight, 96, 1200);
            Config.Misc.TabHistoryCount = QTUtility.ValidateMinMax(Config.Misc.TabHistoryCount, 1, 30);
            Config.Misc.FileHistoryCount = QTUtility.ValidateMinMax(Config.Misc.FileHistoryCount, 1, 30);
            Config.Misc.NetworkTimeout = QTUtility.ValidateMinMax(Config.Misc.NetworkTimeout, 0, 120);
            Config.Skin.TabHeight = QTUtility.ValidateMinMax(Config.Skin.TabHeight, 10, 50);
            Config.Skin.TabMinWidth = QTUtility.ValidateMinMax(Config.Skin.TabMinWidth, 10, 50);
            Config.Skin.TabMaxWidth = QTUtility.ValidateMinMax(Config.Skin.TabMaxWidth, 50, 999);
            Config.Skin.OverlapPixels = QTUtility.ValidateMinMax(Config.Skin.TabHeight, 0, 20);
            Config.Skin.TabTextFont = Config.Skin.TabTextFont ?? Control.DefaultFont;
            Func<Padding, Padding> validatePadding = p => {
                p.Left   = QTUtility.ValidateMinMax(p.Left,   0, 99);
                p.Top    = QTUtility.ValidateMinMax(p.Top,    0, 99);
                p.Right  = QTUtility.ValidateMinMax(p.Right,  0, 99);
                p.Bottom = QTUtility.ValidateMinMax(p.Bottom, 0, 99);
                return p;
            };
            Config.Skin.RebarSizeMargin = validatePadding(Config.Skin.RebarSizeMargin);
            Config.Skin.TabContentMargin = validatePadding(Config.Skin.TabContentMargin);
            Config.Skin.TabSizeMargin = validatePadding(Config.Skin.TabSizeMargin);
            using(IDLWrapper wrapper = new IDLWrapper(Config.Skin.TabImageFile)) {
                if(!wrapper.Available) Config.Skin.TabImageFile = "";
            }
            using(IDLWrapper wrapper = new IDLWrapper(Config.Skin.RebarImageFile)) {
                if(!wrapper.Available) Config.Skin.RebarImageFile = "";
            }
            using(IDLWrapper wrapper = new IDLWrapper(Config.BBar.ImageStripPath)) {
                // todo: check dimensions
                if(!wrapper.Available) Config.BBar.ImageStripPath = "";
            }
            List<int> blist = Config.BBar.ButtonIndexes.ToList();
            blist.RemoveAll(i => (i.HiWord() - 1) >= Config.BBar.ActivePluginIDs.Length);
            Config.BBar.ButtonIndexes = blist.ToArray();
            var keys = Config.Keys.Shortcuts;
            Array.Resize(ref keys, (int)BindAction.KEYBOARD_ACTION_COUNT);
            Config.Keys.Shortcuts = keys;
            foreach(var pair in Config.Keys.PluginShortcuts.Where(p => p.Value == null).ToList()) {
                Config.Keys.PluginShortcuts.Remove(pair.Key);
            }
            if(QTUtility.IsXP) Config.Tweaks.AlwaysShowHeaders = false;
            if(!QTUtility.IsWin7) Config.Tweaks.RedirectLibraryFolders = false;
            if(!QTUtility.IsXP) Config.Tweaks.KillExtWhileRenaming = true;
            if(QTUtility.IsXP) Config.Tweaks.BackspaceUpLevel = true;
            if(!QTUtility.IsWin7) Config.Tweaks.ForceSysListView = true;
        }

        public static void WriteConfig(bool DesktopOnly = false) {
            const string RegPath = RegConst.Root + RegConst.Config;
            
            //Returns details of setting properties from all categories, or only Desktop category
            var settings =
                from categoryProperty in typeof(Config).GetProperties()
                where DesktopOnly ? categoryProperty.Name == "desktop" : categoryProperty.CanWrite
                let categoryType = categoryProperty.PropertyType
                let categoryObject = categoryProperty.GetValue(LoadedConfig,null)
                from settingProperty in categoryType .GetProperties()
                select new {
                    keyPath = RegPath + categoryType.Name.Substring(1),
                    name = settingProperty.Name,
                    type = settingProperty.PropertyType,
                    value = settingProperty.GetValue(categoryObject, null)
                };

            foreach(var setting in settings) {
                using (var key=Registry.CurrentUser.CreateSubKey(setting.keyPath)) {
                    Type t = setting.type;
                    object value = setting.value;

                    if (t==typeof(bool)) {
                        value=(bool)value ? 1 : 0;
                    } else if (t != typeof(int) && t != typeof(string) && !t.IsEnum) {
                        if (t==typeof(Font)) {
                            value = XmlSerializableFont.FromFont((Font)value);
                            t = typeof(XmlSerializableFont);
                        }
                        var ser = new DataContractJsonSerializer(t);
                        using (var stream=new MemoryStream()) {
                            try {
                                ser.WriteObject(stream,value);
                            } catch (Exception e) {
                                QTUtility2.MakeErrorLog(e);
                            }
                            stream.Position = 0;
                            value = new StreamReader(stream).ReadToEnd();
                        }
                    }

                    key.SetValue(setting.name,value);
                }
            }
        }
    }
}

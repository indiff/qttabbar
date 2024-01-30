/* File Info 
 * Author:      indiff
 * CreateTime:  2021/1/5下午1:58:08 
 * LastEditor:  indiff
 * ModifyTime:  2021/8/28下午7:47:22 
 * Description: 
*/
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
using System.IO;
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

   /* 
    * @描述: 标签位置
    */     
    public enum TabPos {
        Rightmost,
        Right,
        Left,
        Leftmost,
        LastActive,
    }

   /* 
    * @描述: 拉伸模式
    */   
    public enum StretchMode {
        Full,
        Real,
        Tile,
    }

   /* 
    * @描述: 鼠标的目标
    */   
    public enum MouseTarget {
        Anywhere,
        Tab,
        TabBarBackground,
        FolderLink,
        ExplorerItem,
        ExplorerBackground
    }

   /* 
    * @描述: 鼠标弦
    */  
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

    /* 
    * @描述: 绑定动作
    */  
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
        /***** add by indiff end *****/
       // AddToGroup,  // 新增到标签组 ， 添加一个新操作有问题
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

        /******* add by indiff start *****/
        // add by indiff 2012 08 10
        OpenCmd
        ,
        ItemsOpenInNewTabNoSel //add bool indiff 2012 08 12
            /***** add by indiff end *****/

          /******* add by indiff start *****/
            // add by indiff 2019 12 16 19:27
        , SortTab 
        , TurnOffRepeat
        //add bool indiff 2019 12 16 19:27
        , KEYBOARD_ACTION_COUNT2
    }

    [Serializable]
    public class Config {
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
        public static _Desktop Desktop { get { return ConfigManager.LoadedConfig.desktop; } }   /*关于信息*/

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
            public bool CaptureWeChatSelection   { get; set; } // 是否捕获微信、qq、钉钉的打开文件选中状态
            public bool RestoreSession           { get; set; }
            public bool RestoreOnlyLocked        { get; set; }
            public bool CloseBtnClosesUnlocked   { get; set; }
            public bool CloseBtnClosesSingleTab  { get; set; }
            public bool TrayOnClose              { get; set; }
            public bool TrayOnMinimize           { get; set; }
            public bool AutoHookWindow           { get; set; }
            public bool ShowFailNavMsg           { get; set; } // SHOW_FAIL_NAV_MSG
           
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
                if (QTUtility.IsThanWin11)
                {
                    CaptureWeChatSelection = false;
                }
                else
                {
                    CaptureWeChatSelection = true;
                }
                
                RestoreSession = true;
                RestoreOnlyLocked = false;
                CloseBtnClosesUnlocked = false;
                CloseBtnClosesSingleTab = true;
                TrayOnClose = false;
                TrayOnMinimize = false;
                // 默认关闭自动启动hook
                AutoHookWindow = false;
  //              string idl = Environment.OSVersion.Version >= new Version(6, 1)
  //                       ? "::{031E4825-7B94-4DC3-B131-E946B44C8DD5}"  // Libraries
  //                     : "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // Computer
                string idl = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"; // 我的电脑， 默认打开
                using(IDLWrapper w = new IDLWrapper(idl)) {
                    DefaultLocation = w.IDL;
                }
                // 不显示导航失败的提示框
                ShowFailNavMsg = false;
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
            public bool NeedPlusButton           { get; set; }

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
                ActivateNewTab = true;  // 是否立即切换到新标签
                NeverOpenSame = true;   //  重用新标签
                RenameAmbTabs = true;  // 模糊重命名标签
                DragOverTabOpensSDT = false; // 拖拽标签后的操作
                ShowFolderIcon = true;  // 显示驱动器图标
                ShowSubDirTipOnTab = false;  // 在文件夹显示子标签
                ShowDriveLetters = false; // 显示驱动器字符
                ShowCloseButtons = true;  // 显示关闭按钮
                CloseBtnsWithAlt = false; // 只有alt按住进行关闭
                CloseBtnsOnHover = false;  // 仅鼠标悬浮关闭
                ShowNavButtons = false;  // 在标签显示导航按钮 (	(默认配置关闭按钮显示)标签栏右侧的两个按钮能否添加一个选项以控制是否去掉呢。#28）
                NavButtonsOnRight = true; // 在最右边
                MultipleTabRows = true; // 允许多行标签
                ActiveTabOnBottomRow = false; // 始终将活动标签置于底部行
                NeedPlusButton = true; // 是否显示蓝色新增标签按钮
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
                if (QTUtility.IsWin7)
                {
                    AlwaysShowHeaders = true;  // 显示列标题
                }
                else {
                    AlwaysShowHeaders = false;  // 显示列标题
                }
                
                RedirectLibraryFolders = false; // 使用库文件夹
                KillExtWhileRenaming = true;  // 重命名时候，不使用扩展名
                F2Selection = false; // 禁用F2重命名周期选择
                WrapArrowKeySelection = true; // 使用箭头键时候环绕选择文件夹
                BackspaceUpLevel = true;  // backupspace 键回到上一级目录
                HorizontalScroll = true;  // 同时按住shift滚轮水平滚动
                ForceSysListView = false; // 启用旧版列表视图控件
                ToggleFullRowSelect = QTUtility.IsXP; // 详细视图选中整行
                DetailsGridLines = false;  // 网格线
                AlternateRowColors = false;// 交替行颜色
                AltRowForegroundColor = SystemColors.WindowText; // 前景色
                AltRowBackgroundColor = QTUtility2.MakeColor(0xfaf5f1); // 背景色
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

                ShowSubDirTips = true;  // 是否显示子目录提示
                SubDirTipsPreview = true;  // 子目录提示预览
                SubDirTipsFiles = true;  // 子目录提示文件
                SubDirTipsWithShift = false ; // 仅当shift键按下显示子目录
                ShowTooltipPreviews = true;  
                ShowPreviewInfo = true; // 启用文件预览
                ShowPreviewsWithShift = true; // 仅当shift健按下, 启用文件预览
                
                // 预览的宽高
                PreviewMaxWidth = 600;
                PreviewMaxHeight = 400;
                //  PreviewMaxWidth = 512;
                // PreviewMaxHeight = 256;
                //  字体配置
                PreviewFont = Control.DefaultFont;
                PreviewFont = new Font(new FontFamily("微软雅黑"), 9f);
                // 文本类型扩展名 添加 yml yaml 文件支持
                TextExt = new List<string> { 
                    ".txt",".rtf"
                    ,".ini", ".inf",".properties", ".ruleset", ".settings"
                    ,".cs", ".log"
                    ,".js", ".vbs", ".bat", ".cmd", ".sh"
                    ,".c",".cpp",".cc",".h", ".rc"
                    ,".xml"
                    ,".yml",".yaml"
                    ,".htm",".html",".mht",".mhtml", ".shtml", ".hta"
                    ,".HxT",".HxC",".hhc",".hhk", ".hhp"
                    ,".java"
                    ,".sql"
                    ,".csv"
                    ,".md" 

                    ,".m" 
                    ,".reg" 

                    ,".wxl" 
                    ,".wxs" 
                    
                    ,".py", ".rb"
                    ,".jsp", ".asp", ".php",".aspx"
                    ,".resx",".xaml",  ".config", ".manifest", ".csproj", ".vbproj"
                };
                // 配置默认的图像扩展名
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
            public bool SoundBox { get; set; }
            public bool EnableLog { get; set; }

            public _Misc() {
                TaskbarThumbnails = false;
                KeepHistory = true;
                TabHistoryCount = 15;
                KeepRecentFiles = true;
                FileHistoryCount = 15;
                NetworkTimeout = 0;
                AutoUpdate = true;
                // 默认关闭声音播放
                SoundBox = false;
                // 默认不启用日志功能 
                EnableLog = false;
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
            public Color ToolBarTextColor        { get; set; }
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
            public bool SkinAutoColorChangeClose              { get; set; }
            public bool DrawHorizontalExplorerBarBgColor { get; set; }
            public bool DrawVerticalExplorerBarBgColor { get; set; }

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
                UseTabSkin = false;  // 标签背景
                TabImageFile = "";  // 标签背景文件
                TabSizeMargin = Padding.Empty;  // 设置边缘
                TabContentMargin = Padding.Empty; // 内容边缘
                OverlapPixels = 0;  // 
                HitTestTransparent = false;
                TabHeight = 30;  // 高度
                TabMinWidth = 100;  // 最小宽度
                TabMaxWidth = 200;  // 最大宽度
                FixedWidthTabs = false;
                TabTextFont = new Font(new FontFamily("微软雅黑"), 9f);
                ToolBarTextColor = Color.Black;  // 工具栏文本颜色
                TabTextActiveColor = Color.Black;
                TabTextInactiveColor = Color.Black;
                TabTextHotColor = Color.Black;
                // TabShadActiveColor = Color.Gray;
                TabShadActiveColor = Color.WhiteSmoke;
                TabShadInactiveColor = Color.WhiteSmoke;
                TabShadHotColor = Color.WhiteSmoke;
                RebarColor = Color.WhiteSmoke;
                // RebarColor = Color.FromArgb(230,230,230);
                // 设置标签背景色
                // RebarColor = Color.FromArgb(245, 246, 247);
                TabTitleShadows = false;  // 标签文本阴影是否启用
                TabTextCentered = true; // 标签文本是否居中
                UseRebarBGColor = true;  // 是否启用配置背景颜色
                UseRebarImage = false;  // 是否工具栏自定义图片,启用自定义图片
                RebarStretchMode = StretchMode.Tile;  // 布局方式
                RebarImageFile = "";  // 工具栏自定义图片
                RebarImageSeperateBars = false;
                RebarSizeMargin = Padding.Empty;
                ActiveTabInBold = true;
                SkinAutoColorChangeClose = false;  // 是否关闭自动变色？
                DrawHorizontalExplorerBarBgColor = false; // 
                DrawVerticalExplorerBarBgColor = false; // 
            }

            internal void SwitchNighMode(bool isNighMode)
            {
                if (this.SkinAutoColorChangeClose)
                {
                    return;
                }

                if (isNighMode)
                {
                    QTUtility2.log("change nightMode white skinChanged " + this.SkinAutoColorChangeClose);
                    // UseTabSkin = false;  // 标签背景
                    // TabImageFile = "";  // 标签背景文件
                    // TabSizeMargin = Padding.Empty;  // 设置边缘
                    // TabContentMargin = Padding.Empty; // 内容边缘
                    // OverlapPixels = 0;  // 
                    // HitTestTransparent = false;
                    // TabHeight = 24;
                    // TabMinWidth = 50;
                    // TabMaxWidth = 200;
                    // FixedWidthTabs = false;
                    // TabTextFont = new Font(new FontFamily("微软雅黑"), 9f );
                    ToolBarTextColor = Color.White;
                    TabTextActiveColor = Color.White;
                    TabTextInactiveColor = Color.White;
                    TabTextHotColor = Color.White;
                    TabShadActiveColor = Color.Black;
                    TabShadInactiveColor = Color.Black;
                    TabShadHotColor = Color.Black;
                    RebarColor = Color.Black;
                    UseRebarBGColor = true;
                    // TabTitleShadows = false;
                    // TabTextCentered = false;
                    // UseRebarBGColor = false;
                    // RebarColor = Color.FromArgb(230,230,230);
                    // 设置标签背景色
                    
                    // UseRebarImage = false;  // 是否工具栏自定义图片,启用自定义图片
                    // RebarStretchMode = StretchMode.Tile;
                    // RebarImageFile = "";  // 工具栏自定义图片
                    // RebarImageSeperateBars = false;
                    // RebarSizeMargin = Padding.Empty;
                    // ActiveTabInBold = true;
                }
                else
                {
                    // UseTabSkin = false;  // 标签背景
                    // TabImageFile = "";  // 标签背景文件
                    // TabSizeMargin = Padding.Empty;  // 设置边缘
                    // TabContentMargin = Padding.Empty; // 内容边缘
                    // OverlapPixels = 0;  // 
                    // HitTestTransparent = false;
                    // TabHeight = 24;
                    // TabMinWidth = 50;
                    // TabMaxWidth = 200;
                    // FixedWidthTabs = false;
                    // TabTextFont = new Font(new FontFamily("微软雅黑"), 9f);
                    QTUtility2.log("change nightMode black skinChanged " + this.SkinAutoColorChangeClose);
                    ToolBarTextColor = Color.Black;
                    TabTextActiveColor = Color.Black;
                    TabTextInactiveColor = Color.Black;
                    TabTextHotColor = Color.Black;
                    // TabShadActiveColor = Color.FromArgb(245, 246, 247);
                    TabShadActiveColor = Color.White;
                    TabShadInactiveColor = Color.White;
                    TabShadHotColor = Color.White;
                    // RebarColor = Color.FromArgb(245, 246, 247);
                    RebarColor = Color.White;
                    // TabTitleShadows = false;
                    // TabTextCentered = false;
                    UseRebarBGColor = true;
                    // // RebarColor = Color.FromArgb(230,230,230);
                    // // 设置标签背景色
                    // UseRebarImage = false;  // 是否工具栏自定义图片,启用自定义图片
                    // RebarStretchMode = StretchMode.Tile;
                    // RebarImageFile = "";  // 工具栏自定义图片
                    // RebarImageSeperateBars = false;
                    // RebarSizeMargin = Padding.Empty;
                    // ActiveTabInBold = true;
                }
                SkinAutoColorChangeClose = false;
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

                /* indiff 's default. */
                ButtonIndexes	=	QTUtility.IsXP
                        // ? new int[] { 1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 21, 9, 20  }
                        // : new System.Int32[] { 3, 4, 5, 0, 6, 7, 0, 17, 11, 12, 14, 15, 13, 0, 21, 9, 19, 10 };
                // 去掉分割线
                        ? new int[] { 1, 2,  3, 4, 5,  6, 7,  11, 13, 12, 14, 15,  21, 9, 20  }
                        : new System.Int32[] { 3, 4, 5, 6, 7,  17, 11, 12, 14, 15, 13,  21, 9, 19, 10 };
                ActivePluginIDs = new string[0];
                LargeButtons	= true;  // 是否显示大按钮
                LockSearchBarWidth	=	true;  // 锁定搜索框大小
                LockDropDownButtons	=	true;  // 锁定下拉按钮菜单顺序
                ShowButtonLabels	=	true; // 是否显示按钮标签
                ImageStripPath	=	"";  // 自定义图片路径
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
                // 全局鼠标动作
                GlobalMouseActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.X1, BindAction.GoBack},
                    {MouseChord.X2, BindAction.GoForward},
                    {MouseChord.X1 | MouseChord.Ctrl, BindAction.GoFirst},
                    {MouseChord.X2 | MouseChord.Ctrl, BindAction.GoLast}
                };
               // 标签动作
                TabActions = new Dictionary<MouseChord, BindAction> { 
                    {MouseChord.Middle, BindAction.CloseTab},
                    {MouseChord.Ctrl | MouseChord.Left, BindAction.LockTab},
                    {MouseChord.Double, BindAction.UpOneLevelTab},
                };
                // 标签Bar处动作
               BarActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Double, BindAction.NewTab},
                    {MouseChord.Middle, BindAction.RestoreLastClosed},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.TearOffCurrent}
                };
                // 文件夹链接动作
                LinkActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.None, BindAction.ItemsOpenInNewTabNoSel},
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewWindow}
                };
                // 资源管理器项目空白处
               ItemActions = new Dictionary<MouseChord, BindAction> {
                    {MouseChord.Middle, BindAction.ItemOpenInNewTab},
                    {MouseChord.Ctrl | MouseChord.Middle, BindAction.ItemOpenInNewTabNoSel}                        
                };

               // 资源管理器空白处
               MarginActions = new Dictionary<MouseChord, BindAction> {
                    { MouseChord.Double, BindAction.UpOneLevel}
                    // add by qwop //
                    ,{ MouseChord.Middle, BindAction.BrowseFolder}
                    // ctrl + 双击 打开命令提示符
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
                    // 取消锁定键盘快捷键
                   // {BindAction.LockCurrent,        Key.L     | Key.Control},
                   // {BindAction.LockAll,            Key.L     | Key.Control | Key.Shift},
                    {BindAction.BrowseFolder,       Key.O     | Key.Control},
                    {BindAction.ShowOptions,        Key.O     | Key.Alt},
                    {BindAction.ShowToolbarMenu,    Key.Oemcomma  | Key.Alt},
                    {BindAction.ShowTabMenuCurrent, Key.OemPeriod | Key.Alt},
                    {BindAction.ShowGroupMenu,      Key.G     | Key.Alt},
                    {BindAction.ShowUserAppsMenu,   Key.H     | Key.Alt},
                    {BindAction.ShowRecentTabsMenu, Key.U     | Key.Alt},
                    {BindAction.ShowRecentFilesMenu,Key.F     | Key.Alt},
                    


                    // Bug fix 热键冲突， 调整 by indiff 
                    // {BindAction.NewFile,            Key.N     | Key.Control},
                    {BindAction.NewFile,            Key.N     | Key.Control | Key.Alt},
                    // {BindAction.NewFolder,          Key.N     | Key.Control | Key.Shift},
                   //  {BindAction.NewFolder,          Key.N     | Key.Shift }, // 系统默认自带
                   // 创建标签组
                   {BindAction.CreateNewGroup,     Key.D    | Key.Control},
                   // 添加到标签组
                 //  {BindAction.AddToGroup,         Key.D    | Key.Control  },
                   // {BindAction.AddToGroup,         Key.G    | Key.Control | Key.Alt },
                };
                // 修复数组越界问题 by indiff
                var keyboardActionCount = (int)BindAction.KEYBOARD_ACTION_COUNT;
                Shortcuts = new int[keyboardActionCount];
                // 插件快捷键
                PluginShortcuts = new Dictionary<string, int[]>();
                foreach(var pair in dict)
                {
                    var pairKey = (int)pair.Key;
                    // 修复数组越界问题 by indiff
                    if (pairKey > keyboardActionCount - 1)
                    {
                        continue;
                    }
                    Shortcuts[pairKey] = (int)pair.Value | QTUtility.FLAG_KEYENABLED;
                }
                // 启用标签切换器
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
                LangFile = "";
                PluginLangFiles = new string[0];
                // WorkingConfig.lang.BuiltInLangSelectedIndex;
                // modify by qwop  at http://q.cnblogs.com/q/14857/  // en-US
                var uiCulture = System.Globalization.CultureInfo.InstalledUICulture.Name;
                var lUiCulture = uiCulture.ToLower();
                if (uiCulture.Equals("zh-CN") || lUiCulture.Equals("zh") || lUiCulture.Equals("cn"))
                {
                    BuiltInLangSelectedIndex = 1;
                    BuiltInLang = "简体中文";
                }
                else if (uiCulture.Equals("de_DE") || lUiCulture.Equals("de"))
                {
                    BuiltInLangSelectedIndex = 2;
                    BuiltInLang = "German";
                }
                else if (uiCulture.Equals("pt_BR") || lUiCulture.Equals("br") ||  lUiCulture.Equals("pt"))
                {
                    BuiltInLangSelectedIndex = 3;
                    BuiltInLang = "Brazil";
                }
                else if (uiCulture.Equals("es_ES") || lUiCulture.Equals("es") )
                {
                    BuiltInLangSelectedIndex = 4;
                    BuiltInLang = "Spanish";
                }
                else if (uiCulture.Equals("fr_FR") || lUiCulture.Equals("fr"))
                {
                    BuiltInLangSelectedIndex = 5;
                    BuiltInLang = "French";
                }
                else if (uiCulture.Equals("tr_TR") || lUiCulture.Equals("tr"))
                {
                    BuiltInLangSelectedIndex = 6;
                    BuiltInLang = "Turkish";
                }
                else if (uiCulture.Equals("ru_RU") || lUiCulture.Equals("ru"))
                {  // 小写判断俄罗斯
                    BuiltInLangSelectedIndex = 7;
                    BuiltInLang = "Russian";
                }
                else {
                    BuiltInLangSelectedIndex = 0;
                    BuiltInLang = "English";
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

                // 最后选中的菜单索引，默认为0.
                lstSelectedIndex = 0;
            }
        }
    }

    public static class ConfigManager {
        public static Config LoadedConfig;

        public static void Initialize() {
            LoadedConfig = new Config();
            QTUtility2.log("初始化配置信息成功");
            ReadConfig();
            QTUtility2.log("注册表读取配置信息成功");
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
            try
            {
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

                                if (t == typeof(bool))
                                {
                                    value = (int)value != 0;
                                }
                                else if (t.IsEnum)
                                {
                                    value = Enum.Parse(t, value.ToString());
                                }
                                else if (t != typeof(int) && t != typeof(string))
                                {
                                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value.ToString())))
                                    {
                                        if (t == typeof(Font))
                                        {
                                            var ser = new DataContractJsonSerializer(typeof(XmlSerializableFont));
                                            var xsf = ser.ReadObject(stream) as XmlSerializableFont;
                                            value = xsf == null ? null : xsf.ToFont();
                                        }
                                        else
                                        {
                                            var ser = new DataContractJsonSerializer(t);
                                            value = ser.ReadObject(stream);
                                        }

                                        QTUtility2.Close(stream);
                                    }
                                }

                                setting.property.SetValue(category.categoryObject, value, null);
                            
                           
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
                // 调整标签最小宽度
				Config.Skin.TabMinWidth = QTUtility.ValidateMinMax(Config.Skin.TabMinWidth, 10, 100);
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
            } catch (Exception e)
            {
                QTUtility2.MakeErrorLog(e, "ReadConfig foreach category");
            }
        }

        public static void WriteConfig(bool DesktopOnly = false) {
            const string RegPath = RegConst.Root + RegConst.Config;
            QTUtility2.log("WriteConfig " + RegPath);
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
                            StreamReader streamReader = new StreamReader(stream);
                            value = streamReader.ReadToEnd();

                            QTUtility2.Close(streamReader);
                            QTUtility2.Close(stream);
                           // if (streamReader != null) { streamReader.Close(); }
                           // if (stream != null) { stream.Close(); }
                        }
                    }
                    key.SetValue(setting.name,value);
                }
            }
        }
    }
}

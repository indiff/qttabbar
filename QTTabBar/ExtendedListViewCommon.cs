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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using QTPlugin;
using QTTabBarLib.Common;
using QTTabBarLib.ExplorerBrowser;
using QTTabBarLib.Interop;
using Control = System.Windows.Forms.Control;
using HResult = QTTabBarLib.Interop.HResult;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using IShellView = QTTabBarLib.Interop.IShellView;
using Timer = System.Windows.Forms.Timer;

namespace QTTabBarLib {
    internal abstract class ExtendedListViewCommon : AbstractListView {

        #region Delegates
        internal delegate bool DoubleClickHandler(Point pt);
        internal delegate void EndLabelEditHandler(LVITEM item);
        internal delegate bool ItemActivatedHandler(Keys modKeys);
        internal delegate void ItemCountChangedHandler(int count);
        internal delegate bool MiddleClickHandler(Point pt);
        internal delegate bool MouseActivateHandler(ref int result);
        internal delegate void SelectionChangedHandler(/*object sender, SelectionChangedEventArgs e*/);
        internal delegate void RefreshHandler();
        #endregion

        #region Events
        internal event DoubleClickHandler DoubleClick;            // OK
        internal event EndLabelEditHandler EndLabelEdit;          // SysListView Only
        internal event ItemActivatedHandler SelectionActivated;        // OK
        internal event ItemCountChangedHandler ItemCountChanged;  // OK
        internal event MiddleClickHandler MiddleClick;            // OK
        internal event MouseActivateHandler MouseActivate;        // OK
        internal event SelectionChangedHandler SelectionChanged;  // OK
        internal event EventHandler SubDirTip_MenuClosed;
        internal event ToolStripItemClickedEventHandler SubDirTip_MenuItemClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MenuItemRightClicked;
        internal event EventHandler SubDirTip_MultipleMenuItemsClicked;
        internal event ItemRightClickedEventHandler SubDirTip_MultipleMenuItemsRightClicked;
        #endregion

        protected static readonly int WM_AFTERPAINT = PInvoke.RegisterWindowMessage("QTTabBar_AfterPaint");
        protected static readonly int WM_REMOTEDISPOSE = PInvoke.RegisterWindowMessage("QTTabBar_RemoteDispose");
        protected static readonly int WM_REGISTERDRAGDROP = PInvoke.RegisterWindowMessage("QTTabBar_RegisterDragDrop");
        protected static readonly int WM_ISITEMSVIEW = PInvoke.RegisterWindowMessage("QTTabBar_IsItemsView");
        protected static readonly int WM_ACTIVATESEL = PInvoke.RegisterWindowMessage("QTTabBar_ActivateSelection");

        protected NativeWindowController ListViewController;
        protected NativeWindowController ShellViewController;
        private DropTargetPassthrough dropTargetPassthrough;
        protected bool fThumbnailPending;
        protected bool fTrackMouseEvent;
        protected IntPtr hwndExplorer;
        private IntPtr hwndSubDirTipMessageReflect;
        protected readonly ShellBrowserEx ShellBrowser;
        protected int subDirIndex = -1;
        protected SubDirTipForm subDirTip;
        protected int thumbnailIndex = -1;
        protected ThumbnailTooltipForm thumbnailTooltip;
        private Timer timer_HoverSubDirTipMenu;
        private Timer timer_Thumbnail;
        protected bool fDragging;

        // private IntPtr hwndListView;
        private static string BG_IMG = Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\Image\bgImage.png";


        internal ExtendedListViewCommon(ShellBrowserEx shellBrowser, IntPtr hwndShellView, IntPtr hwndListView, IntPtr hwndSubDirTipMessageReflect) {
            this.ShellBrowser = shellBrowser;
            this.hwndSubDirTipMessageReflect = hwndSubDirTipMessageReflect;
            // this.hwndListView = hwndListView;

            ListViewController = new NativeWindowController(hwndListView);
            ListViewController.MessageCaptured += ListViewController_MessageCaptured;
            ShellViewController = new NativeWindowController(hwndShellView);
            ShellViewController.MessageCaptured += ShellViewController_MessageCaptured;

            

            TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
            structure.cbSize = Marshal.SizeOf(structure);
            structure.dwFlags = 2;
            structure.hwndTrack = ListViewController.Handle;
            PInvoke.TrackMouseEvent(ref structure);

            timer_HoverSubDirTipMenu = new Timer();
            timer_HoverSubDirTipMenu.Interval = SystemInformation.MouseHoverTime * 6 / 5;
            timer_HoverSubDirTipMenu.Tick += timer_HoverSubDirTipMenu_Tick;

            hwndExplorer = PInvoke.GetAncestor(hwndShellView, 3 /* GA_ROOTOWNER */);

            // If we're late to the party, we'll have to get the IDropTarget the
            // old-fashioned way.  Careful!  Calling RegisterDragDrop will go 
            // through the hook!
            IntPtr ptr = PInvoke.GetProp(hwndListView, "OleDropTargetInterface");
            dropTargetPassthrough = TryMakeDTPassthrough(ptr);
            if(dropTargetPassthrough != null) {
                PInvoke.RevokeDragDrop(hwndListView);
                PInvoke.RegisterDragDrop(hwndListView, dropTargetPassthrough);
            }

            // RefreshViewWatermark(true);
            // 如果文件不存在则不加载背景
            /*if (File.Exists(BG_IMG))
            {
                SetBackgroundImage(true, true, 0, 0);
            }*/

            // 执行不生效
            // SetBackgroundImage(true, true, 0, 0);
            // InstallHooks();
        }

        private CreateWindowExWHookProc hookProc_CreateWindowExW;
        private HookProc hookProc_FillRect;
        private IntPtr hHook_FillRect;

        public delegate int HookProc(IntPtr hDC, [In] ref RECT lprc, IntPtr hbr);

        public IntPtr MyCreateWindowExWProc(
            int exStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x,
            int y,
            int width,
            int height,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInst,
            IntPtr pvParam)
        {
            return IntPtr.Zero;
        }

        int MyFillRect(IntPtr hDC, [In] ref RECT lprc, IntPtr hbr)
        {
            QTUtility2.log("ExtendListViewCommon MyFillRect");
            int ret = PInvoke.FillRect(hDC, ref lprc, hbr);

            RECT pRc;
            PInvoke.GetWindowRect(Handle, out pRc);
            Size wndSize = new Size(lprc.right - pRc.left, lprc.bottom - pRc.top);
            Rectangle rctDw = pRc.ToRectangle();
            //计算图片位置 Calculate picture position
            PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);

            var bgPng = @"D:\下载\Release\Release\x64\Image\bgImage1.png";

            // PInvoke.SaveDC
            if (rendererDown_Normal == null)
            {
                InitializeRenderer();
            }
            IntPtr dC = PInvoke.GetDC(ListViewController.Handle);
            if ((dC != IntPtr.Zero))
            {
                using (Graphics graphics = Graphics.FromHdc(dC))
                {
                    VisualStyleRenderer renderer;
                    // VisualStyleRenderer renderer2;
                    renderer = rendererDown_Normal;
                    // g.DrawImage(QTUtility.ImageListGlobal.Images[base2.ImageKey], rect);
                    var dToutiaoX1080IntellijIdea3Png = @"D:\下载\Release\Release\x64\Image\bgImage.png";
                    using (FreeBitmap freeBitmap = new FreeBitmap(dToutiaoX1080IntellijIdea3Png))
                    using (Bitmap bmp = freeBitmap.Clone())
                    {
                        Point pos = new Point(wndSize.Width - bmp.Width, wndSize.Height - bmp.Height);
                        Size dstSize = new Size(bmp.Width, bmp.Height);

                        bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        QTUtility2.log("SetWaterMarkImage " + dToutiaoX1080IntellijIdea3Png);
                        graphics.DrawImage(bmp, rctDw);
                    }

                    renderer.DrawBackground(graphics, rctDw);
                    // renderer2.DrawBackground(graphics, rctUp);
                }
                PInvoke.ReleaseDC(ListViewController.Handle, dC);
                PInvoke.ValidateRect(ListViewController.Handle, IntPtr.Zero);
                // m.Result = IntPtr.Zero;
            }

            // return PInvoke.CallNextHookEx(hHook_Key, nCode, wParam, lParam);
            return ret;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int dwThreadId);

        private void InstallHooks()
        {
            // this.myCallbackDelegate = new HookProc(MyCreateWindowExWProc);
            // hookProc_CreateWindowExW = new CreateWindowExWHookProc(MyCreateWindowExWProc);
            hookProc_FillRect = new HookProc(MyFillRect);
            int currentThreadId = PInvoke.GetCurrentThreadId();
            IntPtr moduleHandle = PInvoke.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            hHook_FillRect = SetWindowsHookEx(99999 + 1, hookProc_FillRect, moduleHandle, currentThreadId);
            QTUtility2.log("ExtendListViewCommon SetWindowsHookEx " + hHook_FillRect);
        }


        private unsafe void SetWaterMarkImage(Bitmap bmp)
        {
            if (bmp != null)
            {
                LVBKIMAGE* lParam = stackalloc LVBKIMAGE[1];
                lParam->ulFlags = 805306368;
                lParam->hBmp = bmp.GetHbitmap(Color.Black);
                if (!(IntPtr.Zero == PInvoke.SendMessage(this.Handle, 4234, (void*)null, (void*)lParam)) || !(lParam->hBmp != IntPtr.Zero))
                    return;
                PInvoke.DeleteObject(lParam->hBmp);
            }
            else
            {
                LVBKIMAGE* lParam = stackalloc LVBKIMAGE[1];
                lParam->ulFlags = 268435456;
                PInvoke.SendMessage(this.Handle, 4234, (void*)null, (void*)lParam);
            }
        }

        public bool SetBackgroundImage2(bool isWatermark, bool isTiled, int xOffset, int yOffset)
        {
            LVBKIMAGE lvbkimage = new LVBKIMAGE();
            // IntPtr handle = ShellViewController.Handle;
            IntPtr handle = ListViewController.Handle;
            /*var findWindowEx = PInvoke.FindWindowEx(ListViewController.Handle, IntPtr.Zero, "DirectUIHWND", null);
            if (handle != findWindowEx)
            {
                handle = findWindowEx;
            }*/
            // We have to clear any pre-existing background image, otherwise the attempt to set the image will fail.
            // We don't know which type may already have been set, so we just clear both the watermark and the image.
            lvbkimage.ulFlags = LVBKIF_TYPE_WATERMARK;
            IntPtr result = PInvoke.SendMessageLVBKIMAGE(handle, LVM_SETBKIMAGE, 0, ref lvbkimage);
            lvbkimage.ulFlags = LVBKIF_SOURCE_HBITMAP;
            result = PInvoke.SendMessageLVBKIMAGE(handle, LVM_SETBKIMAGE, 0, ref lvbkimage);

            var dToutiaoX1080IntellijIdea3Png = @"D:\下载\Release\Release\x64\Image\bgImage1.png";
            // var dToutiaoX1080IntellijIdea3Png = @"D:\Users\Administrator\Documents\Tencent Files\531299332\Image\Group2\IY\S2\IYS2F)882TXGVT[JIR[`4BY.bmp";

            using (FreeBitmap freeBitmap = new FreeBitmap(dToutiaoX1080IntellijIdea3Png))
            using (Bitmap bm = freeBitmap.Clone())
            {
                // bm.RotateFlip(RotateFlipType.RotateNoneFlipX);
                QTUtility2.log("SetWaterMarkImage " + dToutiaoX1080IntellijIdea3Png);
                

                lvbkimage.hBmp = bm.GetHbitmap();
                lvbkimage.ulFlags = isWatermark ? LVBKIF_TYPE_WATERMARK : (isTiled ? LVBKIF_SOURCE_HBITMAP | LVBKIF_STYLE_TILE : LVBKIF_SOURCE_HBITMAP);
                lvbkimage.xOffset = xOffset;
                lvbkimage.yOffset = yOffset;

                PInvoke.SendMessage(this.Handle, 4234, IntPtr.Zero, ref lvbkimage);
                // result = PInvoke.SendMessageLVBKIMAGE(handle, LVM_SETBKIMAGE, 0, ref lvbkimage);
            }
            return (result != IntPtr.Zero);
        }

        public override IntPtr Handle {
            get { return ListViewController.Handle; }
        }

        public override void RefreshViewWatermark(bool fClear)
        {
            // if (!this.VistaLayout)  return;
            if ( true  
                 // Config.Bool(Scts.ViewWatermarking)
                 )
            {
                var dToutiaoX1080IntellijIdea3Png = @"D:\toutiao\1920x1080-intellij-idea3.png";

                using (FreeBitmap freeBitmap = new FreeBitmap(dToutiaoX1080IntellijIdea3Png))
                using (Bitmap bmp = freeBitmap.Clone())
                {
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    QTUtility2.log("SetWaterMarkImage " + dToutiaoX1080IntellijIdea3Png);
                    SetWaterMarkImage(bmp);
                }
                /*switch (this.ViewPerceivedType)
                {
                    case PerceivedType.Unknown:
                        bmp = ExplorerManager.GetWatermarkImage(BmpCacheKey.Watermark_General);
                        break;
                    case PerceivedType.Image:
                        bmp = ExplorerManager.GetWatermarkImage(BmpCacheKey.Watermark_Picture);
                        break;
                    case PerceivedType.Audio:
                        bmp = ExplorerManager.GetWatermarkImage(BmpCacheKey.Watermark_Music);
                        break;
                    case PerceivedType.Video:
                        bmp = ExplorerManager.GetWatermarkImage(BmpCacheKey.Watermark_Movie);
                        break;
                    case PerceivedType.Document:
                        bmp = ExplorerManager.GetWatermarkImage(BmpCacheKey.Watermark_Document);
                        break;
                }*/
            }
            else
            {
                if (!fClear)
                    return;
                SetWaterMarkImage((Bitmap)null);
            }
        }


       
        #region IDisposable Members

        public override void Dispose(bool fDisposing) {
            if(fDisposed) return;
            // Never call NativeWindow.ReleaseHandle().  EVER!!!
            if(ListViewController != null) {
                ListViewController.MessageCaptured -= ListViewController_MessageCaptured;
                ListViewController = null;
            }
            if(ShellViewController != null) {
                ShellViewController.MessageCaptured -= ShellViewController_MessageCaptured;
                ShellViewController = null;
            }
            if(timer_HoverSubDirTipMenu != null) {
                timer_HoverSubDirTipMenu.Dispose();
                timer_HoverSubDirTipMenu = null;
            }
            if(timer_Thumbnail != null) {
                timer_Thumbnail.Dispose();
                timer_Thumbnail = null;
            }
            if(thumbnailTooltip != null) {
                thumbnailTooltip.Dispose();
                thumbnailTooltip = null;
            }
            if(subDirTip != null) {
                subDirTip.Dispose();
                subDirTip = null;
            }
            if(dropTargetPassthrough != null) {
                dropTargetPassthrough.Dispose();
                dropTargetPassthrough = null;
            }

            if (hHook_FillRect != IntPtr.Zero)
            {
                PInvoke.UnhookWindowsHookEx(hHook_FillRect);
                hHook_FillRect = IntPtr.Zero;
            }
            base.Dispose(fDisposing);
        }

        #endregion

        protected abstract IntPtr GetEditControl();

        protected abstract Rectangle GetFocusedItemRect(); 

        public override int GetHotItem() {
            return HitTest(Control.MousePosition, true);
        }

        protected abstract Point GetSubDirTipPoint(bool fByKey);
        
        protected abstract bool HandleCursorLoop(Keys key);

        public override void HandleF2() {
            IntPtr hWnd = GetEditControl();
            if(hWnd == IntPtr.Zero) return;
            string str;
            using(SafePtr lParam = new SafePtr(520)) {
                if(0 >= ((int)PInvoke.SendMessage(hWnd, 13, (IntPtr)260, lParam))) return;
                str = Marshal.PtrToStringUni(lParam);
            }
            if(str.Length <= 2) return;
            int num = str.LastIndexOf(".");
            if(num != -1) {
                IntPtr ptr3 = PInvoke.SendMessage(hWnd, 0xb0, IntPtr.Zero, IntPtr.Zero);
                int start = QTUtility2.GET_X_LPARAM(ptr3);
                int length = QTUtility2.GET_Y_LPARAM(ptr3);
                if((length - start) >= 0) {
                    if((start == 0) && (length == num)) {
                        start = length = num;
                    }
                    else if((start == length) && (length == num)) {
                        start = num + 1;
                        length = str.Length;
                    }
                    else if((start == (num + 1)) && (length == str.Length)) {
                        start = 0;
                        length = -1;
                    }
                    else if((start == 0) && (length == str.Length)) {
                        start = 0;
                        length = 0;
                    }
                    else {
                        start = 0;
                        length = num;
                    }
                    PInvoke.SendMessage(hWnd, 0xb1, (IntPtr)start, (IntPtr)length);
                }   
            }
        }

        public override void HandleShiftKey() {
            if(!Config.Tips.ShowPreviewsWithShift) {
                HideThumbnailTooltip(5);
            }

            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(MouseIsOverListView()) {
                        RefreshSubDirTip();
                    }
                }
                else if(!SubDirTipMenuIsShowing()) {
                    HideSubDirTip(6);
                }
            }
        }

        public override bool HasFocus() {
            return (ListViewController != null &&
                PInvoke.GetFocus() == ListViewController.Handle);
        }

        public override void HideSubDirTip(int iReason = -1) {
            if(subDirTip == null || !subDirTip.IsShowing) return;
            bool fForce = iReason < 0;
            if(fForce || !subDirTip.IsShownByKey) {
                subDirTip.HideSubDirTip(fForce);
                subDirIndex = -1;
            }
        }

        public override void HideSubDirTipMenu() {
            if(subDirTip != null) {
                subDirTip.HideMenu();
            }
        }

        public override void HideSubDirTip_ExplorerInactivated() {
            if((subDirTip != null) && subDirTip.IsShowing) {
                subDirTip.OnExplorerInactivated();
            }
        }

        public override void HideThumbnailTooltip(int iReason = -1) {
            if((thumbnailTooltip != null) && thumbnailTooltip.IsShowing) {
                if(((iReason == 0) || (iReason == 7)) || (iReason == 9)) {
                    thumbnailTooltip.IsShownByKey = false;
                }
                if(thumbnailTooltip.HideToolTip()) {
                    thumbnailIndex = -1;
                }
            }
        }

        public override int HitTest(IntPtr LParam) {
            return HitTest(QTUtility2.PointFromLPARAM(LParam), false);
        }

        public abstract override int HitTest(Point pt, bool ScreenCoords);

        public abstract override bool HotItemIsSelected(); 

        // If the ListView is in Details mode, returns true only if the mouse
        // is over the ItemName column.  Returns true always for any other mode.
        // This function only returns valid results if the mouse is known to be
        // over an item.  Otherwise, its return value is undefined.
        public abstract override bool IsTrackingItemName();

        public const int LVBKIF_SOURCE_HBITMAP = 0x00000001;
        public const int LVBKIF_STYLE_TILE = 0x00000010;
        private const int LVBKIF_TYPE_WATERMARK = 0x10000000;
        private const int LVBKIF_FLAG_ALPHABLEND = 0x20000000;
        public const int LVM_FIRST = 0x1000;
        public const int LVM_SETBKIMAGE = (LVM_FIRST + 68);


        public  bool SetBackgroundImage(bool isWatermark, bool isTiled, int xOffset, int yOffset)
        {
            LVBKIMAGE lvbkimage = new LVBKIMAGE();
            // IntPtr handle = ShellViewController.Handle;
            IntPtr handle = ListViewController.Handle; // DirectUIHWND  SHELLDLL_DefView
            // find parent ShellTabWindowClass  DUIViewWndClassName DirectUIHWND

            // [log] PID:15516 TID:1 2022/9/22 9:17:17  parent name SHELLDLL_DefView
            //     [log] PID:15516 TID:1 2022/9/22 9:17:17  parent name ShellTabWindowClass
            //     [log] PID:15516 TID:1 2022/9/22 9:17:17  parent name CabinetWClass
            var name = PInvoke.GetClassName(handle);
            QTUtility2.log("name " + name);
            var parent = PInvoke.GetParent(handle);
            name = PInvoke.GetClassName(parent);
            QTUtility2.log(" parent name " + name);
            parent = PInvoke.GetParent(parent);
            name = PInvoke.GetClassName(parent);
            QTUtility2.log(" parent name " + name);

            // var findWindowEx = PInvoke.FindWindowEx(parent, IntPtr.Zero, "DUIViewWndClassName", null);
            IntPtr findWindowEx = WindowUtils.FindChildWindow(parent, hwnd => PInvoke.GetClassName(hwnd) == "DirectUIHWND");
            if (IntPtr.Zero != findWindowEx)
            {
                QTUtility2.log(" found DirectUIHWND ");
                handle = findWindowEx;
            }
            // parent = PInvoke.GetParent(parent);
            // name = PInvoke.GetClassName(parent);
            // QTUtility2.log(" parent name " + name);
            /*handle = findParent("ShellTabWindowClass");
            if (handle == IntPtr.Zero)
            {   
                QTUtility2.log("SetBackgroundImage not found class" );
                return false;
            }*/
            // We have to clear any pre-existing background image, otherwise the attempt to set the image will fail.
            // We don't know which type may already have been set, so we just clear both the watermark and the image.
            lvbkimage.ulFlags = LVBKIF_TYPE_WATERMARK;
            IntPtr result = PInvoke.SendMessageLVBKIMAGE(handle, LVM_SETBKIMAGE, 0, ref lvbkimage);
            lvbkimage.ulFlags = LVBKIF_SOURCE_HBITMAP;
            result = PInvoke.SendMessageLVBKIMAGE(handle, LVM_SETBKIMAGE, 0, ref lvbkimage);


            if (File.Exists(BG_IMG))
            {
                using (FreeBitmap freeBitmap = new FreeBitmap(BG_IMG))
                using (Bitmap bm = freeBitmap.Clone())
                {
                    // bm.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    lvbkimage.hBmp = bm.GetHbitmap(Color.Black);
                }
            }
            else
            {
                lvbkimage.hBmp = IntPtr.Zero;
            }
            lvbkimage.ulFlags = isWatermark ? LVBKIF_TYPE_WATERMARK : (isTiled ? LVBKIF_SOURCE_HBITMAP | LVBKIF_STYLE_TILE : LVBKIF_SOURCE_HBITMAP);
            lvbkimage.xOffset = xOffset;
            lvbkimage.yOffset = yOffset;
            result = PInvoke.SendMessageLVBKIMAGE(handle, LVM_SETBKIMAGE, 0, ref lvbkimage);
            QTUtility2.log("SetWaterMarkImage " + BG_IMG);
            return (result != IntPtr.Zero);
        }

        private IntPtr findParent(string className)
        {
            int count = 0;
            do
            {
                var intPtr = PInvoke.GetParent(Handle);
                var name = PInvoke.GetClassName(intPtr);
                if (name.Equals(className))
                {
                    return intPtr;
                }
                count++;
            } while ( count <= 10);
            return IntPtr.Zero;
        }

        protected virtual bool ListViewController_MessageCaptured(ref Message msg) {
            // QTUtility2.log("ListViewController msg\t" + Enum.GetName(typeof(MsgEnum), msg.Msg) + "\tw\t" + msg.WParam + "\tl\t" + msg.LParam);
            if(msg.Msg == WM_AFTERPAINT) {
                RefreshSubDirTip(true);
                // SetBackgroundImage(true, true, 0, 0);
                /*
                RECT rect;
                PInvoke.GetWindowRect(Handle, out rect);
                Rectangle rctDw1 = new Rectangle(0, 0, 500, 1000);
                Rectangle rctDw = rect.ToRectangle();
                /*if ((iter->second.size.cx != wndSize.cx || iter->second.size.cy != wndSize.cy)
                    && m_config.imgPosMode != 0)
                {
                    InvalidateRect(iter->second.hWnd, 0, TRUE);
                }#1#

                PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);


                //裁剪矩形 Clip rect
                // SaveDC(hDC);
                // IntersectClipRect(hDC, lprc->left, lprc->top, lprc->right, lprc->bottom);

                // PInvoke.SaveDC
                if (rendererDown_Normal == null)
                {
                    InitializeRenderer();
                }
                IntPtr dC = PInvoke.GetDC(ListViewController.Handle);
                if ((dC != IntPtr.Zero))
                {
                    using (Graphics graphics = Graphics.FromHdc(dC))
                    {
                        VisualStyleRenderer renderer;
                        // VisualStyleRenderer renderer2;
                        renderer = rendererDown_Normal;
                        // g.DrawImage(QTUtility.ImageListGlobal.Images[base2.ImageKey], rect);
                        var dToutiaoX1080IntellijIdea3Png = @"D:\下载\Release\Release\x64\Image\bgImage.png";
                        using (FreeBitmap freeBitmap = new FreeBitmap(dToutiaoX1080IntellijIdea3Png))
                        using (Bitmap bmp = freeBitmap.Clone())
                        {
                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            QTUtility2.log("SetWaterMarkImage " + dToutiaoX1080IntellijIdea3Png);
                            graphics.DrawImage(bmp, rctDw);
                        }

                        renderer.DrawBackground(graphics, rctDw);
                        // renderer2.DrawBackground(graphics, rctUp);
                    }
                    PInvoke.ReleaseDC(ListViewController.Handle, dC);
                    PInvoke.ValidateRect(ListViewController.Handle, IntPtr.Zero);
                    // m.Result = IntPtr.Zero;
                }*/
                return true;
            }
            else if(msg.Msg == WM_REGISTERDRAGDROP) {
                IntPtr ptr = Marshal.ReadIntPtr(msg.WParam);
                if(dropTargetPassthrough != null) {
                    // If this is the RegisterDragDrop call from the constructor,
                    // don't mess it up!
                    if(dropTargetPassthrough.Pointer == ptr) {
                        return true;
                    }
                    dropTargetPassthrough.Dispose();
                }
                dropTargetPassthrough = TryMakeDTPassthrough(ptr);
                if(dropTargetPassthrough != null) {
                    Marshal.WriteIntPtr(msg.WParam, dropTargetPassthrough.Pointer);
                }
                return true;
            }

            switch(msg.Msg) {
                /*case 7:
                    try
                    {
                        if ( ShellBrowser.FolderView != null)
                        {
                            if ( ShellBrowser.FolderView is IVisualProperties )
                            {
                                
                                IVisualProperties visualProperties = (IVisualProperties)ShellBrowser.FolderView;
                                // int pcr1;
                                // visualProperties.GetColor(VPCOLORFLAGS.VPCF_BACKGROUND, out pcr1);
                                // int pcr2;
                                // visualProperties.GetColor(VPCOLORFLAGS.VPCF_SORTCOLUMN, out pcr2);
                                // int pcr3;
                                // visualProperties.GetColor(VPCOLORFLAGS.VPCF_TEXT, out pcr3);
                                // QTUtility2.log("on focus changed pcr1 : " + pcr1);
                                // QTUtility2.log("on focus changed pcr2 : " + pcr2);
                                // QTUtility2.log("on focus changed pcr3 : " + pcr3);
                                //
                                // visualProperties.SetColor(VPCOLORFLAGS.VPCF_BACKGROUND, pcr1);
                                // visualProperties.SetColor(VPCOLORFLAGS.VPCF_SORTCOLUMN, pcr2);
                                // visualProperties.SetColor(VPCOLORFLAGS.VPCF_TEXT, pcr3);

                                QTUtility2.log("on focus set water mark: " );
                                var dToutiaoX1080IntellijIdea3Png = @"D:\toutiao\1920x1080-intellij-idea3.png";
                                using (FreeBitmap freeBitmap = new FreeBitmap(dToutiaoX1080IntellijIdea3Png))
                                using (Bitmap bmp = freeBitmap.Clone())
                                {
                                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                    QTUtility2.log("SetWaterMarkImage " + dToutiaoX1080IntellijIdea3Png);
                                    // LVBKIMAGE* lParam = stackalloc LVBKIMAGE[1];
                                    // lParam->ulFlags = 805306368;
                                    // lParam->hbm = bmp.GetHbitmap(Color.Black);
                                    // IntPtr hbmp;
                                    visualProperties.SetWatermark(bmp.GetHbitmap(Color.Black), VPWATERMARKFLAGS.VPWF_ALPHABLEND);
                                    SetWaterMarkImage(bmp);
                                }
                            }
                        }
                    }
                    finally
                    {
                        // if (this.ReleaseShellView && shellView != null)
                        //     Marshal.ReleaseComObject((object) shellView);
                    }

                    // if (this.UpdateColors(true) && this.VistaLayout)
                        // this.Invalidate();
                    /*this.OnFocusChanged(true, msg.WParam);
                    if (this.UpdateColors(true) && this.VistaLayout)
                        this.Invalidate();
                    if (this.VistaLayout)
                    {
                        this.compatibleView.HideFocus();
                        break;
                    }#1#
                    break;*/


                /* case WM.ERASEBKGND:
                     RECT rect;
                     PInvoke.GetWindowRect(Handle, out rect);
                     Rectangle rctDw1 = new Rectangle(0, 0, 500, 1000);
                     Rectangle rctDw = rect.ToRectangle();
                     /*if ((iter->second.size.cx != wndSize.cx || iter->second.size.cy != wndSize.cy)
                         && m_config.imgPosMode != 0)
                     {
                         InvalidateRect(iter->second.hWnd, 0, TRUE);
                     }
                      #1#
 
                     // PInvoke.InvalidateRect(Handle, IntPtr.Zero, true);
 
 
                     //裁剪矩形 Clip rect
                     // SaveDC(hDC);
                     // IntersectClipRect(hDC, lprc->left, lprc->top, lprc->right, lprc->bottom);
 
                     // PInvoke.SaveDC
                     if (rendererDown_Normal == null)
                     {
                         InitializeRenderer();
                     }
                     IntPtr dC = PInvoke.GetDC(ListViewController.Handle);
                     if ((dC != IntPtr.Zero))
                     {
                         using (Graphics graphics = Graphics.FromHdc(dC))
                         {
                             VisualStyleRenderer renderer;
                             // VisualStyleRenderer renderer2;
                             renderer = rendererDown_Normal;
                             // g.DrawImage(QTUtility.ImageListGlobal.Images[base2.ImageKey], rect);
                             var dToutiaoX1080IntellijIdea3Png = @"D:\下载\Release\Release\x64\Image\bgImage.png";
                             using (FreeBitmap freeBitmap = new FreeBitmap(dToutiaoX1080IntellijIdea3Png))
                             using (Bitmap bmp = freeBitmap.Clone())
                             {
                                 bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                 QTUtility2.log("SetWaterMarkImage " + dToutiaoX1080IntellijIdea3Png);
                                 graphics.DrawImage(bmp, rctDw);
                             }
 
                             renderer.DrawBackground(graphics, rctDw);
                             // renderer2.DrawBackground(graphics, rctUp);
                         }
                         PInvoke.ReleaseDC(ListViewController.Handle, dC);
                         PInvoke.ValidateRect(ListViewController.Handle, IntPtr.Zero);
                         // m.Result = IntPtr.Zero;
                     }
                    break;*/
                case WM.DESTROY:
                    HideThumbnailTooltip(7);
                    HideSubDirTip(7);
                    ListViewController.DefWndProc(ref msg);
                    OnListViewDestroyed();
                    return true;

                case WM.PAINT:
                    // 直接在 Paint 消息内部操作不行
                    // It's very dangerous to do automation-related things
                    // during WM_PAINT.  So, use PostMessage to do it later.
                    PInvoke.PostMessage(ListViewController.Handle, WM_AFTERPAINT, IntPtr.Zero, IntPtr.Zero);
                    break;

                case WM.MOUSEMOVE:
                    ResetTrackMouseEvent();
                    break;

                case WM.LBUTTONDBLCLK:
                    if(DoubleClick != null) {
                        return DoubleClick(QTUtility2.PointFromLPARAM(msg.LParam));
                    }
                    break;
                
                case WM.MBUTTONUP:
                    if(MiddleClick != null) {
                        MiddleClick(QTUtility2.PointFromLPARAM(msg.LParam));
                    }
                    break;

                case WM.MOUSEWHEEL: {
                    IntPtr handle = PInvoke.WindowFromPoint(QTUtility2.PointFromLPARAM(msg.LParam));
                    if(handle != IntPtr.Zero && handle != msg.HWnd) {
                        Control control = Control.FromHandle(handle);
                        if(control != null) {
                            DropDownMenuReorderable reorderable = control as DropDownMenuReorderable;
                            if((reorderable != null) && reorderable.CanScroll) {
                                PInvoke.SendMessage(handle, WM.MOUSEWHEEL, msg.WParam, msg.LParam);
                            }
                        }
                    }
                    break;
                }

                case WM.MOUSELEAVE:
                    fTrackMouseEvent = true;
                    HideThumbnailTooltip(4);
                    if(((subDirTip != null) && !subDirTip.MouseIsOnThis()) && !subDirTip.MenuIsShowing) {
                        HideSubDirTip(5);
                    }
                    break;
                /*case 48648: // no walking
                    QTUtility2.log("48648");
                    break;*/
            }
            return false;
        }

        private VisualStyleRenderer rendererDown_Hot;
        private VisualStyleRenderer rendererDown_Normal;
        private VisualStyleRenderer rendererDown_Pressed;
        private VisualStyleRenderer rendererUp_Hot;
        private VisualStyleRenderer rendererUp_Normal;
        private VisualStyleRenderer rendererUp_Pressed;
        private void InitializeRenderer()
        {
            rendererDown_Normal = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Normal);
            rendererUp_Normal = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Normal);
            rendererDown_Hot = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Hot);
            rendererUp_Hot = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Hot);
            rendererDown_Pressed = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Pressed);
            rendererUp_Pressed = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Pressed);
        }

        private DropTargetPassthrough TryMakeDTPassthrough(IntPtr pDropTarget) {
            if(pDropTarget != IntPtr.Zero) {
                object obj = Marshal.GetObjectForIUnknown(pDropTarget);
                try {
                    if(obj is _IDropTarget) {

                        // For some reason, the RCW doesn't work in dropTargetPassthrough's
                        // functions if it's created now.  So, we'll just keep the pointer,
                        // and create the RCW each time we need it.
                        return new DropTargetPassthrough(pDropTarget, this);
                    }
                }
                finally {
                    QTUtility2.log("ReleaseComObject obj");
                    Marshal.ReleaseComObject(obj);
                }
            }
            return null;
        }

        public override bool MouseIsOverListView() {
            return (ListViewController != null &&
                PInvoke.WindowFromPoint(Control.MousePosition) == ListViewController.Handle);
        }

        protected bool OnDoubleClick(Point pt) {
            return DoubleClick != null && DoubleClick(pt);
        }

        protected virtual void OnDragBegin() {
            fDragging = true;
        }

        protected virtual void OnDragEnd() {
            if(subDirTip != null) {
                subDirTip.HideMenu();
            }
            timer_HoverSubDirTipMenu.Enabled = false;
            RefreshSubDirTip(true);
            fDragging = false;
        }

        protected virtual void OnDragOver(Point pt) {
            timer_HoverSubDirTipMenu.Enabled = false;
            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(Control.ModifierKeys == Keys.Shift) {
                        timer_HoverSubDirTipMenu_Tick(null, null);
                    }
                }
                else {
                    timer_HoverSubDirTipMenu.Enabled = true;    
                }
            }
        }

        protected void OnEndLabelEdit(LVITEM item) {
            if(EndLabelEdit != null) {
                EndLabelEdit(item);
            }
        }

        protected bool OnGetInfoTip(int iItem, bool byKey) {
            if(Config.Tips.ShowTooltipPreviews && (!Config.Tips.ShowPreviewsWithShift ^ (Control.ModifierKeys == Keys.Shift))) {
                if(((thumbnailTooltip != null) && thumbnailTooltip.IsShowing) && (iItem == thumbnailIndex)) {
                    return true;
                }
                else if(byKey) {
                    Rectangle rect = GetFocusedItemRect();
                    Point pt = new Point(rect.Right - 32, rect.Bottom - 16);
                    PInvoke.ClientToScreen(Handle, ref pt);
                    return ShowThumbnailTooltip(iItem, pt, true);
                }
                else {
                    return ShowThumbnailTooltip(iItem, Control.MousePosition, false);
                }
            }
            return false;
        }

        protected void OnHotItemChanged(int iItem) {
            Keys modifierKeys = Control.ModifierKeys;
            if(Config.Tips.ShowTooltipPreviews) {
                if((thumbnailTooltip != null) && (thumbnailTooltip.IsShowing || fThumbnailPending)) {
                    if(!Config.Tips.ShowPreviewsWithShift ^ (modifierKeys == Keys.Shift)) {
                        if(iItem != thumbnailIndex) {
                            if(iItem > -1 && IsTrackingItemName()) {
                                if(ShowThumbnailTooltip(iItem, Control.MousePosition, false)) {
                                    return;
                                }
                            }
                            if(thumbnailTooltip.HideToolTip()) {
                                thumbnailIndex = -1;
                            }
                        }
                    }
                    else if(thumbnailTooltip.HideToolTip()) {
                        thumbnailIndex = -1;
                    }
                }
            }
            RefreshSubDirTip();
            
            return;
        }

        protected bool OnSelectionActivated(Keys modKeys) {
            return SelectionActivated != null && SelectionActivated(modKeys);
        }

        protected void OnItemCountChanged() {
            if (ItemCountChanged != null && ShellBrowser != null)
            {
                ItemCountChanged(ShellBrowser.GetItemCount());
            }
        }

        protected bool OnKeyDown(Keys key) {
            if(Config.Tips.ShowTooltipPreviews) {
                if(Config.Tips.ShowPreviewsWithShift) {
                    if(key != Keys.ShiftKey) {
                        HideThumbnailTooltip(2);
                    }
                }
                else {
                    HideThumbnailTooltip(2);
                }
            }
            if(Config.Tips.ShowSubDirTips) {
                if(Config.Tips.SubDirTipsWithShift) {
                    if(key != Keys.ShiftKey) {
                        HideSubDirTip(3);
                    }
                }
                else if(key != Keys.ControlKey) {
                    HideSubDirTip(3);
                }
            }

            if(Config.Tweaks.WrapArrowKeySelection && Control.ModifierKeys == Keys.None) {
                if(key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down) {
                    return HandleCursorLoop(key);
                }
            }
            
            return false;
        }

        protected bool OnMiddleClick(Point pt) {
            return MiddleClick != null && MiddleClick(pt);
        }

        protected bool OnMouseActivate(ref int result) {
            return MouseActivate != null && MouseActivate(ref result);
        }

        protected void OnSelectionChanged(ref Message msg/*object sender, SelectionChangedEventArgs e*/)
        {
            QTUtility2.log("OnSelectionChanged");
            if(SelectionChanged != null) {
                SelectionChanged(/*sender, e*/);
            }
        }

        protected virtual bool OnShellViewNotify(NMHDR nmhdr, ref Message msg) {
            if(nmhdr.hwndFrom != ListViewController.Handle) {
                if(nmhdr.code == -12 /*NM_CUSTOMDRAW*/ && nmhdr.idFrom == IntPtr.Zero) {
                    ResetTrackMouseEvent();
                }
            }
            return false;
        }

        public abstract override bool PointIsBackground(Point pt, bool screenCoords); 

        public override void RefreshSubDirTip(bool force = false) {
            if(fDragging) {
                OnDragOver(Control.MousePosition);
            }
            else if(Config.Tips.ShowSubDirTips && Control.MouseButtons == MouseButtons.None) {
                if((!Config.Tips.SubDirTipsWithShift ^ (Control.ModifierKeys == Keys.Shift)) && hwndExplorer == PInvoke.GetForegroundWindow()) {
                    int iItem = GetHotItem();
                    if(subDirTip != null && (subDirTip.MouseIsOnThis() || subDirTip.MenuIsShowing)) {
                        return;
                    }
                    if(!force && subDirIndex == iItem && (!QTUtility.IsXP || (iItem != -1))) {
                        return;
                    }
                    if(!QTUtility.IsXP) {
                        subDirIndex = iItem;
                    }
                    if(iItem > -1 && ShowSubDirTip(iItem, false, false)) {
                        if(QTUtility.IsXP) {
                            subDirIndex = iItem;
                        }
                        return;
                    }
                }
                HideSubDirTip(2);
                subDirIndex = -1;
            }
        }

        public void RemoteDispose() {
            PInvoke.PostMessage(Handle, WM_REMOTEDISPOSE, IntPtr.Zero, IntPtr.Zero);
        }

        private void ResetTrackMouseEvent() {
            if(fTrackMouseEvent) {
                fTrackMouseEvent = false;
                TRACKMOUSEEVENT structure = new TRACKMOUSEEVENT();
                structure.cbSize = Marshal.SizeOf(structure);
                structure.dwFlags = 2;
                structure.hwndTrack = Handle;
                PInvoke.TrackMouseEvent(ref structure);
            }
        }

        public override void ScrollHorizontal(int amount) {
            if(ListViewController != null) {
                // We'll intercept this message later for the ItemsView.  It's
                // important to use PostMessage here to prevent reentry issues
                // with the Automation Thread.
                PInvoke.PostMessage(ListViewController.Handle, LVM.SCROLL, (IntPtr)(-amount), IntPtr.Zero);
            }
        }

        public override void SetFocus() {
            if(ListViewController != null) {
                PInvoke.SetFocus(ListViewController.Handle);
            }
        }

        public override void SetRedraw(bool redraw) {
            if(ListViewController != null) {
                PInvoke.SetRedraw(ListViewController.Handle, redraw);
            }
        }

        protected virtual bool ShellViewController_MessageCaptured(ref Message msg) {
            // QTUtility2.debugMessage(msg);
            switch(msg.Msg) {
                case WM.MOUSEACTIVATE:
                    int res = (int)msg.Result;
                    bool ret = OnMouseActivate(ref res);
                    msg.Result = (IntPtr)res;
                    return ret;

                case WM.NOTIFY:
                    NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                    return OnShellViewNotify(nmhdr, ref msg);
            }
            return false;
        }

        public override void ShowAndClickSubDirTip() {
            try {
                Address[] addressArray;
                string str;
                if(ShellBrowser.TryGetSelection(out addressArray, out str, false) && ((addressArray.Length == 1) && !string.IsNullOrEmpty(addressArray[0].Path))) {
                    string path = addressArray[0].Path;
                    if(!path.StartsWith("::") && !Directory.Exists(path)) {
                        if(!Path.GetExtension(path).PathEquals(".lnk")) {
                            return;
                        }
                        path = ShellMethods.GetLinkTargetPath(path);
                        if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || QTUtility.IsNetPath(path)) // add by indiff
                        {
                            return;
                        }
                    }

                    if(subDirTip == null) {
                        subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, true, this);
                        subDirTip.MenuClosed += subDirTip_MenuClosed;
                        subDirTip.MenuItemClicked += subDirTip_MenuItemClicked;
                        subDirTip.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                        subDirTip.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                        subDirTip.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                    }

                    int iItem = ShellBrowser.GetFocusedIndex();
                    if(iItem != -1) {
                        ShowSubDirTip(iItem, true, false);
                        subDirTip.PerformClickByKey();
                    }
                }
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception, "ExtendedListViewCommon ShowAndClickSubDirTip");
            }
        }

        private bool ShowSubDirTip(int iItem, bool fByKey, bool fSkipForegroundCheck) {
            string str;
            if((fSkipForegroundCheck || (hwndExplorer == PInvoke.GetForegroundWindow())) && ShellBrowser.TryGetHotTrackPath(iItem, out str)) {
                bool flag = false;
                try {
                    if(!ShellMethods.TryMakeSubDirTipPath(ref str)) {
                        return false;
                    }

                    if (QTUtility.IsNetPath(str))
                    {
                        return false;
                    }
                    Point pnt = GetSubDirTipPoint(fByKey);
                    if(subDirTip == null) {
                        subDirTip = new SubDirTipForm(hwndSubDirTipMessageReflect, true, this);
                        subDirTip.MenuClosed += subDirTip_MenuClosed;
                        subDirTip.MenuItemClicked += subDirTip_MenuItemClicked;
                        subDirTip.MultipleMenuItemsClicked += subDirTip_MultipleMenuItemsClicked;
                        subDirTip.MenuItemRightClicked += subDirTip_MenuItemRightClicked;
                        subDirTip.MultipleMenuItemsRightClicked += subDirTip_MultipleMenuItemsRightClicked;
                        if(dropTargetPassthrough != null) {
                            PInvoke.RegisterDragDrop(subDirTip.Handle, dropTargetPassthrough);
                        }
                    }
                    subDirTip.ShowSubDirTip(str, null, pnt);
                    flag = true;
                }
                catch (Exception exception)
                {
                    QTUtility2.MakeErrorLog(exception, "ExtendedListViewCommon ShowSubDirTip");
                }
                return flag;
            }
            return false;
        }

        private bool ShowThumbnailTooltip(int iItem, Point pnt, bool fKey) {
            string linkTargetPath;
            if (ShellBrowser == null) // 导致空指针问题 by indiff
            {
                return false;
            }
            if(ShellBrowser.TryGetHotTrackPath(iItem, out linkTargetPath)) {
                if((linkTargetPath.StartsWith("::") ||
                    linkTargetPath.StartsWith(@"\\")) ||
                    linkTargetPath.ToLower().StartsWith(@"a:\")) {
                    return false;
                }
                string ext = Path.GetExtension(linkTargetPath).ToLower();
                if(ext == ".lnk") {
                    linkTargetPath = ShellMethods.GetLinkTargetPath(linkTargetPath);
                    if(linkTargetPath.Length == 0) {
                        return false;
                    }
                    ext = Path.GetExtension(linkTargetPath).ToLower();
                }
                if(ThumbnailTooltipForm.ExtIsSupported(ext)) {
                    if(thumbnailTooltip == null) {
                        thumbnailTooltip = new ThumbnailTooltipForm();
                        thumbnailTooltip.ThumbnailVisibleChanged += thumbnailTooltip_ThumbnailVisibleChanged;
                        timer_Thumbnail = new Timer();
                        timer_Thumbnail.Interval = 400;
                        timer_Thumbnail.Tick += timer_Thumbnail_Tick;
                    }
                    if(thumbnailTooltip.IsShownByKey && !fKey) {
                        thumbnailTooltip.IsShownByKey = false;
                        return true;
                    }
                    thumbnailIndex = iItem;
                    thumbnailTooltip.IsShownByKey = fKey;
                    return thumbnailTooltip.ShowToolTip(linkTargetPath, pnt);
                }
                HideThumbnailTooltip(6);
            }
            return false;
        }

        public override bool SubDirTipMenuIsShowing() {
            return subDirTip != null && subDirTip.MenuIsShowing;
        }

        private void subDirTip_MenuClosed(object sender, EventArgs e) {
            if(SubDirTip_MenuClosed != null) {
                SubDirTip_MenuClosed(sender, e);
            }
        }

        private void subDirTip_MenuItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(SubDirTip_MenuItemClicked != null) {
                SubDirTip_MenuItemClicked(sender, e);
            }
        }

        private void subDirTip_MenuItemRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(SubDirTip_MenuItemRightClicked != null) {
                SubDirTip_MenuItemRightClicked(sender, e);
            }
        }

        private void subDirTip_MultipleMenuItemsClicked(object sender, EventArgs e) {
            if(SubDirTip_MultipleMenuItemsClicked != null) {
                SubDirTip_MultipleMenuItemsClicked(sender, e);
            }
        }

        private void subDirTip_MultipleMenuItemsRightClicked(object sender, ItemRightClickedEventArgs e) {
            if(SubDirTip_MultipleMenuItemsRightClicked != null) {
                SubDirTip_MultipleMenuItemsRightClicked(sender, e);
            }
        }

        private void thumbnailTooltip_ThumbnailVisibleChanged(object sender, QEventArgs e) {
            timer_Thumbnail.Enabled = false;
            if(e.Direction == ArrowDirection.Up) {
                fThumbnailPending = false;
            }
            else {
                fThumbnailPending = true;
                timer_Thumbnail.Enabled = true;
            }
        }

        private void timer_HoverSubDirTipMenu_Tick(object sender, EventArgs e) {
            timer_HoverSubDirTipMenu.Enabled = false;
            if(Control.MouseButtons != MouseButtons.None && !(subDirTip != null && subDirTip.IsMouseOnMenus)) {
                int iItem = GetHotItem();
                if(iItem == subDirIndex) {
                    return;
                }
                if(subDirTip != null) {
                    subDirTip.HideMenu();
                }
                // TODO: Check if the item is the Recycle Bin and deny if it is.
                // string.Equals(wrapper.Path, "::{645FF040-5081-101B-9F08-00AA002F954E}"
                if(ShowSubDirTip(iItem, false, true)) {
                    subDirIndex = iItem;
                    if(hwndExplorer != IntPtr.Zero) {
                        WindowUtils.BringExplorerToFront(hwndExplorer);
                    }
                    PInvoke.SetFocus(ListViewController.Handle);
                    PInvoke.SetForegroundWindow(ListViewController.Handle);
                    HideThumbnailTooltip();
                    subDirTip.ShowMenu();
                    return;
                }
            }
            if(subDirTip != null && !subDirTip.IsMouseOnMenus) {
                HideSubDirTip(10);
            }
        }

        private void timer_Thumbnail_Tick(object sender, EventArgs e) {
            timer_Thumbnail.Enabled = false;
            fThumbnailPending = false;
        }

        private class DropTargetPassthrough : _IDropTarget, IDisposable {
            private IntPtr passthrough;
            private ExtendedListViewCommon parent;
            private bool fDraggingOnListView;
            private Point pointLastDrag = new Point(0, 0);

            public DropTargetPassthrough(IntPtr passthrough, ExtendedListViewCommon parent) {
                this.passthrough = passthrough;
                Marshal.AddRef(passthrough);
                this.parent = parent;
                Pointer = Marshal.GetComInterfaceForObject(this, typeof(_IDropTarget));
            }

            public IntPtr Pointer { get; private set; }

            public int DragEnter(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
                fDraggingOnListView = parent.MouseIsOverListView();
                if(fDraggingOnListView) {
                    parent.OnDragBegin();
                }
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragEnter(pDataObj, grfKeyState, pt, ref pdwEffect);
                }
            }

            public int DragOver(int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
                if(pt != pointLastDrag) {
                    pointLastDrag = pt;
                    parent.OnDragOver(pt);
                }
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragOver(grfKeyState, pt, ref pdwEffect);
                }
            }

            public int DragLeave() {
                if(parent.subDirTip != null && !parent.subDirTip.IsMouseOnMenus) {
                    if((fDraggingOnListView && !parent.subDirTip.MouseIsOnThis())
                            || (!fDraggingOnListView && !parent.MouseIsOverListView())) {
                        parent.OnDragEnd();
                    }
                }
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragLeave();
                }
            }

            public int DragDrop(IDataObject pDataObj, int grfKeyState, Point pt, ref DragDropEffects pdwEffect) {
                parent.OnDragEnd();
                using(DTWrapper wrapper = new DTWrapper(passthrough)) {
                    return wrapper.DropTarget.DragDrop(pDataObj, grfKeyState, pt, ref pdwEffect);
                }
            }




            public void Dispose() {
                if(passthrough != IntPtr.Zero) {
                    Marshal.Release(passthrough);
                    passthrough = IntPtr.Zero;
                }
                if(Pointer != IntPtr.Zero) {
                    Marshal.Release(Pointer);
                    Pointer = IntPtr.Zero;
                }
            }

            private class DTWrapper : IDisposable {
                public DTWrapper(IntPtr pDropTarget) {
                    DropTarget = (_IDropTarget)Marshal.GetObjectForIUnknown(pDropTarget);
                }

                public _IDropTarget DropTarget { get; private set; }

                public void Dispose() {
                    if(DropTarget != null) {
                        Marshal.ReleaseComObject(DropTarget);
                        DropTarget = null;
                    }
                }
            }
        }
    }
}

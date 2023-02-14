//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2002-2022  Pavel Zolnikov, Quizo, Paul Accisano, indiff
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SHDocVw;


namespace BandObjectLib {
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/shell/band-objects
    /// https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/cc144099(v=vs.85)?redirectedfrom=MSDN
    /// https://docs.microsoft.com/zh-cn/cpp/mfc/rebar-controls-and-bands?view=msvc-160
    /// </summary>
    public class BandObject : 
        UserControl, 
        IDeskBand, 
        IDockingWindow, 
        IInputObject, 
        IObjectWithSite, 
        IOleWindow,
        IPersistStream
        // , IDpiAwareObject
    {
        /***
         *
         *
         *    // 垂直资源管理器栏	CATID_InfoBand
            // 水平资源管理器栏	CATID_CommBand
            // 桌带	CATID_DeskBand
         *
         *
         */
        private Size minSize = new Size(16, 26);
        private Size maxSize = new Size(-1, -1);

        protected IInputObjectSite BandObjectSite;
        protected WebBrowserClass Explorer;
        protected bool fClosedDW;
        protected bool fFinalRelease;
        protected IntPtr ReBarHandle;
        private RebarBreakFixer RebarSubclass;
        private IAsyncResult result;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        protected const int S_OK = 0;
        protected const int S_FALSE = 1;
        protected const int E_NOTIMPL = -2147467263;	// _HRESULT_TYPEDEF_(0x80004001L)
        protected const int E_FAIL = -2147467259;    // _HRESULT_TYPEDEF_(0x80004005L)

        public static string ProcessName = Process.GetCurrentProcess().ProcessName;

        internal static bool HostedNotByExplorer = (ProcessName != "explorer");

        // 判断是否启用日志，发布改为false， 调试启用. 默认是关闭的，在常规选项里面可以设置启用
        // public static bool ENABLE_LOGGER = true;

        // We must subclass the rebar in order to fix a certain bug in 
        // Windows 7.
        internal sealed class RebarBreakFixer : NativeWindow {
            private readonly BandObject parent;
            public bool MonitorSetInfo { get; set; }
            public bool Enabled { get; set; }

            public RebarBreakFixer(IntPtr hwnd, BandObject parent) {
                this.parent = parent;
                Enabled = true;
                MonitorSetInfo = true;
                AssignHandle(hwnd);
            }

            protected override void WndProc(ref Message m) {
                // bandLog("WndProc");
                if(!Enabled) {
                    base.WndProc(ref m);
                    return;
                }

                // When the bars are first loaded, they will always have 
                // RBBS_BREAK set.  Catch RB_SETBANDINFO to fix this.
                if(m.Msg == RB.SETBANDINFO) {
                    if(MonitorSetInfo) {
                        Util2.bandLog("msg SETBANDINFO");
                        REBARBANDINFO pInfo = (REBARBANDINFO)Marshal.PtrToStructure(m.LParam, typeof(REBARBANDINFO));
                        if(pInfo.hwndChild == parent.Handle && (pInfo.fMask & RBBIM.STYLE) != 0) {
                            // Ask the bar if we actually want a break.
                            if(parent.ShouldHaveBreak()) {
                                pInfo.fStyle |= RBBS.BREAK;
                            }
                            else {
                                pInfo.fStyle &= ~RBBS.BREAK;
                            }
                            Marshal.StructureToPtr(pInfo, m.LParam, false);
                        }
                    }
                }
                // Whenever a band is deleted, the RBBS_BREAKs come back!
                // Catch RB_DELETEBAND to fix it.
                else if(m.Msg == RB.DELETEBAND) {
                    Util2.bandLog("msg DELETEBAND");
                    int del = (int)m.WParam;
                    
                    // Look for our band
                    int n = parent.ActiveRebarCount();
                    for(int i = 0; i < n; ++i) {
                        REBARBANDINFO info = parent.GetRebarBand(i, RBBIM.STYLE | RBBIM.CHILD);
                        if(info.hwndChild == parent.Handle) {
                            // Call the WndProc to let the deletion fall 
                            // through, with the break status safely saved
                            // in the info variable.
                            base.WndProc(ref m);

                            // If *we're* the one being deleted, no need to do
                            // anything else.
                            if(i == del) {
                                return;
                            }
                                
                            // Otherwise, our style has been messed with.
                            // Set it back to what it was.
                            info.cbSize = Marshal.SizeOf(info);
                            info.fMask = RBBIM.STYLE;
                            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
                            Marshal.StructureToPtr(info, ptr, false);
                            bool reset = MonitorSetInfo;
                            MonitorSetInfo = false;
                            SendMessage(parent.ReBarHandle, RB.SETBANDINFO, (IntPtr)i, ptr);
                            MonitorSetInfo = reset;
                            Marshal.FreeHGlobal(ptr);

                            // Return without calling WndProc twice!
                            return;
                        }
                    }
                }
                base.WndProc(ref m);
            }
        }

        private int ActiveRebarCount() {
            return (int)SendMessage(ReBarHandle, RB.GETBANDCOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        // Determines if the DeskBand is preceded by a break.
        protected bool BandHasBreak() {
            int n = ActiveRebarCount();
            for(int i = 0; i < n; ++i) {
                REBARBANDINFO info = GetRebarBand(i, RBBIM.STYLE | RBBIM.CHILD);
                if(info.hwndChild == Handle) {
                    return (info.fStyle & RBBS.BREAK) != 0;
                }
            }
            return true;
        }
        // virtual 关键字用于修改方法、属性、索引器或事件声明，并使它们可以在派生类中被重写。
        public virtual void CloseDW(uint dwReserved) {
            Util2.bandLog("CloseDW");
            fClosedDW = true;
            ShowDW(false);
            Dispose(true);
            if(Explorer != null) {
                // Util2.bandLog("ReleaseComObject Explorer");
                Marshal.ReleaseComObject(Explorer);
                Explorer = null;
            }
            if(BandObjectSite != null) {
                Marshal.ReleaseComObject(BandObjectSite);
                BandObjectSite = null;
            }
            if(RebarSubclass != null) {
                RebarSubclass.Enabled = false;
                RebarSubclass = null;
            }
        }

        public virtual void ContextSensitiveHelp(bool fEnterMode) {
        }




        private int bandID;
        private bool fVertical;

        public int BandID
        {
            get
            {
                return this.bandID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwBandID">容器分配的带的标识符。 如果需要，带区对象可以保留此值。</param>
        /// <param name="dwViewMode">带对象的视图模式。 以下值之一：</param>
        /// DBIF_VIEWMODE_NORMAL
        /// 带状对象正在水平带中显示。
        ///
        /// DBIF_VIEWMODE_VERTICAL
        ///     带状对象显示在垂直带中。
        ///
        /// DBIF_VIEWMODE_FLOATING
        ///     带状对象正在浮动带中显示。
        ///
        /// DBIF_VIEWMODE_TRANSPARENT
        ///     带状对象正在透明带中显示。
        /// <param name="pdbi">带对象的信息</param>
        /// ptMinSize
        /// 
        /// 类型： POINTL
        /// 
        /// 接收带对象的最小大小的 POINTL 结构。 最小宽度在 POINTL 结构的 x 成员中给出，最小高度在 y 成员中给出。
        /// 
        /// ptMaxSize
        /// 
        /// 类型： POINTL
        /// 
        /// 一个 POINTL 结构，它接收带对象的最大大小。 POINTL 结构的 y 成员中提供了最大高度，x 成员将被忽略。 如果带对象对其最大高度没有限制，应使用 (LONG) -1。
        /// 
        /// ptIntegral
        /// 
        /// 类型： POINTL
        /// 
        /// 一个 POINTL 结构，它接收大小调整步骤值 (递增) ，其中调整了带对象的大小。 垂直步骤值在 POINTL 结构的 y 成员中 给出，x 成员将被忽略。
        /// 
        /// dwModeFlags 成员必须包含 DBIMF_VARIABLEHEIGHT 标志;否则，将忽略 ptIntegral。
        /// 
        /// ptActual
        /// 
        /// 类型： POINTL
        /// 
        /// 接收带对象的理想大小的 POINTL 结构。 理想宽度在 POINTL 结构的 x 成员中给出，理想高度在 y 成员中给出。 带容器尝试使用这些值，但无法保证波段大小为此大小。
        /// 
        /// wszTitle[256]
        /// 
        /// 类型： WCHAR[256]
        /// 
        /// 接收带标题的 WCHAR 缓冲区。
        /// 
        /// dwModeFlags
        /// 
        /// 类型：DWORD
        /// 
        /// 一个 值，该值接收一组指定 band 对象的操作模式的标志。 以下一个或多个值：
        /// 
        /// DBIMF_NORMAL
        /// 带使用默认属性。 其他模式标志修改此标志。
        /// 
        /// DBIMF_FIXED
        /// Windows XP 及更高版本： 带对象的大小和位置固定。 使用此标志时，不会在带对象上显示大小调整手柄。
        /// 
        /// DBIMF_FIXEDBMP
        /// Windows XP 及更高版本： band 对象使用固定位图 (.bmp) 文件作为其背景。 请注意，并非所有情况下都支持背景，因此即使设置了此标志，也可能无法看到位图。
        /// 
        /// DBIMF_VARIABLEHEIGHT
        /// 可以更改带对象的高度。 ptIntegral 成员定义可重设带对象大小的步骤值。
        /// 
        /// DBIMF_UNDELETEABLE
        /// Windows XP 及更高版本： 无法从乐队容器中删除 band 对象。
        /// 
        /// DBIMF_DEBOSSED
        /// 带对象以凹陷的外观显示。
        /// 
        /// DBIMF_BKCOLOR
        /// 使用 crBkgnd 中指定的背景色显示带。
        /// 
        /// DBIMF_USECHEVRON
        /// Windows XP 及更高版本： 如果无法显示 (即，带对象小于 ptActual，则会显示 V 形，以指示有更多的可用选项。 单击 V 形时会显示这些选项。
        /// 
        /// DBIMF_BREAK
        /// Windows XP 及更高版本： 乐队对象显示在带容器中的新行中。
        /// 
        /// DBIMF_ADDTOFRONT
        /// Windows XP 及更高版本： band 对象是乐队容器中的第一个对象。
        /// 
        /// DBIMF_TOPALIGN
        /// Windows XP 及更高版本： band 对象显示在乐队容器的顶部行中。
        /// 
        /// DBIMF_NOGRIPPER
        /// Windows Vista 及更高版本： 不会显示大小调整手柄，以允许用户移动或调整带对象的大小。
        /// 
        /// DBIMF_ALWAYSGRIPPER
        /// Windows Vista 及更高版本： 始终显示允许用户移动乐队对象或调整其大小的大小手柄，即使该带对象是容器中唯一的一个。
        /// 
        /// DBIMF_NOMARGINS
        /// Windows Vista 及更高版本： 带对象不应显示边距。
        /// 
        /// crBkgnd
        /// 
        /// 类型： COLORREF
        /// 
        /// 接收带的背景色的 COLORREF 结构。 dwModeFlags 成员必须包含 DBIMF_BKCOLOR 标志;否则，将忽略 crBkgnd。
        public virtual void GetBandInfo(uint dwBandID, uint dwViewMode, ref DESKBANDINFO pdbi) {
            this.bandID = (int) dwBandID;
            this.fVertical = dwViewMode == 1U;

            if((pdbi.dwMask & DBIM.ACTUAL) != 0) {
                pdbi.ptActual.X = Size.Width;
                pdbi.ptActual.Y = Size.Height;
            }
            if((pdbi.dwMask & DBIM.INTEGRAL) != 0) {
                pdbi.ptIntegral.X = -1;
                pdbi.ptIntegral.Y = -1;
            }
            if((pdbi.dwMask & DBIM.MAXSIZE) != 0) {
                pdbi.ptMaxSize.X = pdbi.ptMaxSize.Y = -1;
            }
            if((pdbi.dwMask & DBIM.MINSIZE) != 0) {
                pdbi.ptMinSize.X = MinSize.Width;
                pdbi.ptMinSize.Y = MinSize.Height;
            }
            if((pdbi.dwMask & DBIM.MODEFLAGS) != 0) {
                pdbi.dwModeFlags = DBIMF.NORMAL;
            }
            if((pdbi.dwMask & DBIM.BKCOLOR) != 0) {
                pdbi.dwMask &= ~DBIM.BKCOLOR;
            }
            if((pdbi.dwMask & DBIM.TITLE) != 0) {
                pdbi.wszTitle = null;
            }
        }

        private REBARBANDINFO GetRebarBand(int idx, int fMask) {
            Util2.bandLog("GetRebarBand");
            REBARBANDINFO info = new REBARBANDINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.fMask = fMask;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
            Marshal.StructureToPtr(info, ptr, false);
            SendMessage(ReBarHandle, RB.GETBANDINFO, (IntPtr)idx, ptr);
            info = (REBARBANDINFO)Marshal.PtrToStructure(ptr, typeof(REBARBANDINFO));
            Marshal.FreeHGlobal(ptr);
            return info;
        }

        public virtual int GetSite(ref Guid riid, out object ppvSite) {
            try
            {
                if (this.BandObjectSite != null)
                {
                    ppvSite = BandObjectSite;
                    // ppvSite = Marshal.GetIUnknownForObject((object)this.BandObjectSite);
                    return 0;
                }
            }
            catch 
            {
            }
            // ppvSite = IntPtr.Zero;
            ppvSite = null;
            return -2147467259;
        }

        public virtual void GetWindow(out IntPtr phwnd) {
            if (BandObject.HostedNotByExplorer)
            {
                phwnd = IntPtr.Zero;
                return ;
            }
            phwnd = Handle;
        }

        public virtual int HasFocusIO() {
            if(!ContainsFocus) {
                return 1;
            }
            return 0;
        }

        protected virtual void OnExplorerAttached() {
            Util2.bandLog("BandObject");
        }

        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            if((!fClosedDW && (BandObjectSite != null)) && IsHandleCreated) {
                Util2.bandLog("OnGotFocus");
                BandObjectSite.OnFocusChangeIS(this, 1);
            }
        }

        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            if((!fClosedDW && (BandObjectSite != null)) && (ActiveControl == null)) {
                Util2.bandLog("OnLostFocus");
                BandObjectSite.OnFocusChangeIS(this, 0);
            }
        }

        /// <summary>
        /// 通知停靠窗口对象框架的边框空间已更改。为了响应此方法，IDockingWindow 实现必须调用 SetBorderSpaceDW，即使不需要边框空间或不需要更改。
        /// </summary>
        /// <param name="prcBorder"></param>
        /// <param name="punkToolbarSite"></param>
        /// <param name="fReserved"></param>
        public virtual void ResizeBorderDW(IntPtr prcBorder, object punkToolbarSite, bool fReserved) {
        }

        // Override this to set whether the DeskBand has a break when it is 
        // first displayed
        protected virtual bool ShouldHaveBreak() {
            return true;
        }

        public virtual int SetSite(object pUnkSite)
        {
            /*if(Process.GetCurrentProcess().ProcessName == "iexplore") {
                Marshal.ThrowExceptionForHR(E_FAIL);
                Util2.bandLog("Marshal.ThrowExceptionForHR");

            }*/

            if (pUnkSite == null)
            {
                if (BandObjectSite != null )
                {
                    Marshal.ReleaseComObject(BandObjectSite);
                    Util2.bandLog("Marshal.ReleaseComObject BandObjectSite");
                    this.BandObjectSite = null;
                }
                if (Explorer != null)
                {
                    Marshal.ReleaseComObject(Explorer);
                    Explorer = null;
                    Util2.bandLog("Marshal.ReleaseComObject Explorer");
                }
            } else if (pUnkSite != null)
            {
                BandObjectSite = pUnkSite as IInputObjectSite;
                try {
                    object obj2;
                    ((_IServiceProvider)BandObjectSite).QueryService(
                        ExplorerGUIDs.IID_IWebBrowserApp, 
                        ExplorerGUIDs.IID_IUnknown, 
                        out obj2);
                    Util2.bandLog("BandObjectSite.QueryService");
                    Explorer = (WebBrowserClass)Marshal.CreateWrapperOfType(obj2 as IWebBrowser, typeof(WebBrowserClass));
                    Util2.bandLog("Marshal.CreateWrapperOfType");
                    OnExplorerAttached();
                    Util2.bandLog("OnExplorerAttached");
                }
                catch  (COMException exception) { // exception
                    Util2.MakeErrorLog(exception, "QueryService CreateWrapperOfType");
                }
            }
            try {
                IOleWindow window = pUnkSite as IOleWindow;
                if(window != null) {
                    window.GetWindow(out ReBarHandle);
                }
            }
            catch (Exception e) // exc
            {
                Util2.MakeErrorLog(e, "BandObject SetSite");
               //  logger.Log(exc);
            }
            return 0;
        }

        public virtual void ShowDW(bool fShow) {
            if(ReBarHandle != IntPtr.Zero && Environment.OSVersion.Version.Major > 5) {
                if(RebarSubclass == null) {
                    RebarSubclass = new RebarBreakFixer(ReBarHandle, this);
                }

                RebarSubclass.MonitorSetInfo = true;
                if(result == null || result.IsCompleted) {    
                    result = BeginInvoke(new UnsetInfoDelegate(UnsetInfo));
                }
            }
            Visible = fShow;
        }

        public virtual int TranslateAcceleratorIO(ref MSG msg) {
            if(((msg.message == 0x100) && ((msg.wParam == ((IntPtr)9L)) || (msg.wParam == ((IntPtr)0x75L)))) && SelectNextControl(ActiveControl, (ModifierKeys & Keys.Shift) != Keys.Shift, true, false, false)) {
                return 0;
            }
            return 1;
        }

        public virtual void UIActivateIO(int fActivate, ref MSG msg) {
            if(fActivate != 0) {
                Control nextControl = GetNextControl(this, true);
                if(nextControl != null) {
                    nextControl.Select();
                }
                Focus();
            }
        }

        private delegate void UnsetInfoDelegate();

        private void UnsetInfo() {
            if(RebarSubclass != null) {
                RebarSubclass.MonitorSetInfo = false;
            }
        }

        public Size MinSize {
            get {
                return minSize;
            }
            set {
                minSize = value;
            }
        }

        protected Size MaxSize
        {
            get
            {
                return this.maxSize;
            }
            set
            {
                this.maxSize = value;
            }
        }

        public virtual void GetClassID(out Guid pClassID) {
            pClassID = Guid.Empty;
        }

        public virtual int IsDirty() {
            return 0;
        }

        public virtual void IPersistStreamLoad(object pStm) {
        }

        public virtual void Save(IntPtr pStm, bool fClearDirty) {
        }

        public virtual int GetSizeMax(out ulong pcbSize) {
            const int E_NOTIMPL = -2147467263;
            pcbSize = 0;
            return E_NOTIMPL;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BandObject
            // 
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "BandObject";
            this.ResumeLayout(false);
        }

        public int Dpi { get; private set; } 

        public float Scaling
        {
            get
            {
               return (float) this.Dpi / 96f;
            }
        } 

        /*
        public void NotifyDpiChanged(int oldDpi, int dpiNew)
        {
            Util2.bandLog("BandObject NotifyDpiChanged oldDpi " + oldDpi + " dpiNew " + dpiNew);
            this.Dpi = dpiNew;
            Action<Control> act = (Action<Control>) null;
            act = (Action<Control>)(
                control =>
                {
                    for (var i = 0; i < control.Controls.Count; i++)
                    {
                        var cc = (Control) control.Controls[i];

                        if (cc is IDpiAwareObject)
                            ((IDpiAwareObject)cc).NotifyDpiChanged(oldDpi, dpiNew);
                        act(cc);
                    }
                }
            );
            act((Control) this);
            this.OnDpiChanged(oldDpi, dpiNew);
        }*/


        protected virtual void OnDpiChanged(int oldDpi, int newDpi)
        {
        }

        /*#region 新增区域 by indiff
        public  void RefreshRebarBand()
        {
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
            SendMessage(ReBarHandle, 1052, (IntPtr)wParam, ptr);
            // PInvoke.SendMessage(this.Handle, RB.SETBANDINFOW, (void*)wParam, ref structure);
            lParam.cyChild = this.fVertical ? this.Width : this.Height;
            lParam.cyMinChild = this.fVertical ? this.Width : this.Height;
            PInvoke.SendMessage(this.ReBarHandle, 1035, (IntPtr)wParam, ptr);
            lParam = (REBARBANDINFO)Marshal.PtrToStructure(ptr, typeof(REBARBANDINFO));
            Marshal.FreeHGlobal(ptr);
        }
        #endregion*/

        /*protected override void OnPaintBackground(PaintEventArgs e)
        {

        }*/
    }

    internal class Util2
    {
        private const bool ENABLE_LOGGER = false;


        public static void bandLog(string optional)
        {
            if (ENABLE_LOGGER)
                bandLog("bandLog", optional);
        }

        

        public static void err(string optional)
        {
            if (ENABLE_LOGGER)
                bandLog("err", optional);

        }

        private static void writeStr(string path, StringBuilder formatLogLine)
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(formatLogLine);
            }
        }

        public static void bandLog(string level, string optional)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appdataQT = Path.Combine(appdata, "QTTabBar");
            if (!Directory.Exists(appdataQT))
            {
                Directory.CreateDirectory(appdataQT);
            }

            Process process = Process.GetCurrentProcess();
            var cThreadId = Thread.CurrentThread.ManagedThreadId;
            var currentThreadId = AppDomain.GetCurrentThreadId();

            string path = Path.Combine(appdataQT, "bandLog.log");
            var formatLogLine = new StringBuilder();
            formatLogLine
                .Append("[")
                .Append(level)
                .Append("]");
            if (process != null)
            {
                formatLogLine
                    .Append(" PID:")
                    .Append(process.Id);
            }
            if (cThreadId != null)
            {
                formatLogLine
                    .Append(" TID:")
                    .Append(cThreadId);
            }
            else if (currentThreadId != null)
            {
                formatLogLine
                    .Append(" TID:")
                    .Append(currentThreadId);
            }
            formatLogLine
                .Append(" ")
                .Append(DateTime.Now.ToString())
                .Append(" ")
                .Append(optional);
            writeStr(path, formatLogLine);
        }

        internal static void MakeErrorLog(Exception ex, string optional = null)
        {
            try
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appdataQT = Path.Combine(appdata, "QTTabBar");
                if (!Directory.Exists(appdataQT))
                {
                    Directory.CreateDirectory(appdataQT);
                }
                // string path = Path.Combine(appdataQT, "QTTabBarBandObject.bandLog");
                string path = Path.Combine(appdataQT, "QTTabBarBandObjectException.log");
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(DateTime.Now.ToString());
                    writer.WriteLine(".NET 版本: " + Environment.Version);
                    writer.WriteLine("操作系统版本: " + Environment.OSVersion.Version);
                    //writer.WriteLine("QT 版本: " + MakeVersionString());
                    if (!String.IsNullOrEmpty(optional))
                    {
                        writer.WriteLine("错误信息: " + optional);
                    }
                    if (ex == null)
                    {
                        writer.WriteLine("Exception: None");
                        writer.WriteLine(Environment.StackTrace);
                    }
                    else
                    {
                        // writer.WriteLine(ex.ToString());

                        writer.WriteLine("\nMessage ---\n{0}", ex.Message);
                        writer.WriteLine(
                            "\nHelpLink ---\n{0}", ex.HelpLink);
                        writer.WriteLine("\nSource ---\n{0}", ex.Source);
                        writer.WriteLine(
                            "\nStackTrace ---\n{0}", ex.StackTrace);
                        writer.WriteLine(
                            "\nTargetSite ---\n{0}", ex.TargetSite);


                    }
                    writer.WriteLine("--------------");
                    writer.WriteLine();
                    writer.Close();
                }
                // SystemSounds.Exclamation.Play();
            }
            catch
            {
            }
        }
    }
}

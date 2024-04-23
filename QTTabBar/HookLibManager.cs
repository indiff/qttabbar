//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2021  Quizo, Paul Accisano
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
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    public static class HookLibManager {
        private static bool fShellBrowserIsHooked;
        private static IntPtr hHookLib;
        private static int[] hookStatus = Enumerable.Repeat(-1, Enum.GetNames(typeof(Hooks)).Length).ToArray();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void HookLibCallback(int hookId, int retcode);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool NewWindowCallback(IntPtr pIDL);

        [StructLayout(LayoutKind.Sequential)]
        private struct CallbackStruct {
            public HookLibCallback cbHookResult;
            public NewWindowCallback cbNewWindow;
            // todo: NewTreeView should probably also go here.
            // Using PostThreadMessage has a small chance of causing a memory leak.
        }

        private static readonly CallbackStruct callbackStruct = new CallbackStruct() {
            cbHookResult = HookResult,
            cbNewWindow = NewWindow
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitShellBrowserHookDelegate(IntPtr shellBrowser);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitHookLibDelegate(CallbackStruct fpHookResult);

        public enum HookCheckPoint{
            Initial,
            ShellBrowser,
            NewWindow,
            Automation,
        }

        // Unmarked hooks exist only to set other hooks.
        private enum Hooks {
            CoCreateInstance = 0,           // Treeview Middle-click
            RegisterDragDrop,               // DragDrop into SubDirTips
            SHCreateShellFolderView,
            BrowseObject,                   // Control Panel dialog OK/Cancel buttons
            CreateViewWindow3,              // Header in all Views
            MessageSFVCB,                   // Refresh = clear text
            UiaReturnRawElementProvider,
            QueryInterface,                 // Scrolling Lag workaround
            TravelToEntry,                  // Clear Search bar = back
            OnActivateSelection,            // Recently activated files
            SetNavigationState,             // Breadcrumb Bar Middle-click
            ShowWindow,                     // New Explorer window capturing
            UpdateWindowList,               // Compatibility with SHOpenFolderAndSelectItems
            CreateWindowExW ,
            DestroyWindow,
            BeginPaint,
            FillRect,
            CreateCompatibleDC
        }

        /** Do not initialize hook.*/
        public static void Initialize_donot()
        {
            QTUtility2.log("Do not initialize hook" );
        }

        public static void Initialize_bgtool()
        {
            if (hHookLib != IntPtr.Zero) return;
            string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "QTTabBar");
            // string filename = IntPtr.Size == 8 ? "QTHookLib64.dll" : "QTHookLib32.dll";
            string filename =  "ExplorerBgTool.dll";
            hHookLib = PInvoke.LoadLibrary(Path.Combine(installPath, filename));
            int retcode = -1;
            if (hHookLib == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                QTUtility2.MakeErrorLog(null, "LoadLibrary error: " + error);
            }
            else
            {
                IntPtr pFunc = PInvoke.GetProcAddress(hHookLib, "OnWindowLoad");
                if (pFunc != IntPtr.Zero)
                {
                    InitHookLibDelegate initialize = (InitHookLibDelegate)
                        Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitHookLibDelegate));
                    try
                    {
                        retcode = initialize(callbackStruct);
                    }
                    catch (Exception e)
                    {
                        QTUtility2.MakeErrorLog(e, "");
                    }

                }
            }

            if (retcode == 0)
            {
                QTUtility2.log("HookLib Initialize success");
                return;
            }
            QTUtility2.MakeErrorLog(null, "HookLib Initialize failed: " + retcode);

            MessageForm.Show(IntPtr.Zero,
                String.Format(
                    "{0}: {1} {2}",
                    QTUtility.TextResourcesDic["ErrorDialogs"][4],
                    QTUtility.TextResourcesDic["ErrorDialogs"][5],
                    QTUtility.TextResourcesDic["ErrorDialogs"][7]
                ),
                QTUtility.TextResourcesDic["ErrorDialogs"][1],
                MessageBoxIcon.Hand,
                30000, false, true
            );
        }

        private static Boolean LoadedHook = false;

        public static void Initialize()
        {
            try
            {
                if (LoadedHook)
                {
                    return;
                }
                
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                string sCPUSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get())
                {

                    sCPUSerialNumber = mo["Name"].ToString().ToLower().Trim();//操作系统名字
                    //sCPUSerialNumber = mo["BootDevice"].ToString().Trim();//系统启动分区
                    //sCPUSerialNumber = mo["NumberOfProcesses"].ToString().Trim();//当前运行的进程数
                    //sCPUSerialNumber = mo["SerialNumber"].ToString().Trim();//操作系统序列号
                    //sCPUSerialNumber = mo["OSLanguage"].ToString().Trim();//操作系统的语言
                    //sCPUSerialNumber = mo["Manufacturer"].ToString().Trim();//
                }

                var isServer = sCPUSerialNumber.Contains("windows server");
                if (isServer)
                {
                    QTUtility2.log("can not hook in server by get server");
                    Config.Window.AutoHookWindow = false;
                    LoadedHook = false;
                    return;
                }
            }
            catch (Exception)
            {   
                QTUtility2.log("can not hook in server by get server exception");
                Config.Window.AutoHookWindow = false;
                LoadedHook = false;
                return;
            }

            string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "QTTabBar");
            string filename = IntPtr.Size == 8 ? "QTHookLib64.dll" : "QTHookLib32.dll";
            if (hHookLib != IntPtr.Zero)
            {
                   if (!Config.Window.AutoHookWindow)
                   {
                        PInvoke.FreeLibrary(hHookLib);
                        hHookLib = IntPtr.Zero;
                   }
                return;
            }

            if (!Config.Window.AutoHookWindow)
            {
                LoadedHook = false;
                return;
            }

            if (!File.Exists(Path.Combine(installPath, filename))) // 如果文件不存在则设置为不自动加载
            {
                QTUtility2.flog("not exists file , close auto hook " + Path.Combine(installPath, filename));
                Config.Window.AutoHookWindow = false;
                LoadedHook = false;
                return;
            }
            QTUtility2.flog("load library " + Path.Combine(installPath, filename) );
            hHookLib = PInvoke.LoadLibrary(Path.Combine(installPath, filename));
            QTUtility2.flog("load library hHookLib " + hHookLib);
            int retcode = -1;
            if(hHookLib == IntPtr.Zero) {
                int error = Marshal.GetLastWin32Error();
                QTUtility2.MakeErrorLog(null, "LoadLibrary error: " + error);
            }
            else {
                IntPtr pFunc = PInvoke.GetProcAddress(hHookLib, "Initialize");
                if(pFunc != IntPtr.Zero) {
                    InitHookLibDelegate initialize = (InitHookLibDelegate) 
                        Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitHookLibDelegate));
                    try {
                        retcode = initialize(callbackStruct);
                    }
                    catch(Exception e) {
                        QTUtility2.MakeErrorLog(e, "");
                        LoadedHook = false;
                    }
                }
            }

            if (retcode == 0)
            {
                LoadedHook = true;
                QTUtility2.log("HookLib Initialize success");
                // MessageBox.Show("HookLib Initialize success");
                return;
            }
            QTUtility2.MakeErrorLog(null, "HookLib Initialize failed: " + retcode);
            LoadedHook = false;
            MessageForm.Show(IntPtr.Zero,
                String.Format(
                    "{0}: {1} {2}",
                    QTUtility.TextResourcesDic["ErrorDialogs"][4],
                    QTUtility.TextResourcesDic["ErrorDialogs"][5],
                    QTUtility.TextResourcesDic["ErrorDialogs"][7]
                ),
                QTUtility.TextResourcesDic["ErrorDialogs"][1],
                MessageBoxIcon.Hand, 
                30000, false, true
            );
        }


        private static void HookResult(int hookId, int retcode) {
            lock(callbackStruct.cbHookResult) {
                if (hookId <= hookStatus.Length - 1)
                {
                    hookStatus[hookId] = retcode;
                }
            }
        }

        // We need to use a callback rather than a message for window capturing,
        // since the main instance could be in another process.
        private static bool NewWindow(IntPtr pIDL) {
            byte[] IDL;
            using(IDLWrapper wrapper = new IDLWrapper(PInvoke.ILClone(pIDL))) {
                if(!Config.Window.CaptureNewWindows
                        || InstanceManager.GetTotalInstanceCount() == 0
                        || QTUtility2.IsShellPathButNotFileSystem(wrapper.Path)
                        || wrapper.Path.PathEquals(QTUtility.PATH_SEARCHFOLDER)
                        || QTUtility.NoCapturePathsList.Any(path => wrapper.Path.PathEquals(path))
                        || (Control.ModifierKeys & Keys.Control) != Keys.None) {
                    return false;
                }
                IDL = wrapper.IDL;
            }
            InstanceManager.BeginInvokeMain(tabbar => {
                QTUtility2.log("BeginInvokeMain OpenNewTabOrWindow");
                using (IDLWrapper wrapper = new IDLWrapper(IDL)) {
                    tabbar.OpenNewTabOrWindow(wrapper, true);
                }
            });
            return true;
        }
        /** do not init shell brownser hook. */
        public static void InitShellBrowserHook_old(IShellBrowser shellBrowser) { }

        public static void InitShellBrowserHook(IShellBrowser shellBrowser)
        {
            lock (typeof(HookLibManager))
            {
                if(fShellBrowserIsHooked || hHookLib == IntPtr.Zero) return;
                IntPtr pFunc = PInvoke.GetProcAddress(hHookLib, "InitShellBrowserHook");
                if(pFunc == IntPtr.Zero) return;
                InitShellBrowserHookDelegate initShellBrowserHook = (InitShellBrowserHookDelegate)
                        Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitShellBrowserHookDelegate));
                IntPtr pShellBrowser = Marshal.GetComInterfaceForObject(shellBrowser, typeof(IShellBrowser));
                if(pShellBrowser == IntPtr.Zero) return;
                int retcode = -1;
                try {
                    retcode = initShellBrowserHook(pShellBrowser);
                }
                catch(Exception e) {
                    QTUtility2.MakeErrorLog(e, "");
                }
                finally {
                    Marshal.Release(pShellBrowser);
                }
                if(retcode != 0) {
                    QTUtility2.MakeErrorLog(null, "InitShellBrowserHook failed: " + retcode);

                    MessageForm.Show(IntPtr.Zero,
                        String.Format(
                            "{0}: {1} {2}",
                            QTUtility.TextResourcesDic["ErrorDialogs"][4],
                            QTUtility.TextResourcesDic["ErrorDialogs"][6],
                            QTUtility.TextResourcesDic["ErrorDialogs"][7]
                        ),
                        QTUtility.TextResourcesDic["ErrorDialogs"][1],
                        MessageBoxIcon.Hand, 30000, false, true
                    );
                }
                else {
                    fShellBrowserIsHooked = true;
                }
            }
        }

        public static void CheckHooks() {
            // TODO
        }
    }
}

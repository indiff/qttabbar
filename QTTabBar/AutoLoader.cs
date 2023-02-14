//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022 indiff  Quizo, Paul Accisano
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BandObjectLib;
using Microsoft.Win32;
using SHDocVw;

namespace QTTabBarLib {

    [Guid("D2BF470E-ED1C-487F-A777-2BD8835EB6CE"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class AutoLoader : IObjectWithSite {
        private IWebBrowser2 explorer;       
        private const string BHOKEYNAME = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects\";
        private const int E_FAIL = unchecked((int)0x80004005);

        [ComRegisterFunction]
        public static void Register(Type t) {
            string name = t.GUID.ToString("B");
            using(RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + name)) {
                key.SetValue(null, "QTTabBar AutoLoader");
                key.SetValue("MenuText", "QTTabBar AutoLoader");
                key.SetValue("HelpText", "QTTabBar AutoLoader");
            }
            Registry.LocalMachine.CreateSubKey(BHOKEYNAME + name);
            QTUtility2.flog( "AutoLoader 注册表 QTTabBar 自动加载(安装)");
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t) {
            using(RegistryKey key = Registry.LocalMachine.CreateSubKey(BHOKEYNAME)) {
                key.DeleteSubKey(t.GUID.ToString("B"), false);
            }
            QTUtility2.flog("AutoLoader 注册表 QTTabBar 自动加载(卸载)");
        }

        public int SetSite(object site) {
            // SetProcessDPIAware是Vista以上才有的函数，这样直接调用会使得程序不兼容XP
            // PInvoke.SetProcessDPIAware();
            // QTUtility2.log("QTUtility AutoLoader SetSite SetProcessDPIAware 不兼容XP");
            QTUtility2.log("SetSite");
            explorer = site as IWebBrowser2;
            // QTUtility2.flog("QTTabBar AutoLoader SetSite ");
            /*if(explorer == null || Process.GetCurrentProcess().ProcessName == "iexplore") {
                QTUtility2.log("QTTabBar AutoLoader SetSite Throw Exception ");
                // QTUtility2.flog("QTTabBar AutoLoader SetSite Throw Exception ");
                // 基于指定的 IErrorInfo 接口，用特定失败 HRESULT 引发异常
                Marshal.ThrowExceptionForHR(E_FAIL);
            }
            else {*/

            if (explorer != null && Process.GetCurrentProcess().ProcessName.ToLower() != "iexplore")
            {
                QTUtility2.log("QTTabBar AutoLoader SetSite ActivateIt ");
                // QTUtility2.flog("QTTabBar AutoLoader SetSite ActivateIt ");
                ActivateIt();
            }

            return 0;
        }

        public int GetSite(ref Guid guid, out object ppvSite) {
            ppvSite = explorer;
            return 0;
        }

        private void ActivateIt() {
            string installDateString;
            DateTime installDate;
            string minDate = DateTime.MinValue.ToString();
            using(RegistryKey key = Registry.LocalMachine.OpenSubKey(RegConst.Root)) {
                installDateString = key == null ? minDate : (string)key.GetValue("InstallDate", minDate);
                installDate = DateTime.Parse(installDateString);
            }
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                DateTime lastActivation = DateTime.Parse((string)key.GetValue("ActivationDate", minDate));
                if(installDate.CompareTo(lastActivation) <= 0) return;

                object secViewBar = new Guid("{d2bf470e-ed1c-487f-a333-2bd8835eb6ce}").ToString("B");
                object pvaTabBar = new Guid("{d2bf470e-ed1c-487f-a333-2bd8835eb6ce}").ToString("B");
                object pvaButtonBar = new Guid("{d2bf470e-ed1c-487f-a666-2bd8835eb6ce}").ToString("B");
                object pvarShow = true;
                object pvarSize = null;
                try {


                    explorer.ShowBrowserBar(pvaTabBar, pvarShow, pvarSize);
                    QTUtility2.log("QTTabBar AutoLoader 显示标签");
                    
                    explorer.ShowBrowserBar(pvaButtonBar, pvarShow, pvarSize);
                    QTUtility2.log("QTTabBar AutoLoader 显示工具栏");

                    explorer.ShowBrowserBar(secViewBar, pvarShow, pvarSize);
                    QTUtility2.log("QTTabBar AutoLoader 显示标签");
                }
                catch(COMException e) {
                    QTUtility2.MakeErrorLog(e, "ActivateIt");
                    MessageForm.Show(
                        IntPtr.Zero,
                        QTUtility.TextResourcesDic["ErrorDialogs"][2],
                        QTUtility.TextResourcesDic["ErrorDialogs"][3],
                        MessageBoxIcon.Warning, 
                        30000, 
                        false, 
                        true
                    );
                }

                key.SetValue("ActivationDate", installDateString);
                QTUtility2.flog("QTTabBar AutoLoader add ActivationDate");
            }
        }
    }
}
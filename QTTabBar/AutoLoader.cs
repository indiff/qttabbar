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
using QTTabBarLib.Interop;
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
            QTUtility2.flog( "AutoLoader registry: QTTabBar auto-load (install)");
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t) {
            using(RegistryKey key = Registry.LocalMachine.CreateSubKey(BHOKEYNAME)) {
                key.DeleteSubKey(t.GUID.ToString("B"), false);
            }
            QTUtility2.flog("AutoLoader registry: QTTabBar auto-load (uninstall)");
        }

        public int SetSite(object site) {
            // SetProcessDPIAware only exists on Vista and later - calling it directly would make the program incompatible with XP
            // PInvoke.SetProcessDPIAware();
            QTUtility2.log("SetSite");
            explorer = site as IWebBrowser2;
            // QTUtility2.flog("QTTabBar AutoLoader SetSite ");
            /*if(explorer == null || Process.GetCurrentProcess().ProcessName == "iexplore") {
                QTUtility2.log("QTTabBar AutoLoader SetSite Throw Exception ");
                // QTUtility2.flog("QTTabBar AutoLoader SetSite Throw Exception ");
                // Raise an exception with a specific failure HRESULT based on the given IErrorInfo interface
                Marshal.ThrowExceptionForHR(E_FAIL);
            }
            else {*/

            if (explorer != null && Process.GetCurrentProcess().ProcessName.ToLower() != "iexplore")
            {
                QTUtility2.log("QTTabBar AutoLoader SetSite ActivateIt ");
                // QTUtility2.flog("QTTabBar AutoLoader SetSite ActivateIt ");
                ActivateIt();

                // Normally the toolbar band triggers this on load. Without a band (e.g.
                // Explorer never hosts the toolbar), InstanceManager/Config/etc. are
                // never set up, so force it before checking the setting below.
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(QTUtility).TypeHandle);
                // Windows 10 still has a real toolbar rebar - users enable QTTabBar (and
                // separately QTButtonBar, if they only want one of the two) through
                // Explorer's own View > Toolbars menu there, same as any other classic
                // toolband. This auto-attach path never parents into a real rebar (see
                // ContextMenuOptions.AttachToWindow), so it only ever gives double-click
                // and hover preview, not tabs - worth running only where Windows 11's
                // lack of a rebar leaves no other way to get even that much.
                if (QTUtility.IsWin11 && Config.Window.AutoEnableExperimental) {
                    // SetSite fires far earlier in the window's life than the existing
                    // right-click "Enable QTTabBar" path ever did (that only ever ran on an
                    // already-open, already-visible window). Attaching here immediately once
                    // took down Explorer's ability to open further windows at all. Instead,
                    // poll (on this same thread's own message loop, via a WinForms Timer, so
                    // there's no cross-thread risk) until the window is actually visible
                    // before attaching, and give up quietly after a few tries rather than
                    // retrying forever if something's wrong.
                    IWebBrowser2 wb = explorer;
                    ActionDelayer.Add(() => {
                        try {
                            IntPtr hwnd = (IntPtr)wb.HWND;
                            if (hwnd == IntPtr.Zero || !PInvoke.IsWindowVisible(hwnd)) return false;
                            ContextMenuOptions.AttachToWindow(wb);
                        }
                        catch {
                            // Fall through - treat as "done trying", not "keep retrying".
                        }
                        return true;
                    }, 500, 500, 10);
                }
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
                    /*
                    explorer.ShowBrowserBar(pvaTabBar, pvarShow, pvarSize);
                    QTUtility2.log("QTTabBar AutoLoader 显示标签");
                    
                    explorer.ShowBrowserBar(pvaButtonBar, pvarShow, pvarSize);
                    QTUtility2.log("QTTabBar AutoLoader 显示工具栏");

                    explorer.ShowBrowserBar(secViewBar, pvarShow, pvarSize);*/
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
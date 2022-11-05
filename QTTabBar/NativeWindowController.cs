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
using System.Windows.Forms;

namespace QTTabBarLib {
    public sealed class NativeWindowController : NativeWindow {
        internal event MessageEventHandler MessageCaptured;
        private bool fNoMoreImmersiveColorSet;

        public NativeWindowController(IntPtr hwnd) {
            AssignHandle(hwnd);
        }

        protected override void WndProc(ref Message m) {
            bool consumed = false;
            if(MessageCaptured != null) {
                /*switch (m.Msg)
                {
                    case 26:
                        QTUtility2.log("NativeWindowController WndProc 26");
                        string str = string.Empty;
                        try
                        {
                            if (m.LParam != IntPtr.Zero)
                                str = Marshal.PtrToStringUni(m.LParam);
                        }
                        catch
                        {
                        }
                        if (!(str == "Environment"))
                        {
                            if (str == "ImmersiveColorSet")
                            {
                                if (!this.fNoMoreImmersiveColorSet)
                                {
                                    QTUtility.RefreshShellStateValues();
                                    ShellColors.Refresh();
                                    InstanceManager.SyncToolbarColorThreads();
                                    this.fNoMoreImmersiveColorSet = true;
                                    ActionDelayer.Add((Action)(() => this.fNoMoreImmersiveColorSet = false), 3000);
                                }
                            }
                            else
                            {
                                QTUtility.RefreshShellStateValues();
                            }
                        }
                        else
                        {
                           //  LauncherManager.SetDirty();
                        }
                            

                        if ((int)(long)m.WParam == 20)
                        {
                           //  CompatibleView.UpdateDesktopFocusedColor();
                        }
                            
                        m.Result = IntPtr.Zero;
                        break;
                }*/
                try {
                    consumed = MessageCaptured(ref m);
                }
                catch(Exception ex) {
                    QTUtility2.MakeErrorLog(ex, String.Format(m.ToString()));
                }
            }
            if(!consumed) {
                base.WndProc(ref m);
            }
        }

        // The real ReleaseHandle is unsafe.  NEVER EVER EVER call it!
        // Just clear the event subscription list instead.
        public override void ReleaseHandle() {
            MessageCaptured = null;
        }

        public IntPtr OptionalHandle { get; set; }

        internal delegate bool MessageEventHandler(ref Message msg);
    }
}

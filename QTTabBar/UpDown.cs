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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class UpDown : Control {
        private NativeUpDown nativeUpDown;

        public event QEventHandler ValueChanged;

        public UpDown() {
            Size = new Size(0x24, 0x18);
            nativeUpDown = new NativeUpDown(this);
        }

        protected override void Dispose(bool disposing) {
            if(nativeUpDown != null) {
                nativeUpDown.DestroyHandle();
                nativeUpDown = null;
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m) {
            if(((nativeUpDown != null) && (m.Msg == WM.NOTIFY)) && (ValueChanged != null)) {
                NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                if((nmhdr.code == -722) && (nmhdr.hwndFrom == nativeUpDown.Handle)) {
                    NMUPDOWN nmupdown = (NMUPDOWN)Marshal.PtrToStructure(m.LParam, typeof(NMUPDOWN));
                    ValueChanged(this, new QEventArgs((nmupdown.iDelta < 0) ? ArrowDirection.Right : ArrowDirection.Left));
                }
            }
            base.WndProc(ref m);
        }

        private sealed class NativeUpDown : NativeWindow {
            private bool fTrackMouseEvent;
            private static Rectangle rctDw = new Rectangle(0, 0, 0x12, 0x18);
            private static Rectangle rctUp = new Rectangle(0x12, 0, 0x12, 0x18);
            private VisualStyleRenderer rendererDown_Hot;
            private VisualStyleRenderer rendererDown_Normal;
            private VisualStyleRenderer rendererDown_Pressed;
            private VisualStyleRenderer rendererUp_Hot;
            private VisualStyleRenderer rendererUp_Normal;
            private VisualStyleRenderer rendererUp_Pressed;
            private int stateDown;
            private int stateUP;
            private TRACKMOUSEEVENT TME;

            public NativeUpDown(UpDown OwnerControl) {
                CreateParams cp = new CreateParams();
                cp.ClassName = "msctls_updown32";
                cp.X = cp.Y = 0;
                cp.Width = OwnerControl.Width;
                cp.Height = OwnerControl.Height;
                cp.Parent = OwnerControl.Handle;
                cp.Style = 0x50000040;
                CreateHandle(cp);
                fTrackMouseEvent = true;
                TME = new TRACKMOUSEEVENT();
                TME.cbSize = Marshal.SizeOf(TME);
                TME.dwFlags = 2;
                TME.hwndTrack = Handle;
            }

            private void InitializeRenderer() {
                rendererDown_Normal = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Normal);
                rendererUp_Normal = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Normal);
                rendererDown_Hot = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Hot);
                rendererUp_Hot = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Hot);
                rendererDown_Pressed = new VisualStyleRenderer(VisualStyleElement.Spin.DownHorizontal.Pressed);
                rendererUp_Pressed = new VisualStyleRenderer(VisualStyleElement.Spin.UpHorizontal.Pressed);
            }

            protected override void WndProc(ref Message m) {
                if(VisualStyleRenderer.IsSupported) {
                    switch(m.Msg) {
                        case WM.MOUSEMOVE: {
                                if(fTrackMouseEvent) {
                                    fTrackMouseEvent = false;
                                    PInvoke.TrackMouseEvent(ref TME);
                                }
                                bool flag = (((int)m.WParam) & 1) == 1;
                                if((((int)((long)m.LParam)) & 0xffff) < 0x13) {
                                    stateDown = flag ? 2 : 1;
                                    stateUP = 0;
                                }
                                else {
                                    stateDown = 0;
                                    stateUP = flag ? 2 : 1;
                                }
                                PInvoke.InvalidateRect(m.HWnd, IntPtr.Zero, false);
                                break;
                            }
                        case WM.LBUTTONDOWN:
                            if((((int)((long)m.LParam)) & 0xffff) >= 0x13) {
                                stateUP = 2;
                                break;
                            }
                            stateDown = 2;
                            break;

                        case WM.LBUTTONUP:
                            if((((int)((long)m.LParam)) & 0xffff) >= 0x13) {
                                stateUP = 1;
                                break;
                            }
                            stateDown = 1;
                            break;

                        case WM.MOUSELEAVE:
                            stateDown = stateUP = 0;
                            fTrackMouseEvent = true;
                            PInvoke.InvalidateRect(m.HWnd, IntPtr.Zero, false);
                            break;

                        case WM.PAINT: {
                                if(rendererDown_Normal == null) {
                                    InitializeRenderer();
                                }
                                IntPtr dC = PInvoke.GetDC(m.HWnd);
                                if(!(dC != IntPtr.Zero)) {
                                    break;
                                }
                                using(Graphics graphics = Graphics.FromHdc(dC)) {
                                    VisualStyleRenderer renderer;
                                    VisualStyleRenderer renderer2;
                                    if(stateDown == 0) {
                                        renderer = rendererDown_Normal;
                                    }
                                    else if(stateDown == 1) {
                                        renderer = rendererDown_Hot;
                                    }
                                    else {
                                        renderer = rendererDown_Pressed;
                                    }
                                    if(stateUP == 0) {
                                        renderer2 = rendererUp_Normal;
                                    }
                                    else if(stateUP == 1) {
                                        renderer2 = rendererUp_Hot;
                                    }
                                    else {
                                        renderer2 = rendererUp_Pressed;
                                    }
                                    renderer.DrawBackground(graphics, rctDw);
                                    renderer2.DrawBackground(graphics, rctUp);
                                }
                                PInvoke.ReleaseDC(m.HWnd, dC);
                                PInvoke.ValidateRect(m.HWnd, IntPtr.Zero);
                                m.Result = IntPtr.Zero;
                                return;
                            }
                    }
                }
                base.WndProc(ref m);
            }
        }
    }
}

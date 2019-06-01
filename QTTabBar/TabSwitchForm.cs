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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class TabSwitchForm : Form {
        private IContainer components;
        private Dictionary<int, Rectangle> dicItemRcts = new Dictionary<int, Rectangle>();
        private bool fCompositionEnabled;
        private bool fDoubleBufferRequired;
        private bool fIsShown;
        private Font fntMenu;
        private Font fntMenuBold;
        private int iHoveredIndex = -1;
        private int initialSelectedIndex;
        private List<PathData> lstPaths = new List<PathData>();
        private const int MAXITEM = 11;
        private static int menuHeight;
        private int selectedIndex;
        private ToolTip toolTipSwitcher;

        public event ItemCheckEventHandler Switched;

        public TabSwitchForm() {
            InitializeComponent();
            fntMenu = SystemFonts.MenuFont;
            fntMenuBold = new Font(fntMenu, FontStyle.Bold);
            SetCompositionState();
        }

        protected override void Dispose(bool disposing) {
            if(fntMenu != null) {
                fntMenu.Dispose();
                fntMenu = null;
            }
            if(fntMenuBold != null) {
                fntMenuBold.Dispose();
                fntMenuBold = null;
            }
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DrawItems(Graphics g) {
            dicItemRcts.Clear();
            int x = fCompositionEnabled ? 2 : 6;
            int num2 = fCompositionEnabled ? 0 : (!QTUtility.IsXP ? 2 : 8);
            int y = !QTUtility.IsXP ? 4 : 8;
            int num4 = TabSwitchForm.menuHeight + (!QTUtility.IsXP ? 11 : 0x12);
            int width = ClientSize.Width;
            int menuHeight = TabSwitchForm.menuHeight;
            int num7 = Math.Max((menuHeight - 0x10) / 2, 0);
            int count = lstPaths.Count;
            int num9 = 5;
            bool flag = count > 11;
            int num10 = flag ? num9 : ((count - 1) / 2);
            int num11 = flag ? num9 : ((count - 1) - num10);
            int num12 = selectedIndex - num10;
            if(num12 < 0) {
                num12 += count;
            }
            DrawText(g, GetTitleText(lstPaths[selectedIndex]), new Rectangle(num2, y, (width - 4) - num2, menuHeight), false, false, StringAlignment.Center);
            for(int i = 0; i < num10; i++) {
                int num14 = i + num12;
                if(num14 < 0) {
                    num14 += count;
                }
                else if(num14 > (count - 1)) {
                    num14 -= count;
                }
                Rectangle rectangle = new Rectangle(x, num4, 0x18, menuHeight);
                Rectangle rectangle2 = new Rectangle(0x24, num4 + num7, 0x10, 0x10);
                Rectangle rectangle3 = new Rectangle(0x36, num4, 300, menuHeight);
                Rectangle rectangle4 = new Rectangle(4, num4, width - 8, menuHeight);
                bool flag2 = num14 == initialSelectedIndex;
                DrawText(g, (num14 + 1).ToString(), rectangle, false, flag2, StringAlignment.Far);
                g.DrawImage(GetImage(lstPaths[num14]), rectangle2);
                DrawText(g, lstPaths[num14].strDisplay, rectangle3, false, flag2, StringAlignment.Near);
                dicItemRcts[num14] = rectangle4;
                if(num14 == iHoveredIndex) {
                    DrawSelection(g, rectangle4, MouseButtons == MouseButtons.Left);
                }
                num4 += menuHeight;
            }
            Rectangle rct = new Rectangle(x, num4, 0x18, menuHeight);
            Rectangle rect = new Rectangle(0x24, num4 + num7, 0x10, 0x10);
            Rectangle rectangle7 = new Rectangle(0x36, num4, 300, menuHeight);
            Rectangle rectangle8 = new Rectangle(4, num4, width - 8, menuHeight);
            bool fBold = selectedIndex == initialSelectedIndex;
            if(!fCompositionEnabled) {
                g.FillRectangle(SystemBrushes.MenuHighlight, rectangle8);
            }
            DrawText(g, (selectedIndex + 1).ToString(), rct, true, fBold, StringAlignment.Far);
            g.DrawImage(GetImage(lstPaths[selectedIndex]), rect);
            DrawText(g, lstPaths[selectedIndex].strDisplay, rectangle7, true, fBold, StringAlignment.Near);
            if(fCompositionEnabled) {
                DrawSelection(g, rectangle8, true);
            }
            dicItemRcts[selectedIndex] = rectangle8;
            num4 += menuHeight;
            for(int j = 0; j < num11; j++) {
                int num16 = (j + selectedIndex) + 1;
                if(num16 < 0) {
                    num16 += count;
                }
                else if(num16 > (count - 1)) {
                    num16 -= count;
                }
                Rectangle rectangle9 = new Rectangle(x, num4, 0x18, menuHeight);
                Rectangle rectangle10 = new Rectangle(0x24, num4 + num7, 0x10, 0x10);
                Rectangle rectangle11 = new Rectangle(0x36, num4, 300, menuHeight);
                Rectangle rectangle12 = new Rectangle(4, num4, width - 8, menuHeight);
                bool flag4 = num16 == initialSelectedIndex;
                DrawText(g, (num16 + 1).ToString(), rectangle9, false, flag4, StringAlignment.Far);
                g.DrawImage(GetImage(lstPaths[num16]), rectangle10);
                DrawText(g, lstPaths[num16].strDisplay, rectangle11, false, flag4, StringAlignment.Near);
                dicItemRcts[num16] = rectangle12;
                if(num16 == iHoveredIndex) {
                    DrawSelection(g, rectangle12, MouseButtons == MouseButtons.Left);
                }
                num4 += menuHeight;
            }
        }

        private static void DrawSelection(Graphics g, Rectangle rct, bool fHot) {
            if(!QTUtility.IsXP) {
                int x = rct.X;
                int y = rct.Y;
                int num3 = rct.Width - 2;
                int height = rct.Height;
                if(!fHot) {
                    g.DrawImage(Resources_Image.imgVistaMenu_LT, new Rectangle(x + 3, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_LM, new Rectangle(x + 3, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_LB, new Rectangle(x + 3, (y + height) - 4, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MT, new Rectangle(x + 7, y, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MM, new Rectangle(x + 7, y + 4, num3 - 11, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_MB, new Rectangle(x + 7, (y + height) - 4, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RT, new Rectangle((x + num3) - 4, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RM, new Rectangle((x + num3) - 4, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_RB, new Rectangle((x + num3) - 4, (y + height) - 4, 4, 4));
                }
                else {
                    g.DrawImage(Resources_Image.imgVistaMenu_LT_hot, new Rectangle(x + 3, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_LM_hot, new Rectangle(x + 3, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_LB_hot, new Rectangle(x + 3, (y + height) - 4, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MT_hot, new Rectangle(x + 7, y, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_MM_hot, new Rectangle(x + 7, y + 4, num3 - 11, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_MB_hot, new Rectangle(x + 7, (y + height) - 4, num3 - 11, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RT_hot, new Rectangle((x + num3) - 4, y, 4, 4));
                    g.DrawImage(Resources_Image.imgVistaMenu_RM_hot, new Rectangle((x + num3) - 4, y + 4, 4, height - 8));
                    g.DrawImage(Resources_Image.imgVistaMenu_RB_hot, new Rectangle((x + num3) - 4, (y + height) - 4, 4, 4));
                }
            }
            else {
                int alpha = fHot ? 0x60 : 0x20;
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, SystemColors.MenuHighlight))) {
                    g.FillRectangle(brush, rct);
                }
            }
        }

        private void DrawText(Graphics g, string text, Rectangle rct, bool fSelect, bool fBold, StringAlignment horizontalAlign) {
            if(fCompositionEnabled) {
                IntPtr hdc = g.GetHdc();
                DrawTextOnGlass(hdc, text, fBold ? fntMenuBold : fntMenu, rct, 4, horizontalAlign);
                g.ReleaseHdc(hdc);
            }
            else {
                using(StringFormat format = StringFormat.GenericDefault) {
                    format.Alignment = horizontalAlign;
                    format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    format.FormatFlags |= StringFormatFlags.NoWrap;
                    if(horizontalAlign == StringAlignment.Center) {
                        format.Trimming = StringTrimming.EllipsisPath;
                    }
                    g.DrawString(text, fBold ? fntMenuBold : fntMenu, fSelect ? SystemBrushes.HighlightText : SystemBrushes.MenuText, rct, format);
                }
            }
        }

        private static void DrawTextOnGlass(IntPtr hDC, string text, Font font, Rectangle rct, int iGlowSize, StringAlignment horizontalAlign) {
            RECT rect = new RECT();
            RECT pRect = new RECT();
            rect.left = rct.Left;
            rect.right = rct.Right + (4 * iGlowSize);
            rect.top = rct.Top;
            rect.bottom = rct.Bottom + (2 * iGlowSize);
            pRect.left = 2 * iGlowSize;
            pRect.top = 2;
            pRect.right = rect.Width - (3 * iGlowSize);
            pRect.bottom = rect.Height - 2;
            TextFormatFlags dwFlags = TextFormatFlags.ModifyString | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            switch(horizontalAlign) {
                case StringAlignment.Near:
                    dwFlags |= TextFormatFlags.EndEllipsis;
                    break;

                case StringAlignment.Center:
                    dwFlags |= TextFormatFlags.PathEllipsis | TextFormatFlags.HorizontalCenter;
                    break;

                case StringAlignment.Far:
                    dwFlags |= TextFormatFlags.Right;
                    break;
            }
            IntPtr ptr = PInvoke.CreateCompatibleDC(hDC);
            if(ptr != IntPtr.Zero) {
                IntPtr ptr5;
                BITMAPINFO pbmi = new BITMAPINFO();
                pbmi.bmiHeader.biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
                pbmi.bmiHeader.biWidth = rect.Width;
                pbmi.bmiHeader.biHeight = -rect.Height;
                pbmi.bmiHeader.biPlanes = 1;
                pbmi.bmiHeader.biBitCount = 0x20;
                pbmi.bmiHeader.biCompression = 0;
                IntPtr hgdiobj = PInvoke.CreateDIBSection(ptr, ref pbmi, 0, out ptr5, IntPtr.Zero, 0);
                if(hgdiobj != IntPtr.Zero) {
                    IntPtr ptr3 = PInvoke.SelectObject(ptr, hgdiobj);
                    IntPtr ptr6 = font.ToHfont();
                    IntPtr ptr4 = PInvoke.SelectObject(ptr, ptr6);
                    VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
                    DTTOPTS pOptions = new DTTOPTS();
                    pOptions.dwSize = Marshal.SizeOf(typeof(DTTOPTS));
                    pOptions.dwFlags = 0x2800;
                    pOptions.iGlowSize = iGlowSize;
                    PInvoke.DrawThemeTextEx(renderer.Handle, ptr, 0, 0, text, -1, dwFlags, ref pRect, ref pOptions);
                    PInvoke.BitBlt(hDC, rect.left, rect.top, rect.Width, rect.Height, ptr, 0, 0, 0xcc0020);
                    PInvoke.SelectObject(ptr, ptr3);
                    PInvoke.SelectObject(ptr, ptr4);
                    PInvoke.DeleteObject(hgdiobj);
                    PInvoke.DeleteObject(ptr6);
                }
                PInvoke.DeleteDC(ptr);
            }
        }

        private static Image GetImage(PathData pathData) {
            string strImageKey = pathData.strImageKey;
            if(!QTUtility.ImageListGlobal.Images.ContainsKey(strImageKey)) {
                strImageKey = QTUtility.GetImageKey(pathData.strPath, null);
            }
            return QTUtility.ImageListGlobal.Images[strImageKey];
        }

        private static string GetTitleText(PathData pathData) {
            string strPath = pathData.strPath;
            if(strPath.StartsWith("::")) {
                return pathData.strDisplay;
            }
            int index = strPath.IndexOf("???");
            int length = strPath.IndexOf("*?*?*");
            if(index != -1) {
                return strPath.Substring(0, index);
            }
            if(length != -1) {
                strPath = strPath.Substring(0, length);
            }
            return strPath;
        }

        public void HideSwitcher(bool fSwitch) {
            HideSwitcherInner(false, fSwitch);
        }

        private void HideSwitcherInner(bool fClickClose, bool fSwitch) {
            if(fIsShown) {
                if(fCompositionEnabled) {
                    SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                    fDoubleBufferRequired = true;
                }
                fIsShown = false;
                PInvoke.ShowWindow(Handle, 0);
                if(fSwitch && (Switched != null)) {
                    ItemCheckEventArgs e = new ItemCheckEventArgs(fClickClose ? iHoveredIndex : selectedIndex, CheckState.Checked, CheckState.Checked);
                    Switched(this, e);
                }
            }
        }

        private void InitializeComponent() {
            components = new Container();
            toolTipSwitcher = new ToolTip(components);
            SuspendLayout();
            toolTipSwitcher.ShowAlways = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(0x110, 0x130);
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TabSwitchForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            ResumeLayout(false);
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            if(iHoveredIndex != -1) {
                iHoveredIndex = -1;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            Dictionary<int, Rectangle> dictionary = new Dictionary<int, Rectangle>(dicItemRcts);
            foreach(int num in dictionary.Keys.Where(num => dicItemRcts[num].Contains(e.Location))) {
                if(iHoveredIndex != num) {
                    if(dicItemRcts.ContainsKey(iHoveredIndex)) {
                        Invalidate(dicItemRcts[iHoveredIndex]);
                    }
                    iHoveredIndex = num;
                    Invalidate(dicItemRcts[num]);
                    toolTipSwitcher.Active = false;
                    toolTipSwitcher.SetToolTip(this, GetTitleText(lstPaths[iHoveredIndex]));
                    toolTipSwitcher.Active = true;
                }
                return;
            }
            if(iHoveredIndex != -1) {
                Invalidate();
                iHoveredIndex = -1;
            }
            toolTipSwitcher.Active = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            if(lstPaths.Count > 0) {
                DrawItems(e.Graphics);
            }
            if(fDoubleBufferRequired) {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                fDoubleBufferRequired = false;
            }
        }

        private void SetCompositionState() {
            if(!QTUtility.IsXP) {
                if((0 <= PInvoke.DwmIsCompositionEnabled(out fCompositionEnabled)) && fCompositionEnabled) {
                    MARGINS pMarInset = new MARGINS();
                    pMarInset.cxLeftWidth = -1;
                    if(0 <= PInvoke.DwmExtendFrameIntoClientArea(Handle, ref pMarInset)) {
                        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                        BackColor = Color.Black;
                        return;
                    }
                }
                BackColor = SystemColors.Menu;
            }
            else {
                FormBorderStyle = FormBorderStyle.FixedToolWindow;
            }
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        public void ShowSwitcher(IntPtr hwndExplr, int indexCurrent, List<PathData> lstPaths) {
            RECT rect;
            initialSelectedIndex = selectedIndex = indexCurrent;
            this.lstPaths = lstPaths;
            menuHeight = SystemInformation.MenuHeight;
            if(!QTUtility.IsXP && (PInvoke.DwmIsCompositionEnabled(out fCompositionEnabled) != 0)) {
                fCompositionEnabled = false;
            }
            int num = Math.Min(11, lstPaths.Count);
            int cy = 0x2a + (menuHeight * (num + 1));
            int cx = 0x184;
            PInvoke.GetWindowRect(hwndExplr, out rect);
            int x = rect.left + ((rect.Width - cx) / 2);
            int y = rect.top + ((rect.Height - cy) / 2);
            PInvoke.SetWindowPos(Handle, (IntPtr)(-1), x, y, cx, cy, 0x18);
            PInvoke.ShowWindow(Handle, 4);
            fIsShown = true;
            fDoubleBufferRequired = fCompositionEnabled;
        }

        public int Switch(bool fShift) {
            selectedIndex += fShift ? -1 : 1;
            iHoveredIndex = -1;
            if(selectedIndex < 0) {
                selectedIndex = lstPaths.Count - 1;
            }
            else if(selectedIndex > (lstPaths.Count - 1)) {
                selectedIndex = 0;
            }
            if(toolTipSwitcher.Active) {
                toolTipSwitcher.Hide(this);
            }
            Invalidate();
            return selectedIndex;
        }

        protected override void WndProc(ref Message m) {
            switch(m.Msg) {
                case WM.LBUTTONUP:
                    if(iHoveredIndex != -1) {
                        Point pt = QTUtility2.PointFromLPARAM(m.LParam);
                        Dictionary<int, Rectangle> dictionary2 = new Dictionary<int, Rectangle>(dicItemRcts);
                        foreach(Rectangle rect in from num in dictionary2.Keys 
                                where num == iHoveredIndex select dicItemRcts[num]) {
                            if(rect.Contains(pt)) {
                                HideSwitcherInner(true, true);
                            }
                            break;
                        }
                    }
                    goto Label_01D5;

                case WM.DWMCOMPOSITIONCHANGED:
                    SetCompositionState();
                    goto Label_01D5;

                case WM.MOUSEACTIVATE:
                    if((QTUtility2.GET_Y_LPARAM(m.LParam) == 0x201) && (iHoveredIndex != -1)) {
                        Point point = PointToClient(MousePosition);
                        Dictionary<int, Rectangle> dictionary = new Dictionary<int, Rectangle>(dicItemRcts);
                        foreach(int num in dictionary.Keys) {
                            if(num == iHoveredIndex) {
                                Rectangle rectangle = dicItemRcts[num];
                                if(rectangle.Contains(point)) {
                                    Invalidate(dicItemRcts[num]);
                                }
                                break;
                            }
                        }
                    }
                    break;

                case WM.NCHITTEST:
                    base.WndProc(ref m);
                    switch(((int)m.Result)) {
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 0x10:
                        case 0x11:
                            m.Result = (IntPtr)0x12;
                            return;
                    }
                    return;

                default:
                    goto Label_01D5;
            }
            m.Result = (IntPtr)4;
            return;
        Label_01D5:
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                if(QTUtility.IsXP) {
                    createParams.ClassStyle |= 0x20000;
                }
                return createParams;
            }
        }

        public bool IsShown {
            get {
                return fIsShown;
            }
        }

        public int SelectedIndex {
            get {
                return selectedIndex;
            }
        }
    }

    internal sealed class PathData {
        public string strDisplay;
        public string strImageKey;
        public string strPath;

        public PathData(string strDisplay, string strPath, string strImageKey) {
            this.strDisplay = strDisplay;
            this.strPath = strPath;
            this.strImageKey = strImageKey;
        }
    }
}

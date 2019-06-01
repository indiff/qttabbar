//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
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
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using QTPlugin.Interop;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace QuizoPlugins {
    internal sealed partial class FolderViewModeWindow : Form {
        private bool fFirstFocusing;
        public event EventHandler ViewModeChanged;
        private Color clrBG, clrBorder;


        public FolderViewModeWindow(string[] resLabels) {
            ProfessionalColorTable pct = new ProfessionalColorTable();
            clrBG = pct.ToolStripDropDownBackground;
            clrBorder = pct.MenuBorder;

            BackColor = clrBG;

            InitializeComponent();

            labelTHUMBSTRIP.Text = resLabels[0];
            labelTHUMBNAIL.Text = resLabels[1];
            labelTILE.Text = resLabels[2];
            labelICON.Text = resLabels[3];
            labelLIST.Text = resLabels[4];
            labelDETAIL.Text = resLabels[5];

            Rectangle rct = new Rectangle(4, 4, 16, 16);
            PixelFormat pf = PixelFormat.Format32bppArgb;
            labelTHUMBSTRIP.Image2 = Resource.imgFilm.Clone(rct, pf);
            labelTHUMBNAIL.Image2 = Resource.imgThumb.Clone(rct, pf);
            labelTILE.Image2 = Resource.imgTiles.Clone(rct, pf);
            labelICON.Image2 = Resource.imgIcon.Clone(rct, pf);
            labelLIST.Image2 = Resource.imgList.Clone(rct, pf);
            labelDETAIL.Image2 = Resource.imgDetails.Clone(rct, pf);

            labelTHUMBSTRIP.Tag = FOLDERVIEWMODE.FVM_THUMBSTRIP;
            labelTHUMBNAIL.Tag = FOLDERVIEWMODE.FVM_THUMBNAIL;
            labelTILE.Tag = FOLDERVIEWMODE.FVM_TILE;
            labelICON.Tag = FOLDERVIEWMODE.FVM_ICON;
            labelLIST.Tag = FOLDERVIEWMODE.FVM_LIST;
            labelDETAIL.Tag = FOLDERVIEWMODE.FVM_DETAILS;

            trackBar1.LostFocus += trackBar1_LostFocus;
            trackBar1.KeyDown += trackBar1_KeyDown;
        }


        public void ShowWindow(Point pnt, FOLDERVIEWMODE fvmCurrentMode) {
            const uint SWP_NOACTIVATE = 0x0010;
            const int SW_SHOWNOACTIVATE = 4;

            trackBar1.Value = ModeToInt(fvmCurrentMode);

            // set the slider of trackbar under mouse position
            RECT rct = GetThumbRect();
            Point pntCenter = new Point(rct.left + rct.Width / 2, rct.top + rct.Height / 2);
            Rectangle rctScreen = Screen.FromPoint(pnt).Bounds;

            pnt.X = pnt.X - pntCenter.X;
            pnt.Y = pnt.Y - pntCenter.Y;

            // ensure visible in the screen
            if(pnt.X < rctScreen.Left)
                pnt.X = rctScreen.Left;
            else if(pnt.X + Width > rctScreen.Right)
                pnt.X = rctScreen.Right - Width;

            if(pnt.Y < rctScreen.Top)
                pnt.Y = rctScreen.Top;
            else if(pnt.Y + Height > rctScreen.Bottom)
                pnt.Y = rctScreen.Bottom - Height;

            PInvoke.SetWindowPos(Handle, (IntPtr)(-1), pnt.X, pnt.Y, Width, Height, SWP_NOACTIVATE);
            PInvoke.ShowWindow(Handle, SW_SHOWNOACTIVATE);

            trackBar1.Focus();
        }

        public void HideWindow() {
            PInvoke.ShowWindow(Handle, 0);
        }

        public FOLDERVIEWMODE ViewMode {
            get {
                switch(trackBar1.Value) {
                    case 5:
                        return FOLDERVIEWMODE.FVM_THUMBSTRIP;
                    case 4:
                        return FOLDERVIEWMODE.FVM_THUMBNAIL;
                    case 3:
                        return FOLDERVIEWMODE.FVM_TILE;
                    case 1:
                        return FOLDERVIEWMODE.FVM_LIST;
                    case 0:
                        return FOLDERVIEWMODE.FVM_DETAILS;
                    default:
                        return FOLDERVIEWMODE.FVM_ICON;
                }
            }
        }

        protected override CreateParams CreateParams {
            get {
                const int CS_DROPSHADOW = 0x00020000;

                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            using(Pen p = new Pen(clrBorder)) {
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }



        private void ChangeLabelsState() {
            labelDETAIL.ForeColor =
            labelLIST.ForeColor =
            labelICON.ForeColor =
            labelTILE.ForeColor =
            labelTHUMBNAIL.ForeColor =
            labelTHUMBSTRIP.ForeColor = SystemColors.GrayText;

            switch(trackBar1.Value) {
                case 0:
                    labelDETAIL.ForeColor = SystemColors.ControlText;
                    break;
                case 1:
                    labelLIST.ForeColor = SystemColors.ControlText;
                    break;
                case 2:
                    labelICON.ForeColor = SystemColors.ControlText;
                    break;
                case 3:
                    labelTILE.ForeColor = SystemColors.ControlText;
                    break;
                case 4:
                    labelTHUMBNAIL.ForeColor = SystemColors.ControlText;
                    break;
                case 5:
                    labelTHUMBSTRIP.ForeColor = SystemColors.ControlText;
                    break;
            }
        }

        private static int ModeToInt(FOLDERVIEWMODE fvm) {
            switch(fvm) {
                case FOLDERVIEWMODE.FVM_THUMBSTRIP:
                    return 5;
                case FOLDERVIEWMODE.FVM_THUMBNAIL:
                    return 4;
                case FOLDERVIEWMODE.FVM_TILE:
                    return 3;
                case FOLDERVIEWMODE.FVM_LIST:
                    return 1;
                case FOLDERVIEWMODE.FVM_DETAILS:
                    return 0;
                default:
                    return 2;
            }
        }

        private RECT GetThumbRect() {
            RECT rct = new RECT();

            if(trackBar1.IsHandleCreated) {
                const int WM_USER = 0x0400;
                const int TBM_GETTHUMBRECT = (WM_USER + 25);

                PInvoke.SendMessage(trackBar1.Handle, TBM_GETTHUMBRECT, IntPtr.Zero, ref rct);
            }
            return rct;
        }


        private void trackBar1_LostFocus(object sender, EventArgs e) {
            if(fFirstFocusing) {
                HideWindow();
                fFirstFocusing = false;
            }
            else
                fFirstFocusing = true;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) {
            if(ViewModeChanged != null)
                ViewModeChanged(this, e);

            ChangeLabelsState();
        }

        private void trackBar1_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Escape) {
                HideWindow();
            }
        }

        private void labelButtons_Click(object sender, EventArgs e) {
            FOLDERVIEWMODE mode = (FOLDERVIEWMODE)((LabelEx)sender).Tag;

            trackBar1.Value = ModeToInt(mode);
        }
    }

    sealed class LabelEx : Label {
        private bool fMouseOn;
        private VisualStyleRenderer vsrNormal, vsrPressed;
        private Image image;

        public LabelEx() {
            Size = new Size(107, 28);
            Padding = new Padding(24, 0, 4, 0);
            Margin = Padding.Empty;
            TextAlign = ContentAlignment.MiddleLeft;
        }

        protected override void Dispose(bool disposing) {
            image = null;
            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e) {
            fMouseOn = true;
            base.OnMouseEnter(e);
            Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e) {
            fMouseOn = false;
            base.OnMouseLeave(e);
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            Invalidate();
        }


        protected override void OnPaintBackground(PaintEventArgs e) {
            if(fMouseOn) {
                MouseButtons mb = MouseButtons;

                if(VisualStyleRenderer.IsSupported) {
                    if(vsrNormal == null)
                        vsrNormal = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);
                    if(vsrPressed == null)
                        vsrPressed = new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Pressed);

                    if(mb == MouseButtons.Left)
                        vsrPressed.DrawBackground(e.Graphics, e.ClipRectangle);
                    else
                        vsrNormal.DrawBackground(e.Graphics, e.ClipRectangle);
                }
                else {
                    ControlPaint.DrawButton(e.Graphics, e.ClipRectangle, mb == MouseButtons.Left ? ButtonState.Pushed : ButtonState.Normal);
                }
            }
            else
                base.OnPaintBackground(e);

            if(image != null) {
                e.Graphics.DrawImage(image, new Rectangle(6, 6, 16, 16));
            }
        }

        public Image Image2 {
            get {
                return image;
            }
            set {
                image = value;
            }
        }
    }

    sealed class TrackBarEx : TrackBar {
        private int cumulativeWheelData;

        protected override void OnMouseWheel(MouseEventArgs e) {
            HandledMouseEventArgs args = e as HandledMouseEventArgs;
            if(args != null) {
                if(args.Handled) {
                    return;
                }
                args.Handled = true;
            }
            if(((ModifierKeys & (Keys.Alt | Keys.Shift)) == Keys.None) && (MouseButtons == MouseButtons.None)) {
                cumulativeWheelData += e.Delta;
                int num3 = (int)((cumulativeWheelData) / 120f);
                if(num3 != 0) {
                    if(num3 > 0) {
                        Value = Math.Min(num3 + Value, Maximum);
                        cumulativeWheelData -= (int)(num3 * 120f);
                    }
                    else {
                        Value = Math.Max(num3 + Value, Minimum);
                        cumulativeWheelData -= (int)(num3 * 120f);
                    }
                }
                if(e.Delta != Value) {
                    OnScroll(EventArgs.Empty);
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }
    }

}
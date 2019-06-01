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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class ToolStripClasses : ToolStrip {
        private bool fMA;

        public event EventHandler MouseActivated;

        public void HideToolTip() {
            if(ShowItemToolTips) {
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
                try {
                    typeof(ToolStrip).GetMethod("UpdateToolTip", bindingAttr).Invoke(this, new object[1]);
                    Type type = Type.GetType("System.Windows.Forms.MouseHoverTimer, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    PropertyInfo property = typeof(ToolStrip).GetProperty("MouseHoverTimer", bindingAttr);
                    type.GetMethod("Cancel", Type.EmptyTypes).Invoke(property.GetValue(this, null), null);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs mea) {
            if(OverflowButton.DropDown.Visible) return;
            if(Items.OfType<ToolStripDropDownItem>().Any(item =>
                    item.Visible && item.HasDropDownItems && item.DropDown.Visible)) {
                return;
            }
            base.OnMouseMove(mea);
        }

        internal void RaiseOnResize() {
            OnResize(EventArgs.Empty);
        }

        protected override void WndProc(ref Message m) {
            if(m.Msg == WM.MOUSEACTIVATE) {
                fMA = false;
                if(0x201 == QTUtility2.GET_Y_LPARAM(m.LParam)) {
                    base.WndProc(ref m);
                    if(2 == ((int)m.Result)) {
                        fMA = true;
                    }
                    return;
                }
            }
            else if(m.Msg == WM.LBUTTONUP) {
                if(fMA && (MouseActivated != null)) {
                    base.WndProc(ref m);
                    MouseActivated(this, EventArgs.Empty);
                    fMA = false;
                    return;
                }
                fMA = false;
            }
            base.WndProc(ref m);
        }

        protected override bool DefaultShowItemToolTips {
            get {
                return false;
            }
        }
    }

    internal sealed class ToolStripTrackBar : ToolStripControlHost {
        private bool fSuppressEvent;

        public event EventHandler ValueChanged;

        public ToolStripTrackBar()
            : base(new TrackBar()) {
            TrackBar control = (TrackBar)Control;
            control.MaximumSize = new Size(80, 0x16);
            control.Maximum = 0xff;
            control.Minimum = 20;
            control.Value = 0xff;
            control.SmallChange = 15;
            control.LargeChange = 0x1a;
            control.TickFrequency = 0x1a;
        }

        protected override void OnSubscribeControlEvents(Control control) {
            base.OnSubscribeControlEvents(control);
            TrackBar bar = (TrackBar)control;
            bar.ValueChanged += OnValueChange;
        }

        private void OnValueChange(object sender, EventArgs e) {
            if(!fSuppressEvent && (ValueChanged != null)) {
                ValueChanged(this, e);
            }
        }

        public void SetValueWithoutEvent(int value) {
            TrackBar control = (TrackBar)Control;
            if((control.Minimum <= value) && (value <= control.Maximum)) {
                fSuppressEvent = true;
                control.Value = value;
                fSuppressEvent = false;
            }
        }

        public int Value {
            get {
                return ((TrackBar)Control).Value;
            }
        }
    }

    internal sealed class ToolStripSearchBox : ToolStripControlHost {
        private bool fLocked;
        private bool fNowDragging;
        private bool fSuppressTextChangeEvent;
        private const int GRIPWIDTH = 12;
        private const int MAXWIDTH = 0x400;
        private const int MINWIDTH = 0x20;
        private string strDefText;
        private TextBox tb;
        private const int TBSIZE_LARGE = 0x18;
        private const int TBSIZE_SMALL = 0x12;

        public event CancelEventHandler ErasingText;

        public event EventHandler ResizeComplete;

        public ToolStripSearchBox(bool fLarge, bool fLocked, string strDefText, int tbWidth)
            : base(CreateControlInstance(fLarge, strDefText, tbWidth)) {
            AutoSize = false;
            Padding = fLarge ? new Padding(4, 0, 4, 0) : new Padding(2, 0, 2, 0);
            tb = (TextBox)Control;
            this.strDefText = strDefText;
            this.fLocked = fLocked;
        }

        private static Control CreateControlInstance(bool fLarge, string strDefText, int tbWidth) {
            TextBox box = new TextBox();
            box.AutoSize = QTUtility.IsXP;
            box.ForeColor = SystemColors.GrayText;
            box.Text = strDefText;
            box.ImeMode = ImeMode.NoControl;
            box.Size = new Size(tbWidth, fLarge ? 0x18 : 0x12);
            box.Font = new Font(SystemFonts.IconTitleFont.FontFamily, fLarge ? 9f : (!QTUtility.IsXP ? 8.25f : 9f));
            return box;
        }

        private bool IsMouseOnTheEdge(Point pnt) {
            if(pnt.X <= (tb.Width - 12)) {
                return false;
            }
            return ((pnt.Y < tb.Bottom) && (pnt.Y > tb.TabIndex));
        }

        protected override void OnBoundsChanged() {
            base.OnBoundsChanged();
            if((Parent != null) && !Parent.Disposing) {
                Parent.Refresh();
            }
        }

        protected override void OnGotFocus(EventArgs e) {
            if(tb.Text == strDefText) {
                tb.ForeColor = SystemColors.ControlText;
                fSuppressTextChangeEvent = true;
                tb.Text = string.Empty;
                fSuppressTextChangeEvent = false;
            }
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e) {
            fNowDragging = false;
            if((tb.Text.Length == 0) && (ErasingText != null)) {
                CancelEventArgs args = new CancelEventArgs();
                ErasingText(this, args);
                if(!args.Cancel) {
                    fSuppressTextChangeEvent = true;
                    tb.ForeColor = SystemColors.GrayText;
                    tb.Text = strDefText;
                    fSuppressTextChangeEvent = false;
                }
            }
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if((!fLocked && IsMouseOnTheEdge(e.Location)) && (e.Button == MouseButtons.Left)) {
                StartDrag(true);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            fNowDragging = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if(!fLocked) {
                if(IsMouseOnTheEdge(e.Location)) {
                    tb.Cursor = Cursors.VSplit;
                }
                else {
                    tb.Cursor = Cursors.IBeam;
                }
                if(fNowDragging) {
                    int min = 32;
                    int max = 1024;
                    ToolStrip owner = Owner;
                    if(((owner != null) && !owner.Disposing) && !(owner is ToolStripOverflow)) {
                        max = (owner.DisplayRectangle.Width - Bounds.X) - 24;
                    }
                    Width = Math.Max(Math.Min(e.X + 12, max), min);
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent) {
            if(!fLocked) {
                StartDrag(false);
            }
            base.OnMouseUp(mevent);
        }

        protected override void OnTextChanged(EventArgs e) {
            if(!fSuppressTextChangeEvent) {
                base.OnTextChanged(e);
            }
        }

        public void RefreshText() {
            fSuppressTextChangeEvent = true;
            if(tb.Focused) {
                tb.ForeColor = SystemColors.ControlText;
                tb.Text = string.Empty;
            }
            else {
                tb.ForeColor = SystemColors.GrayText;
                tb.Text = strDefText;
            }
            fSuppressTextChangeEvent = false;
        }

        private void StartDrag(bool fStart) {
            fNowDragging = fStart;
            if(!fStart && (ResizeComplete != null)) {
                ResizeComplete(this, EventArgs.Empty);
            }
        }

        public TextBox TextBox {
            get {
                return tb;
            }
        }
    }

    internal sealed class ToolbarRenderer : ToolStripSystemRenderer {
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
        }
    }

    internal sealed class VistaMenuRenderer : ToolStripSystemRenderer {
        private static Bitmap bmpCheck = Resources_Image.imgVistaMenu_Check;
        private static Bitmap bmpLB = Resources_Image.imgVistaMenu_LB;
        private static Bitmap bmpLM = Resources_Image.imgVistaMenu_LM;
        private static Bitmap bmpLT = Resources_Image.imgVistaMenu_LT;
        private static Bitmap bmpMB = Resources_Image.imgVistaMenu_MB;
        private static Bitmap bmpMM = Resources_Image.imgVistaMenu_MM;
        private static Bitmap bmpMT = Resources_Image.imgVistaMenu_MT;
        private static Bitmap bmpRB = Resources_Image.imgVistaMenu_RB;
        private static Bitmap bmpRM = Resources_Image.imgVistaMenu_RM;
        private static Bitmap bmpRT = Resources_Image.imgVistaMenu_RT;
        private static Color clrBG = Color.FromArgb(240, 240, 240);
        private static Color clrGray = Color.FromArgb(0x80, 0x80, 0x80);
        private static Color clrLight = Color.FromArgb(0xe3, 0xe3, 0xe3);
        private static Color clrLightLight = Color.White;
        private static Color clrTxt = Color.Black;
        private bool fNoPrefix;

        public VistaMenuRenderer(bool fNoPrefix) {
            this.fNoPrefix = fNoPrefix;
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            e.ArrowColor = e.Item.Enabled ? clrTxt : clrGray;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
            if(e.ToolStrip.RightToLeft == RightToLeft.No) {
                using(Pen pen = new Pen(clrLight)) {
                    e.Graphics.DrawLine(pen, new Point(e.AffectedBounds.Width - 2, 0), new Point(e.AffectedBounds.Width - 2, e.AffectedBounds.Height));
                }
                using(Pen pen2 = new Pen(clrLightLight)) {
                    e.Graphics.DrawLine(pen2, new Point(e.AffectedBounds.Width - 1, 0), new Point(e.AffectedBounds.Width - 1, e.AffectedBounds.Height));
                    return;
                }
            }
            using(Pen pen3 = new Pen(clrLight)) {
                e.Graphics.DrawLine(pen3, new Point(e.ToolStrip.Width - e.ToolStrip.Padding.Right, 0), new Point(e.ToolStrip.Width - e.ToolStrip.Padding.Right, e.AffectedBounds.Height));
            }
            using(Pen pen4 = new Pen(clrLightLight)) {
                e.Graphics.DrawLine(pen4, new Point((e.ToolStrip.Width - e.ToolStrip.Padding.Right) + 1, 0), new Point((e.ToolStrip.Width - e.ToolStrip.Padding.Right) + 1, e.AffectedBounds.Height));
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
            ToolStripDropDownMenu toolStrip = e.ToolStrip as ToolStripDropDownMenu;
            if(toolStrip != null) {
                if(!toolStrip.ShowImageMargin) {
                    e.Graphics.DrawImage(bmpCheck, new Rectangle(3, 0, 0x16, Math.Min(0x16, e.Item.Size.Height)));
                }
                else {
                    Rectangle rect = new Rectangle(7, 2, 0x12, 0x12);
                    using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x80, SystemColors.Highlight))) {
                        e.Graphics.FillRectangle(brush, rect);
                        e.Graphics.DrawRectangle(SystemPens.Highlight, rect);
                    }
                }
            }
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e) {
            QMenuItem item = e.Item as QMenuItem;
            if(((item != null) && item.IsCut) && ((e.ImageRectangle != Rectangle.Empty) && (e.Image != null))) {
                ColorMatrix newColorMatrix = new ColorMatrix();
                using(ImageAttributes attributes = new ImageAttributes()) {
                    newColorMatrix.Matrix33 = 0.5f;
                    attributes.SetColorMatrix(newColorMatrix);
                    Size size = e.Image.Size;
                    e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes);
                    return;
                }
            }
            base.OnRenderItemImage(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            if(e.Item.Enabled) {
                e.TextColor = clrTxt;
            }
            else {
                e.TextColor = clrGray;
            }
            if(e.Item.RightToLeft == RightToLeft.No) {
                Rectangle textRectangle = e.TextRectangle;
                textRectangle.X -= 6;
                e.TextRectangle = textRectangle;
            }
            if(fNoPrefix) {
                e.TextFormat |= TextFormatFlags.NoPrefix;
            }
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if(e.Item.BackColor != e.ToolStrip.BackColor) {
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(Point.Empty, e.Item.Size));
            }
            if(e.Item.Selected) {
                int num = e.Item.Size.Width - 2;
                int height = e.Item.Size.Height;
                e.Graphics.DrawImage(bmpLT, new Rectangle(3, 0, 4, 4));
                e.Graphics.DrawImage(bmpLM, new Rectangle(3, 4, 4, height - 8));
                e.Graphics.DrawImage(bmpLB, new Rectangle(3, height - 4, 4, 4));
                e.Graphics.DrawImage(bmpMT, new Rectangle(7, 0, num - 11, 4));
                e.Graphics.DrawImage(bmpMM, new Rectangle(7, 4, num - 11, height - 8));
                e.Graphics.DrawImage(bmpMB, new Rectangle(7, height - 4, num - 11, 4));
                e.Graphics.DrawImage(bmpRT, new Rectangle(num - 4, 0, 4, 4));
                e.Graphics.DrawImage(bmpRM, new Rectangle(num - 4, 4, 4, height - 8));
                e.Graphics.DrawImage(bmpRB, new Rectangle(num - 4, height - 4, 4, 4));
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            Rectangle rectangle = new Rectangle(Point.Empty, e.Item.Size);
            if(e.ToolStrip.RightToLeft == RightToLeft.No) {
                rectangle.X += e.ToolStrip.Padding.Left - 10;
                rectangle.Width = e.ToolStrip.Width;
            }
            else {
                rectangle.X += 2;
                rectangle.Width = (e.ToolStrip.Width - rectangle.X) - e.ToolStrip.Padding.Right;
            }
            int y = rectangle.Height / 2;
            using(Pen pen = new Pen(clrLight)) {
                e.Graphics.DrawLine(pen, new Point(rectangle.X, y), new Point(rectangle.Width, y));
            }
            using(Pen pen2 = new Pen(clrLightLight)) {
                e.Graphics.DrawLine(pen2, new Point(rectangle.X, y + 1), new Point(rectangle.Width, y + 1));
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
            using(SolidBrush brush = new SolidBrush(clrBG)) {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }
    }

    internal sealed class XPMenuRenderer : ToolStripSystemRenderer {
        private bool fNoPrefix;

        public XPMenuRenderer(bool fNoPrefix) {
            this.fNoPrefix = fNoPrefix;
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e) {
            ToolStripMenuItem item = e.Item as ToolStripMenuItem;
            if((item != null) && item.Checked) {
                Rectangle imageRectangle = e.ImageRectangle;
                imageRectangle.Inflate(1, 1);
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(0x80, SystemColors.Highlight))) {
                    e.Graphics.FillRectangle(brush, imageRectangle);
                    e.Graphics.DrawRectangle(SystemPens.Highlight, imageRectangle);
                }
            }
            QMenuItem item2 = e.Item as QMenuItem;
            if(((item2 != null) && item2.IsCut) && ((e.ImageRectangle != Rectangle.Empty) && (e.Image != null))) {
                ColorMatrix newColorMatrix = new ColorMatrix();
                using(ImageAttributes attributes = new ImageAttributes()) {
                    newColorMatrix.Matrix33 = 0.5f;
                    attributes.SetColorMatrix(newColorMatrix);
                    Size size = e.Image.Size;
                    e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes);
                    return;
                }
            }
            base.OnRenderItemImage(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            if(fNoPrefix) {
                e.TextFormat |= TextFormatFlags.NoPrefix;
            }
            e.TextColor = e.Item.Selected ? SystemColors.HighlightText : SystemColors.MenuText;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
            if((e.Item.BackColor != e.ToolStrip.BackColor) && !e.Item.Selected) {
                using(SolidBrush brush = new SolidBrush(e.Item.BackColor)) {
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                    return;
                }
            }
            e.Graphics.FillRectangle(e.Item.Selected ? SystemBrushes.MenuHighlight : SystemBrushes.Menu, new Rectangle(Point.Empty, e.Item.Size));
        }
    }

    internal sealed class DefaultMenuRenderer : ToolStripProfessionalRenderer {
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e) {
            QMenuItem item = e.Item as QMenuItem;
            if(((item != null) && item.IsCut) && ((e.ImageRectangle != Rectangle.Empty) && (e.Image != null))) {
                ColorMatrix newColorMatrix = new ColorMatrix();
                using(ImageAttributes attributes = new ImageAttributes()) {
                    newColorMatrix.Matrix33 = 0.5f;
                    attributes.SetColorMatrix(newColorMatrix);
                    Size size = e.Image.Size;
                    e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes);
                    return;
                }
            }
            base.OnRenderItemImage(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            e.TextFormat |= TextFormatFlags.NoPrefix;
            base.OnRenderItemText(e);
        }
    }
}

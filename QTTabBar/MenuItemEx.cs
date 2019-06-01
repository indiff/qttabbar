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

using System.Drawing;
using System.Windows.Forms;

namespace QTTabBarLib {
    internal sealed class MenuItemEx : MenuItem {
        private Image img;
        private const int MARGIN_Y = 8;

        public MenuItemEx(string text)
            : base(text) {
            OwnerDraw = true;
        }

        protected override void OnDrawItem(DrawItemEventArgs e) {
            bool flag = (e.State & DrawItemState.Selected) != DrawItemState.None;
            int num = e.Bounds.Height * e.Index;
            int num2 = (e.Bounds.Height - 0x10) / 2;
            e.Graphics.FillRectangle(flag ? SystemBrushes.Highlight : SystemBrushes.Menu, e.Bounds);
            if(img != null) {
                e.Graphics.DrawImage(img, new Rectangle(2, num + num2, 0x10, 0x10), new RectangleF(0f, 0f, 16f, 16f), GraphicsUnit.Pixel);
            }
            e.Graphics.DrawString(Text, SystemFonts.MenuFont, flag ? SystemBrushes.HighlightText : SystemBrushes.MenuText, new PointF(24f, (4 + num)));
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e) {
            SizeF ef = e.Graphics.MeasureString(Text, SystemFonts.MenuFont);
            e.ItemHeight = ((int)ef.Height) + 8;
            e.ItemWidth = ((int)ef.Width) + 0x1a;
        }

        public Image Image {
            set {
                img = value;
            }
        }
    }
}

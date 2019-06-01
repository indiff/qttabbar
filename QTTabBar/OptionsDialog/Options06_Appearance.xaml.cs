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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QTTabBarLib {
    internal partial class Options06_Appearance : OptionsDialogTab {

        public Options06_Appearance() {
            InitializeComponent();

            // Took me forever to figure out that this was necessary.  Why isn't this the default?!!
            // Bindings in context menus won't work without this.
            NameScope.SetNameScope(ctxTabTextColor, NameScope.GetNameScope(this));
            NameScope.SetNameScope(ctxShadowTextColor, NameScope.GetNameScope(this));
        }

        public override void InitializeConfig() {
            // Not needed; everything is done through bindings
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.skin = new Config._Skin();
        }

        public override void CommitConfig() {
            // Not needed; everything is done through bindings
        }

        private void btnShadTextColor_OnChecked(object sender, RoutedEventArgs e) {
            var button = ((ToggleButton)sender);
            ContextMenu menu = button.ContextMenu;
            foreach(MenuItem mi in menu.Items) {
                mi.Icon = new Image { Source = ConvertToBitmapSource((Rectangle)mi.Tag) };
            }
            // Yeah, this is necessary even with the IsChecked <=> IsOpen binding.
            // Not sure why.
            menu.PlacementTarget = button;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void miColorMenuEntry_OnClick(object sender, RoutedEventArgs e) {
            var mi = (MenuItem)sender;
            var rect = (Rectangle)mi.Tag;
            ColorDialogEx cd = new ColorDialogEx { Color = (System.Drawing.Color)rect.Tag };
            if(System.Windows.Forms.DialogResult.OK == cd.ShowDialog()) {
                rect.Tag = cd.Color;
            }
        }

        private void btnRebarBGColorChoose_Click(object sender, RoutedEventArgs e) {
            ColorDialogEx cd = new ColorDialogEx { Color = WorkingConfig.skin.RebarColor };
            if(System.Windows.Forms.DialogResult.OK == cd.ShowDialog()) {
                ((Button)sender).Tag = cd.Color;
            }
        }

        // Draws a control to a bitmap
        private static BitmapSource ConvertToBitmapSource(UIElement element) {
            var target = new RenderTargetBitmap((int)(element.RenderSize.Width), (int)(element.RenderSize.Height), 96, 96, PixelFormats.Pbgra32);
            var brush = new VisualBrush(element);
            var visual = new DrawingVisual();
            var drawingContext = visual.RenderOpen();

            drawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0),
                new Point(element.RenderSize.Width, element.RenderSize.Height)));
            drawingContext.Close();
            target.Render(visual);
            return target;
        }
    }
}

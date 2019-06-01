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

namespace QTTabBarLib {
    internal partial class Options03_Tweaks : OptionsDialogTab {
        public Options03_Tweaks() {
            InitializeComponent();
        }

        public static bool IsXP { get { return QTUtility.IsXP; } }
        public static bool IsNotXP { get { return !QTUtility.IsXP; } }
        public static bool IsWin7 { get { return QTUtility.IsWin7; } }

        public override void InitializeConfig() {
            // Not needed; everything is done through bindings
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.tweaks = new Config._Tweaks();
        }

        public override void CommitConfig() {
            // Not needed; everything is done through bindings
        }

        private void btnAltRowColor_Click(object sender, RoutedEventArgs e) {
            // Works for both buttons.  Each button's Tag is bound to the corresponding property.
            var button = (Button)sender;
            ColorDialogEx cd = new ColorDialogEx { Color = (System.Drawing.Color)button.Tag };
            if(System.Windows.Forms.DialogResult.OK == cd.ShowDialog()) {
                button.Tag = cd.Color;
            }
        }
    }
}

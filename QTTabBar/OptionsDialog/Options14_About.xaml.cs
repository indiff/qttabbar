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
using System.Diagnostics;

namespace QTTabBarLib {
    internal partial class Options14_About : OptionsDialogTab {
        public Options14_About() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            lblVersion.Content = "QTTabBar " + QTUtility2.MakeVersionString();
            runSite.Text = Resources_String.SiteURL;
            hypSite.NavigateUri = new Uri(Resources_String.SiteURL);
            hypSite.RequestNavigate += (sender, args) => Process.Start(Resources_String.SiteURL);

            // add by qwop 2012/06/16 http://qwop.iteye.com
            string mySite = "http://code.google.com/p/qwop-software/"; 
            runQwopSite.Text = mySite;
			qwopSite.NavigateUri = new Uri( mySite );
            qwopSite.RequestNavigate += (sender, args) => Process.Start( mySite );
        }

        public override void ResetConfig() {    
        }

        public override void CommitConfig() {
        }

        private void imgPaypal_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start(Resources_String.PayPalURL);
        }
    }
}

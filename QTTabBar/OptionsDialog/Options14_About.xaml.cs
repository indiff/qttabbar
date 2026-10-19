//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2025  Quizo, Paul Accisano, indiff
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
        private const string DonationUrl = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=7YNCVL5P9ZDY8";

        public Options14_About() {
            InitializeComponent();
            hypSite.RequestNavigate += hypSite_RequestNavigate;
            qwopSite.RequestNavigate += qwopSite_RequestNavigate;
        }

        public override void InitializeConfig() {
            try {
                // 设置默认的title 和版本
                string str = QTUtility.CurrentVersion.ToString();
                if (QTUtility.BetaRevision.Major > 0)
                {
                    str = str + " Beta " + QTUtility.BetaRevision.Major;
                }
                else if (QTUtility.BetaRevision.Minor > 0)
                {
                    str = str + " Alpha " + QTUtility.BetaRevision.Minor;
                }

                
                lblVersion.Content = "QTTabBar " + str + " " + QTUtility2.MakeVersionString();
                runSite.Text = Resources_String.SiteURL;
                hypSite.NavigateUri = new Uri(Resources_String.SiteURL);

                runQwopSite.Text = "Make a donation to support QTTabBar";
                qwopSite.NavigateUri = new Uri(DonationUrl);
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception, "Options13_Language InitializeConfig");
            }         
        }

        private static void hypSite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs args)
        {
            args.Handled = true;
            Process.Start(args.Uri.AbsoluteUri);
        }

        private static void qwopSite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs args)
        {
            args.Handled = true;
            Process.Start(args.Uri.AbsoluteUri);
        }

        public override void ResetConfig() {    
        }

        public override void CommitConfig() {
        }

        /**
         * 中文方式，则进入微信
         */
        private void imgPaypal_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var uiCulture = System.Globalization.CultureInfo.InstalledUICulture.Name;
            var lUiCulture = uiCulture.ToLower();
            if (uiCulture.Equals("zh-CN") || lUiCulture.Equals("zh") || lUiCulture.Equals("cn"))
            {
                Process.Start(Resources_String.PayPalURL);
            }
            else
            {
                Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=7YNCVL5P9ZDY8");
            }
            
        }
    }
}

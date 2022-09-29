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
using System.Diagnostics;

namespace QTTabBarLib {
    internal partial class Options14_About : OptionsDialogTab {
        public Options14_About() {
            InitializeComponent();
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
                hypSite.RequestNavigate += (sender, args) => Process.Start(Resources_String.SiteURL);

                // add by qwop 2012/06/16 http://qwop.iteye.com
                //string mySite = "http://code.google.com/p/qwop-software/"; 
			    // string mySite = "https://github.com/indiff/qttabbar";
                string mySite = "https://www.paypal.com/donate/?cmd=_donations&business=qwop%40live.cn&item_name=QTTabBar&return=https%3A%2F%2Findiff.github.io%2Fqttabbar%2F&cancel_return=https%3A%2F%2Findiff.github.io%2Fqttabbar%2F&no_shipping=1&on0=msg&os0=&currency_code=USD&submit.x=154&submit.y=31"; 
			
                runQwopSite.Text = "Make a donation to support qttabbar";
			    qwopSite.NavigateUri = new Uri( mySite );
                qwopSite.RequestNavigate += (sender, args) => Process.Start( mySite );
            }
            catch (Exception exception)
            {
                QTUtility2.MakeErrorLog(exception, "Options13_Language InitializeConfig");

            }         
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

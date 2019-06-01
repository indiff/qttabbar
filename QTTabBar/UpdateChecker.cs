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
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace QTTabBarLib {
    internal static class UpdateChecker {
        private static string strMsgCaption = "QTTabBar " + QTUtility2.MakeVersionString();
        private static bool fCheckDone;
        private const int INTERVAL_CHECK_DAY = 5;

        public static void Check(bool fForce) {
            // check if new version of QTTabBar exists.

            if(fForce) {
                string msg;
                ShowMsg(CheckInternal(out msg), msg);
                SaveLastCheck();
            }
            else if(!fCheckDone) {
                fCheckDone = true;
                if(DayHasCome()) {
                    string msg;
                    int code = CheckInternal(out msg);
                    SaveLastCheck();

                    if(code == 2) {
                        MessageForm.Show(
                                QTUtility.TextResourcesDic["UpdateCheck"][0] +
                                Environment.NewLine + Environment.NewLine + msg + Environment.NewLine + Environment.NewLine +
                                QTUtility.TextResourcesDic["UpdateCheck"][1],
                                strMsgCaption,
                                Resources_String.SiteURL,
                                MessageBoxIcon.Information,
                                60000);
                    }
                }
            }
        }

        private static int CheckInternal(out string msg) {
            // Reads text file (latestversion.txt) in the web site, 
            // and compare version strings.
            //
            // latestversion.txt template is like this:
            //		1.2.3;1.2.4;1.0
            // released version;Beta version(if exists);Beta revision(if exists)
            //
            // ex. (1.2.3 is released, and no beta version)
            //		1.2.3
            //
            // each string must be parsable into System.Version class object.

            msg = null;
            string str = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Resources_String.SiteURL + "/files/latestversion.txt");
            req.Timeout = 5000;

            try {
                using(HttpWebResponse res = (HttpWebResponse)req.GetResponse()) {
                    Stream stream = res.GetResponseStream();
                    if(stream != null) {
                        using(StreamReader sr = new StreamReader(stream, Encoding.ASCII)) {
                            str = sr.ReadToEnd();
                        }                        
                    }
                }
            }
            catch(WebException exception) {
                switch(exception.Status) {
                    case WebExceptionStatus.ProtocolError:
                        HttpStatusCode status = HttpStatusCode.InternalServerError;
                        if(exception.Response != null && exception.Response is HttpWebResponse) {
                            status = ((HttpWebResponse)exception.Response).StatusCode;
                        }
                        return status == HttpStatusCode.NotFound ? -1 : -2;

                    case WebExceptionStatus.NameResolutionFailure:
                        return -3;

                    default:
                        return -2;
                }
            }

            if(!String.IsNullOrEmpty(str)) {
                try {
                    string[] strs = str.Split(QTUtility.SEPARATOR_CHAR);
                    Version verNew = new Version(strs[0]);

                    if((verNew > QTUtility.CurrentVersion) ||
                            (strs.Length == 1 && verNew == QTUtility.CurrentVersion && QTUtility.BetaRevision.Major > 0)) {
                        // New version found!!
                        //  or, 
                        // Current is Beta, and Released version found!!
                        msg = verNew.ToString();
                        return 2;
                    }

                    if(strs.Length > 2) {
                        try {
                            Version verBeta = new Version(strs[1]);
                            Version verBetaRev = new Version(strs[2]);

                            if((verBeta > QTUtility.CurrentVersion) ||
                                    (verBeta == QTUtility.CurrentVersion && verBetaRev > QTUtility.BetaRevision)) {
                                // newer Beta found.
                                msg = strs[1] + " Beta " + verBetaRev.Major;
                                return 1;
                            }
                        }
                        catch {
                        }
                    }
                    return 0;
                }
                catch(FormatException) {
                }
            }
            return -4;
        }

        private static bool DayHasCome() {
            // determines if INTERVAL_CHECK_DAYs have passed since the last check.
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root)) {
                if(key != null) {
                    long ticks = QTUtility2.GetRegistryValueSafe(key, "LastChecked", -1L);
                    if(DateTime.MinValue.Ticks < ticks && ticks < DateTime.MaxValue.Ticks) {
                        return (DateTime.Now - new DateTime(ticks)).Days > INTERVAL_CHECK_DAY;
                    }
                }
            }

            // if no reg value
            SaveLastCheck();
            return false;
        }

        private static void SaveLastCheck() {
            using(RegistryKey key = Registry.CurrentUser.OpenSubKey(RegConst.Root, true)) {
                if(key != null) {
                    key.SetValue("LastChecked", DateTime.Now.Ticks, RegistryValueKind.QWord);
                }
            }
        }

        private static void ShowMsg(int code, string strOptional) {
            // show result message.
            // error when code value is less than 0.

            string strMsg = String.Empty;
            switch(code) {
                case -1:
                    strMsg = QTUtility.TextResourcesDic["UpdateCheck"][4];
                    break;
                case -2:
                    strMsg = QTUtility.TextResourcesDic["UpdateCheck"][5];
                    break;
                case -3:
                    strMsg = QTUtility.TextResourcesDic["UpdateCheck"][6];
                    break;
                case -4:
                    strMsg = QTUtility.TextResourcesDic["UpdateCheck"][7];
                    break;

                case 0:
                    // current is up to date.
                    strMsg = QTUtility.TextResourcesDic["UpdateCheck"][2];
                    break;

                case 1:
                    // beta found.
                    strMsg = QTUtility.TextResourcesDic["UpdateCheck"][3] + " " + strOptional;
                    break;

                case 2:
                    // New version found.
                    if(DialogResult.OK == MessageBox.Show(
                            QTUtility.TextResourcesDic["UpdateCheck"][0] +
                            Environment.NewLine + Environment.NewLine + strOptional + Environment.NewLine + Environment.NewLine +
                            QTUtility.TextResourcesDic["UpdateCheck"][1],
                            strMsgCaption,
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information)) {
                        try {
                            Process.Start(Resources_String.SiteURL);
                        }
                        catch {
                        }
                    }
                    return;
            }

            MessageBox.Show(strMsg, strMsgCaption, MessageBoxButtons.OK,
                    code < 0 ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }
    }
}
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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    [Plugin(PluginType.Background, Author = "Quizo", Name = "MigemoLoader", Version = "0.9.1.0", Description = "Migemo integration")]
    public class MigemoLoader : IFilter {
        private IPluginServer pluginServer;

        private MigemoWrapper migemoWrapper;

        private static bool fPartialMatch;
        private static string pathDLL, pathDic;


        #region IFilter member

        public bool QueryRegex(string strQuery, out Regex re) {
            re = null;

            if(String.IsNullOrEmpty(strQuery))
                return false;

            if(migemoWrapper == null) {
                try {
                    migemoWrapper = new MigemoWrapper(pathDLL, pathDic);
                }
                catch {
                    return false;
                }
            }

            if(migemoWrapper != null && migemoWrapper.IsEnable) {
                try {
                    bool fStartWithNoPartial = strQuery.StartsWith("^");
                    if(fStartWithNoPartial && strQuery.Length > 1)
                        strQuery = strQuery.Substring(1);

                    string strPrefix = String.Empty;
                    if(fStartWithNoPartial || !fPartialMatch)
                        strPrefix = "^";

                    re = new Regex(strPrefix + migemoWrapper.QueryRegexStr(strQuery), RegexOptions.IgnoreCase);
                    return true;
                }
                catch {
                }
            }
            return false;
        }

        #endregion


        #region IPluginClient member

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            ReadSettings();
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode endCode) {
            if(migemoWrapper != null) {
                migemoWrapper.Dispose();
                migemoWrapper = null;
            }
        }

        public bool HasOption {
            get {
                return true;
            }
        }

        public void OnOption() {
            pluginServer.ExecuteCommand(Commands.SetModalState, true);
            try {
                using(MigemoOptionForm mof = new MigemoOptionForm(pathDLL, pathDic, fPartialMatch)) {
                    if(DialogResult.OK == mof.ShowDialog()) {
                        pathDLL = mof.pathDLL;
                        pathDic = mof.pathDic;
                        fPartialMatch = mof.fPartialMatch;

                        using(RegistryKey rkMigemo = Registry.CurrentUser.CreateSubKey(CONSTANTS.REGISTRY_PLUGINSETTINGS + "\\MigemoLoader")) {
                            if(rkMigemo != null) {
                                rkMigemo.SetValue("dll", pathDLL);
                                rkMigemo.SetValue("dic", pathDic);
                                rkMigemo.SetValue("PartialMatch", fPartialMatch ? 1 : 0);
                            }
                        }
                    }
                }
            }
            finally {
                pluginServer.ExecuteCommand(Commands.SetModalState, false);
            }
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public void OnShortcutKeyPressed(int index) {
        }

        #endregion


        public static void Uninstall() {
            using(RegistryKey rkPluginSetting = Registry.CurrentUser.OpenSubKey(CONSTANTS.REGISTRY_PLUGINSETTINGS, true)) {
                if(rkPluginSetting != null)
                    rkPluginSetting.DeleteSubKey("MigemoLoader", false);
            }
        }

        private static bool ReadSettings() {
            using(RegistryKey rkMigemo = Registry.CurrentUser.OpenSubKey(CONSTANTS.REGISTRY_PLUGINSETTINGS + "\\MigemoLoader", false)) {
                if(rkMigemo != null) {
                    string pathDLL = (string)rkMigemo.GetValue("dll");
                    string pathDic = (string)rkMigemo.GetValue("dic");
                    fPartialMatch = (int)rkMigemo.GetValue("PartialMatch", 0) == 1;

                    //if( File.Exists( pathDLL ) && File.Exists( pathDic ) )
                    //{
                    //    return true;
                    //}
                    MigemoLoader.pathDLL = pathDLL;
                    MigemoLoader.pathDic = pathDic;

                }
            }
            return false;
        }
    }
}

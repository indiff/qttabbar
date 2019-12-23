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

using System.Drawing;
using System.Globalization;
using System.Media;
using System.Windows.Forms;
using QTPlugin;
using QTPlugin.Interop;
using System.Collections.Generic;

namespace QuizoPlugins {
    /// <summary>
    /// Cut button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "indiff", Name = "关重复", Version = "1.0.0.0", Description = "关闭tab页上路径重复的文件夹")]
    public class CloseRepeatButton : IBarButton {
        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[0] };
            }

            this.pluginServer.SelectionChanged += pluginServer_SelectionChanged;
            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            pluginServer = null;
            shellBrowser = null;
        }

        public void OnShortcutKeyPressed(int index) {
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.CloseRepeatButton_large : Resource.CloseRepeatButton_small;
        }

        public void OnButtonClick() {
            ITab[] tabs = pluginServer.GetTabs();
            if ( null != tabs && tabs.Length > 0)
            {
                List<string> notdup = new List<string>();
                List<ITab> dups = new List<ITab>();
                for (int i = 0; i < tabs.Length; i++)
                {
                    if (notdup.Contains(tabs[i].Address.Path))
                    {
                        dups.Add(tabs[i]);
                    }
                    else
                    {
                        notdup.Add(tabs[i].Address.Path);
                    }

                    // MessageBox.Show(tabs[i].Address.Path);
                }

                if (dups.Count > 0)
                {
                    foreach (ITab tab in dups)
                    {
                        tab.Close();
                    }
                }  
            }
                         
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            Update();
        }

        private void Update() {
            Address[] addresses;
            if(pluginServer.TryGetSelection(out addresses)) {
                pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }

    /// <summary>
    /// Copy button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "indiff", Name = "排序", Version = "1.0.0.0", Description = "排序Tab")]
    public class SortButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[1] };
            }

            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
            this.pluginServer.SelectionChanged += pluginServer_SelectionChanged;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.Sort24 : Resource.Sort16;
        }

        public void OnButtonClick() {
            //  去重
            ITab[] tabs = pluginServer.GetTabs();
            if (null != tabs && tabs.Length > 0)
            {
                List<string> notdup = new List<string>();
                List<ITab> dups = new List<ITab>();
                for (int i = 0; i < tabs.Length; i++)
                {
                    if (notdup.Contains(tabs[i].Address.Path))
                    {
                        dups.Add(tabs[i]);
                    }
                    else
                    {
                        notdup.Add(tabs[i].Address.Path);
                    }

                    // MessageBox.Show(tabs[i].Address.Path);
                }

                if (dups.Count > 0)
                {
                    foreach (ITab tab in dups)
                    {
                        tab.Close();
                    }
                }
            }
            // 排序
            tabs = pluginServer.GetTabs();
            if (tabs.Length > 0)
            {
                List<string> sorts = new List<string>();
                for (int i = 0; i < tabs.Length; i++)
                {
                    sorts.Add(tabs[i].Text);
                }

                sorts.Sort();

                for (int i = 0; i < tabs.Length; i++)
                {
                    int idx = sorts.IndexOf(tabs[i].Text);
                    if (idx >= 0) {
                        tabs[i].Insert(idx);
                    }
                }
            }
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            Update();
        }

        private void Update() {
            Address[] addresses;
            if(pluginServer.TryGetSelection(out addresses)) {
                pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }



    static class StringResources {
        public static string[] ButtonNames;
        static StringResources() {
            if (  CultureInfo.CurrentCulture.Parent.Name.ToLower().StartsWith( "zh" )  )
            {
                ButtonNames = Resource.str_zh.Split(new char[] { ';' });
            }
            else if (CultureInfo.CurrentCulture.Parent.Name.ToLower().StartsWith("ja") )
            {
                ButtonNames = Resource.str_ja.Split(new char[] { ';' });
            }
            else {
                ButtonNames = Resource.str.Split(new char[] { ';' });
            }
        }
    }
}

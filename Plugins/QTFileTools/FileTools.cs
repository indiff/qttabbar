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

namespace QuizoPlugins {
    /// <summary>
    /// Cut button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Cut", Version = "1.0.0.0", Description = "Cut files")]
    public class CutButton : IBarButton {
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
            return fLarge ? Resource.CutButton_large : Resource.CutButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Cut, pluginServer.ExplorerHandle, shellBrowser);
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
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Copy", Version = "1.0.0.0", Description = "Copy files")]
    public class CopyButton : IBarButton {
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
            return fLarge ? Resource.CopyButton_large : Resource.CopyButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Copy, pluginServer.ExplorerHandle, null);
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
    /// Paste Button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Paste", Version = "1.0.0.0", Description = "Paste files")]
    public class PasteButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[2] };
            }
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
            return fLarge ? Resource.PasteButton_large : Resource.PasteButton_small;
        }

        public void OnButtonClick() {
            bool fFilesInClipboard = false;
            try {
                fFilesInClipboard = Clipboard.ContainsFileDropList();
            }
            catch {
            }

            if(fFilesInClipboard)
                FileOps.FileOperation(FileOpActions.Paste, pluginServer.ExplorerHandle, null);
            else
                SystemSounds.Beep.Play();
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
    }

    /// <summary>
    /// Delete button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Delete", Version = "1.0.0.0", Description = "Delete files")]
    public class DeleteButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[3] };
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
            return fLarge ? Resource.DeleteButton_large : Resource.DeleteButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Delete, pluginServer.ExplorerHandle, null);
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
    /// CopyTo button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Copy To Folder", Version = "1.0.0.0", Description = "Open copy-to-folder dialog")]
    public class CopyToButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[4] };
            }

            this.pluginServer.SelectionChanged += pluginServer_SelectionChanged;
            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            FileOps.FileOperation(FileOpActions.CopyTo, pluginServer.ExplorerHandle, null);
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
            return fLarge ? Resource.CopyToButton_large : Resource.CopyToButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.CopyTo, pluginServer.ExplorerHandle, null);
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
    /// MoveTo button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Move To Folder", Version = "1.0.0.0", Description = "Open move-to-folder dialog")]
    public class MoveToButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[5] };
            }

            this.pluginServer.SelectionChanged += pluginServer_SelectionChanged;
            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            FileOps.FileOperation(FileOpActions.MoveTo, pluginServer.ExplorerHandle, null);
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
            return fLarge ? Resource.MoveToButton_large : Resource.MoveToButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.MoveTo, pluginServer.ExplorerHandle, null);
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
    /// Undo Button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Undo", Version = "1.0.0.0", Description = "Undo operation")]
    public class UndoButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[6] };
            }
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
            return fLarge ? Resource.UndoButton_large : Resource.UndoButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Undo, pluginServer.ExplorerHandle, null);
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
    }

    /// <summary>
    /// Send up button
    /// </summary>
    [Plugin(PluginType.Background, Author = "Quizo", Name = "Send To Parent", Version = "1.0.0.1", Description = "Send files to parent folder. This copies when Ctrl key is down.")]
    public class SendToParentButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[7] };
            }

            this.pluginServer.SelectionChanged += pluginServer_SelectionChanged;
            this.pluginServer.NavigationComplete += pluginServer_NavigationComplete;
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            if(code != EndCode.Hidden)
                pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            if(!FileOps.MoveSelectedToParent(pluginServer))
                SystemSounds.Beep.Play();
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
            return fLarge ? Resource.SendToParentButton_large : Resource.SendToParentButton_small;
        }

        public void OnButtonClick() {
            if(!FileOps.MoveSelectedToParent(pluginServer))
                SystemSounds.Beep.Play();
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
            try {
                string currentPath = pluginServer.SelectedTab.Address.Path;

                Address[] addresses;
                if(pluginServer.TryGetSelection(out addresses)) {
                    bool fEnabled = currentPath != null && currentPath.Length > 3 && !currentPath.StartsWith("::") && addresses.Length > 0;

                    pluginServer.UpdateItem(this, fEnabled, false);
                }
            }
            catch {
                pluginServer.UpdateItem(this, false, false);
            }
        }
    }

    /// <summary>
    /// Properties button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Properties", Version = "1.0.0.1", Description = "Show file properties")]
    public class PropertiesButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out ResStr)) {
                ResStr = new string[] { StringResources.ButtonNames[8] };
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            FileOps.FileOperation(FileOpActions.Properties, pluginServer.ExplorerHandle, null);
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
            return fLarge ? Resource.PropertiesButton_large : Resource.PropertiesButton_small;
        }

        public void OnButtonClick() {
            Address[] addresses;
            if(pluginServer.TryGetSelection(out addresses)) {
                if(addresses.Length > 0)
                    FileOps.FileOperation(FileOpActions.Properties, pluginServer.ExplorerHandle, null);
                else
                    FileOps.ShowProperties(pluginServer);
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

    }


    static class StringResources {
        public static string[] ButtonNames;
        static StringResources() {
            if(CultureInfo.CurrentCulture.Parent.Name == "ja") {
                ButtonNames = Resource.str_ja.Split(new char[] { ';' });
            }
            else {
                ButtonNames = Resource.str.Split(new char[] { ';' });
            }
        }
    }
}

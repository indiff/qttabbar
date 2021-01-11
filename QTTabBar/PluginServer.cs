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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    public sealed partial class QTTabBarClass {
        internal sealed class PluginServer : IPluginServer, IDisposable {
            private Dictionary<string, string[]> dicLocalizingStrings;
            private QTPlugin.Interop.IShellBrowser shellBrowser;
            private QTTabBarClass tabBar;
            private Dictionary<string, Plugin> dicPluginInstances = new Dictionary<string, Plugin>();
            
            // todo: are these really necessary?
            internal Dictionary<string, string> dicFullNamesMenuRegistered_Sys = new Dictionary<string, string>();
            internal Dictionary<string, string> dicFullNamesMenuRegistered_Tab = new Dictionary<string, string>();

            public event PluginEventHandler ExplorerStateChanged;
            public event EventHandler MenuRendererChanged;
            public event EventHandler MouseEnter;
            public event EventHandler MouseLeave;
            public event PluginEventHandler NavigationComplete;
            public event PluginEventHandler PointedTabChanged;
            public event PluginEventHandler SelectionChanged;
            public event PluginEventHandler SettingsChanged;
            public event PluginEventHandler TabAdded;
            public event PluginEventHandler TabChanged;
            public event PluginEventHandler TabRemoved;

            public PluginServer(QTTabBarClass tabBar) {
                this.tabBar = tabBar;
                shellBrowser = (QTPlugin.Interop.IShellBrowser)this.tabBar.ShellBrowser.GetIShellBrowser();
                dicLocalizingStrings = new Dictionary<string, string[]>();
                foreach(string file in Config.Lang.PluginLangFiles) {
                    if(file.Length <= 0 || !File.Exists(file)) continue;
                    var dict = QTUtility.ReadLanguageFile(file);
                    if(dict == null) continue;
                    foreach(var pair in dict) {
                        dicLocalizingStrings[pair.Key] = pair.Value;
                    }
                }
                LoadStartupPlugins();
            }

            public bool AddApplication(string name, ProcessStartInfo startInfo) {
                return false;
            }

            public bool AddGroup(string groupName, string[] paths) {
                if(paths == null || paths.Length == 0) return false;
                GroupsManager.AddGroup(groupName, paths);
                return true;
            }

            internal void ClearEvents() {
                TabChanged = null;
                TabAdded = null;
                TabRemoved = null;
                NavigationComplete = null;
                SelectionChanged = null;
                ExplorerStateChanged = null;
                SettingsChanged = null;
                MouseEnter = null;
                PointedTabChanged = null;
                MouseLeave = null;
                MenuRendererChanged = null;
            }

            public void ClearFilterEngines() {
                FilterPlugin = null;
                FilterCorePlugin = null;
            }
            // 创建新标签 地址  索引 是否锁定  是否选中
            public bool CreateTab(Address address, int index, bool fLocked, bool fSelect) {
                using(IDLWrapper wrapper = new IDLWrapper(address)) {
                    address.ITEMIDLIST = wrapper.IDL;
                    address.Path = wrapper.Path;
                }
                if((address.ITEMIDLIST == null) || (address.ITEMIDLIST.Length <= 0)) {
                    return false;
                }
                QTabItem tab = new QTabItem(QTUtility2.MakePathDisplayText(address.Path, false), address.Path, tabBar.tabControl1);
                tab.NavigatedTo(address.Path, address.ITEMIDLIST, -1, false);
                tab.ToolTipText = QTUtility2.MakePathDisplayText(address.Path, true);
                tab.TabLocked = fLocked;
                if(index < 0) {
                    tabBar.AddInsertTab(tab);
                }
                else {
                    if(index > tabBar.tabControl1.TabCount) {
                        index = tabBar.tabControl1.TabCount;
                    }
                    tabBar.tabControl1.TabPages.Insert(index, tab);
                }
                if(fSelect) {
                    tabBar.tabControl1.SelectTab(tab);
                }
                return true;
            }

            public bool CreateWindow(Address address) {
                using(IDLWrapper wrapper = new IDLWrapper(address)) {
                    if(wrapper.Available) {
                        tabBar.OpenNewWindow(wrapper);
                        return true;
                    }
                }
                return false;
            }

            public void Dispose() {
                // todo check
                ClearEvents();
                foreach(Plugin plugin in dicPluginInstances.Values) {
                    if(plugin.PluginInformation != null) {
                        plugin.Close(EndCode.WindowClosed);
                    }
                }
                FilterPlugin = null;
                FilterCorePlugin = null;
                dicPluginInstances.Clear();
                tabBar = null;
                shellBrowser = null;
            }

            public bool ExecuteCommand(Commands command, object arg) {
                if(tabBar != null) {
                    IntPtr ptr;
                    switch(command) {
                        case Commands.GoBack:
                        case Commands.GoForward:
                            if(arg is int) {
                                return tabBar.NavigateToIndex(command == Commands.GoBack, (int)arg);
                            }
                            break;

                        case Commands.GoUpOneLevel:
                            tabBar.UpOneLevel();
                            return true;

                        case Commands.RefreshBrowser:
                            tabBar.Explorer.Refresh();
                            return true;

                        case Commands.CloseCurrentTab:
                            return tabBar.CloseTab(tabBar.CurrentTab);

                        case Commands.CloseLeft:
                        case Commands.CloseRight:
                            tabBar.CloseLeftRight(command == Commands.CloseLeft, -1);
                            return true;

                        case Commands.CloseAllButCurrent:
                            tabBar.CloseAllTabsExcept(tabBar.CurrentTab);
                            return true;

                        case Commands.CloseAllButOne: {
                                TabWrapper wrapper = arg as TabWrapper;
                                if(wrapper == null) break;
                                tabBar.CloseAllTabsExcept(wrapper.Tab);
                                return true;
                            }
                        case Commands.CloseWindow: // 关闭窗口 2 indiff
                           /* using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root))
                            {
                                string[] list = (from QTabItem item2 in tabControl1.TabPages
                                                 where item2.TabLocked
                                                 select item2.CurrentPath).ToArray();

                                MessageBox.Show(String.Join(",", list));
                                QTUtility2.WriteRegBinary(list, "TabsLocked", key);
                            }*/
                            WindowUtils.CloseExplorer(tabBar.ExplorerHandle, 2);
                            return true;

                        case Commands.UndoClose:
                            tabBar.RestoreLastClosed();
                            return true;

                        case Commands.BrowseFolder:
                            tabBar.ChooseNewDirectory();
                            return true;

                        case Commands.ToggleTopMost:
                            tabBar.ToggleTopMost();
                            TryCallButtonBar(bbar => bbar.RefreshButtons());
                            return true;

                        case Commands.FocusFileList:
                            tabBar.listView.SetFocus();
                            return true;

                        case Commands.OpenTabBarOptionDialog:
                            OptionsDialog.Open();
                            return true;

                        case Commands.OpenButtonBarOptionDialog:
                            OptionsDialog.Open(); // todo: open at bbar page
                            return true;

                        case Commands.IsFolderTreeVisible:
                            return tabBar.ShellBrowser.IsFolderTreeVisible();

                        case Commands.IsButtonBarVisible:
                            return InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr);

                        case Commands.ShowFolderTree:
                            if(!QTUtility.IsXP || !(arg is bool)) {
                                break;
                            }
                            tabBar.ShowFolderTree((bool)arg);
                            return true;

                        case Commands.ShowButtonBar:
                            if(!InstanceManager.TryGetButtonBarHandle(tabBar.ExplorerHandle, out ptr)) {
                            }
                            break;

                        case Commands.MD5:
                            if(!(arg is string[])) {
                                break;
                            }
                            if(md5Form == null) {
                                md5Form = new FileHashComputerForm();
                            }
                            if(md5Form.InvokeRequired) {
                                md5Form.Invoke(new FormMethodInvoker(ShowMD5FormCore), new object[] { arg });
                            }
                            else {
                                ShowMD5FormCore(arg);
                            }
                            return true;

                        case Commands.ShowProperties: {
                                if((arg == null) || !(arg is Address)) {
                                    break;
                                }
                                Address address = (Address)arg;
                                using(IDLWrapper wrapper = new IDLWrapper(address)) {
                                    if(!wrapper.Available) break;
                                    ShellMethods.ShowProperties(wrapper.IDL, tabBar.ExplorerHandle);
                                    return true;
                                }
                            }
                        case Commands.SetModalState:
                            if(((arg == null) || !(arg is bool)) || !((bool)arg)) {
                                tabBar.NowModalDialogShown = false;
                                break;
                            }
                            tabBar.NowModalDialogShown = true;
                            break;

                        case Commands.SetSearchBoxStr:
                            return arg != null && arg is string &&
                                    TryCallButtonBar(bbar => bbar.SetSearchBarText((string)arg));

                        case Commands.ReorderTabsByName:
                        case Commands.ReorderTabsByPath:
                        case Commands.ReorderTabsByActv:
                        case Commands.ReorderTabsRevers:
                            if(tabBar.tabControl1.TabCount > 1) {
                                bool fDescending = ((arg != null) && (arg is bool)) && ((bool)arg);
                                tabBar.ReorderTab(((int)command) - 0x18, fDescending);
                            }
                            break;
                    }
                }
                return false;
            }

            public ProcessStartInfo[] GetApplications(string name) {
                return null;
            }

            public string[] GetGroupPaths(string groupName) {
                Group g = GroupsManager.GetGroup(groupName);
                return g == null ? null : g.Paths.ToArray();
            }

            public ToolStripRenderer GetMenuRenderer() {
                return DropDownMenuBase.CurrentRenderer;
            }

            public ITab[] GetTabs() {
                return (from QTabItem item in tabBar.tabControl1.TabPages
                        select (ITab)(new TabWrapper(item, tabBar))).ToArray();
            }

            public ITab HitTest(Point pnt) {
                QTabItem tabMouseOn = tabBar.tabControl1.GetTabMouseOn();
                return tabMouseOn != null ? new TabWrapper(tabMouseOn, tabBar) : null;
            }

            private string InstanceToFullName(IPluginClient pluginClient, bool fTypeFullName) {
                Plugin plugin = dicPluginInstances.Values.FirstOrDefault(plugin1 => plugin1.Instance == pluginClient);
                return plugin == null
                        ? null
                        : fTypeFullName
                                ? plugin.PluginInformation.TypeFullName
                                : plugin.PluginInformation.PluginID;
            }

            public bool IsPluginInstantialized(string pluginID) {
                return dicPluginInstances.ContainsKey(pluginID);
            }

            public Plugin Load(PluginInformation pi, PluginAssembly pa) {
                try {
                    if(pa == null && !PluginManager.GetAssembly(pi.Path, out pa)) {
                        return null;
                    }
                    Plugin plugin = pa.Load(pi.PluginID);
                    if(plugin == null) return null;
                    dicPluginInstances[pi.PluginID] = plugin;
                    plugin.Instance.Open(this, shellBrowser);
                    return plugin;
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, IntPtr.Zero, pi.Name, "Loading plugin.");
                    QTUtility2.MakeErrorLog(exception);
                }
                return null;
            }

            private void LoadStartupPlugins() {
                foreach(PluginInformation information in PluginManager.PluginInformations.Where(information => information.Enabled)) {
                    if(information.PluginType == PluginType.Background) {
                        Plugin plugin = Load(information, null);
                        if(plugin != null) {
                            if(FilterPlugin == null) {
                                FilterPlugin = plugin.Instance as IFilter;
                            }
                            if(FilterCorePlugin == null) {
                                FilterCorePlugin = plugin.Instance as IFilterCore;
                            }
                        }
                        else {
                            information.Enabled = false;
                        }
                    }
                    else if(information.PluginType != PluginType.Static && Load(information, null) == null) {
                        information.Enabled = false;
                    }
                }
            }

            public void OnExplorerStateChanged(ExplorerWindowActions windowAction) {
                if(ExplorerStateChanged != null) {
                    ExplorerStateChanged(this, new PluginEventArgs(windowAction));
                }
            }

            // todo: Menu Renderer does not change anymore.
            public void OnMenuRendererChanged() {
                if(MenuRendererChanged != null) {
                    MenuRendererChanged(this, EventArgs.Empty);
                }
            }

            public void OnMouseEnter() {
                if(MouseEnter != null) {
                    MouseEnter(this, EventArgs.Empty);
                }
            }

            public void OnMouseLeave() {
                if(MouseLeave != null) {
                    MouseLeave(this, EventArgs.Empty);
                }
            }

            public void OnNavigationComplete(int index, byte[] idl, string path) {
                if(NavigationComplete != null) {
                    NavigationComplete(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnPointedTabChanged(int index, byte[] idl, string path) {
                if(PointedTabChanged != null) {
                    PointedTabChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnSelectionChanged(int index, byte[] idl, string path) {
                if(SelectionChanged != null) {
                    SelectionChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnSettingsChanged(int iType) {
                if(SettingsChanged != null) {
                    SettingsChanged(this, new PluginEventArgs(iType, new Address()));
                }
            }

            public void OnTabAdded(int index, byte[] idl, string path) {
                if(TabAdded != null) {
                    TabAdded(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnTabChanged(int index, byte[] idl, string path) {
                if(TabChanged != null) {
                    TabChanged(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OnTabRemoved(int index, byte[] idl, string path) {
                if(TabRemoved != null) {
                    TabRemoved(this, new PluginEventArgs(index, new Address(idl, path)));
                }
            }

            public void OpenGroup(string[] groupNames) {
                foreach(string str in groupNames) {
                    tabBar.OpenGroup(str, false);
                }
            }

            public void RefreshPlugins() {
                ClearFilterEngines();
                foreach(PluginInformation information in PluginManager.PluginInformations) {
                    if(!information.Enabled) {
                        UnloadPluginInstance(information.PluginID, EndCode.Unloaded);
                    }
                    else if(information.PluginType == PluginType.Background) {
                        Plugin plugin;
                        if(!TryGetPlugin(information.PluginID, out plugin)) {
                            plugin = Load(information, null);
                        }
                        if(plugin != null) {
                            if(FilterPlugin == null) {
                                FilterPlugin = plugin.Instance as IFilter;
                            }
                            if(FilterCorePlugin == null) {
                                FilterCorePlugin = plugin.Instance as IFilterCore;
                            }
                        }
                        else {
                            information.Enabled = false;
                        }
                    }
                    else if(information.PluginType == PluginType.BackgroundMultiple) {
                        if(!IsPluginInstantialized(information.PluginID) && Load(information, null) == null) {
                            information.Enabled = false;
                        }
                    }
                }
            }

            public void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister) {
                foreach(Plugin plugin in dicPluginInstances.Values.Where(plugin => plugin.Instance == pluginClient)) {
                    if(fRegister) {
                        if((menuType & MenuType.Bar) == MenuType.Bar) {
                            dicFullNamesMenuRegistered_Sys[plugin.PluginInformation.PluginID] = menuText;
                        }
                        if((menuType & MenuType.Tab) == MenuType.Tab) {
                            dicFullNamesMenuRegistered_Tab[plugin.PluginInformation.PluginID] = menuText;
                        }
                    }
                    else {
                        if((menuType & MenuType.Bar) == MenuType.Bar) {
                            dicFullNamesMenuRegistered_Sys.Remove(plugin.PluginInformation.PluginID);
                        }
                        if((menuType & MenuType.Tab) == MenuType.Tab) {
                            dicFullNamesMenuRegistered_Tab.Remove(plugin.PluginInformation.PluginID);
                        }
                    }
                    break;
                }
            }

            public bool RemoveApplication(string name) {
                return false;
            }

            internal void RemoveEvents(IPluginClient pluginClient) {
                if(TabChanged != null) {
                    foreach(PluginEventHandler handler in TabChanged.GetInvocationList()) {
                        if(handler.Target == pluginClient) {
                            TabChanged = (PluginEventHandler)Delegate.Remove(TabChanged, handler);
                        }
                    }
                }
                if(TabAdded != null) {
                    foreach(PluginEventHandler handler2 in TabAdded.GetInvocationList()) {
                        if(handler2.Target == pluginClient) {
                            TabAdded = (PluginEventHandler)Delegate.Remove(TabAdded, handler2);
                        }
                    }
                }
                if(TabRemoved != null) {
                    foreach(PluginEventHandler handler3 in TabRemoved.GetInvocationList()) {
                        if(handler3.Target == pluginClient) {
                            TabRemoved = (PluginEventHandler)Delegate.Remove(TabRemoved, handler3);
                        }
                    }
                }
                if(NavigationComplete != null) {
                    foreach(PluginEventHandler handler4 in NavigationComplete.GetInvocationList()) {
                        if(handler4.Target == pluginClient) {
                            NavigationComplete = (PluginEventHandler)Delegate.Remove(NavigationComplete, handler4);
                        }
                    }
                }
                if(SelectionChanged != null) {
                    foreach(PluginEventHandler handler5 in SelectionChanged.GetInvocationList()) {
                        if(handler5.Target == pluginClient) {
                            SelectionChanged = (PluginEventHandler)Delegate.Remove(SelectionChanged, handler5);
                        }
                    }
                }
                if(ExplorerStateChanged != null) {
                    foreach(PluginEventHandler handler6 in ExplorerStateChanged.GetInvocationList()) {
                        if(handler6.Target == pluginClient) {
                            ExplorerStateChanged = (PluginEventHandler)Delegate.Remove(ExplorerStateChanged, handler6);
                        }
                    }
                }
                if(SettingsChanged != null) {
                    foreach(PluginEventHandler handler7 in SettingsChanged.GetInvocationList()) {
                        if(handler7.Target == pluginClient) {
                            SettingsChanged = (PluginEventHandler)Delegate.Remove(SettingsChanged, handler7);
                        }
                    }
                }
                if(MouseEnter != null) {
                    foreach(EventHandler handler8 in MouseEnter.GetInvocationList()) {
                        if(handler8.Target == pluginClient) {
                            MouseEnter = (EventHandler)Delegate.Remove(MouseEnter, handler8);
                        }
                    }
                }
                if(PointedTabChanged != null) {
                    foreach(PluginEventHandler handler9 in PointedTabChanged.GetInvocationList()) {
                        if(handler9.Target == pluginClient) {
                            PointedTabChanged = (PluginEventHandler)Delegate.Remove(PointedTabChanged, handler9);
                        }
                    }
                }
                if(MouseLeave != null) {
                    foreach(EventHandler handler10 in MouseLeave.GetInvocationList()) {
                        if(handler10.Target == pluginClient) {
                            MouseLeave = (EventHandler)Delegate.Remove(MouseLeave, handler10);
                        }
                    }
                }
                if(MenuRendererChanged != null) {
                    foreach(EventHandler handler11 in MenuRendererChanged.GetInvocationList()) {
                        if(handler11.Target == pluginClient) {
                            MenuRendererChanged = (EventHandler)Delegate.Remove(MenuRendererChanged, handler11);
                        }
                    }
                }
            }

            public bool RemoveGroup(string groupName) {
                return GroupsManager.RemoveGroup(groupName);
            }

            public bool TryGetLocalizedStrings(IPluginClient pluginClient, int count, out string[] arrStrings) {
                string key = InstanceToFullName(pluginClient, true);
                if(key.Length > 0 && dicLocalizingStrings.TryGetValue(key, out arrStrings) && arrStrings != null && arrStrings.Length == count) {
                    return true;
                }
                arrStrings = null;
                return false;
            }

            public bool TryGetPlugin(string pluginID, out Plugin plugin) {
                return dicPluginInstances.TryGetValue(pluginID, out plugin);
            }

            public bool TryGetSelection(out Address[] adSelectedItems) {
                string str;
                return tabBar.ShellBrowser.TryGetSelection(out adSelectedItems, out str, false);
            }

            public bool TrySetSelection(Address[] itemsToSelect, bool fDeselectOthers) {
                return tabBar.ShellBrowser.TrySetSelection(itemsToSelect, null, fDeselectOthers);
            }

            public void UpdateItem(IBarButton barItem, bool fEnabled, bool fRefreshImage) {
                string pid = InstanceToFullName(barItem, false);
                if(pid.Length > 0) TryCallButtonBar(bbar => 
                    bbar.UpdatePluginItem(pid, barItem, fEnabled, fRefreshImage));
            }

            public void UnloadPluginInstance(string pluginID, EndCode code) {
                Plugin plugin;
                dicFullNamesMenuRegistered_Sys.Remove(pluginID);
                dicFullNamesMenuRegistered_Tab.Remove(pluginID);
                if(!dicPluginInstances.TryGetValue(pluginID, out plugin)) return;
                RemoveEvents(plugin.Instance);
                dicPluginInstances.Remove(pluginID);
                plugin.Close(code);
            }

            public IntPtr ExplorerHandle {
                get {
                    return tabBar.ExplorerHandle;
                }
            }

            public IFilter FilterPlugin { get; private set; }

            public IFilterCore FilterCorePlugin { get; private set; }

            public string[] Groups {
                get {
                    return GroupsManager.Groups.Select(g => g.Name).ToArray();
                }
            }

            public IntPtr Handle {
                get {
                    return tabBar.IsHandleCreated ? tabBar.Handle : IntPtr.Zero;
                }
            }

            public IEnumerable<Plugin> Plugins {
                get {
                    return new List<Plugin>(dicPluginInstances.Values);
                }
            }

            public ITab SelectedTab {
                get {
                    return tabBar.CurrentTab != null ? new TabWrapper(tabBar.CurrentTab, tabBar) : null;
                }
                set {
                    TabWrapper wrapper = value as TabWrapper;
                    if((wrapper.Tab != null) && tabBar.tabControl1.TabPages.Contains(wrapper.Tab)) {
                        tabBar.tabControl1.SelectTab(wrapper.Tab);
                    }
                }
            }

            public bool SelectionChangedAttached {
                get {
                    return (SelectionChanged != null);
                }
            }

            public TabBarOption TabBarOption {
                get {
                    return QTUtility.GetTabBarOption();
                }
                set {
                    QTUtility.SetTabBarOption(value, tabBar);
                }
            }

            internal sealed class TabWrapper : ITab {
                private QTabItem tab;
                private QTTabBarClass tabBar;

                public TabWrapper(QTabItem tab, QTTabBarClass tabBar) {
                    this.tab = tab;
                    this.tabBar = tabBar;
                    this.tab.Closed += tab_Closed;
                }

                public bool Browse(Address address) {
                    if(tab != null) {
                        tabBar.tabControl1.SelectTab(tab);
                        using(IDLWrapper wrapper = new IDLWrapper(address)) {
                            return tabBar.ShellBrowser.Navigate(wrapper) == 0;
                        }
                    }
                    return false;
                }

                public bool Browse(bool fBack) {
                    if(tab != null) {
                        tabBar.tabControl1.SelectTab(tab);
                        return tabBar.NavigateCurrentTab(fBack);
                    }
                    return false;
                }

                public void Clone(int index, bool fSelect) {
                    if(tab != null) {
                        tabBar.CloneTabButton(tab, null, fSelect, index);
                    }
                }

                public bool Close() {
                    return (((tab != null) && (tabBar.tabControl1.TabCount > 1)) && tabBar.CloseTab(tab, true));
                }

                public Address[] GetBraches() {
                    if(tab == null) {
                        return null;
                    }
                    return (from data in tab.Branches
                            where data.IDL != null || !string.IsNullOrEmpty(data.Path)
                            select new Address(data.IDL, data.Path)).ToArray();
                }

                public Address[] GetHistory(bool fBack) {
                    if(tab == null) {
                        return null;
                    }
                    IEnumerable<LogData> logs = tab.GetLogs(fBack);
                    return logs.Select(data => new Address(data.IDL, data.Path)).ToArray();
                }

                public bool Insert(int index) {
                    if(((tab != null) && (-1 < index)) && (index < (tabBar.tabControl1.TabCount + 1))) {
                        int indexSource = tabBar.tabControl1.TabPages.IndexOf(tab);
                        if(indexSource > -1) {
                            tabBar.tabControl1.TabPages.Relocate(indexSource, index);
                            return true;
                        }
                    }
                    return false;
                }

                private void tab_Closed(object sender, EventArgs e) {
                    tab.Closed -= tab_Closed;
                    tab = null;
                    tabBar = null;
                }

                public Address Address {
                    get {
                        if(tab == null) {
                            return new Address();
                        }
                        Address address = new Address(tab.CurrentIDL, tab.CurrentPath);
                        if((address.ITEMIDLIST == null) && !string.IsNullOrEmpty(address.Path)) {
                            IDLWrapper wrapper;
                            IntPtr pidl = PInvoke.ILCreateFromPath(address.Path);
                            if(pidl != IntPtr.Zero) {
                                address = new Address(pidl, tab.CurrentPath);
                                PInvoke.CoTaskMemFree(pidl);
                                return address;
                            }
                            if(!IDLWrapper.TryGetCache(address.Path, out wrapper)) {
                                return address;
                            }
                            using(wrapper) {
                                address.ITEMIDLIST = wrapper.IDL;
                            }
                        }
                        return address;
                    }
                }

                public int Index {
                    get {
                        if(tab != null) {
                            return tabBar.tabControl1.TabPages.IndexOf(tab);
                        }
                        return -1;
                    }
                }

                // 设置为锁定
                public bool Locked {
                    get {
                        return ((tab != null) && tab.TabLocked);
                    }
                    set {
                        if(tab != null) {
                            tab.TabLocked = value;
                            tabBar.tabControl1.Refresh();
                        }
                    }
                }

                public bool Selected {
                    get {
                        return ((tab != null) && (tabBar.CurrentTab == tab));
                    }
                    set {
                        if((tab != null) && value) {
                            tabBar.tabControl1.SelectTab(tab);
                        }
                    }
                }

                public string SubText {
                    get {
                        if(tab != null) {
                            return tab.Comment;
                        }
                        return string.Empty;
                    }
                    set {
                        if((tab != null) && (value != null)) {
                            tab.Comment = value;
                            tab.RefreshRectangle();
                            tabBar.tabControl1.Refresh();
                        }
                    }
                }

                public QTabItem Tab {
                    get {
                        return tab;
                    }
                }

                public string Text {
                    get {
                        return tab != null ? tab.Text : string.Empty;
                    }
                    set {
                        if(tab != null && !string.IsNullOrEmpty(value)) {
                            tab.Text = value;
                        }
                    }
                }
            }
        }

        public static Dictionary<String,String[]> testQTUtilityReadLanguageFile(string path )
        {
            return QTUtility.ReadLanguageFile( path );
        }
    }
}

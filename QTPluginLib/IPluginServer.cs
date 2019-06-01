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
using System.Drawing;
using System.Windows.Forms;

namespace QTPlugin {
    public interface IPluginServer {
        event PluginEventHandler ExplorerStateChanged;

        event EventHandler MenuRendererChanged;

        event EventHandler MouseEnter;

        event EventHandler MouseLeave;

        event PluginEventHandler NavigationComplete;

        event PluginEventHandler PointedTabChanged;

        event PluginEventHandler SelectionChanged;

        event PluginEventHandler SettingsChanged;

        event PluginEventHandler TabAdded;

        event PluginEventHandler TabChanged;

        event PluginEventHandler TabRemoved;

        bool AddApplication(string name, ProcessStartInfo startInfo);
        bool AddGroup(string groupName, string[] paths);
        bool CreateTab(Address address, int index, bool fLocked, bool fSelect);
        bool CreateWindow(Address address);
        bool ExecuteCommand(Commands command, object arg);
        ProcessStartInfo[] GetApplications(string name);
        string[] GetGroupPaths(string groupName);
        ToolStripRenderer GetMenuRenderer();
        ITab[] GetTabs();
        ITab HitTest(Point pnt);
        void OpenGroup(string[] groupNames);
        void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister);
        bool RemoveApplication(string name);
        bool RemoveGroup(string groupName);
        bool TryGetLocalizedStrings(IPluginClient pluginClient, int count, out string[] arrStrings);
        bool TryGetSelection(out Address[] selectedItems);
        bool TrySetSelection(Address[] itemsToSelect, bool fDeselectOthers);
        void UpdateItem(IBarButton barItem, bool fEnabled, bool fRefreshImage);

        IntPtr ExplorerHandle { get; }

        string[] Groups { get; }

        IntPtr Handle { get; }

        ITab SelectedTab { get; set; }

        TabBarOption TabBarOption { get; set; }
    }
}

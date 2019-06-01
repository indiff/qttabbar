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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace QTTabBarLib {

    internal class Group {
        public string Name { get; set; }
        public Keys ShortcutKey { get; set; }
        public List<string> Paths { get; private set; }
        public bool Startup { get; set; }

        public Group(string name, Keys shortcutKey, bool startup, List<string> paths) {
            Name = name;
            ShortcutKey = shortcutKey;
            Paths = paths;
            Startup = startup;
        }
    }

    static class GroupsManager {
        private static List<Group> groupList = new List<Group>();
        private static Dictionary<string, Group> groupDict = new Dictionary<string, Group>();

        public static int GroupCount {
            get { return groupList.Count; }
        }

        public static IEnumerable<Group> Groups {
            get { return groupList; }
            set {
                groupList = value.ToList();
                groupDict = groupList.ToDictionary(g => g.Name, g => g);
                SaveGroups();
            }
        }

        public static Group GetGroup(string name) {
            Group group;
            groupDict.TryGetValue(name, out group);
            return group;
        }

        public static void LoadGroups() {
            groupList = new List<Group>();
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + RegConst.Groups)) {
                int i = 0;
                while(true) {
                    using(RegistryKey gkey = key.OpenSubKey("" + i++)) {
                        if(gkey == null) break;
                        try {
                            string name = gkey.GetValue("") as string;
                            if(name == null) continue;
                            Keys shortcut = (Keys)gkey.GetValue("key", Keys.None);
                            bool startup = gkey.GetValue("startup") != null;                            
                            List<string> paths = new List<string>();
                            int j = 0;
                            while(true) {
                                string path = gkey.GetValue("" + j++) as string;
                                if(path == null) break;
                                paths.Add(path);
                            }
                            groupList.Add(new Group(name, shortcut, startup, paths));
                        }
                        catch {
                        }
                    }
                }
            }
            groupDict = groupList.ToDictionary(g => g.Name, g => g);
        }

        public static void SaveGroups() {
            Registry.CurrentUser.DeleteSubKeyTree(RegConst.Root + RegConst.Groups);
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + RegConst.Groups)) {
                for(int i = 0; i < groupList.Count; i++) {
                    Group g = groupList[i];
                    using(RegistryKey gkey = key.CreateSubKey("" + i)) {
                        gkey.SetValue("", g.Name);
                        if(g.ShortcutKey != Keys.None) gkey.SetValue("key", (int)g.ShortcutKey);
                        if(g.Startup) gkey.SetValue("startup", "");
                        for(int j = 0; j < g.Paths.Count; j++) {
                            gkey.SetValue("" + j, g.Paths[j]);
                        }
                    }
                }
            }
            InstanceManager.LocalBBarBroadcast(bbar => bbar.RefreshButtons());
            InstanceManager.StaticBroadcast(() => {
                LoadGroups();
                InstanceManager.LocalBBarBroadcast(bbar => bbar.RefreshButtons());
            });
            // todo: desktop
        }

        public static void AddGroup(string key, IEnumerable<string> paths) {
            if(groupDict.ContainsKey(key)) return;
            Group g = new Group(key, Keys.None, false, paths.ToList());
            groupList.Add(g);
            groupDict[key] = g;
            SaveGroups();
        }

        public static bool RemoveGroup(string groupName) {
            if(!groupDict.ContainsKey(groupName)) return false;
            groupList.Remove(groupDict[groupName]);
            groupDict.Remove(groupName);
            SaveGroups();
            return true;
        }

        public static void HandleReorder(IEnumerable<ToolStripItem> items) {
            Groups = items.Select(item => groupDict[item.Text]);
            SaveGroups();
        }
    }
}

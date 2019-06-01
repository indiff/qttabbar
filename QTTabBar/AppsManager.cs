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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal sealed class UserApp {
        public string Name;
        public string Args;
        public Keys ShortcutKey;
        public string Path;
        public string WorkingDir;
        public int ChildrenCount;

        public bool IsFolder {
            get { return ChildrenCount != -1; }
        }

        public UserApp(string name, string path, string args, string workingDir, Keys shortcutKey) {
            Name = name ?? "";
            Args = args ?? "";
            ShortcutKey = shortcutKey;
            Path = path ?? "";
            WorkingDir = workingDir ?? "";
            ChildrenCount = -1;
        }

        public UserApp(string name, int childrenCount = 0) {
            Name = name ?? "";
            ChildrenCount = childrenCount;
        }
    }

    internal static class AppsManager {
        private static readonly string[] strVariables = { "%cd%", "%c%", "%d%", "%f%", "%s%" };

        private static readonly Regex[] reVariables = strVariables.Select(
                s => new Regex(s, RegexOptions.Compiled | RegexOptions.IgnoreCase)).ToArray();

        private static List<UserApp> appList;

        public static IEnumerable<UserApp> UserApps {
            get { return appList; }
            set {
                appList = value.ToList();
                SaveApps();
            }
        }

        // These functions convert the app list between a sequential list (convenient for storage and iteration) 
        // and a nested structure (required for interface).
        public static IEnumerable<T> BuildNestedStructure<T>(Converter<UserApp, T> makeAppNode, Func<string, T[], T> makeFolderNode) where T : class {
            return BuildNestedStructure(new RefInt(), int.MaxValue, makeAppNode, makeFolderNode);
        }
        
        // Can't use "ref" parameters in yield return blocks.
        // Using the RefInt class (which is a reference type) is a workaround.
        // The recursive calls must be evaluated immediately!  Lazy evaluation will cause problems.
        private class RefInt {
            public int i;
        }
        private static IEnumerable<T> BuildNestedStructure<T>(RefInt r, int max, Converter<UserApp, T> makeAppNode, Func<string, T[], T> makeFolderNode) where T : class {
            for(int j = 0; j < max && r.i < appList.Count; j++) {
                UserApp app = appList[r.i++];
                T node;
                if(app.IsFolder) {
                    var children = BuildNestedStructure(r, app.ChildrenCount, makeAppNode, makeFolderNode);
                    node = makeFolderNode(app.Name, children.ToArray());
                }
                else {
                    node = makeAppNode(app);
                }
                if(node != null) yield return node;
            }
        }

        public static void SetUserAppsFromNestedStructure<T>(IEnumerable<T> root, Converter<T, UserApp> appConvert, Converter<T, IEnumerable<T>> getChildren) {
            int dummy;
            List<UserApp> apps = new List<UserApp>();
            ListFromNestedStructure(apps, out dummy, root, appConvert, getChildren);
            UserApps = apps;
        }

        private static void ListFromNestedStructure<T>(ICollection<UserApp> outList, out int levelCount, IEnumerable<T> root, Converter<T, UserApp> appConvert, Converter<T, IEnumerable<T>> getChildren) {
            levelCount = 0;
            foreach(T node in root) {
                ++levelCount;
                UserApp app = appConvert(node);
                outList.Add(app);
                var children = getChildren(node);
                if(children != null) {
                    ListFromNestedStructure(outList, out app.ChildrenCount, children, appConvert, getChildren);
                }
            }
        }

        public static void LoadApps() {
            appList = new List<UserApp>();
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + RegConst.Apps)) {
                int i = 0;
                while(true) {
                    using(RegistryKey akey = key.OpenSubKey("" + i++)) {
                        if(akey == null) break;
                        try {
                            string name = akey.GetValue("") as string;
                            if(name == null) continue;
                            int children = (int)akey.GetValue("children", -1);
                            if(children != -1) {
                                appList.Add(new UserApp(name, children));
                            }
                            else {
                                string path = (string)akey.GetValue("path", "");
                                string args = (string)akey.GetValue("args", "");
                                string wdir = (string)akey.GetValue("wdir", "");
                                Keys shortcut = (Keys)akey.GetValue("key", Keys.None);
                                appList.Add(new UserApp(name, path, args, wdir, shortcut));
                            }
                        }
                        catch {
                        }
                    }
                }
            }
        }

        public static void SaveApps() {
            Registry.CurrentUser.DeleteSubKeyTree(RegConst.Root + RegConst.Apps);
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + RegConst.Apps)) {
                for(int i = 0; i < appList.Count; i++) {
                    UserApp a = appList[i];
                    using(RegistryKey akey = key.CreateSubKey("" + i)) {
                        akey.SetValue("", a.Name);
                        if(a.IsFolder) {
                            akey.SetValue("children", a.ChildrenCount);
                        }
                        else {
                            akey.SetValue("path", a.Path);
                            akey.SetValue("args", a.Args);
                            akey.SetValue("wdir", a.WorkingDir);
                            if(a.ShortcutKey != Keys.None) akey.SetValue("key", (int)a.ShortcutKey);
                        }
                    }
                }
            }
            InstanceManager.StaticBroadcast(LoadApps);
        }

        public static void Execute(UserApp app, ShellBrowserEx shellBrowser) {
            // todo validate app
            string pathCurrent;
            using(IDLWrapper wrapper = shellBrowser.GetShellPath()) {
                pathCurrent = wrapper.Path;
            }
            Address[] selection;
            shellBrowser.TryGetSelection(out selection, false);

            List<string> lstFiles = new List<string>();
            List<string> lstDirs = new List<string>();
            if(selection != null) {
                foreach(Address address in selection) {
                    using(IDLWrapper wrapper = new IDLWrapper(address.ITEMIDLIST)) {
                        if(!wrapper.Available || !wrapper.HasPath) continue;
                        (wrapper.IsFileSystemFile ? lstFiles : lstDirs).Add(address.Path.TrimEnd('\\').Enquote());
                    }
                }
            }

            string strFiles = lstFiles.StringJoin(" ");
            string strDirs = lstDirs.StringJoin(" ");
            string strBoth = (strFiles + " " + strDirs).Trim();
            pathCurrent = Directory.Exists(pathCurrent) ? pathCurrent.TrimEnd('\\').Enquote() : "";
            string args = app.Args;
            string work = app.WorkingDir;
            string path = app.Path;

            string[] variableValues = {
                    strDirs.Length > 0 ? strDirs : pathCurrent,     // %cd%
                    pathCurrent,                                    // %c%
                    strDirs,                                        // %d%
                    strFiles,                                       // %f%
                    strBoth                                         // %s%
            };
            for(int i = 0; i < variableValues.Length; i++) {
                args = reVariables[i].Replace(args, variableValues[i]);
            }
            variableValues = new string[] {
                    lstDirs.Count > 0 ? lstDirs[0] : pathCurrent,   // %cd%
                    pathCurrent,                                    // %c%
                    lstDirs.Count > 0 ? lstDirs[0] : "",            // %d%
                    "",                                             // %f%
                    lstDirs.Count > 0 ? lstDirs[0] : ""             // %s%
            };
            for(int i = 0; i < variableValues.Length; i++) {
                work = reVariables[i].Replace(work, variableValues[i]);
            }

            const int SW_SHOWNORMAL = 1;
            const int SEE_MASK_IDLIST = 0x00000004;
            const int SEE_MASK_DOENVSUBST = 0x00000200; // Expand any environment variables specified in the string given by the lpDirectory or lpFile 
            const int SEE_MASK_ASYNCOK = 0x00100000;
            const int SEE_MASK_FLAG_LOG_USAGE = 0x04000000;

            // Open NameSpace folder.
            if(path.StartsWith(IDLWrapper.INDICATOR_NAMESPACE)) {
                using(IDLWrapper idlw = new IDLWrapper(path)) {
                    if(idlw.Available) {
                        SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO {
                            cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO)),
                            nShow = SW_SHOWNORMAL,
                            fMask = SEE_MASK_IDLIST,
                            lpIDList = idlw.PIDL,
                            hwnd = shellBrowser.GetExplorerHandle()
                        };
                        PInvoke.ShellExecuteEx(ref sei);
                        return;
                    }
                }
            }
            else {

                // check whether target exists if link
                using(IDLWrapper idlw = new IDLWrapper(path)) {
                    if(idlw.IsLinkToDeadFolder) return;
                }

                SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO {
                    cbSize = Marshal.SizeOf(typeof(SHELLEXECUTEINFO)),
                    nShow = SW_SHOWNORMAL,
                    fMask = SEE_MASK_DOENVSUBST | SEE_MASK_FLAG_LOG_USAGE | SEE_MASK_ASYNCOK,
                    hwnd = shellBrowser.GetExplorerHandle()
                };

                try {
                    sei.lpFile = Marshal.StringToCoTaskMemUni(path);

                    // Arguments
                    if(!string.IsNullOrEmpty(args)) {
                        sei.lpParameters = Marshal.StringToCoTaskMemUni(args);
                    }

                    // Working directory
                    if(!string.IsNullOrEmpty(work)) {
                        work = work.Trim(new char[] { '"', '\'' });
                        sei.lpDirectory = Marshal.StringToCoTaskMemUni(work);
                    }
                    else {
                        // "::" will cause an exception in Path.GetDirectoryName
                        if(QTUtility2.IsExecutable(Path.GetExtension(path)) &&
                                !path.StartsWith(IDLWrapper.INDICATOR_NAMESPACE)) {
                            work = Path.GetDirectoryName(path);
                        }
                        if(!string.IsNullOrEmpty(work)) {
                            sei.lpDirectory = Marshal.StringToCoTaskMemUni(work);
                        }
                    }

                    if(PInvoke.ShellExecuteEx(ref sei)) {
                        StaticReg.ExecutedPathsList.Add(path);
                        return;
                    }
                }
                finally {
                    if(sei.lpFile != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(sei.lpFile);
                    }
                    if(sei.lpParameters != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(sei.lpParameters);
                    }
                    if(sei.lpDirectory != IntPtr.Zero) {
                        Marshal.FreeCoTaskMem(sei.lpDirectory);
                    }
                }
            }

            // Show Error Message.
            if(!String.IsNullOrEmpty(args))
                path += ", " + args;

            if(!String.IsNullOrEmpty(work))
                path += ", " + work;

            MessageBox.Show(
                String.Format(QTUtility.TextResourcesDic["ErrorDialogs"][0], path),
                QTUtility.TextResourcesDic["ErrorDialogs"][1],
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        public static void CreateNewApp(List<string> paths) {
            appList.AddRange(paths.Select(path => 
                    new UserApp(Path.GetFileName(path), path, "", "", Keys.None)));
            SaveApps();
        }

        public static void HandleReorder(IEnumerable<ToolStripItem> items) {
            int dummy;
            List<UserApp> reordered = new List<UserApp>();
            ListFromNestedStructure(reordered, out dummy,
                    items.OfType<QMenuItem>(), // todo: separators
                    item => item.MenuItemArguments.App,
                    item => item.MenuItemArguments.App.IsFolder
                        ? item.DropDown.Items.Cast<QMenuItem>()
                        : null);
            
            // Find the index of the first item in the level that was reordered.
            Stack<int> startIdx = new Stack<int>();
            Stack<int> levelChildren = new Stack<int>();
            startIdx.Push(0);
            int children = int.MaxValue;
            int i = 0;
            foreach(UserApp app in appList) {
                i++;
                while(children-- == 0) {
                    children = levelChildren.Pop();
                    startIdx.Pop();
                }
                if(app == reordered[0]) break;
                if(app.IsFolder) {
                    levelChildren.Push(children);
                    startIdx.Push(i);
                    children = app.ChildrenCount;
                }
            }

            i = startIdx.Pop();
            if(i + reordered.Count > appList.Count) {
                // This should never happen
                SystemSounds.Hand.Play();
            }
            else {
                for(int j = 0; j < reordered.Count; j++) {
                    appList[i + j] = reordered[j];
                }
                SaveApps();
            }
        }
    }
}

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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;

namespace QTTabBarLib {
    internal static class PluginManager {
        private static IEncodingDetector plgEncodingDetector;
        private static Dictionary<string, PluginAssembly> dicPluginAssemblies = new Dictionary<string, PluginAssembly>();
        private static Dictionary<string, Plugin> dicStaticPluginInstances = new Dictionary<string, Plugin>();      

        public static void ClearIEncodingDetector() {
            plgEncodingDetector = null;
        }

        public static bool GetAssembly(string path, out PluginAssembly asm) {
            return dicPluginAssemblies.TryGetValue(path, out asm);
        }

        private static bool IsPluginEnabled(string[] enabled, PluginInformation info) {
            if(enabled == null || info == null) {
                return false;
            }

            string typeSuffix = "+" + info.TypeFullName;
            return enabled.Contains(info.PluginID) ||
                    enabled.Any(id => !string.IsNullOrEmpty(id) && id.EndsWith(typeSuffix, StringComparison.Ordinal));
        }

        // 处理插件的异常
        public static void HandlePluginException(Exception ex, IntPtr hwnd, string pluginID, string strCase) {
            MessageForm.Show(hwnd, 
                "Error : " + strCase + "\r\nPlugin : \"" + pluginID + "\"\r\nErrorType : " + ex, 
                "Plugin Error", 
                MessageBoxIcon.Hand, 
                0x7530);
        }

        public static void Initialize() {
            // add by indiff.
            InitDefaultQTConfigPlugin();    
            foreach(PluginAssembly pa in ReadAssemblyPaths().Select(LoadAssembly)) {
                if(pa == null) continue;
                foreach(PluginInformation info in pa.PluginInformations ?? Enumerable.Empty<PluginInformation>()) {
                    if(info.Enabled) LoadStaticInstance(info, pa);        
                }
            }
            EnsureDefaultClockPlugin();
        }

        private static void EnsureDefaultClockPlugin() {
            PluginInformation clock = PluginInformations.FirstOrDefault(info =>
                    info != null &&
                    string.Equals(System.IO.Path.GetFileName(info.Path), "QTClock.dll", StringComparison.OrdinalIgnoreCase));
            if(clock == null) return;

            clock.Enabled = true;
            string[] enabled = Config.Plugin.Enabled ?? new string[0];
            if(!enabled.Contains(clock.PluginID)) {
                Config.Plugin.Enabled = enabled.Concat(new[] { clock.PluginID }).ToArray();
            }

            string[] activePluginIDs = Config.BBar.ActivePluginIDs ?? new string[0];
            if(activePluginIDs.Contains(clock.PluginID)) return;

            Config.BBar.ActivePluginIDs = activePluginIDs.Concat(new[] { clock.PluginID }).ToArray();
            int pluginButtonIndex = (activePluginIDs.Length + 1) << 16;
            Config.BBar.ButtonIndexes = (Config.BBar.ButtonIndexes ?? new int[0])
                    .Concat(new[] { pluginButtonIndex }).ToArray();
            ConfigManager.WriteConfig();
        }

        /// <summary>
        /// 加载一个默认的 插件， 如果存在的话！ TODO
        /// </summary>
        private static void InitDefaultQTConfigPlugin()
        {
            ///////////// add default plugin by qwop 2012-07-10////////////////
            // the program data 's default plugin.
            // string defaultQtConfigPath = Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\QTQuick.dll";
            // string turnOffRepeatPath = Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\TurnOffRepeat.dll";
            // Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\SampleSplitButton.dll",                 
            string[] plugins = new string[] { 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\QTQuick.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\TurnOffRepeat.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\CreateNewItem.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\FolderTreeButton.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\Memo.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\MigemoLoader.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\QTClock.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\QTFileTools.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\QTViewModeButton.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\QTWindowManager.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\ShowStatusBar.dll", 
                //Environment.GetEnvironmentVariable("ProgramData") + @"\QTTabBar\ActivateByMouseHover.dll"

                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\QTQuick.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\TurnOffRepeat.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\CreateNewItem.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\FolderTreeButton.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\Memo.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\MigemoLoader.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\QTClock.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\QTFileTools.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\QTViewModeButton.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\QTWindowManager.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\ShowStatusBar.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"QTTabBar\ActivateByMouseHover.dll"),
            };

            List<string> lists = new List<string>();
            foreach(string plugin in plugins ) {
                if (File.Exists(plugin))
                {
                    lists.Add(plugin);
                }
            }
             /* Config.BBar.ActivePluginIDs = new string[] { "QTConfig1.0.0.0(9CD80883)+Qwop.QTConfigPluginButton" };
                Config.BBar.ButtonIndexes = QTUtility.IsXP
                     ? new int[] { 1, 2, 0, 3, 4, 5, 0, 6, 7, 0, 11, 13, 12, 14, 15, 0, 65536, 9, 20 }
                     : new System.Int32[] { 3, 4, 5, 0, 6, 7, 0, 17, 11, 12, 14, 15, 13, 0, 65536, 9, 19, 10 };
				*/

            /*
            if (File.Exists(defaultQtConfigPath))
            {
                // plugin.
               
               
              //  List<string> lists = new string[] { defaultQtConfigPath }.ToList();
                lists.Add(defaultQtConfigPath);
            }


            if (File.Exists(turnOffRepeatPath))
            {
                lists.Add(turnOffRepeatPath);
            }
             */

            if (lists.Count > 0) {
                PluginManager.SavePluginAssemblyPaths(lists);
            }
        }

        public static PluginAssembly LoadAssembly(string path) {
            if(path.Length > 0 && File.Exists(path)) {
                PluginAssembly pa = new PluginAssembly(path);
                if(pa.PluginInfosExist) {
                    string[] enabled = Config.Plugin.Enabled;
                    foreach(PluginInformation info in pa.PluginInformations
                            .Where(info => IsPluginEnabled(enabled, info))) {
                        info.Enabled = true;
                        pa.Enabled = true;
                    }
                    dicPluginAssemblies[pa.Path] = pa;
                    return pa;
                }
            }
            return null;
        }

        private static void LoadStaticInstance(PluginInformation pi, PluginAssembly pa) {
            Plugin plugin;
            if(!dicStaticPluginInstances.TryGetValue(pi.PluginID, out plugin)) {
                plugin = pa.Load(pi.PluginID);
                if(plugin == null || plugin.Instance == null) return;
                dicStaticPluginInstances[pi.PluginID] = plugin;
                if(plugin.PluginInformation.PluginType == PluginType.Static) {
                    try {
                        plugin.Instance.Open(null, null);
                    }
                    catch(Exception exception) {
                        HandlePluginException(exception, IntPtr.Zero, pi.Name, "Loading static plugin.");
                        return;
                    }
                }
            }
            if(plgEncodingDetector == null) {
                plgEncodingDetector = plugin.Instance as IEncodingDetector;
            }
        }

        private static IEnumerable<string> ReadAssemblyPaths() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Plugins\Paths")) {
                if(key == null) yield break;
                foreach(string str in key.GetValueNames())
                {
                    // 需要判断文件是否存在
                    var path = (string)key.GetValue(str, string.Empty);
                    if (File.Exists(path))
                    {
                        yield return path;
                    }
                }
            }
        }

        private static readonly object _refreshLock = new object();
        public static void RefreshPlugins()
        {
            // 防止并发调用导致字典损坏和 NRE
            lock (_refreshLock)
            {
                try
                {
                    MyRefreshPlugins();
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex, "PluginManager.MyRefreshPlugins");
                }
            }
        }

        private static void MyRefreshPlugins()
        {
            string[] enabled = Config.Plugin.Enabled ?? Array.Empty<string>();

            List<PluginAssembly> asmsToRefresh = ReadAssemblyPaths()
                .Select(path =>
                {
                    try
                    {
                        PluginAssembly asm;
                        if (!GetAssembly(path, out asm))
                        {
                            asm = LoadAssembly(path);
                        }
                        else
                        {
                            PluginAssembly oldAsm = asm;
                            asm = LoadAssembly(path);
                            if (asm == null)
                            {
                                asm = oldAsm;
                            }
                            else if (!ReferenceEquals(oldAsm, asm))
                            {
                                oldAsm.Dispose();
                            }
                        }

                        // ✅ 核心修复：确保返回的对象及其关键属性完全有效
                        if (asm == null || asm.PluginInformations == null)
                        {
                            return null; // 统一返回 null，由后续 Where 过滤
                        }

                        foreach (PluginInformation info in asm.PluginInformations)
                        {
                            if (info != null &&
                                IsPluginEnabled(enabled, info) &&
                                !string.IsNullOrEmpty(info.Path) &&
                                File.Exists(info.Path))
                            {
                                info.Enabled = true;
                                asm.Enabled = true;
                            }
                        }

                        return asm;
                    }
                    catch (Exception ex)
                    {
                        // ✅ 防止单个插件加载失败导致整个 RefreshPlugins 崩溃
                        QTUtility2.MakeErrorLog(ex, $"LoadPluginFailed: {path}");
                        return null;
                    }
                })
                .Where(asm => asm != null && asm.PluginInformations != null) // ✅ 双重保险
                .Distinct()
                .ToList();

            if (asmsToRefresh.Count == 0) return;

            // 卸载不在新列表中的程序集
            //foreach (PluginAssembly asm in PluginAssemblies.Except(asmsToRefresh).ToList())
            //{
            //    if (asm != null) UninstallPluginAssembly(asm);
            //}

            // ✅ 修复：Except 可能因引用比较产生意外结果，改用路径比较
            var newPaths = new HashSet<string>(
                asmsToRefresh.Where(a => a != null).Select(a => a.Path),
                StringComparer.OrdinalIgnoreCase);

            foreach (PluginAssembly asm in PluginAssemblies.ToList())
            {
                if (asm == null || string.IsNullOrEmpty(asm.Path)) continue;
                if (!newPaths.Contains(asm.Path))
                {
                    UninstallPluginAssembly(asm);
                }
            }

            // ✅ 你已有的修复保持不变，但建议加上 try-catch 包裹每个插件的处理
            // ✅ 修复：双重防御 + 安全访问 PluginID
            List<string> pidsToUnload = asmsToRefresh
                .Where(asm => asm != null)
                .SelectMany(asm =>
                {
                    try { return asm.PluginInformations ?? Enumerable.Empty<PluginInformation>(); }
                    catch { return Enumerable.Empty<PluginInformation>(); }
                })
                .Where(pi => pi != null && !pi.Enabled && !string.IsNullOrEmpty(pi.PluginID))
                .Select(pi => pi.PluginID)
                .ToList();

            foreach (string pid in pidsToUnload)
            {
                Plugin plugin;
                if (!dicStaticPluginInstances.TryGetValue(pid, out plugin)) continue;
                try
                {
                    if (plugin != null && plugin.PluginInformation != null &&
                        plugin.PluginInformation.PluginType == PluginType.Static)
                    {
                        plugin.Close(EndCode.Removed);
                    }
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex, $"UnloadPluginFailed: {pid}");
                }
                dicStaticPluginInstances.Remove(pid);
            }
            ClearIEncodingDetector();

            // 重新加载启用的插件
            foreach (PluginAssembly pa in asmsToRefresh)
            {
                if (pa == null || pa.PluginInformations == null) continue;

                foreach (PluginInformation info in pa.PluginInformations)
                {
                    if (info != null && info.Enabled)
                    {
                        try
                        {
                            LoadStaticInstance(info, pa);
                        }
                        catch (Exception ex)
                        {
                            QTUtility2.MakeErrorLog(ex, $"LoadStaticInstanceFailed: {info?.PluginID}");
                        }
                    }
                }
            }

            InstanceManager.LocalTabBroadcast(tabbar =>
            {
                if (tabbar != null && tabbar.pluginServer != null)
                    tabbar.pluginServer.RefreshPlugins();
            });
        }

        public static void SavePluginAssemblyPaths(List<string> paths) {
            const string RegPath = RegConst.Root + @""; // TODO
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath + @"Plugins\Paths")) {
                foreach(string str in key.GetValueNames()) {
                    key.DeleteValue(str);
                }
                int idx = 0;
                foreach(string path in paths) {
                    key.SetValue("" + idx++, path);
                }
            }
        }

        public static bool TryGetStaticPluginInstance(string pid, out Plugin plugin) {
            return dicStaticPluginInstances.TryGetValue(pid, out plugin);
        }

        public static void UninstallPluginAssembly(PluginAssembly pa)
        {
            if (pa == null) return;
            try
            {
                if (pa.PluginInformations == null) return;

                List<string> pids = pa.PluginInformations
                    .Where(pi => pi != null)
                    .Select(pi => pi.PluginID)
                    .Where(pid => !string.IsNullOrEmpty(pid))
                    .ToList();

                InstanceManager.LocalTabBroadcast(tabbar => pids.ForEach(pid =>
                        tabbar.pluginServer.UnloadPluginInstance(pid, EndCode.Removed)));

                foreach (PluginInformation info in pa.PluginInformations)
                {
                    if (info == null) continue;
                    Plugin plugin;
                    if (!dicStaticPluginInstances.TryGetValue(info.PluginID, out plugin)) continue;
                    if (plugin != null && info.PluginType == PluginType.Static)
                        plugin.Close(EndCode.Removed);
                    dicStaticPluginInstances.Remove(info.PluginID);
                }
                dicPluginAssemblies.Remove(pa.Path);
                pa.Uninstall();
                pa.Dispose();
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "UninstallPluginAssembly: " + (pa?.Path ?? "null"));
            }
        }

        public static void UninstallPluginAssembly_bak(PluginAssembly pa) {
            List<string> pids = pa.PluginInformations.Select(pi => pi.PluginID).ToList();
            InstanceManager.LocalTabBroadcast(tabbar => pids.ForEach(pid =>
                    tabbar.pluginServer.UnloadPluginInstance(pid, EndCode.Removed)));

            foreach(PluginInformation info in pa.PluginInformations) {
                Plugin plugin;
                if(!dicStaticPluginInstances.TryGetValue(info.PluginID, out plugin)) continue;
                if(info.PluginType == PluginType.Static) plugin.Close(EndCode.Removed);
                dicStaticPluginInstances.Remove(info.PluginID);
            }
            if(pa != null) {
                dicPluginAssemblies.Remove(pa.Path);
                pa.Uninstall();
                pa.Dispose();
            }
        }

        public static IEncodingDetector IEncodingDetector {
            get {
                return plgEncodingDetector;
            }
        }

        public static IEnumerable<PluginAssembly> PluginAssemblies {
            get {
                lock (_refreshLock)
                {
                    return dicPluginAssemblies.Values
                        .Where(v => v != null)
                        .ToList();
                }
            }
        }

        //public static IEnumerable<PluginInformation> PluginInformations {
        //    get {
        //        return dicPluginAssemblies.Values
        //                .Where(pa => pa != null && pa.PluginInformations != null)
        //                .SelectMany(pa => pa.PluginInformations)
        //                .Where(info => info != null);
        //    }
        //}

        public static IEnumerable<PluginInformation> PluginInformations
        {
            get
            {
                return dicPluginAssemblies.Values
                    .Where(pa => pa != null && pa.PluginInformations != null)
                    .SelectMany(pa => pa.PluginInformations ?? Enumerable.Empty<PluginInformation>())
                    .Where(info => info != null);
            }
        }


        [Serializable]
        public class PluginButton {
            public string id { get; set; }
            public int index { get; set; }
        }
    }


    public sealed class Plugin {
        private bool fBackgroundButtonIsEnabled;
        private bool fBackgroundButtonIsSupported;
        private IPluginClient pluginClient;
        private PluginInformation pluginInfo;

        public Plugin(IPluginClient pluginClient, PluginInformation pluginInfo) {
            this.pluginClient = pluginClient;
            this.pluginInfo = pluginInfo;
            fBackgroundButtonIsSupported = ((pluginInfo.PluginType == PluginType.Background) && ((pluginClient is IBarButton) || (pluginClient is IBarCustomItem))) || ((pluginInfo.PluginType == PluginType.BackgroundMultiple) && (pluginClient is IBarMultipleCustomItems));
        }

        public void Close(EndCode code) {
            if(pluginClient != null) {
                try {
                    pluginClient.Close(code);
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, IntPtr.Zero, pluginInfo.Name, "Closing plugin.");
                }
                pluginClient = null;
            }
            pluginInfo = null;
        }

        public bool BackgroundButtonEnabled {
            get {
                return (fBackgroundButtonIsSupported && fBackgroundButtonIsEnabled);
            }
            set {
                if(fBackgroundButtonIsSupported) {
                    fBackgroundButtonIsEnabled = value;
                }
            }
        }

        public bool BackgroundButtonSupported {
            get {
                return fBackgroundButtonIsSupported;
            }
        }

        public IPluginClient Instance {
            get {
                return pluginClient;
            }
        }

        public PluginInformation PluginInformation {
            get {
                return pluginInfo;
            }
        }
    }

    public sealed class PluginInformation : IDisposable {
        public string Author;
        public string Description;
        public bool Enabled;
        public Image ImageLarge;
        public Image ImageSmall;
        public string Name;
        public string Path;
        public string PluginID;
        public PluginType PluginType;
        public string TypeFullName;
        public string Version;

        public PluginInformation(PluginAttribute pluginAtt, string path, string pluginID, string typeFullName) {
            Author = pluginAtt.Author;
            Name = pluginAtt.Name;
            Version = pluginAtt.Version;
            Description = pluginAtt.Description;
            PluginType = pluginAtt.PluginType;
            Path = path;
            PluginID = pluginID;
            TypeFullName = typeFullName;
        }

        public void Dispose() {
            if(ImageLarge != null) {
                ImageLarge.Dispose();
                ImageLarge = null;
            }
            if(ImageSmall != null) {
                ImageSmall.Dispose();
                ImageSmall = null;
            }
        }
    }

    public sealed class PluginAssembly : IDisposable {
        private Assembly assembly;
        public string Author;
        public string Description;
        private Dictionary<string, PluginInformation> dicPluginInformations = new Dictionary<string, PluginInformation>();
        public bool Enabled;
        private const string IMGLARGE = "_large";
        private const string IMGSMALL = "_small";
        public string Name;
        public string Path;
        private const string RESNAME = "Resource";
        private static Type T_PLUGINATTRIBUTE = typeof(PluginAttribute);
        public string Title;
        private static string TYPENAME_PLUGINCLIENT = typeof(IPluginClient).FullName;
        public string Version;

        public PluginAssembly(string path) {
            Path = path;
            Title = Author = Description = Version = Name = string.Empty;
            if(File.Exists(path)) {
                try {
                    assembly = Assembly.Load(File.ReadAllBytes(path));
                    AssemblyName name = assembly.GetName();
                    AssemblyTitleAttribute customAttribute = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                    AssemblyCompanyAttribute attribute2 = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
                    AssemblyDescriptionAttribute attribute3 = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    Version = name.Version.ToString();
                    if(customAttribute != null) {
                        Title = customAttribute.Title;
                    }
                    if(attribute2 != null) {
                        Author = attribute2.Company;
                    }
                    if(attribute3 != null) {
                        Description = attribute3.Description;
                    }
                    Name = Title + Version + "(" + path.GetHashCode().ToString("X") + ")";
                    foreach(Type type in assembly.GetTypes()) {
                        try {
                            if(ValidateType(type)) {
                                PluginAttribute pluginAtt = Attribute.GetCustomAttribute(type, T_PLUGINATTRIBUTE) as PluginAttribute;
                                if(pluginAtt != null) {
                                    string pluginID = Name + "+" + type.FullName;
                                    PluginInformation info = new PluginInformation(pluginAtt, path, pluginID, type.FullName);
                                    GetImageFromAssembly(assembly, type, info);
                                    dicPluginInformations[pluginID] = info;
                                }
                                else {
                                    QTUtility2.MakeErrorLog(null, "failed attribute");
                                }
                            }
                        }
                        catch {
                        }
                    }
                }
                catch(ReflectionTypeLoadException exception) {
                    QTUtility2.MakeErrorLog(exception, "Failed to load plugin assembly.\r\n"
                            + exception.LoaderExceptions.StringJoin("\r\n") + "\r\n" + path);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, "Failed to load plugin assembly.\r\n" + path);
                }
            }
        }

        public void Dispose() {
            assembly = null;
            foreach(PluginInformation information in dicPluginInformations.Values) {
                information.Dispose();
            }
            dicPluginInformations.Clear();
        }

        private static void GetImageFromAssembly(Assembly asm, Type type, PluginInformation info) {
            try {
                Type type2 = asm.GetType(type.Namespace + "." + RESNAME);
                if(type2 != null) {
                    PropertyInfo property = type2.GetProperty(type.Name + IMGLARGE, BindingFlags.NonPublic | BindingFlags.Static);
                    PropertyInfo info3 = type2.GetProperty(type.Name + IMGSMALL, BindingFlags.NonPublic | BindingFlags.Static);
                    if(property != null) {
                        info.ImageLarge = (Image)property.GetValue(null, null);
                    }
                    if(info3 != null) {
                        info.ImageSmall = (Image)info3.GetValue(null, null);
                    }
                }
            }
            catch {
            }
        }

        public Plugin Load(string pluginID) {
            if(File.Exists(Path)) {
                try {
                    PluginInformation information;
                    if(dicPluginInformations.TryGetValue(pluginID, out information)) {
                        IPluginClient pluginClient = assembly.CreateInstance(information.TypeFullName) as IPluginClient;
                        if(pluginClient != null) {
                            Plugin plugin = new Plugin(pluginClient, information);
                            IBarButton button = pluginClient as IBarButton;
                            if(button != null) {
                                Image imageLarge = information.ImageLarge;
                                Image imageSmall = information.ImageSmall;
                                try {
                                    Image image = button.GetImage(true);
                                    Image image4 = button.GetImage(false);
                                    if(image != null) {
                                        information.ImageLarge = image;
                                        if(imageLarge != null) {
                                            imageLarge.Dispose();
                                        }
                                    }
                                    if(image4 != null) {
                                        information.ImageSmall = image4;
                                        if(imageSmall != null) {
                                            imageSmall.Dispose();
                                        }
                                    }
                                }
                                catch(Exception exception) {
                                    PluginManager.HandlePluginException(exception, IntPtr.Zero, information.Name, "Getting image from pluging.");
                                    throw;
                                }
                            }
                            return plugin;
                        }
                    }
                }
                catch(Exception exception2) {
                    QTUtility2.MakeErrorLog(exception2);
                }
            }
            return null;
        }

        public bool TryGetPluginInformation(string pluginID, out PluginInformation info) {
            return dicPluginInformations.TryGetValue(pluginID, out info);
        }

        public void Uninstall() {
            try {
                foreach(Type type in assembly.GetTypes()) {
                    try {
                        if(ValidateType(type)) {
                            MethodInfo method = type.GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Static);
                            if(method != null) {
                                method.Invoke(null, null);
                            }
                        }
                    }
                    catch {
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "failed uninstall type");
            }
        }

        private static bool ValidateType(Type t) {
            return (((t.IsClass && t.IsPublic) && !t.IsAbstract) && (t.GetInterface(TYPENAME_PLUGINCLIENT) != null));
        }

        public List<PluginInformation> PluginInformations {
            get
            {
                if (dicPluginInformations == null)
                    return new List<PluginInformation>();
                return dicPluginInformations.Values
                    .Where(v => v != null)
                    .ToList();
            }
        }

        public bool PluginInfosExist {
            get {
                return (dicPluginInformations.Count > 0);
            }
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct PluginKey {
        public string PluginID;
        public int[] Keys;
        public PluginKey(string pluginID, int[] keys) {
            PluginID = pluginID;
            Keys = keys;
        }
    }
}

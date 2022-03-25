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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using QTPlugin;
using QTTabBarLib.Interop;
using System.Media;

namespace QTTabBarLib {
    internal static class QTUtility {
        internal static readonly Version BetaRevision = new Version(4, 0); // 主版本 beta  次版本 alpha
        internal static readonly Version CurrentVersion = new Version(1, 5, 5, 0);
        internal const int FIRST_MOUSE_ONLY_ACTION = 1000;
        internal const int FLAG_KEYENABLED = 0x100000;
        internal const string IMAGEKEY_FOLDER = "folder";
        internal const string IMAGEKEY_MYNETWORK = "mynetwork";
        internal const string IMAGEKEY_NOEXT = "noext";
        internal const string IMAGEKEY_NOIMAGE = "noimage";
        internal const bool IS_DEV_VERSION = true;  // <----------------- Change me before releasing!
        internal static readonly bool IsRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        internal static readonly bool IsWin7 = Environment.OSVersion.Version >= new Version(6, 1);
        internal static readonly bool IsXP = Environment.OSVersion.Version.Major <= 5;
        internal static readonly string PATH_MYNETWORK = IsXP
                ? "::{208D2C60-3AEA-1069-A2D7-08002B30309D}"
                : "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}";
        internal static readonly string PATH_SEARCHFOLDER = IsXP
                ? "::{E17D4FC0-5564-11D1-83F2-00A0C90DC849}"
                : "::{9343812E-1C37-4A49-A12E-4B2D810D956B}";
        internal const string REGUSER = RegConst.Root;
        internal static readonly char[] SEPARATOR_CHAR = new char[] { ';' };
        internal const string SEPARATOR_PATH_HASH_SESSION = "*?*?*";
        internal const bool NOW_DEBUGGING =
#if DEBUG
            true;
#else
            false;
#endif

        
        // TODO: almost all of these need to be either sync'd or removed.
        // TODO: we should store actual TabItems, not just strings.
        internal static Dictionary<string, string> DisplayNameCacheDic = new Dictionary<string, string>();
        internal static bool fExplorerPrevented;
        internal static bool fRestoreFolderTree;
        internal static bool fSingleClick;
        internal static int iIconUnderLineVal;
        internal static ImageList ImageListGlobal;
        internal static Dictionary<string, byte[]> ITEMIDLIST_Dic_Session = new Dictionary<string, byte[]>();
        internal static List<string> NoCapturePathsList = new List<string>();
        internal static string[] ResMain;
        internal static string[] ResMisc;
        internal static bool RestoreFolderTree_Hide;
        // internal static SolidBrush sbAlternate;
       // internal static Font StartUpTabFont;
        internal static Dictionary<string, string[]> TextResourcesDic;
        internal static byte WindowAlpha = 0xff;

        /// <summary>
        /// 只执行一次
        /// </summary>
        static QTUtility() {
            // I'm tempted to just return for everything except "explorer"
            // Maybe I should...
            String processName = Process.GetCurrentProcess().ProcessName.ToLower();
            if(processName == "iexplore" || processName == "regasm" || processName == "gacutil") {
                return;
            }

            // Register a callback for AssemblyResolve in order to load embedded assemblies.
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                String resourceName = "QTTabBarLib.Resources." + new AssemblyName(args.Name).Name + ".dll";
                using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                    if(stream == null) return null;
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            try {
                // Load the config
                ConfigManager.Initialize();

                // Initialize the instance manager
                InstanceManager.Initialize();

                // Create and enable the API hooks
                HookLibManager.Initialize();

                // Create the global imagelist
                ImageListGlobal = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
                ImageListGlobal.Images.Add("folder", GetIcon(string.Empty, false));

                // Load groups/apps
                GroupsManager.LoadGroups();
                AppsManager.LoadApps();

                if(Config.Lang.UseLangFile && File.Exists(Config.Lang.LangFile)) {
                    TextResourcesDic = ReadLanguageFile(Config.Lang.LangFile);
                }
                ValidateTextResources();

                using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                    if(key != null) {
                        using(RegistryKey key2 = key.CreateSubKey("RecentlyClosed")) {
                            if(key2 != null) {
                                List<string> collection = key2.GetValueNames()
                                        .Select(str4 => (string)key2.GetValue(str4)).ToList();
                                StaticReg.ClosedTabHistoryList = new UniqueList<string>(collection, Config.Misc.TabHistoryCount);
                            }
                        }
                        using(RegistryKey key3 = key.CreateSubKey("RecentFiles")) {
                            if(key3 != null) {
                                List<string> list2 = key3.GetValueNames().Select(str5 =>
                                        (string)key3.GetValue(str5)).ToList();
                                StaticReg.ExecutedPathsList = new UniqueList<string>(list2, Config.Misc.FileHistoryCount);
                            }
                        }
                        RefreshLockedTabsList();
                        string str7 = (string)key.GetValue("NoCaptureAt", string.Empty);
                        if(str7.Length > 0) {
                            NoCapturePathsList = new List<string>(str7.Split(SEPARATOR_CHAR));
                        }
                        if(!byte.TryParse((string)key.GetValue("WindowAlpha", "255"), out WindowAlpha)) {
                            WindowAlpha = 0xff;
                        }
                    }
                }
                // default add ::{20D04FE0-3AEA-1069-A2D8-08002B30309D};::{26EE0668-A00A-44D7-9371-BEB064C98683}
                NoCapturePathsList.Add("::{26EE0668-A00A-44D7-9371-BEB064C98683}");
                NoCapturePathsList.Add("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"); // 控制面板
                NoCapturePathsList.Add("::{21EC2020-3AEA-1069-A2DD-08002B30309D}"); // 控制面板
                NoCapturePathsList.Add("::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{ED834ED6-4B5A-4BFE-8F11-A626DCB6A921}");
                NoCapturePathsList.Add("::{7007ACC7-3202-11D1-AAD2-00805FC1270E}");
                // 回收站      NoCapturePathsList.Add("::{645FF040-5081-101B-9F08-00AA002F954E}");
                /*
                                               回收站 – {645FF040-5081-101B-9F08-00AA002F954E}

 

                               控制面板 – {21EC2020-3AEA-1069-A2DD-08002B30309D}
 

                               运行 – {2559A1F3-21D7-11D4-BDAF-00C04F60B9F0}
 

                               搜索 – {2559A1F0-21D7-11D4-BDAF-00C04F60B9F0}
 

                               Internet Explorer – {871C5380-42A0-1069-A2EA-08002B30309D}
 

                               管理工具 – {D20EA4E1-3957-11D2-A40B-0C5020524153}
 

                               网络连接 – {7007ACC7-3202-11D1-AAD2-00805FC1270E}
 

                               打印机和传真 – {2227A280-3AEA-1069-A2DE-08002B30309D}
                                               */
                
                // 配置不捕获控制面板
                GetShellClickMode();

                // Initialize plugins
                PluginManager.Initialize();
            }
            catch(Exception exception) {
                // TODO: Any errors here would be very serious.  Alert the user as such.
                QTUtility2.MakeErrorLog(exception);
            }
        }

        public static object ByteArrayToObject(byte[] arrBytes) {
            using(MemoryStream memStream = new MemoryStream()) {
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return new BinaryFormatter().Deserialize(memStream);                
            }
        }

        private readonly static string[] strIconExt = new string[] { ".exe", ".lnk", ".ico", ".url", ".sln" };
        public static bool ExtHasIcon(string ext) {
            return strIconExt.Contains(ext);
        }

        private readonly static string[] strCompressedExt = new string[] { ".zip", ".lzh", ".cab" };

        public static bool ExtIsCompressed(string ext) {
            return strCompressedExt.Contains(ext);
        }

        public static void GetHiddenFileSettings(out bool fShowHidden, out bool fShowSystem) {
            const uint SSF_SHOWALLOBJECTS   = 0x00001;
            const uint SSF_SHOWSUPERHIDDEN  = 0x40000;
            SHELLSTATE ss = new SHELLSTATE();
            PInvoke.SHGetSetSettings(ref ss, SSF_SHOWALLOBJECTS | SSF_SHOWSUPERHIDDEN, false);
            fShowHidden = ss.fShowAllObjects != 0;
            fShowSystem = ss.fShowSuperHidden != 0;
        }

        public static Icon GetIcon(IntPtr pIDL) {
            SHFILEINFO psfi = new SHFILEINFO();
            if((IntPtr.Zero != PInvoke.SHGetFileInfo(pIDL, 0, ref psfi, Marshal.SizeOf(psfi), 0x109)) && (psfi.hIcon != IntPtr.Zero)) {
                Icon icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                PInvoke.DestroyIcon(psfi.hIcon);
                return icon;
            }
            return Resources_Image.icoEmpty;
        }

        public static Icon GetIcon(string path, bool fExtension) {
            Icon icon;
            SHFILEINFO psfi = new SHFILEINFO();
            if(fExtension) {
                if(path.Length == 0) {
                    path = ".*";
                }
                if((IntPtr.Zero != PInvoke.SHGetFileInfo("*" + path, 0x80, ref psfi, Marshal.SizeOf(psfi), 0x111)) && (psfi.hIcon != IntPtr.Zero)) {
                    icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                    PInvoke.DestroyIcon(psfi.hIcon);
                    return icon;
                }
                return Resources_Image.icoEmpty;
            }
            if(path.Length == 0) {
                if((IntPtr.Zero != PInvoke.SHGetFileInfo("dummy", 0x10, ref psfi, Marshal.SizeOf(psfi), 0x111)) && (psfi.hIcon != IntPtr.Zero)) {
                    icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                    PInvoke.DestroyIcon(psfi.hIcon);
                    return icon;
                }
                return Resources_Image.icoEmpty;
            }
            if(!IsXP && path.StartsWith("::")) {
                IntPtr pszPath = PInvoke.ILCreateFromPath(path);
                if(pszPath != IntPtr.Zero) {
                    if((IntPtr.Zero != PInvoke.SHGetFileInfo(pszPath, 0, ref psfi, Marshal.SizeOf(psfi), 0x109)) && (psfi.hIcon != IntPtr.Zero)) {
                        icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                        PInvoke.DestroyIcon(psfi.hIcon);
                        PInvoke.CoTaskMemFree(pszPath);
                        return icon;
                    }
                    PInvoke.CoTaskMemFree(pszPath);
                }
            }
            else if((IntPtr.Zero != PInvoke.SHGetFileInfo(path, 0, ref psfi, Marshal.SizeOf(psfi), 0x101)) && (psfi.hIcon != IntPtr.Zero)) {
                icon = new Icon(Icon.FromHandle(psfi.hIcon), 0x10, 0x10);
                PInvoke.DestroyIcon(psfi.hIcon);
                return icon;
            }
            return Resources_Image.icoEmpty;
        }

        public static string GetImageKey(string path, string ext) {
            if(!string.IsNullOrEmpty(path)) {
                if(QTUtility2.IsNetworkPath(path)) {
                    if(ext != null) {
                        ext = ext.ToLower();
                        if(ext.Length == 0) {
                            SetImageKey("noext", path);
                            return "noext";
                        }
                        if(!ImageListGlobal.Images.ContainsKey(ext)) {
                            ImageListGlobal.Images.Add(ext, GetIcon(ext, true));
                        }
                        return ext;
                    }
                    if(IsNetworkRootFolder(path)) {
                        SetImageKey(path, path);
                        return path;
                    }
                    SetImageKey("mynetwork", PATH_MYNETWORK);
                    return "mynetwork";
                }
                if(path.StartsWith("::")) {
                    SetImageKey(path, path);
                    return path;
                }
                if(ext != null) {
                    ext = ext.ToLower();
                    if(ext.Length == 0) {
                        SetImageKey("noext", path);
                        return "noext";
                    }
                    if(ExtHasIcon(ext)) {
                        SetImageKey(path, path);
                        return path;
                    }
                    SetImageKey(ext, path);
                    return ext;
                }
                if(path.Contains("*?*?*")) {
                    byte[] buffer;
                    if(ImageListGlobal.Images.ContainsKey(path)) {
                        return path;
                    }
                    if(ITEMIDLIST_Dic_Session.TryGetValue(path, out buffer)) {
                        using(IDLWrapper w = new IDLWrapper(buffer)) {
                            if(w.Available) {
                                ImageListGlobal.Images.Add(path, GetIcon(w.PIDL));
                                return path;
                            }
                        }
                    }
                    return "noimage";
                }
                if(QTUtility2.IsShellPathButNotFileSystem(path)) {
                    IDLWrapper wrapper;
                    if(ImageListGlobal.Images.ContainsKey(path)) {
                        return path;
                    }
                    if(IDLWrapper.TryGetCache(path, out wrapper)) {
                        using(wrapper) {
                            if(wrapper.Available) {
                                ImageListGlobal.Images.Add(path, GetIcon(wrapper.PIDL));
                                return path;
                            }
                        }
                    }
                    return "noimage";
                }
                if(path.StartsWith("ftp://") || path.StartsWith("http://")) {
                    return "folder";
                }
                try {
                    DirectoryInfo info = new DirectoryInfo(path);
                    if(info.Exists) {
                        FileAttributes attributes = info.Attributes;
                        if(((attributes & FileAttributes.System) != 0) || ((attributes & FileAttributes.ReadOnly) != 0)) {
                            SetImageKey(path, path);
                            return path;
                        }
                        return "folder";
                    }
                    if(File.Exists(path)) {
                        ext = Path.GetExtension(path).ToLower();
                        if(ext.Length == 0) {
                            SetImageKey("noext", path);
                            return "noext";
                        }
                        if(ExtHasIcon(ext)) {
                            SetImageKey(path, path);
                            return path;
                        }
                        SetImageKey(ext, path);
                        return ext;
                    }
                    if(path.ToLower().Contains(@".zip\")) {
                        return "folder";
                    }
                }
                catch {
                }
            }
            return "noimage";
        }

        public static DateTime GetLinkerTimestamp() {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] buf = new byte[2048];
            Stream stream = null;

            try {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                stream.Read(buf, 0, 2048);
            }
            finally {
                if(stream != null) {
                    stream.Close();
                }
            }

            int offset = BitConverter.ToInt32(buf, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(buf, offset + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetResourceStrings(this ResourceManager res) {
            var dict = res.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            var e = dict.GetEnumerator();
            while(e.MoveNext()) {
                yield return new KeyValuePair<string, string>((string)e.Key, (string)e.Value);
            }
        }

        public static T[] GetSettingValue<T>(T[] inputValues, T[] defaultValues, bool fClone) {
            if((inputValues == null) || (inputValues.Length == 0)) {
                if(!fClone) {
                    return defaultValues;
                }
                return (T[])defaultValues.Clone();
            }
            int length = defaultValues.Length;
            int num2 = inputValues.Length;
            T[] localArray = new T[length];
            for(int i = 0; i < length; i++) {
                if(i < num2) {
                    localArray[i] = inputValues[i];
                }
                else {
                    localArray[i] = defaultValues[i];
                }
            }
            return localArray;
        }

        public static void GetShellClickMode() {
            const string lpSubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer";
            iIconUnderLineVal = 0;
            int lpcbData = 4;
            try {
                IntPtr ptr;
                if(PInvoke.RegOpenKeyEx((IntPtr)(-2147483647), lpSubKey, 0, 0x20019, out ptr) == 0) {
                    using(SafePtr lpData = new SafePtr(4)) {
                        int num2;
                        if(PInvoke.RegQueryValueEx(ptr, "IconUnderline", IntPtr.Zero, out num2, lpData, ref lpcbData) == 0) {
                            byte[] destination = new byte[4];
                            Marshal.Copy(lpData, destination, 0, 4);
                            iIconUnderLineVal = destination[0];
                        }                        
                    }
                    PInvoke.RegCloseKey(ptr);
                }
                using(RegistryKey key = Registry.CurrentUser.OpenSubKey(lpSubKey, false)) {
                    byte[] buffer2 = (byte[])key.GetValue("ShellState");
                    fSingleClick = false;
                    if((buffer2 != null) && (buffer2.Length > 3)) {
                        byte num3 = buffer2[4];
                        fSingleClick = (num3 & 0x20) == 0;
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception);
            }
        }

        public static TabBarOption GetTabBarOption() {
            return null; // TODO
        }

        private static bool IsNetworkRootFolder(string path) {
            string str = path.Substring(2);
            int index = str.IndexOf(Path.DirectorySeparatorChar);
            if(index != -1) {
                string str2 = str.Substring(index + 1);
                if(str2.Length > 0) {
                    return (str2.IndexOf(Path.DirectorySeparatorChar) == -1);
                }
            }
            return false;
        }

        public static void Initialize() {
            // This method exists just to cause the static constructor to fire, if it hasn't already.
        }

        public static void LoadReservedImage(ImageReservationKey irk) {
            if(!ImageListGlobal.Images.ContainsKey(irk.ImageKey)) {
                switch(irk.ImageType) {
                    case 0:
                        if(irk.ImageKey != "noimage") {
                            if(irk.ImageKey == "noext") {
                                ImageListGlobal.Images.Add("noext", GetIcon(string.Empty, true));
                                return;
                            }
                            return;
                        }
                        return;

                    case 1:
                        ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(irk.ImageKey, true));
                        return;

                    case 2:
                    case 4:
                        ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(irk.ImageKey, false));
                        return;

                    case 3:
                        return;

                    case 5:
                        byte[] buffer;
                        if(ITEMIDLIST_Dic_Session.TryGetValue(irk.ImageKey, out buffer)) {
                            using(IDLWrapper w = new IDLWrapper(buffer)) {
                                if(!w.Available) return;
                                ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(w.PIDL));
                            }
                        }
                        return;

                    case 6:
                        IDLWrapper wrapper;
                        if(IDLWrapper.TryGetCache(irk.ImageKey, out wrapper)) {
                            using(wrapper) {
                                if(wrapper.Available) {
                                    ImageListGlobal.Images.Add(irk.ImageKey, GetIcon(wrapper.PIDL));
                                }
                            }
                        }
                        return;
                }
            }
        }

        public static MouseChord MakeMouseChord(MouseChord button, Keys modifiers) {
            if((modifiers & Keys.Shift) != 0) button |= MouseChord.Shift;
            if((modifiers & Keys.Control) != 0) button |= MouseChord.Ctrl;
            if((modifiers & Keys.Alt) != 0) button |= MouseChord.Alt;
            return button;
        }

        public static byte[] ObjectToByteArray(Object obj) {
            if(obj == null) return null;
            using(MemoryStream ms = new MemoryStream()) {
                new BinaryFormatter().Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private static Regex singleLinebreakAtStart = new Regex(@"^(\r\n)?");
        public static Dictionary<string, string[]> ReadLanguageFile(string path) {
          //  const string linebreak = "\r\n";
          //  const string linebreakLiteral = @"\r\n";

            //We have to remove the first linebreak in the XML element's value, before we can split 
            //on the linebreak. It's there in the XML, when the XML is created using the editor.
            //Other linebreaks should be left in place, even if the line is empty, in order to preserve
            //the relative places of the other substrings.
            //The simplest way to do this is with a regular expression.

            try {
               /* var dictionary = XElement.Load(path).Elements().ToDictionary(
                    element => element.Name.ToString(),
                    element => {
                        string[] substrings =
                            ((string)element)
                            .Replace(singleLinebreakAtStart, "")
                            .Split(new[] { linebreak }, StringSplitOptions.None)
                            .Select(
                                s => s.Replace(linebreakLiteral, linebreak)
                            )
                            .ToArray();
                        return substrings;
                    }
                );*/
                const string newValue = "\r\n";
                const string oldValue = @"\r\n";
                Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();

                using (XmlTextReader reader = new XmlTextReader(path))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.Name == "root") continue;
                        string[] str = reader.ReadString().Split(new string[] { newValue }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < str.Length; i++)
                        {
                            str[i] = str[i].Replace(oldValue, newValue);
                        }
                        dictionary[reader.Name] = str;
                    }
                }
                return dictionary;
            } catch (XmlException xmlException) {
                string msg = String.Join("\r\n", new[] {
                    "Invalid language file.",
                    "",
                    xmlException.SourceUri,
                    "Line: " + xmlException.LineNumber,
                    "Position: " + xmlException.LinePosition,
                    "Detail: " + xmlException.Message
                });
                MessageBox.Show(msg);
                return null;
            } catch (Exception exception) {
                QTUtility2.MakeErrorLog(exception);
                return null;
            }
        }
       // private string QTTabBar = @"Software\QTTabBar\Config\Misc";
     

        
        public static void AsteriskPlay()
        {
            if (Config.Misc.SoundBox) {
                SystemSounds.Asterisk.Play();
            }

           
        }



        public static void SoundPlay()
        {
            if (Config.Misc.SoundBox)
            {
                SystemSounds.Hand.Play();
            }
        }


        public static void RefreshLockedTabsList() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    string[] collection = QTUtility2.ReadRegBinary<string>("TabsLocked", key);
                    if((collection != null) && (collection.Length != 0)) {
                        StaticReg.LockedTabsToRestoreList.Assign(collection);
                    }
                    else {
                        StaticReg.LockedTabsToRestoreList.Clear();
                    }
                }
            }
        }

        public static ImageReservationKey ReserveImageKey(QMenuItem qmi, string path, string ext) {
            ImageReservationKey key = null;
            if(string.IsNullOrEmpty(path)) {
                return new ImageReservationKey("noimage", 0);
            }
            if(!string.IsNullOrEmpty(ext)) {
                ext = ext.ToLower();
                if(ExtHasIcon(ext) && !QTUtility2.IsNetworkPath(path)) {
                    return new ImageReservationKey(path, 2);
                }
                return new ImageReservationKey(ext, 1);
            }
            if(QTUtility2.IsNetworkPath(path)) {
                if(IsNetworkRootFolder(path)) {
                    return new ImageReservationKey(path, 4);
                }
                return new ImageReservationKey("folder", 3);
            }
            if(path.StartsWith("::")) {
                return new ImageReservationKey(path, 4);
            }
            if(path.Contains("*?*?*")) {
                return new ImageReservationKey(path, 5);
            }
            if(QTUtility2.IsShellPathButNotFileSystem(path)) {
                return new ImageReservationKey(path, 6);
            }
            if(path.StartsWith("ftp://") || path.StartsWith("http://")) {
                return new ImageReservationKey("folder", 3);
            }
            try {
                if(qmi.Exists) {
                    if(qmi.Target == MenuTarget.Folder) {
                        if(qmi.HasIcon) {
                            return new ImageReservationKey(path, 4);
                        }
                        return new ImageReservationKey("folder", 3);
                    }
                    if(qmi.Target == MenuTarget.File) {
                        ext = Path.GetExtension(path).ToLower();
                        if(ext.Length == 0) {
                            return new ImageReservationKey("noext", 0);
                        }
                        if(ExtHasIcon(ext)) {
                            return new ImageReservationKey(path, 2);
                        }
                        return new ImageReservationKey(ext, 1);
                    }
                }
                DirectoryInfo info = new DirectoryInfo(path);
                if(info.Exists) {
                    FileAttributes attributes = info.Attributes;
                    if(((attributes & FileAttributes.System) != 0) || ((attributes & FileAttributes.ReadOnly) != 0)) {
                        return new ImageReservationKey(path, 4);
                    }
                    return new ImageReservationKey("folder", 3);
                }
                if(!File.Exists(path)) {
                    return new ImageReservationKey("noimage", 0);
                }
                ext = Path.GetExtension(path).ToLower();
                if(ext.Length == 0) {
                    return new ImageReservationKey("noext", 0);
                }
                if(ExtHasIcon(ext)) {
                    return new ImageReservationKey(path, 2);
                }
                key = new ImageReservationKey(ext, 1);
            }
            catch {
            }
            return key;
        }

        public static void SaveClosing(List<string> closingPaths) {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root)) {
                if(key != null) {
                    key.SetValue("TabsOnLastClosedWindow", closingPaths.StringJoin(";"));
                }
            }
        }

        public static void SaveRecentFiles(RegistryKey rkUser) {
            if(rkUser != null) {
                using(RegistryKey key = rkUser.CreateSubKey("RecentFiles")) {
                    if(key != null) {
                        foreach(string str in key.GetValueNames()) {
                            key.DeleteValue(str, false);
                        }
                        for(int i = 0; i < StaticReg.ExecutedPathsList.Count; i++) {
                            key.SetValue(i.ToString(), StaticReg.ExecutedPathsList[i]);
                        }
                    }
                }
            }
        }

        public static void SaveRecentlyClosed(RegistryKey rkUser) {
            if(rkUser != null) {
                using(RegistryKey key = rkUser.CreateSubKey("RecentlyClosed")) {
                    if(key != null) {
                        foreach(string str in key.GetValueNames()) {
                            key.DeleteValue(str, false);
                        }
                        for(int i = 0; i < StaticReg.ClosedTabHistoryList.Count; i++) {
                            key.SetValue(i.ToString(), StaticReg.ClosedTabHistoryList[i]);
                        }
                    }
                }
            }
        }
        
        private static void SetImageKey(string key, string itemPath) {
            if(!ImageListGlobal.Images.ContainsKey(key)) {
                ImageListGlobal.Images.Add(key, GetIcon(itemPath, false));
            }
        }

        public static void SetTabBarOption(TabBarOption tabBarOption, QTTabBarClass tabBar) {
            // TODO
        }

        public static void ValidateMinMax(ref int value, int min, int max) {
            value = ValidateMinMax(value, min, max);
        }

        // 判断是否为暗黑模式  Environment.OSVersion.Version.Major
        public static bool InNightMode
        {
            get
            {
                if (Environment.OSVersion.Version.Major > 9)
                {
                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize"))
                    {
                        if (rk != null)
                            return QTUtility2.GetValueSafe<int>(rk, "AppsUseLightTheme", 1) == 0;
                    }
                }
                return false;
            }
        }

        public static int ValidateMinMax(int value, int min, int max) {
            int a = Math.Min(min, max);
            int b = Math.Max(min, max);
            if(value < a) {
                value = a;
            }
            else if(value > b) {
                value = b;
            }
            return value;
        }

        public static void ValidateTextResources() {
            ValidateTextResources(ref TextResourcesDic);
            ResMain = TextResourcesDic["TabBar_Menu"];
            ResMisc = TextResourcesDic["Misc_Strings"];
            Resx.UpdateAll();
        }
    
        public static void ValidateTextResources(ref Dictionary<string, string[]> dict)
        {
            // MessageBox.Show("Config.Lang.UseLangFile:" + Config.Lang.UseLangFile + ",dict == null:" + (dict == null));
            // 需要过滤的掉的 url
            string[] urlKeys = { "SiteURL", "PayPalURL" };
            
            // dict 的检测
            if (dict == null)
            {
                dict = new Dictionary<string, string[]>();
            }

            // 加载内置语言,在此可添加内置语言
            IEnumerable<KeyValuePair<string, string>> keyValuePairs = null;
            switch (Config.Lang.BuiltInLangSelectedIndex)
            {
                case 0: keyValuePairs = Resources_String.ResourceManager.GetResourceStrings(); break;
                case 1: keyValuePairs = Resource_String_zh_CN.ResourceManager.GetResourceStrings(); break;
                case 2: keyValuePairs = Resources_String_de_DE.ResourceManager.GetResourceStrings(); break;
            }

            // 如果加载为空， 则读取默认的应用语言
            if (null == keyValuePairs)
            {
                keyValuePairs = Resources_String.ResourceManager.GetResourceStrings();
            }


            // 判断是否未使用内置语言,如果是的话，则直接遍历 内置语言
            if ( !Config.Lang.UseLangFile )
            {
                foreach (var pair in keyValuePairs)
                {
                    dict[pair.Key] = pair.Value.Split(SEPARATOR_CHAR);
                }
            }
            else // 加载外部语言文件
            {
                // 遍历内置语言
                foreach (var pair in keyValuePairs)
                {
                    if (urlKeys.Contains(pair.Key)) continue;
                    // 分号分隔字符串获得数组形式
                    string[] buildinValue = pair.Value.Split(SEPARATOR_CHAR);
                    string[] res;
                    dict.TryGetValue(pair.Key, out res);
                    if (res == null) // 如果从 dict 中未获取到对应的 值， 则从 内置语言覆盖掉.
                    {
                        dict[pair.Key] = buildinValue;
                    }
                    else if (res.Length < buildinValue.Length)// 如果获取到，但是于内置语言的数目不一致
                    {
                        int len = res.Length;
                        Array.Resize(ref res, buildinValue.Length);
                        Array.Copy(buildinValue, len, res, len, buildinValue.Length - len);
                        dict[pair.Key] = res;
                    }
                }
            }
        }
    }
}

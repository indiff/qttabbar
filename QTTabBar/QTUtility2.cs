//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2020  Quizo, Paul Accisano, Indiff
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
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using BandObjectLib;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using QTTabBarLib.Interop;
using SHDocVw;

namespace QTTabBarLib {
    public static class QTUtility2 {
        private const int THRESHOLD_ELLIPSIS = 40;
        private static bool fConsoleAllocated;
        // 判断是否启用日志，发布改为false， 调试启用. 默认是关闭的，在常规选项里面可以设置启用
        public static bool ENABLE_LOGGER = false;

        public static string ExplorerPath
        {
            get
            {
                // Environment.SpecialFolder.CommonApplicationData + 1 => Environment.SpecialFolder.Windows
                return Environment.GetFolderPath((Environment.SpecialFolder.CommonApplicationData + 1)) + Path.DirectorySeparatorChar + "explorer.exe";
            }
        }

        public static void AllocDebugConsole() {
            if(fConsoleAllocated) {
                return;
            }
            const int STD_OUTPUT_HANDLE = -11;
            const int MY_CODE_PAGE = 437;
            PInvoke.AllocConsole();
            IntPtr stdHandle = PInvoke.GetStdHandle(STD_OUTPUT_HANDLE);
            SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            Encoding encoding = Encoding.GetEncoding(MY_CODE_PAGE);
            StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            fConsoleAllocated = true;
        }



        public static T DeepClone<T>(T obj) {
            using(var ms = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                var deserialize = (T)formatter.Deserialize(ms);
                Close( ms );
                return deserialize;
            }
        }

        public static string Enquote(this string s) {
            return "\"" + s + "\"";
        }

        public static int GET_X_LPARAM(IntPtr lParam) {
            return ((int)lParam).LoWord();
        }

        public static int GET_Y_LPARAM(IntPtr lParam) {
            return ((int)lParam).HiWord();
        }

        public static int HiWord(this int i) {
            return (short)((i >> 0x10) & 0xffff);
        }

        public static int LoWord(this int i) {
            return (short)(i & 0xffff);
        }

        public static int HiWord(this IntPtr i) {
            return ((((int)(long)i) >> 16) & 0xFFFF);
        }

        public static int LoWord(this IntPtr i) {
            return ((int)(long)i & 0xFFFF);
        }

        public static IEnumerable<T> RangeSelect<T>(this int i, Converter<int, T> converter) {
            for(int j = 0; j < i; j++) {
                yield return converter(j);
            }
        }

        public static string GetDriveDisplayText(string path) {
            if((path.Length != 3) || !path.EndsWith(@":\")) {
                return String.Empty;
            }
            switch(PInvoke.GetDriveType(path)) {
                case 0:
                case 1:
                case 4:
                    return path;
            }
            return ShellMethods.GetDisplayName(path);
        }

        public static T GetRegistryValueSafe<T>(RegistryKey rk, string valName, T defaultVal) {
            object obj2 = rk.GetValue(valName, defaultVal);
            if((obj2 != null) && (obj2 is T)) {
                return (T)obj2;
            }
            return defaultVal;
        }

        public static void InitializeTemporaryPaths() {
            StaticReg.CreateWindowPaths.Clear();
            StaticReg.CreateWindowIDLs.Clear();
        }

        internal static IEnumerable<T> Interleave<T>(this IEnumerable<T> first, IEnumerable<T> second) {
            using(var enumerator1 = first.GetEnumerator())
            using(var enumerator2 = second.GetEnumerator()) {
                while(enumerator1.MoveNext()) {
                    yield return enumerator1.Current;
                    if(enumerator2.MoveNext()) {
                        yield return enumerator2.Current;
                    }
                }
                while(enumerator2.MoveNext()) {
                    yield return enumerator2.Current;
                }
            }
        }

        internal static void Invoke<T>(this T control, Action<T> action) where T : Control {
            control.Invoke(action, control);
        }

        internal static void Invoke(this Control control, Action action) {
            control.Invoke(action);
        }

        public static bool IsExecutable(string ext) {
            const string EXTS = ".COM|.EXE|.BAT|.CMD|.VBS|.VBE|.JS|.JSE|.WSF|.WSH|.MSC|.LNK";
            return ext != null && ext.Length > 2 && -1 != EXTS.IndexOf(ext.ToUpper());
        }

        public static bool IsNetworkPath(string path) {
            if(path.StartsWith("::")) {
                return false;
            }
            else if(path.StartsWith(@"\\")) {
                return true;
            }
            try {
                if(Path.IsPathRooted(path)) {
                    DriveInfo drive;
                    try {
                        drive = new DriveInfo(Path.GetPathRoot(path));
                    }
                    catch {
                        return false;
                    }
                    if(drive.DriveType == DriveType.Network) {
                        return true;
                    }
                }
            }
            catch {
            }
            return false;
        }

        public static bool IsShellPathButNotFileSystem(string path) {
            path = path.ToLower();
            return ((!path.StartsWith("http://") && !path.StartsWith("ftp://")) && !Path.IsPathRooted(path));
        }

        public static bool IsValidPathChar(char ch) {
            return (((((ch != '"') && (ch != '<')) && ((ch != '>') && (ch != '|'))) && (ch != '*')) && (ch != '?'));
        }

        public static int Make_INT(int x, int y) {
            return ((x & 0xffff) | ((y & 0xffff) << 0x10));
        }

        public static IntPtr Make_LPARAM(int x, int y) {
            return (IntPtr)((x & 0xffff) | ((y & 0xffff) << 0x10));
        }

        public static IntPtr Make_LPARAM(Point pt) {
            return (IntPtr)((pt.X & 0xffff) | ((pt.Y & 0xffff) << 0x10));
        }
        
        public static Color MakeColor(int colorref) {
            return Color.FromArgb(colorref & 0xff, (colorref >> 8) & 0xff, (colorref >> 0x10) & 0xff);
        }

        public static int MakeCOLORREF(Color clr) {
            return ((clr.R | (clr.G << 8)) | (clr.B << 0x10));
        }

        /**
         * force log
         */
        public static void flog(string optional)
        {
            StackTrace trace = new StackTrace();
            Dictionary<String, String> dic = new Dictionary<String, String>();
            if (trace != null)
            {
                StackFrame frame = trace.GetFrame(1);//1代表上级，2代表上上级，以此类推
                if (frame != null)
                {
                    MethodBase method = frame.GetMethod();
                    if (method != null)
                    {
                        dic.Add("methodName", method.Name);
                        if (method.ReflectedType != null)
                        {
                            String className = method.ReflectedType.Name;
                            dic.Add("className", className);
                        }
                    }
                }
            }
            log("flog", optional, dic);
        }

        public static void log(string optional)
        {
            if (ENABLE_LOGGER)
            {
                StackTrace trace = new StackTrace();
                Dictionary<String, String> dic = new Dictionary<String, String>();
                if (trace == null)
                {
                    // trace = Environment.StackTrace;
                }

                if (trace != null)
                {
                    StackFrame frame = trace.GetFrame(1);//1代表上级，2代表上上级，以此类推
                    if (frame != null)
                    {
                        MethodBase method = frame.GetMethod();
                        if (method != null)
                        {
                            dic.Add( "methodName", method.Name);
                            if (method.ReflectedType != null)
                            {
                                String className = method.ReflectedType.Name;
                                dic.Add("className", className);
                            }
                        }
                    }
                }
                log("log", optional, dic);
            }
        }


        public static void log2(string optional)
        {
            if (ENABLE_LOGGER)
            {
                log("log", optional);
            }
        }

        public static void err(string optional)
        {
            if (ENABLE_LOGGER)
            {
                StackTrace trace = new StackTrace();
                Dictionary<String, String> dic = new Dictionary<String, String>();
                if (trace == null)
                {
                    // trace = Environment.StackTrace;
                }

                if (trace != null)
                {
                    StackFrame frame = trace.GetFrame(1);//1代表上级，2代表上上级，以此类推
                    if (frame != null)
                    {
                        MethodBase method = frame.GetMethod();
                        if (method != null)
                        {
                            dic.Add("methodName", method.Name);
                            if (method.ReflectedType != null)
                            {
                                String className = method.ReflectedType.Name;
                                dic.Add("className", className);
                            }
                        }
                    }
                }
                log("err", optional, dic);
            }
        }

        // private static DateTime dateTime ;
        private static Dictionary<int, DateTime> dictTime = new Dictionary<int, DateTime>();
        // 忽略一些添加 日志
        private static string[] IGNORES = { "ReleaseComObject" };
        
        public static void log(string level, string optional,Dictionary<String, String> dic=null)
        {
            // ignore 
            if (null != IGNORES && IGNORES.Length > 0)
            {
                foreach (var ignore in IGNORES)
                {
                    var lower1 = ignore.ToLower();
                    var lower2 = optional.ToLower();
                    if (lower2.Contains(lower1))
                    {
                        return; // ignore ;
                    }
                }
            }

            /*
             var useTime = "";
             if (null != dateTime)
            {
                DateTime oldTime = dateTime;
                dateTime = DateTime.Now;
                useTime = "" + ((dateTime - oldTime).TotalMilliseconds) + "毫秒";
            }
            else
            {
                dateTime = DateTime.Now;
            }*/
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appdataQT = Path.Combine(appdata, "QTTabBar");
            if (!Directory.Exists(appdataQT))
            {
                Directory.CreateDirectory(appdataQT);
            }

            Process process = Process.GetCurrentProcess();
            var cThreadId = Thread.CurrentThread.ManagedThreadId;
            var currentThreadId = AppDomain.GetCurrentThreadId();
            if (null == cThreadId)
            {
                cThreadId = currentThreadId;
            } 

            var useTime = "";
            if (null != cThreadId)
            {
                if (null != dictTime)
                {
                    if (!dictTime.ContainsKey(cThreadId))
                    {
                        dictTime[cThreadId] = DateTime.Now;
                    }
                    var oldTime = dictTime[cThreadId];
                    if (null != oldTime)
                    {
                        useTime = "" + ((DateTime.Now - oldTime).TotalMilliseconds) + "毫秒";
                        dictTime[cThreadId] = DateTime.Now;
                    }
                }
                else
                {
                    ;
                    dictTime[cThreadId] = DateTime.Now;
                }
            } 

            string path = Path.Combine(appdataQT, "QTTabBarException.log");
            var line = new StringBuilder();
            line
                .Append("[")
                .Append(level)
                .Append("]");

            // add className and methodName debug
            if (null != dic && dic.Count > 0 && dic.ContainsKey("methodName") && dic.ContainsKey("className"))
            {
                // 输出类名和方法名
                if (dic.ContainsKey("className"))
                {
                    var className = dic["className"];
                    if (!string.IsNullOrEmpty(className))
                    {
                        line
                            .Append("\tC:")
                            .Append(className);
                    }
                }

                if (dic.ContainsKey("methodName"))
                {
                    var methodName = dic["methodName"];
                    if (!string.IsNullOrEmpty(methodName))
                    {
                        line
                            .Append("\tM:")
                            .Append(methodName);
                    }
                }
            }
            // 进程ID
            if (process != null)
            {
                line
                    .Append("\tP:")
                    .Append(process.Id);
            }
            // 线程 ID
            if (cThreadId != null)
            {
                line
                    .Append("\tT:")
                    .Append(cThreadId);
            }
            else if (currentThreadId != null)
            {
                line
                    .Append("\tT:")
                    .Append(currentThreadId);
            }

            if (!string.IsNullOrEmpty(useTime))
            {
                line
                    .Append("\tcost:")
                    .Append( useTime );
            }
            line
                .Append("\t")
                .Append(DateTime.Now.ToString())
                .Append("\t")
                .Append(optional);
            writeStr(path, line);
        }


        public static void MakeErrorLog(Exception ex, string optional = null) {
            try
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appdataQT = Path.Combine(appdata, "QTTabBar");
                if (!Directory.Exists(appdataQT))
                {
                    Directory.CreateDirectory(appdataQT);
                }
                string path = Path.Combine(appdataQT, "QTTabBarException.log");
                var line = new StringBuilder();
                line.AppendLine(DateTime.Now.ToString());
                line.AppendLine(".NET 版本: " + Environment.Version);
                line.AppendLine("操作系统版本: " + Environment.OSVersion.Version + 
                                " Major: " + Environment.OSVersion.Version.Major +
                                " 环境: " + getEnv()
                                );
                line.AppendLine("QT 版本: " + MakeVersionString());
                if (!String.IsNullOrEmpty(optional))
                {
                    line.AppendLine("错误信息: " + optional);
                }
                if (ex == null)
                {
                    line.AppendLine("Exception: None");
                    if (Environment.StackTrace != null)
                    {
                        line.AppendLine(Environment.StackTrace);
                    }
                }
                else
                {
                    line.AppendFormat("\nMessage ---\n{0}", ex.Message);
                    line.AppendFormat(
                        "\nHelpLink ---\n{0}", ex.HelpLink);
                    line.AppendFormat("\nSource ---\n{0}", ex.Source);
                    line.AppendFormat(
                        "\nStackTrace ---\n{0}", ex.StackTrace);
                    line.AppendFormat(
                        "\nTargetSite ---\n{0}", ex.TargetSite);

                    if (ex.InnerException != null)
                    {
                        line.AppendLine("****************InnnerExcetpion");
                        line.AppendFormat("\n  InnerMessage ---\n{0}", ex.InnerException.Message);
                        line.AppendFormat(
                            "\n InnerHelpLink ---\n{0}", ex.InnerException.HelpLink);
                        line.AppendFormat("\n  InnerSource ---\n{0}", ex.InnerException.Source);
                        line.AppendFormat(
                            "\n InnerStackTrace ---\n{0}", ex.InnerException.StackTrace);
                        line.AppendFormat(
                            "\n InnerTargetSite ---\n{0}", ex.InnerException.TargetSite);
                    }
                }
                line.AppendLine("--------------");
                line.AppendLine();

                writeStr(path, line);
            }
            catch {
            }
            finally {
            }
        }

        private static string getEnv()
        {
            if (4 == IntPtr.Size)
            {
                return "32";
            } else if (8 == IntPtr.Size)
            {
                return "64";
            }
            return "unknown";
        }

        /*
        public static object lockObject = new object();
        //读写锁，锁定文件写入权限，每个线程依次等待上个写入完成
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        */


        /*
        互斥锁Mutex
        定义：
            private static readonly Mutex mutex = new Mutex();
            使用：
            mutex.WaitOne();
            mutex.ReleaseMutex();
            作用：将会锁住代码块的内容，并阻止其他线程进入该代码块，直到该代码块运行完成，释放该锁。
         * Mutex本身是可以系统级别的，所以是可以跨越进程的。
         */
        private static readonly Mutex M_MUTEX = new Mutex();

        private static void writeStr(string path, StringBuilder formatLogLine)
        {
            try
            {
                M_MUTEX.WaitOne();
                //设置读写锁为写入模式独占资源
                //因写入模式的进入与释放在同一个代码块内，请保证在块内进入写入模式前不会触发异常，否则会因为进入与释放次数不符从而触发异常
                //请勿长时间占用读写锁否则会导致其他线程饥饿。
                // LogWriteLock.EnterWriteLock();
                // lock (lockObject) {

                // 修复 正由另一进程使用，因此该进程无法访问该文件
                if (File.Exists(path)) {
                    using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter sr = new StreamWriter(fs))
                        {
                            sr.WriteLine(formatLogLine);
                        }
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter sr = new StreamWriter(fs))
                        {
                            sr.WriteLine(formatLogLine);
                        }
                    }
                }

                // 存在问题 正由另一进程使用，因此该进程无法访问该文件
                    /*using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine(formatLogLine);
                    }
                     */
            }
            finally
            {
                //退出写入模式，释放资源占用
                //注意释放与进入次数相同否则会触发异常
                // LogWriteLock.ExitWriteLock();

                M_MUTEX.ReleaseMutex();
            }
            /*if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    writer.WriteLine(formatLogLine);
                    // Close(writer);
                    // Close(fs);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    writer.WriteLine(formatLogLine);
                    // Close(writer);
                    // Close(fs);
                }
            }*/
        }

        public static void Close(TextReader sr)
        {
            if (sr == null)
            {
                sr.Close();
                sr.Dispose();
            }
        }

        public static void Close(Stream stream)
        {
            if (stream == null)
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public static void Close(TextWriter sw)
        {
            if (sw == null)
            {
                sw.Close();
                sw.Dispose();
            }
        }

        public static void MakeErrorLog( string optional = null)
        {
            try
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appdataQT = Path.Combine(appdata, "QTTabBar");
                if (!Directory.Exists(appdataQT))
                {
                    Directory.CreateDirectory(appdataQT);
                }
                string path = Path.Combine(appdataQT, "QTTabBarException.log");
                using (StreamWriter writer = new StreamWriter(path, true))
                {

                    if (!String.IsNullOrEmpty(optional))
                    {
                        writer.WriteLine("错误信息: " + optional);
                    }
                   
                    writer.WriteLine("--------------");
                    writer.WriteLine();
                    Close( writer );
                }
                SystemSounds.Exclamation.Play();
            }
            catch
            {
            }
        }

        public static string MakeKeyString(Keys key) {
            if(key == Keys.None) {
                return " - ";
            }
            string str = String.Empty;
            if((key & Keys.Control) == Keys.Control) {
                str = "Ctrl + ";
            }
            if((key & Keys.Shift) == Keys.Shift) {
                str = str + "Shift + ";
            }
            if((key & Keys.Alt) == Keys.Alt) {
                str = str + "Alt + ";
            }
            return (str + ((key & Keys.KeyCode)));
        }

        public static Color MakeModColor(Color clr) {
            float num = 0.875f;
            return Color.FromArgb(((int)((0xff - clr.R) * num)) + clr.R, 
                ((int)((0xff - clr.G) * num)) + clr.G,
                ((int)((0xff - clr.B) * num)) + clr.B);
        }

        public static string MakeNameEllipsis(string name) {
            bool dummy;
            return MakeNameEllipsis(name, out dummy);
        }

        public static string MakeNameEllipsis(string name, out bool fTruncated) {
            fTruncated = false;
            if(name.Length > 40) {
                name = name.Substring(0, 0x25) + "...";
                fTruncated = true;
            }
            return name;
        }

        public static string MakePathDisplayText(string path, bool fToolTip) {
            int index = path.IndexOf("???");
            int length = path.IndexOf("*?*?*");
            if((index != -1) && IsShellPathButNotFileSystem(path)) {
                return path.Substring(0, index);
            }
            if(fToolTip && !path.StartsWith("::")) {
                if(length != -1) {
                    return path.Substring(0, length);
                }
                return path;
            }
            if(((path.Length == 3) && path.EndsWith(@":\")) || (path.StartsWith("::") || (length != -1))) {
                string driveDisplayText;
                if(QTUtility.DisplayNameCacheDic.TryGetValue(path, out driveDisplayText)) {
                    return driveDisplayText;
                }
                if(path.Length == 3) {
                    driveDisplayText = GetDriveDisplayText(path);
                }
                else if(length != -1) {
                    string str2 = path.Substring(0, length);
                    if(path.StartsWith("::")) {
                        driveDisplayText = ShellMethods.GetDisplayName(str2);
                    }
                    else {
                        driveDisplayText = str2;
                    }
                }
                else {
                    driveDisplayText = ShellMethods.GetDisplayName(path);
                }
                if(!String.IsNullOrEmpty(driveDisplayText)) {
                    QTUtility.DisplayNameCacheDic[path] = driveDisplayText;
                    return driveDisplayText;
                }
                return path;
            }
            if(!path.StartsWith("http://") && !path.StartsWith("ftp://")) {
                try {
                    string fileName = Path.GetFileName(path);
                    if(fileName.Length > 0x40) {
                        fileName = fileName.Substring(0, 0x3d) + "...";
                    }
                    return fileName;
                }
                catch (Exception e)
                {
                    QTUtility2.MakeErrorLog(e, "check http ftp");
                }
            }
            return path;
        }

        public static string MakeRootName(string path) {
            if((path == null) || (path.Length <= 2)) {
                return path;
            }
            if(path.StartsWith(@"\\")) {
                int index = path.IndexOf(@"\", 2);
                return index != -1 ? path.Substring(0, index) : path;
            }
            return path.Substring(0, 3);
        }

        public static string MakeVersionString() {
            // qwop comment  添加 .net framework 的版本号
            if(QTUtility.IS_DEV_VERSION) {
                return "DevBuild: " + QTUtility.GetLinkerTimestamp() + " (" + Environment.Version + ")";
            }
            else {
                string str = QTUtility.CurrentVersion.ToString();
                if(QTUtility.BetaRevision.Major > 0) {
                    str = str + " Beta " + QTUtility.BetaRevision.Major;
                }
                else if(QTUtility.BetaRevision.Minor > 0) {
                    str = str + " Alpha " + QTUtility.BetaRevision.Minor;
                }
                return str;
                /* */
            } 
        }

        public static bool PathEquals(this string str1, string str2) {
            return String.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        public static bool PathExists(string path) {
            if(String.IsNullOrEmpty(path)) {
                return false;
            }
            path = path.ToLower();
            if(path.StartsWith("::") || path.StartsWith(@"\\") || path.StartsWith("http://") || path.StartsWith("ftp://") || path.Contains("???")) {
                return true;
            }
            if(Path.IsPathRooted(path)) {
                DriveInfo drive;
                try {
                    drive = new DriveInfo(Path.GetPathRoot(path));
                }
                catch (Exception e)
                {
                    QTUtility2.MakeErrorLog(e, "new DriveInfo");
                    return false;
                }
                switch(drive.DriveType) {
                    case DriveType.Unknown:
                    case DriveType.NoRootDirectory:
                        return false;
                    case DriveType.Network:
                        return true;
                }
            }
            if(Directory.Exists(path)) {
                return true;
            }
            if(File.Exists(path)) {
                string ext = Path.GetExtension(path).ToLower();
                return (QTUtility.ExtIsCompressed(ext) || (!QTUtility.IsXP && (ext == ".search-ms")));
            }
            if(QTUtility.IsXP || ((!path.Contains(@".zip\") && !path.Contains(@".cab\")) && !path.Contains(@".lzh\"))) {
                return !Path.IsPathRooted(path);
            }
            string str2 = String.Empty;
            if(path.Contains(@".zip\")) {
                str2 = @".zip\";
            }
            else if(path.Contains(@".cab\")) {
                str2 = @".cab\";
            }
            else if(path.Contains(@".lzh\")) {
                str2 = @".lzh\";
            }
            return File.Exists(path.Substring(0, path.IndexOf(str2) + 4));
        }

        public static bool PathStartsWith(this string str1, string str2) {
            return str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);
        }

        public static Point PointFromLPARAM(IntPtr lParam) {
            return new Point(
                (short)(((int)lParam) & 0xffff),
                (short)((((int)lParam) >> 0x10) & 0xffff));
        }

        public static T[] ReadRegBinary<T>(string regValueName, RegistryKey rkUserApps) {
            byte[] buffer;
            try {
                buffer = (byte[])rkUserApps.GetValue(regValueName, null);
            }
            catch (Exception e)
            {
                QTUtility2.MakeErrorLog(e, "ReadRegBinary");
                return null;
            }
            if((buffer != null) && (buffer.Length > 0)) {
                using(MemoryStream stream = new MemoryStream(buffer)) {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (T[])formatter.Deserialize(stream);
                }
            }
            return null;
        }

        public static IntPtr ReadRegHandle(string valName, RegistryKey rk) {
            if(IntPtr.Size == 4) {
                object obj2 = rk.GetValue(valName, 0);
                if(obj2 is int) {
                    return (IntPtr)((int)obj2);
                }
                return (IntPtr)((uint)obj2);
            }
            else {
                object obj2 = rk.GetValue(valName, 0L);
                if(obj2 is long) {
                    return (IntPtr)((long)obj2);
                }
                return (IntPtr)((ulong)obj2);
            }
        }

        public static T GetValueSafe<T>(RegistryKey rk, string valName, T defaultVal)
        {
            object obj1 = rk.GetValue(valName, (object) defaultVal);
            return obj1 != null && obj1 is T ? (T) obj1 : defaultVal;
        }


        public static string SanitizePathString(string path) {
            if(path == null) {
                return null;
            }
            path = path.Trim();
            StringBuilder builder = new StringBuilder(path.Length);
            foreach(char ch in path) {
                if(IsValidPathChar(ch) && (ch > '\x001f')) {
                    builder.Append(ch);
                }
            }
            return builder.ToString();
        }

        public static IntPtr SendCOPYDATASTRUCT(IntPtr hWnd, IntPtr wParam, string strMsg, IntPtr dwData) {
            if(String.IsNullOrEmpty(strMsg)) {
                strMsg = "null";
            }
            using(SafePtr hglobal = new SafePtr(strMsg)) {
                COPYDATASTRUCT structure = new COPYDATASTRUCT {
                    lpData = hglobal,
                    cbData = (strMsg.Length + 1)*2,
                    dwData = dwData
                };
                return PInvoke.SendMessage(hWnd, WM.COPYDATA, wParam, ref structure);
            }
        }



        /**
         * 设置字符串到剪贴板
         */
        internal static void SetStringClipboard(string str) {
            try {
                Clipboard.SetDataObject(str, true);
                QTUtility.AsteriskPlay();
            }
            catch (Exception e)
            {
                QTUtility2.MakeErrorLog(e, "SetStringClipboard");
                QTUtility.SoundPlay();
            }
        }

        /**
         * 设置字符串到剪贴板
         */
        internal static string GetStringClipboard()
        {
            try
            {
                if (Clipboard.ContainsText(TextDataFormat.Text))
                {
                    string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                    QTUtility.AsteriskPlay();
                    return clipboardText;
                }
            }
            catch
            {
                QTUtility.SoundPlay();
            }
            return "";
        }
        // 字符串通过分隔符连接
        public static string StringJoin<T>(this IEnumerable<T> list, string separator) {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach(T t in list) {
                if(first) first = false;
                else sb.Append(separator);
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }
        // 字符串通过分隔符连接
        public static string StringJoin(this IEnumerable list, string separator) {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach(object t in list) {
                if(first) first = false;
                else sb.Append(separator);
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }

        public static string Replace(this string s, Regex regex, string replaceWith) {
            if (regex!=null) {
                s = regex.Replace(s, replaceWith);
            }
            return s;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIDL"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TargetIsInNoCapture(IntPtr pIDL, string path) {
            if(pIDL != IntPtr.Zero) {
                path = ShellMethods.GetPath(pIDL);
            }
            return !String.IsNullOrEmpty(path) && QTUtility.NoCapturePathsList.Any(path2 => path.PathEquals(path2));
        }

        /// <summary>
        ///  写入注册表， 关闭消息去除写入锁定标签的调用
        ///  qttabbarclass  public override void CloseDW(uint dwReserved)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="regValueName"></param>
        /// <param name="rkUserApps"></param>
        public static void WriteRegBinary<T>(T[] array, string regValueName, RegistryKey rkUserApps) {
            // 如果锁定标签路径有内容
            if ("TabsLocked".Equals(regValueName))
            {
                // MessageBox.Show("写入锁定标签");
                if (null != array && array.Length > 0)
                {
                    if (rkUserApps != null)
                    {
                        string[] newArray = (from string path in array
                                            // where Directory.Exists(path)
                                             where path.Trim().Length > 0 
                                             select path).ToArray();

                        if (null == newArray || newArray.Length == 0)
                        {
                            // MessageBox.Show("锁定标签数据为空：" + array.StringJoin(";"));
                            if (rkUserApps != null)
                            {
                                rkUserApps.SetValue("TabsLocked2", "");
                            }
                        }
                        else
                        {
                            //  MessageBox.Show("锁定标签数据为：" + array.StringJoin(";"));
                            rkUserApps.SetValue("TabsLocked2", newArray.StringJoin(";"));
                        }
                        /*
                        string value = array.StringJoin(";");
                        if (value.Trim().Length > 0)
                        {
                            rkUserApps.SetValue("TabsLocked2", value );
                        }
                        else {
                            rkUserApps.SetValue("TabsLocked2", "");
                        } */
                    }
                }
                else if (null == array || array.Length == 0  )
                {
                    //   MessageBox.Show("锁定标签数据为空：" + array.StringJoin(";"));
                    if (rkUserApps != null)
                    {
                        rkUserApps.SetValue("TabsLocked2", "");
                    }
                }
            }
            

            if(array != null) {
                byte[] buffer;
                BinaryFormatter formatter = new BinaryFormatter();
                using(MemoryStream stream = new MemoryStream()) {
                    formatter.Serialize(stream, array);
                    buffer = stream.GetBuffer();
                    Close( stream );
                }
                int num = 0;
                for(int i = 0; i < buffer.Length; i++) {
                    if(buffer[i] == 0) {
                        if(num == 0) {
                            num = i;
                        }
                    }
                    else {
                        num = 0;
                    }
                }
                byte[] buffer2 = new byte[num];
                if(num != 0) {
                    for(int j = 0; j < num; j++) {
                        buffer2[j] = buffer[j];
                    }
                }
                else {
                    buffer2 = buffer;
                }
                if(rkUserApps != null) {
                    rkUserApps.SetValue(regValueName, buffer2);
                }
            }
            // rkUserApps.Close();
        }

        public static void WriteRegHandle(string valName, RegistryKey rk, IntPtr hwnd) {
            if(IntPtr.Size == 4) {
                rk.SetValue(valName, (int)hwnd);
               // rk.Close();
            }
            else {
                rk.SetValue(valName, (long)hwnd, RegistryValueKind.QWord);
             //   rk.Close();
            }
        }

        // [MethodImpl(MethodImplOptions.InternalCall)]
        public static int Round(float f)
        {
            return (int)((double)f + ((double)f > 0.0 ? 0.5 : -0.5));
        }

        public static bool IsDrive(string path)
        {
            if (path != null)
            {
                path = ExpandEnvironmentVariables(path);
                if (path != null)
                {
                    path = path.ToLower();
                    if (path.Length == 2)
                        return 'a' <= path[0] && path[0] <= 'z' && path[1] == ':';
                    if (path.Length == 3 && 'a' <= path[0] && path[0] <= 'z' && path[1] == ':')
                        return path[2] == '\\';
                }
            }
            return false;
        }

        private static string ExpandEnvironmentVariables(string str)
        {
            try
            {
                if (str != null)
                    return Environment.ExpandEnvironmentVariables(str);
            }
            catch
            {
            }
            return str;
        }


        /// <summary>
        ///     A .NET framework 3.5 way to mimic the FX4 "Has Flag" method.
        /// </summary>
        /// <param name="variable">The tested enum</param>
        /// <param name="value">The value to test</param>
        /// <returns>True if the flag is set, otherwise false</returns>
        public static bool HasFlag(Enum variable, Enum value)
        {
            // check if from the same type.
            if (variable.GetType() != value.GetType())
                throw new ArgumentException("The checked flag is not from the same type as the checked variable.");

            Convert.ToUInt64(value);
            ulong num = Convert.ToUInt64(value);
            ulong num2 = Convert.ToUInt64(variable);

            return (num2 & num) == num;
        }

        public static string File2Text(string cTxtPath)
        {
            string value = "";
            using (StreamReader streamReader = new StreamReader(cTxtPath))
            {
                value = streamReader.ReadToEnd();
            }
            return value;
        }

        /// <summary>
        ///    用于调试消息
        /// </summary>
        /// <param name="msg"></param>
        public static void debugMessage(Message msg)
        {
            
            // if (msg.LParam != null && msg.LParam.ToString().Equals(File2Text(@"c:\1.txt")))
                if (msg.WParam != null && msg.WParam.ToString().Equals(File2Text(@"c:\1.txt")))
            {
                var name = Enum.GetName(typeof(MsgEnum), msg.Msg);
                if (
                    "WM_TIMER".Equals(name) ||
                    "RB_GETBANDBORDERS".Equals(name) ||
                    "WM_NCCALCSIZE".Equals(name) ||
                    "WM_IME_SETCONTEXT".Equals(name) ||
                    "WM_SHOWWINDOW".Equals(name)
                )
                {
                    // ignore 
                    return;
                }

                log("check msg\t " + Enum.GetName(typeof(MsgEnum), msg.Msg) + " msg int " + msg.Msg +
                               "\tw\t" + msg.WParam + "\tl\t" + msg.LParam);
            }
        }

        /// <summary>
        ///    用于调试消息
        /// </summary>
        /// <param name="msg"></param>
        public static void debugMessage(MSG msg)
        {
            // if (msg.lParam != null && msg.lParam.ToString().Equals(File2Text(@"c:\1.txt")))
            if (msg.wParam != null && msg.wParam.ToString().Equals(File2Text(@"c:\1.txt")))
            {
                var name = Enum.GetName(typeof(MsgEnum), msg.message);
                if ( 
                    "WM_TIMER".Equals(name) ||
                    "RB_GETBANDBORDERS".Equals(name) ||
                    "WM_NCCALCSIZE".Equals(name) ||
                    "WM_IME_SETCONTEXT".Equals(name) ||
                    "WM_SHOWWINDOW".Equals(name)
                    )
                {
                    // ignore 
                    return;
                }
                log("check msg\t " + name + " msg int " + msg.message +
                    "\tw\t" + msg.wParam + "\tl\t" + msg.lParam);
            }
        }

        public static void KillCurrentProcess()
        {
            // TASKKILL /T /PID 1230 /PID 1241 /PID 1253 
            // Process process = Process.GetCurrentProcess();
            // process.Kill();

            // Process.Start("TASKKILL /F /T /PID " + process.Id);
            /*string MyDosComLine1;
            MyDosComLine1 = "TASKKILL /F /T /PID " + process.Id;//返回根目录命令
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "cmd.exe ";//打开DOS控制平台 
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;//是否显示DOS窗口，true代表隐藏;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.Start();
            StreamWriter sIn = myProcess.StandardInput;//标准输入流 
            sIn.AutoFlush = true;
            StreamReader sOut = myProcess.StandardOutput;//标准输入流
            StreamReader sErr = myProcess.StandardError;//标准错误流 
            sIn.Write(MyDosComLine1 + Environment.NewLine);//第一条DOS命令
            log("write dos command: " + MyDosComLine1);
            sIn.Write("exit" + Environment.NewLine);//第四条DOS命令，退出DOS窗口
            if (myProcess.HasExited == false)
            {
                myProcess.Kill();
            }
            else
            {
            }
            sIn.Close();
            sOut.Close();
            sErr.Close();
            myProcess.Close();*/
        }

        public static void Wait4SelectFiles(WebBrowserClass explorer)
        {
            if (explorer != null)
            {
                explorer.Quit();
            }
        }

        public static bool IsEmpty(string text)
        {
            return null == text || text.Trim().Length == 0;
        }

        public static bool IsNotEmpty(string text)
        {
            return !IsEmpty(text);
        }

        public static int CurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }
    }
}

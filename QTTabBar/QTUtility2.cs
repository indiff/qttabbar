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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class QTUtility2 {
        private const int THRESHOLD_ELLIPSIS = 40;
        private static bool fConsoleAllocated;

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
                return (T)formatter.Deserialize(ms);
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

        public static void MakeErrorLog(Exception ex, string optional = null) {
            try {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appdataQT = Path.Combine(appdata, "QTTabBar");
                if(!Directory.Exists(appdataQT)) {
                    Directory.CreateDirectory(appdataQT);
                }
                string path = Path.Combine(appdataQT, "QTTabBarException.log");
                using(StreamWriter writer = new StreamWriter(path, true)) {
                    writer.WriteLine(DateTime.Now.ToString());
                    writer.WriteLine(".NET ver: " + Environment.Version);
                    writer.WriteLine("OS ver: " + Environment.OSVersion.Version);
                    writer.WriteLine("QT ver: " + MakeVersionString());
                    if(!String.IsNullOrEmpty(optional)) {
                        writer.WriteLine("Optional information: " + optional);
                    }
                    if(ex == null) {
                        writer.WriteLine("Exception: None");
                        writer.WriteLine(Environment.StackTrace);
                    }
                    else {
                        writer.WriteLine(ex.ToString());
                    }                        
                    writer.WriteLine("--------------");
                    writer.WriteLine();
                }
                SystemSounds.Exclamation.Play();
            }
            catch {
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
            return Color.FromArgb(((int)((0xff - clr.R) * num)) + clr.R, ((int)((0xff - clr.G) * num)) + clr.G, ((int)((0xff - clr.B) * num)) + clr.B);
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
                catch {
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
            if(QTUtility.IS_DEV_VERSION) {
                return "DevBuild: " + QTUtility.GetLinkerTimestamp();
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
                catch {
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
            catch {
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

        internal static void SetStringClipboard(string str) {
            try {
                Clipboard.SetDataObject(str, true);
                SystemSounds.Asterisk.Play();
            }
            catch {
                SystemSounds.Hand.Play();
            }
        }

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

        public static bool TargetIsInNoCapture(IntPtr pIDL, string path) {
            if(pIDL != IntPtr.Zero) {
                path = ShellMethods.GetPath(pIDL);
            }
            return !String.IsNullOrEmpty(path) && QTUtility.NoCapturePathsList.Any(path2 => path.PathEquals(path2));
        }

        public static void WriteRegBinary<T>(T[] array, string regValueName, RegistryKey rkUserApps) {
            if(array != null) {
                byte[] buffer;
                BinaryFormatter formatter = new BinaryFormatter();
                using(MemoryStream stream = new MemoryStream()) {
                    formatter.Serialize(stream, array);
                    buffer = stream.GetBuffer();
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
        }

        public static void WriteRegHandle(string valName, RegistryKey rk, IntPtr hwnd) {
            if(IntPtr.Size == 4) {
                rk.SetValue(valName, (int)hwnd);
            }
            else {
                rk.SetValue(valName, (long)hwnd, RegistryValueKind.QWord);
            }
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace QTTabBarLib {

    static class RegFileWriter {
        // implements exporting of registry key as a reg file
        // to avoid UAC dialog caused by using Regedit.exe
        const string NEWLINE = "\r\n";
        const bool fNewLineForBinary = true;

        public static void Export(string keyName, string filePath) {
            using(RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyName)) {
                StringBuilder sb = new StringBuilder("Windows Registry Editor Version 5.00" + NEWLINE + NEWLINE);

                buildSubkeyString(rk, sb);

                using(FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                    // reg file is encoded by UTF16LE with BOM
                    using(StreamWriter sw = new StreamWriter(fs, new UnicodeEncoding(false, true))) {
                        sw.Write(sb.ToString());
                    }
                }
            }
        }


        private static void buildSubkeyString(RegistryKey rk, StringBuilder sb) {
            // exclude volatile keys
            // TODO: make this more general
            if(rk.Name == @"HKEY_CURRENT_USER\Software\QTTabBar\Cache") {
                return;
            }
            sb.Append(readValues(rk));

            foreach(string subKeyName in rk.GetSubKeyNames()) {
                using(RegistryKey rkSub = rk.OpenSubKey(subKeyName)) {
                    buildSubkeyString(rkSub, sb);
                }
            }
        }

        private static string readValues(RegistryKey rk) {
            string s = "";
            foreach(string valName in rk.GetValueNames()) {
                switch(rk.GetValueKind(valName)) {
                    case RegistryValueKind.Binary:
                        s += binaryToString(rk, valName);
                        break;

                    case RegistryValueKind.QWord:
                        s += qwordToString(rk, valName);
                        break;

                    case RegistryValueKind.DWord:
                        s += dwordToString(rk, valName);
                        break;

                    case RegistryValueKind.String:
                        s += szToString(rk, valName);
                        break;

                    case RegistryValueKind.ExpandString:
                        s += expandSzToString(rk, valName);
                        break;

                    case RegistryValueKind.MultiString:
                        s += multiSzToString(rk, valName);
                        break;
                }
            }
            return s.Length > 0
                    ? "[" + rk.Name + "]" + NEWLINE + s + NEWLINE
                    : "";
        }


        private static string binaryToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=hex:" + byteArrayToString((byte[])rk.GetValue(valName)) + NEWLINE;
        }

        private static string qwordToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=hex(b):" + byteArrayToString(BitConverter.GetBytes((long)rk.GetValue(valName))) + NEWLINE;
        }

        private static string dwordToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=dword:" + ((int)rk.GetValue(valName)).ToString("x8") + NEWLINE;
        }

        private static string szToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=\"" + ((string)rk.GetValue(valName)).Replace(@"\", @"\\") + "\"" + NEWLINE;
        }

        private static string expandSzToString(RegistryKey rk, string valName) {
            //REG_EXPAND_SZ
            return "\"" + sanitizeValName(valName) + "\"=hex(2):" + byteArrayToString(new UnicodeEncoding().GetBytes((string)rk.GetValue(valName) + "\0")) + NEWLINE;
        }

        private static string multiSzToString(RegistryKey rk, string valName) {
            //REG_MULTI_SZ
            string str = ((string[])rk.GetValue(valName)).StringJoin("\0") + "\0\0";
            return "\"" + sanitizeValName(valName) + "\"=hex(7):" + byteArrayToString(new UnicodeEncoding().GetBytes(str)) + NEWLINE;
        }

        private static string sanitizeValName(string str) {
            return str.Replace(@"\", @"\\");
        }

        private static string byteArrayToString(byte[] bytes) {
            StringBuilder sb = new StringBuilder();
            int c = 0, n = 20;
            for(int i = 0; i < bytes.Length; i++) {
                sb.Append(bytes[i].ToString("x2"));
                if(i == bytes.Length - 1) continue;
                sb.Append(",");

                if(fNewLineForBinary) {
                    c++;
                    if(c == n) {
                        sb.Append("\\" + NEWLINE + "  ");
                        c = 0;
                        n = 25;
                    }
                }
            }
            return sb.ToString();
        }
    }

    class SafePtr : IDisposable {
        private IntPtr ptr;

        public SafePtr(int size) {
            ptr = Marshal.AllocHGlobal(size);
        }

        public SafePtr(string str, bool unicode = true) {
            ptr = unicode ? Marshal.StringToHGlobalUni(str) : Marshal.StringToHGlobalAnsi(str);
        }

        public static implicit operator IntPtr(SafePtr safePtr) {
            return safePtr.ptr;
        }

        public void Dispose() {
            if(ptr != IntPtr.Zero) {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }

    // Normally, delegates are only serializable if they don't include any
    // stack variables.  But using this class, we can serialize any delegate.
    [Serializable]
    public class SerializeDelegate : ISerializable {
        public Delegate Delegate { get; private set; }
        public SerializeDelegate(Delegate del) {
            Delegate = del;
        }

        public SerializeDelegate(SerializationInfo info, StreamingContext context) {
            Type delType = (Type)info.GetValue("delegateType", typeof(Type));

            //If it's a "simple" delegate we just read it straight off
            if(info.GetBoolean("isSerializable")) {
                Delegate = (Delegate)info.GetValue("delegate", delType);
            }
            //otherwise, we need to read its anonymous class
            else {
                MethodInfo method = (MethodInfo)info.GetValue("method", typeof(MethodInfo));
                AnonymousClassWrapper w = (AnonymousClassWrapper)info.GetValue("class", typeof(AnonymousClassWrapper));
                Delegate = Delegate.CreateDelegate(delType, w.obj, method);
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("delegateType", Delegate.GetType());

            //If it's an "simple" delegate we can serialize it directly
            if(Delegate != null && (Delegate.Target == null || Delegate.Method.DeclaringType.GetCustomAttributes(
                    typeof(SerializableAttribute), false).Length > 0)) {
                info.AddValue("isSerializable", true);
                info.AddValue("delegate", Delegate);
            }
            //otherwise, serialize anonymous class
            else {
                info.AddValue("isSerializable", false);
                info.AddValue("method", Delegate.Method);
                info.AddValue("class", new AnonymousClassWrapper(Delegate.Method.DeclaringType, Delegate.Target));
            }
        }

        [Serializable]
        private class AnonymousClassWrapper : ISerializable {
            private Type type;
            public object obj;
            
            internal AnonymousClassWrapper(Type type, object obj) {
                this.type = type;
                this.obj = obj;
            }

            internal AnonymousClassWrapper(SerializationInfo info, StreamingContext context) {
                Type classType = (Type)info.GetValue("classType", typeof(Type));
                obj = Activator.CreateInstance(classType);

                foreach(FieldInfo field in classType.GetFields()) {
                    //If the field is a delegate
                    if(typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                        field.SetValue(obj, ((SerializeDelegate)info.GetValue(field.Name, typeof(SerializeDelegate))).Delegate);
                    }
                    //If the field is an anonymous class
                    else if(!field.FieldType.IsSerializable) {
                        field.SetValue(obj, ((AnonymousClassWrapper)info.GetValue(field.Name, typeof(AnonymousClassWrapper))).obj);
                    }
                    //otherwise
                    else {
                        field.SetValue(obj, info.GetValue(field.Name, field.FieldType));
                    }
                }
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
                info.AddValue("classType", type);

                foreach(FieldInfo field in type.GetFields()) {
                    //See corresponding comments above
                    if(typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                        info.AddValue(field.Name, new SerializeDelegate((Delegate)field.GetValue(obj)));
                    }
                    else if(!field.FieldType.IsSerializable) {
                        Debug.Assert(field.Name.Contains("<>")); // compiler-generated only
                        info.AddValue(field.Name, new AnonymousClassWrapper(field.FieldType, field.GetValue(obj)));
                    }
                    else {
                        info.AddValue(field.Name, field.GetValue(obj));
                    }
                }
            }
        }
    }

    internal sealed class StackDictionary<S, T> {
        private Dictionary<S, T> dictionary;
        private List<S> lstKeys;

        public StackDictionary() {
            lstKeys = new List<S>();
            dictionary = new Dictionary<S, T>();
        }

        public T Peek() {
            S local;
            return popPeekInternal(false, out local);
        }

        public T Peek(out S key) {
            return popPeekInternal(false, out key);
        }

        public T Pop() {
            S local;
            return popPeekInternal(true, out local);
        }

        public T Pop(out S key) {
            return popPeekInternal(true, out key);
        }

        private T popPeekInternal(bool fPop, out S lastKey) {
            if(lstKeys.Count == 0) {
                throw new InvalidOperationException("This StackDictionary is empty.");
            }
            lastKey = lstKeys[lstKeys.Count - 1];
            T local = dictionary[lastKey];
            if(fPop) {
                lstKeys.RemoveAt(lstKeys.Count - 1);
                dictionary.Remove(lastKey);
            }
            return local;
        }

        public void Push(S key, T value) {
            lstKeys.Remove(key);
            lstKeys.Add(key);
            dictionary[key] = value;
        }

        public bool Remove(S key) {
            lstKeys.Remove(key);
            return dictionary.Remove(key);
        }

        public int RemoveAllValues(Predicate<T> match) {
            var removeMe = lstKeys.Where(s => match(dictionary[s])).ToList();
            foreach(var s in removeMe) {
                lstKeys.Remove(s);
                dictionary.Remove(s);
            }
            return removeMe.Count;
        }

        public bool TryGetValue(S key, out T value) {
            return dictionary.TryGetValue(key, out value);
        }

        public int Count { get { return lstKeys.Count; } }

        public ICollection<S> Keys { get { return dictionary.Keys; } }

        public ICollection<T> Values { get { return dictionary.Values; } }
    }

    internal class Keychain : IDisposable {
        private ReaderWriterLock rwlock;
        private bool write;

        public Keychain(ReaderWriterLock rwlock, bool write) {
            this.rwlock = rwlock;
            this.write = write;
            if(write) {
                rwlock.AcquireWriterLock(Timeout.Infinite);
            }
            else {
                rwlock.AcquireReaderLock(Timeout.Infinite);
            }
        }

        public void Dispose() {
            if(rwlock == null) return;
            if(write) {
                rwlock.ReleaseWriterLock();
            }
            else {
                rwlock.ReleaseReaderLock();
            }
            rwlock = null;
        }
    }

    // Delegate.BeginInvoke is stupid because it leaks if you don't call EndInvoke.
    // This class implements fire-and-forget functionality.
    internal static class AsyncHelper {
        private class TargetInfo {
            public TargetInfo(Delegate d, object[] args, int delay) {
                Target = d;
                Args = args;
                Delay = delay;
            }
            public readonly Delegate Target;
            public readonly object[] Args;
            public readonly int Delay;
        }

        public static void BeginInvoke(int delayMillis, Delegate d, params object[] args) {
            ThreadPool.QueueUserWorkItem(DynamicInvokeCallback, new TargetInfo(d, args, delayMillis));
        }

        public static void BeginInvoke(Delegate d, params object[] args) {
            ThreadPool.QueueUserWorkItem(DynamicInvokeCallback, new TargetInfo(d, args, 0));
        }

        private static void DynamicInvokeCallback(object state) {
            TargetInfo ti = (TargetInfo)state;
            try {
                if(ti.Delay > 0) Thread.Sleep(ti.Delay);
                ti.Target.DynamicInvoke(ti.Args);
            }
            catch(Exception ex) {
                QTUtility2.MakeErrorLog(ex, "AsyncHelper");
            }
        }
    }

    [Serializable]
    internal class DisList<T> : List<T>, IDisposable where T : IDisposable {
        public DisList() {
        }

        public DisList(IEnumerable<T> col) : base(col) {
        }

        public void Dispose() {
            foreach(T t in this) {
                try {
                    t.Dispose();
                }
                catch {
                }
            }
            Clear();
        }
    }
}


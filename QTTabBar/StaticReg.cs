using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace QTTabBarLib {
    internal static class StaticReg {
        internal static string CreateWindowGroup {
            get { return (string)ReadProp("CreateWindowGroup") ?? ""; }
            set { WriteProp("CreateWindowGroup", value); }
        }

        internal static bool SkipNextCapture {
            get { return (int)(ReadProp("SkipNextCapture") ?? 0) != 0; }
            set { WriteProp("SkipNextCapture", value ? 1 : 0); }
        }

        internal static UniqueList<string> ExecutedPathsList = new UniqueList<string>(16); // todo
        internal static UniqueList<string> ClosedTabHistoryList = new UniqueList<string>(16); // todo

        private static RegBackedList<string> _LockedTabsToRestoreList = new RegBackedList<string>("LockedTabs");
        internal static RegBackedList<string> LockedTabsToRestoreList {
            get {
                _LockedTabsToRestoreList.Update();
                return _LockedTabsToRestoreList;
            }
        }

        private static RegBackedList<byte[]> _CreateWindowIDLs = new RegBackedList<byte[]>("CreateWindowIDLs");
        internal static RegBackedList<byte[]> CreateWindowIDLs {
            get {
                _CreateWindowIDLs.Update();
                return _CreateWindowIDLs;
            }
        }

        private static RegBackedList<string> _CreateWindowPaths = new RegBackedList<string>("CreateWindowPaths");
        internal static RegBackedList<string> CreateWindowPaths {
            get {
                _CreateWindowPaths.Update();
                return _CreateWindowPaths;
            }
        }

        private static object ReadProp(string key) {
            using(RegistryKey reg = Registry.CurrentUser.CreateSubKey(RegConst.StaticReg)) {
                return reg.GetValue(key);
            }
        }

        private static void WriteProp(string key, object value) {
            using(RegistryKey reg = Registry.CurrentUser.CreateSubKey(RegConst.StaticReg)) {
                reg.SetValue(key, value);
            }
        }
    }

    internal sealed class RegBackedList<T> : Collection<T> {
        private string key;
        private int lastUpdate = 0;
        private bool updating = false;
        public RegBackedList(IList<T> list, string key) : base(list) {
            this.key = key;
        }

        public RegBackedList(string key) {
            this.key = key;
        }

        protected override void SetItem(int index, T item) {
            base.SetItem(index, item);
            if(updating) return;
            Update();
            using(RegistryKey reg = Registry.CurrentUser.CreateSubKey(RegConst.StaticReg + key)) {
                reg.SetValue("" + index, item);
                reg.SetValue("", ++lastUpdate);
            }
        }

        protected override void RemoveItem(int index) {
            base.RemoveItem(index);
            if(updating) return;
            Update();
            using(RegistryKey reg = Registry.CurrentUser.CreateSubKey(RegConst.StaticReg + key)) {
                for(int i = index; i < Count; i++) {
                    reg.SetValue("" + i, this[i]);
                }
                reg.DeleteValue("" + Count);
                reg.SetValue("", ++lastUpdate);
            }
        }

        protected override void InsertItem(int index, T item) {
            base.InsertItem(index, item);
            if(updating) return;
            Update();
            using(RegistryKey reg = Registry.CurrentUser.CreateSubKey(RegConst.StaticReg + key)) {
                for(int i = index; i < Count; i++) {
                    reg.SetValue("" + i, this[i]);
                }
                reg.SetValue("", ++lastUpdate);
            }
        }

        protected override void ClearItems() {
            base.ClearItems();
            if(updating) return;
            Update();
            using(RegistryKey reg = Registry.CurrentUser.CreateSubKey(RegConst.StaticReg + key)) {
                foreach(string name in reg.GetValueNames()) {
                    reg.DeleteValue(name);
                }
                reg.SetValue("", ++lastUpdate);
            }            
        }

        public void Update() {
            using(RegistryKey reg = Registry.CurrentUser.OpenSubKey(RegConst.StaticReg + key)) {
                if(reg == null) return;
                int update = (int)reg.GetValue("", 0);
                if(update == lastUpdate) return;
                lastUpdate = update;
                updating = true;
                Clear();
                for(int i = 0;; i++) {
                    object obj = reg.GetValue("" + i);
                    if(obj == null) break;
                    Add((T)obj);
                }
                updating = false;
            }
        }

        public void AddRange(IEnumerable<T> range) {
            foreach(T t in range) {
                Add(t); // todo: make more efficient
            }
        }

        public void Assign(IEnumerable<T> collection) {
            Clear(); // todo: make more efficient
            AddRange(collection);
        }
    }
}

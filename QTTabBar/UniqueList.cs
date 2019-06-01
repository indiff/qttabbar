using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace QTTabBarLib {
    sealed class UniqueList<T> : Collection<T> {
        private int maxCapacity;
        private EqualityComparer<T> equalityComparer;

        public UniqueList(int maxCapacity = 0, EqualityComparer<T> equalityComparer = null) {
            this.maxCapacity = maxCapacity;
            this.equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }
        
        public UniqueList(IEnumerable<T> init, int maxCapacity = 0, EqualityComparer<T> equalityComparer = null)
            : this(maxCapacity, equalityComparer) {
            foreach(T t in init) {
                Add(t);
            }
        }

        public UniqueList(UniqueList<T> ul)
            : this(ul, ul.maxCapacity, ul.equalityComparer) {
        }

        protected override void InsertItem(int index, T item) {
            int i = EnsureUnique(item);
            if(i != -1 && i < index) {
                index--;
            }
            base.InsertItem(index, item);
            EnsureCount();
        }

        public new bool Remove(T item) {
            EqualityComparer<T> comparer = equalityComparer ?? EqualityComparer<T>.Default;
            for(int i = 0; i < Count; i++) {
                if(!comparer.Equals(item, base[i])) continue;
                RemoveItem(i);
                return true;
            }
            return false;
        }

        // make indexer read-only
        public new T this[int index] {
            get { return base[index]; }
        }

        public int MaxCapacity {
            get {
                return maxCapacity;
            }
            set {
                maxCapacity = Math.Max(0, value);
                EnsureCount();
            }
        }

        private int EnsureUnique(T item) {
            EqualityComparer<T> comparer = equalityComparer ?? EqualityComparer<T>.Default;
            for(int i = 0; i < Count; i++) {
                if(!comparer.Equals(item, base[i])) continue;
                RemoveItem(i);
                return i;
            }
            return -1;
        }

        private void EnsureCount() {
            if(maxCapacity < 1) return;
            while(Count > maxCapacity) {
                RemoveItem(0);
            }
        }
    }

    sealed class StringEqualityComparer : EqualityComparer<string> {
        private static StringEqualityComparer sec;

        private StringEqualityComparer() {
        }

        public override bool Equals(string x, string y) {
            return String.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode(string obj) {
            return obj.GetHashCode();
        }

        public static StringEqualityComparer CaseInsensitiveComparer {
            get { return sec ?? (sec = new StringEqualityComparer()); }
        }
    }

    /*
     * TODO: This is from Quizo.  Can we use it?
    sealed class RecentFileListComparer : EqualityComparer<string[]> {
        private static RecentFileListComparer rflc;

        private RecentFileListComparer() {
        }

        public override bool Equals(string[] x, string[] y) {
            if(x != null && y != null && x.Length == y.Length) {
                // ignore 4th element ( menu title )
                for(int i = 0; i < x.Length; i++) {
                    if(i != 3 && !String.Equals(x[i], y[i], StringComparison.OrdinalIgnoreCase)) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode(string[] obj) {
            return obj.GetHashCode();
        }

        public static RecentFileListComparer CaseInsensitiveComparer {
            get { return rflc ?? (rflc = new RecentFileListComparer()); }
        }
    }*/
}

//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Quizo, Paul Accisano
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
using System.Runtime.InteropServices;
using System.Text;

namespace QuizoPlugins {
    sealed class MigemoWrapper : IDisposable {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        internal static extern int lstrlenA(IntPtr ptr);

        private delegate IntPtr migemo_open(string dict);
        private delegate IntPtr migemo_query(IntPtr pMigemo, IntPtr query);
        private delegate void migemo_release(IntPtr pMigemo, IntPtr stringToRelease);
        private delegate void migemo_close(IntPtr pMigemo);
        private delegate int migemo_is_enable(IntPtr pMigemo);

        private migemo_query mQuery;
        private migemo_release mRelease;
        private migemo_close mClose;
        private migemo_is_enable mIsEnable;

        private IntPtr pMigemo;
        private IntPtr hModuleMigemo;

        public MigemoWrapper(string pathMigemoDll, string pathDict) {
            if(!String.IsNullOrEmpty(pathMigemoDll) && !String.IsNullOrEmpty(pathDict)) {
                hModuleMigemo = LoadLibrary(pathMigemoDll);

                if(hModuleMigemo != IntPtr.Zero) {
                    IntPtr pOpen = GetProcAddress(hModuleMigemo, "migemo_open");
                    IntPtr pQuery = GetProcAddress(hModuleMigemo, "migemo_query");
                    IntPtr pRelease = GetProcAddress(hModuleMigemo, "migemo_release");
                    IntPtr pClose = GetProcAddress(hModuleMigemo, "migemo_close");
                    IntPtr pIsEnable = GetProcAddress(hModuleMigemo, "migemo_is_enable");

                    bool fSuccess = pOpen != IntPtr.Zero &&
                                    pQuery != IntPtr.Zero &&
                                    pRelease != IntPtr.Zero &&
                                    pClose != IntPtr.Zero &&
                                    pIsEnable != IntPtr.Zero;

                    if(fSuccess) {
                        migemo_open mOpen = Marshal.GetDelegateForFunctionPointer(pOpen, typeof(migemo_open)) as migemo_open;
                        mQuery = Marshal.GetDelegateForFunctionPointer(pQuery, typeof(migemo_query)) as migemo_query;
                        mRelease = Marshal.GetDelegateForFunctionPointer(pRelease, typeof(migemo_release)) as migemo_release;
                        mClose = Marshal.GetDelegateForFunctionPointer(pClose, typeof(migemo_close)) as migemo_close;
                        mIsEnable = Marshal.GetDelegateForFunctionPointer(pIsEnable, typeof(migemo_is_enable)) as migemo_is_enable;

                        if(mOpen != null &&
                            mQuery != null &&
                            mRelease != null &&
                            mClose != null &&
                            mIsEnable != null) {
                            pMigemo = mOpen(pathDict);

                            if(IsEnable) {
                                return;
                            }
                            else if(pMigemo != IntPtr.Zero) {
                                mClose(pMigemo);
                            }
                        }
                    }

                    FreeLibrary(hModuleMigemo);
                    hModuleMigemo = IntPtr.Zero;
                }
            }

            throw new ArgumentException();
        }

        public string QueryRegexStr(string strQuery) {
            if(IsEnable && strQuery != null) {
                IntPtr pRegexStr = IntPtr.Zero;
                IntPtr pSourceStr = IntPtr.Zero;
                try {
                    byte[] bytes = Encoding.UTF8.GetBytes(strQuery);
                    pSourceStr = Marshal.AllocHGlobal(bytes.Length+1);
                    Marshal.Copy(bytes, 0, pSourceStr, bytes.Length);
                    Marshal.WriteByte(pSourceStr, bytes.Length, 0);
                    pRegexStr = mQuery(pMigemo, pSourceStr);

                    if(pRegexStr != IntPtr.Zero) {
                        int nb = lstrlenA(pRegexStr);
                        if(nb > 0) {
                            bytes = new byte[nb];
                            Marshal.Copy(pRegexStr, bytes, 0, nb);
                            return Encoding.UTF8.GetString(bytes); ;
                        }
                    }
                }
                finally {
                    if(pSourceStr != IntPtr.Zero)
                        Marshal.FreeHGlobal(pSourceStr);
                    if(pRegexStr != IntPtr.Zero)
                        mRelease(pMigemo, pRegexStr);
                }
            }
            return strQuery;
        }

        public bool IsEnable {
            get {
                return pMigemo != IntPtr.Zero && 0 != mIsEnable(pMigemo);
            }
        }

        #region IDisposable member

        public void Dispose() {
            if(pMigemo != IntPtr.Zero) {
                mClose(pMigemo);
                pMigemo = IntPtr.Zero;
            }
            if(hModuleMigemo != IntPtr.Zero) {
                FreeLibrary(hModuleMigemo);
                hModuleMigemo = IntPtr.Zero;
            }
        }

        #endregion

    }
}

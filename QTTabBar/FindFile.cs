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
using System.Runtime.InteropServices;
using System.Security;

namespace QTTabBarLib {
    internal sealed class FindFile : IDisposable {
        private const int ERROR_NO_MORE_FILES = 0x12;
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_HIDDEN = 2;
        private const int FILE_ATTRIBUTE_SYSTEM = 4;
        private bool fSearchHidden;
        private bool fSearchSystem;
        private bool fSubDirectoryFound;
        private bool fSubFileFound;
        private IntPtr hFindFile;
        private int iLastError;
        private static IntPtr INVALID_HANDLE_VALUE = ((IntPtr)(-1));
        private const int MAX_WAITFORFILES = 0x20;
        private string path;

        public FindFile(string path, bool fSearchHidden, bool fSearchSystem) {
            this.path = path;
            if(!this.path.EndsWith(@"\")) {
                this.path = this.path + @"\";
            }
            this.path = this.path + "*";
            this.fSearchHidden = fSearchHidden;
            this.fSearchSystem = fSearchSystem;
            if(this.path.StartsWith(@"\\")) {
                fSubDirectoryFound = fSubFileFound = true;
            }
        }

        public void Dispose() {
            if((hFindFile != IntPtr.Zero) && (hFindFile != INVALID_HANDLE_VALUE)) {
                FindClose(hFindFile);
                path = null;
                hFindFile = IntPtr.Zero;
            }
        }

        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool FindClose(IntPtr hFindFile);
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string fileName, [In, Out] WIN32_FIND_DATA lpFindFileData);
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool FindNextFile(IntPtr hFindFile, [In, Out] WIN32_FIND_DATA lpFindFileData);
        [SuppressUnmanagedCodeSecurity, DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int SetErrorMode(int uMode);
        public bool SubDirectoryExists() {
            return subObjectExists(true);
        }

        public bool SubFileExists() {
            return subObjectExists(false);
        }

        private bool subObjectExists(bool fDirectory) {
            if(hFindFile != INVALID_HANDLE_VALUE) {
                if((fDirectory && fSubDirectoryFound) || (!fDirectory && fSubFileFound)) {
                    return true;
                }
                if(iLastError == 0x12) {
                    return false;
                }
                int num = 0;
                WIN32_FIND_DATA lpFindFileData = new WIN32_FIND_DATA();
                int uMode = SetErrorMode(1);
                try {
                    if(hFindFile == IntPtr.Zero) {
                        hFindFile = FindFirstFile(path, lpFindFileData);
                    }
                    if(hFindFile != INVALID_HANDLE_VALUE) {
                        do {
                            if(((lpFindFileData.cFileName != null) && (lpFindFileData.cFileName != ".")) && (lpFindFileData.cFileName != "..")) {
                                if((fSearchHidden || ((lpFindFileData.dwFileAttributes & 2) == 0)) && (fSearchSystem || ((lpFindFileData.dwFileAttributes & 4) == 0))) {
                                    if((lpFindFileData.dwFileAttributes & 0x10) != 0) {
                                        fSubDirectoryFound = true;
                                        if(fDirectory) {
                                            return true;
                                        }
                                    }
                                    else {
                                        fSubFileFound = true;
                                        if(!fDirectory) {
                                            return true;
                                        }
                                    }
                                }
                                if(++num > 0x20) {
                                    if(fDirectory) {
                                        fSubDirectoryFound = true;
                                    }
                                    else {
                                        fSubFileFound = true;
                                    }
                                    return true;
                                }
                            }
                        }
                        while(FindNextFile(hFindFile, lpFindFileData));
                        iLastError = Marshal.GetLastWin32Error();
                    }
                }
                finally {
                    SetErrorMode(uMode);
                }
            }
            return false;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class WIN32_FIND_DATA {
            public int dwFileAttributes;
            public int ftCreationTime_dwLowDateTime;
            public int ftCreationTime_dwHighDateTime;
            public int ftLastAccessTime_dwLowDateTime;
            public int ftLastAccessTime_dwHighDateTime;
            public int ftLastWriteTime_dwLowDateTime;
            public int ftLastWriteTime_dwHighDateTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }
    }
}

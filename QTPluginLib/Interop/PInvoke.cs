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
using System.Text;

namespace QTPlugin.Interop {
    public static class PInvoke {
        public static IntPtr CreatePIDL(byte[] data) {
            if((data == null) || (data.Length == 0)) {
                return IntPtr.Zero;
            }
            int length = data.Length;
            IntPtr zero;
            try {
                zero = Marshal.AllocCoTaskMem(length);
            }
            catch {
                return IntPtr.Zero;
            }
            Marshal.Copy(data, 0, zero, length);
            return zero;
        }

        public static byte[] GetIDListData(IntPtr pidl) {
            if(pidl != IntPtr.Zero) {
                uint num = ILGetSize(pidl);
                if(num != 0) {
                    byte[] destination = new byte[num];
                    Marshal.Copy(pidl, destination, 0, (int)num);
                    return destination;
                }
            }
            return null;
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr ILClone(IntPtr pidl);
        [DllImport("shell32.dll")]
        public static extern uint ILGetSize(IntPtr pidl);
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrRetToBuf(ref STRRET pstr, IntPtr pidl, StringBuilder pszBuf, uint cchBuf);
    }
}

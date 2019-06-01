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

namespace QTPlugin.Interop {
    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public struct VARIANT {
        [FieldOffset(8)]
        public byte bValue;
        [FieldOffset(8)]
        public double dValue;
        [FieldOffset(8)]
        public float fValue;
        [FieldOffset(8)]
        public int iValue;
        [FieldOffset(8)]
        public long lValue;
        [FieldOffset(8)]
        public IntPtr pValue;
        [FieldOffset(8)]
        public short sValue;
        [FieldOffset(0)]
        public short vt;
        [FieldOffset(2)]
        public short wReserved1;
        [FieldOffset(4)]
        public short wReserved2;
        [FieldOffset(6)]
        public short wReserved3;
    }
}

//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2002-2010  Pavel Zolnikov, Quizo, Paul Accisano
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

namespace BandObjectLib {
    [Flags]
    public enum DBIMF : uint {
        NORMAL = 0x0000,
        MINSIZE = 0x0001,
        MAXSIZE = 0x0002,
        INTEGRAL = 0x0004,
        VARIABLEHEIGHT = 0x0008,
        TITLE = 0x0010,
        MODEFLAGS = 0x0020,
        BKCOLOR = 0x0040,
        USECHEVRON = 0x0080,
        BREAK = 0x0100,
        ADDTOFRONT = 0x0200,
        TOPALIGN = 0x0400,
        NOGRIPPER = 0x0800,
        ALWAYSGRIPPER = 0x1000,
        NOMARGINS = 0x2000
    }
}

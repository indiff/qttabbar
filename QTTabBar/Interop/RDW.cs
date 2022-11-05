//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano, indiff
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

namespace QTTabBarLib.Interop
{
  [Flags]
  public enum RDW
  {
    INVALIDATE = 1,
    INTERNALPAINT = 2,
    ERASE = 4,
    VALIDATE = 8,
    NOINTERNALPAINT = 16, // 0x00000010
    NOERASE = 32, // 0x00000020
    NOCHILDREN = 64, // 0x00000040
    ALLCHILDREN = 128, // 0x00000080
    UPDATENOW = 256, // 0x00000100
    ERASENOW = 512, // 0x00000200
    FRAME = 1024, // 0x00000400
    NOFRAME = 2048, // 0x00000800
  }
}

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

    /// <summary>
    /// Represents different styles of a band object.
    /// </summary>
    [Flags]
    [Serializable]
    public enum BandObjectStyle : uint {
        Vertical = 1,
        Horizontal = 2,
        ExplorerToolbar = 4,
        TaskbarToolBar = 8
    }

    /// <summary>
    /// Specifies Style of the band object, its Name(displayed in explorer menu) and HelpText(displayed in status bar when menu command selected).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BandObjectAttribute : Attribute {
        public BandObjectAttribute() { }

        public BandObjectAttribute(string name, BandObjectStyle style) {
            Name = name;
            Style = style;
        }
        public BandObjectStyle Style;
        public string Name;
        public string HelpText;
    }
}

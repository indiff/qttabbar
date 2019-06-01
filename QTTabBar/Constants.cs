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


namespace QTTabBarLib {

    // This file will be for various global constants.

    internal static class RegConst {
        public const string Root = @"Software\QTTabBar\";
        public const string Config = @"Config\";
        public const string Groups = @"Groups\";
        public const string Apps = @"UserApps\";
        public const string StaticReg = @"Shared\";
    }

    internal static class MC {
        public const int TAB_FIRST = 0;
        public const int TAB_OPEN_IDL = 4;
        public const int TAB_OPEN_IDLS = 9;
        public const int TAB_REPLACEBYGROUP = 80;
    }
}

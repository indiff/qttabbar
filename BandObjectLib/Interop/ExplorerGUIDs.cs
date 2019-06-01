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
    public abstract class ExplorerGUIDs {
        public static readonly Guid CGID_DeskBand = new Guid("{EB0FE172-1A3A-11D0-89B3-00A0C90A90AC}");
        public static readonly Guid CLSID_TaskbarList = new Guid("56FDF344-FD6D-11d0-958A-006097C9A090");
        public static readonly Guid IID_IContextMenu = new Guid("{000214e4-0000-0000-c000-000000000046}");
        public static readonly Guid IID_IDataObject = new Guid("{0000010e-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IDropTarget = new Guid("00000122-0000-0000-C000-000000000046");
        public static readonly Guid IID_IDropTargetHelper = new Guid("4657278B-411B-11D2-839A-00C04FD918D0");
        public static readonly Guid IID_IEnumIDList = new Guid("{000214F2-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IExtractImage = new Guid("BB2E617C-0920-11d1-9A0B-00C04FC2D6C1");
        public static readonly Guid IID_IPersistFolder2 = new Guid("{1AC3D9F0-175C-11d1-95BE-00609797EA4F}");
        public static readonly Guid IID_IQueryInfo = new Guid("00021500-0000-0000-c000-000000000046");
        public static readonly Guid IID_IShellBrowser = new Guid("{000214E2-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IShellFolder = new Guid("{000214E6-0000-0000-C000-000000000046}");
        public static readonly Guid IID_ITaskbarList = new Guid("56FDF342-FD6D-11d0-958A-006097C9A090");
        public static readonly Guid IID_ITravelLogStg = new Guid("{7EBFDD80-AD18-11d3-A4C5-00C04F72D6B8}");
        public static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IWebBrowserApp = new Guid("{0002DF05-0000-0000-C000-000000000046}");
    }
}

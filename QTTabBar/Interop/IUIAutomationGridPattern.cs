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

using System.Runtime.InteropServices;

namespace QTTabBarLib.Interop {
    [ComImport, Guid("414c3cdc-856b-4f5b-8538-3131c6302550"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationGridPattern {
        void GetItem(int row, int column, out IUIAutomationElement element);
        void get_CurrentRowCount(out int retVal);
        void get_CurrentColumnCount(out int retVal);
        void get_CachedRowCount(out int retVal);
        void get_CachedColumnCount(out int retVal);
    }
}

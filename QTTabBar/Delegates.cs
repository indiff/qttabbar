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
using System.Windows.Forms;

namespace QTTabBarLib {
    internal delegate void FormMethodInvoker(object obj);
    internal delegate void ItemRightClickedEventHandler(object sender, ItemRightClickedEventArgs e);
    internal delegate void MenuReorderedEventHandler(object sender, ToolStripItemClickedEventArgs e);
    internal delegate void NavigationCompleteCallback(object obj, IntPtr ptr);
    internal delegate void QEventHandler(object sender, QEventArgs e);
    internal delegate void QTabCancelEventHandler(object sender, QTabCancelEventArgs e);
    internal delegate void WaitTimeoutCallback(int msec);
    internal delegate bool FolderClickedHandler(IDLWrapper item, Keys modkeys, bool middle);
}

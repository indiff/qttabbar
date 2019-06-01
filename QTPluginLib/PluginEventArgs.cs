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

namespace QTPlugin {
    public sealed class PluginEventArgs : EventArgs {
        private Address address;
        private int index;
        private ExplorerWindowActions windowAction;

        public PluginEventArgs(ExplorerWindowActions windowAction) {
            this.windowAction = windowAction;
        }

        public PluginEventArgs(int index, Address address) {
            this.index = index;
            this.address = address;
        }

        public Address Address {
            get {
                return address;
            }
        }

        public int Index {
            get {
                return index;
            }
        }

        public ExplorerWindowActions WindowAction {
            get {
                return windowAction;
            }
        }
    }
}

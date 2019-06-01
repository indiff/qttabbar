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
    internal sealed class ImageReservationKey {
        private int iReservedType;
        private string reservedKey;

        public ImageReservationKey(string key, int iReservedType) {
            reservedKey = key;
            this.iReservedType = iReservedType;
        }

        public string ImageKey {
            get {
                return reservedKey;
            }
        }

        public int ImageType {
            get {
                return iReservedType;
            }
        }
    }
}

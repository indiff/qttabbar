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
    [ComImport, Guid("88f4d42a-e881-459d-a77c-73bbbb7e02dc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationScrollPattern {
        void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount);
        void SetScrollPercent(double horizontalPercent, double verticalPercent);
        void get_CurrentHorizontalScrollPercent(out double retVal);
        void get_CurrentVerticalScrollPercent(out double retVal);
        void get_CurrentHorizontalViewSize(out double retVal);
        void get_CurrentVerticalViewSize(out double retVal);
        void get_CurrentHorizontallyScrollable(out bool retVal);
        void get_CurrentVerticallyScrollable(out bool retVal);
        void get_CachedHorizontalScrollPercent(out double retVal);
        void get_CachedVerticalScrollPercent(out double retVal);
        void get_CachedHorizontalViewSize(out double retVal);
        void get_CachedVerticalViewSize(out double retVal);
        void get_CachedHorizontallyScrollable(out bool retVal);
        void get_CachedVerticallyScrollable(out bool retVal);
    }

    public enum ScrollAmount {
        LargeDecrement = 0,
        SmallDecrement = 1,
        NoAmount = 2,
        LargeIncrement = 3,
        SmallIncrement = 4
    }
}

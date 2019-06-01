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
    [ComImport, Guid("4042c624-389c-4afc-a630-9df854a541fc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationTreeWalker {
        void GetParentElement(IUIAutomationElement element, out IUIAutomationElement parent);
        void GetFirstChildElement(IUIAutomationElement element, out IUIAutomationElement first);
        void GetLastChildElement(IUIAutomationElement element, out IUIAutomationElement last);
        void GetNextSiblingElement(IUIAutomationElement element, out IUIAutomationElement next);
        void GetPreviousSiblingElement(IUIAutomationElement element, out IUIAutomationElement previous);
        void NormalizeElement(IUIAutomationElement element, out IUIAutomationElement normalized);
        /* NOT DECLARED */ void GetParentElementBuildCache();
        /* NOT DECLARED */ void GetFirstChildElementBuildCache();
        /* NOT DECLARED */ void GetLastChildElementBuildCache();
        /* NOT DECLARED */ void GetNextSiblingElementBuildCache();
        /* NOT DECLARED */ void GetPreviousSiblingElementBuildCache();
        /* NOT DECLARED */ void NormalizeElementBuildCache();
        /* NOT DECLARED */ void get_Condition();
    }
}

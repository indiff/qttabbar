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
using System.Drawing;
using System.Runtime.InteropServices;

namespace QTTabBarLib.Interop {
    [ComImport, Guid("30cbe57d-d9d0-452a-ab13-7ac5ac4825ee"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomation {
        /* NOT DECLARED */ void CompareElements();
        /* NOT DECLARED */ void CompareRuntimeIds();
        /* NOT DECLARED */ void GetRootElement();
        void ElementFromHandle(IntPtr hwnd, out IUIAutomationElement element);
        void ElementFromPoint(Point pt, out IUIAutomationElement element);
        void GetFocusedElement(out IUIAutomationElement element);
        /* NOT DECLARED */ void GetRootElementBuildCache();
        /* NOT DECLARED */ void ElementFromHandleBuildCache();
        /* NOT DECLARED */ void ElementFromPointBuildCache();
        /* NOT DECLARED */ void GetFocusedElementBuildCache();
        /* NOT DECLARED */ void CreateTreeWalker();
        void get_ControlViewWalker(out IUIAutomationTreeWalker walker);
        /* NOT DECLARED */ void get_ContentViewWalker();
        /* NOT DECLARED */ void get_RawViewWalker();
        /* NOT DECLARED */ void get_RawViewCondition();
        /* NOT DECLARED */ void get_ControlViewCondition();
        /* NOT DECLARED */ void get_ContentViewCondition();
        /* NOT DECLARED */ void CreateCacheRequest();
        /* NOT DECLARED */ void CreateTrueCondition();
        /* NOT DECLARED */ void CreateFalseCondition();
        /* NOT DECLARED */ void CreatePropertyCondition();
        /* NOT DECLARED */ void CreatePropertyConditionEx();
        /* NOT DECLARED */ void CreateAndCondition();
        /* NOT DECLARED */ void CreateAndConditionFromArray();
        /* NOT DECLARED */ void CreateAndConditionFromNativeArray();
        /* NOT DECLARED */ void CreateOrCondition();
        /* NOT DECLARED */ void CreateOrConditionFromArray();
        /* NOT DECLARED */ void CreateOrConditionFromNativeArray();
        /* NOT DECLARED */ void CreateNotCondition();
        /* NOT DECLARED */ void AddAutomationEventHandler();
        /* NOT DECLARED */ void RemoveAutomationEventHandler();
        /* NOT DECLARED */ void AddPropertyChangedEventHandlerNativeArray();
        /* NOT DECLARED */ void AddPropertyChangedEventHandler();
        /* NOT DECLARED */ void RemovePropertyChangedEventHandler();
        /* NOT DECLARED */ void AddStructureChangedEventHandler();
        /* NOT DECLARED */ void RemoveStructureChangedEventHandler();
        /* NOT DECLARED */ void AddFocusChangedEventHandler();
        /* NOT DECLARED */ void RemoveFocusChangedEventHandler();
        /* NOT DECLARED */ void RemoveAllEventHandlers();
        /* NOT DECLARED */ void IntNativeArrayToSafeArray();
        /* NOT DECLARED */ void IntSafeArrayToNativeArray();
        /* NOT DECLARED */ void RectToVariant();
        /* NOT DECLARED */ void VariantToRect();
        /* NOT DECLARED */ void SafeArrayToRectNativeArray();
        /* NOT DECLARED */ void CreateProxyFactoryEntry();
        /* NOT DECLARED */ void get_ProxyFactoryMapping();
        /* NOT DECLARED */ void GetPropertyProgrammaticName();
        /* NOT DECLARED */ void GetPatternProgrammaticName();
        /* NOT DECLARED */ void PollForPotentialSupportedPatterns();
        /* NOT DECLARED */ void PollForPotentialSupportedProperties();
        /* NOT DECLARED */ void CheckNotSupported();
        /* NOT DECLARED */ void get_ReservedNotSupportedValue();
        /* NOT DECLARED */ void get_ReservedMixedAttributeValue();
        /* NOT DECLARED */ void ElementFromIAccessible();
        /* NOT DECLARED */ void ElementFromIAccessibleBuildCache();
    };
}

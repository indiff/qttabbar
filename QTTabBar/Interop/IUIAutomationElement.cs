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
    [ComImport, Guid("d22108aa-8ac5-49a5-837b-37bbb3d7591e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUIAutomationElement {
        /* NOT DECLARED */ void SetFocus();
        /* NOT DECLARED */ void GetRuntimeId();
        /* NOT DECLARED */ void FindFirst();
        /* NOT DECLARED */ void FindAll();
        /* NOT DECLARED */ void FindFirstBuildCache();
        /* NOT DECLARED */ void FindAllBuildCache();
        /* NOT DECLARED */ void BuildUpdatedCache();
        void GetCurrentPropertyValue(int propertyId, out object retVal);
        /* NOT DECLARED */ void GetCurrentPropertyValueEx();
        /* NOT DECLARED */ void GetCachedPropertyValue();
        /* NOT DECLARED */ void GetCachedPropertyValueEx();
        /* NOT DECLARED */ void GetCurrentPatternAs();
        /* NOT DECLARED */ void GetCachedPatternAs();
        void GetCurrentPattern(int patternId, [MarshalAs(UnmanagedType.IUnknown)] out object patternObject);
        /* NOT DECLARED */ void GetCachedPattern();
        /* NOT DECLARED */ void GetCachedParent();
        /* NOT DECLARED */ void GetCachedChildren();
        /* NOT DECLARED */ void get_CurrentProcessId();
        /* NOT DECLARED */ void get_CurrentControlType();
        /* NOT DECLARED */ void get_CurrentLocalizedControlType();
        /* NOT DECLARED */ void get_CurrentName();
        /* NOT DECLARED */ void get_CurrentAcceleratorKey();
        /* NOT DECLARED */ void get_CurrentAccessKey();
        /* NOT DECLARED */ void get_CurrentHasKeyboardFocus();
        /* NOT DECLARED */ void get_CurrentIsKeyboardFocusable();
        /* NOT DECLARED */ void get_CurrentIsEnabled();
        /* NOT DECLARED */ void get_CurrentAutomationId();
        /* NOT DECLARED */ void get_CurrentClassName();
        /* NOT DECLARED */ void get_CurrentHelpText();
        /* NOT DECLARED */ void get_CurrentCulture();
        /* NOT DECLARED */ void get_CurrentIsControlElement();
        /* NOT DECLARED */ void get_CurrentIsContentElement();
        /* NOT DECLARED */ void get_CurrentIsPassword();
        /* NOT DECLARED */ void get_CurrentNativeWindowHandle();
        /* NOT DECLARED */ void get_CurrentItemType();
        /* NOT DECLARED */ void get_CurrentIsOffscreen();
        /* NOT DECLARED */ void get_CurrentOrientation();
        /* NOT DECLARED */ void get_CurrentFrameworkId();
        /* NOT DECLARED */ void get_CurrentIsRequiredForForm();
        /* NOT DECLARED */ void get_CurrentItemStatus();
        /* NOT DECLARED */ void get_CurrentBoundingRectangle();
        /* NOT DECLARED */ void get_CurrentLabeledBy();
        /* NOT DECLARED */ void get_CurrentAriaRole();
        /* NOT DECLARED */ void get_CurrentAriaProperties();
        /* NOT DECLARED */ void get_CurrentIsDataValidForForm();
        /* NOT DECLARED */ void get_CurrentControllerFor();
        /* NOT DECLARED */ void get_CurrentDescribedBy();
        /* NOT DECLARED */ void get_CurrentFlowsTo();
        /* NOT DECLARED */ void get_CurrentProviderDescription();
        /* NOT DECLARED */ void get_CachedProcessId();
        /* NOT DECLARED */ void get_CachedControlType();
        /* NOT DECLARED */ void get_CachedLocalizedControlType();
        /* NOT DECLARED */ void get_CachedName();
        /* NOT DECLARED */ void get_CachedAcceleratorKey();
        /* NOT DECLARED */ void get_CachedAccessKey();
        /* NOT DECLARED */ void get_CachedHasKeyboardFocus();
        /* NOT DECLARED */ void get_CachedIsKeyboardFocusable();
        /* NOT DECLARED */ void get_CachedIsEnabled();
        /* NOT DECLARED */ void get_CachedAutomationId();
        /* NOT DECLARED */ void get_CachedClassName();
        /* NOT DECLARED */ void get_CachedHelpText();
        /* NOT DECLARED */ void get_CachedCulture();
        /* NOT DECLARED */ void get_CachedIsControlElement();
        /* NOT DECLARED */ void get_CachedIsContentElement();
        /* NOT DECLARED */ void get_CachedIsPassword();
        /* NOT DECLARED */ void get_CachedNativeWindowHandle();
        /* NOT DECLARED */ void get_CachedItemType();
        /* NOT DECLARED */ void get_CachedIsOffscreen();
        /* NOT DECLARED */ void get_CachedOrientation();
        /* NOT DECLARED */ void get_CachedFrameworkId();
        /* NOT DECLARED */ void get_CachedIsRequiredForForm();
        /* NOT DECLARED */ void get_CachedItemStatus();
        /* NOT DECLARED */ void get_CachedBoundingRectangle();
        /* NOT DECLARED */ void get_CachedLabeledBy();
        /* NOT DECLARED */ void get_CachedAriaRole();
        /* NOT DECLARED */ void get_CachedAriaProperties();
        /* NOT DECLARED */ void get_CachedIsDataValidForForm();
        /* NOT DECLARED */ void get_CachedControllerFor();
        /* NOT DECLARED */ void get_CachedDescribedBy();
        /* NOT DECLARED */ void get_CachedFlowsTo();
        /* NOT DECLARED */ void get_CachedProviderDescription();
        /* NOT DECLARED */ void GetClickablePoint();
    };
}

//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    internal static class PropertySystemNativeMethods
    {
        internal enum RelativeDescriptionType
        {
            General,
            Date,
            Size,
            Count,
            Revision,
            Length,
            Duration,
            Speed,
            Rate,
            Rating,
            Priority
        }

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int PSGetNameFromPropertyKey(
            ref PropertyKey propkey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszCanonicalName
        );

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern HResult PSGetPropertyDescription(
            ref PropertyKey propkey,
            ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPropertyDescription ppv
        );

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int PSGetPropertyDescriptionListFromString(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPropList,
            [In] ref Guid riid,
            out IPropertyDescriptionList ppv
        );

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int PSGetPropertyKeyFromName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszCanonicalName,
            out PropertyKey propkey
        );
    }
}
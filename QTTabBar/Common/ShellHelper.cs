//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.IO;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>A helper class for Shell Objects</summary>
    internal static class ShellHelper
    {
        internal static PropertyKey ItemTypePropertyKey = new PropertyKey(new Guid("28636AA6-953D-11D2-B5D6-00C04FD918D0"), 11);

        internal static string GetAbsolutePath(string path)
        {
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                return path;
            }
            return Path.GetFullPath((path));
        }

        internal static string GetItemType(IShellItem2 shellItem)
        {
            if (shellItem != null)
            {
                string itemType;
                var hr = shellItem.GetString(ref ItemTypePropertyKey, out itemType);
                if (hr == HResult.Ok) { return itemType; }
            }

            return null;
        }

        internal static string GetParsingName(IShellItem shellItem)
        {
            if (shellItem == null) { return null; }

            string path = null;

            var pszPath = IntPtr.Zero;
            var hr = shellItem.GetDisplayName(ShellNativeMethods.ShellItemDesignNameOptions.DesktopAbsoluteParsing, out pszPath);

            if (hr != HResult.Ok && hr != HResult.InvalidArguments)
            {
                throw new ShellException(LocalizedMessages.ShellHelperGetParsingNameFailed, hr);
            }

            if (pszPath != IntPtr.Zero)
            {
                path = Marshal.PtrToStringAuto(pszPath);
                Marshal.FreeCoTaskMem(pszPath);
                pszPath = IntPtr.Zero;
            }

            return path;
        }

        internal static IntPtr PidlFromParsingName(string name)
        {
            IntPtr pidl;
            ShellNativeMethods.ShellFileGetAttributesOptions sfgao;
            var retCode = ShellNativeMethods.SHParseDisplayName(
                name, IntPtr.Zero, out pidl, 0,
                out sfgao);

            return (CoreErrorHelper.Succeeded(retCode) ? pidl : IntPtr.Zero);
        }

        internal static IntPtr PidlFromShellItem(IShellItem nativeShellItem)
        {
            var unknown = Marshal.GetIUnknownForObject(nativeShellItem);
            return PidlFromUnknown(unknown);
        }

        internal static IntPtr PidlFromUnknown(IntPtr unknown)
        {
            IntPtr pidl;
            var retCode = ShellNativeMethods.SHGetIDListFromObject(unknown, out pidl);
            return (CoreErrorHelper.Succeeded(retCode) ? pidl : IntPtr.Zero);
        }
    }
}
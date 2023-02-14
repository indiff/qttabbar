//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace QTTabBarLib.Common
{
    /// <summary>
    /// Internal class that contains interop declarations for functions that are considered benign but that are performance critical.
    /// </summary>
    /// <remarks>Functions that are benign but not performance critical should be located in the NativeMethods class.</remarks>
    [SuppressUnmanagedCodeSecurity]
    internal static class KnownFoldersSafeNativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeFolderDefinition
        {
            internal FolderCategory category;
            internal IntPtr name;
            internal IntPtr description;
            internal Guid parentId;
            internal IntPtr relativePath;
            internal IntPtr parsingName;
            internal IntPtr tooltip;
            internal IntPtr localizedName;
            internal IntPtr icon;
            internal IntPtr security;
            internal uint attributes;
            internal DefinitionOptions definitionOptions;
            internal Guid folderTypeId;
        }
    }
}
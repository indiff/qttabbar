// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>Creates the helper class for known folders.</summary>
    public static class KnownFolderHelper
    {
        /// <summary>Returns the known folder given its canonical name.</summary>
        /// <param name="canonicalName">A non-localized canonical name for the known folder, such as MyComputer.</param>
        /// <returns>A known folder representing the specified name.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the given canonical name is invalid or if the KnownFolder could not be created.
        /// </exception>
        public static IKnownFolder FromCanonicalName(string canonicalName)
        {
            var knownFolderManager = (IKnownFolderManager)new KnownFolderManagerClass();
            IKnownFolderNative knownFolderNative;
            knownFolderManager.GetFolderByName(canonicalName, out knownFolderNative);
            var kf = KnownFolderHelper.GetKnownFolder(knownFolderNative);

            if (kf == null)
            {
                throw new ArgumentException(LocalizedMessages.ShellInvalidCanonicalName, "canonicalName");
            }
            return kf;
        }

        /// <summary>Returns a known folder given a globally unique identifier.</summary>
        /// <param name="knownFolderId">A GUID for the requested known folder.</param>
        /// <returns>A known folder representing the specified name.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the given Known Folder ID is invalid.</exception>
        public static IKnownFolder FromKnownFolderId(Guid knownFolderId)
        {
            var knownFolderManager = new KnownFolderManagerClass();
            IKnownFolderNative knownFolderNative;
            var hr = knownFolderManager.GetFolder(knownFolderId, out knownFolderNative);
            if (hr != HResult.Ok) { throw new ShellException(hr); }

            var kf = GetKnownFolder(knownFolderNative);
            if (kf == null)
            {
                throw new ArgumentException(LocalizedMessages.KnownFolderInvalidGuid, "knownFolderId");
            }
            return kf;
        }

        /// <summary>
        /// Returns a known folder given its shell namespace parsing name, such as <c>::{645FF040-5081-101B-9F08-00AA002F954E}</c> for the
        /// Recycle Bin.
        /// </summary>
        /// <param name="parsingName">The parsing name (or path) for the requested known folder.</param>
        /// <returns>A known folder representing the specified name.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the given parsing name is invalid.</exception>
        public static IKnownFolder FromParsingName(string parsingName)
        {
            if (parsingName == null)
            {
                throw new ArgumentNullException("parsingName");
            }

            var pidl = IntPtr.Zero;
            var pidl2 = IntPtr.Zero;

            try
            {
                pidl = ShellHelper.PidlFromParsingName(parsingName);

                if (pidl == IntPtr.Zero)
                {
                    throw new ArgumentException(LocalizedMessages.KnownFolderParsingName, "parsingName");
                }

                // It's probably a special folder, try to get it
                var knownFolderNative = KnownFolderHelper.FromPIDL(pidl);
                if (knownFolderNative != null)
                {
                    var kf = KnownFolderHelper.GetKnownFolder(knownFolderNative);
                    if (kf == null)
                    {
                        throw new ArgumentException(LocalizedMessages.KnownFolderParsingName, "parsingName");
                    }
                    return kf;
                }

                // No physical storage was found for this known folder We'll try again with a different name

                // try one more time with a trailing \0
                pidl2 = ShellHelper.PidlFromParsingName(parsingName.PadRight(1, '\0'));

                if (pidl2 == IntPtr.Zero)
                {
                    throw new ArgumentException(LocalizedMessages.KnownFolderParsingName, "parsingName");
                }

                var kf2 = KnownFolderHelper.GetKnownFolder(KnownFolderHelper.FromPIDL(pidl));
                if (kf2 == null)
                {
                    throw new ArgumentException(LocalizedMessages.KnownFolderParsingName, "parsingName");
                }

                return kf2;
            }
            finally
            {
                ShellNativeMethods.ILFree(pidl);
                ShellNativeMethods.ILFree(pidl2);
            }
        }

        /// <summary>
        /// Returns a known folder given its shell path, such as <c>C:\users\public\documents</c> or
        /// <c>::{645FF040-5081-101B-9F08-00AA002F954E}</c> for the Recycle Bin.
        /// </summary>
        /// <param name="path">The path for the requested known folder; either a physical path or a virtual path.</param>
        /// <returns>A known folder representing the specified name.</returns>
        public static IKnownFolder FromPath(string path)
        {
            return KnownFolderHelper.FromParsingName(path);
        }

        /// <summary>Returns a known folder given a globally unique identifier.</summary>
        /// <param name="knownFolderId">A GUID for the requested known folder.</param>
        /// <returns>A known folder representing the specified name. Returns null if Known Folder is not found or could not be created.</returns>
        internal static IKnownFolder FromKnownFolderIdInternal(Guid knownFolderId)
        {
            var knownFolderManager = (IKnownFolderManager)new KnownFolderManagerClass();
            IKnownFolderNative knownFolderNative;
            var hr = knownFolderManager.GetFolder(knownFolderId, out knownFolderNative);

            return (hr == HResult.Ok) ? GetKnownFolder(knownFolderNative) : null;
        }

        /// <summary>Returns the native known folder (IKnownFolderNative) given a PID list</summary>
        /// <param name="pidl"></param>
        /// <returns></returns>
        internal static IKnownFolderNative FromPIDL(IntPtr pidl)
        {
            var knownFolderManager = new KnownFolderManagerClass();
            IKnownFolderNative knownFolder;
            var hr = knownFolderManager.FindFolderFromIDList(pidl, out knownFolder);

            return (hr == HResult.Ok) ? knownFolder : null;
        }

        /// <summary>
        /// Given a native KnownFolder (IKnownFolderNative), create the right type of IKnownFolder object (FileSystemKnownFolder or NonFileSystemKnownFolder)
        /// </summary>
        /// <param name="knownFolderNative">Native Known Folder</param>
        /// <returns></returns>
        private static IKnownFolder GetKnownFolder(IKnownFolderNative knownFolderNative)
        {
            Debug.Assert(knownFolderNative != null, "Native IKnownFolder should not be null.");

            // Get the native IShellItem2 from the native IKnownFolder
            var guid = new Guid(ShellIIDGuid.IShellItem2);
            IShellItem2 shellItem;
            var hr = knownFolderNative.GetShellItem(0, ref guid, out shellItem);

            if (!CoreErrorHelper.Succeeded(hr)) { return null; }

            var isFileSystem = false;

            // If we have a valid IShellItem, try to get the FileSystem attribute.
            if (shellItem != null)
            {
                ShellNativeMethods.ShellFileGetAttributesOptions sfgao;
                shellItem.GetAttributes(ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem, out sfgao);

                // Is this item a FileSystem item?
                isFileSystem = (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem) != 0;
            }

            // If it's FileSystem, create a FileSystemKnownFolder, else NonFileSystemKnownFolder
            if (isFileSystem)
            {
                var kf = new FileSystemKnownFolder(knownFolderNative);
                return kf;
            }

            var knownFsFolder = new NonFileSystemKnownFolder(knownFolderNative);
            return knownFsFolder;
        }
    }
}
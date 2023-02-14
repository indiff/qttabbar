// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    internal static class ShellObjectFactory
    {
        /// <summary>Creates a ShellObject given a native IShellItem interface</summary>
        /// <param name="nativeShellItem"></param>
        /// <returns>A newly constructed ShellObject object</returns>
        internal static ShellObject Create(IShellItem nativeShellItem)
        {
            // Sanity check
            Debug.Assert(nativeShellItem != null, "nativeShellItem should not be null");

            // Need to make sure we're running on Vista or higher
            if (!CoreHelpers.RunningOnVista)
            {
                throw new PlatformNotSupportedException(LocalizedMessages.ShellObjectFactoryPlatformNotSupported);
            }

            // A lot of APIs need IShellItem2, so just keep a copy of it here
            var nativeShellItem2 = nativeShellItem as IShellItem2;

            // Get the System.ItemType property
            var itemType = ShellHelper.GetItemType(nativeShellItem2);

            if (!string.IsNullOrEmpty(itemType)) { itemType = itemType.ToLowerInvariant(); }

            ShellNativeMethods.ShellFileGetAttributesOptions sfgao;
            // Get some IShellItem attributes
            nativeShellItem2.GetAttributes(ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem | ShellNativeMethods.ShellFileGetAttributesOptions.Folder, out sfgao);

            // Is this item a FileSystem item?
            var isFileSystem = (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem) != 0;

            // Is this item a Folder?
            var isFolder = (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.Folder) != 0;

            // Shell Library
            ShellLibrary shellLibrary = null;

            // Create the right type of ShellObject based on the above information

            // 1. First check if this is a Shell Link
            if (StringComparer.OrdinalIgnoreCase.Equals(itemType, ".lnk"))
            {
                return new ShellLink(nativeShellItem2);
            }
            // 2. Check if this is a container or a single item (entity)
            else if (isFolder)
            {
                // 3. If this is a folder, check for types: Shell Library, Shell Folder or Search Container
                if (itemType == ".library-ms" && (shellLibrary = ShellLibrary.FromShellItem(nativeShellItem2, true)) != null)
                {
                    return shellLibrary; // we already created this above while checking for Library
                }
                else if (itemType == ".searchconnector-ms")
                {
                    return new ShellSearchConnector(nativeShellItem2);
                }
                else if (itemType == ".search-ms")
                {
                    return new ShellSavedSearchCollection(nativeShellItem2);
                }

                // 4. It's a ShellFolder
                if (isFileSystem)
                {
                    // 5. Is it a (File-System / Non-Virtual) Known Folder
                    if (!IsVirtualKnownFolder(nativeShellItem2))
                    { //needs to check if it is a known folder and not virtual
                        var kf = new FileSystemKnownFolder(nativeShellItem2);
                        return kf;
                    }

                    return new ShellFileSystemFolder(nativeShellItem2);
                }

                // 5. Is it a (Non File-System / Virtual) Known Folder
                if (IsVirtualKnownFolder(nativeShellItem2))
                { //needs to check if known folder is virtual
                    var kf = new NonFileSystemKnownFolder(nativeShellItem2);
                    return kf;
                }

                return new ShellNonFileSystemFolder(nativeShellItem2);
            }

            // 6. If this is an entity (single item), check if its filesystem or not
            if (isFileSystem) { return new ShellFile(nativeShellItem2); }

            return new ShellNonFileSystemItem(nativeShellItem2);
        }

        /// <summary>Creates a ShellObject given a parsing name</summary>
        /// <param name="parsingName"></param>
        /// <returns>A newly constructed ShellObject object</returns>
        internal static ShellObject Create(string parsingName)
        {
            if (string.IsNullOrEmpty(parsingName))
            {
                throw new ArgumentNullException("parsingName");
            }

            IShellItem2 nativeShellItem;
            // Create a native shellitem from our path
            var guid = new Guid(ShellIIDGuid.IShellItem2);
            var retCode = ShellNativeMethods.SHCreateItemFromParsingName(parsingName, IntPtr.Zero, ref guid, out
            // Create a native shellitem from our path
            nativeShellItem);

            if (!CoreErrorHelper.Succeeded(retCode))
            {
                throw new ShellException(LocalizedMessages.ShellObjectFactoryUnableToCreateItem, Marshal.GetExceptionForHR(retCode));
            }
            return ShellObjectFactory.Create(nativeShellItem);
        }

        /// <summary>Constructs a new Shell object from IDList pointer</summary>
        /// <param name="idListPtr"></param>
        /// <returns></returns>
        internal static ShellObject Create(IntPtr idListPtr)
        {
            // Throw exception if not running on Win7 or newer.
            CoreHelpers.ThrowIfNotVista();

            var guid = new Guid(ShellIIDGuid.IShellItem2);
            IShellItem2 nativeShellItem;
            var retCode = ShellNativeMethods.SHCreateItemFromIDList(idListPtr, ref guid, out nativeShellItem);

            if (!CoreErrorHelper.Succeeded(retCode)) { return null; }
            return ShellObjectFactory.Create(nativeShellItem);
        }

        /// <summary>Constructs a new Shell object from IDList pointer</summary>
        /// <param name="idListPtr"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal static ShellObject Create(IntPtr idListPtr, ShellContainer parent)
        {
            IShellItem nativeShellItem;
            var retCode = ShellNativeMethods.SHCreateShellItem(
                IntPtr.Zero,
                parent.NativeShellFolder,
                idListPtr, out nativeShellItem);

            if (!CoreErrorHelper.Succeeded(retCode)) { return null; }

            return ShellObjectFactory.Create(nativeShellItem);
        }

        // This is a work around for the STA thread bug. This will execute the call on a non-sta thread, then return the result
        private static bool IsVirtualKnownFolder(IShellItem2 nativeShellItem2)
        {
            var pidl = IntPtr.Zero;
            try
            {
                IKnownFolderNative nativeFolder = null;
                var definition = new KnownFoldersSafeNativeMethods.NativeFolderDefinition();

                // We found a bug where the enumeration of shell folders was not reliable when called from a STA thread - it would return
                // different results the first time vs the other times.
                //
                // This is a work around. We call FindFolderFromIDList on a worker MTA thread instead of the main STA thread.
                //
                // Ultimately, it would be a very good idea to replace the 'getting shell object' logic to get a list of pidl's in 1 step,
                // then look up their information in a 2nd, rather than looking them up as we get them. This would replace the need for the
                // work around.
                var padlock = new object();
                lock (padlock)
                {
                    var unknown = Marshal.GetIUnknownForObject(nativeShellItem2);

                    ThreadPool.QueueUserWorkItem(obj =>
                    {
                        lock (padlock)
                        {
                            pidl = ShellHelper.PidlFromUnknown(unknown);

                            new KnownFolderManagerClass().FindFolderFromIDList(pidl, out nativeFolder);

                            if (nativeFolder != null)
                            {
                                nativeFolder.GetFolderDefinition(out definition);
                            }

                            Monitor.Pulse(padlock);
                        }
                    });

                    Monitor.Wait(padlock);
                }

                return nativeFolder != null && definition.category == FolderCategory.Virtual;
            }
            finally
            {
                ShellNativeMethods.ILFree(pidl);
            }
        }
    }
}
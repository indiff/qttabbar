//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>
    /// Represents the base class for all types of Shell "containers". Any class deriving from this class can contain other ShellObjects
    /// (e.g. ShellFolder, FileSystemKnownFolder, ShellLibrary, etc)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This will complicate the class hierarchy and naming convention used in the Shell area")]
    public abstract class ShellContainer : ShellObject, IEnumerable<ShellObject>, IDisposable
    {
        private IShellFolder desktopFolderEnumeration;
        private IShellFolder nativeShellFolder;

        internal ShellContainer()
        {
        }

        internal ShellContainer(IShellItem2 shellItem) : base(shellItem)
        {
        }

        internal IShellFolder NativeShellFolder
        {
            get
            {
                if (nativeShellFolder == null)
                {
                    var guid = new Guid(ShellIIDGuid.IShellFolder);
                    var handler = new Guid(ShellBHIDGuid.ShellFolderObject);

                    var hr = NativeShellItem.BindToHandler(
                        IntPtr.Zero, ref handler, ref guid, out nativeShellFolder);

                    if (CoreErrorHelper.Failed(hr))
                    {
                        var str = ShellHelper.GetParsingName(NativeShellItem);
                        if (str != null && str != Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                        {
                            throw new ShellException(hr);
                        }
                    }
                }

                return nativeShellFolder;
            }
        }

        /// <summary>Enumerates through contents of the ShellObjectContainer</summary>
        /// <returns>Enumerated contents</returns>
        public IEnumerator<ShellObject> GetEnumerator()
        {
            if (NativeShellFolder == null)
            {
                if (desktopFolderEnumeration == null)
                {
                    ShellNativeMethods.SHGetDesktopFolder(out desktopFolderEnumeration);
                }

                nativeShellFolder = desktopFolderEnumeration;
            }

            return new ShellFolderItems(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ShellFolderItems(this);
        }

        /// <summary>Release resources</summary>
        /// <param name="disposing"><B>True</B> indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (nativeShellFolder != null)
            {
                Marshal.ReleaseComObject(nativeShellFolder);
                nativeShellFolder = null;
            }

            if (desktopFolderEnumeration != null)
            {
                Marshal.ReleaseComObject(desktopFolderEnumeration);
                desktopFolderEnumeration = null;
            }

            base.Dispose(disposing);
        }
    }
}
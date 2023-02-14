//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    internal class ShellFolderItems : IEnumerator<ShellObject>
    {
        private readonly ShellContainer nativeShellFolder;
        private ShellObject currentItem;
        private IEnumIDList nativeEnumIdList;
        private object current;
        private ShellObject current1;

        internal ShellFolderItems(ShellContainer nativeShellFolder)
        {
            this.nativeShellFolder = nativeShellFolder;

            var hr = nativeShellFolder.NativeShellFolder.EnumObjects(
                IntPtr.Zero,
                ShellNativeMethods.ShellFolderEnumerationOptions.Folders | ShellNativeMethods.ShellFolderEnumerationOptions.NonFolders,
                out nativeEnumIdList);

            if (!CoreErrorHelper.Succeeded(hr))
            {
                if (hr == HResult.Canceled)
                {
                    throw new System.IO.FileNotFoundException();
                }
                else
                {
                    throw new ShellException(hr);
                }
            }
        }

        public ShellObject Current()
        {
            return currentItem;
        }

        

        ShellObject IEnumerator<ShellObject>.Current
        {
            get { return current1; }
        }

        object IEnumerator.Current
        {
            get { return current; }
        }

        public void Dispose()
        {
            if (nativeEnumIdList != null)
            {
                Marshal.ReleaseComObject(nativeEnumIdList);
                nativeEnumIdList = null;
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (nativeEnumIdList == null) { return false; }

            uint itemsRequested = 1;
            IntPtr item;
            uint numItemsReturned;
            var hr = nativeEnumIdList.Next(itemsRequested, out item, out numItemsReturned);

            if (numItemsReturned < itemsRequested || hr != HResult.Ok) { return false; }

            currentItem = ShellObjectFactory.Create(item, nativeShellFolder);

            return true;
        }

        /// <summary></summary>
        public void Reset()
        {
            if (nativeEnumIdList != null)
            {
                nativeEnumIdList.Reset();
            }
        }

       
    }
}
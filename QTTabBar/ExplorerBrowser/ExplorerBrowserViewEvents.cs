//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using QTTabBarLib.Common;

namespace QTTabBarLib.ExplorerBrowser
{
    /// <summary>This provides a connection point container compatible dispatch interface for hooking into the ExplorerBrowser view.</summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ExplorerBrowserViewEvents : IDisposable
    {
        private readonly WindowsForms.ExplorerBrowser parent;
        private Guid IID_DShellFolderViewEvents = new Guid(ExplorerBrowserIIDGuid.DShellFolderViewEvents);
        private Guid IID_IDispatch = new Guid(ExplorerBrowserIIDGuid.IDispatch);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        private IntPtr nullPtr = IntPtr.Zero;

        private uint viewConnectionPointCookie;
        private object viewDispatch;

        /// <summary>Default constructor for ExplorerBrowserViewEvents</summary>
        public ExplorerBrowserViewEvents() : this(null) { }

        internal ExplorerBrowserViewEvents(WindowsForms.ExplorerBrowser parent)
        {
            this.parent = parent;
        }

        /// <summary>Finalizer for ExplorerBrowserViewEvents</summary>
        ~ExplorerBrowserViewEvents()
        {
            Dispose(false);
        }

        /// <summary>Disconnects and disposes object.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>The contents of the view have changed</summary>
        [DispId(ExplorerBrowserViewDispatchIds.ContentsChanged)]
        public void ViewContentsChanged()
        {
            parent.FireContentChanged();
        }

        /// <summary>The enumeration of files in the view is complete</summary>
        [DispId(ExplorerBrowserViewDispatchIds.FileListEnumDone)]
        public void ViewFileListEnumDone()
        {
            parent.FireContentEnumerationComplete();
        }

        /// <summary>The selected item in the view has changed (not the same as the selection has changed)</summary>
        [DispId(ExplorerBrowserViewDispatchIds.SelectedItemChanged)]
        public void ViewSelectedItemChanged()
        {
            parent.FireSelectedItemChanged();
        }

        /// <summary>The view selection has changed</summary>
        [DispId(ExplorerBrowserViewDispatchIds.SelectionChanged)]
        public void ViewSelectionChanged()
        {
            parent.FireSelectionChanged();
        }

        internal void ConnectToView(IShellView psv)
        {
            DisconnectFromView();

            var hr = psv.GetItemObject(
                ShellViewGetItemObject.Background,
                ref IID_IDispatch,
                out viewDispatch);

            if (hr == HResult.Ok)
            {
                hr = ExplorerBrowserNativeMethods.ConnectToConnectionPoint(
                    this,
                    ref IID_DShellFolderViewEvents,
                    true,
                    viewDispatch,
                    ref viewConnectionPointCookie,
                    ref nullPtr);

                if (hr != HResult.Ok)
                {
                    Marshal.ReleaseComObject(viewDispatch);
                }
            }
        }

        internal void DisconnectFromView()
        {
            if (viewDispatch != null)
            {
                ExplorerBrowserNativeMethods.ConnectToConnectionPoint(
                    IntPtr.Zero,
                    ref IID_DShellFolderViewEvents,
                    false,
                    viewDispatch,
                    ref viewConnectionPointCookie,
                    ref nullPtr);

                Marshal.ReleaseComObject(viewDispatch);
                viewDispatch = null;
                viewConnectionPointCookie = 0;
            }
        }

        // These need to be public to be accessible via AutoDual reflection
        /// <summary>Disconnects and disposes object.</summary>
        /// <param name="disposed"></param>
        protected virtual void Dispose(bool disposed)
        {
            if (disposed)
            {
                DisconnectFromView();
            }
        }
    }
}
//Copyright (c) Microsoft Corporation.  All rights reserved.


namespace QTTabBarLib.Common
{
    /// <summary>A Serch Connector folder in the Shell Namespace</summary>
    public sealed class ShellSearchConnector : ShellSearchCollection
    {
        internal ShellSearchConnector()
        {
            CoreHelpers.ThrowIfNotWin7();
        }

        internal ShellSearchConnector(IShellItem2 shellItem)
            : this()
        {
            nativeShellItem = shellItem;
        }

        /// <summary>Indicates whether this feature is supported on the current platform.</summary>
        public new static bool IsPlatformSupported
        {
            get
            {
                return // We need Windows 7 onwards ...
                    CoreHelpers.RunningOnWin7;
            }
        }
    }
}
//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace QTTabBarLib.ExplorerBrowser
{
    /// <summary>The event argument for NavigationLogChangedEvent</summary>
    public class NavigationLogEventArgs : EventArgs
    {
        /// <summary>Indicates CanNavigateBackward has changed</summary>
        public bool CanNavigateBackwardChanged { get; set; }

        /// <summary>Indicates CanNavigateForward has changed</summary>
        public bool CanNavigateForwardChanged { get; set; }

        /// <summary>Indicates the Locations collection has changed</summary>
        public bool LocationsChanged { get; set; }
    }
}
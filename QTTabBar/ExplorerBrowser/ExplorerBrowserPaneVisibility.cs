//Copyright (c) Microsoft Corporation.  All rights reserved.


namespace QTTabBarLib.ExplorerBrowser
{
    /// <summary>Controls the visibility of the various ExplorerBrowser panes on subsequent navigation</summary>
    public class ExplorerBrowserPaneVisibility
    {
        /// <summary>Additional fields and options to aid in a search.</summary>
        public PaneVisibilityState AdvancedQuery { get; set; }

        /// <summary>Commands module along the top of the Windows Explorer window.</summary>
        public PaneVisibilityState Commands { get; set; }

        /// <summary>Organize menu within the commands module.</summary>
        public PaneVisibilityState CommandsOrganize { get; set; }

        /// <summary>View menu within the commands module.</summary>
        public PaneVisibilityState CommandsView { get; set; }

        /// <summary>Pane showing metadata along the bottom of the Windows Explorer window.</summary>
        public PaneVisibilityState Details { get; set; }

        /// <summary>The pane on the left side of the Windows Explorer window that hosts the folders tree and Favorites.</summary>
        public PaneVisibilityState Navigation { get; set; }

        /// <summary>Pane on the right of the Windows Explorer window that shows a large reading preview of the file.</summary>
        public PaneVisibilityState Preview { get; set; }

        /// <summary>Quick filter buttons to aid in a search.</summary>
        public PaneVisibilityState Query { get; set; }
    }
}
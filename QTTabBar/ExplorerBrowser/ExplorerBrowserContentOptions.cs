//Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Runtime.InteropServices;
using QTTabBarLib.Common;

namespace QTTabBarLib.ExplorerBrowser
{
    /// <summary>These options control how the content of the Explorer Browser is rendered.</summary>
    public class ExplorerBrowserContentOptions
    {
        // This is a one-way property of the explorer browser. Keeping it around for the get implementations.
        internal FolderSettings folderSettings = new FolderSettings();

        private readonly WindowsForms.ExplorerBrowser eb;

        internal ExplorerBrowserContentOptions(WindowsForms.ExplorerBrowser eb)
        {
            this.eb = eb;
        }

        /// <summary>The view should be left-aligned.</summary>
        public bool AlignLeft
        {
            get
            {
               return IsFlagSet(ExplorerBrowserContentSectionOptions.AlignLeft);
            }
            set
            {
                SetFlag(ExplorerBrowserContentSectionOptions.AlignLeft, value);
            }
        }

        /// <summary>Automatically arrange the elements in the view.</summary>
        public bool AutoArrange
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.AutoArrange);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.AutoArrange, value);} 
        }

        /// <summary>Turns on check mode for the view</summary>
        public bool CheckSelect
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.CheckSelect);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.CheckSelect, value);} 
        }

        /// <summary>When the view is in "tile view mode" the layout of a single item should be extended to the width of the view.</summary>
        public bool ExtendedTiles
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.ExtendedTiles);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.ExtendedTiles, value); } 
        }

        /// <summary>The binary representation of the ExplorerBrowser content flags</summary>
        public ExplorerBrowserContentSectionOptions Flags
        {
            get { return (ExplorerBrowserContentSectionOptions)folderSettings.Options; }
            set
            {
                folderSettings.Options = (FolderOptions)value | FolderOptions.UseSearchFolders | FolderOptions.NoWebView;
                if (eb.explorerBrowserControl != null)
                {
                    eb.explorerBrowserControl.SetFolderSettings(folderSettings);
                }
            }
        }

        /// <summary>When an item is selected, the item and all its sub-items are highlighted.</summary>
        public bool FullRowSelect
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.FullRowSelect);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.FullRowSelect, value);} 
        }

        /// <summary>The view should not display file names</summary>
        public bool HideFileNames
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.HideFileNames);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.HideFileNames, value);} 
        }

        /// <summary>The view should not save view state in the browser.</summary>
        public bool NoBrowserViewState
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.NoBrowserViewState);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.NoBrowserViewState, value);} 
        }

        /// <summary>Do not display a column header in the view in any view mode.</summary>
        public bool NoColumnHeader
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.NoColumnHeader);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.NoColumnHeader, value);} 
        }

        /// <summary>Only show the column header in details view mode.</summary>
        public bool NoHeaderInAllViews
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.NoHeaderInAllViews);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.NoHeaderInAllViews, value);} 
        }

        /// <summary>The view should not display icons.</summary>
        public bool NoIcons
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.NoIcons);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.NoIcons, value);} 
        }

        /// <summary>Do not show subfolders.</summary>
        public bool NoSubfolders
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.NoSubfolders);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.NoSubfolders, value);} 
        }

        /// <summary>Navigate with a single click</summary>
        public bool SingleClickActivate
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.SingleClickActivate);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.SingleClickActivate, value);} 
        }

        /// <summary>Do not allow more than a single item to be selected.</summary>
        public bool SingleSelection
        {
            get { return IsFlagSet(ExplorerBrowserContentSectionOptions.SingleSelection);
            } set { SetFlag(ExplorerBrowserContentSectionOptions.SingleSelection, value);} 
        }

        /// <summary>The size of the thumbnails in pixels</summary>
        public int ThumbnailSize
        {
            get
            {
                var iconSize = 0;
                var iFV2 = eb.GetFolderView2();
                if (iFV2 != null)
                {
                    try
                    {
                        int fvm;
                        var hr = iFV2.GetViewModeAndIconSize(out fvm, out iconSize);
                        if (hr != HResult.Ok)
                        {
                            throw new CommonControlException(LocalizedMessages.ExplorerBrowserIconSize, hr);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(iFV2);
                        iFV2 = null;
                    }
                }

                return iconSize;
            }
            set
            {
                var iFV2 = eb.GetFolderView2();
                if (iFV2 != null)
                {
                    try
                    {
                        int fvm;
                        int iconSize;
                        var hr = iFV2.GetViewModeAndIconSize(out fvm, out iconSize);
                        if (hr != HResult.Ok)
                        {
                            throw new CommonControlException(LocalizedMessages.ExplorerBrowserIconSize, hr);
                        }
                        hr = iFV2.SetViewModeAndIconSize(fvm, value);
                        if (hr != HResult.Ok)
                        {
                            throw new CommonControlException(LocalizedMessages.ExplorerBrowserIconSize, hr);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(iFV2);
                        iFV2 = null;
                    }
                }
            }
        }

        /// <summary>The viewing mode of the Explorer Browser</summary>
        public ExplorerBrowserViewMode ViewMode
        {
            get
            {
                return (ExplorerBrowserViewMode)folderSettings.ViewMode;
            }
            set
            {
                folderSettings.ViewMode = (FolderViewMode)value;

                if (eb.explorerBrowserControl != null)
                {
                    eb.explorerBrowserControl.SetFolderSettings(folderSettings);
                }
            }
        }

        private bool IsFlagSet(ExplorerBrowserContentSectionOptions flag)
        {
            return (folderSettings.Options & (FolderOptions)flag) != 0;
        }

        private void SetFlag(ExplorerBrowserContentSectionOptions flag, bool value)
        {
            if (value)
                folderSettings.Options |= (FolderOptions)flag;
            else
                folderSettings.Options = folderSettings.Options & ~(FolderOptions)flag;

            if (eb.explorerBrowserControl != null)
                eb.explorerBrowserControl.SetFolderSettings(folderSettings);
        }
    }
}
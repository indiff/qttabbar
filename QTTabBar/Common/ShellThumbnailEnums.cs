// Copyright (c) Microsoft Corporation. All rights reserved.

namespace QTTabBarLib.Common
{
    /// <summary>Represents the format options for the thumbnails and icons.</summary>
    public enum ShellThumbnailFormatOption
    {
        /// <summary>
        /// The default behavior loads a thumbnail. An HBITMAP for the icon of the item is retrieved if there is no thumbnail for the current
        /// Shell Item.
        /// </summary>
        Default,

        /// <summary>
        /// The ThumbnailOnly behavior returns only the thumbnails, never the icon. Note that not all items have thumbnails so
        /// ShellThumbnailFormatOption.ThumbnailOnly can fail in these cases.
        /// </summary>
        ThumbnailOnly = ShellNativeMethods.SIIGBF.ThumbnailOnly,

        /// <summary>The IconOnly behavior returns only the icon, never the thumbnail.</summary>
        IconOnly = ShellNativeMethods.SIIGBF.IconOnly,
    }

    /// <summary>
    /// Represents the different retrieval options for the thumbnail or icon, such as extracting the thumbnail or icon from a file, from the
    /// cache only, or from memory only.
    /// </summary>
    public enum ShellThumbnailRetrievalOption
    {
        /// <summary>
        /// The default behavior loads a thumbnail. If there is no thumbnail for the current ShellItem, the icon is retrieved. The thumbnail
        /// or icon is extracted if it is not currently cached.
        /// </summary>
        Default,

        /// <summary>
        /// The CacheOnly behavior returns a cached thumbnail if it is available. Allows access to the disk, but only to retrieve a cached
        /// item. If no cached thumbnail is available, a cached per-instance icon is returned but a thumbnail or icon is not extracted.
        /// </summary>
        CacheOnly = ShellNativeMethods.SIIGBF.InCacheOnly,

        /// <summary>
        /// The MemoryOnly behavior returns the item only if it is in memory. The disk is not accessed even if the item is cached. Note that
        /// this only returns an already-cached icon and can fall back to a per-class icon if an item has a per-instance icon that has not
        /// been cached yet. Retrieving a thumbnail, even if it is cached, always requires the disk to be accessed, so this method should not
        /// be called from the user interface (UI) thread without passing ShellThumbnailCacheOptions.MemoryOnly.
        /// </summary>
        MemoryOnly = ShellNativeMethods.SIIGBF.MemoryOnly,
    }
}
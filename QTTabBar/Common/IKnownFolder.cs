// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;

namespace QTTabBarLib.Common
{
    /// <summary>Represents a registered or known folder in the system.</summary>
    public interface IKnownFolder : IDisposable, IEnumerable<ShellObject>
    {
        /// <summary>Gets this known folder's canonical name.</summary>
        string CanonicalName { get; }

        /// <summary>Gets the category designation for this known folder.</summary>
        FolderCategory Category { get; }

        /// <summary>Gets an value that describes this known folder's behaviors.</summary>
        DefinitionOptions DefinitionOptions { get; }

        /// <summary>Gets this known folder's description.</summary>
        string Description { get; }

        /// <summary>Gets this known folder's file attributes, such as "read-only".</summary>
        FileAttributes FileAttributes { get; }

        /// <summary>Gets the unique identifier for this known folder.</summary>
        Guid FolderId { get; }

        /// <summary>Gets a string representation of this known folder's type.</summary>
        string FolderType { get; }

        /// <summary>Gets the unique identifier for this known folder's type.</summary>
        Guid FolderTypeId { get; }

        /// <summary>Gets this known folder's localized name.</summary>
        string LocalizedName { get; }

        /// <summary>Gets the resource identifier for this known folder's localized name.</summary>
        string LocalizedNameResourceId { get; }

        /// <summary>Gets the unique identifier for this known folder's parent folder.</summary>
        Guid ParentId { get; }

        /// <summary>Gets this known folder's parsing name.</summary>
        string ParsingName { get; }

        /// <summary>Gets the path for this known folder.</summary>
        string Path { get; }

        /// <summary>Gets a value that indicates whether this known folder's path exists on the computer.</summary>
        /// <remarks>
        /// If this property value is <b>false</b>, the folder might be a virtual folder ( <see cref="Category"/> property will be
        /// <see cref="FolderCategory.Virtual"/> for virtual folders)
        /// </remarks>
        bool PathExists { get; }

        /// <summary>
        /// Gets a value that states whether this known folder can have its path set to a new value, including any restrictions on the redirection.
        /// </summary>
        RedirectionCapability Redirection { get; }

        /// <summary>Gets this known folder's relative path.</summary>
        string RelativePath { get; }

        /// <summary>Gets this known folder's security attributes.</summary>
        string Security { get; }

        /// <summary>Gets this known folder's tool tip text.</summary>
        string Tooltip { get; }

        /// <summary>Gets the resource identifier for this known folder's tool tip text.</summary>
        string TooltipResourceId { get; }
    }
}
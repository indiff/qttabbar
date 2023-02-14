//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QTTabBarLib.Common
{
    /// <summary>Represents a registered non file system Known Folder</summary>
    public class NonFileSystemKnownFolder : ShellNonFileSystemFolder, IKnownFolder, IDisposable
    {
        private IKnownFolderNative knownFolderNative;
        private KnownFolderSettings knownFolderSettings;

        internal NonFileSystemKnownFolder(IShellItem2 shellItem) : base(shellItem)
        {
        }

        internal NonFileSystemKnownFolder(IKnownFolderNative kf)
        {
            Debug.Assert(kf != null);
            knownFolderNative = kf;

            // Set the native shell item and set it on the base class (ShellObject)
            var guid = new Guid(ShellIIDGuid.IShellItem2);
            knownFolderNative.GetShellItem(0, ref guid, out nativeShellItem);
        }

        /// <summary>Gets this known folder's canonical name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string CanonicalName
        {
            get { return KnownFolderSettings.CanonicalName; }
        }

        /// <summary>Gets the category designation for this known folder.</summary>
        /// <value>A <see cref="FolderCategory"/> value.</value>
        public FolderCategory Category
        {
            get { return KnownFolderSettings.Category; }
        }

        /// <summary>Gets an value that describes this known folder's behaviors.</summary>
        /// <value>A <see cref="DefinitionOptions"/> value.</value>
        public DefinitionOptions DefinitionOptions
        {
            get { return KnownFolderSettings.DefinitionOptions; }
        }

        /// <summary>Gets this known folder's description.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Description
        {
            get { return KnownFolderSettings.Description; }
        }

        /// <summary>Gets this known folder's file attributes, such as "read-only".</summary>
        /// <value>A <see cref="System.IO.FileAttributes"/> value.</value>
        public System.IO.FileAttributes FileAttributes
        {
            get { return KnownFolderSettings.FileAttributes; }
        }

        /// <summary>Gets the unique identifier for this known folder.</summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderId
        {
            get { return KnownFolderSettings.FolderId; }
        }

        /// <summary>Gets a string representation of this known folder's type.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string FolderType
        {
            get { return KnownFolderSettings.FolderType; }
        }

        /// <summary>Gets the unique identifier for this known folder's type.</summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderTypeId {
            get { return  KnownFolderSettings.FolderTypeId;}}

        /// <summary>Gets this known folder's localized name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string LocalizedName {
            get { return  KnownFolderSettings.LocalizedName;}}

        /// <summary>Gets the resource identifier for this known folder's localized name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string LocalizedNameResourceId {
            get { return  KnownFolderSettings.LocalizedNameResourceId;}}

        /// <summary>Gets the unique identifier for this known folder's parent folder.</summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid ParentId {
            get { return  KnownFolderSettings.ParentId;}}

        /// <summary>Gets this known folder's parsing name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>

        public  string ParsingName {
            get { return  base.ParsingName;}
        }

        /// <summary>Gets the path for this known folder.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Path {
            get { return  KnownFolderSettings.Path;}}

        /// <summary>Gets a value that indicates whether this known folder's path exists on the computer.</summary>
        /// <value>A bool <see cref="System.Boolean"/> value.</value>
        /// <remarks>
        /// If this property value is <b>false</b>, the folder might be a virtual folder ( <see cref="Category"/> property will be
        /// <see cref="FolderCategory.Virtual"/> for virtual folders)
        /// </remarks>
        public bool PathExists {
            get { return  KnownFolderSettings.PathExists;}}

        /// <summary>
        /// Gets a value that states whether this known folder can have its path set to a new value, including any restrictions on the redirection.
        /// </summary>
        /// <value>A <see cref="RedirectionCapability"/> value.</value>
        public RedirectionCapability Redirection {
            get { return  KnownFolderSettings.Redirection;}}

        /// <summary>Gets this known folder's relative path.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string RelativePath {
            get { return  KnownFolderSettings.RelativePath;}}

        /// <summary>Gets this known folder's security attributes.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Security {
            get { return  KnownFolderSettings.Security;}}

        /// <summary>Gets this known folder's tool tip text.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Tooltip {
            get { return  KnownFolderSettings.Tooltip;}}

        /// <summary>Gets the resource identifier for this known folder's tool tip text.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string TooltipResourceId {
            get { return  KnownFolderSettings.TooltipResourceId;}}

        private KnownFolderSettings KnownFolderSettings
        {
            get
            {
                if (knownFolderNative == null)
                {
                    // We need to get the PIDL either from the NativeShellItem, or from base class's property (if someone already set it on
                    // us). Need to use the PIDL to get the native IKnownFolder interface.

                    // Get teh PIDL for the ShellItem
                    if (nativeShellItem != null && base.PIDL == IntPtr.Zero)
                    {
                        base.PIDL = ShellHelper.PidlFromShellItem(nativeShellItem);
                    }

                    // If we have a valid PIDL, get the native IKnownFolder
                    if (base.PIDL != IntPtr.Zero)
                    {
                        knownFolderNative = KnownFolderHelper.FromPIDL(base.PIDL);
                    }

                    Debug.Assert(knownFolderNative != null);
                }

                // If this is the first time this property is being called, get the native Folder Defination (KnownFolder properties)
                if (knownFolderSettings == null)
                {
                    knownFolderSettings = new KnownFolderSettings(knownFolderNative);
                }

                return knownFolderSettings;
            }
        }

        /// <summary>Release resources</summary>
        /// <param name="disposing">Indicates that this mothod is being called from Dispose() rather than the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                knownFolderSettings = null;
            }

            if (knownFolderNative != null)
            {
                Marshal.ReleaseComObject(knownFolderNative);
                knownFolderNative = null;
            }

            base.Dispose(disposing);
        }
    }
}
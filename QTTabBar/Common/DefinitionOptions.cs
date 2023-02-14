//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace QTTabBarLib.Common
{
    /// <summary>Specifies behaviors for known folders.</summary>
    [Flags]
    public enum DefinitionOptions
    {
        /// <summary>No behaviors are defined.</summary>
        None = 0x0,

        /// <summary>Prevents a per-user known folder from being redirected to a network location.</summary>
        LocalRedirectOnly = 0x2,

        /// <summary>The known folder can be roamed through PC-to-PC synchronization.</summary>
        Roamable = 0x4,

        /// <summary>Creates the known folder when the user first logs on.</summary>
        Precreate = 0x8
    }
}
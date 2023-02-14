//Copyright (c) Microsoft Corporation.  All rights reserved.

namespace QTTabBarLib.Common
{
    /// <summary>Specifies the redirection capabilities for known folders.</summary>
    public enum RedirectionCapability
    {
        /// <summary>Redirection capability is unknown.</summary>
        None = 0x00,

        /// <summary>The known folder can be redirected.</summary>
        AllowAll = 0xff,

        /// <summary>
        /// The known folder can be redirected. Currently, redirection exists only for common and user folders; fixed and virtual folders
        /// cannot be redirected.
        /// </summary>
        Redirectable = 0x1,

        /// <summary>Redirection is not allowed.</summary>
        DenyAll = 0xfff00,

        /// <summary>The folder cannot be redirected because it is already redirected by group policy.</summary>
        DenyPolicyRedirected = 0x100,

        /// <summary>The folder cannot be redirected because the policy prohibits redirecting this folder.</summary>
        DenyPolicy = 0x200,

        /// <summary>The folder cannot be redirected because the calling application does not have sufficient permissions.</summary>
        DenyPermissions = 0x400
    }
}
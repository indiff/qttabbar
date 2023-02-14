//Copyright (c) Microsoft Corporation.  All rights reserved.

/* Unmerged change from project 'Shell (net452)'
Before:
using System;
using Microsoft.WindowsAPICodePack.Shell;
After:
using Microsoft.WindowsAPICodePack.Shell;
using System;
*/

/* Unmerged change from project 'Shell (net462)'
Before:
using System;
using Microsoft.WindowsAPICodePack.Shell;
After:
using Microsoft.WindowsAPICodePack.Shell;
using System;
*/

/* Unmerged change from project 'Shell (net472)'
Before:
using System;
using Microsoft.WindowsAPICodePack.Shell;
After:
using Microsoft.WindowsAPICodePack.Shell;
using System;
*/

using System;

namespace QTTabBarLib.Common
{
    /// <summary>Defines the properties used by a Shell Property.</summary>
    public interface IShellProperty
    {
        /// <summary>Gets the case-sensitive name of the property as it is known to the system, regardless of its localized name.</summary>
        string CanonicalName { get; }

        /// <summary>Get the property description object.</summary>
        ShellPropertyDescription Description { get; }

        /// <summary>
        /// Gets the image reference path and icon index associated with a property value. This API is only available in Windows 7.
        /// </summary>
        IconReference IconReference { get; }

        /// <summary>Gets the property key that identifies this property.</summary>
        PropertyKey PropertyKey { get; }

        /// <summary>Gets the value for this property using the generic Object type.</summary>
        /// <remarks>
        /// To obtain a specific type for this value, use the more strongly-typed <c>Property&lt;T&gt;</c> class. You can only set a value
        /// for this type using the <c>Property&lt;T&gt;</c> class.
        /// </remarks>
        object ValueAsObject { get; }

        /// <summary>Gets the <c>System.Type</c> value for this property.</summary>
        Type ValueType { get; }

        /// <summary>Gets a formatted, Unicode string representation of a property value.</summary>
        /// <param name="format">One or more <c>PropertyDescriptionFormat</c> flags chosen to produce the desired display format.</param>
        /// <returns>The formatted value as a string.</returns>
        string FormatForDisplay(PropertyDescriptionFormatOptions format);
    }
}
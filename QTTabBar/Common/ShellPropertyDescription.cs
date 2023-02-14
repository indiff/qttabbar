//Copyright (c) Microsoft Corporation.  All rights reserved.

/* Unmerged change from project 'Shell (net452)'
Before:
using System;
After:
using MS.WindowsAPICodePack.Internal;
using System;
*/

/* Unmerged change from project 'Shell (net462)'
Before:
using System;
After:
using MS.WindowsAPICodePack.Internal;
using System;
*/

/* Unmerged change from project 'Shell (net472)'
Before:
using System;
After:
using MS.WindowsAPICodePack.Internal;
using System;
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>Defines the shell property description information for a property.</summary>
    public class ShellPropertyDescription : IDisposable
    {
        private PropertyAggregationType? aggregationTypes;
        private string canonicalName;
        private PropertyColumnStateOptions? columnState;
        private PropertyConditionOperation? conditionOperation;
        private PropertyConditionType? conditionType;
        private uint? defaultColumWidth;
        private string displayName;
        private PropertyDisplayType? displayType;
        private string editInvitation;
        private PropertyGroupingRange? groupingRange;
        private IPropertyDescription nativePropertyDescription;
        private ReadOnlyCollection<ShellPropertyEnumType> propertyEnumTypes;
        private PropertyKey propertyKey;
        private PropertyTypeOptions? propertyTypeFlags;
        private PropertyViewOptions? propertyViewFlags;
        private PropertySortDescription? sortDescription;
        private Type valueType;
        private VarEnum? varEnumType = null;

        internal ShellPropertyDescription(PropertyKey key)
        {
            propertyKey = key;
        }

        /// <summary>Release the native objects</summary>
        ~ShellPropertyDescription()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a value that describes how the property values are displayed when multiple items are selected in the user interface (UI).
        /// </summary>
        public PropertyAggregationType AggregationTypes
        {
            get
            {
                if (NativePropertyDescription != null && aggregationTypes == null)
                {
                    PropertyAggregationType tempAggregationTypes;
                    var hr = NativePropertyDescription.GetAggregationType(out tempAggregationTypes);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        aggregationTypes = tempAggregationTypes;
                    }
                }

                return aggregationTypes.HasValue ? aggregationTypes.Value : default(PropertyAggregationType);
            }
        }

        /// <summary>Gets the case-sensitive name of a property as it is known to the system, regardless of its localized name.</summary>
        public string CanonicalName
        {
            get
            {
                if (canonicalName == null)
                {
                    PropertySystemNativeMethods.PSGetNameFromPropertyKey(ref propertyKey, out canonicalName);
                }

                return canonicalName;
            }
        }

        /// <summary>
        /// Gets the column state flag, which describes how the property should be treated by interfaces or APIs that use this flag.
        /// </summary>
        public PropertyColumnStateOptions ColumnState
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (NativePropertyDescription != null && columnState == null)
                {
                    PropertyColumnStateOptions state;
                    var hr = NativePropertyDescription.GetColumnState(out state);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        columnState = state;
                    }
                }

                return columnState.HasValue ? columnState.Value : default(PropertyColumnStateOptions);
            }
        }

        /// <summary>
        /// Gets the default condition operation to use when displaying the property in the query builder user interface (UI). This
        /// influences the list of predicate conditions (for example, equals, less than, and contains) that are shown for this property.
        /// </summary>
        /// <remarks>
        /// For more information, see the <c>conditionType</c> attribute of the <c>typeInfo</c> element in the property's .propdesc file.
        /// </remarks>
        public PropertyConditionOperation ConditionOperation
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (NativePropertyDescription != null && conditionOperation == null)
                {
                    PropertyConditionType tempConditionType;
                    PropertyConditionOperation tempConditionOperation;
                    var hr = NativePropertyDescription.GetConditionType(out tempConditionType, out tempConditionOperation);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        conditionOperation = tempConditionOperation;
                        conditionType = tempConditionType;
                    }
                }

                return conditionOperation.HasValue ? conditionOperation.Value : default(PropertyConditionOperation);
            }
        }

        /// <summary>
        /// Gets the condition type to use when displaying the property in the query builder user interface (UI). This influences the list of
        /// predicate conditions (for example, equals, less than, and
        /// contains) that are shown for this property.
        /// </summary>
        /// <remarks>
        /// For more information, see the <c>conditionType</c> attribute of the <c>typeInfo</c> element in the property's .propdesc file.
        /// </remarks>
        public PropertyConditionType ConditionType
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (NativePropertyDescription != null && conditionType == null)
                {
                    PropertyConditionType tempConditionType;
                    PropertyConditionOperation tempConditionOperation;
                    var hr = NativePropertyDescription.GetConditionType(out tempConditionType, out  tempConditionOperation);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        conditionOperation = tempConditionOperation;
                        conditionType = tempConditionType;
                    }
                }

                return conditionType.HasValue ? conditionType.Value : default(PropertyConditionType);
            }
        }

        /// <summary>Gets the default user interface (UI) column width for this property.</summary>
        public uint DefaultColumWidth
        {
            get
            {
                if (NativePropertyDescription != null && !defaultColumWidth.HasValue)
                {
                    uint tempDefaultColumWidth;
                    var hr = NativePropertyDescription.GetDefaultColumnWidth(out tempDefaultColumWidth);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        defaultColumWidth = tempDefaultColumWidth;
                    }
                }

                return defaultColumWidth.HasValue ? defaultColumWidth.Value : default(uint);
            }
        }

        /// <summary>Gets the display name of the property as it is shown in any user interface (UI).</summary>
        public string DisplayName
        {
            get
            {
                if (NativePropertyDescription != null && displayName == null)
                {
                    var dispNameptr = IntPtr.Zero;

                    var hr = NativePropertyDescription.GetDisplayName(out dispNameptr);

                    if (CoreErrorHelper.Succeeded(hr) && dispNameptr != IntPtr.Zero)
                    {
                        displayName = Marshal.PtrToStringUni(dispNameptr);

                        // Free the string
                        Marshal.FreeCoTaskMem(dispNameptr);
                    }
                }

                return displayName;
            }
        }

        /// <summary>Gets the current data type used to display the property.</summary>
        public PropertyDisplayType DisplayType
        {
            get
            {
                if (NativePropertyDescription != null && displayType == null)
                {
                    PropertyDisplayType tempDisplayType;
                    var hr = NativePropertyDescription.GetDisplayType(out tempDisplayType);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        displayType = tempDisplayType;
                    }
                }

                return displayType.HasValue ? displayType.Value : default(PropertyDisplayType);
            }
        }

        /// <summary>Gets the text used in edit controls hosted in various dialog boxes.</summary>
        public string EditInvitation
        {
            get
            {
                if (NativePropertyDescription != null && editInvitation == null)
                {
                    // EditInvitation can be empty, so ignore the HR value, but don't throw an exception
                    var ptr = IntPtr.Zero;

                    var hr = NativePropertyDescription.GetEditInvitation(out ptr);

                    if (CoreErrorHelper.Succeeded(hr) && ptr != IntPtr.Zero)
                    {
                        editInvitation = Marshal.PtrToStringUni(ptr);
                        // Free the string
                        Marshal.FreeCoTaskMem(ptr);
                    }
                }

                return editInvitation;
            }
        }

        /// <summary>Gets the method used when a view is grouped by this property.</summary>
        /// <remarks>
        /// The information retrieved by this method comes from the <c>groupingRange</c> attribute of the <c>typeInfo</c> element in the
        /// property's .propdesc file.
        /// </remarks>
        public PropertyGroupingRange GroupingRange
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (NativePropertyDescription != null && groupingRange == null)
                {
                    PropertyGroupingRange tempGroupingRange ;
                    var hr = NativePropertyDescription.GetGroupingRange(out tempGroupingRange);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        groupingRange = tempGroupingRange;
                    }
                }

                return groupingRange.HasValue ? groupingRange.Value : default(PropertyGroupingRange);
            }
        }

        /// <summary>Gets a value that determines if the native property description is present on the system.</summary>
        public bool HasSystemDescription()
        {
            return NativePropertyDescription != null;
        }

        /// <summary>Gets a list of the possible values for this property.</summary>
        public ReadOnlyCollection<ShellPropertyEnumType> PropertyEnumTypes
        {
            get
            {
                if (NativePropertyDescription != null && propertyEnumTypes == null)
                {
                    var propEnumTypeList = new List<ShellPropertyEnumType>();

                    var guid = new Guid(ShellIIDGuid.IPropertyEnumTypeList);
                    IPropertyEnumTypeList nativeList ;
                    var hr = NativePropertyDescription.GetEnumTypeList(ref guid, out nativeList);

                    if (nativeList != null && CoreErrorHelper.Succeeded(hr))
                    {
                        uint count;
                        nativeList.GetCount(out count);
                        guid = new Guid(ShellIIDGuid.IPropertyEnumType);

                        for (uint i = 0; i < count; i++)
                        {
                            IPropertyEnumType nativeEnumType ;
                            nativeList.GetAt(i, ref guid, out nativeEnumType);
                            propEnumTypeList.Add(new ShellPropertyEnumType(nativeEnumType));
                        }
                    }

                    propertyEnumTypes = new ReadOnlyCollection<ShellPropertyEnumType>(propEnumTypeList);
                }

                return propertyEnumTypes;
            }
        }

        /// <summary>Gets the property key identifying the underlying property.</summary>
        public PropertyKey PropertyKey()
        {
            return propertyKey;
        }

        /// <summary>Gets the current sort description flags for the property, which indicate the particular wordings of sort offerings.</summary>
        /// <remarks>
        /// The settings retrieved by this method are set through the <c>sortDescription</c> attribute of the <c>labelInfo</c> element in the
        /// property's .propdesc file.
        /// </remarks>
        public PropertySortDescription SortDescription
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if (NativePropertyDescription != null && sortDescription == null)
                {
                    PropertySortDescription tempSortDescription;
                    var hr = NativePropertyDescription.GetSortDescription(out tempSortDescription);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        sortDescription = tempSortDescription;
                    }
                }

                return sortDescription.HasValue ? sortDescription.Value : default(PropertySortDescription);
            }
        }

        /// <summary>Gets a set of flags that describe the uses and capabilities of the property.</summary>
        public PropertyTypeOptions TypeFlags
        {
            get
            {
                if (NativePropertyDescription != null && propertyTypeFlags == null)
                {
                    PropertyTypeOptions tempFlags;
                    var hr = NativePropertyDescription.GetTypeFlags(PropertyTypeOptions.MaskAll, out tempFlags);

                    propertyTypeFlags = CoreErrorHelper.Succeeded(hr) ? tempFlags : default(PropertyTypeOptions);
                }

                return propertyTypeFlags.HasValue ? propertyTypeFlags.Value : default(PropertyTypeOptions);
            }
        }

        /// <summary>Gets the .NET system type for a value of this property, or null if the value is empty.</summary>
        public Type ValueType
        {
            get
            {
                if (valueType == null)
                {
                    valueType = ShellPropertyFactory.VarEnumToSystemType(VarEnumType);
                }

                return valueType;
            }
        }

        /// <summary>Gets the VarEnum OLE type for this property.</summary>
        public VarEnum VarEnumType
        {
            get
            {
                if (NativePropertyDescription != null && varEnumType == null)
                {
                    VarEnum tempType;
                    var hr = NativePropertyDescription.GetPropertyType(out tempType);

                    if (CoreErrorHelper.Succeeded(hr))
                    {
                        varEnumType = tempType;
                    }
                }

                return varEnumType.HasValue ? varEnumType.Value : default(VarEnum);
            }
        }

        /// <summary>Gets the current set of flags governing the property's view.</summary>
        public PropertyViewOptions ViewFlags
        {
            get
            {
                if (NativePropertyDescription != null && propertyViewFlags == null)
                {
                    PropertyViewOptions tempFlags;
                    var hr = NativePropertyDescription.GetViewFlags(out tempFlags);

                    propertyViewFlags = CoreErrorHelper.Succeeded(hr) ? tempFlags : default(PropertyViewOptions);
                }

                return propertyViewFlags.HasValue ? propertyViewFlags.Value : default(PropertyViewOptions);
            }
        }

        /// <summary>Get the native property description COM interface</summary>
        internal IPropertyDescription NativePropertyDescription
        {
            get
            {
                if (nativePropertyDescription == null)
                {
                    var guid = new Guid(ShellIIDGuid.IPropertyDescription);
                    PropertySystemNativeMethods.PSGetPropertyDescription(ref propertyKey, ref guid, out nativePropertyDescription);
                }

                return nativePropertyDescription;
            }
        }

        /// <summary>Release the native objects</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Gets the localized display string that describes the current sort order.</summary>
        /// <param name="descending">
        /// Indicates the sort order should reference the string "Z on top"; otherwise, the sort order should reference the string "A on top".
        /// </param>
        /// <returns>The sort description for this property.</returns>
        /// <remarks>
        /// The string retrieved by this method is determined by flags set in the <c>sortDescription</c> attribute of the <c>labelInfo</c>
        /// element in the property's .propdesc file.
        /// </remarks>
        public string GetSortDescriptionLabel(bool descending)
        {
            var ptr = IntPtr.Zero;
            var label = string.Empty;

            if (NativePropertyDescription != null)
            {
                var hr = NativePropertyDescription.GetSortDescriptionLabel(descending, out ptr);

                if (CoreErrorHelper.Succeeded(hr) && ptr != IntPtr.Zero)
                {
                    label = Marshal.PtrToStringUni(ptr);
                    // Free the string
                    Marshal.FreeCoTaskMem(ptr);
                }
            }

            return label;
        }

        /// <summary>Release the native objects</summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (nativePropertyDescription != null)
            {
                Marshal.ReleaseComObject(nativePropertyDescription);
                nativePropertyDescription = null;
            }

            if (disposing)
            {
                // and the managed ones
                canonicalName = null;
                displayName = null;
                editInvitation = null;
                defaultColumWidth = null;
                valueType = null;
                propertyEnumTypes = null;
            }
        }
    }
}
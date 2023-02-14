//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace QTTabBarLib.Common
{
    /// <summary>Property Enumeration Types</summary>
    public enum PropEnumType
    {
        /// <summary>Use DisplayText and either RangeMinValue or RangeSetValue.</summary>
        DiscreteValue = 0,

        /// <summary>Use DisplayText and either RangeMinValue or RangeSetValue</summary>
        RangedValue = 1,

        /// <summary>Use DisplayText</summary>
        DefaultValue = 2,

        /// <summary>Use Value or RangeMinValue</summary>
        EndRange = 3
    };

    /// <summary>Property Aggregation Type</summary>
    public enum PropertyAggregationType
    {
        /// <summary>The string "Multiple Values" is displayed.</summary>
        Default = 0,

        /// <summary>The first value in the selection is displayed.</summary>
        First = 1,

        /// <summary>
        /// The sum of the selected values is displayed. This flag is never returned for data types VT_LPWSTR, VT_BOOL, and VT_FILETIME.
        /// </summary>
        Sum = 2,

        /// <summary>
        /// The numerical average of the selected values is displayed. This flag is never returned for data types VT_LPWSTR, VT_BOOL, and VT_FILETIME.
        /// </summary>
        Average = 3,

        /// <summary>
        /// The date range of the selected values is displayed. This flag is only returned for values of the VT_FILETIME data type.
        /// </summary>
        DateRange = 4,

        /// <summary>
        /// A concatenated string of all the values is displayed. The order of individual values in the string is undefined. The concatenated
        /// string omits duplicate values; if a value occurs more than once, it only appears a single time in the concatenated string.
        /// </summary>
        Union = 5,

        /// <summary>The highest of the selected values is displayed.</summary>
        Max = 6,

        /// <summary>The lowest of the selected values is displayed.</summary>
        Min = 7
    }

    /// <summary>Describes how a property should be treated for display purposes.</summary>
    [Flags]
    public enum PropertyColumnStateOptions
    {
        /// <summary>Default value</summary>
        None = 0x00000000,

        /// <summary>The value is displayed as a string.</summary>
        StringType = 0x00000001,

        /// <summary>The value is displayed as an integer.</summary>
        IntegerType = 0x00000002,

        /// <summary>The value is displayed as a date/time.</summary>
        DateType = 0x00000003,

        /// <summary>A mask for display type values StringType, IntegerType, and DateType.</summary>
        TypeMask = 0x0000000f,

        /// <summary>The column should be on by default in Details view.</summary>
        OnByDefault = 0x00000010,

        /// <summary>Will be slow to compute. Perform on a background thread.</summary>
        Slow = 0x00000020,

        /// <summary>Provided by a handler, not the folder.</summary>
        Extended = 0x00000040,

        /// <summary>Not displayed in the context menu, but is listed in the More... dialog.</summary>
        SecondaryUI = 0x00000080,

        /// <summary>Not displayed in the user interface (UI).</summary>
        Hidden = 0x00000100,

        /// <summary>VarCmp produces same result as IShellFolder::CompareIDs.</summary>
        PreferVariantCompare = 0x00000200,

        /// <summary>PSFormatForDisplay produces same result as IShellFolder::CompareIDs.</summary>
        PreferFormatForDisplay = 0x00000400,

        /// <summary>Do not sort folders separately.</summary>
        NoSortByFolders = 0x00000800,

        /// <summary>Only displayed in the UI.</summary>
        ViewOnly = 0x00010000,

        /// <summary>Marks columns with values that should be read in a batch.</summary>
        BatchRead = 0x00020000,

        /// <summary>Grouping is disabled for this column.</summary>
        NoGroupBy = 0x00040000,

        /// <summary>Can't resize the column.</summary>
        FixedWidth = 0x00001000,

        /// <summary>The width is the same in all dots per inch (dpi)s.</summary>
        NoDpiScale = 0x00002000,

        /// <summary>Fixed width and height ratio.</summary>
        FixedRatio = 0x00004000,

        /// <summary>Filters out new display flags.</summary>
        DisplayMask = 0x0000F000,
    }

    /// <summary>Provides a set of flags to be used with IConditionFactory, ICondition, and IConditionGenerator to indicate the operation.</summary>
    public enum PropertyConditionOperation
    {
        /// <summary>The implicit comparison between the value of the property and the value of the constant.</summary>
        Implicit,

        /// <summary>The value of the property and the value of the constant must be equal.</summary>
        Equal,

        /// <summary>The value of the property and the value of the constant must not be equal.</summary>
        NotEqual,

        /// <summary>The value of the property must be less than the value of the constant.</summary>
        LessThan,

        /// <summary>The value of the property must be greater than the value of the constant.</summary>
        GreaterThan,

        /// <summary>The value of the property must be less than or equal to the value of the constant.</summary>
        LessThanOrEqual,

        /// <summary>The value of the property must be greater than or equal to the value of the constant.</summary>
        GreaterThanOrEqual,

        /// <summary>The value of the property must begin with the value of the constant.</summary>
        ValueStartsWith,

        /// <summary>The value of the property must end with the value of the constant.</summary>
        ValueEndsWith,

        /// <summary>The value of the property must contain the value of the constant.</summary>
        ValueContains,

        /// <summary>The value of the property must not contain the value of the constant.</summary>
        ValueNotContains,

        /// <summary>
        /// The value of the property must match the value of the constant, where '?' matches any single character and '*' matches any
        /// sequence of characters.
        /// </summary>
        DOSWildCards,

        /// <summary>The value of the property must contain a word that is the value of the constant.</summary>
        WordEqual,

        /// <summary>The value of the property must contain a word that begins with the value of the constant.</summary>
        WordStartsWith,

        /// <summary>The application is free to interpret this in any suitable way.</summary>
        ApplicationSpecific,
    }

    /// <summary>Specifies the condition type to use when displaying the property in the query builder user interface (UI).</summary>
    public enum PropertyConditionType
    {
        /// <summary>The default condition type.</summary>
        None = 0,

        /// <summary>The string type.</summary>
        String = 1,

        /// <summary>The size type.</summary>
        Size = 2,

        /// <summary>The date/time type.</summary>
        DateTime = 3,

        /// <summary>The Boolean type.</summary>
        Boolean = 4,

        /// <summary>The number type.</summary>
        Number = 5,
    }

    /// <summary>Delineates the format of a property string.</summary>
    /// <remarks>
    /// Typically use one, or a bitwise combination of these flags, to specify the format. Some flags are mutually exclusive, so combinations
    /// like <c>ShortTime | LongTime | HideTime</c> are not allowed.
    /// </remarks>
    [Flags]
    public enum PropertyDescriptionFormatOptions
    {
        /// <summary>The format settings specified in the property's .propdesc file.</summary>
        None = 0,

        /// <summary>The value preceded with the property's display name.</summary>
        /// <remarks>
        /// This flag is ignored when the <c>hideLabelPrefix</c> attribute of the <c>labelInfo</c> element in the property's .propinfo file
        /// is set to true.
        /// </remarks>
        PrefixName = 0x1,

        /// <summary>The string treated as a file name.</summary>
        FileName = 0x2,

        /// <summary>The sizes displayed in kilobytes (KB), regardless of size.</summary>
        /// <remarks>This flag applies to properties of <c>Integer</c> types and aligns the values in the column.</remarks>
        AlwaysKB = 0x4,

        /// <summary>Reserved.</summary>
        RightToLeft = 0x8,

        /// <summary>The time displayed as 'hh:mm am/pm'.</summary>
        ShortTime = 0x10,

        /// <summary>The time displayed as 'hh:mm:ss am/pm'.</summary>
        LongTime = 0x20,

        /// <summary>The time portion of date/time hidden.</summary>
        HideTime = 64,

        /// <summary>The date displayed as 'MM/DD/YY'. For example, '3/21/04'.</summary>
        ShortDate = 0x80,

        /// <summary>The date displayed as 'DayOfWeek Month day, year'. For example, 'Monday, March 21, 2004'.</summary>
        LongDate = 0x100,

        /// <summary>The date portion of date/time hidden.</summary>
        HideDate = 0x200,

        /// <summary>The friendly date descriptions, such as "Yesterday".</summary>
        RelativeDate = 0x400,

        /// <summary>The text displayed in a text box as a cue for the user, such as 'Enter your name'.</summary>
        /// <remarks>
        /// The invitation text is returned if formatting failed or the value was empty. Invitation text is text displayed in a text box as a
        /// cue for the user, Formatting can fail if the data entered is not of an expected type, such as putting alpha characters in a phone
        /// number field.
        /// </remarks>
        UseEditInvitation = 0x800,

        /// <summary>
        /// This flag requires UseEditInvitation to also be specified. When the formatting flags are ReadOnly | UseEditInvitation and the
        /// algorithm would have shown invitation text, a string is returned that indicates the value is "Unknown" instead of the invitation text.
        /// </summary>
        ReadOnly = 0x1000,

        /// <summary>
        /// The detection of the reading order is not automatic. Useful when converting to ANSI to omit the Unicode reading order characters.
        /// </summary>
        NoAutoReadingOrder = 0x2000,

        /// <summary>Smart display of DateTime values</summary>
        SmartDateTime = 0x4000
    }

    /// <summary>Specifies the display types for a property.</summary>
    public enum PropertyDisplayType
    {
        /// <summary>The String Display. This is the default if the property doesn't specify a display type.</summary>
        String = 0,

        /// <summary>The Number Display.</summary>
        Number = 1,

        /// <summary>The Boolean Display.</summary>
        Boolean = 2,

        /// <summary>The DateTime Display.</summary>
        DateTime = 3,

        /// <summary>The Enumerated Display.</summary>
        Enumerated = 4
    }

    /// <summary>Specifies the property description grouping ranges.</summary>
    public enum PropertyGroupingRange
    {
        /// <summary>The individual values.</summary>
        Discrete = 0,

        /// <summary>The static alphanumeric ranges.</summary>
        Alphanumeric = 1,

        /// <summary>The static size ranges.</summary>
        Size = 2,

        /// <summary>The dynamically-created ranges.</summary>
        Dynamic = 3,

        /// <summary>The month and year groups.</summary>
        Date = 4,

        /// <summary>The percent groups.</summary>
        Percent = 5,

        /// <summary>The enumerated groups.</summary>
        Enumerated = 6,
    }

    /// <summary>Describes the particular wordings of sort offerings.</summary>
    /// <remarks>Note that the strings shown are English versions only; localized strings are used for other locales.</remarks>
    public enum PropertySortDescription
    {
        /// <summary>The default ascending or descending property sort, "Sort going up", "Sort going down".</summary>
        General,

        /// <summary>The alphabetical sort, "A on top", "Z on top".</summary>
        AToZ,

        /// <summary>The numerical sort, "Lowest on top", "Highest on top".</summary>
        LowestToHighest,

        /// <summary>The size sort, "Smallest on top", "Largest on top".</summary>
        SmallestToBiggest,

        /// <summary>The chronological sort, "Oldest on top", "Newest on top".</summary>
        OldestToNewest,
    }

    /// <summary>Property store cache state</summary>
    public enum PropertyStoreCacheState
    {
        /// <summary>Contained in file, not updated.</summary>
        Normal = 0,

        /// <summary>Not contained in file.</summary>
        NotInSource = 1,

        /// <summary>Contained in file, has been updated since file was consumed.</summary>
        Dirty = 2
    }

    /// <summary>Describes the attributes of the <c>typeInfo</c> element in the property's <c>.propdesc</c> file.</summary>
    [Flags]
    public enum PropertyTypeOptions
    {
        /// <summary>The property uses the default values for all attributes.</summary>
        None = 0x00000000,

        /// <summary>The property can have multiple values.</summary>
        /// <remarks>
        /// These values are stored as a VT_VECTOR in the PROPVARIANT structure. This value is set by the multipleValues attribute of the
        /// typeInfo element in the property's .propdesc file.
        /// </remarks>
        MultipleValues = 0x00000001,

        /// <summary>This property cannot be written to.</summary>
        /// <remarks>This value is set by the isInnate attribute of the typeInfo element in the property's .propdesc file.</remarks>
        IsInnate = 0x00000002,

        /// <summary>The property is a group heading.</summary>
        /// <remarks>This value is set by the isGroup attribute of the typeInfo element in the property's .propdesc file.</remarks>
        IsGroup = 0x00000004,

        /// <summary>The user can group by this property.</summary>
        /// <remarks>This value is set by the canGroupBy attribute of the typeInfo element in the property's .propdesc file.</remarks>
        CanGroupBy = 0x00000008,

        /// <summary>The user can stack by this property.</summary>
        /// <remarks>This value is set by the canStackBy attribute of the typeInfo element in the property's .propdesc file.</remarks>
        CanStackBy = 0x00000010,

        /// <summary>This property contains a hierarchy.</summary>
        /// <remarks>This value is set by the isTreeProperty attribute of the typeInfo element in the property's .propdesc file.</remarks>
        IsTreeProperty = 0x00000020,

        /// <summary>Include this property in any full text query that is performed.</summary>
        /// <remarks>This value is set by the includeInFullTextQuery attribute of the typeInfo element in the property's .propdesc file.</remarks>
        IncludeInFullTextQuery = 0x00000040,

        /// <summary>This property is meant to be viewed by the user.</summary>
        /// <remarks>
        /// This influences whether the property shows up in the "Choose Columns" dialog, for example. This value is set by the isViewable
        /// attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        IsViewable = 0x00000080,

        /// <summary>This property is included in the list of properties that can be queried.</summary>
        /// <remarks>
        /// A queryable property must also be viewable. This influences whether the property shows up in the query builder UI. This value is
        /// set by the isQueryable attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        IsQueryable = 0x00000100,

        /// <summary>
        /// Used with an innate property (that is, a value calculated from other property values) to indicate that it can be deleted.
        /// </summary>
        /// <remarks>
        /// Windows Vista with Service Pack 1 (SP1) and later. This value is used by the Remove Properties user interface (UI) to determine
        /// whether to display a check box next to an property that allows that property to be selected for removal. Note that a property
        /// that is not innate can always be purged regardless of the presence or absence of this flag.
        /// </remarks>
        CanBePurged = 0x00000200,

        /// <summary>This property is owned by the system.</summary>
        IsSystemProperty = unchecked((int)0x80000000),

        /// <summary>A mask used to retrieve all flags.</summary>
        MaskAll = unchecked((int)0x800001FF),
    }

    /// <summary>Associates property names with property description list strings.</summary>
    [Flags]
    public enum PropertyViewOptions
    {
        /// <summary>The property is shown by default.</summary>
        None = 0x00000000,

        /// <summary>The property is centered.</summary>
        CenterAlign = 0x00000001,

        /// <summary>The property is right aligned.</summary>
        RightAlign = 0x00000002,

        /// <summary>The property is shown as the beginning of the next collection of properties in the view.</summary>
        BeginNewGroup = 0x00000004,

        /// <summary>The remainder of the view area is filled with the content of this property.</summary>
        FillArea = 0x00000008,

        /// <summary>The property is reverse sorted if it is a property in a list of sorted properties.</summary>
        SortDescending = 0x00000010,

        /// <summary>The property is only shown if it is present.</summary>
        ShowOnlyIfPresent = 0x00000020,

        /// <summary>The property is shown by default in a view (where applicable).</summary>
        ShowByDefault = 0x00000040,

        /// <summary>The property is shown by default in primary column selection user interface (UI).</summary>
        ShowInPrimaryList = 0x00000080,

        /// <summary>The property is shown by default in secondary column selection UI.</summary>
        ShowInSecondaryList = 0x00000100,

        /// <summary>The label is hidden if the view is normally inclined to show the label.</summary>
        HideLabel = 0x00000200,

        /// <summary>The property is not displayed as a column in the UI.</summary>
        Hidden = 0x00000800,

        /// <summary>The property is wrapped to the next row.</summary>
        CanWrap = 0x00001000,

        /// <summary>A mask used to retrieve all flags.</summary>
        MaskAll = 0x000003ff,
    }
}
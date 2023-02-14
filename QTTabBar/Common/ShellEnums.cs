//Copyright (c) Microsoft Corporation.  All rights reserved.

namespace QTTabBarLib.Common
{
    /// <summary>One of the values that indicates how the ShellObject DisplayName should look.</summary>
    public enum DisplayNameType
    {
        /// <summary>Returns the display name relative to the desktop.</summary>
        Default = 0x00000000,

        /// <summary>Returns the parsing name relative to the parent folder.</summary>
        RelativeToParent = unchecked((int)0x80018001),

        /// <summary>Returns the path relative to the parent folder in a friendly format as displayed in an address bar.</summary>
        RelativeToParentAddressBar = unchecked((int)0x8007c001),

        /// <summary>Returns the parsing name relative to the desktop.</summary>
        RelativeToDesktop = unchecked((int)0x80028000),

        /// <summary>Returns the editing name relative to the parent folder.</summary>
        RelativeToParentEditing = unchecked((int)0x80031001),

        /// <summary>Returns the editing name relative to the desktop.</summary>
        RelativeToDesktopEditing = unchecked((int)0x8004c000),

        /// <summary>Returns the display name relative to the file system path.</summary>
        FileSystemPath = unchecked((int)0x80058000),

        /// <summary>Returns the display name relative to a URL.</summary>
        Url = unchecked((int)0x80068000),
    }

    /// <summary>CommonFileDialog AddPlace locations</summary>
    public enum FileDialogAddPlaceLocation
    {
        /// <summary>At the bottom of the Favorites or Places list.</summary>
        Bottom = 0x00000000,

        /// <summary>At the top of the Favorites or Places list.</summary>
        Top = 0x00000001,
    }

    /// <summary>Used to describe the view mode.</summary>
    public enum FolderLogicalViewMode
    {
        /// <summary>The view is not specified.</summary>
        Unspecified = -1,

        /// <summary>This should have the same affect as Unspecified.</summary>
        None = 0,

        /// <summary>The minimum valid enumeration value. Used for validation purposes only.</summary>
        First = 1,

        /// <summary>Details view.</summary>
        Details = 1,

        /// <summary>Tiles view.</summary>
        Tiles = 2,

        /// <summary>Icons view.</summary>
        Icons = 3,

        /// <summary>Windows 7 and later. List view.</summary>
        List = 4,

        /// <summary>Windows 7 and later. Content view.</summary>
        Content = 5,

        /// <summary>The maximum valid enumeration value. Used for validation purposes only.</summary>
        Last = 5
    }

    /// <summary>Available Library folder types</summary>
    public enum LibraryFolderType
    {
        /// <summary>General Items</summary>
        Generic = 0,

        /// <summary>Documents</summary>
        Documents,

        /// <summary>Music</summary>
        Music,

        /// <summary>Pictures</summary>
        Pictures,

        /// <summary>Videos</summary>
        Videos
    }

    /// <summary>
    /// Used by IQueryParserManager::SetOption to set parsing options. This can be used to specify schemas and localization options.
    /// </summary>
    public enum QueryParserManagerOption
    {
        /// <summary>
        /// A VT_LPWSTR containing the name of the file that contains the schema binary. The default value is StructuredQuerySchema.bin for
        /// the SystemIndex catalog and StructuredQuerySchemaTrivial.bin for the trivial catalog.
        /// </summary>
        SchemaBinaryName = 0,

        /// <summary>
        /// Either a VT_BOOL or a VT_LPWSTR. If the value is a VT_BOOL and is FALSE, a pre-localized schema will not be used. If the value is
        /// a VT_BOOL and is TRUE, IQueryParserManager will use the pre-localized schema binary in "%ALLUSERSPROFILE%\Microsoft\Windows". If
        /// the value is a VT_LPWSTR, the value should contain the full path of the folder in which the pre-localized schema binary can be
        /// found. The default value is VT_BOOL with TRUE.
        /// </summary>
        PreLocalizedSchemaBinaryPath = 1,

        /// <summary>
        /// A VT_LPWSTR containing the full path to the folder that contains the unlocalized schema binary. The default value is "%SYSTEMROOT%\System32".
        /// </summary>
        UnlocalizedSchemaBinaryPath = 2,

        /// <summary>
        /// A VT_LPWSTR containing the full path to the folder that contains the localized schema binary that can be read and written to as
        /// needed. The default value is "%LOCALAPPDATA%\Microsoft\Windows".
        /// </summary>
        LocalizedSchemaBinaryPath = 3,

        /// <summary>
        /// A VT_BOOL. If TRUE, then the paths for pre-localized and localized binaries have "\(LCID)" appended to them, where language code
        /// identifier (LCID) is the decimal locale ID for the localized language. The default is TRUE.
        /// </summary>
        AppendLCIDToLocalizedPath = 4,

        /// <summary>
        /// A VT_UNKNOWN with an object supporting ISchemaLocalizerSupport. This object will be used instead of the default localizer support object.
        /// </summary>
        LocalizerSupport = 5
    }

    /// <summary>
    /// Provides a set of flags to be used with <see cref="Microsoft.WindowsAPICodePack.Shell.SearchCondition"/> to indicate the operation in
    /// <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory"/> methods.
    /// </summary>
    public enum SearchConditionOperation
    {
        /// <summary>An implicit comparison between the value of the property and the value of the constant.</summary>
        Implicit = 0,

        /// <summary>The value of the property and the value of the constant must be equal.</summary>
        Equal = 1,

        /// <summary>The value of the property and the value of the constant must not be equal.</summary>
        NotEqual = 2,

        /// <summary>The value of the property must be less than the value of the constant.</summary>
        LessThan = 3,

        /// <summary>The value of the property must be greater than the value of the constant.</summary>
        GreaterThan = 4,

        /// <summary>The value of the property must be less than or equal to the value of the constant.</summary>
        LessThanOrEqual = 5,

        /// <summary>The value of the property must be greater than or equal to the value of the constant.</summary>
        GreaterThanOrEqual = 6,

        /// <summary>The value of the property must begin with the value of the constant.</summary>
        ValueStartsWith = 7,

        /// <summary>The value of the property must end with the value of the constant.</summary>
        ValueEndsWith = 8,

        /// <summary>The value of the property must contain the value of the constant.</summary>
        ValueContains = 9,

        /// <summary>The value of the property must not contain the value of the constant.</summary>
        ValueNotContains = 10,

        /// <summary>
        /// The value of the property must match the value of the constant, where '?' matches any single character and '*' matches any
        /// sequence of characters.
        /// </summary>
        DosWildcards = 11,

        /// <summary>The value of the property must contain a word that is the value of the constant.</summary>
        WordEqual = 12,

        /// <summary>The value of the property must contain a word that begins with the value of the constant.</summary>
        WordStartsWith = 13,

        /// <summary>The application is free to interpret this in any suitable way.</summary>
        ApplicationSpecific = 14
    }

    /// <summary>Set of flags to be used with <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory"/>.</summary>
    public enum SearchConditionType
    {
        /// <summary>Indicates that the values of the subterms are combined by "AND".</summary>
        And = 0,

        /// <summary>Indicates that the values of the subterms are combined by "OR".</summary>
        Or = 1,

        /// <summary>Indicates a "NOT" comparison of subterms.</summary>
        Not = 2,

        /// <summary>Indicates that the node is a comparison between a property and a constant value using a <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionOperation"/>.</summary>
        Leaf = 3,
    }

    /// <summary>The direction in which the items are sorted.</summary>
    public enum SortDirection
    {
        /// <summary>A default value for sort direction, this value should not be used; instead use Descending or Ascending.</summary>
        Default = 0,

        /// <summary>
        /// The items are sorted in descending order. Whether the sort is alphabetical, numerical, and so on, is determined by the data type
        /// of the column indicated in propkey.
        /// </summary>
        Descending = -1,

        /// <summary>
        /// The items are sorted in ascending order. Whether the sort is alphabetical, numerical, and so on, is determined by the data type
        /// of the column indicated in propkey.
        /// </summary>
        Ascending = 1,
    }

    /// <summary>Provides a set of flags to be used with IQueryParser::SetMultiOption to indicate individual options.</summary>
    public enum StructuredQueryMultipleOption
    {
        /// <summary>
        /// The key should be property name P. The value should be a VT_UNKNOWN with an IEnumVARIANT which has two values: a VT_BSTR that is
        /// another property name Q and a VT_I4 that is a CONDITION_OPERATION cop. A predicate with property name P, some operation and a
        /// value V will then be replaced by a predicate with property name Q, operation cop and value V before further processing happens.
        /// </summary>
        VirtualProperty,

        /// <summary>
        /// The key should be a value type name V. The value should be a VT_LPWSTR with a property name P. A predicate with no property name
        /// and a value of type V (or any subtype of V) will then use property P.
        /// </summary>
        DefaultProperty,

        /// <summary>
        /// The key should be a value type name V. The value should be a VT_UNKNOWN with a IConditionGenerator G. The GenerateForLeaf method
        /// of G will then be applied to any predicate with value type V and if it returns a query expression, that will be used. If it
        /// returns NULL, normal processing will be used instead.
        /// </summary>
        GeneratorForType,

        /// <summary>
        /// The key should be a property name P. The value should be a VT_VECTOR|VT_LPWSTR, where each string is a property name. The count
        /// must be at least one. This "map" will be added to those of the loaded schema and used during resolution. A second call with the
        /// same key will replace the current map. If the value is VT_NULL, the map will be removed.
        /// </summary>
        MapProperty,
    }

    /// <summary>Provides a set of flags to be used with IQueryParser::SetOption and IQueryParser::GetOption to indicate individual options.</summary>
    public enum StructuredQuerySingleOption
    {
        /// <summary>The value should be VT_LPWSTR and the path to a file containing a schema binary.</summary>
        Schema,

        /// <summary>
        /// The value must be VT_EMPTY (the default) or a VT_UI4 that is an LCID. It is used as the locale of contents (not keywords) in the
        /// query to be searched for, when no other information is available. The default value is the current keyboard locale. Retrieving
        /// the value always returns a VT_UI4.
        /// </summary>
        Locale,

        /// <summary>
        /// This option is used to override the default word breaker used when identifying keywords in queries. The default word breaker is
        /// chosen according to the language of the keywords (cf. SQSO_LANGUAGE_KEYWORDS below). When setting this option, the value should
        /// be VT_EMPTY for using the default word breaker, or a VT_UNKNOWN with an object supporting the IWordBreaker interface. Retrieving
        /// the option always returns a VT_UNKNOWN with an object supporting the IWordBreaker interface.
        /// </summary>
        WordBreaker,

        /// <summary>
        /// The value should be VT_EMPTY or VT_BOOL with VARIANT_TRUE to allow natural query syntax (the default) or VT_BOOL with
        /// VARIANT_FALSE to allow only advanced query syntax. Retrieving the option always returns a VT_BOOL. This option is now deprecated,
        /// use SQSO_SYNTAX.
        /// </summary>
        NaturalSyntax,

        /// <summary>
        /// The value should be VT_BOOL with VARIANT_TRUE to generate query expressions as if each word in the query had a star appended to
        /// it (unless followed by punctuation other than a parenthesis), or VT_EMPTY or VT_BOOL with VARIANT_FALSE to use the words as they
        /// are (the default). A word-wheeling application will generally want to set this option to true. Retrieving the option always
        /// returns a VT_BOOL.
        /// </summary>
        AutomaticWildcard,

        /// <summary>Reserved. The value should be VT_EMPTY (the default) or VT_I4. Retrieving the option always returns a VT_I4.</summary>
        TraceLevel,

        /// <summary>The value must be a VT_UI4 that is a LANGID. It defaults to the default user UI language.</summary>
        LanguageKeywords,

        /// <summary>The value must be a VT_UI4 that is a STRUCTURED_QUERY_SYNTAX value. It defaults to SQS_NATURAL_QUERY_SYNTAX.</summary>
        Syntax,

        /// <summary>
        /// The value must be a VT_BLOB that is a copy of a TIME_ZONE_INFORMATION structure. It defaults to the current time zone.
        /// </summary>
        TimeZone,

        /// <summary>
        /// This setting decides what connector should be assumed between conditions when none is specified. The value must be a VT_UI4 that
        /// is a CONDITION_TYPE. Only CT_AND_CONDITION and CT_OR_CONDITION are valid. It defaults to CT_AND_CONDITION.
        /// </summary>
        ImplicitConnector,

        /// <summary>
        /// This setting decides whether there are special requirements on the case of connector keywords (such as AND or OR). The value must
        /// be a VT_UI4 that is a CASE_REQUIREMENT value. It defaults to CASE_REQUIREMENT_UPPER_IF_AQS.
        /// </summary>
        ConnectorCase,
    }

    /// <summary>Flags controlling the appearance of a window</summary>
    public enum WindowShowCommand
    {
        /// <summary>Hides the window and activates another window.</summary>
        Hide = 0,

        /// <summary>Activates and displays the window (including restoring it to its original size and position).</summary>
        Normal = 1,

        /// <summary>Minimizes the window.</summary>
        Minimized = 2,

        /// <summary>Maximizes the window.</summary>
        Maximized = 3,

        /// <summary>Similar to <see cref="Normal"/>, except that the window is not activated.</summary>
        ShowNoActivate = 4,

        /// <summary>Activates the window and displays it in its current size and position.</summary>
        Show = 5,

        /// <summary>Minimizes the window and activates the next top-level window.</summary>
        Minimize = 6,

        /// <summary>Minimizes the window and does not activate it.</summary>
        ShowMinimizedNoActivate = 7,

        /// <summary>Similar to <see cref="Normal"/>, except that the window is not activated.</summary>
        ShowNA = 8,

        /// <summary>Activates and displays the window, restoring it to its original size and position.</summary>
        Restore = 9,

        /// <summary>Sets the show state based on the initial value specified when the process was created.</summary>
        Default = 10,

        /// <summary>
        /// Minimizes a window, even if the thread owning the window is not responding. Use this only to minimize windows from a different thread.
        /// </summary>
        ForceMinimize = 11
    }
}
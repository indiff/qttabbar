//Copyright (c) Microsoft Corporation.  All rights reserved.


namespace QTTabBarLib.Common
{
    /// <summary>Defines the enumeration values for a property type.</summary>
    public class ShellPropertyEnumType
    {
        private string displayText;
        private PropEnumType? enumType;
        private object minValue, setValue, enumerationValue;

        internal ShellPropertyEnumType(IPropertyEnumType nativePropertyEnumType)
        {
            NativePropertyEnumType = nativePropertyEnumType;
        }

        /// <summary>Gets display text from an enumeration information structure.</summary>
        public string DisplayText
        {
            get
            {
                if (displayText == null)
                {
                    NativePropertyEnumType.GetDisplayText(out displayText);
                }
                return displayText;
            }
        }

        /// <summary>Gets an enumeration type from an enumeration information structure.</summary>
        public PropEnumType EnumType
        {
            get
            {
                if (!enumType.HasValue)
                {
                    PropEnumType tempEnumType ;
                    NativePropertyEnumType.GetEnumType(out tempEnumType);
                    enumType = tempEnumType;
                }
                return enumType.Value;
            }
        }

        /// <summary>Gets a minimum value from an enumeration information structure.</summary>
        public object RangeMinValue
        {
            get
            {
                if (minValue == null)
                {
                    using (var propVar = new PropVariant())
                    {
                        NativePropertyEnumType.GetRangeMinValue(propVar);
                        minValue = propVar.Value;
                    }
                }
                return minValue;
            }
        }

        /// <summary>Gets a set value from an enumeration information structure.</summary>
        public object RangeSetValue
        {
            get
            {
                if (setValue == null)
                {
                    using (var propVar = new PropVariant())
                    {
                        NativePropertyEnumType.GetRangeSetValue(propVar);
                        setValue = propVar.Value;
                    }
                }
                return setValue;
            }
        }

        /// <summary>Gets a value from an enumeration information structure.</summary>
        public object RangeValue
        {
            get
            {
                if (enumerationValue == null)
                {
                    using (var propVar = new PropVariant())
                    {
                        NativePropertyEnumType.GetValue(propVar);
                        enumerationValue = propVar.Value;
                    }
                }
                return enumerationValue;
            }
        }

        private IPropertyEnumType NativePropertyEnumType
        {
            set;
            get;
        }
    }
}
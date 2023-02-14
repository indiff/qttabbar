//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>
    /// Defines a strongly-typed property object. All writable property objects must be of this type to be able to call the value setter.
    /// </summary>
    /// <typeparam name="T">The type of this property's value. Because a property value can be empty, only nullable types are allowed.</typeparam>
    public class ShellProperty<T> : IShellProperty
    {
        private readonly ShellPropertyDescription description = null;
        private int? imageReferenceIconIndex;
        private string imageReferencePath = null;
        private PropertyKey propertyKey;

        /// <summary>Constructs a new Property object</summary>
        /// <param name="propertyKey"></param>
        /// <param name="description"></param>
        /// <param name="parent"></param>
        internal ShellProperty(
            PropertyKey propertyKey,
            ShellPropertyDescription description,
            ShellObject parent)
        {
            this.propertyKey = propertyKey;
            this.description = description;
            ParentShellObject = parent;
            AllowSetTruncatedValue = false;
        }

        /// <summary>Constructs a new Property object</summary>
        /// <param name="propertyKey"></param>
        /// <param name="description"></param>
        /// <param name="propertyStore"></param>
        internal ShellProperty(
            PropertyKey propertyKey,
            ShellPropertyDescription description,
            IPropertyStore propertyStore)
        {
            this.propertyKey = propertyKey;
            this.description = description;
            NativePropertyStore = propertyStore;
            AllowSetTruncatedValue = false;
        }

        /// <summary>Gets or sets a value that determines if a value can be truncated. The default for this property is false.</summary>
        /// <remarks>
        /// An <see cref="ArgumentOutOfRangeException"/> will be thrown if this property is not set to true, and a property value was set but
        /// later truncated.
        /// </remarks>
        public bool AllowSetTruncatedValue { get; set; }

        /// <summary>Get the property description object.</summary>
        public ShellPropertyDescription Description
        {
            get
            {
                return description;
            }
        }

        /// <summary>Gets the image reference path and icon index associated with a property value (Windows 7 only).</summary>
        public IconReference IconReference
        {
            get
            {
                if (!CoreHelpers.RunningOnWin7)
                {
                    throw new PlatformNotSupportedException(LocalizedMessages.ShellPropertyWindows7);
                }

                GetImageReference();
                var index = (imageReferenceIconIndex.HasValue ? imageReferenceIconIndex.Value : -1);

                return new IconReference(imageReferencePath, index);
            }
        }

        /// <summary>Gets the property key identifying this property.</summary>
        public PropertyKey PropertyKey
        {
            get
            {
                return propertyKey;
            }
        }

        /// <summary>
        /// Gets or sets the strongly-typed value of this property. The value of the property is cleared if the value is set to null.
        /// </summary>
        /// <exception cref="System.Runtime.InteropServices.COMException">
        /// If the property value cannot be retrieved or updated in the Property System
        /// </exception>
        /// <exception cref="NotSupportedException">If the type of this property is not supported; e.g. writing a binary object.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <see cref="AllowSetTruncatedValue"/> is false, and either a string value was truncated or a numeric value was rounded.
        /// </exception>
        public T Value
        {
            get
            {
                // Make sure we load the correct type
                Debug.Assert(ValueType == ShellPropertyFactory.VarEnumToSystemType(Description.VarEnumType));

                using (var propVar = new PropVariant())
                {
                    if (ParentShellObject.NativePropertyStore != null)
                    {
                        // If there is a valid property store for this shell object, then use it.
                        ParentShellObject.NativePropertyStore.GetValue(ref propertyKey, propVar);
                    }
                    else if (ParentShellObject != null)
                    {
                        // Use IShellItem2.GetProperty instead of creating a new property store The file might be locked. This is probably
                        // quicker, and sufficient for what we need
                        ParentShellObject.NativeShellItem2.GetProperty(ref propertyKey, propVar);
                    }
                    else if (NativePropertyStore != null)
                    {
                        NativePropertyStore.GetValue(ref propertyKey, propVar);
                    }

                    //Get the value
                    return propVar.Value != null ? (T)propVar.Value : default(T);
                }
            }
            set
            {
                // Make sure we use the correct type
                Debug.Assert(ValueType == ShellPropertyFactory.VarEnumToSystemType(Description.VarEnumType));

                if (typeof(T) != ValueType)
                {
                    throw new NotSupportedException(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        LocalizedMessages.ShellPropertyWrongType, ValueType.Name));
                }

                if (value is Nullable)
                {
                    var t = typeof(T);
                    var pi = t.GetProperty("HasValue");
                    if (pi != null)
                    {
                        var hasValue = (bool)pi.GetValue(value, null);
                        if (!hasValue)
                        {
                            ClearValue();
                            return;
                        }
                    }
                }
                else if (value == null)
                {
                    ClearValue();
                    return;
                }

                if (ParentShellObject != null)
                {
                    using (var propertyWriter = ParentShellObject.Properties.GetPropertyWriter())
                    {
                        propertyWriter.WriteProperty<T>(this, value, AllowSetTruncatedValue);
                    }
                }
                else if (NativePropertyStore != null)
                {
                    throw new InvalidOperationException(LocalizedMessages.ShellPropertyCannotSetProperty);
                }
            }
        }

        /// <summary>
        /// Gets the value for this property using the generic Object type. To obtain a specific type for this value, use the more type
        /// strong Property&lt;T&gt; class. Also, you can only set a value for this type using Property&lt;T&gt;
        /// </summary>
        public object ValueAsObject
        {
            get
            {
                using (var propVar = new PropVariant())
                {
                    if (ParentShellObject != null)
                    {
                        var store = ShellPropertyCollection.CreateDefaultPropertyStore(ParentShellObject);

                        store.GetValue(ref propertyKey, propVar);

                        Marshal.ReleaseComObject(store);
                        store = null;
                    }
                    else if (NativePropertyStore != null)
                    {
                        NativePropertyStore.GetValue(ref propertyKey, propVar);
                    }

                    return propVar != null ? propVar.Value : null;
                }
            }
        }

        /// <summary>Gets the associated runtime type.</summary>
        public Type ValueType
        {
            get
            {
                // The type for this object need to match that of the description
                Debug.Assert(Description.ValueType == typeof(T));

                return Description.ValueType;
            }
        }

        /// <summary>Gets the case-sensitive name of a property as it is known to the system, regardless of its localized name.</summary>
        public string CanonicalName
        {
            get
            {
                return Description.CanonicalName;
            }
        }

        private IPropertyStore NativePropertyStore { get; set; }
        private ShellObject ParentShellObject { get; set; }

        /// <summary>Clears the value of the property.</summary>
        public void ClearValue()
        {
            using (var propVar = new PropVariant())
            {
                StorePropVariantValue(propVar);
            }
        }

        /// <summary>Returns a formatted, Unicode string representation of a property value.</summary>
        /// <param name="format">One or more of the PropertyDescriptionFormat flags that indicate the desired format.</param>
        /// <returns>The formatted value as a string, or null if this property cannot be formatted for display.</returns>
        public string FormatForDisplay(PropertyDescriptionFormatOptions format)
        {
            if (Description == null || Description.NativePropertyDescription == null)
            {
                // We cannot do anything without a property description
                return null;
            }

            var store = ShellPropertyCollection.CreateDefaultPropertyStore(ParentShellObject);

            using (var propVar = new PropVariant())
            {
                store.GetValue(ref propertyKey, propVar);

                // Release the Propertystore
                Marshal.ReleaseComObject(store);
                store = null;
                string formattedString;
                var hr = Description.NativePropertyDescription.FormatForDisplay(propVar, ref format, out formattedString);

                // Sometimes, the value cannot be displayed properly, such as for blobs or if we get argument exception
                if (!CoreErrorHelper.Succeeded(hr))
                    throw new ShellException(hr);

                return formattedString;
            }
        }

        /// <summary>Returns a formatted, Unicode string representation of a property value.</summary>
        /// <param name="format">One or more of the PropertyDescriptionFormat flags that indicate the desired format.</param>
        /// <param name="formattedString">The formatted value as a string, or null if this property cannot be formatted for display.</param>
        /// <returns>True if the method successfully locates the formatted string; otherwise False.</returns>
        public bool TryFormatForDisplay(PropertyDescriptionFormatOptions format, out string formattedString)
        {
            if (Description == null || Description.NativePropertyDescription == null)
            {
                // We cannot do anything without a property description
                formattedString = null;
                return false;
            }

            var store = ShellPropertyCollection.CreateDefaultPropertyStore(ParentShellObject);

            using (var propVar = new PropVariant())
            {
                store.GetValue(ref propertyKey, propVar);

                // Release the Propertystore
                Marshal.ReleaseComObject(store);
                store = null;

                var hr = Description.NativePropertyDescription.FormatForDisplay(propVar, ref format, out formattedString);

                // Sometimes, the value cannot be displayed properly, such as for blobs or if we get argument exception
                if (!CoreErrorHelper.Succeeded(hr))
                {
                    formattedString = null;
                    return false;
                }
                return true;
            }
        }

        private void GetImageReference()
        {
            var store = ShellPropertyCollection.CreateDefaultPropertyStore(ParentShellObject);

            using (var propVar = new PropVariant())
            {
                store.GetValue(ref propertyKey, propVar);

                Marshal.ReleaseComObject(store);
                store = null;
                string refPath;
                ((IPropertyDescription2)Description.NativePropertyDescription).GetImageReferenceForValue(
                    propVar, out refPath);

                if (refPath == null) { return; }

                var index = ShellNativeMethods.PathParseIconLocation(ref refPath);
                if (refPath != null)
                {
                    imageReferencePath = refPath;
                    imageReferenceIconIndex = index;
                }
            }
        }

        private void StorePropVariantValue(PropVariant propVar)
        {
            var guid = new Guid(ShellIIDGuid.IPropertyStore);
            IPropertyStore writablePropStore = null;
            try
            {
                var hr = ParentShellObject.NativeShellItem2.GetPropertyStore(
                        ShellNativeMethods.GetPropertyStoreOptions.ReadWrite,
                        ref guid,
                        out writablePropStore);

                if (!CoreErrorHelper.Succeeded(hr))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty,
                        Marshal.GetExceptionForHR(hr));
                }

                var result = writablePropStore.SetValue(ref propertyKey, propVar);

                if (!AllowSetTruncatedValue && (int)result == ShellNativeMethods.InPlaceStringTruncated)
                {
                    throw new ArgumentOutOfRangeException("propVar", LocalizedMessages.ShellPropertyValueTruncated);
                }

                if (!CoreErrorHelper.Succeeded(result))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertySetValue, Marshal.GetExceptionForHR((int)result));
                }

                writablePropStore.Commit();
            }
            catch (InvalidComObjectException e)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty, e);
            }
            catch (InvalidCastException)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty);
            }
            finally
            {
                if (writablePropStore != null)
                {
                    Marshal.ReleaseComObject(writablePropStore);
                    writablePropStore = null;
                }
            }
        }
    }
}
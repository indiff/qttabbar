//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;

namespace QTTabBarLib.Common
{
    /// <summary>Creates a property writer capable of setting multiple properties for a given ShellObject.</summary>
    public class ShellPropertyWriter : IDisposable
    {
        // Reference to our writable PropertyStore
        internal IPropertyStore writablePropStore;

        private ShellObject parentShellObject;

        internal ShellPropertyWriter(ShellObject parent)
        {
            ParentShellObject = parent;

            // Open the property store for this shell object...
            var guid = new Guid(ShellIIDGuid.IPropertyStore);

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
                else
                {
                    // If we succeed in creating a valid property store for this ShellObject, then set it on the parent shell object for
                    // others to use. Once this writer is closed/commited, we will set the
                    if (ParentShellObject.NativePropertyStore == null)
                    {
                        ParentShellObject.NativePropertyStore = writablePropStore;
                    }
                }
            }
            catch (InvalidComObjectException e)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty, e);
            }
            catch (InvalidCastException)
            {
                throw new PropertySystemException(LocalizedMessages.ShellPropertyUnableToGetWritableProperty);
            }
        }

        /// <summary></summary>
        ~ShellPropertyWriter()
        {
            Dispose(false);
        }

        /// <summary>Reference to parent ShellObject (associated with this writer)</summary>
        protected ShellObject ParentShellObject
        {
            get
            {
                return parentShellObject;
            }
            private set
            {
                parentShellObject = value;
            }
        }

        /// <summary>Call this method to commit the writes (calls to WriteProperty method) and dispose off the writer.</summary>
        public void Close()
        {
            // Close the property writer (commit, etc)
            if (writablePropStore != null)
            {
                writablePropStore.Commit();

                Marshal.ReleaseComObject(writablePropStore);
                writablePropStore = null;
            }

            ParentShellObject.NativePropertyStore = null;
        }

        /// <summary>Release the native objects.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Writes the given property key and value.</summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The value associated with the key.</param>
        public void WriteProperty(PropertyKey key, object value)
        {
            WriteProperty(key, value, true);
        }

        /// <summary>Writes the given property key and value. To allow truncation of the given value, set allowTruncatedValue to true.</summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        /// <exception cref="System.InvalidOperationException">If the writable property store is already closed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If AllowTruncatedValue is set to false and while setting the value on the property it had to be truncated in a string or rounded
        /// in a numeric value.
        /// </exception>
        public void WriteProperty(PropertyKey key, object value, bool allowTruncatedValue)
        {
            if (writablePropStore == null)
                throw new InvalidOperationException("Writeable store has been closed.");

            using (var propVar = PropVariant.FromObject(value))
            {
                var result = writablePropStore.SetValue(ref key, propVar);

                if (!allowTruncatedValue && ((int)result == ShellNativeMethods.InPlaceStringTruncated))
                {
                    // At this point we can't revert back the commit so don't commit, close the property store and throw an exception to let
                    // the user know.
                    Marshal.ReleaseComObject(writablePropStore);
                    writablePropStore = null;

                    throw new ArgumentOutOfRangeException("value", LocalizedMessages.ShellPropertyValueTruncated);
                }

                if (!CoreErrorHelper.Succeeded(result))
                {
                    throw new PropertySystemException(LocalizedMessages.ShellPropertySetValue, Marshal.GetExceptionForHR((int)result));
                }
            }
        }

        /// <summary>Writes the specified property given the canonical name and a value.</summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <param name="value">The property value.</param>
        public void WriteProperty(string canonicalName, object value)
        {
            WriteProperty(canonicalName, value, true);
        }

        /// <summary>
        /// Writes the specified property given the canonical name and a value. To allow truncation of the given value, set
        /// allowTruncatedValue to true.
        /// </summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        /// <exception cref="System.ArgumentException">If the given canonical name is not valid.</exception>
        public void WriteProperty(string canonicalName, object value, bool allowTruncatedValue)
        {
            // Get the PropertyKey using the canonicalName passed in
            PropertyKey propKey;
            var result = PropertySystemNativeMethods.PSGetPropertyKeyFromName(canonicalName, out propKey);

            if (!CoreErrorHelper.Succeeded(result))
            {
                throw new ArgumentException(
                    LocalizedMessages.ShellInvalidCanonicalName,
                    Marshal.GetExceptionForHR(result));
            }

            WriteProperty(propKey, value, allowTruncatedValue);
        }

        /// <summary>Writes the specified property using an IShellProperty and a value.</summary>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        public void WriteProperty(IShellProperty shellProperty, object value)
        {
            WriteProperty(shellProperty, value, true);
        }

        /// <summary>
        /// Writes the specified property given an IShellProperty and a value. To allow truncation of the given value, set
        /// allowTruncatedValue to true.
        /// </summary>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        public void WriteProperty(IShellProperty shellProperty, object value, bool allowTruncatedValue)
        {
            if (shellProperty == null) { throw new ArgumentNullException("shellProperty"); }
            WriteProperty(shellProperty.PropertyKey, value, allowTruncatedValue);
        }

        /// <summary>Writes the specified property using a strongly-typed ShellProperty and a value.</summary>
        /// <typeparam name="T">The type of the property name.</typeparam>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        public void WriteProperty<T>(ShellProperty<T> shellProperty, T value)
        {
            WriteProperty<T>(shellProperty, value, true);
        }

        /// <summary>
        /// Writes the specified property given a strongly-typed ShellProperty and a value. To allow truncation of the given value, set
        /// allowTruncatedValue to true.
        /// </summary>
        /// <typeparam name="T">The type of the property name.</typeparam>
        /// <param name="shellProperty">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="allowTruncatedValue">True to allow truncation (default); otherwise False.</param>
        public void WriteProperty<T>(ShellProperty<T> shellProperty, T value, bool allowTruncatedValue)
        {
            if (shellProperty == null) { throw new ArgumentNullException("shellProperty"); }
            WriteProperty(shellProperty.PropertyKey, value, allowTruncatedValue);
        }

        /// <summary>Release the native and managed objects.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            Close();
        }
    }
}
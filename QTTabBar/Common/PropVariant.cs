//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using QTTabBarLib.Common;

namespace QTTabBarLib.Common
{
    /// <summary>Represents the OLE struct PROPVARIANT. This class is intended for internal use only.</summary>
    /// <remarks>
    /// Originally sourced from http://blogs.msdn.com/adamroot/pages/interop-with-propvariants-in-net.aspx and modified to support additional
    /// types including vectors and ability to set values
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public sealed class PropVariant : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Blob
        {
            public int Number;
            public IntPtr Pointer;
        }

        // A static dictionary of delegates to get data from array's contained within PropVariants
        private static Dictionary<Type, Action<PropVariant, Array, uint>> _vectorActions = null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Dictionary<Type, Action<PropVariant, Array, uint>> GenerateVectorActions()
        {
            Dictionary<Type, Action<PropVariant, Array, uint>> cache = new Dictionary<Type, Action<PropVariant, Array, uint>>
            {
                {
                    typeof(short),
                    (pv, array, i) =>
                    {
                        short val ;
                        PropVariantNativeMethods.PropVariantGetInt16Elem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(ushort),
                    (pv, array, i) =>
                    {
                        ushort val ;
                        PropVariantNativeMethods.PropVariantGetUInt16Elem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(int),
                    (pv, array, i) =>
                    {
                        int val ;
                        PropVariantNativeMethods.PropVariantGetInt32Elem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(uint),
                    (pv, array, i) =>
                    {
                        uint val ;
                        PropVariantNativeMethods.PropVariantGetUInt32Elem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(long),
                    (pv, array, i) =>
                    {
                        long val ;
                        PropVariantNativeMethods.PropVariantGetInt64Elem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(ulong),
                    (pv, array, i) =>
                    {
                        ulong val;
                        PropVariantNativeMethods.PropVariantGetUInt64Elem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(DateTime),
                    (pv, array, i) =>
                    {
                        System.Runtime.InteropServices.ComTypes.FILETIME val;
                        PropVariantNativeMethods.PropVariantGetFileTimeElem(pv, i, out val);

                        long fileTime = GetFileTimeAsLong(ref val);

                        array.SetValue(DateTime.FromFileTime(fileTime), i);
                    }
                },

                {
                    typeof(bool),
                    (pv, array, i) =>
                    {
                        bool val;
                        PropVariantNativeMethods.PropVariantGetBooleanElem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(double),
                    (pv, array, i) =>
                    {
                        double val;
                        PropVariantNativeMethods.PropVariantGetDoubleElem(pv, i, out val);
                        array.SetValue(val, i);
                    }
                },

                {
                    typeof(float),
                    (pv, array, i) => // float
                    {
                        float[] val = new float[1];
                        Marshal.Copy(pv._blob.Pointer, val, (int)i, 1);
                        array.SetValue(val[0], (int)i);
                    }
                },

                {
                    typeof(decimal),
                    (pv, array, i) =>
                    {
                        int[] val = new int[4];
                        for (int a = 0; a < val.Length; a++)
                        {
                            val[a] = Marshal.ReadInt32(pv._blob.Pointer,
                                (int)i * sizeof(decimal) + a * sizeof(int)); //index * size + offset quarter
                                  }
                        array.SetValue(new decimal(val), i);
                    }
                },

                {
                    typeof(string),
                    (pv, array, i) =>
                    {
                        string val = string.Empty;
                        PropVariantNativeMethods.PropVariantGetStringElem(pv, i, ref val);
                        array.SetValue(val, i);
                    }
                }
            };

            return cache;
        }

        /// <summary>Attempts to create a PropVariant by finding an appropriate constructor.</summary>
        /// <param name="value">Object from which PropVariant should be created.</param>
        public static PropVariant FromObject(object value)
        {
            if (value == null)
            {
                return new PropVariant();
            }
            else
            {
                Func<object, PropVariant> func = GetDynamicConstructor(value.GetType());
                return func(value);
            }
        }

        // A dictionary and lock to contain compiled expression trees for constructors
        private static readonly Dictionary<Type, Func<object, PropVariant>> _cache = new Dictionary<Type, Func<object, PropVariant>>();

        private static readonly object _padlock = new object();

        // Retrieves a cached constructor expression. If no constructor has been cached, it attempts to find/add it. If it cannot be found an
        // exception is thrown. This method looks for a public constructor with the same parameter type as the object.
        private static Func<object, PropVariant> GetDynamicConstructor(Type type)
        {
            lock (_padlock)
            {
                Func<object, PropVariant> action;
                // initial check, if action is found, return it
                if (!_cache.TryGetValue(type, out action))
                {
                    // iterates through all constructors
                    System.Reflection.ConstructorInfo constructor = typeof(PropVariant)
                        .GetConstructor(new Type[] { type });

                    if (constructor == null)
                    {
                        // if the method was not found, throw.
                        throw new ArgumentException(LocalizedMessages.PropVariantTypeNotSupported);
                    }
                    else // if the method was found, create an expression to call it.
                    {
                        // create parameters to action
                        ParameterExpression arg = Expression.Parameter(typeof(object), "arg");

                        // create an expression to invoke the constructor with an argument cast to the correct type
                        NewExpression create = Expression.New(constructor, Expression.Convert(arg, type));

                        // compiles expression into an action delegate
                        action = Expression.Lambda<Func<object, PropVariant>>(create, arg).Compile();
                        _cache.Add(type, action);
                    }
                }
                return action;
            }
        }

        [FieldOffset(0)]
        private readonly decimal _decimal;

        // This is actually a VarEnum value, but the VarEnum type requires 4 bytes instead of the expected 2.
        [FieldOffset(0)]
        private ushort _valueType;

        // Reserved Fields
        //[FieldOffset(2)]
        //ushort _wReserved1;
        //[FieldOffset(4)]
        //ushort _wReserved2;
        //[FieldOffset(6)]
        //ushort _wReserved3;

        [FieldOffset(8)]
        private readonly Blob _blob;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        [FieldOffset(8)]
        private IntPtr _ptr;

        [FieldOffset(8)]
        private readonly int _int32;

        [FieldOffset(8)]
        private readonly uint _uint32;

        [FieldOffset(8)]
        private readonly byte _byte;

        [FieldOffset(8)]
        private readonly sbyte _sbyte;

        [FieldOffset(8)]
        private readonly short _short;

        [FieldOffset(8)]
        private readonly ushort _ushort;

        [FieldOffset(8)]
        private readonly long _long;

        [FieldOffset(8)]
        private readonly ulong _ulong;

        [FieldOffset(8)]
        private readonly double _double;

        [FieldOffset(8)]
        private readonly float _float;

        /// <summary>Default constrcutor</summary>
        public PropVariant()
        {
            // left empty
        }

        /// <summary>Set a string value</summary>
        public PropVariant(string value)
        {
            if (value == null)
            {
                throw new ArgumentException(LocalizedMessages.PropVariantNullString, "value");
            }

            _valueType = (ushort)VarEnum.VT_LPWSTR;
            _ptr = Marshal.StringToCoTaskMemUni(value);
        }

        /// <summary>Set a string vector</summary>
        public PropVariant(string[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromStringVector(value, (uint)value.Length, this);
        }

        /// <summary>Set a bool vector</summary>
        public PropVariant(bool[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromBooleanVector(value, (uint)value.Length, this);
        }

        /// <summary>Set a short vector</summary>
        public PropVariant(short[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromInt16Vector(value, (uint)value.Length, this);
        }

        /// <summary>Set a short vector</summary>
        public PropVariant(ushort[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromUInt16Vector(value, (uint)value.Length, this);
        }

        /// <summary>Set an int vector</summary>
        public PropVariant(int[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromInt32Vector(value, (uint)value.Length, this);
        }

        /// <summary>Set an uint vector</summary>
        public PropVariant(uint[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromUInt32Vector(value, (uint)value.Length, this);
        }

        /// <summary>Set a long vector</summary>
        public PropVariant(long[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromInt64Vector(value, (uint)value.Length, this);
        }

        /// <summary>Set a ulong vector</summary>
        public PropVariant(ulong[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromUInt64Vector(value, (uint)value.Length, this);
        }

        /// <summary>&gt; Set a double vector</summary>
        public PropVariant(double[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromDoubleVector(value, (uint)value.Length, this);
        }

        /// <summary>Set a DateTime vector</summary>
        public PropVariant(DateTime[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }
            System.Runtime.InteropServices.ComTypes.FILETIME[] fileTimeArr =
                new System.Runtime.InteropServices.ComTypes.FILETIME[value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                fileTimeArr[i] = DateTimeToFileTime(value[i]);
            }

            PropVariantNativeMethods.InitPropVariantFromFileTimeVector(fileTimeArr, (uint)fileTimeArr.Length, this);
        }

        /// <summary>Set a bool value</summary>
        public PropVariant(bool value)
        {
            _valueType = (ushort)VarEnum.VT_BOOL;
            _int32 = (value == true) ? -1 : 0;
        }

        /// <summary>Set a DateTime value</summary>
        public PropVariant(DateTime value)
        {
            _valueType = (ushort)VarEnum.VT_FILETIME;

            System.Runtime.InteropServices.ComTypes.FILETIME ft = DateTimeToFileTime(value);
            PropVariantNativeMethods.InitPropVariantFromFileTime(ref ft, this);
        }

        /// <summary>Set a byte value</summary>
        public PropVariant(byte value)
        {
            _valueType = (ushort)VarEnum.VT_UI1;
            _byte = value;
        }

        /// <summary>Set a sbyte value</summary>
        public PropVariant(sbyte value)
        {
            _valueType = (ushort)VarEnum.VT_I1;
            _sbyte = value;
        }

        /// <summary>Set a short value</summary>
        public PropVariant(short value)
        {
            _valueType = (ushort)VarEnum.VT_I2;
            _short = value;
        }

        /// <summary>Set an unsigned short value</summary>
        public PropVariant(ushort value)
        {
            _valueType = (ushort)VarEnum.VT_UI2;
            _ushort = value;
        }

        /// <summary>Set an int value</summary>
        public PropVariant(int value)
        {
            _valueType = (ushort)VarEnum.VT_I4;
            _int32 = value;
        }

        /// <summary>Set an unsigned int value</summary>
        public PropVariant(uint value)
        {
            _valueType = (ushort)VarEnum.VT_UI4;
            _uint32 = value;
        }

        /// <summary>Set a decimal value</summary>
        public PropVariant(decimal value)
        {
            _decimal = value;

            // It is critical that the value type be set after the decimal value, because they overlap. If valuetype is written first, its
            // value will be lost when _decimal is written.
            _valueType = (ushort)VarEnum.VT_DECIMAL;
        }

        /// <summary>Create a PropVariant with a contained decimal array.</summary>
        /// <param name="value">Decimal array to wrap.</param>
        public PropVariant(decimal[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            _valueType = (ushort)(VarEnum.VT_DECIMAL | VarEnum.VT_VECTOR);
            _int32 = value.Length;

            // allocate required memory for array with 128bit elements
            _blob.Pointer = Marshal.AllocCoTaskMem(value.Length * sizeof(decimal));
            for (int i = 0; i < value.Length; i++)
            {
                int[] bits = decimal.GetBits(value[i]);
                Marshal.Copy(bits, 0, _blob.Pointer, bits.Length);
            }
        }

        /// <summary>Create a PropVariant containing a float type.</summary>
        public PropVariant(float value)
        {
            _valueType = (ushort)VarEnum.VT_R4;

            _float = value;
        }

        /// <summary>Creates a PropVariant containing a float[] array.</summary>
        public PropVariant(float[] value)
        {
            if (value == null) { throw new ArgumentNullException("value"); }

            _valueType = (ushort)(VarEnum.VT_R4 | VarEnum.VT_VECTOR);
            _int32 = value.Length;

            _blob.Pointer = Marshal.AllocCoTaskMem(value.Length * sizeof(float));

            Marshal.Copy(value, 0, _blob.Pointer, value.Length);
        }

        /// <summary>Set a long</summary>
        public PropVariant(long value)
        {
            _long = value;
            _valueType = (ushort)VarEnum.VT_I8;
        }

        /// <summary>Set a ulong</summary>
        public PropVariant(ulong value)
        {
            _valueType = (ushort)VarEnum.VT_UI8;
            _ulong = value;
        }

        /// <summary>Set a double</summary>
        public PropVariant(double value)
        {
            _valueType = (ushort)VarEnum.VT_R8;
            _double = value;
        }

        /// <summary>Set an IUnknown value</summary>
        /// <param name="value">The new value to set.</param>
        internal void SetIUnknown(object value)
        {
            _valueType = (ushort)VarEnum.VT_UNKNOWN;
            _ptr = Marshal.GetIUnknownForObject(value);
        }

        /// <summary>Set a safe array value</summary>
        /// <param name="array">The new value to set.</param>
        internal void SetSafeArray(Array array)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            const ushort vtUnknown = 13;
            IntPtr psa = PropVariantNativeMethods.SafeArrayCreateVector(vtUnknown, 0, (uint)array.Length);

            IntPtr pvData = PropVariantNativeMethods.SafeArrayAccessData(psa);
            try // to remember to release lock on data
            {
                for (int i = 0; i < array.Length; ++i)
                {
                    object obj = array.GetValue(i);
                    IntPtr punk = (obj != null) ? Marshal.GetIUnknownForObject(obj) : IntPtr.Zero;
                    Marshal.WriteIntPtr(pvData, i * IntPtr.Size, punk);
                }
            }
            finally
            {
                PropVariantNativeMethods.SafeArrayUnaccessData(psa);
            }

            _valueType = (ushort)VarEnum.VT_ARRAY | (ushort)VarEnum.VT_UNKNOWN;
            _ptr = psa;
        }

        /// <summary>Gets or sets the variant type.</summary>
        public VarEnum VarType
        {
            get
            {
                return (VarEnum)_valueType;
            }
            set
            {
                _valueType = (ushort)value;
            }
        }

        /// <summary>Checks if this has an empty or null value</summary>
        /// <returns></returns>
        public bool IsNullOrEmpty()
        {
            return (_valueType == (ushort)VarEnum.VT_EMPTY || _valueType == (ushort)VarEnum.VT_NULL);
        }

        /// <summary>Gets the variant value.</summary>
        public object Value
        {
            get
            {
                switch ((VarEnum)_valueType)
                {
                    case VarEnum.VT_I1:
                        return _sbyte;

                    case VarEnum.VT_UI1:
                        return _byte;

                    case VarEnum.VT_I2:
                        return _short;

                    case VarEnum.VT_UI2:
                        return _ushort;

                    case VarEnum.VT_I4:
                    case VarEnum.VT_INT:
                        return _int32;

                    case VarEnum.VT_UI4:
                    case VarEnum.VT_UINT:
                        return _uint32;

                    case VarEnum.VT_I8:
                        return _long;

                    case VarEnum.VT_UI8:
                        return _ulong;

                    case VarEnum.VT_R4:
                        return _float;

                    case VarEnum.VT_R8:
                        return _double;

                    case VarEnum.VT_BOOL:
                        return _int32 == -1;

                    case VarEnum.VT_ERROR:
                        return _long;

                    case VarEnum.VT_CY:
                        return _decimal;

                    case VarEnum.VT_DATE:
                        return DateTime.FromOADate(_double);

                    case VarEnum.VT_FILETIME:
                        return DateTime.FromFileTime(_long);

                    case VarEnum.VT_BSTR:
                        return Marshal.PtrToStringBSTR(_ptr);

                    case VarEnum.VT_BLOB:
                        return GetBlobData();

                    case VarEnum.VT_LPSTR:
                        return Marshal.PtrToStringAnsi(_ptr);

                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(_ptr);

                    case VarEnum.VT_UNKNOWN:
                        return Marshal.GetObjectForIUnknown(_ptr);

                    case VarEnum.VT_DISPATCH:
                        return Marshal.GetObjectForIUnknown(_ptr);

                    case VarEnum.VT_DECIMAL:
                        return _decimal;

                    case VarEnum.VT_ARRAY | VarEnum.VT_UNKNOWN:
                        return CrackSingleDimSafeArray(_ptr);

                    case (VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR):
                        return GetVector<string>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_I2):
                        return GetVector<short>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_UI2):
                        return GetVector<ushort>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_I4):
                        return GetVector<int>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_UI4):
                        return GetVector<uint>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_I8):
                        return GetVector<long>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_UI8):
                        return GetVector<ulong>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_R4):
                        return GetVector<float>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_R8):
                        return GetVector<double>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_BOOL):
                        return GetVector<bool>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_FILETIME):
                        return GetVector<DateTime>();

                    case (VarEnum.VT_VECTOR | VarEnum.VT_DECIMAL):
                        return GetVector<decimal>();

                    default:
                        // if the value cannot be marshaled
                        return null;
                }
            }
        }

        private static long GetFileTimeAsLong(ref System.Runtime.InteropServices.ComTypes.FILETIME val)
        {
            return (((long)val.dwHighDateTime) << 32) + val.dwLowDateTime;
        }

        private static System.Runtime.InteropServices.ComTypes.FILETIME DateTimeToFileTime(DateTime value)
        {
            long hFT = value.ToFileTime();
            System.Runtime.InteropServices.ComTypes.FILETIME ft =
                new System.Runtime.InteropServices.ComTypes.FILETIME
                {
                    dwLowDateTime = (int)(hFT & 0xFFFFFFFF),
                    dwHighDateTime = (int)(hFT >> 32)
                };
            return ft;
        }

        private object GetBlobData()
        {
            byte[] blobData = new byte[_int32];

            IntPtr pBlobData = _blob.Pointer;
            Marshal.Copy(pBlobData, blobData, 0, _int32);

            return blobData;
        }

        private Array GetVector<T>()
        {
            int count = PropVariantNativeMethods.PropVariantGetElementCount(this);
            if (count <= 0) { return null; }

            lock (_padlock)
            {
                if (_vectorActions == null)
                {
                    _vectorActions = GenerateVectorActions();
                }
            }

            Action<PropVariant, Array, uint> action;
            if (!_vectorActions.TryGetValue(typeof(T), out action))
            {
                throw new InvalidCastException(LocalizedMessages.PropVariantUnsupportedType);
            }

            Array array = new T[count];
            for (uint i = 0; i < count; i++)
            {
                action(this, array, i);
            }

            return array;
        }

        private static Array CrackSingleDimSafeArray(IntPtr psa)
        {
            uint cDims = PropVariantNativeMethods.SafeArrayGetDim(psa);
            if (cDims != 1)
                throw new ArgumentException(LocalizedMessages.PropVariantMultiDimArray, "psa");

            int lBound = PropVariantNativeMethods.SafeArrayGetLBound(psa, 1U);
            int uBound = PropVariantNativeMethods.SafeArrayGetUBound(psa, 1U);

            int n = uBound - lBound + 1; // uBound is inclusive

            object[] array = new object[n];
            for (int i = lBound; i <= uBound; ++i)
            {
                array[i] = PropVariantNativeMethods.SafeArrayGetElement(psa, ref i);
            }

            return array;
        }

        /// <summary>Disposes the object, calls the clear function.</summary>
        public void Dispose()
        {
            PropVariantNativeMethods.PropVariantClear(this);

            GC.SuppressFinalize(this);
        }

        /// <summary>Finalizer</summary>
        ~PropVariant()
        {
            Dispose();
        }

        /// <summary>Provides an simple string representation of the contained data and type.</summary>
        /// <returns></returns>
        public override string ToString()
        {
           return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0}: {1}", Value, VarType.ToString());
        }
    }
}
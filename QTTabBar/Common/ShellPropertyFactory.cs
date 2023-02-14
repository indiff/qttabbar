using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace QTTabBarLib.Common
{
    /// <summary>Factory class for creating typed ShellProperties. Generates/caches expressions to create generic ShellProperties.</summary>
    internal static class ShellPropertyFactory
    {
        // Constructor cache. It takes object as the third param so a single function will suffice for both constructors.
        private static readonly Dictionary<int, Func<PropertyKey, ShellPropertyDescription, object, IShellProperty>> _storeCache
            = new Dictionary<int, Func<PropertyKey, ShellPropertyDescription, object, IShellProperty>>();

        /// <summary>Creates a generic ShellProperty.</summary>
        /// <param name="propKey">PropertyKey</param>
        /// <param name="shellObject">Shell object from which to get property</param>
        /// <returns>ShellProperty matching type of value in property.</returns>
        public static IShellProperty CreateShellProperty(PropertyKey propKey, ShellObject shellObject)
        {
            return GenericCreateShellProperty(propKey, shellObject);
        }

        /// <summary>Creates a generic ShellProperty.</summary>
        /// <param name="propKey">PropertyKey</param>
        /// <param name="store">IPropertyStore from which to get property</param>
        /// <returns>ShellProperty matching type of value in property.</returns>
        public static IShellProperty CreateShellProperty(PropertyKey propKey, IPropertyStore store)
        {
            return GenericCreateShellProperty(propKey, store);
        }

        /// <summary>Converts VarEnum to its associated .net Type.</summary>
        /// <param name="VarEnumType">VarEnum value</param>
        /// <returns>Associated .net equivelent.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static Type VarEnumToSystemType(VarEnum VarEnumType)
        {
            switch (VarEnumType)
            {
                case (VarEnum.VT_EMPTY):
                case (VarEnum.VT_NULL):
                    return typeof(object);

                case (VarEnum.VT_UI1):
                    return typeof(byte?);

                case (VarEnum.VT_I2):
                    return typeof(short?);

                case (VarEnum.VT_UI2):
                    return typeof(ushort?);

                case (VarEnum.VT_I4):
                    return typeof(int?);

                case (VarEnum.VT_UI4):
                    return typeof(uint?);

                case (VarEnum.VT_I8):
                    return typeof(long?);

                case (VarEnum.VT_UI8):
                    return typeof(ulong?);

                case (VarEnum.VT_R8):
                    return typeof(double?);

                case (VarEnum.VT_BOOL):
                    return typeof(bool?);

                case (VarEnum.VT_FILETIME):
                    return typeof(DateTime?);

                case (VarEnum.VT_CLSID):
                    return typeof(IntPtr?);

                case (VarEnum.VT_CF):
                    return typeof(IntPtr?);

                case (VarEnum.VT_BLOB):
                    return typeof(byte[]);

                case (VarEnum.VT_LPWSTR):
                    return typeof(string);

                case (VarEnum.VT_UNKNOWN):
                    return typeof(IntPtr?);

                case (VarEnum.VT_STREAM):
                    return typeof(IStream);

                case (VarEnum.VT_VECTOR | VarEnum.VT_UI1):
                    return typeof(byte[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_I2):
                    return typeof(short[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_UI2):
                    return typeof(ushort[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_I4):
                    return typeof(int[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_UI4):
                    return typeof(uint[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_I8):
                    return typeof(long[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_UI8):
                    return typeof(ulong[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_R8):
                    return typeof(double[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_BOOL):
                    return typeof(bool[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_FILETIME):
                    return typeof(DateTime[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_CLSID):
                    return typeof(IntPtr[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_CF):
                    return typeof(IntPtr[]);

                case (VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR):
                    return typeof(string[]);

                default:
                    return typeof(object);
            }
        }

        // Creates an expression for the specific constructor of the given type.
        private static Func<PropertyKey, ShellPropertyDescription, object, IShellProperty> ExpressConstructor(Type type, Type[] argTypes)
        {
            var typeHash = GetTypeHash(argTypes);

            // Finds the correct constructor by matching the hash of the types.
            var ctorInfo = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(x => typeHash == GetTypeHash(x.GetParameters().Select(a => a.ParameterType)));

            if (ctorInfo == null)
            {
                throw new ArgumentException(LocalizedMessages.ShellPropertyFactoryConstructorNotFound, "type");
            }

            var key = Expression.Parameter(argTypes[0], "propKey");
            var desc = Expression.Parameter(argTypes[1], "desc");
            var third = Expression.Parameter(typeof(object), "third"); //needs to be object to avoid casting later

            var create = Expression.New(ctorInfo, key, desc,
                Expression.Convert(third, argTypes[2]));

            return Expression.Lambda<Func<PropertyKey, ShellPropertyDescription, object, IShellProperty>>(
                create, key, desc, third).Compile();
        }

        private static IShellProperty GenericCreateShellProperty<T>(PropertyKey propKey, T thirdArg)
        {
            var thirdType = (thirdArg is ShellObject) ? typeof(ShellObject) : typeof(T);

            var propDesc = ShellPropertyDescriptionsCache.Cache.GetPropertyDescription(propKey);

            // Get the generic type
            var type = typeof(ShellProperty<>).MakeGenericType(VarEnumToSystemType(propDesc.VarEnumType));

            // The hash for the function is based off the generic type and which type (constructor) we're using.
            var hash = GetTypeHash(type, thirdType);

            Func<PropertyKey, ShellPropertyDescription, object, IShellProperty> ctor;
            if (!_storeCache.TryGetValue(hash, out ctor))
            {
                Type[] argTypes = { typeof(PropertyKey), typeof(ShellPropertyDescription), thirdType };
                ctor = ExpressConstructor(type, argTypes);
                _storeCache.Add(hash, ctor);
            }

            return ctor(propKey, propDesc, thirdArg);
        }

        private static int GetTypeHash(params Type[] types)
        {
            return GetTypeHash((IEnumerable<Type>)types);
        }

        // Creates a hash code, unique to the number and order of types.
        private static int GetTypeHash(IEnumerable<Type> types)
        {
            var hash = 0;
            foreach (var type in types)
            {
                hash = hash * 31 + type.GetHashCode();
            }
            return hash;
        }
    }
}
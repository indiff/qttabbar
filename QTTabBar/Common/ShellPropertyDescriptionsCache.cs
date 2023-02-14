//Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Collections.Generic;

namespace QTTabBarLib.Common
{
    internal class ShellPropertyDescriptionsCache
    {
        private static ShellPropertyDescriptionsCache cacheInstance;

        private readonly IDictionary<PropertyKey, ShellPropertyDescription> propsDictionary;

        private ShellPropertyDescriptionsCache()
        {
            propsDictionary = new Dictionary<PropertyKey, ShellPropertyDescription>();
        }

        public static ShellPropertyDescriptionsCache Cache
        {
            get
            {
                if (cacheInstance == null)
                {
                    cacheInstance = new ShellPropertyDescriptionsCache();
                }
                return cacheInstance;
            }
        }

        public ShellPropertyDescription GetPropertyDescription(PropertyKey key)
        {
            if (!propsDictionary.ContainsKey(key))
            {
                propsDictionary.Add(key, new ShellPropertyDescription(key));
            }
            return propsDictionary[key];
        }
    }
}
using System;
using System.Collections.Generic;

namespace Hop.Core.Base
{
    internal class TypeCache
    {
        private static readonly Dictionary<Type, TypeCacheEntry> _cache = new Dictionary<Type, TypeCacheEntry>();

        public static TypeCacheEntry Get<T>()
        {
            return Get(typeof(T));
        }

        public static TypeCacheEntry Get(Type type)
        {
            if (!_cache.ContainsKey(type))
                _cache.Add(type, new TypeCacheEntry(type));

            return _cache[type];
        }
    }
}
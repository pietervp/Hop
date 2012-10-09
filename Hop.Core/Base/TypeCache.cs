using System;
using System.Collections.Generic;

namespace Hop.Core.Base
{
    internal class TypeCache
    {
        private static readonly Dictionary<Type, TypeCacheEntry> _cache = new Dictionary<Type, TypeCacheEntry>();

        public static TypeCacheEntry Get<T>()
        {
            if (_cache.ContainsKey(typeof (T)))
                return _cache[typeof (T)];

            var typeCacheEntry = new TypeCacheEntry(typeof (T));
            _cache.Add(typeof (T), typeCacheEntry);
            return typeCacheEntry;
        }
    }
}
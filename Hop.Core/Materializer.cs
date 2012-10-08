using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Hop.Core
{
    public abstract class Materializer<T> : Materializer
    {
        private Dictionary<string, int> _cache;

        public abstract T GetObject(IDataReader reader);

        public void PrepareDataReader(IDataReader reader)
        {
            if (!TypeCache.ContainsKey(typeof (T)))
                TypeCache.Add(typeof (T), typeof (T).GetProperties().ToList());

            _cache = Enumerable
                .Range(0, reader.FieldCount)
                .ToDictionary(reader.GetName, x => x);

            foreach (PropertyInfo notInCache in TypeCache[typeof (T)].Where(x => _cache.All(y => y.Key != x.Name)))
            {
                _cache.Add(notInCache.Name, -1);
            }
        }

        public TU GetValue<TU>(IDataReader reader, string propertyName, TU currentValue)
        {
            int index = _cache[propertyName];

            if (index == -1)
                return currentValue;

            if (reader.IsDBNull(index))
                return default(TU);

            return reader.Get<TU>(index);
        }
    }

    public abstract class Materializer
    {
        public static readonly Dictionary<Type, ICollection<PropertyInfo>> TypeCache = new Dictionary<Type, ICollection<PropertyInfo>>();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Hop.Core.Base;
using Hop.Core.Services.Base;

namespace Hop.Core.Services
{
    public class IlBasedIdExtractorService : IIdExtractorService
    {
        private static readonly Dictionary<Type, IdExtractor> TypeExtractorCache = new Dictionary<Type, IdExtractor>();
        private readonly MsilGeneratorService _msilGeneratorService = new MsilGeneratorService();

        #region IIdExtractorService Members

        public IEnumerable<object> GetIds<T>(IEnumerable<T> instances)
        {
            if (!TypeExtractorCache.ContainsKey(typeof (T)))
                TypeExtractorCache.Add(typeof (T), _msilGeneratorService.CreateIdExtractor<T>());

            var extractor = (TypeExtractorCache[typeof (T)] as IdExtractor<T>);

            if (extractor != null)
            {
                foreach (T instance in instances)
                {
                    yield return extractor.GetId(instance);
                }
            }
        }

        public IEnumerable<U> GetIds<T, U>(IEnumerable<T> instances)
        {
            foreach (object instance in GetIds(instances))
            {
                yield return (U) instance;
            }
        }

        public string GetIdField<T>()
        {
            return "Id";
        }

        public T SetId<T>(T source, object id)
        {
            source.GetType().GetProperty("Id").SetValue(source, id, null);

            return source;
        }

        public object GetId<T>(T instance)
        {
            return GetIds<T, object>(new[] {instance}).FirstOrDefault();
        }

        #endregion
    }
}
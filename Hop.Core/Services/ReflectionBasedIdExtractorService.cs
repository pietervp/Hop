using System;
using System.Collections.Generic;
using System.Linq;
using Hop.Core.Base;
using Hop.Core.Services.Base;

namespace Hop.Core.Services
{
    public class ReflectionBasedIdExtractorService : IIdExtractorService
    {
        private static readonly Dictionary<Type, IdExtractor> TypeExtractorCache = new Dictionary<Type, IdExtractor>();
        private readonly MsilGeneratorService _msilGeneratorService = new MsilGeneratorService();

        public IEnumerable<object> GetIds<T>(IEnumerable<T> instances)
        {
            if (!TypeExtractorCache.ContainsKey(typeof (T)))
                TypeExtractorCache.Add(typeof (T), _msilGeneratorService.CreateIdExtractor<T>());

            var extractor = (TypeExtractorCache[typeof (T)] as IdExtractor<T>);

            if (extractor == null) 
                yield break;

            foreach (T instance in instances)
            {
                yield return extractor.GetId(instance);
            }
        }

        public IEnumerable<TU> GetIds<T, TU>(IEnumerable<T> instances)
        {
            return GetIds(instances).Select(instance => (TU) instance);
        }

        public string GetIdField<T>()
        {
            return HopBase.GetIdPropertyService(typeof(T)).Name;
        }

        public void SetId<T>(T source, object id)
        {
            HopBase.GetIdPropertyService(typeof(T)).SetValue(source, id, null);
        }

        public object GetId<T>(T instance)
        {
            return GetIds<T, object>(new[] {instance}).FirstOrDefault();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using Hop.Core.Services.Base;

namespace Hop.Core.Services
{
    public class IlBasedMaterializerService : IMaterializerService
    {
        private static readonly Dictionary<Type, Materializer> TypeMaterializerCache = new Dictionary<Type, Materializer>();

        private readonly MsilGeneratorService _msilGeneratorService = new MsilGeneratorService();

        #region IMaterializerService Members

        public IEnumerable<T> ReadObjects<T>(IDataReader dataReader) where T : new()
        {
            if (!TypeMaterializerCache.ContainsKey(typeof (T)))
                TypeMaterializerCache.Add(typeof (T), _msilGeneratorService.CreateMaterializer<T>());

            var materializer = (TypeMaterializerCache[typeof (T)] as Materializer<T>);

            if (materializer != null)
            {
                materializer.PrepareDataReader(dataReader);

                while (dataReader.Read())
                    yield return materializer.GetObject(dataReader);
            }
        }

        #endregion
    }
}
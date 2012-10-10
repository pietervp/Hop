using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hop.Core.Base;
using Hop.Core.Services.Base;

namespace Hop.Core.Services
{
    public class ReflectionBasedMaterializerService : IMaterializerService
    {
        public IEnumerable<T> ReadObjects<T>(IDataReader dataReader) where T : new()
        {
            return ReadObjects<T>(dataReader, null);
        }

        public IEnumerable<T> ReadObjects<T>(IDataReader dataReader, Task<Materializer<T>> emitterTask) where T : new()
        {
            var selectedColumnNamesInOrder = Enumerable
                .Range(0, dataReader.FieldCount)
                .Select(x => new {ColumnName = dataReader.GetName(x), OrdinalIndex = x})
                .Select(x => new {ColumName = x.ColumnName, PropertyInfo = TypeCache.Get<T>().Properties.FirstOrDefault(prop => prop.Name.ToLower() == x.ColumnName.ToLower()), OrindalIndex = x.OrdinalIndex})
                .ToList();

            while (dataReader.Read())
            {
                var newObject = new T();

                foreach (var filler in selectedColumnNamesInOrder)
                {
                    var value = dataReader.GetValue(filler.OrindalIndex);
                    filler.PropertyInfo.SetValue(newObject, value is DBNull ? HopBase.GetDefault(filler.PropertyInfo.PropertyType) : value, null);
                }

                yield return newObject;
            }
        }
    }
}
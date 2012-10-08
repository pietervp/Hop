using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hop.Core
{
    public class SubOptimalMaterializerService : IMaterializerService
    {
        private static readonly Dictionary<Type, ICollection<PropertyInfo>> TypeCache = new Dictionary<Type, ICollection<PropertyInfo>>();

        #region IMaterializerService Members

        public IEnumerable<T> ReadObjects<T>(IDataReader dataReader) where T : new()
        {
            return ReadObjects<T>(dataReader, null);
        }

        #endregion

        public IEnumerable<T> ReadObjects<T>(IDataReader dataReader, Task<Materializer<T>> emitterTask) where T : new()
        {
            if (!TypeCache.ContainsKey(typeof (T)))
                TypeCache.Add(typeof (T), typeof (T).GetProperties().ToList());

            var selectedColumnNamesInOrder = Enumerable
                .Range(0, dataReader.FieldCount)
                .Select(x => new {ColumnName = dataReader.GetName(x), OrdinalIndex = x})
                .Select(x => new {ColumName = x.ColumnName, PropertyInfo = TypeCache[typeof (T)].FirstOrDefault(prop => prop.Name.ToLower() == x.ColumnName.ToLower()), OrindalIndex = x.OrdinalIndex})
                .ToList();

            while (dataReader.Read())
            {
                var newObject = new T();

                foreach (var filler in selectedColumnNamesInOrder)
                {
                    object value = dataReader.GetValue(filler.OrindalIndex);
                    filler.PropertyInfo.SetValue(newObject, value is DBNull ? GetDefault(filler.PropertyInfo.PropertyType) : value, null);
                }

                yield return newObject;
            }
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
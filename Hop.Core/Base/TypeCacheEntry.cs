using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hop.Core.Base
{
    internal class TypeCacheEntry
    {
        private string _tableName;
        private PropertyInfo _idProperty;

        public TypeCacheEntry(Type type)
        {
            Type = type;
            Properties = type.GetProperties();
            PropertiesWithoutId = Properties.Where(p => p.Name != IdProperty.Name);
        }

        public Type Type { get; private set; }
        public IEnumerable<PropertyInfo> Properties { get; private set; }
        public IEnumerable<PropertyInfo> PropertiesWithoutId { get; private set; }

        public PropertyInfo IdProperty
        {
            get
            {
                if (_idProperty == null)
                {
                    var idProp = Properties.FirstOrDefault(x => x.GetCustomAttributes(typeof(IdAttribute), true).Any());
                    _idProperty = idProp ?? Properties.FirstOrDefault(x => x.Name == "Id");
                }
                return _idProperty;
            }
        }

        public string TableName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tableName))
                {
                    var tableNameAttribute = Type.GetCustomAttributes(typeof (TableNameAttribute), true).OfType<TableNameAttribute>().FirstOrDefault();
                    _tableName = tableNameAttribute != null ? tableNameAttribute.Name : Type.Name;
                }
                return _tableName;
            }
        }
    }
}
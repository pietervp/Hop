using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hop.Core.Base
{
    internal class TypeCacheEntry
    {
        public TypeCacheEntry(Type type)
        {
            Type = type;
            IdProperty = HopBase.GetIdPropertyService(type);
            Properties = type.GetProperties();
            PropertiesWithoutId = Properties.Where(p => p.Name != IdProperty.Name);
        }

        public Type Type { get; set; }
        public PropertyInfo IdProperty { get; set; }
        public IEnumerable<PropertyInfo> Properties { get; set; }
        public IEnumerable<PropertyInfo> PropertiesWithoutId { get; set; }
    }
}
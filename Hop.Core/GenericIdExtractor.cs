using System.Reflection;
using Hop.Core.Base;

namespace Hop.Core
{
    public class GenericIdExtractor<T> : IdExtractor<T>
    {
        public override object GetId(T instance)
        {
            return GetKeyProperty().GetValue(instance, null);
        }

        private PropertyInfo GetKeyProperty()
        {
            return TypeCache.Get<T>().IdProperty;
        }
    }
}
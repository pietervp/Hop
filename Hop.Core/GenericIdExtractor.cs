using Hop.Core.Base;

namespace Hop.Core
{
    public class GenericIdExtractor<T> : IdExtractor<T>
    {
        #region Overrides of IdExtractor<T>

        public override object GetId(T instance)
        {
            return instance.GetType().GetProperty("Id").GetValue(instance, null);
        }

        #endregion
    }
}
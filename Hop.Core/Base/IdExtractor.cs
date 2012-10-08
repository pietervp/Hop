namespace Hop.Core.Base
{
    public abstract class IdExtractor<T> : IdExtractor
    {
        public abstract object GetId(T instance);
    }

    public abstract class IdExtractor
    {
    }
}
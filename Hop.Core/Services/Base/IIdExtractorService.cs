using System.Collections.Generic;

namespace Hop.Core.Services.Base
{
    public interface IIdExtractorService
    {
        IEnumerable<object> GetIds<T>(IEnumerable<T> instances);
        IEnumerable<U> GetIds<T, U>(IEnumerable<T> instances);
        string GetIdField<T>();
        T SetId<T>(T source, object id);
    }
}
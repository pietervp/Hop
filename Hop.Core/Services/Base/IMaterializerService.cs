using System.Collections.Generic;
using System.Data;

namespace Hop.Core
{
    public interface IMaterializerService
    {
        IEnumerable<T> ReadObjects<T>(IDataReader dataReader) where T : new();
    }
}
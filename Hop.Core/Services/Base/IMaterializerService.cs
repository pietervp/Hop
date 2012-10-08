using System.Collections.Generic;
using System.Data;

namespace Hop.Core.Services.Base
{
    public interface IMaterializerService
    {
        IEnumerable<T> ReadObjects<T>(IDataReader dataReader) where T : new();
    }
}
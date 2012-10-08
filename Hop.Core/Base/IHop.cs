using System.ComponentModel;
using System.Data;

namespace Hop.Core.Base
{
    public interface IHop : IHideSig
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDbConnection Connection { get; set; }
    }
}
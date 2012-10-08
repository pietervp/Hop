using System.ComponentModel;
using System.Data;

namespace Hop.Core
{
    public interface IHop : IHideSig
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDbConnection Connection { get; set; }
    }
}
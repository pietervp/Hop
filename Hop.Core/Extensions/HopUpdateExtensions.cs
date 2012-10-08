using System.Collections.Generic;

namespace Hop.Core
{
    public static class HopUpdateExtensions
    {
        public static void UpdateSingle<T>(this IHop hopper, T instance)
        {
            int afftectedRows;
            hopper.Update(new[] {instance}, out afftectedRows);
            ;
        }

        public static void Update<T>(this IHop hopper, IEnumerable<T> instances, out int afftectedRows)
        {
            afftectedRows = 0;
        }

        public static void Update<T>(this IHop hopper, string where = "", string setClause = "")
        {
        }
    }
}
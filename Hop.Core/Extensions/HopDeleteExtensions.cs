using System.Collections.Generic;

namespace Hop.Core
{
    public static class HopDeleteExtensions
    {
        public static void DeleteSingle<T>(this IHop hopper, T instance)
        {
            int affectedRows;
            hopper.Delete(new[] {instance}, out affectedRows);
        }

        public static void Delete<T>(this IHop hopper, IEnumerable<T> instances, out int affectedRows)
        {
            affectedRows = 0;
        }

        public static void Delete<T>(this IHop hopper, string whereClause = "")
        {
        }
    }
}
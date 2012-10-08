using System;
using System.Data;
using Hop.Core.Base;

namespace Hop.Core.Extensions
{
    public static class HopExtensions
    {
        public static IHop Hop(this IDbConnection connection)
        {
            return new HopBase(connection);
        }

        #region DataReader Extensions

        public static T Get<T>(this IDataReader reader, int index)
        {
            return (T) reader.Get(typeof (T), index);
        }

        public static object Get(this IDataReader reader, Type type, int index)
        {
            if (type == typeof (int) || type == typeof (Int32))
                return reader.GetInt32(index);

            if (type == typeof (double) || type == typeof (Double))
                return reader.GetDouble(index);

            if (type == typeof (string) || type == typeof (String))
                return reader.GetString(index);

            if (type == typeof (DateTime))
                return reader.GetDateTime(index);

            return reader.GetValue(index);
        }

        #endregion
    }
}
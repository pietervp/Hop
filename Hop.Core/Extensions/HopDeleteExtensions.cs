using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Hop.Core.Base;
using Hop.Core.Services.Base;

namespace Hop.Core.Extensions
{
    public static class HopDeleteExtensions
    {
        public static void DeleteSingle<T>(this IHop hopper, T instance)
        {
            hopper.Delete(new[] {instance});
        }

        public static void Delete<T>(this IHop hopper, T instance)
        {
            Delete<T>(hopper, new[] {instance});
        }

        public static void Delete<T>(this IHop hopper, ICollection<T> instances)
        {
            if (instances == null)
                throw new ArgumentNullException("instances", "Please provide a non null value for parameter instances");

            string tableName = HopBase.GetTypeToTableNameService(typeof (T));
            IIdExtractorService idExtractorService = HopBase.GetIdExtractorService();
            string idField = idExtractorService.GetIdField<T>();
            string inClause = idExtractorService.GetIds<T, int>(instances).Select(x => x.ToString(CultureInfo.InvariantCulture)).Aggregate((id1, id2) => id1 + ", " + id2);

            Delete<T>(hopper, tableName, string.Format("{0} IN({1})", idField, inClause));
        }

        public static void Delete<T>(this IHop hopper, string tableName = "", string whereClause = "")
        {
            using (IDbCommand dbCommand = hopper.Connection.CreateCommand())
            {
                tableName = string.IsNullOrWhiteSpace(tableName) ? HopBase.GetTypeToTableNameService(typeof (T)) : tableName;

                string cmdText = string.Format("DELETE FROM {0}", tableName);

                if (!string.IsNullOrWhiteSpace(whereClause))
                    cmdText += string.Format(" WHERE {0}", whereClause);

                dbCommand.CommandText = cmdText;

                dbCommand.Connection.Open();
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    throw new HopWhereClauseParseException(dbCommand.CommandText, sqlException);
                }

                dbCommand.Connection.Close();
            }
        }
    }
}
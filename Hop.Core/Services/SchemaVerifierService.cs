using System;
using System.Data;
using System.Data.SqlClient;

namespace Hop.Core
{
    public class SchemaVerifierService
    {
        public static void AddTypeToCache<T>(IDbConnection connection) where T : new()
        {
            var sqlConnection = connection as SqlConnection;

            if (sqlConnection == null)
                return;

            var tableName = HopBase.GetTypeToTableNameService()(typeof (T));

            var sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = string.Format(@"select column_name,ordinal_position from information_schema.columns where table_name = '{0}' order by ordinal_position", tableName);

            sqlCommand.Connection.Open();

            using (var reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (true)
                {
                    connection.Hop().ReadTupleSingle<Tuple<string, int>, T>("column_name, ordinal_position", "table_name = 'Beer'", "information_schema.columns");
                }
            }
        }
    }
}
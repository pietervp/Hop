using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Hop.Core.Extensions;
using Microsoft.SqlServer.Server;

namespace Hop.Core.Services
{
    public class SchemaVerifierService
    {
        private  static List<Type> cache =new List<Type>();

        public static void AddTypeToCache<T>(IDbConnection connection) where T : new()
        {
            if (cache.Contains(typeof(T)))
                return;

            var sqlConnection = connection as SqlConnection;

            if (sqlConnection == null)
                return;

            var tableName = HopBase.GetTypeToTableNameService()(typeof (T));

            var sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = string.Format(@"select column_name,ordinal_position from information_schema.columns where table_name = '{0}' order by ordinal_position", tableName);

            sqlCommand.Connection.Open();
            var columns = connection.Hop().ReadTuples<Tuple<string, int>, T>("column_name, ordinal_position", "table_name = 'Beer'", "information_schema.columns");
            
            var sb = new StringBuilder();
            sb.Append(string.Format("ALTER TABLE  {0} ", HopBase.GetTypeToTableNameService()(typeof(T))));

            var propertyInfos = typeof (T).GetProperties().Where(x => columns.Any(y => y.Item1 == x.Name)).ToList();

            cache.Add(typeof(T));

            if (!propertyInfos.Any())
                return;

            foreach (var source in propertyInfos)
            {
                var sqlDbType = SqlMetaData.InferFromValue(Activator.CreateInstance(source.PropertyType), "test").SqlDbType;
                sb.Append(string.Format(" ADD {0} {1}", source.Name, sqlDbType.ToString()));
            }

            var dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sb.ToString();

            connection.Open();
            dbCommand.ExecuteNonQuery();
            connection.Close();
        }
    }
}
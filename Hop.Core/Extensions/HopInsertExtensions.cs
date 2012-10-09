using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Hop.Core.Base;
using Hop.Core.Services;

namespace Hop.Core.Extensions
{
    public static class HopInsertExtensions
    {
        public static void InsertSingle<T>(this IHop hopper, T instance) where T : new()
        {
            hopper.Insert(new[] {instance});
        }

        public static void Insert<T>(this IHop hopper, ICollection<T> instances) where T : new()
        {
            if (instances == null)
                throw new ArgumentNullException("instances", "Please provide a non null value to parameter instances");

            if (!instances.Any())
                return;

            SchemaVerifierService.AddTypeToCache<T>(hopper.Connection);

            IEnumerable<PropertyInfo> propertyInfos = TypeCache.Get<T>().PropertiesWithoutId;
            IEnumerable<string> objects = instances.Select(insertion => propertyInfos.Select(prop => prop.GetValue(insertion, null)).Select(x => x == null ? "NULL" : x is string ? string.Format("'{0}'", x) : x.ToString()).Aggregate((field1, field2) => field1 + ", " + field2));
            int lastId = hopper.Insert<T>(string.Format("{0} ({1})", HopBase.GetTypeToTableNameService(typeof(T)), propertyInfos.Select(x => x.Name).Aggregate((field1, field2) => field1 + ", " + field2)), objects.Select(x => "(" + x + ")").Aggregate((obj1, obj2) => obj1 + ", " + obj2));

            foreach (T source in instances.Reverse())
            {
                HopBase.GetIdExtractorService().SetId(source, lastId--);
            }
        }

        public static int Insert<T>(this IHop hopper, string intoClause = "", string insertClause = "")
        {
            using (IDbCommand dbCommand = hopper.Connection.CreateCommand())
            {
                dbCommand.CommandText = string.Format("INSERT INTO {0} VALUES {1}; SELECT @@IDENTITY AS 'Identity'", intoClause, insertClause);

                hopper.Connection.Open();

                int lastId;

                try
                {
                    lastId = (int) (decimal) dbCommand.ExecuteScalar();
                }
                catch (SqlException exception)
                {
                    throw new HopInsertClauseParseException(exception) {InsertClause = dbCommand.CommandText};
                }

                hopper.Connection.Close();

                return lastId;
            }
        }
    }
}
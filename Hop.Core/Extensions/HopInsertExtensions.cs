using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Hop.Core.Base;
using Hop.Core.PrivateExtensions;
using Hop.Core.Services;

namespace Hop.Core.Extensions
{
    public static class HopInsertExtensions
    {
        /// <summary>
        /// Insert a single entity of type T to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hopper">   </param>
        /// <param name="instance">Instance to insert to database</param>
        /// <exception cref="ArgumentNullException">thrown when instance is null</exception>
        public static void InsertSingle<T>(this IHop hopper, T instance) where T : new()
        {
            if(instance == null)
                throw new ArgumentNullException("instance", "Please provide a non null value to parameter instace ");

            hopper.Insert(new[] {instance});
        }

        /// <summary>
        /// Inserts a collection of entities to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hopper"></param>
        /// <param name="instances">Collection to insert</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Insert<T>(this IHop hopper, ICollection<T> instances) where T : new()
        {
            if (instances == null)
                throw new ArgumentNullException("instances", "Please provide a non null value to parameter instances");

            if (!instances.Any())
                return;

            SchemaVerifierService.AddTypeToCache<T>(hopper.Connection);

            var propertyInfos = TypeCache.Get<T>().PropertiesWithoutId;
            var objects = 
                instances
                .Select(objToInsert =>
                            propertyInfos
                            .Select(prop => prop.GetValue(objToInsert, null).ToSqlString())
                            .Aggregate((field1, field2) => field1 + ", " + field2)
                    );

            var tableName = HopBase.GetTypeToTableNameService(typeof (T));
            var columnList =
                propertyInfos
                .Select(x => x.Name)
                .Aggregate((field1, field2) => field1 + ", " + field2);
            
            var intoClause = string.Format("{0} ({1})", tableName, columnList);
            var valuesClause = 
                objects
                .Select(x => "(" + x + ")")
                .Aggregate((obj1, obj2) => obj1 + ", " + obj2);

            var lastId = hopper.Insert<T>(intoClause, valuesClause);

            foreach (T source in instances.Reverse())
            {
                HopBase.GetIdExtractorService().SetId(source, lastId--);
            }
        }

        /// <summary>
        /// Insert a custom query into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hopper"></param>
        /// <param name="intoClause">Tablename and optional column names to insert to</param>
        /// <param name="insertClause">Values part of the query</param>
        /// <returns>Last inserted Id</returns>
        /// <exception cref="HopInsertClauseParseException"></exception>
        public static int Insert<T>(this IHop hopper, string intoClause = "", string insertClause = "")
        {
            using (var dbCommand = hopper.Connection.CreateCommand())
            {
                intoClause = string.IsNullOrWhiteSpace(intoClause) ? HopBase.GetTypeToTableNameService(typeof(T)) : intoClause;
                dbCommand.CommandText = string.Format("INSERT INTO {0} VALUES {1}; SELECT @@IDENTITY AS 'Identity'", intoClause, insertClause);
                
                var lastId = 0;

                try
                {
                    hopper.Connection.Open();
                    lastId = (int) (decimal) dbCommand.ExecuteScalar();
                }
                catch (SqlException exception)
                {
                    throw new HopInsertClauseParseException(exception) {InsertClause = dbCommand.CommandText};
                }
                finally
                {
                    if(hopper.Connection.State == ConnectionState.Open)
                        hopper.Connection.Close();    
                }

                return lastId;
            }
        }
    }
}
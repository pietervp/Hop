using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Hop.Core.Base;
using Hop.Core.PrivateExtensions;
using Hop.Core.Services.Base;

namespace Hop.Core.Extensions
{
    public static class HopReadExtensions
    {
        public static IEnumerable<T> ReadTuples<T, TEntity>(this IHop hop, string columnSelectQuery, string whereQuery = null, string fromTable = null) where T : class, IStructuralEquatable, IStructuralComparable, IComparable where TEntity : new()
        {
            var selectFrom = SelectFrom<TEntity>(columnSelectQuery, fromTable);

            using (var dbCommand = hop.Connection.CreateCommand())
            {
                dbCommand.CommandText = selectFrom + " WHERE " + (whereQuery ?? "1=1");
                dbCommand.Connection.Open();

                var genericArguments = typeof (T).GetGenericArguments();
                var openGenericTupleCreator = 
                    typeof (Tuple)
                    .GetMethods()
                    .FirstOrDefault(x => x.GetGenericArguments().Count() == genericArguments.Count());
                
                if (openGenericTupleCreator == null) 
                    yield break;

                var tupleCreationFactory =openGenericTupleCreator.MakeGenericMethod(genericArguments);

                using (var executeReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                	var parameterStubList = Enumerable.Range(0, genericArguments.Count());
                	
                    while (executeReader.Read())
                    {
                        var factoryParameters = parameterStubList
						                            .Select(x => executeReader.Get(genericArguments[x], x))
						                            .ToArray();

                        yield return tupleCreationFactory.Invoke(null, factoryParameters) as T;
                    }
                }
            }
        }

        public static T ReadTupleSingle<T, TEntity>(this IHop hop, string columnSelectQuery, string whereQuery = null, string fromTable = null) where T : class, IStructuralEquatable, IStructuralComparable, IComparable where TEntity : new()
        {
            return ReadTuples<T, TEntity>(hop, columnSelectQuery, whereQuery, fromTable).FirstOrDefault();
        }

        public static T ReadSingle<T>(this IHop hopper, string whereClause = "") where T : new()
        {
            return hopper.Read<T>(whereClause).FirstOrDefault();
        }

        public static T ReadSingle<T>(this IHop hopper, T instance) where T : class, new()
        {
            if (instance == null)
                throw new ArgumentNullException("instance", "ReadSingle method expects an instance to read, not null.");

            return hopper.Read(new[] {instance}).FirstOrDefault();
        }

        public static IEnumerable<T> Read<T>(this IHop hopper, IEnumerable<T> instances) where T : new()
        {
            if (instances == null)
                throw new ArgumentNullException("instances", "Read extension method expects an array of instances to read");

            var listInstances = instances as List<T> ?? instances.ToList();

            if (!listInstances.Any())
                return Enumerable.Empty<T>();

            var idExtractorService = HopBase.GetIdExtractorService();

            var ids = idExtractorService.GetIds(listInstances).Select((x, i) =>
                {
                    if (HopBase.GetDefault(x.GetType()).Equals(x))
                        throw new HopReadWithoutKeyException(listInstances[i]);

                    return new SqlParameter(string.Format("@param{0}", i), x);
                }).ToArray();

            var idField = idExtractorService.GetIdField<T>();
            var whereClause = ids.Select(p => p.ParameterName).Aggregate((p1, p2) => p1 + " , " + p2);
            var selectFrom = SelectFrom<T>();

            var cmdText = string.Format("{0} WHERE {2} IN ( {1} )", selectFrom, whereClause, idField);

            using (var command = new SqlCommand())
            {
                command.CommandText = cmdText;
                command.Parameters.AddRange(ids);

                return hopper.ReadInternal<T>(command);
            }
        }

        public static IEnumerable<T> Read<T>(this IHop hopper, string whereClause) where T : new()
        {
            string cmdText = null;
            try
            {
                cmdText = string.Format("{0} WHERE {1}", SelectFrom<T>(), whereClause);
                
                using (var sqlCommand = new SqlCommand(cmdText))
                {
                    return hopper.ReadInternal<T>(sqlCommand);
                }
            }
            catch (SqlException sqlException)
            {
                throw new HopWhereClauseParseException(cmdText, sqlException);
            }
        }

        public static IEnumerable<T> ReadAll<T>(this IHop hopper) where T : new()
        {
            return hopper.ReadInternal<T>(new SqlCommand(SelectFrom<T>()));
        }

        private static IEnumerable<T> ReadInternal<T>(this IHop hopper, IDbCommand command) where T : new()
        {
            using (command)
            {
                command.Connection = hopper.Connection;
                command.Connection.Open();

                return HopBase.GetMaterializerService().ReadObjects<T>(command.ExecuteReader(CommandBehavior.CloseConnection)).ToList();
            }
        }

        private static string SelectFrom<T>(string columnsToSelect = "*", string fromTable = null) where T : new()
        {
            var tableName = HopBase.GetTypeToTableNameService(typeof (T));

            return string.Format("SELECT {1} FROM {0} ", fromTable ?? tableName, columnsToSelect);
        }
    }
}

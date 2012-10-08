using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Hop.Core.Base;
using Hop.Core.Services.Base;

namespace Hop.Core.Extensions
{
    public static class HopReadExtensions
    {
        public static IEnumerable<T> ReadTuples<T, TEntity>(this IHop hop, string columnSelectQuery, string whereQuery = null, string fromTable = null) where T : class, IStructuralEquatable, IStructuralComparable, IComparable where TEntity : new()
        {
            var selectFrom = SelectFrom<TEntity>(columnSelectQuery, fromTable);

            var dbCommand = hop.Connection.CreateCommand();
            dbCommand.CommandText = selectFrom + " WHERE " + (whereQuery ?? "1=1");
            dbCommand.Connection.Open();

            var genericArguments = typeof(T).GetGenericArguments();
            var firstOrDefault = typeof(Tuple).GetMethods().Where(x => x.GetGenericArguments().Count() == genericArguments.Count()).FirstOrDefault().MakeGenericMethod(genericArguments);

            using (var executeReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (executeReader.Read())
                {
                    yield return firstOrDefault.Invoke(null, (Enumerable.Range(0, genericArguments.Count()).Select(x => executeReader.Get(genericArguments[x], x)).ToArray())) as T;
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

            if (!instances.Any())
                return Enumerable.Empty<T>();

            IIdExtractorService idExtractorService = HopBase.GetIdExtractorService();

            IEnumerable<SqlParameter> ids = idExtractorService.GetIds(instances).Select((x, i) => new SqlParameter(string.Format("@param{0}", i), x));
            string idField = idExtractorService.GetIdField<T>();

            var command = new SqlCommand();

            string whereClause = ids.Select(p => p.ParameterName).Aggregate((p1, p2) => p1 + " , " + p2);
            string selectFrom = SelectFrom<T>();

            string cmdText = string.Format("{0} WHERE {2} IN ( {1} )", selectFrom, whereClause, idField);

            command.CommandText = cmdText;
            command.Parameters.AddRange(ids.ToArray());

            return hopper.ReadInternal<T>(command);
        }

        public static IEnumerable<T> Read<T>(this IHop hopper, string whereClause) where T : new()
        {
            string cmdText = null;
            try
            {
                cmdText = string.Format("{0} WHERE {1}", SelectFrom<T>(), whereClause);
                return hopper.ReadInternal<T>(new SqlCommand(cmdText));
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
            command.Connection = hopper.Connection;
            command.Connection.Open();

            return HopBase.GetMaterializerService().ReadObjects<T>(command.ExecuteReader(CommandBehavior.CloseConnection));
        }

        private static string SelectFrom<T>(string columnsToSelect = "*", string fromTable = null) where T : new()
        {
            var tableName = HopBase.GetTypeToTableNameService()(typeof(T));
            return string.Format("SELECT {1} FROM {0} ", fromTable ?? tableName, columnsToSelect);
        }
    }
}
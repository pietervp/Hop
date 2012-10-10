using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Hop.Core.Base;

namespace Hop.Core.Extensions
{
    public static class HopDeleteExtensions
    {
        /// <summary>
        /// Deletes a single instance in the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hopper"></param>
        /// <param name="instance">Instance to delete</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void DeleteSingle<T>(this IHop hopper, T instance)
        {
            if(instance == null)
                throw new ArgumentNullException("instance", "Please provide a non null value for parameter instance");

            hopper.Delete(new[] {instance});
        }
        
        /// <summary>
        /// Deletes a collection of instances in the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hopper"></param>
        /// <param name="instances"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Delete<T>(this IHop hopper, ICollection<T> instances)
        {
            if (instances == null)
                throw new ArgumentNullException("instances", "Please provide a non null value for parameter instances");

            var tableName = HopBase.GetTypeToTableNameService(typeof (T));
            var idExtractorService = HopBase.GetIdExtractorService();
            var idField = idExtractorService.GetIdField<T>();
            var inClause = 
                idExtractorService
                .GetIds<T, int>(instances)
                .Select(x => x.ToString(CultureInfo.InvariantCulture))
                .Aggregate((id1, id2) => id1 + ", " + id2);

            Delete<T>(hopper, tableName, string.Format("{0} IN({1})", idField, inClause));
        }
        
        /// <summary>
        /// Provides low level functionality to delete entities from a given table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hopper"></param>
        /// <param name="tableName"></param>
        /// <param name="whereClause"></param>
        /// <exception cref="HopWhereClauseParseException"></exception>
        public static void Delete<T>(this IHop hopper, string tableName = "", string whereClause = "")
        {
            tableName = string.IsNullOrWhiteSpace(tableName) ? HopBase.GetTypeToTableNameService(typeof(T)) : tableName;
            var cmdText = string.Format("DELETE FROM {0}", tableName);
           
            if (!string.IsNullOrWhiteSpace(whereClause))
                cmdText += string.Format(" WHERE {0}", whereClause);

            using (var dbCommand = hopper.Connection.CreateCommand())
            {
                dbCommand.CommandText = cmdText;
                try
                {
                    dbCommand.Connection.Open();
                    dbCommand.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    throw new HopWhereClauseParseException(dbCommand.CommandText, sqlException);
                }
                finally
                {
                    dbCommand.Connection.Close();
                }
            }
        }
    }
}
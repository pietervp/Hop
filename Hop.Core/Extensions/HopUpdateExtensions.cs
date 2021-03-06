using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Hop.Core.Base;
using Hop.Core.Services;
using Hop.Core.Services.Base;

namespace Hop.Core.Extensions
{
    public static class HopUpdateExtensions
    {
        public static void UpdateSingle<T>(this IHop hopper, T instance) where T : new()
        {
            hopper.Update(new[] {instance});
        }

        public static void Update<T>(this IHop hopper, IEnumerable<T> instances) where T : new()
        {
            SchemaVerifierService.AddTypeToCache<T>(hopper.Connection);

            IIdExtractorService idExtractorService = HopBase.GetIdExtractorService();
            List<T> instanceList = instances as List<T> ?? instances.ToList();

            var sb = new StringBuilder();
            int paramCounter = 0;

            using (var dbCommand = new SqlCommand())
            {
                dbCommand.Connection = (SqlConnection) hopper.Connection;

                foreach (T inst in instanceList)
                {
                    sb.AppendLine(string.Format("UPDATE {0} SET ", HopBase.GetTypeToTableNameService(typeof (T))));

                    sb.AppendLine(
                        TypeCache.Get<T>().PropertiesWithoutId
                            .Select((x, i) =>
                                {
                                    string paramName = string.Format("@param{0}{1}", paramCounter, i);
                                    object value = x.GetValue(inst, null);
                                    if (value == null)
                                        return string.Empty;
                                    dbCommand.Parameters.Add(new SqlParameter(paramName, value));
                                    return string.Format("{0} = {1}", x.Name, paramName);
                                })
                            .Where(x => x != string.Empty)
                            .Aggregate((set1, set2) => set1 + ", " + set2));

                    object instanceId = idExtractorService.GetId(inst);

                    if (instanceId == null || HopBase.GetDefault(instanceId.GetType()).Equals(instanceId))
                        throw new HopUpdateWithoutKeyException(inst);

                    sb.AppendLine(string.Format(" WHERE {0} = {1}", idExtractorService.GetIdField<T>(), instanceId));
                    paramCounter++;
                }

                dbCommand.CommandText = sb.ToString();
                dbCommand.Connection.Open();
                dbCommand.ExecuteNonQuery();
                dbCommand.Connection.Close();
            }
        }

        public static void Update<T>(this IHop hopper, string where = "", string setClause = "") where T : new()
        {
            SchemaVerifierService.AddTypeToCache<T>(hopper.Connection);

            using (var dbCommand = hopper.Connection.CreateCommand())
            {
                where = string.IsNullOrWhiteSpace(where) ? " 1 = 1" : where;
                dbCommand.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}", HopBase.GetTypeToTableNameService(typeof (T)), setClause, where);

                dbCommand.Connection.Open();
                dbCommand.ExecuteNonQuery();
                dbCommand.Connection.Close();
            }
        }
    }
}
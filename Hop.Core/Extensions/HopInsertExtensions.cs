using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Hop.Core
{
    public static class HopInsertExtensions
    {
        public static void InsertSingle<T>(this IHop hopper, T instance)
        {
            hopper.Insert(new[] {instance});
        }

        public static void Insert<T>(this IHop hopper, IEnumerable<T> instances)
        {
            IIdExtractorService idExtractorService = HopBase.GetIdExtractorService();

            IEnumerable<PropertyInfo> propertyInfos = typeof (T).GetProperties().Where(x => x.Name != idExtractorService.GetIdField<T>());
            IEnumerable<T> insertCollecion = instances;
            IEnumerable<string> objects = insertCollecion.Select(insertion => propertyInfos.Select(prop => prop.GetValue(insertion, null)).Select(x => x == null ? "NULL" : x is string ? string.Format("'{0}'", x) : x.ToString()).Aggregate((field1, field2) => field1 + ", " + field2));
            int lastId = hopper.Insert<T>(string.Format("{0} ({1})", typeof (T).Name, propertyInfos.Select(x => x.Name).Aggregate((field1, field2) => field1 + ", " + field2)), string.Format("{0}", objects.Aggregate((obj1, obj2) => "(" + obj1 + "), (" + obj2 + ")")));

            foreach (T source in insertCollecion.Reverse())
            {
                idExtractorService.SetId(source, lastId--);
            }
        }

        public static int Insert<T>(this IHop hopper, string intoClause = "", string insertClause = "")
        {
            using (IDbCommand dbCommand = hopper.Connection.CreateCommand())
            {
                dbCommand.CommandText = string.Format("INSERT INTO {0} VALUES {1}; SELECT @@IDENTITY AS 'Identity'", intoClause, insertClause);

                hopper.Connection.Open();

                var lastId = (int) (decimal) dbCommand.ExecuteScalar();

                hopper.Connection.Close();

                return lastId;
            }
        }
    }
}
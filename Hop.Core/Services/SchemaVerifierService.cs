using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using Hop.Core.Extensions;

namespace Hop.Core.Services
{
    public class SchemaVerifierService
    {
        private static readonly List<Type> cache = new List<Type>();

        public static void AddTypeToCache<T>(IDbConnection connection) where T : new()
        {
            if (cache.Contains(typeof (T)))
                return;

            var sqlConnection = connection as SqlConnection;

            if (sqlConnection == null)
                return;

            string tableName = HopBase.GetTypeToTableNameService()(typeof (T));

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = string.Format(@"select column_name,ordinal_position from information_schema.columns where table_name = '{0}' order by ordinal_position", tableName);

            List<Tuple<string, int>> columns = connection.Hop().ReadTuples<Tuple<string, int>, T>("column_name, ordinal_position", "table_name = 'Beer'", "information_schema.columns").ToList();

            var sb = new StringBuilder();
            sb.Append(string.Format("ALTER TABLE  {0} ", HopBase.GetTypeToTableNameService()(typeof (T))));

            List<PropertyInfo> propertyInfos = typeof (T).GetProperties().Where(x => !columns.Any(y => y.Item1 == x.Name)).ToList();

            cache.Add(typeof (T));

            if (!propertyInfos.Any())
                return;

            foreach (PropertyInfo source in propertyInfos)
            {
                SqlDbType sqlDbType = GetSqlType(source.PropertyType);
                sb.Append(string.Format(" ADD {0} {1}", source.Name, sqlDbType.ToString()));
            }

            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sb.ToString();

            connection.Open();
            dbCommand.ExecuteNonQuery();
            connection.Close();
        }

        public static SqlDbType GetSqlType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    throw new NotImplementedException();
                case TypeCode.Object:
                    if (type == typeof (byte[]))
                        return SqlDbType.VarBinary;
                    if (type == typeof (char[]))
                        return SqlDbType.NVarChar;
                    if (type == typeof (Guid))
                        return SqlDbType.UniqueIdentifier;
                    if (type == typeof (object))
                        return SqlDbType.Variant;
                    if (type == typeof (SqlBinary))
                        return SqlDbType.VarBinary;
                    if (type == typeof (SqlBoolean))
                        return SqlDbType.Bit;
                    if (type == typeof (SqlByte))
                        return SqlDbType.TinyInt;
                    if (type == typeof (SqlDateTime))
                        return SqlDbType.DateTime;
                    if (type == typeof (SqlDouble))
                        return SqlDbType.Float;
                    if (type == typeof (SqlGuid))
                        return SqlDbType.UniqueIdentifier;
                    if (type == typeof (SqlInt16))
                        return SqlDbType.SmallInt;
                    if (type == typeof (SqlInt32))
                        return SqlDbType.Int;
                    if (type == typeof (SqlInt64))
                        return SqlDbType.BigInt;
                    if (type == typeof (SqlMoney))
                        return SqlDbType.Money;
                    if (type == typeof (SqlDecimal))
                        return SqlDbType.Decimal;
                    if (type == typeof (SqlSingle))
                        return SqlDbType.Real;
                    if (type == typeof (SqlString))
                        return SqlDbType.NVarChar;
                    if (type == typeof (SqlChars))
                        return SqlDbType.NVarChar;
                    if (type == typeof (SqlBytes))
                        return SqlDbType.VarBinary;
                    if (type == typeof (SqlXml))
                        return SqlDbType.Xml;
                    if (type == typeof (TimeSpan))
                        return SqlDbType.Time;
                    if (type == typeof (DateTimeOffset))
                        return SqlDbType.DateTimeOffset;

                    throw new NotImplementedException();
                case TypeCode.DBNull:
                    throw new NotImplementedException();
                case TypeCode.Boolean:
                    return SqlDbType.Bit;
                case TypeCode.Char:
                    return SqlDbType.NVarChar;
                case TypeCode.SByte:
                    throw new NotImplementedException();
                case TypeCode.Byte:
                    return SqlDbType.TinyInt;
                case TypeCode.Int16:
                    return SqlDbType.SmallInt;
                case TypeCode.UInt16:
                    throw new NotImplementedException();
                case TypeCode.Int32:
                    return SqlDbType.Int;
                case TypeCode.UInt32:
                    throw new NotImplementedException();
                case TypeCode.Int64:
                    return SqlDbType.BigInt;
                case TypeCode.UInt64:
                    throw new NotImplementedException();
                case TypeCode.Single:
                    return SqlDbType.Real;
                case TypeCode.Double:
                    return SqlDbType.Float;
                case TypeCode.Decimal:
                    return SqlDbType.Decimal;
                case TypeCode.DateTime:
                    return SqlDbType.DateTime;
                case TypeCode.String:
                    return SqlDbType.NVarChar;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
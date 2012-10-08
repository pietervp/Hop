using System;
using System.Data.SqlClient;

namespace Hop.Core.Base
{
    public class HopWhereClauseParseException : Exception
    {
        public HopWhereClauseParseException(string whereClause, SqlException sqlException)
        {
            WhereClause = whereClause;
            SqlException = sqlException;
        }

        public string WhereClause { get; set; }
        public SqlException SqlException { get; set; }
    }
}
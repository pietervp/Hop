using System;
using System.Data.SqlClient;

namespace Hop.Core.Base
{
    public class HopInsertClauseParseException : Exception
    {
        public HopInsertClauseParseException(SqlException exception)
        {
            SqlException = exception;
        }

        public SqlException SqlException { get; set; }
        public string InsertClause { get; set; }
    }
}
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    public class BaseHopTest
    {
        [TestInitialize]
        public void WarmupSqlConnectionPool()
        {
            var sqlConnection = GetSqlConnection();
            sqlConnection.Open();
            sqlConnection.Close(); 
        }

        protected virtual SqlConnection GetSqlConnection()
        {
            var sqlConnection = new SqlConnection(GetConnectionString());
            return sqlConnection;
        }

        protected virtual string GetConnectionString()
        {
            return "Data source=SPHINXW7PVPS2; Initial Catalog=DemoDb; User=sa;Password=pyrAmid09";
        }
    }
}
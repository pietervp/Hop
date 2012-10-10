using System.Linq;
using Hop.Core.Base;
using Hop.Core.Extensions;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopTests : BaseHopTest
    {
        [TestInitialize]
        public void InitializeTest()
        {
            using (var sqlCommand = GetSqlConnection().CreateCommand())
            {
                sqlCommand.CommandText = @"IF OBJECT_ID('dbo.Soda', 'U') IS NOT NULL
                                                                                  DROP TABLE dbo.Soda";

                sqlCommand.Connection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Connection.Close();
            }
        }

        [TestMethod]
        public void SchemaUpdatedWhenUnknownEntityIsAdded()
        {
            GetSqlConnection().Hop().InsertSingle(new Soda(){Name = "Soda1"});
        }

        [TestMethod]
        public void DbConnectionCanBeReusedAfterBeingClosed()
        {
            var sqlConnection = GetSqlConnection();
            var hop = sqlConnection.Hop();

            hop.Insert(new[] { new Beer() });
            hop.Delete<Beer>(whereClause: "Name IS NULL");
            var readAll = hop.ReadAll<Beer>();
            readAll = hop.ReadAll<Beer>();

            Assert.IsNotNull(readAll);
        }
    }

    public class Soda
    {
        public int Id { get; set; }
        [Id]
        public int IdPropWasAFake { get; set; }
        public string Name { get; set; }
    }
}
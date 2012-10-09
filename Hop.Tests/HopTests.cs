using System.Linq;
using Hop.Core.Extensions;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopTests : BaseHopTest
    {
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
}
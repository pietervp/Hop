using System.Linq;
using Hop.Core.Extensions;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopDeleteTests : BaseHopTest
    {
        [TestInitialize]
        public void BeforeTest()
        {
            var newBeers = new[] { new Beer { Id = 1, Name = "One" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" } };

            GetSqlConnection().Hop().Insert(newBeers);
        }

        [TestCleanup]
        public void AfterTest()
        {
            var newBeers = new[] { new Beer { Id = 1, Name = "One" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" }, new Beer() { Name = "Two" } };

            GetSqlConnection().Hop().Insert(newBeers);
        }

        [TestMethod]
        public void DeleteAllRecordsShouldClearATable()
        {
            GetSqlConnection().Hop().Delete<Beer>();

            var beers = GetSqlConnection().Hop().ReadAll<Beer>();

            Assert.IsNotNull(beers);
            Assert.AreEqual(0, beers.Count());
        }    
    }
}
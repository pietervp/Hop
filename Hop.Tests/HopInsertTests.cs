using System.Linq;
using Hop.Core;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopInsertTests : BaseHopTest
    {
        [TestMethod]
        public void InsertMultipleInstancesShouldResultInCorrectIdAllocation()
        {
            var newBeers = new[] { new Beer { Id = 1, Name = "One" }, new Beer() { Name = "Two" } };

            GetSqlConnection().Hop().Insert(newBeers);

            var lastTwoInsertedBeers = GetSqlConnection().Hop().Read<Beer>("Name = One ORDER BY ID Desc").Take(2);

            Assert.AreEqual(lastTwoInsertedBeers.Count(), 2);
            Assert.IsNotNull(lastTwoInsertedBeers.FirstOrDefault());
            Assert.IsNotNull(lastTwoInsertedBeers.Skip(1).FirstOrDefault());
            Assert.AreNotEqual(lastTwoInsertedBeers.FirstOrDefault().Id, 0);
        }

        [TestMethod]
        public void TestHopInsertSingleExtensionMethod()
        {
            var newBeer = new Beer() { Id = int.MaxValue, Name = "PieterOne" };

            GetSqlConnection().Hop().InsertSingle(newBeer);

            Assert.IsNotNull(newBeer);
            Assert.AreNotEqual(int.MaxValue, newBeer.Id);
            Assert.AreNotEqual(0, newBeer.Id);
            Assert.IsNotNull(newBeer.Name);
            Assert.IsNull(newBeer.Name1);
            Assert.AreEqual(newBeer.Name, "PieterOne");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Hop.Core.Base;
using Hop.Core.Extensions;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopUpdateTests : BaseHopTest
    {
        public ICollection<Beer> Beers { get; set; }

        [TestInitialize]
        public void BeforeTest()
        {
            GetSqlConnection().Hop().Delete<Beer>(whereClause: "Name = '11-1'");
            GetSqlConnection().Hop().Insert(new[]{new Beer(){Name = "11"},new Beer(){Name = "11"},new Beer(){Name = "11"},new Beer(){Name = "11"},new Beer(){Name = "11"},new Beer(){Name = "11"}});
            Beers = GetSqlConnection().Hop().ReadAll<Beer>().Where(x => x.Name == "11").ToList();
        }

        [TestCleanup]
        public void AfterTest()
        {
            GetSqlConnection().Hop().Delete(Beers);
        }

        [ExpectedException(typeof(HopUpdateWithoutKeyException))]
        [TestMethod]
        public void UpdateOfObjectWithoutValidKeyShouldThrow()
        {
            GetSqlConnection().Hop().UpdateSingle(new Beer(){Name = "NewName"});
        }

        [TestMethod]
        public void UpdateMultipleEntitiesThenReadAndUpdatesSucces()
        {
            var random = new Random();

            foreach (var beer in Beers)
            {
                beer.Name = random.Next(0, 100) + " Multiple ";
            }

            GetSqlConnection().Hop().Update(Beers);

            Beers = GetSqlConnection().Hop().Read(Beers).ToList();

            Assert.IsNotNull(Beers);
            Assert.IsNotNull(Beers.FirstOrDefault());
            Assert.IsFalse(Beers.Any(x=> x.Name == "11"));
            Assert.IsTrue(Beers.FirstOrDefault().Name.Contains("Multiple"));
        }

        [TestMethod]
        public void UpdateSingleEntityReturnsUpdatedEntity()
        {
            var beer = Beers.FirstOrDefault();
            beer.Name = "11-1";

            GetSqlConnection().Hop().UpdateSingle(beer);

            var readSingle = GetSqlConnection().Hop().ReadSingle<Beer>("Name = '11-1'");

            Assert.IsNotNull(beer);
            Assert.IsNotNull(readSingle);
            Assert.AreEqual(beer.Id, readSingle.Id);
            Assert.AreEqual(beer.Name, readSingle.Name);
        }
    }
}
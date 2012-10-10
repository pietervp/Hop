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

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void DeleteNullObjectShouldThrowArgumentException()
        {
            GetSqlConnection().Hop().Delete<Beer>(null as ICollection<Beer>);
        }

        [ExpectedException(typeof(HopWhereClauseParseException))]
        [TestMethod]
        public void DeleteUsingFaultyWhereClauseShouldThrow()
        {
            GetSqlConnection().Hop().Delete<Beer>(whereClause: "UnknownField = 1");
        }

        [TestMethod]
        public void DeleteUsingFaultyWhereClauseShouldThrowAndGiveSufficientInfo()
        {
            HopWhereClauseParseException hopException = null;
            try
            {
                GetSqlConnection().Hop().Delete<Beer>(whereClause: "UnknownField = 1");    
            }
            catch(HopWhereClauseParseException ex)
            {
                hopException = ex;
            }

            Assert.IsNotNull(hopException);
            Assert.IsNotNull(hopException.SqlException);
            Assert.IsNotNull(hopException.WhereClause);
            Assert.IsFalse(string.IsNullOrWhiteSpace(hopException.WhereClause));
        }

        [TestMethod]
        public void DeleteUsingWhereClauseShouldWork()
        {
            GetSqlConnection().Hop().Insert(new []{new Beer(){Name = "WhereClauseTest"}});

            var whereClause = "Name = 'WhereClauseTest'";
            GetSqlConnection().Hop().Delete<Beer>(whereClause: whereClause);

            var enumerable = GetSqlConnection().Hop().Read<Beer>(whereClause);

            Assert.IsNotNull(enumerable);
            Assert.AreEqual(0, enumerable.Count());
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void DeleteSingleWithNullParameterShouldThrow()
        {
            GetSqlConnection().Hop().DeleteSingle<Beer>(null);
        }

        [TestMethod]
        public void DeleteSingleObject()
        {
            BeforeTest();

            var firstOrDefault = GetSqlConnection().Hop().ReadAll<Beer>().FirstOrDefault();

            GetSqlConnection().Hop().DeleteSingle(firstOrDefault);

            var beerFoundAfterDelete = GetSqlConnection().Hop().ReadSingle<Beer>(string.Format("Id = {0}", firstOrDefault.Id));

            Assert.IsNull(beerFoundAfterDelete);
        }
    }
}
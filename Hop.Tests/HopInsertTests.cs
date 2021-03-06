using System;
using System.Linq;
using Hop.Core;
using Hop.Core.Base;
using Hop.Core.Extensions;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopInsertTests : BaseHopTest
    {
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void InsertNullReferenceShouldThrowException()
        {
            GetSqlConnection().Hop().Insert<Beer>(null);
        }

        [TestMethod]
        public void InsertEmptyArrayShouldNotReportIssues()
        {
            GetSqlConnection().Hop().Insert(Enumerable.Empty<Beer>().ToList());
        }

        [ExpectedException(typeof(HopInsertClauseParseException))]
        [TestMethod]
        public void InsertWithFaultySqlStatementShouldResultInException()
        {
            GetSqlConnection().Hop().Insert<Beer>(insertClause: "Hello World");
        }

        [TestMethod]
        public void InsertWithFaultySqlStatementShouldProvideEnoughErrorInfo()
        {
            HopInsertClauseParseException exception = null;

            try
            {
                GetSqlConnection().Hop().Insert<Beer>(insertClause: "Hello World");
            }
            catch (HopInsertClauseParseException ex)
            {
                exception = ex;   
            }

            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.SqlException);
            Assert.IsNotNull(exception.InsertClause);
            Assert.AreNotEqual(string.Empty, exception.InsertClause);
        }

        [TestMethod]
        public void InsertMultipleInstancesShouldResultInCorrectIdAllocation()
        {
            var newBeers = new[] { new Beer { Id = 1, Name = "One" }, new Beer() { Name = "Two" } };

            GetSqlConnection().Hop().Insert(newBeers);

            var lastTwoInsertedBeers = GetSqlConnection().Hop().Read<Beer>("Name = 'One' ORDER BY ID Desc").Take(2).ToList();

            Assert.AreEqual(lastTwoInsertedBeers.Count(), 2);
            Assert.IsNotNull(lastTwoInsertedBeers.FirstOrDefault());
            Assert.IsNotNull(lastTwoInsertedBeers.Skip(1).FirstOrDefault());
            Assert.AreNotEqual(lastTwoInsertedBeers.FirstOrDefault().Id, 0);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void InsertSingleWithNullValueShouldThrowException()
        {
            GetSqlConnection().Hop().InsertSingle<Beer>(null);
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
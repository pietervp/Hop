using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Hop.Core;
using Hop.Core.Base;
using Hop.Core.Extensions;
using Hop.Core.Services;
using Hop.Core.Services.Base;
using Hop.TestConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hop.Tests
{
    [TestClass]
    public class HopReadTests : BaseHopTest
    {
        [TestMethod]
        public void ReadMultipleTupleWithTopClause()
        {
            var readTuple = GetSqlConnection().Hop().ReadTuples<Tuple<int, string>, Beer>("TOP(2) Id, Name");

            Assert.IsNotNull(readTuple);
            Assert.AreEqual(2, readTuple.Count());

            Assert.IsNotNull(readTuple.FirstOrDefault());
            Assert.IsNotNull(readTuple.FirstOrDefault().Item1, readTuple.FirstOrDefault().Item2);
        }
           
        [TestMethod]
        public void ReadMultipleTupleUsingCustomTable()
        {
            var beerSchema = GetSqlConnection().Hop().ReadTuples<Tuple<string, int>, Beer>("column_name, ordinal_position", "table_name = 'Beer'", "information_schema.columns");

            Assert.IsNotNull(beerSchema);
            Assert.AreNotEqual(0, beerSchema.Count());
            Assert.AreEqual("Id", beerSchema.FirstOrDefault().Item1);
        }

        [TestMethod]
        public void ReadMultipleTuple()
        {
            var readTuple = GetSqlConnection().Hop().ReadTuples<Tuple<int, string>, Beer>("Id, Name");

            Assert.IsNotNull(readTuple);
            Assert.IsTrue(readTuple.Any());

            Assert.IsNotNull(readTuple.FirstOrDefault());
            Assert.IsNotNull(readTuple.FirstOrDefault().Item1, readTuple.FirstOrDefault().Item2);
        }

        [TestMethod]
        public void ReadSingleTupleShouldWork()
        {
            var readTuple = GetSqlConnection().Hop().ReadTupleSingle<Tuple<int, string>, Beer>("Id, Name");

            Assert.IsNotNull(readTuple);
            Assert.IsNotNull(readTuple.Item1, readTuple.Item2);
        }     
        
        [TestMethod]
        public void ReadSingleTupleWithWhereClauseShouldWork()
        {
            var readTuple = GetSqlConnection().Hop().ReadTupleSingle<Tuple<int, string>, Beer>("Id, Name", "Id = 1");

            Assert.IsNotNull(readTuple);
            Assert.IsNotNull(readTuple.Item1, readTuple.Item2);
            Assert.AreEqual(1, readTuple.Item1);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void ReadSingleWithNullParameterShouldResultInArgumentException()
        {
            GetSqlConnection().Hop().ReadSingle(null as Beer);
        }

        [ExpectedException(typeof(HopWhereClauseParseException))]
        [TestMethod]
        public void ReadSingleWithFaultyWhereClauseShouldThrow()
        {
            GetSqlConnection().Hop().Read<Beer>("SomeCrazyNonExistingProperty = 1");
        }

        [TestMethod]
        public void ReadSingleWithFaultyWhereClauseShouldThrowAndExceptionShouldContainMoreInfo()
        {
            HopWhereClauseParseException exception = null;
            const string whereClause = "SomeCrazyNonExistingProperty = 1";

            try
            {
                GetSqlConnection().Hop().Read<Beer>(whereClause);
            }
            catch (HopWhereClauseParseException ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.IsNotNull(exception.SqlException);
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.WhereClause));
        }

        [TestMethod]
        public void ReadWithEmptyArrayOfTShouldReturnEmptyArray()
        {
            var beers = GetSqlConnection().Hop().Read(Enumerable.Empty<Beer>());

            Assert.IsNotNull(beers);
            Assert.IsFalse(beers.Any());
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void ReadWithNullParameterShouldResultInArgumentException()
        {
            GetSqlConnection().Hop().Read(null as IEnumerable<Beer>);
        }
        
        [TestMethod]
        public void ReadAllShouldNeverResultInNullReference()
        {
            var readAll = GetSqlConnection().Hop().ReadAll<Beer>();

            Assert.IsNotNull(readAll);
        }

        [TestMethod]
        public void MaterializeObjectsUsingReflectionOnlyMaterializer()
        {
            for (var i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();

                var readObjects = Materialize<Beer>(new SubOptimalMaterializerService());

                Debug.WriteLine(startNew.ElapsedMilliseconds);

                Assert.IsNotNull(readObjects);
                Assert.IsNotNull(readObjects.FirstOrDefault());
                Assert.AreNotEqual(readObjects.FirstOrDefault().Id, 0);
                Assert.IsTrue(readObjects.Any());
            }
        }
        
        [TestMethod]
        public void MaterializeObjectsUsingMSILGeneratorMaterializer()
        {
            for (int i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();
                var readObjects = Materialize<Beer>(new IlBasedMaterializerService());

                Debug.WriteLine(startNew.ElapsedMilliseconds);

                Assert.IsNotNull(readObjects);
                Assert.IsNotNull(readObjects.FirstOrDefault());
                Assert.AreNotEqual(readObjects.FirstOrDefault().Id, 0);
                Assert.IsTrue(readObjects.Any());
            }
        }

        private IEnumerable<T> Materialize<T>(IMaterializerService materializerService) where T : new()
        {
            IDbCommand dbCommand = GetSqlConnection().CreateCommand();
            dbCommand.CommandText = "SELECT * FROM Beer";
            dbCommand.Connection.Open();

            List<T> readObjects;
            using (var executeReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
            {
                readObjects = materializerService.ReadObjects<T>(executeReader).ToList();
            }

            return readObjects;
        }

        [TestMethod]
        public void ReadAllPerformanceTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();
                var readAll = GetSqlConnection().Hop().ReadAll<Beer>().ToList();

                Debug.WriteLine(startNew.ElapsedMilliseconds);

                if(i>0)
                    Assert.IsTrue(startNew.ElapsedMilliseconds < 20);

                Assert.IsNotNull(readAll);
                Assert.IsTrue(readAll.Any());
                Assert.IsNotNull(readAll.FirstOrDefault());
                Assert.IsNotNull(readAll.FirstOrDefault().Id);
                Assert.AreNotEqual(readAll.FirstOrDefault().Id, 0);
            }
        }      

        [TestMethod]
        public void TestHopReadAsRefreshExtensionMethod()
        {
            for (int i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();
                var readAll = GetSqlConnection().Hop().ReadAll<Beer>().ToList();
                var countBefore = readAll.Count;

                readAll = GetSqlConnection().Hop().Read(readAll).ToList();
                Debug.WriteLine(startNew.ElapsedMilliseconds);

                Assert.IsNotNull(readAll);
                Assert.IsTrue(readAll.Any());
                Assert.IsNotNull(readAll.FirstOrDefault());
                Assert.AreEqual(countBefore, readAll.Count);
                Assert.IsNotNull(readAll.FirstOrDefault().Id);
                Assert.AreNotEqual(readAll.FirstOrDefault().Id, 0);
            }
        }
        
        [TestMethod]
        public void TestHopReadAsRefreshSingleExtensionMethod()
        {
            for (int i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();
                var beer = GetSqlConnection().Hop().ReadSingle(new Beer() { Id = 15 });
                Debug.WriteLine(startNew.ElapsedMilliseconds);

                Assert.IsNotNull(beer);
                Assert.IsNotNull(beer.Id);
                Assert.IsNotNull(beer.Name);
                Assert.AreEqual(beer.Id, 15);
            }
        }
        [TestMethod]
        public void TestHopReadSingleExtensionMethod()
        {
            for (int i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();
                var beer = GetSqlConnection().Hop().ReadSingle<Beer>("Id = 15");
                Debug.WriteLine(startNew.ElapsedMilliseconds);

                Assert.IsNotNull(beer);
                Assert.IsNotNull(beer.Id);
                Assert.AreEqual(beer.Id, 15);
            }
        }

        [TestMethod]
        public void TestSqlParameters()
        {
            var dbCommand = new SqlCommand {CommandText = "SELECT * FROM tbl WHERE Name = @param1"};
            dbCommand.Parameters.AddWithValue("@param1", "test");
            
            Assert.AreEqual(ConvertCommandParamatersToLiteralValues(dbCommand), "SELECT * FROM tbl WHERE Name = 'test'");
        }

        private static string ConvertCommandParamatersToLiteralValues(SqlCommand cmd)
        {
            string query = cmd.CommandText;
            foreach (SqlParameter prm in cmd.Parameters)
            {
                switch (prm.SqlDbType)
                {
                    case SqlDbType.Bit:
                        int boolToInt = (bool)prm.Value ? 1 : 0;
                        query = query.Replace(prm.ParameterName, string.Format("{0}", (bool)prm.Value ? 1 : 0));
                        break;
                    case SqlDbType.Int:
                        query = query.Replace(prm.ParameterName, string.Format("{0}", prm.Value));
                        break;
                    case SqlDbType.VarChar:
                        query = query.Replace(prm.ParameterName, string.Format("'{0}'", prm.Value));
                        break;
                    default:
                        query = query.Replace(prm.ParameterName, string.Format("'{0}'", prm.Value));
                        break;
                }
            }
            return query;
        }
    }
}

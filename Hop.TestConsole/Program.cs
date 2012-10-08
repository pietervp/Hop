using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Hop.Core;
using Hop.Core.Base;
using Hop.Core.Services;

namespace Hop.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                var startNew = Stopwatch.StartNew();

                IDbCommand dbCommand = GetSqlConnection().CreateCommand();
                dbCommand.CommandText = "SELECT * FROM Beer";
                dbCommand.Connection.Open();

                IEnumerable<Beer> readObjects;
                using (var executeReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    readObjects = new IlBasedMaterializerService().ReadObjects<Beer>(executeReader).ToList();
                }
                
                Console.WriteLine(startNew.ElapsedMilliseconds);
            }
        }

        private static SqlConnection GetSqlConnection()
        {
            var sqlConnection = new SqlConnection(GetConnectionString());
            return sqlConnection;
        }

        private static string GetConnectionString()
        {
            return "Data source=SPHINXW7PVPS2; Initial Catalog=DemoDb; User=sa;Password=pyrAmid09";
        }
    }
    public class Beer
    {
        public string Name { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }
        public string Name6 { get; set; }
        public string Name7 { get; set; }
        public string Name8 { get; set; }
        public string Name9 { get; set; }
        public string Name10 { get; set; }
        public string Name11 { get; set; }
        public string Name12 { get; set; }
        public string Name13 { get; set; }
        public string Name14 { get; set; }
        public string Name15 { get; set; }
        public string Name16 { get; set; }
        public string Name17 { get; set; }
        public string Name18 { get; set; }
        public int Id { get; set; }
    }


    public class BeerMat : Materializer<Beer>
    {
        #region Overrides of Materializer<Beer>

        public override Beer GetObject(IDataReader reader)
        {
            PrepareDataReader(reader);

            var beer = new Beer();

            beer.Name = (string)(GetValue(reader, "Name", beer.Name));
            beer.Id = (int)(GetValue(reader, "Id", beer.Id));

            return beer;
        }

        #endregion
    }


    public class BeerIdExtractor : IdExtractor<Beer>
    {
        #region Overrides of IdExtractor<Beer>

        public override object GetId(Beer instance)
        {
            return instance.Id;
        }

        #endregion
    }
}

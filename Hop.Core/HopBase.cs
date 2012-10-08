using System;
using System.Data;

namespace Hop.Core
{
    public class HopBase : IHop
    {
        public static Func<IGeneratorService> GetGeneratorService;
        public static Func<IIdExtractorService> GetIdExtractorService;
        public static Func<IMaterializerService> GetMaterializerService;
        public static Func<Func<Type, string>> GetTypeToTableNameService;

        static HopBase()
        {
            GetGeneratorService = () => new MsilGeneratorService();
            GetIdExtractorService = () => new IlBasedIdExtractorService();
            GetMaterializerService = () => new IlBasedMaterializerService();

            //define default services
            GetTypeToTableNameService = () => type => type.Name;
        }

        public HopBase(IDbConnection connection)
        {
            Connection = connection;
        }

        #region IHop Members

        public IDbConnection Connection { get; set; }

        #endregion
    }
}
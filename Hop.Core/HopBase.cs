using System;
using System.Data;
using System.Reflection;
using Hop.Core.Base;
using Hop.Core.Services;
using Hop.Core.Services.Base;

namespace Hop.Core
{
    public class HopBase : IHop
    {
        public static Func<IGeneratorService> GetGeneratorService;
        public static Func<IIdExtractorService> GetIdExtractorService;
        public static Func<IMaterializerService> GetMaterializerService;
        public static Func<Func<Type, string>> GetTypeToTableNameService;

        public static Func<Type, PropertyInfo> GetIdPropertyService;

        static HopBase()
        {
            GetGeneratorService = () => new MsilGeneratorService();
            GetIdExtractorService = () => new IlBasedIdExtractorService();
            GetMaterializerService = () => new IlBasedMaterializerService();

            //define default services
            GetTypeToTableNameService = () => type => type.Name;
            GetIdPropertyService = type => type.GetProperty("Id");
        }

        public HopBase(IDbConnection connection)
        {
            Connection = connection;
        }

        #region IHop Members

        public IDbConnection Connection { get; set; }

        #endregion

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
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
        public static Func<Type, string> GetTypeToTableNameService;
        public static Func<Type, PropertyInfo> GetIdPropertyService;

        static HopBase()
        {
            GetGeneratorService = () => new MsilGeneratorService();
            GetIdExtractorService = () => new ReflectionBasedIdExtractorService();
            GetMaterializerService = () => new IlBasedMaterializerService();

            //define default services
            GetTypeToTableNameService = type => TypeCache.Get(type).TableName;
            GetIdPropertyService = type => TypeCache.Get(type).IdProperty;
        }

        public HopBase(IDbConnection connection)
        {
            Connection = connection;
        }

        public IDbConnection Connection { get; set; }
        
        public static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
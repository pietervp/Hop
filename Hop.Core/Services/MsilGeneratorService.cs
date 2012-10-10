using System;
using System.Reflection;
using System.Reflection.Emit;
using BLToolkit.Reflection.Emit;
using Hop.Core.Base;
using Hop.Core.Services.Base;

namespace Hop.Core.Services
{
    public class MsilGeneratorService : IGeneratorService
    {
        private static readonly AssemblyBuilderHelper AssemblyBuilder = new AssemblyBuilderHelper("temp.generated.dll");

        public IdExtractor<T> CreateIdExtractor<T>()
        {
            return new GenericIdExtractor<T>();
        }

        public Materializer<T> CreateMaterializer<T>()
        {
            var baseType = typeof (Materializer<T>);
            var localType =TypeCache.Get<T>();
            var emptyArgsArray = new Type[] {};

            var typeBuilderHelper = AssemblyBuilder.DefineType(localType.Type.Name + "Materializer", baseType);

            //constructor
            typeBuilderHelper.DefaultConstructor.Emitter.ldarg_0.call(baseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, emptyArgsArray, null)).ret();

            //getobject impl
            var defineMethod = typeBuilderHelper.DefineMethod(baseType.GetMethod("GetObject"));

            var emitter = defineMethod.Emitter;
            var returnVar = emitter.DeclareLocal(localType.Type);

            //create new T
            emitter
                .newobj(localType.Type.GetConstructor(emptyArgsArray))
                .stloc(returnVar);

            foreach (var propertyInfo in localType.Properties)
            {
                emitter =
                    emitter
                        .ldloc_0
                        .ldarg_0
                        .ldarg_1
                        .ldstr(propertyInfo.Name)
                        .ldloc_0
                        .callvirt(propertyInfo.GetGetMethod())
                        .call(baseType.GetMethod("GetValue").MakeGenericMethod(propertyInfo.PropertyType))
                        .callvirt(propertyInfo.GetSetMethod())
                        .nop;
            }

            emitter
                .ldloc_0
                .ret();

            var type = typeBuilderHelper.Create();

            AssemblyBuilder.Save();

            return (Materializer<T>) Activator.CreateInstance(type);
        }
    }
}
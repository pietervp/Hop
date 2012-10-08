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

        #region IGeneratorService Members

        public Materializer<T> CreateMaterializer<T>()
        {
            Type baseType = typeof (Materializer<T>);
            Type localType = typeof (T);
            var emptyArgsArray = new Type[] {};

            TypeBuilderHelper typeBuilderHelper = AssemblyBuilder.DefineType(localType.Name + "Materializer", baseType);

            //constructor
            typeBuilderHelper.DefaultConstructor.Emitter.ldarg_0.call(baseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, emptyArgsArray, null)).ret();

            //getobject impl
            MethodBuilderHelper defineMethod = typeBuilderHelper.DefineMethod(baseType.GetMethod("GetObject"));

            EmitHelper emitter = defineMethod.Emitter;
            LocalBuilder returnVar = emitter.DeclareLocal(localType);

            //create new T
            emitter
                .newobj(localType.GetConstructor(emptyArgsArray))
                .stloc(returnVar);

            foreach (PropertyInfo propertyInfo in localType.GetProperties())
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

            Type type = typeBuilderHelper.Create();

            AssemblyBuilder.Save();

            return (Materializer<T>) Activator.CreateInstance(type);
        }

        #endregion

        public IdExtractor<T> CreateIdExtractor<T>()
        {
            return new GenericIdExtractor<T>();
        }
    }
}
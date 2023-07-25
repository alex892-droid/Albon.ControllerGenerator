using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    internal class DTOConstructor
    {
        private INamingConvention NamingConvention { get; set; }

        public DTOConstructor(INamingConvention namingConvention)
        {
            NamingConvention = NamingConvention;
        }

        public Type CreateDTO(MethodInfo methodInfo, ModuleBuilder moduleBuilder)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(NamingConvention.GetDTOName(methodInfo.Name), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

            var parameters = methodInfo.GetParameters();

            foreach (var parameter in parameters)
            {
                // Create a new field in the target type
                FieldBuilder fieldBuilder = typeBuilder.DefineField(parameter.Name, parameter.ParameterType, FieldAttributes.Public);

                // Define the property with appropriate getter and setter methods
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(parameter.Name, PropertyAttributes.None, parameter.ParameterType, new Type[] { parameter.ParameterType });

                // Define the getter method for the property
                MethodBuilder getterBuilder = typeBuilder.DefineMethod($"get_{parameter.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, parameter.ParameterType, Type.EmptyTypes);
                ILGenerator getterIL = getterBuilder.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getterIL.Emit(OpCodes.Ret);

                // Define the setter method for the property
                MethodBuilder setterBuilder = typeBuilder.DefineMethod($"set_{parameter.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { parameter.ParameterType });
                ILGenerator setterIL = setterBuilder.GetILGenerator();
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldarg_1);
                setterIL.Emit(OpCodes.Stfld, fieldBuilder);
                setterIL.Emit(OpCodes.Ret);

                // Associate the getter and setter methods with the property
                propertyBuilder.SetGetMethod(getterBuilder);
                propertyBuilder.SetSetMethod(setterBuilder);
            }

            return typeBuilder.CreateType();
        }

        public Type CreateSignedDTOFromParametersType(Type[] types, ModuleBuilder moduleBuilder, string[] names, string methodName, bool isSigned)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(ControllerGenerator.NamingConvention.GetDTOName(methodName), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);
            if (isSigned)
            {
                typeBuilder.SetParent(typeof(AbstractSignedDTO));
            }
            for (int i = 0; i < types.Length; i++)
            {
                // Create a new field in the target type
                //typeBuilder.DefineField(parameter.Name, parameter.ParameterType, FieldAttributes.Public);

                // Create a new field in the target type
                FieldBuilder fieldBuilder = typeBuilder.DefineField(names[i], types[i], FieldAttributes.Public);

                // Define the property with appropriate getter and setter methods
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(names[i], PropertyAttributes.None, types[i], new Type[] { types[i] });

                // Define the getter method for the property
                MethodBuilder getterBuilder = typeBuilder.DefineMethod($"get_{names[i]}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, types[i], Type.EmptyTypes);
                ILGenerator getterIL = getterBuilder.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getterIL.Emit(OpCodes.Ret);

                // Define the setter method for the property
                MethodBuilder setterBuilder = typeBuilder.DefineMethod($"set_{names[i]}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { types[i] });
                ILGenerator setterIL = setterBuilder.GetILGenerator();
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldarg_1);
                setterIL.Emit(OpCodes.Stfld, fieldBuilder);
                setterIL.Emit(OpCodes.Ret);

                // Associate the getter and setter methods with the property
                propertyBuilder.SetGetMethod(getterBuilder);
                propertyBuilder.SetSetMethod(setterBuilder);
            }

            return typeBuilder.CreateType();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    internal class DTOConstructor : IDTOConstructor
    {
        private INamingConvention NamingConvention { get; set; }

        public DTOConstructor(INamingConvention namingConvention)
        {
            NamingConvention = namingConvention;
        }

        public Type CreateDTO(MethodInfo methodInfo, ModuleBuilder moduleBuilder, bool isSigned)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType(NamingConvention.GetDTOName(methodInfo.Name), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

            if (isSigned)
            {
                typeBuilder.SetParent(typeof(AbstractSignedDTO));
            }

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
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ControllerGenerator
{
    public static class ControllerGenerator
    {
        public static AssemblyBuilder DynamicAssembly { get; set; }

        public static Type CreateController<T>()
        {
            Type type = typeof(T);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName && m.Name != "GetType" && m.Name != "ToString" && m.Name != "Equals" && m.Name != "GetHashCode").ToArray();

            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            DynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = DynamicAssembly.DefineDynamicModule("DynamicClass");
            TypeBuilder typeBuilder = moduleBuilder.DefineType($"{typeof(T)}Controller", TypeAttributes.Public);

            ConstructorInfo apiControllerAttributeCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder apiControllerAttributeBuilder = new CustomAttributeBuilder(apiControllerAttributeCtor, new object[0]);
            typeBuilder.SetCustomAttribute(apiControllerAttributeBuilder);

            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { "[controller]" });
            typeBuilder.SetCustomAttribute(routeAttributeBuilder);

            typeBuilder.SetParent(typeof(ControllerBase));

            foreach (MethodInfo method in methods)
            {
                AddRedirectionMethod(typeBuilder, method);
            }

            return typeBuilder.CreateType();
        }

        private static void AddRedirectionMethod(TypeBuilder typeBuilder, MethodInfo originalMethod)
        {
            var parameters = originalMethod.GetParameters().Select(x => x.GetType()).ToArray();
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                originalMethod.Name,
                originalMethod.Attributes,
                originalMethod.CallingConvention,
                originalMethod.ReturnType,
                parameters
            );

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            // Emit the call to the original method
            ilGenerator.Emit(OpCodes.Ldarg_0); // Load the first argument (object instance) onto the stack
            ilGenerator.Emit(OpCodes.Call, originalMethod); // Call the original method
            ilGenerator.Emit(OpCodes.Ret); // Return from the method

            // Return from the method
            ilGenerator.Emit(OpCodes.Ret);

            ConstructorInfo httpPostAttributeCtor = originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)).GetType().GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder httpPostAttributeBuilder = new CustomAttributeBuilder(httpPostAttributeCtor, new object[] { originalMethod.Name });
            methodBuilder.SetCustomAttribute(httpPostAttributeBuilder);
        }

        public static Type CreateDTO(MethodInfo methodInfo)
        {
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicClass");
            TypeBuilder typeBuilder = moduleBuilder.DefineType($"{methodInfo.Name}Parameters", TypeAttributes.Public);

            foreach (var property in methodInfo.GetParameters())
            {
                // Define a property
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name,
                    PropertyAttributes.None, property.ParameterType, null);

                // Define the backing field for the property
                FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{property.Name}",
                    property.ParameterType, FieldAttributes.Public);

                // Define the get method for the property
                MethodBuilder getMethodBuilder = typeBuilder.DefineMethod($"get_{property.Name}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    property.ParameterType, Type.EmptyTypes);
                ILGenerator getMethodIL = getMethodBuilder.GetILGenerator();
                getMethodIL.Emit(OpCodes.Ldarg_0); // Load 'this' onto the stack
                getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder); // Load the value of the field
                getMethodIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getMethodBuilder);

                // Define the set method for the property
                MethodBuilder setMethodBuilder = typeBuilder.DefineMethod($"set_{property.Name}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null, new[] { property.ParameterType });
                ILGenerator setMethodIL = setMethodBuilder.GetILGenerator();
                setMethodIL.Emit(OpCodes.Ldarg_0); // Load 'this' onto the stack
                setMethodIL.Emit(OpCodes.Ldarg_1); // Load the value to be assigned onto the stack
                setMethodIL.Emit(OpCodes.Stfld, fieldBuilder); // Store the value in the field
                setMethodIL.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }

            return typeBuilder.CreateType();
        }
    }
}

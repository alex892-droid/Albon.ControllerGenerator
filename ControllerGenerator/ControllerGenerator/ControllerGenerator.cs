using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace ControllerGenerator
{
    public class ControllerGenerator
    {
        public static AssemblyBuilder DynamicAssembly { get; set; }

        public static Type CreateController<T>(IRoutingConvention routingConvention)
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
                AddRedirectionMethod(typeBuilder, method, routingConvention);
            }

            return typeBuilder.CreateType();
        }

        private static void AddRedirectionMethod(TypeBuilder typeBuilder, MethodInfo originalMethod, IRoutingConvention routingConvention)
        {
            var parameterTypes = originalMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            var parameterNames = originalMethod.GetParameters().Select(x => x.Name).ToArray();

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    originalMethod.Name,
                    originalMethod.Attributes,
                    originalMethod.CallingConvention,
                    originalMethod.ReturnType,
                    parameterTypes
                );

            ConstructorInfo httpPostAttributeCtor = originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)).GetType().GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder httpPostAttributeBuilder = new CustomAttributeBuilder(httpPostAttributeCtor, new object[0]);
            methodBuilder.SetCustomAttribute(httpPostAttributeBuilder);

            // Define the parameter names and attributes for the new method
            var newMethodParameters = new ParameterBuilder[parameterNames.Length];
            for (int i = 0; i < parameterNames.Length; i++)
            {
                newMethodParameters[i] = methodBuilder.DefineParameter(
                    i + 1, // Parameter position, starting from 1 (0 is reserved for the return value)
                    ParameterAttributes.None,
                    parameterNames[i]
                );

                if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpGetAttribute)
                {
                    ConstructorInfo fromRouteAttributeCtor = typeof(FromRouteAttribute).GetConstructor(Type.EmptyTypes);
                    CustomAttributeBuilder fromRouteAttributeBuilder = new CustomAttributeBuilder(fromRouteAttributeCtor, new object[0]);
                    newMethodParameters[i].SetCustomAttribute(fromRouteAttributeBuilder);
                }
            }

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);

            // Load each parameter onto the stack
            for (int i = 1; i < parameterTypes.Length + 1; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i);
            }

            // Call the original method with the parameters
            ilGenerator.Emit(OpCodes.Call, originalMethod);

            // If the original method is not a void method, handle its return value
            if (originalMethod.ReturnType != typeof(void))
            {
                ilGenerator.Emit(OpCodes.Ret);
            }
            else
            {
                // If the original method is void, add a return statement
                ilGenerator.Emit(OpCodes.Pop);
                ilGenerator.Emit(OpCodes.Ret);
            }

            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { routingConvention.GetRoute(originalMethod.Name, originalMethod.GetParameters(), originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute))) });
            methodBuilder.SetCustomAttribute(routeAttributeBuilder);
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

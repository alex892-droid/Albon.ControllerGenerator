using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection.PortableExecutable;

namespace ControllerGenerator
{
    public class ControllerGenerator
    {
        public static AssemblyBuilder DynamicAssembly { get; set; }

        public static Type CreateController<T>(IRoutingConvention routingConvention)
        {
            TypeBuilder typeBuilder = CreateTypeBuilder<T>();
            SetApiControllerAttribute(typeBuilder);
            SetControllerRouteAttribute(typeBuilder);
            typeBuilder.SetParent(typeof(ControllerBase));
            AddRedirectionMethodsFromType<T>(typeBuilder, routingConvention);
            return typeBuilder.CreateType();
        }

        private static TypeBuilder CreateTypeBuilder<T>()
        {
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            DynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = DynamicAssembly.DefineDynamicModule("DynamicClass");
            return moduleBuilder.DefineType($"{typeof(T)}Controller", TypeAttributes.Public);
        }

        private static void AddRedirectionMethodsFromType<T>(TypeBuilder typeBuilder, IRoutingConvention routingConvention)
        {
            var methods = GetMethodInfosFromType<T>();
            foreach (MethodInfo method in methods)
            {
                AddRedirectionMethod(typeBuilder, method, routingConvention);
            }
        }

        private static MethodInfo[] GetMethodInfosFromType<T>()
        {
            return typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.Name != "GetType" && m.Name != "ToString" && m.Name != "Equals" && m.Name != "GetHashCode")
                .ToArray();
        }

        private static void EmitILRedirection(MethodBuilder methodBuilder, Type[] parameterTypes, MethodInfo originalMethod)
        {
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
        }

        private static void AddRedirectionMethod(TypeBuilder typeBuilder, MethodInfo originalMethod, IRoutingConvention routingConvention)
        {
            if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpGetAttribute)
            {
                SetMethodBuilderWithParameters(typeBuilder, originalMethod, routingConvention);
            }
            else if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPostAttribute)
            {
                SetMethodBuilderWithParameters(typeBuilder, originalMethod, routingConvention);
            }
            else if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPutAttribute)
            {
                SetMethodBuilderWithParameters(typeBuilder, originalMethod, routingConvention);
            }
            else if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpDeleteAttribute)
            {
                SetMethodBuilderWithParameters(typeBuilder, originalMethod, routingConvention);
            }
            else if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPatchAttribute)
            {
                SetMethodBuilderWithParameters(typeBuilder, originalMethod, routingConvention);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void SetMethodBuilderWithParameters(TypeBuilder typeBuilder, MethodInfo originalMethod, IRoutingConvention routingConvention)
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
                    SetFromRouteAttribute(newMethodParameters[i]);
                }
            }

            EmitILRedirection(methodBuilder, parameterTypes, originalMethod);
            SetHttpMethodAttribute(originalMethod, methodBuilder);
            SetRouteAttribute(methodBuilder, originalMethod, routingConvention);
        }

        public static Type CreateDTO(MethodInfo methodInfo)
        {
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicClass");
            TypeBuilder typeBuilder = moduleBuilder.DefineType($"{methodInfo.Name}Parameters", TypeAttributes.Public);

            foreach (var parameter in methodInfo.GetParameters())
            {
                // Create a new field in the target type
                FieldBuilder newField = typeBuilder.DefineField(
                    parameter.Name, parameter.ParameterType, FieldAttributes.Public);
            }

            return typeBuilder.CreateType();
        }

        #region Attribute setter methods

        private static void SetFromRouteAttribute(ParameterBuilder parameterBuilder)
        {
            ConstructorInfo fromRouteAttributeCtor = typeof(FromRouteAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder fromRouteAttributeBuilder = new CustomAttributeBuilder(fromRouteAttributeCtor, new object[0]);
            parameterBuilder.SetCustomAttribute(fromRouteAttributeBuilder);
        }

        private static void SetApiControllerAttribute(TypeBuilder typeBuilder)
        {
            ConstructorInfo apiControllerAttributeCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder apiControllerAttributeBuilder = new CustomAttributeBuilder(apiControllerAttributeCtor, new object[0]);
            typeBuilder.SetCustomAttribute(apiControllerAttributeBuilder);
        }

        private static void SetControllerRouteAttribute(TypeBuilder typeBuilder)
        {
            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { "[controller]" });
            typeBuilder.SetCustomAttribute(routeAttributeBuilder);
        }

        private static void SetRouteAttribute(MethodBuilder methodBuilder, MethodInfo originalMethod, IRoutingConvention routingConvention)
        {
            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { routingConvention.GetRoute(originalMethod.Name, originalMethod.GetParameters(), originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute))) });
            methodBuilder.SetCustomAttribute(routeAttributeBuilder);
        }

        private static void SetHttpMethodAttribute(MethodInfo originalMethod, MethodBuilder methodBuilder)
        {
            ConstructorInfo httpPostAttributeCtor = originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)).GetType().GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder httpPostAttributeBuilder = new CustomAttributeBuilder(httpPostAttributeCtor, new object[0]);
            methodBuilder.SetCustomAttribute(httpPostAttributeBuilder);
        }

        #endregion
    }
}

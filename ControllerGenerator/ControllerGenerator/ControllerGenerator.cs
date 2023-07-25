using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using AttributeSharedKernel;
using System.Runtime.InteropServices;

namespace Albon.ControllerGenerator
{
    public static class ControllerGenerator
    {
        #region Properties

        private const string ModuleName = "DynamicModule";

        private static FieldBuilder ServiceField { get; set; }

        private static TypeBuilder ControllerType { get; set; }

        internal static INamingConvention NamingConvention { get; set; }

        internal static IRoutingConvention RoutingConvention { get; set; }

        internal static ISignatureVerifier SignatureVerifier { get; set; }

        private static AssemblyBuilder _dynamicAssembly;

        public static AssemblyBuilder DynamicAssembly
        {
            get
            {
                if (_dynamicAssembly == null)
                {
                    _dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(nameof(DynamicAssembly)), AssemblyBuilderAccess.Run);
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
                    {
                        AssemblyName requestedName = new AssemblyName(e.Name);

                        if (requestedName.Name == nameof(_dynamicAssembly))
                        {
                            // Load assembly from startup path
                            return _dynamicAssembly;
                        }
                        else
                        {
                            return null;
                        }
                    };
                }

                return _dynamicAssembly;
            }
        }

        private static ModuleBuilder _moduleBuilder;

        public static ModuleBuilder ModuleBuilder
        {
            get
            {
                if (_moduleBuilder == null)
                {
                    _moduleBuilder = DynamicAssembly.DefineDynamicModule(ModuleName);
                }

                return _moduleBuilder;
            }
        }

        #endregion

        public static Type CreateController<TService>()
        {
            return CreateController<TService>(new DefaultRoutingConvention(), new DefaultNamingConvention(), new DefaultSignatureVerifier());
        }

        public static Type CreateController<TService>(IRoutingConvention routingConvention)
        {
            return CreateController<TService>(routingConvention, new DefaultNamingConvention(), new DefaultSignatureVerifier());
        }

        public static Type CreateController<TService>(INamingConvention namingConvention)
        {
            return CreateController<TService>(new DefaultRoutingConvention(), namingConvention, new DefaultSignatureVerifier());
        }

        public static Type CreateController<TService>(ISignatureVerifier signatureVerifier)
        {
            return CreateController<TService>(new DefaultRoutingConvention(), new DefaultNamingConvention(), signatureVerifier);
        }

        public static Type CreateController<TService>(IRoutingConvention routingConvention, INamingConvention namingConvention)
        {
            return CreateController<TService>(routingConvention, namingConvention, new DefaultSignatureVerifier());
        }

        public static Type CreateController<TService>(INamingConvention namingConvention, ISignatureVerifier signatureVerifier)
        {
            return CreateController<TService>(new DefaultRoutingConvention(), namingConvention, signatureVerifier);
        }

        public static Type CreateController<TService>(IRoutingConvention routingConvention, ISignatureVerifier signatureVerifier)
        {
            return CreateController<TService>(routingConvention, new DefaultNamingConvention(), signatureVerifier);
        }

        public static Type CreateController<TService>(IRoutingConvention routingConvention, INamingConvention namingConvention, ISignatureVerifier signatureVerifier)
        {
            //Setting properties
            RoutingConvention = routingConvention;
            NamingConvention = namingConvention;
            SignatureVerifier = signatureVerifier;

            //Defining type
            ControllerType = ModuleBuilder.DefineType(NamingConvention.GetControllerName<TService>(), TypeAttributes.Public | TypeAttributes.Class);

            //Setting attributes
            AttributeSetter.SetApiControllerAttribute(ControllerType);
            AttributeSetter.SetControllerRouteAttribute(ControllerType);

            //Setting parent
            ControllerType.SetParent(typeof(ControllerBase));

            //Defining field of class
            ServiceField = ControllerType.DefineField("_service", typeof(TService), FieldAttributes.Private);

            CreateControllerConstructor<TService>();

            AddRedirectionMethodsFromType<TService>();

            return ControllerType.CreateType();
        }

        private static void CreateControllerConstructor<TService>()
        {
            var serviceConstructorParameters = typeof(TService).GetConstructors().FirstOrDefault().GetParameters().Select(x => x.ParameterType).ToArray();
            // Create a constructor that takes an instance of TService and stores it in the field
            ConstructorBuilder constructor = ControllerType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, serviceConstructorParameters);
            ILGenerator ctorIL = constructor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);                  // Load "this" onto the stack

            int i = 1;
            foreach (var param in serviceConstructorParameters)
            {
                ctorIL.Emit(OpCodes.Ldarg, i++);                  // Load the TService parameter onto the stack
            }
            ctorIL.Emit(OpCodes.Newobj, typeof(TService).GetConstructors().FirstOrDefault());
            ctorIL.Emit(OpCodes.Stfld, ServiceField);      // Store the TService parameter in the private field
            ctorIL.Emit(OpCodes.Ret);
        }

        private static void AddRedirectionMethodsFromType<T>()
        {
            var methods = typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.Name != "GetType" && m.Name != "ToString" && m.Name != "Equals" && m.Name != "GetHashCode")
                .ToArray();

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpGetAttribute)
                {
                    MethodConstructor.SetHttpGetMethodBuilder(method, ServiceField, ControllerType);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPostAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPutAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpDeleteAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPatchAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method);
                }
            }
        }
    }
}

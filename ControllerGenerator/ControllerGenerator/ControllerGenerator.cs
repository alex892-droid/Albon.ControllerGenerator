using System.Reflection.Emit;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using AttributeSharedKernel;
using System.Runtime.InteropServices;

namespace Albon.ControllerGenerator
{
    public class ControllerGenerator
    {
        #region Properties

        private const string ModuleName = "DynamicModule";

        private FieldBuilder ServiceField { get; set; }

        private TypeBuilder ControllerType { get; set; }

        private INamingConvention NamingConvention { get; set; }

        private IRoutingConvention RoutingConvention { get; set; }

        private ISignatureVerifier SignatureVerifier { get; set; }

        private IAttributeSetter AttributeSetter { get; set; }

        private IMethodConstructor MethodConstructor { get; set; }

        private IRedirectionMethodConstructor RedirectionMethodConstructor { get; set; }

        private IDTOConstructor DTOConstructor { get; set; }

        private AssemblyBuilder _dynamicAssembly;

        public AssemblyBuilder DynamicAssembly
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

        private ModuleBuilder _moduleBuilder;

        public ModuleBuilder ModuleBuilder
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

        public ControllerGenerator() : this(new DefaultRoutingConvention(), new DefaultNamingConvention(), new DefaultSignatureVerifier()) { }

        public ControllerGenerator(IRoutingConvention routingConvention) : this(routingConvention, new DefaultNamingConvention(), new DefaultSignatureVerifier()) { }

        public ControllerGenerator(INamingConvention namingConvention) : this(new DefaultRoutingConvention(), namingConvention, new DefaultSignatureVerifier()) { }

        public ControllerGenerator(ISignatureVerifier signatureVerifier) : this(new DefaultRoutingConvention(), new DefaultNamingConvention(), signatureVerifier) { }

        public ControllerGenerator(IRoutingConvention routingConvention, INamingConvention namingConvention) : this(routingConvention, namingConvention, new DefaultSignatureVerifier()) { }

        public ControllerGenerator(INamingConvention namingConvention, ISignatureVerifier signatureVerifier) : this(new DefaultRoutingConvention(), namingConvention, signatureVerifier) { }

        public ControllerGenerator(IRoutingConvention routingConvention, ISignatureVerifier signatureVerifier) : this(routingConvention, new DefaultNamingConvention(), signatureVerifier) { }

        public ControllerGenerator(IRoutingConvention routingConvention, INamingConvention namingConvention, ISignatureVerifier signatureVerifier)
        {
            RoutingConvention = routingConvention;
            NamingConvention = namingConvention;
            SignatureVerifier = signatureVerifier;
            AttributeSetter = new AttributeSetter(routingConvention, namingConvention);
            RedirectionMethodConstructor = new RedirectionMethodConstructor(signatureVerifier);
            DTOConstructor = new DTOConstructor(NamingConvention);
            MethodConstructor = new MethodConstructor(signatureVerifier, RedirectionMethodConstructor, DTOConstructor, namingConvention, routingConvention, AttributeSetter);
        }

        public Type CreateController<TService>()
        {
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

        private void CreateControllerConstructor<TService>()
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

        private void AddRedirectionMethodsFromType<T>()
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
                    MethodConstructor.SetMethodBuilderWithDTO(method, ControllerType, ModuleBuilder, ServiceField);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPutAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method, ControllerType, ModuleBuilder, ServiceField);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpDeleteAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method, ControllerType, ModuleBuilder, ServiceField);
                }
                else if (method.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpPatchAttribute)
                {
                    MethodConstructor.SetMethodBuilderWithDTO(method, ControllerType, ModuleBuilder, ServiceField);
                }
            }
        }
    }
}

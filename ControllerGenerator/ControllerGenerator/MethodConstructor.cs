using AttributeSharedKernel;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Albon.ControllerGenerator
{
    internal class MethodConstructor : IMethodConstructor
    {
        public ISignatureVerifier SignatureVerifier { get; set; }
        public IRedirectionMethodConstructor RedirectionMethodConstructor { get; set; }
        public IDTOConstructor DTOConstructor { get; set; }
        public INamingConvention NamingConvention { get; set; }
        public IRoutingConvention RoutingConvention { get; set; }
        public IAttributeSetter AttributeSetter { get; set; }
        public MethodConstructor(ISignatureVerifier signatureVerifier, IRedirectionMethodConstructor redirectionMethodConstructor, IDTOConstructor dtoConstructor, INamingConvention namingConvention, IRoutingConvention routingConvention, IAttributeSetter attributeSetter)
        {
            SignatureVerifier = signatureVerifier;
            RedirectionMethodConstructor = redirectionMethodConstructor;
            DTOConstructor = dtoConstructor;
            NamingConvention = namingConvention;
            RoutingConvention = routingConvention;
            AttributeSetter = attributeSetter;
        }

        public void SetHttpGetMethodBuilder(MethodInfo originalMethod, FieldBuilder ServiceField, TypeBuilder controllerType)
        {
            var parameterTypes = originalMethod.GetParameters().Select(x => x.ParameterType).ToList();
            var parameterNames = originalMethod.GetParameters().Select(x => x.Name).ToList();
            bool signed = false;
            if (originalMethod.GetCustomAttribute(typeof(SignatureVerifiedAttribute)) is not null)
            {
                parameterTypes.Add(typeof(string));
                parameterNames.Add("publicKey");

                parameterTypes.Add(typeof(string));
                parameterNames.Add("signature");
                signed = true;
            }

            var paramTypes = parameterTypes.ToArray();
            var paramNames = parameterNames.ToArray();

            MethodBuilder methodBuilder = controllerType.DefineMethod(
            NamingConvention.GetMethodName(originalMethod.Name),
            originalMethod.Attributes,
            originalMethod.CallingConvention,
            originalMethod.ReturnType,
            paramTypes
            );

            // Define the parameter names and attributes for the new method
            var newMethodParameters = new ParameterBuilder[paramNames.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                newMethodParameters[i] = methodBuilder.DefineParameter(
                    i + 1, // Parameter position, starting from 1 (0 is reserved for the return value)
                    ParameterAttributes.None,
                    paramNames[i]
                );

                if (originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)) is HttpGetAttribute)
                {
                    AttributeSetter.SetFromRouteAttribute(newMethodParameters[i]);
                }
            }

            RedirectionMethodConstructor.CreateRedirectionMethodFromParameters(methodBuilder, paramTypes, originalMethod, ServiceField, signed);
            AttributeSetter.SetHttpMethodAttribute(originalMethod, methodBuilder);
            AttributeSetter.SetRouteAttribute(methodBuilder, originalMethod, signed);
        }

        public void SetMethodBuilderWithDTO(MethodInfo originalMethod, TypeBuilder controllerType, ModuleBuilder moduleBuilder, FieldBuilder serviceField)
        {
            var parameters = originalMethod.GetParameters();

            MethodBuilder methodBuilder;
            Type DTOType;
            if (parameters.Length == 0)
            {
                var parametersType = new Type[] { };
                if (originalMethod.GetCustomAttribute(typeof(SignatureVerifiedAttribute)) is not null)
                {
                    DTOType = DTOConstructor.CreateDTO(originalMethod, moduleBuilder, true);
                    methodBuilder = controllerType.DefineMethod(
                        NamingConvention.GetMethodName(originalMethod.Name),
                        originalMethod.Attributes,
                        originalMethod.CallingConvention,
                        originalMethod.ReturnType,
                        new Type[] { DTOType }
                        );

                    // Define the parameter names and attributes for the new method
                    var parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, DTOType.Name);
                    AttributeSetter.SetFromBodyAttribute(parameterBuilder);

                    RedirectionMethodConstructor.CreateRedirectionMethodFromVoid(methodBuilder, originalMethod, true);
                }
                else
                {
                    methodBuilder = controllerType.DefineMethod(
                        NamingConvention.GetMethodName(originalMethod.Name),
                        originalMethod.Attributes,
                        originalMethod.CallingConvention,
                        originalMethod.ReturnType,
                        parametersType
                        );

                    RedirectionMethodConstructor.CreateRedirectionMethodFromVoid(methodBuilder, originalMethod, false);
                }
            }
            else
            {
                bool isSigned = originalMethod.GetCustomAttribute(typeof(SignatureVerifiedAttribute)) is not null;

                DTOType = DTOConstructor.CreateDTO(originalMethod, moduleBuilder, isSigned);
                methodBuilder = controllerType.DefineMethod(
                    NamingConvention.GetMethodName(originalMethod.Name),
                    originalMethod.Attributes,
                    originalMethod.CallingConvention,
                    originalMethod.ReturnType,
                    new Type[] { DTOType }
                    );

                // Define the parameter names and attributes for the new method
                var parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, DTOType.Name);
                AttributeSetter.SetFromBodyAttribute(parameterBuilder);

                RedirectionMethodConstructor.CreateRedirectionMethodFromDTO(methodBuilder, DTOType, originalMethod, serviceField, isSigned);
            }

            AttributeSetter.SetHttpMethodAttribute(originalMethod, methodBuilder);
            AttributeSetter.SetRouteAttribute(methodBuilder, originalMethod, false);
        }
    }
}

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

namespace Albon.ControllerGenerator
{
    internal static class MethodConstructor
    {
        public static void SetHttpGetMethodBuilder(MethodInfo originalMethod, FieldBuilder ServiceField, TypeBuilder controllerType)
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
            ControllerGenerator.NamingConvention.GetMethodName(originalMethod.Name),
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

            ILEmitter.EmitILRedirectionFromParameters(methodBuilder, paramTypes, originalMethod, ServiceField, ControllerGenerator.SignatureVerifier, signed);
            AttributeSetter.SetHttpMethodAttribute(originalMethod, methodBuilder);
            AttributeSetter.SetRouteAttribute(methodBuilder, originalMethod, ControllerGenerator.RoutingConvention, ControllerGenerator.NamingConvention, signed);
        }

        internal static void SetMethodBuilderWithDTO(MethodInfo originalMethod)
        {
            var parameters = originalMethod.GetParameters();

            MethodBuilder methodBuilder;
            Type DTOType;
            if (parameters.Length == 0)
            {
                var parametersType = new Type[] { };
                if (originalMethod.GetCustomAttribute(typeof(SignatureVerifiedAttribute)) is not null)
                {
                    parametersType = new Type[1] { typeof(DateTime) };

                    DTOType = CreateSignedDTOFromParametersType(parametersType, new string[] { "callDate" }, originalMethod.Name, true);
                    methodBuilder = ControllerType.DefineMethod(
                        NamingConvention.GetMethodName(originalMethod.Name),
                        originalMethod.Attributes,
                        originalMethod.CallingConvention,
                        originalMethod.ReturnType,
                        new Type[] { DTOType }
                        );

                    // Define the parameter names and attributes for the new method
                    var parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, DTOType.Name);
                    AttributeSetter.SetFromBodyAttribute(parameterBuilder);

                    ILEmitter.EmitILRedirectionFromVoid(methodBuilder, originalMethod, SignatureVerifier, true);
                }
                else
                {
                    methodBuilder = ControllerType.DefineMethod(
                        NamingConvention.GetMethodName(originalMethod.Name),
                        originalMethod.Attributes,
                        originalMethod.CallingConvention,
                        originalMethod.ReturnType,
                        parametersType
                        );

                    ILEmitter.EmitILRedirectionFromVoid(methodBuilder, originalMethod, SignatureVerifier, false);
                }
            }
            else
            {
                bool isSigned = false;
                if (originalMethod.GetCustomAttribute(typeof(SignatureVerifiedAttribute)) is not null)
                {
                    DTOType = CreateSignedDTOFromParametersType(parameters.Select(x => x.ParameterType).ToArray(), parameters.Select(x => x.Name).ToArray(), originalMethod.Name, true);
                    methodBuilder = ControllerType.DefineMethod(
                        NamingConvention.GetMethodName(originalMethod.Name),
                        originalMethod.Attributes,
                        originalMethod.CallingConvention,
                        originalMethod.ReturnType,
                        new Type[] { DTOType }
                        );
                    isSigned = true;
                }
                else
                {
                    DTOType = CreateDTO(originalMethod);
                    methodBuilder = ControllerType.DefineMethod(
                        NamingConvention.GetMethodName(originalMethod.Name),
                        originalMethod.Attributes,
                        originalMethod.CallingConvention,
                        originalMethod.ReturnType,
                        new Type[] { DTOType }
                        );
                }

                // Define the parameter names and attributes for the new method
                var parameterBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.None, DTOType.Name);
                AttributeSetter.SetFromBodyAttribute(parameterBuilder);

                ILEmitter.EmitILRedirectionFromDTO(methodBuilder, DTOType, originalMethod, ServiceField, SignatureVerifier, isSigned);
            }

            AttributeSetter.SetHttpMethodAttribute(originalMethod, methodBuilder);
            AttributeSetter.SetRouteAttribute(methodBuilder, originalMethod, RoutingConvention, NamingConvention, false);
        }
    }
}

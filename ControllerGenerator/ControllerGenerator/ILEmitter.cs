using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Albon.ControllerGenerator
{
    internal static class ILEmitter
    {
        internal static void EmitILRedirectionFromParameters(MethodBuilder methodBuilder, Type[] parameterTypes, MethodInfo originalMethod, FieldBuilder serviceField, ISignatureVerifier signatureVerifier, bool isSigned)
        {
            ILGenerator ilGenerator = methodBuilder.GetILGenerator();
            int nbParam = parameterTypes.Length;
            Label verifyLabel = ilGenerator.DefineLabel();
            if (isSigned)
            {
                nbParam -= 2;
                ilGenerator.Emit(OpCodes.Ldarg_0);
                // Create a list to hold the parameters for JSON serialization
                ilGenerator.Emit(OpCodes.Newobj, typeof(List<object>).GetConstructor(Type.EmptyTypes));
                for (int i = 1; i < nbParam + 1; i++)
                {
                    ilGenerator.Emit(OpCodes.Dup); // Duplicate the list reference on the stack
                    ilGenerator.Emit(OpCodes.Ldarg, i); // Load the parameter onto the stack
                    ilGenerator.Emit(OpCodes.Box, parameterTypes[i - 1]); // Box the parameter to object
                    MethodInfo addMethod = typeof(List<object>).GetMethod("Add");
                    ilGenerator.Emit(OpCodes.Callvirt, addMethod); // Call the Add method to add the parameter to the list
                }

                // Call the method to concatenate JSON representation of the parameters
                ilGenerator.Emit(OpCodes.Call, typeof(JsonConcat).GetMethod("ToJsonConcat", new[] { typeof(List<object>) }));

                // Save the result of the first method call to a local variable
                LocalBuilder resultVariable = ilGenerator.DeclareLocal(typeof(string));
                ilGenerator.Emit(OpCodes.Stloc, resultVariable);

                ilGenerator.Emit(OpCodes.Ldarg_0);

                // Load the result of the first method call (local variable) back onto the stack
                ilGenerator.Emit(OpCodes.Ldloc, resultVariable);

                ilGenerator.Emit(OpCodes.Ldarg, nbParam + 2);
                ilGenerator.Emit(OpCodes.Ldarg, nbParam + 1);
                ilGenerator.Emit(OpCodes.Call, signatureVerifier.GetType().GetMethod("VerifySignature", new[] { typeof(string), typeof(string), typeof(string) }));
            }

            // Mark the label where verification ends and original method call starts
            ilGenerator.MarkLabel(verifyLabel);

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, serviceField);
            // Load each parameter onto the stack
            for (int i = 1; i < nbParam + 1; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i);
            }
            // Call the original method with the parameters 
            ilGenerator.Emit(OpCodes.Call, originalMethod);
            ilGenerator.Emit(OpCodes.Ret);
        }

        internal static void EmitILRedirectionFromDTO(MethodBuilder methodBuilder, Type DTO, MethodInfo originalMethod, FieldBuilder serviceField, ISignatureVerifier signatureVerifier, bool isSigned)
        {
            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, serviceField);

            // Create a local variable to store the DTO instance
            var dtoLocal = ilGenerator.DeclareLocal(DTO);

            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stloc, dtoLocal);

            //if (isSigned)
            //{
            //    ilGenerator.Emit(OpCodes.Ldarg_0);
            //    ilGenerator.Emit(OpCodes.Ldloc, dtoLocal); // Assuming 1 is the index of the DTO
            //    ilGenerator.Emit(OpCodes.Call, signatureVerifier.GetType().GetMethod("VerifySignature", new[] { typeof(AbstractSignedDTO) }));
            //}

            // Load properties from the DTO and push them onto the stack
            // assuming that the properties in the DTO match the parameters of the original method
            var properties = DTO.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldloc, dtoLocal);
                ilGenerator.Emit(OpCodes.Callvirt, properties[i].GetGetMethod());
            }

            // Call the original method with the parameters
            ilGenerator.Emit(OpCodes.Call, originalMethod);
            ilGenerator.Emit(OpCodes.Ret);
        }

        internal static void EmitILRedirectionFromVoid(MethodBuilder methodBuilder, MethodInfo originalMethod, ISignatureVerifier signatureVerifier, bool isSigned)
        {
            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            if (isSigned)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1); // Assuming 1 is the index of the DTO
                ilGenerator.Emit(OpCodes.Call, signatureVerifier.GetType().GetMethod("VerifySignature", new[] { typeof(AbstractSignedDTO) }));
            }

            ilGenerator.Emit(OpCodes.Ldarg_0);

            // Call the original method with the parameters
            ilGenerator.Emit(OpCodes.Call, originalMethod);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}

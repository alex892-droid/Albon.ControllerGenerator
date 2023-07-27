using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    internal interface IRedirectionMethodConstructor
    {
        void CreateRedirectionMethodFromParameters(MethodBuilder methodBuilder, Type[] parameterTypes, MethodInfo originalMethod, FieldBuilder serviceField, bool isSigned);

        void CreateRedirectionMethodFromDTO(MethodBuilder methodBuilder, Type DTO, MethodInfo originalMethod, FieldBuilder serviceField, bool isSigned);

        void CreateRedirectionMethodFromVoid(MethodBuilder methodBuilder, MethodInfo originalMethod, bool isSigned);
    }
}

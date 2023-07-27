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
    public interface IMethodConstructor
    {
        public void SetHttpGetMethodBuilder(MethodInfo originalMethod, FieldBuilder ServiceField, TypeBuilder controllerType);

        public void SetMethodBuilderWithDTO(MethodInfo originalMethod, TypeBuilder controllerType, ModuleBuilder moduleBuilder, FieldBuilder serviceField);
    }
}

using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public interface IAttributeSetter
    {
        public void SetFromRouteAttribute(ParameterBuilder parameterBuilder);

        public void SetFromBodyAttribute(ParameterBuilder parameterBuilder);

        public void SetApiControllerAttribute(TypeBuilder controllerType);

        public void SetControllerRouteAttribute(TypeBuilder controllerType);

        public void SetRouteAttribute(MethodBuilder methodBuilder, MethodInfo originalMethod, bool signed);

        public void SetHttpMethodAttribute(MethodInfo originalMethod, MethodBuilder methodBuilder);
    }
}

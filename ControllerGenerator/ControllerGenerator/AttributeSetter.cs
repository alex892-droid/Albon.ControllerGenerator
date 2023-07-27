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
    internal class AttributeSetter : IAttributeSetter
    {
        public IRoutingConvention RoutingConvention { get; set; }
        public INamingConvention NamingConvention { get; set; }
        public AttributeSetter(IRoutingConvention routingConvention, INamingConvention namingConvention) 
        {
            RoutingConvention = routingConvention;
            NamingConvention = namingConvention;
        }

        public void SetFromRouteAttribute(ParameterBuilder parameterBuilder)
        {
            ConstructorInfo fromRouteAttributeCtor = typeof(FromRouteAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder fromRouteAttributeBuilder = new CustomAttributeBuilder(fromRouteAttributeCtor, new object[0]);
            parameterBuilder.SetCustomAttribute(fromRouteAttributeBuilder);
        }

        public void SetFromBodyAttribute(ParameterBuilder parameterBuilder)
        {
            ConstructorInfo fromRouteAttributeCtor = typeof(FromBodyAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder fromRouteAttributeBuilder = new CustomAttributeBuilder(fromRouteAttributeCtor, new object[0]);
            parameterBuilder.SetCustomAttribute(fromRouteAttributeBuilder);
        }

        public void SetApiControllerAttribute(TypeBuilder controllerType)
        {
            ConstructorInfo apiControllerAttributeCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder apiControllerAttributeBuilder = new CustomAttributeBuilder(apiControllerAttributeCtor, new object[0]);
            controllerType.SetCustomAttribute(apiControllerAttributeBuilder);
        }

        public void SetControllerRouteAttribute(TypeBuilder controllerType)
        {
            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { "[controller]" });
            controllerType.SetCustomAttribute(routeAttributeBuilder);
        }

        public void SetRouteAttribute(MethodBuilder methodBuilder, MethodInfo originalMethod, bool signed)
        {
            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { RoutingConvention.GetRoute(NamingConvention.GetMethodName(originalMethod.Name), originalMethod.GetParameters(), originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)), signed) });
            methodBuilder.SetCustomAttribute(routeAttributeBuilder);
        }

        public void SetHttpMethodAttribute(MethodInfo originalMethod, MethodBuilder methodBuilder)
        {
            ConstructorInfo httpPostAttributeCtor = originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)).GetType().GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder httpPostAttributeBuilder = new CustomAttributeBuilder(httpPostAttributeCtor, new object[0]);
            methodBuilder.SetCustomAttribute(httpPostAttributeBuilder);
        }
    }
}

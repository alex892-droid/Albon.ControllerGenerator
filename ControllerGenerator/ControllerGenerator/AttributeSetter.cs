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
    internal class AttributeSetter
    {
        internal static void SetFromRouteAttribute(ParameterBuilder parameterBuilder)
        {
            ConstructorInfo fromRouteAttributeCtor = typeof(FromRouteAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder fromRouteAttributeBuilder = new CustomAttributeBuilder(fromRouteAttributeCtor, new object[0]);
            parameterBuilder.SetCustomAttribute(fromRouteAttributeBuilder);
        }

        internal static void SetFromBodyAttribute(ParameterBuilder parameterBuilder)
        {
            ConstructorInfo fromRouteAttributeCtor = typeof(FromBodyAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder fromRouteAttributeBuilder = new CustomAttributeBuilder(fromRouteAttributeCtor, new object[0]);
            parameterBuilder.SetCustomAttribute(fromRouteAttributeBuilder);
        }

        internal static void SetApiControllerAttribute(TypeBuilder controllerType)
        {
            ConstructorInfo apiControllerAttributeCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder apiControllerAttributeBuilder = new CustomAttributeBuilder(apiControllerAttributeCtor, new object[0]);
            controllerType.SetCustomAttribute(apiControllerAttributeBuilder);
        }

        internal static void SetControllerRouteAttribute(TypeBuilder controllerType)
        {
            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { "[controller]" });
            controllerType.SetCustomAttribute(routeAttributeBuilder);
        }

        internal static void SetRouteAttribute(MethodBuilder methodBuilder, MethodInfo originalMethod, IRoutingConvention routingConvention, INamingConvention namingConvention, bool signed)
        {
            ConstructorInfo routeAttributeCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            CustomAttributeBuilder routeAttributeBuilder = new CustomAttributeBuilder(routeAttributeCtor, new object[] { routingConvention.GetRoute(namingConvention.GetMethodName(originalMethod.Name), originalMethod.GetParameters(), originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)), signed) });
            methodBuilder.SetCustomAttribute(routeAttributeBuilder);
        }

        internal static void SetHttpMethodAttribute(MethodInfo originalMethod, MethodBuilder methodBuilder)
        {
            ConstructorInfo httpPostAttributeCtor = originalMethod.GetCustomAttribute(typeof(HttpMethodAttribute)).GetType().GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder httpPostAttributeBuilder = new CustomAttributeBuilder(httpPostAttributeCtor, new object[0]);
            methodBuilder.SetCustomAttribute(httpPostAttributeBuilder);
        }
    }
}

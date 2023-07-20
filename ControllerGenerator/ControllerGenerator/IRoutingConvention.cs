using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace ControllerGenerator
{
    public interface IRoutingConvention
    {
        public string GetRoute(string methodName, ParameterInfo[]? parameters, Attribute httpMethodAttributeType);
    }
}

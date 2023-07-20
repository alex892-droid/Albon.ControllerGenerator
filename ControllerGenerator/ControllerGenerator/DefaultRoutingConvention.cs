using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace ControllerGenerator
{
    public class DefaultRoutingConvention : IRoutingConvention
    {
        public string GetRoute(string methodName, ParameterInfo[]? parameters, Attribute httpMethodAttributeType)
        {
            string route = string.Empty;
            if(httpMethodAttributeType is HttpGetAttribute)
            {
                route = $"{methodName}";
                for(int i = 0; i < parameters.Count(); i++)
                {
                    route += "/{" + parameters[i].Name + "}" ;
                }
            }
            else if(httpMethodAttributeType is HttpPostAttribute)
            {
                route = $"{methodName}";
            }

            return route;
        }
    }
}

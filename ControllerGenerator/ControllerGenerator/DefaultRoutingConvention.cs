using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ControllerGenerator
{
    internal class DefaultRoutingConvention : IRoutingConvention
    {
        public string GetRoute(string methodName, ParameterInfo[]? parameters, Attribute httpMethodAttributeType)
        {
            string route;
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
            else if (httpMethodAttributeType is HttpPutAttribute)
            {
                route = $"{methodName}";
            }
            else if (httpMethodAttributeType is HttpDeleteAttribute)
            {
                route = $"{methodName}";
            }
            else if (httpMethodAttributeType is HttpPatchAttribute)
            {
                route = $"{methodName}";
            }
            else
            {
                throw new NotImplementedException();
            }

            return route;
        }
    }
}

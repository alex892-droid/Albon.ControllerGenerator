using System.Reflection;

namespace Albon.ControllerGenerator
{
    public interface IRoutingConvention
    {
        public string GetRoute(string methodName, ParameterInfo[]? parameters, Attribute httpMethodAttributeType, bool signed);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public interface INamingConvention
    {
        public string GetControllerName<TService>();

        public string GetMethodName(string originalMethodName);

        public string GetDTOName(string input);
    }
}

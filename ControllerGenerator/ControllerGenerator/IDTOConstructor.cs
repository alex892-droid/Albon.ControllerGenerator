using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public interface IDTOConstructor
    {
        Type CreateDTO(MethodInfo methodInfo, ModuleBuilder moduleBuilder, bool isSigned);
    }
}

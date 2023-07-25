using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public class JsonConcat
    {
        public string ToJsonConcat(List<object> objs)
        {
            string result = string.Empty;
            foreach(object obj in objs)
            {
                result += JsonConvert.SerializeObject(obj);
            }
            return result;
        }

        public string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}

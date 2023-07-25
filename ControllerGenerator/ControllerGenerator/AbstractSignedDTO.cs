using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public abstract class AbstractSignedDTO
    {
        [JsonIgnore]
        public string Signature { get; set; }

        [JsonIgnore]
        public string PublicKey { get; set; }

        public string GetMessage()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public interface ISignatureVerifier
    {
        public void VerifySignature(string message, string signature, string publicKey);

        public void VerifySignature(AbstractSignedDTO DTO);
    }
}

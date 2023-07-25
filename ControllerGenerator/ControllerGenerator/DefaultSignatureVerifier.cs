using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albon.ControllerGenerator
{
    public class DefaultSignatureVerifier : ISignatureVerifier
    {
        public void VerifySignature(string message, string signature, string publicKey)
        {
            if(!Cryptography.CryptographyService.VerifySignature(message, signature, publicKey))
            {
                throw new ArgumentException("Signature invalid.");
            }
        }

        public void VerifySignature(AbstractSignedDTO DTO)
        {
            if (!Cryptography.CryptographyService.VerifySignature(DTO.GetMessage(), DTO.Signature, DTO.PublicKey))
            {
                throw new ArgumentException("Signature invalid.");
            }
        }
    }
}

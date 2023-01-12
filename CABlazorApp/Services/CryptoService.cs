using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CABlazorApp.Services;

public class CryptoService
{
    public async Task<byte[]> Sign(byte[] certFile, byte[] docFile, string password)
    {
        X509Certificate2 cert = new X509Certificate2(certFile, password);
        
        MemoryStream stream = new MemoryStream();
        using (RSA rsa = cert.GetRSAPrivateKey())
        {
            var signature = rsa.SignData(docFile, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            stream.Write(signature);
        }

        return stream.ToArray();
    }
    
    public async Task<byte[]> UnSign(byte[] certFile, byte[] docFile, string password)
    {
        X509Certificate2 cert = new X509Certificate2(certFile, password);
        
        MemoryStream stream = new MemoryStream();
        using (RSA rsa = cert.GetRSAPrivateKey())
        {
            using (RSA rsa2 = cert.GetRSAPublicKey())
            {
                if (rsa == rsa2)
                {
                    Console.WriteLine("yes");
                }
                else
                {
                    Console.WriteLine("no");
                }
                // var signature = rsa.SignData(docFile, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                // var publicKeyProvider = (RSACryptoServiceProvider)cert.PublicKey.Key;
                // stream.Write(signature);
            }
            
        }

        return stream.ToArray();
    }
}
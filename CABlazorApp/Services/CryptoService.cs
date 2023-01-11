using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace CABlazorApp.Services;

public class CryptoService
{
    public async Task<byte[]> Sign(byte[] certFile, byte[] docFile, string password)
    {
        X509Certificate2 cert = new X509Certificate2(certFile, password);


        MemoryStream stream = new MemoryStream(docFile);
        using (RSA rsa = cert.GetRSAPrivateKey())
        {
            var signature = rsa.SignData(docFile, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            stream.Write(signature);
        }

        return stream.ToArray();
    }
}
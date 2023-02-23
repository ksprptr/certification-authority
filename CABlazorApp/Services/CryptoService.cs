using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CABlazorApp.Services;

public class CryptoService
{
    public async Task<byte[]> Sign(byte[] certFile, byte[] docFile, string password)
    {
        try
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
        catch (Exception e)
        {
            string wrongPassword = "wrongPass";
            MemoryStream stream = new MemoryStream();
            stream.Write(Encoding.ASCII.GetBytes(wrongPassword));
            return stream.ToArray();
        }
    }

    public async Task<byte[]> Generate(string password)
    {
        RSA rsa = RSA.Create();
        CertificateRequest certRequest = new CertificateRequest("CN=MyCert", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        X509Certificate2 certificate = certRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(3650));
        byte[] certBytes = certificate.Export(X509ContentType.Pfx, password);
        return certBytes;
    }

}
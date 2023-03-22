using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CABlazorApp.Services;

public class CryptoService
{
    public async Task<Tuple<byte[], Exception, byte[]>> Sign(byte[] certFile, byte[] docFile, string password)
    {
        try
        {
            X509Certificate2 cert = new X509Certificate2(certFile, password);
            MemoryStream stream = new MemoryStream();
            stream.Write(docFile);
            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                var signature = rsa.SignData(docFile, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                return new Tuple<byte[], Exception, byte[]>(stream.ToArray(), null, signature);
            }
        }
        catch (Exception e)
        {
            return new Tuple<byte[], Exception, byte[]>(null, e, null);
        }
    }

    public async Task<Tuple<byte[], RSA>> Generate(string password)
    {
        RSA rsa = RSA.Create();
        CertificateRequest certRequest = new CertificateRequest("CN=MyCert", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        X509Certificate2 certificate = certRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(3650));
        RSA rsaPublic = certificate.GetRSAPublicKey();
        byte[] certBytes = certificate.Export(X509ContentType.Pfx, password);
        return new Tuple<byte[], RSA>(certBytes, rsaPublic);
    }

    public async Task<int> Verify(byte[] data, byte[] signature, byte[] public_key)
    {
        RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(public_key, out int bytesRead);
        if (rsa.VerifyData(data, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1))
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

}
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

    public byte[] Generate(string password)
    {
        var ecdsa = ECDsa.Create(); // generate asymmetric key pair
        var req = new CertificateRequest("cn=foobar", ecdsa, HashAlgorithmName.SHA256);
        var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

        // Create PFX (PKCS #12) with private key
        return cert.Export(X509ContentType.Pfx, password);

        // Create Base 64 encoded CER (public key only)
        // File.WriteAllText("c:\\temp\\mycert.cer",
        //     "-----BEGIN CERTIFICATE-----\r\n"
        //     + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
        //     + "\r\n-----END CERTIFICATE-----");
    }
}
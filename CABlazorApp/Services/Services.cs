using System.DirectoryServices.Protocols;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CABlazorApp.Services;

public class Services
{
    public static Tuple<byte[]?, Exception?, byte[]?> Sign(byte[] certificate, byte[] file, string password)
    {
        try
        {
            X509Certificate2 cert = new(certificate, password);
            MemoryStream stream = new();
            stream.Write(file);
            using var rsa = cert.GetRSAPrivateKey();
            var signature = rsa.SignData(file, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            return new Tuple<byte[]?, Exception?, byte[]?>(stream.ToArray(), null, signature);
        }
        catch (Exception e)
        {
            return new Tuple<byte[]?, Exception?, byte[]?>(null, e, null);
        }
    }

    public static Tuple<byte[], byte[]> Generate(string password)
    {
        var rsa = RSA.Create();
        CertificateRequest certRequest = new("CN=MyCert", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate = certRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(3650));
        var certPublicBytes = certificate.Export(X509ContentType.Cert);
        var certPrivateBytes = certificate.Export(X509ContentType.Pfx, password);
        return new Tuple<byte[], byte[]>(certPrivateBytes, certPublicBytes);
    }

    public static bool Verify(byte[] data, byte[] signature, byte[] certificate)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportSubjectPublicKeyInfo(certificate, out _);

            var rsaParams = rsa.ExportParameters(false);
            rsaParams.Exponent = new byte[] { 1, 0, 1 }; // Předpokládaná hodnota exponentu, uprav podle potřeby
            rsa.ImportParameters(rsaParams);

            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
    }
}
namespace CABlazorApp.Data;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class Enc
{
    public static async Task Encrypt(string filePath, string certName, string password)
    {
        Aes aesKey = Aes.Create();
        aesKey.GenerateKey();
        byte[] ivKey = new byte[aesKey.IV.Length];
        Array.Copy(aesKey.Key, ivKey, aesKey.IV.Length);
        aesKey.IV = ivKey;
        var encryptor = aesKey.CreateEncryptor();
        var encryptedKey = EncryptKey(aesKey.Key, certName, password);
        var file = await File.ReadAllBytesAsync(filePath);
        using (var outputStream = new FileStream(filePath, FileMode.Truncate))
        {
            await outputStream.WriteAsync(encryptedKey, 0, encryptedKey.Length);
            using (var encryptStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
            using (var inputStream = new MemoryStream(file))
                await inputStream.CopyToAsync(encryptStream);
        }
    }
    public static byte[] EncryptKey(byte[] key, string certName, string password)
    {
        var cert = new X509Certificate2(certName, password);
        var publicKey = cert.GetRSAPublicKey();
        return publicKey!.Encrypt(key, RSAEncryptionPadding.OaepSHA256);
    }
    public static async Task Decrypt(string filePath, string certName, string password)
    {
        var file = (await File.ReadAllBytesAsync(filePath)).ToList();
        var encryptedKey = new Collection<byte>();
        var encryptLength = EncryptKey(Encoding.UTF8.GetBytes("string"), certName, password).Length;
        for (var i = 0; i < encryptLength; i++)
            encryptedKey.Add(file[i]);
        file.RemoveRange(0, encryptLength);
        var decryptedKey = DecryptKey(encryptedKey.ToArray(), certName, password);
        using (var managed = new AesManaged())
        {
            Aes aesKey = Aes.Create();
            aesKey.Key = decryptedKey;
            byte[] ivKey = new byte[aesKey.IV.Length];
            Array.Copy(aesKey.Key, ivKey, aesKey.IV.Length);
            aesKey.IV = ivKey;
            var decryptor = aesKey.CreateDecryptor();
            using (var fileStream = new FileStream(filePath, FileMode.Truncate))
            using (var decryptStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Write))
            using (var encryptedFileStream = new MemoryStream(file.ToArray()))
                await encryptedFileStream.CopyToAsync(decryptStream);
        }
    }
    private static byte[] DecryptKey(byte[] keyBytes, string certName, string password)
    {
        var cert = new X509Certificate2(certName, password);
        var privateKey = cert.GetRSAPrivateKey();
        return privateKey!.Decrypt(keyBytes, RSAEncryptionPadding.OaepSHA256);
    }
}
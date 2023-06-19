using Microsoft.IdentityModel.Tokens;

namespace CABlazorApp.Services.GeneratePage;

public static class Verify
{
    private const string Required = "Toto pole je povinné.";
    private static readonly List<string> AllowedFileExtensions = new() { ".docx", ".docm", ".doc", ".pdf", ".pptx", ".pptm", ".ppt", ".xlsx", ".xlsm", ".xls", ".csv", ".txt" };
    private static readonly List<string> AllowedCertificateExtensions = new() { ".crt", ".cer", ".p7b", ".p7c", ".p7s", ".pem", ".p12", ".pfx" };

    public static Tuple<bool?, string[]> VerifyFile(string userName, byte[] file, string fileName, byte[] certificate, byte[] fileWithoutSignature, string fileWithoutSignatureName)
    {
        // Check if the inputs aren't null or empty
        if (fileWithoutSignature.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", Required, "", "", "" });
        if (file.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", Required, "", "" });
        if (certificate.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", "", Required, "" });

        // Check if the input files are the same
        if (Path.GetExtension(fileName) != Path.GetExtension(fileWithoutSignatureName)) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", "", "", "Vybrané soubory pro ověření musí mít stejnou příponu." });

        // Set the paths
        var dirPath = Path.Combine("Archive", userName, "temp", "verify");
        var filePath = Path.Combine("Archive", userName, "temp", "verify", fileName);
        
        // Check if the directories and files exist or not
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        if (File.Exists(filePath)) { File.Delete(filePath); File.Create(filePath).Close(); }
        if (!File.Exists(filePath)) File.Create(filePath).Close();
        
        // Write all bytes to the file
        File.WriteAllBytes(filePath, file);
        
        try
        {
            return new Tuple<bool?, string[]>(Services.Verify(fileWithoutSignature, Metadata.GetMetadata(filePath), certificate), new[] { "", "", "", "", "", "", "", "", "", "" });
        }
        catch (Exception exception)
        {
            File.Delete(filePath);
            return exception.Message == "Object reference not set to an instance of an object." ? new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", "", "", "Tento soubor nebyl podepsán certifikátem vydaným naší certifikační autoritou." }) : new Tuple<bool?, string[]>(null, new []{ "", "", "", "", "", "", "", "", "", "ERROR: " + exception.Message });
        }
    }
}
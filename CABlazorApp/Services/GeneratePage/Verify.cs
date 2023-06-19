using Microsoft.IdentityModel.Tokens;
using static CABlazorApp.Services.Properties;

namespace CABlazorApp.Services.GeneratePage;

public static class Verify
{
    public static Tuple<bool?, string[]> VerifyFile(string userName, FileData fileWithoutSignature, FileData file, FileData certificate)
    {
        // Check if the inputs aren't null or empty
        if (fileWithoutSignature.Data.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", Required, "", "", "" });
        if (file.Data.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", Required, "", "" });
        if (certificate.Data.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", "", Required, "" });

        // Check if the input files are the same
        if (Path.GetExtension(fileWithoutSignature.FileName) != Path.GetExtension(file.FileName)) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", "", "", "Vybrané soubory pro ověření musí mít stejnou příponu." });

        // Set the paths
        var dirPath = Path.Combine("Archive", userName, "temp", "verify");
        var filePath = Path.Combine("Archive", userName, "temp", "verify", file.FileName);
        
        // Check if the directories and files exist or not
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        if (File.Exists(filePath)) { File.Delete(filePath); File.Create(filePath).Close(); }
        if (!File.Exists(filePath)) File.Create(filePath).Close();
        
        // Write all bytes to the file
        File.WriteAllBytes(filePath, file.Data);
        
        try
        {
            return new Tuple<bool?, string[]>(Services.Verify(fileWithoutSignature.Data, Metadata.GetMetadata(filePath), certificate.Data), new[] { "", "", "", "", "", "", "", "", "", "" });
        }
        catch (Exception exception)
        {
            File.Delete(filePath);
            return exception.Message == "Object reference not set to an instance of an object." ? new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", "", "", "Tento soubor nebyl podepsán certifikátem vydaným naší certifikační autoritou." }) : new Tuple<bool?, string[]>(null, new []{ "", "", "", "", "", "", "", "", "", "ERROR: " + exception.Message });
        }
    }
}
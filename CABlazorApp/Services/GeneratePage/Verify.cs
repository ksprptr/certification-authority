using Microsoft.IdentityModel.Tokens;

namespace CABlazorApp.Services.GeneratePage;

public static class Verify
{
    private const string Required = "Toto pole je povinné.";

    public static Tuple<bool?, string[]> VerifyFile(string userName, byte[] file, string fileName, byte[] certificate)
    {
        // Check if the inputs aren't null or empty
        if (file.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", Required, "", "" });
        if (certificate.IsNullOrEmpty()) return new Tuple<bool?, string[]>(null, new[] { "", "", "", "", "", "", "", Required, "" });

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
            return new Tuple<bool?, string[]>(Services.Verify(file, Metadata.GetMetadata(filePath), certificate), new[] { "", "", "", "", "", "", "", "", "" });
        }
        catch (Exception exception)
        {
            File.Delete(filePath);
            return new Tuple<bool?, string[]>(null, new []{ "", "", "", "", "", "", "", "", "ERROR: " + exception.Message });
        }
    }
}
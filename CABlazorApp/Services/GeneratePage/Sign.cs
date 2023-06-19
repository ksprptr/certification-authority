using Microsoft.IdentityModel.Tokens;

namespace CABlazorApp.Services.GeneratePage;

public static class Sign
{
    private const string Required = "Toto pole je povinné.";
    private static readonly List<string> AllowedFileExtensions = new() { ".docx", ".docm", ".doc", ".pdf", ".pptx", ".pptm", ".ppt", ".xlsx", ".xlsm", ".xls", ".csv", ".txt" };
    private static readonly List<string> AllowedCertificateExtensions = new() { ".crt", ".cer", ".p7b", ".p7c", ".p7s", ".pem", ".p12", ".pfx" };
    
    public static Tuple<bool, string[]> SignFile(string userName, byte[] certificate, string certificateName, string password, byte[] file, string fileName)
    {
        // Get extensions
        var fileExtension = Path.GetExtension(fileName);
        var certificateExtension = Path.GetExtension(certificateName);

        // Check if the inputs aren't null or empty
        if (certificate.IsNullOrEmpty()) return new Tuple<bool, string[]>(false, new[] { Required, "", "", "", "", "", "", "", "", "" });
        if (string.IsNullOrWhiteSpace(password)) return new Tuple<bool, string[]>(false, new []{ "", Required, "", "", "", "", "", "", "", "" });
        if (file is null) return new Tuple<bool, string[]>(false, new []{ "", "", Required, "", "", "", "", "", "", "" });
        
        // Check if the extensions are allowed
        if (!AllowedCertificateExtensions.Contains(certificateExtension)) return new Tuple<bool, string[]>(false, new [] { "Tento formát certifikátu není podporován.", "", "", "", "", "", "", "", "", "" });
        if (!AllowedFileExtensions.Contains(fileExtension)) return new Tuple<bool, string[]>(false, new [] { "", "", "Tento typ souboru není podporován.", "", "", "", "", "", "", "" });

        // Save the file, exception and signature
        var (signedFile, exception, signature) = Services.Sign(certificate, file, password);

        // Check if the exception isn't null
        if (exception != null)
        {
            switch (exception.Message)
            {
                case "The specified network password is not correct.":
                    return new Tuple<bool, string[]>(false, new []{ "", "Zadal jste špatné heslo.", "", "", "", "", "", "", "", "" });

                case "Cannot find the requested object.":
                    return new Tuple<bool, string[]>(false, new []{ "", "", "", "", "", "Tento certifikát neobsahuje heslo nebo je poškozený.", "", "", "", "" });
            
                case "Object reference not set to an instance of an object.":
                    return new Tuple<bool, string[]>(false, new []{ "", "", "", "", "", "Tento certifikát neobsahuje heslo nebo je poškozený.", "", "", "", "" });
                
                default:
                    return new Tuple<bool, string[]>(false, new[] { "", "", "", "", "", "ERROR: " + exception.Message, "", "", "", "" });
            }
        }

        // Set the paths
        var dirPath = Path.Combine("Archive", userName, "files");
        var filePath = Path.Combine("Archive", userName, "files", fileName);

        // Check if the directories and files exist or not
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        if (File.Exists(filePath)) { File.Delete(filePath); File.Create(filePath).Close(); }
        if (!File.Exists(filePath)) File.Create(filePath).Close();
        
        // Write the signed file to the file
        File.WriteAllBytesAsync(filePath, signedFile);
        
        // Set a metadata
        Metadata.SetMetadata(filePath, signature);

        // Return the success
        return new Tuple<bool, string[]>(true, new[] { "", "", "", "", "", "", "", "", "", "" });
    }
}
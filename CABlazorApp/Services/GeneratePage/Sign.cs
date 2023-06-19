using Microsoft.IdentityModel.Tokens;
using static CABlazorApp.Services.Properties;

namespace CABlazorApp.Services.GeneratePage;

public static class Sign
{

    public static Tuple<bool, string[]> SignFile(string userName, FileData certificate, string password, FileData file)
    {
        // Get extensions
        var fileExtension = Path.GetExtension(file.FileName);
        var certificateExtension = Path.GetExtension(certificate.FileName);

        // Check if the inputs aren't null or empty
        if (certificate.Data.IsNullOrEmpty()) return new Tuple<bool, string[]>(false, new[] { Required, "", "", "", "", "", "", "", "", "" });
        if (string.IsNullOrWhiteSpace(password)) return new Tuple<bool, string[]>(false, new []{ "", Required, "", "", "", "", "", "", "", "" });
        if (file is null) return new Tuple<bool, string[]>(false, new []{ "", "", Required, "", "", "", "", "", "", "" });
        
        // Check if the extensions are allowed
        if (!AllowedCertificateExtensions.Contains(certificateExtension)) return new Tuple<bool, string[]>(false, new [] { "Tento formát certifikátu není podporován.", "", "", "", "", "", "", "", "", "" });
        if (!AllowedFileExtensions.Contains(fileExtension)) return new Tuple<bool, string[]>(false, new [] { "", "", "Tento typ souboru není podporován.", "", "", "", "", "", "", "" });

        // Save the file, exception and signature
        var (signedFile, exception, signature) = Services.Sign(certificate.Data, file.Data, password);

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
        var filePath = Path.Combine("Archive", userName, "files", file.FileName);

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
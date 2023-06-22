using Microsoft.IdentityModel.Tokens;
using static CABlazorApp.Services.Properties;

namespace CABlazorApp.Services.GeneratePage;

public static class Sign
{

    public static Result SignFile(string userName, FileData? certificate, string password, FileData file)
    {
        // Check if the inputs aren't null or empty
        if (certificate == null) return new Result(false, new[] { Required, "", "", "", "", "", "", "", "", "" });
        if (string.IsNullOrWhiteSpace(password)) return new Result(false, new []{ "", Required, "", "", "", "", "", "", "", "" });
        if (file is null) return new Result(false, new []{ "", "", Required, "", "", "", "", "", "", "" });

        // Check if the extensions are allowed
        if (!AllowedCertificateExtensions.Contains(Path.GetExtension(certificate.Name))) return new Result(false, new [] { "Tento formát certifikátu není podporován.", "", "", "", "", "", "", "", "", "" });
        if (!AllowedFileExtensions.Contains(Path.GetExtension(file.Name))) return new Result(false, new [] { "", "", "Tento formát souboru není podporován.", "", "", "", "", "", "", "" });

        // Save the file, exception and signature
        var (signedFile, exception, signature) = Services.Sign(certificate.Data, file.Data, password);

        // Check if the exception isn't null
        if (exception != null)
        {
            switch (exception.Message)
            {
                case "The specified network password is not correct.":
                    return new Result(false, new []{ "", "Zadal jste špatné heslo.", "", "", "", "", "", "", "", "" });

                case "Cannot find the requested object." or "Object reference not set to an instance of an object.":
                    return new Result(false, new []{ "", "", "", "", "", "Tento certifikát není zaheslovaný nebo je poškozený.", "", "", "", "" });

                default:
                    return new Result(false, new[] { "", "", "", "", "", "Error: " + exception.Message, "", "", "", "" });
            }
        }

        // Set the paths
        var dirPath = Path.Combine("Archive", userName, "files");
        var filePath = Path.Combine("Archive", userName, "files", file.Name);

        // Check if the directories and files exist or not
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        if (File.Exists(filePath)) { File.Delete(filePath); Thread.Sleep(100); File.Create(filePath).Close(); }
        if (!File.Exists(filePath)) File.Create(filePath).Close();
        
        // Write data into the file
        File.WriteAllBytesAsync(filePath, signedFile);
        
        // Set a metadata
        Metadata.SetMetadata(filePath, signature);

        // Return the success
        return new Result(true);
    }
}
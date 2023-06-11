using Microsoft.IdentityModel.Tokens;

namespace CABlazorApp.Services.GeneratePage;

public static class Sign
{
    private const string Required = "Toto pole je povinné.";

    public static Tuple<bool, string[]> SignFile(string userName, byte[] certificate, string password, byte[] file, string fileName)
    {
        // Check if the inputs aren't null or empty
        if (certificate.IsNullOrEmpty()) return new Tuple<bool, string[]>(false, new[] { Required, "", "", "", "", "", "", "", "" });
        if (string.IsNullOrWhiteSpace(password)) return new Tuple<bool, string[]>(false, new []{ "", Required, "", "", "", "", "", "", "" });
        if (file.IsNullOrEmpty()) return new Tuple<bool, string[]>(false, new []{ "", "", Required, "", "", "", "", "", "" });

        // Save the file, exception and signature
        var (signedFile, exception, signature) = Services.Sign(certificate, file, password);

        // Check if the exception isn't null
        if (exception != null)
        {
            switch (exception.Message)
            {
                case "The specified network password is not correct.":
                    return new Tuple<bool, string[]>(false, new []{ "", "Zadal jste špatné heslo.", "", "", "", "", "", "", "" });

                case "Cannot find the requested object.":
                    return new Tuple<bool, string[]>(false, new []{ "", "", "", "", "", "Vybral jste neplatný certifikát. Vyberte prosím platný certifikát.", "", "", "" });
            
                default:
                    return new Tuple<bool, string[]>(false, new[] { "", "", "", "", "", exception.Message, "", "", "" });
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
        
        // Write ADS named 'Signature' to the file
        AlternateDataStream.WriteAds(filePath, "Signature", Convert.ToBase64String(signature));

        // Return the success
        return new Tuple<bool, string[]>(true, new[] { "", "", "", "", "", "", "", "", "" });
    }
}
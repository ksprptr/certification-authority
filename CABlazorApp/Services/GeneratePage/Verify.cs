using Microsoft.IdentityModel.Tokens;
using static CABlazorApp.Services.Properties;

namespace CABlazorApp.Services.GeneratePage;

public static class Verify
{
    public static Result VerifyFile(string userName, FileData? fileWithoutSignature, FileData? file, FileData? certificate)
    {
        // Check if the inputs aren't null or empty
        if (fileWithoutSignature == null) return new Result(new[] { "", "", "", "", "", "", Required, "", "", "" });
        if (file == null) return new Result(new[] { "", "", "", "", "", "", "", Required, "", "" });
        if (certificate == null) return new Result(new[] { "", "", "", "", "", "", "", "", Required, "" });

        // Check if the input files are the same
        if (Path.GetExtension(fileWithoutSignature.Name) != Path.GetExtension(file.Name)) return new Result(new[] { "", "", "", "", "", "", "", "", "", "Vybrané soubory pro ověření musí mít stejnou příponu." });

        // Check if the extensions are allowed
        if (!AllowedFileExtensions.Contains(Path.GetExtension(fileWithoutSignature.Name))) return new Result(new[] { "", "", "", "", "", "", "Tento formát souboru není podporován.", "", "", "" });
        if (!AllowedFileExtensions.Contains(Path.GetExtension(file.Name))) return new Result(new[] { "", "", "", "", "", "", "", "Tento formát souboru není podporován.", "", "" });
        if (!AllowedCertificateExtensions.Contains(Path.GetExtension(certificate.Name))) return new Result(new[] { "", "", "", "", "", "", "", "", "Tento formát certifikátu není podporován.", "" });

        // Set the paths
        var dirPath = Path.Combine("Archive", userName, "temp", "verify");
        var filePath = Path.Combine("Archive", userName, "temp", "verify", file.Name);
        
        // Check if the directory exists
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        // Create the file and write the data into it
        File.Create(filePath).Close();
        File.WriteAllBytes(filePath, file.Data);
        
        try
        {
            var result = new Result(Services.Verify(fileWithoutSignature.Data, Metadata.GetMetadata(filePath), certificate.Data));
            File.Delete(filePath);
            return result;
        }
        catch (Exception exception)
        {
            File.Delete(filePath);
            switch (exception.Message)
            {
                case "The specified network password is not correct.":
                    return new Result(new []{ "", "", "", "", "", "", "", "", "", "Tento certifikát je zaheslovaný a nelze z něj získat veřejný klíč. Vyberte prosím certifikát s veřejným klíčem." });
                
                case "Object reference not set to an instance of an object." or "Index was outside the bounds of the array.":
                    return new Result(new[] { "", "", "", "", "", "", "", "", "", "Tento soubor nebyl podepsán naší certifikační autoritou." });

                default:
                    return new Result(new []{ "", "", "", "", "", "", "", "", "", "Error: " + exception.Message });
            }
        }
    }
}
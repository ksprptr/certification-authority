using System.Text;
using Aspose.Zip;
using Aspose.Zip.Saving;
using static CABlazorApp.Services.Properties;

namespace CABlazorApp.Services.GeneratePage;

public static class Generate
{
    public static Result GenCertificate(string userName, string name, string password)
    {
        // Check if the inputs aren't null or empty
        if (string.IsNullOrWhiteSpace(name)) return new Result(false, new[] { "", "", "", Required, "", "", "", "", "", "" });
        if (string.IsNullOrWhiteSpace(password)) return new Result(false, new[] { "", "", "", "", Required, "", "", "", "", "" });

        // Generate the certificates (private, public)
        var(privateCertificate, publicCertificate) = Services.Generate(password);
        
        // Set the file names with extensions
        var privateCertName = name + ".pfx";
        var publicCertName = name + "_public.crt";
        
        // Set the paths
        var certsPath = Path.Combine("Archive", userName, "certificates");
        var tempPath = Path.Combine("Archive", userName, "temp");
        var zipPath = Path.Combine("Archive", userName, "certificates", name + ".zip");
        var privateCertPath = Path.Combine("Archive", userName, "temp", privateCertName);
        var publicCertPath = Path.Combine("Archive", userName, "temp", publicCertName);

        // Check if the directories and files exist or not
        if (!Directory.Exists(certsPath)) Directory.CreateDirectory(certsPath);
        if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
        if (File.Exists(zipPath)) File.Delete(zipPath);

        // Create the files and write data into them
        File.Create(publicCertPath).Close();
        File.Create(publicCertPath).Close();
        File.WriteAllBytes(privateCertPath, privateCertificate);
        File.WriteAllBytes(publicCertPath, publicCertificate);
        
        // Zip the files
        using (var zipFile = File.Open(zipPath, FileMode.Create))
        {
            using (Archive archive = new())
            {
                archive.CreateEntry(privateCertName, privateCertPath);
                archive.CreateEntry(publicCertName, publicCertPath);
                archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.ASCII });
            }
        }
        
        // Delete the un-zipped files
        File.Delete(privateCertPath);
        File.Delete(publicCertPath);
        
        // Return the success
        return new Result(true);
    }
}
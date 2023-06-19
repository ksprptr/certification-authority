using System.Text;
using Aspose.Zip;
using Aspose.Zip.Saving;

namespace CABlazorApp.Services.GeneratePage;

public static class Generate
{
    private const string Required = "Toto pole je povinné.";
    public static Tuple<bool, string[]> GenCertificate(string userName, string name, string password)
    {
        // Check if the inputs aren't null or empty
        if (string.IsNullOrWhiteSpace(name)) return new Tuple<bool, string[]>(false, new[] { "", "", "", Required, "", "", "", "", "", "" });
        if (string.IsNullOrWhiteSpace(password)) return new Tuple<bool, string[]>(false, new[] { "", "", "", "", Required, "", "", "", "", "" });

        // Generate the certificates (private, public)
        var(privateCertificate, publicCertificate) = Services.Generate(password);
        
        // Set the file names with extensions
        var privateCertName = name + ".pfx";
        var publicCertName = name + "_public.crt";
        
        // Set the paths
        var dirPath = Path.Combine("Archive", userName, "certificates");
        var dirPath2 = Path.Combine("Archive", userName, "temp");
        var zipPath = Path.Combine("Archive", userName, "certificates", name + ".zip");
        var privateCertPath = Path.Combine("Archive", userName, "temp", privateCertName);
        var publicCertPath = Path.Combine("Archive", userName, "temp", publicCertName);

        // Check if the directories and files exist or not
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        if (!Directory.Exists(dirPath2)) Directory.CreateDirectory(dirPath2);
        if (File.Exists(privateCertPath)) { File.Delete(privateCertPath); File.Create(privateCertPath).Close(); }
        if (File.Exists(publicCertPath)) { File.Delete(publicCertPath); File.Create(publicCertPath).Close(); }
        if (!File.Exists(publicCertPath)) File.Create(publicCertPath).Close();
        if (!File.Exists(publicCertPath)) File.Create(publicCertPath).Close();
        if (File.Exists(zipPath)) File.Delete(zipPath);

        // Write all bytes to the files
        File.WriteAllBytes(privateCertPath, privateCertificate);
        File.WriteAllBytes(publicCertPath, publicCertificate);
        
        // Zip the files
        using (var zipFile = File.Open(zipPath, FileMode.Create))
        {
            using (var archive = new Archive())
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
        return new Tuple<bool, string[]>(true, new []{ "", "", "", "", "", "", "", "", "", "" });
    }
}
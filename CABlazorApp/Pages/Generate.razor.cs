using System.Text;
using Aspose.Zip;
using Aspose.Zip.Saving;
using CABlazorApp.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Generate
{
    // Inputs of sign
    private byte[]? _encCertFile;
    private Tuple<string, byte[]>? _encDocFile;
    private string? _encPass;

    // Inputs of generate 
    private string? _certName;
    private string? _certPass;

    // Inputs of verify
    private byte[]? _verifyFile;
    private byte[]? _verifyCert;
    private string? _verifyFileName;

    // 0 - encCertError, 1 - encPassError, 2 - encDocError, 3 - certNameError, 4 - certPassError, 5 - exceptionError, 6 - verifyFileError, 7 - verifyPublicKeyError, 8 - exceptionErrorOfVerify
    private string[]? _errors = new string[9];

    // States of successfull
    private bool _encState;
    private bool _certState;
    private int _verifyState;

    // Text of the required field
    private readonly string _required = "Toto pole je povinné.";

    private async Task Sign()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity.Name;

        if (_encCertFile == null)
        {
            _encState = false;
            _errors[0] = _required;
        }
        else if (String.IsNullOrWhiteSpace(_encPass))
        {
            _encState = false;
            _errors[1] = _required;
        }
        else if (_encDocFile == null)
        {
            _encState = false;
            _errors[2] = _required;
        }
        else
        {
            Tuple<byte[], Exception, byte[]> result = await CryptoService.Sign(_encCertFile, _encDocFile.Item2, _encPass);
            byte[] signedFile = result.Item1;
            Exception exception = result.Item2;
            byte[] signature = result.Item3;

            if (exception != null)
            {
                if (exception.Message == "The specified network password is not correct.")
                {
                    _encState = false;
                    _errors[0] = "";
                    _errors[1] = "Zadal jste špatné heslo.";
                    _errors[2] = "";
                    _errors[5] = null;
                }
                else if (exception.Message == "Cannot find the requested object.")
                {
                    _errors[0] = "";
                    _errors[1] = "";
                    _errors[2] = "";
                    _errors[5] = "Vybral jste neplatný certifikát. Vyberte prosím platný certifikát.";
                }
                else
                {
                    _errors[0] = "";
                    _errors[1] = "";
                    _errors[2] = "";
                    _errors[5] = exception.Message;
                }
            }
            else
            {
                String[] fileInfo = _encDocFile.Item1.Split('.');
                String fileName = fileInfo[0];

                string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files");
                string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files", _encDocFile.Item1);
                string zipPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files", fileName + ".zip");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    File.Create(filePath).Close();
                }

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                File.WriteAllBytes(filePath, signedFile);
                AlternateDataStream.WriteAds(filePath, "Signature", Convert.ToBase64String(signature));

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                using (FileStream zipFile = File.Open(zipPath, FileMode.Create))
                {
                    using (var archive = new Archive())
                    {
                        archive.CreateEntry(_encDocFile.Item1, filePath);
                        archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.ASCII });
                    }
                }
                
                Console.WriteLine(AlternateDataStream.ReadAds(filePath, "Signature"));

                _errors[0] = "";
                _errors[1] = "";
                _errors[2] = "";
                _encState = true;
                
                File.Delete(filePath);
            }
        }
    }

    // Generate a certificate
    private async Task GenerateCertificate()
    {
        // Get an username
        var userName = await GetUsername();
        
        // Check if the certificate name isn't empty
        if (String.IsNullOrWhiteSpace(_certName))
        {
            // Set certificate generate to un-successful
            _certState = false;
            
            // Set an error to required message
            _errors[3] = _required;
            return;
        }
        
        // Check if the certificate's password isn't empty
        if (String.IsNullOrWhiteSpace(_certPass))
        {
            // Set certificate generate to un-successful
            _certState = false;
            
            // Set the error to required message
            _errors[4] = _required;
            return;
        }
        
        // Generate the certificates (private, public)
        Tuple<byte[], byte[]> certificate = await CryptoService.Generate(_certPass);
        
        // Set the paths
        string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates");
        string dirPath2 = Path.Combine(Environment.ContentRootPath, "Archive", userName, "temp");
        string zipPath = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", _certName + ".zip");
        string certPath = Path.Combine(Environment.ContentRootPath, "Archive", userName, "temp", _certName + ".pfx");
        string filePath = Path.Combine(Environment.ContentRootPath, "Archive", userName, "temp", _certName + "_public" + ".crt");

        // Check if the directory doesn't exist
        if (!Directory.Exists(dirPath))
        {
            // Create the directory
            Directory.CreateDirectory(dirPath);
        }

        // Check if the directory doesn't exist
        if (!Directory.Exists(dirPath2))
        {
            // Create the directory
            Directory.CreateDirectory(dirPath2);
        }

        // Check if the certificate is already exist
        if (File.Exists(certPath))
        {
            // Delete the old certificate
            File.Delete(certPath);
            
            // Create the new certificate
            File.Create(certPath).Close();
        }

        // Check if the file is already exist
        if (File.Exists(filePath))
        {
            // Delete the old file
            File.Delete(filePath);
            
            // Create the new file
            File.Create(filePath).Close();
        }

        // Check if the certificate doesn't exist
        if (!File.Exists(certPath))
        {
            // Create the new certificate
            File.Create(certPath).Close();
        }

        // Check if the file doesn't exist
        if (!File.Exists(filePath))
        {
            // Create the new file
            File.Create(filePath).Close();
        }

        // Write all bytes to the files
        await File.WriteAllBytesAsync(certPath, certificate.Item1);
        await File.WriteAllBytesAsync(filePath, certificate.Item2);

        // Check if the zip file doesn't exist
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        // Zip the files
        await using (var zipFile = File.Open(zipPath, FileMode.Create))
        {
            using (var archive = new Archive())
            {
                archive.CreateEntry(_certName + ".pfx", certPath);
                archive.CreateEntry(_certName + "_public" + ".crt", filePath);
                archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.ASCII });
            }
        }

        // Clear the errors
        _errors[3] = "";
        _errors[4] = "";
        
        // Set certificate generate to successful
        _certState = true;

        // Delete the un-zipped files
        File.Delete(certPath);
        File.Delete(filePath);
    }

    // Verify method
    private async Task Verify()
    {
        // Get an username
        var userName = await GetUsername();
        
        // Set the file path
        var filePath = Path.Combine(Environment.ContentRootPath, "Archive", userName, "temp", _verifyFileName);

        // Check if the file input isn't empty
        if (_verifyFile.IsNullOrEmpty())
        {
            // Set verify state to un-successful
            _verifyState = 0;
            
            // Set an error to required message
            _errors[6] = _required;
            return;
        }
        
        // Check if the certificate input isn't empty
        if (_verifyCert.IsNullOrEmpty())
        {
            // Set verify state to un-successful
            _verifyState = 0;
            
            // Set the error to required message
            _errors[7] = _required;
            return;
        }
        
        // Check if the file is already exist
        if (File.Exists(filePath))
        {
            // Delete the old file
            File.Delete(filePath);
            
            // Create the new file & write all bytes
            File.Create(filePath).Close();
            await File.WriteAllBytesAsync(filePath, _verifyFile);
        }

        // Check if the file doesn't exist
        if (!File.Exists(filePath))
        {
            // Create the new file & write all bytes
            File.Create(filePath).Close();
            await File.WriteAllBytesAsync(filePath, _verifyFile);
        }

        try
        {
            // Try to get a signature from alternate data stream
            byte[] signature = Convert.FromBase64String(AlternateDataStream.ReadAds(filePath, "Signature"));
            
            // Verify if the signature is from certificate
            _verifyState = await CryptoService.Verify(_verifyFile, signature, _verifyCert);

            // Clear all errors
            _errors[6] = "";
            _errors[7] = "";
            _errors[8] = "";
        }
        catch (Exception e)
        {
            // If ADS doesn't exist
            if (e.Message == "Failed to open the ADS 'Signature' for '" + filePath + "'.")
            {
                // Set an error message
                _errors[8] = "Nepodařilo se najít vlastnost s názvem 'Signature'.";
                
                // Delete temp file
                File.Delete(filePath);
                return;
            }

            // Delete temp file
            File.Delete(filePath);
        }
    }

    // Download method
    private async Task Download(string fileNameWithExtension, bool file)
    {
        // Get an username
        var userName = await GetUsername();

        // Get a file type
        var fileType = MimeTypes.GetContentType(fileNameWithExtension);
        
        // Split the file name
        var splitFileName = fileNameWithExtension.Split(".");
        
        // Get the file name without an extension
        var fileName = splitFileName.Take(splitFileName.Length - 1);

        // Get a path of file
        var path = file ? Path.Combine(Environment.ContentRootPath, "Archive", userName ?? throw new InvalidOperationException(), "files", fileName + ".zip") : Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", fileNameWithExtension);
        
        // Execute JS download
        await JsRuntime.InvokeVoidAsync("Download", fileName, fileType, Convert.ToBase64String(await File.ReadAllBytesAsync(path)));
    }
    
    // Methods after input change event
    private async Task OnEncryptDocumentFile(InputFileChangeEventArgs obj)
    {
        // Create a memory stream
        await using MemoryStream stream = new MemoryStream();
        
        // Copy object to stream
        await obj.File.OpenReadStream().CopyToAsync(stream);
        
        // Save the stream
        _encDocFile = new Tuple<string, byte[]>(obj.File.Name, stream.ToArray());
    }

    private async Task OnEncryptCertFile(InputFileChangeEventArgs obj)
    {
        // Create a memory stream
        await using MemoryStream stream = new MemoryStream();
        
        // Copy object to stream
        await obj.File.OpenReadStream().CopyToAsync(stream);
        
        // Save the stream
        _encCertFile = stream.ToArray();
    }

    private async Task OnVerifyDocumentFile(InputFileChangeEventArgs obj)
    {
        // Create a memory stream
        await using MemoryStream stream = new MemoryStream();
        
        // Copy object to stream
        await obj.File.OpenReadStream().CopyToAsync(stream);
        
        // Save the stream and a file name
        _verifyFile = stream.ToArray();
        _verifyFileName = obj.File.Name;
    }
    
    private async Task OnVerifyCert(InputFileChangeEventArgs obj)
    {
        // Create a memory stream
        await using MemoryStream stream = new MemoryStream();
        
        // Copy object to stream
        await obj.File.OpenReadStream().CopyToAsync(stream);
        
        // Save the stream
        _verifyCert = stream.ToArray();
    }

    // Get a username method
    private async Task<string?> GetUsername()
    {
        // Get auth state
        var authState = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        
        // Return username as string
        return authState.User.Identity?.Name;
    }
}
using System.IO.Compression;
using System.Text;
using Aspose.Zip;
using Aspose.Zip.Saving;
using CABlazorApp.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Generate
{
    //Inputs of sign
    private byte[]? _encCertFile;
    private Tuple<string, byte[]>? _encDocFile;
    private string? _encPass;

    //Inputs of generate 
    private string? _certName;
    private string? _certPass;

    //Inputs of verify
    private byte[]? _verifyFile;
    private byte[]? _verifyCert;
    private string? _verifyFileName;

    //0 - encCertError, 1 - encPassError, 2 - encDocError, 3 - certNameError, 4 - certPassError, 5 - exceptionError, 6 - verifyFileError, 7 - verifyPublicKeyError, 8 - exceptionErrorOfVerify
    private string[]? _errors = new string[9];

    //States of successfull
    private bool _encState;
    private bool _certState;
    private int _verifyState;

    //Text of the required field
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
                String fileExtension = fileInfo[fileInfo.Length-1];
                
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
                
                using (FileStream zipFile = File.Open(zipPath, FileMode.Create))
                {
                    using (var archive = new Archive())
                    {
                        archive.CreateEntry(fileName + fileExtension, filePath);
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

    private async Task GenerateCertificate()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity.Name;

        if (String.IsNullOrWhiteSpace(_certName))
        {
            _certState = false;
            _errors[3] = _required;
        }
        else if (String.IsNullOrWhiteSpace(_certPass))
        {
            _certState = false;
            _errors[4] = _required;
        }
        else
        {
            Tuple<byte[], byte[]> certificate = await CryptoService.Generate(_certPass);
            if (name != null)
            {
                string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates");
                string dirPath2 = Path.Combine(Environment.ContentRootPath, "Archive", name, "temp");
                string zipPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates",
                    _certName + ".zip");
                string certPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "temp",
                    _certName + ".pfx");
                string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "temp",
                    _certName + "_public" + ".crt");


                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                if (!Directory.Exists(dirPath2))
                {
                    Directory.CreateDirectory(dirPath2);
                }

                if (File.Exists(certPath))
                {
                    File.Delete(certPath);
                    File.Create(certPath).Close();
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    File.Create(filePath).Close();
                }

                if (!File.Exists(certPath))
                {
                    File.Create(certPath).Close();
                }

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                File.WriteAllBytes(certPath, certificate.Item1);
                File.WriteAllBytes(filePath, certificate.Item2);

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                using (FileStream zipFile = File.Open(zipPath, FileMode.Create))
                {
                    using (var archive = new Archive())
                    {
                        archive.CreateEntry(_certName + ".pfx", certPath);
                        archive.CreateEntry(_certName + "_public" + ".crt", filePath);
                        archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.ASCII });
                    }
                }

                _errors[3] = "";
                _errors[4] = "";
                _certState = true;

                File.Delete(certPath);
                File.Delete(filePath);
            }
        }
    }

    private async Task Verify()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity.Name;
        string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "temp", _verifyFileName);
        string extractPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "temp");
        string[] splitted = _verifyFileName.Split('.');

        if (_verifyFile == null)
        {
            _verifyState = 0;
            _errors[6] = _required;
        }
        else if (_verifyCert == null)
        {
            _verifyState = 0;
            _errors[7] = _required;
        }
        else
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                File.Create(filePath).Close();
                await File.WriteAllBytesAsync(filePath, _verifyFile);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                await File.WriteAllBytesAsync(filePath, _verifyFile);
            }

            ZipFile.ExtractToDirectory(filePath, extractPath);

            string file = Path.Combine(Environment.ContentRootPath, "Archive", name, "temp", "file.txt");

            try
            {
                byte[] signature = Convert.FromBase64String(AlternateDataStream.ReadAds(file, "Signature"));
                _verifyState = await CryptoService.Verify(_verifyFile, signature, _verifyCert);

                _errors[6] = "";
                _errors[7] = "";
                _errors[8] = "";
            }
            catch (Exception e)
            {
                if (e.Message == "Failed to open the ADS 'Signature' for '" + filePath + "'.")
                {
                    _errors[8] = "Nepodařilo se najít vlastnost s názvem 'Signature'.";
                    File.Delete(filePath);
                    return;
                }

                File.Delete(filePath);
            }
        }
    }

    private async Task Download(string fileNameWithExtension, bool file)
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var userName = user.Identity.Name;
        string path;
        string fileType;
        string fileName = "";
        
        if (file)
        {
            List<string> splitedFileName = fileNameWithExtension.Split('.').ToList();
            splitedFileName.RemoveAt(splitedFileName.Count - 1);
            
            foreach (var item in splitedFileName)
            {
                if (fileName == "")
                {
                    fileName += item;
                }
                else
                {
                    fileName += "." + item;
                }
            }
            
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "files", fileName);
            fileType = "text/plain";
        }
        else
        {
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", fileNameWithExtension);
            fileType = "application/x-pkcs12";
        }

        if (fileName == "")
        {
            await JsRuntime.InvokeVoidAsync("Download", fileNameWithExtension, fileType);
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("Download", fileName, fileName, fileType);
        }
        
    }
    
    private async Task OnEncryptDocumentFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _encDocFile = new Tuple<string, byte[]>(obj.File.Name, stream.ToArray());
    }

    private async Task OnEncryptCertFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _encCertFile = stream.ToArray();
    }

    private async Task OnVerifyDocumentFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyFile = stream.ToArray();
        _verifyFileName = obj.File.Name;
    }

    private async Task OnVerifyCert(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyCert = stream.ToArray();
    }
}
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace CABlazorApp.Pages;

public partial class Generate
{
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetFileInformationByHandle(
        string hFile,
        int FileInformationClass,
        byte[] lpFileInformation,
        int dwBufferSize
    );

    const int FileExtendedAttributeInformation = 35;
    //Inputs of sign
    private byte[] _encCertFile;
    private Tuple<string, byte[]> _encDocFile;
    private string _encPass;

    //Inputs of generate 
    private string _certName;
    private string _certPass;

    //0 - encCertError, 1 - encPassError, 2 - encDocError, 3 - certNameError, 4 - certPassError, 5 - exceptionError
    private string[] _errors = new string[6];

    //States of successfull
    private bool _encState;
    private bool _certState;

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
            Tuple<byte[], Exception, byte[]> result =
                await CryptoService.Sign(_encCertFile, _encDocFile.Item2, _encPass);
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
                string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files");
                string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files",
                    _encDocFile.Item1);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    var fileSream = File.Create(filePath);
                    fileSream.Close();
                }

                if (!File.Exists(filePath))
                {
                    var fileStream = File.Create(filePath);
                    fileStream.Close();
                }

                
                File.WriteAllBytes(filePath, signedFile);

                File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
                SetFileInformationByHandle(filePath, FileExtendedAttributeInformation, signature, signature.Length);
                File.SetCreationTime(filePath, DateTime.Now);
                File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Normal);

                _errors[0] = "";
                _errors[1] = "";
                _errors[2] = "";
                _encState = true;
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
            byte[] certificate = await CryptoService.Generate(_certPass);
            string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates");
            string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates",
                _certName + ".pfx");

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                var fileSream = File.Create(filePath);
                fileSream.Close();
            }

            if (!File.Exists(filePath))
            {
                var fileStream = File.Create(filePath);
                fileStream.Close();
            }

            File.WriteAllBytes(filePath, certificate);

            _errors[3] = "";
            _errors[4] = "";
            _certState = true;
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

    private async Task Download(string fileName, bool file)
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var userName = user.Identity.Name;
        string path = String.Empty;
        string fileType = String.Empty;

        if (file)
        {
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "files", fileName);
            fileType = "text/plain";
        }
        else
        {
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", fileName);
            fileType = "application/x-pkcs12";
        }

        byte[] fileData = File.ReadAllBytes(path);
        await JsRuntime.InvokeVoidAsync("Download", fileName, fileType, Convert.ToBase64String(fileData));
    }
}
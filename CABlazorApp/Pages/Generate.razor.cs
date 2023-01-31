using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Generate
{
    //Inputs
    private byte[] _encCertFile;
    private byte[] _encDocFile;
    private string _encPass;

    private string _certName;
    private string _certPass;

    //Errors
    private string _encCertError;
    private string _encPassError;
    private string _encPassError2;
    private string _encDocError;

    private string _certNameError;
    private string _certPassError;
    
    //States
    private bool _encState;
    private bool _archState;

    private bool _certState;
    
    //Other
    private string _encDocName;
    private string _encSignedFile;
    private string _certFile;
    private string _fileInfo;
    private string _certInfo;

    private readonly string _required = "Toto pole je povinné.";

    private async Task Sign()
    {
        if (_encCertFile == null)
        {
            _encPassError2 = "";
            _encState = false;
            _encCertError = _required;
        }
        if (_encDocFile == null)
        {
            _encPassError2 = "";
            _encState = false;
            _encDocError = _required;
        }
        if (String.IsNullOrWhiteSpace(_encPass))
        {
            _encPassError2 = "";
            _encState = false;
            _encPassError = _required;
        }
        else
        {
            byte[] signedFile = await CryptoService.Sign(_encCertFile, _encDocFile, _encPass);
            if (Encoding.ASCII.GetString(signedFile) == "wrongPass")
            {
                _encState = false;
                _encCertError = "";
                _encPassError = "";
                _encDocError = "";
                _encPassError2 = "Zadal jste špatné heslo.";
            }
            else
            {
                _encPassError2 = "";
                _encCertError = "";
                _encPassError = "";
                _encDocError = "";
                _encSignedFile = Convert.ToBase64String(signedFile);
                _encState = true;
            }
        }
    }
    
    
    private async Task DownloadSigned()
    {
        await JsRuntime.InvokeVoidAsync("DownloadFile", _encDocName, "text/plain", _encSignedFile);
    }
    
    private async Task DownloadCert()
    {
        await JsRuntime.InvokeVoidAsync("DownloadFile", _certName, "pfx", _certFile);
    }

    private async Task AddToArchive()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity.Name;
        string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files");
        string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files", _encDocName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (!File.Exists(filePath))
        {
            var fileStream = File.Create(filePath);
            fileStream.Close();
            File.WriteAllBytes(filePath, Convert.FromBase64String(_encSignedFile));
        }
        else
        {
            _fileInfo = filePath;
            _encState = false;
            _archState = true;
        }
    }
    
    private async Task GenerateCertificate()
    {
        if (String.IsNullOrWhiteSpace(_certName))
        {
            _certState = false;
            _certNameError = _required;
        } 
        if (String.IsNullOrWhiteSpace(_certPass))
        {
            _certState = false;
            _certPassError = _required;
        }
        else
        {
            _certNameError = "";
            _certPassError = "";
            byte[] certificate = CryptoService.Generate(_encPass);
            _certFile = Convert.ToBase64String(certificate);
            _certState = true;
        }
        
    }

    private void ReplaceFile()
    {
        File.Delete(_fileInfo);
        var fileStream = File.Create(_fileInfo);
        fileStream.Close();
        File.WriteAllBytes(_fileInfo, Convert.FromBase64String(_encSignedFile));
    }

    private async Task OnEncryptDocumentFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _encDocFile = stream.ToArray();
        _encDocName = obj.File.Name; //doc.txt
    }

    private async Task OnEncryptCertFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _encCertFile = stream.ToArray();
    }
}
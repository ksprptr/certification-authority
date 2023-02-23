using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Generate
{
    //Inputs of sign
    private byte[] _encCertFile;
    private Tuple<string, byte[]> _encDocFile;
    private string _encPass;
    
    //Inputs of generate 
    private string _certName;
    private string _certPass;

    //0 - encCertError, 1 - encPassError, 2 - encDocError, 3 - certNameError, 4 - certPassError
    private string[] _errors = new string[6];

    //States of successfull
    private bool _encState;
    private bool _certState;
    private string _archEncState;
    private string _archCertState;

    private bool _maxFiles;
    private bool _maxCerts;
    

    //Outputs
    private byte[] _encSignedFile;
    private byte[] _certFile;
    
    //Text of the required field
    private readonly string _required = "Toto pole je povinné.";

    private async Task Sign()
    {
        if (_encCertFile == null)
        {
            _encState = false;
            _errors[0] = _required;
        }
        if (String.IsNullOrWhiteSpace(_encPass))
        {
            _encState = false;
            _errors[1] = _required;
        }
        if (_encDocFile == null)
        {
            _encState = false;
            _errors[2] = _required;
        }
        else
        {
            byte[] signedFile = await CryptoService.Sign(_encCertFile, _encDocFile.Item2, _encPass);
            if (Encoding.ASCII.GetString(signedFile) == "wrongPass")
            {
                _encState = false;
                _errors[2] = "";
                _errors[0] = "";
                _errors[1] = "Zadal jste špatné heslo.";
            }
            else
            {
                _errors[0] = "";
                _errors[1] = "";
                _errors[2] = "";
                _encSignedFile = signedFile;
                _encState = true;
                
            }
        }
    }
    
    private async Task GenerateCertificate()
    {
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
            _errors[3] = "";
            _errors[4] = "";
            _certFile = await CryptoService.Generate(_certPass);
            _certState = true;
        }
        
    }

    private async Task AddToArchive(bool file)
    {
        //Getting user
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity.Name;
        
        //Paths
        string dirPath = String.Empty;
        string filePath = String.Empty;
        if (file)
        {
            dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files");
            filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files", _encDocFile.Item1);
        }
        else
        {
            dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates");
            filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates", _certName + ".pfx");
        }
        
        int fCount = Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly).Length;
        
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (!File.Exists(filePath))
        {
            if (fCount >= 5)
            {
                if (file)
                {
                    _maxFiles = true;
                    _encState = false;
                }
                else
                {
                    _maxCerts = true;
                    _certState = false;
                }
                
            }
            else
            {
                var fileStream = File.Create(filePath);
                fileStream.Close();
                if (file)
                {
                    File.WriteAllBytes(filePath, _encSignedFile);
                }
                else
                {
                    File.WriteAllBytes(filePath, _certFile);
                }
            }
        }
        else
        {
            if (file)
            {
                _encState = false;
                _archEncState = filePath;
            }
            else
            {
                _certState = false;
                _archCertState = filePath;
            }
        }
    }

    private void ReplaceFile(string path, bool file)
    {
        File.Delete(path);
        var fileStream = File.Create(path);
        fileStream.Close();
        if (file)
        {
            File.WriteAllBytes(path, _encSignedFile); 
        }
        else
        {
            File.WriteAllBytes(path, _certFile);
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
    
    
    private async Task DownloadSigned()
    {
        await JsRuntime.InvokeVoidAsync("DownloadFile", _encDocFile.Item1, "text/plain", Convert.ToBase64String(_encSignedFile));
    }
    
    private async Task DownloadCert()
    {
        await JsRuntime.InvokeVoidAsync("DownloadPFXFile", _certName + ".pfx", _certFile);
    }
}
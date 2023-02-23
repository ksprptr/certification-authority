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

    //0 - encCertError, 1 - encPassError, 2 - encDocError, 3 - certNameError, 4 - certPassError, 5 - exceptionError
    private string[] _errors = new string[6];

    //States of successfull
    private bool _encState;
    private bool _certState;

    //Outputs
    private byte[] _encSignedFile;
    private byte[] _certFile;
    
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
                    _errors[5] = "";
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
                string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files", _encDocFile.Item1);
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
            _errors[3] = "";
            _errors[4] = "";
            byte[] certificate = await CryptoService.Generate(_certPass);
            string dirPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates");
            string filePath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates", _certName + ".pfx");
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
            _certFile = certificate;
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
    
    
    private async Task DownloadSigned()
    {
        await JsRuntime.InvokeVoidAsync("DownloadFile", _encDocFile.Item1, "text/plain", Convert.ToBase64String(_encSignedFile));
    }
    
    private async Task DownloadCert()
    {
        await JsRuntime.InvokeVoidAsync("DownloadPFXFile", _certName + ".pfx", _certFile);
    }
}
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

    //Errors
    private string _encCertError;
    private string _encPassError;
    private string _encPassError2;
    private string _encDocError;
    
    //States
    private bool _encState;
    private bool _archState;
    
    //Other
    private string _encDocName;
    private string _encSignedFile;

    private readonly string _required = "Toto pole je povinné.";

    private async Task Sign()
    {
        if (_encCertFile == null || _encDocFile == null || String.IsNullOrWhiteSpace(_encPass))
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
            _archState = true;
        }
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
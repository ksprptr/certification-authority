using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Generate
{
    private string _encDocName;
    private string _encSignedFile;
    private bool _encState;

    private byte[] _encCertFile;
    private byte[] _encDocFile;
    private string _encPass;

    private string _encCertError;
    private string _encPassError;
    private string _encDocError;

    private readonly string _required = "Toto pole je povinné.";

    private async Task Sign()
    {
        if (_encCertFile == null || _encDocFile == null || String.IsNullOrWhiteSpace(_encPass))
        {
            if (_encCertFile == null)
                _encCertError = _required;

            if (_encDocFile == null)
                _encDocError = _required;

            if (String.IsNullOrWhiteSpace(_encPass))
                _encPassError = _required;
        }
        else
        {
            byte[] signedFile = await CryptoService.Sign(_encCertFile, _encDocFile, _encPass);
            _encSignedFile = Convert.ToBase64String(signedFile);
            _encState = true;
        }
    }
    
    private async Task DownloadSigned()
    {
        await JsRuntime.InvokeVoidAsync("BlazorDownloadFile", _encDocName, "text/plain", _encSignedFile);
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
            File.Create(filePath);
            using (TextWriter tw = new StreamWriter(filePath))
            {
                tw.WriteLine(_encSignedFile);
                tw.Close();
            }
        }
        else
        {
            Console.WriteLine("Tento soubor již existuje.");
        }
    }

    private async Task OnEncryptDocumentFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _encDocFile = stream.ToArray();
        _encDocName = obj.File.Name;
    }

    private async Task OnEncryptCertFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new MemoryStream();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _encCertFile = stream.ToArray();
    }
}
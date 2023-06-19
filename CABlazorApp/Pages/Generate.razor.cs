using CABlazorApp.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using static CABlazorApp.Services.GeneratePage.Sign;
using static CABlazorApp.Services.GeneratePage.Generate;
using static CABlazorApp.Services.GeneratePage.Verify;

namespace CABlazorApp.Pages;

public partial class Generate
{
    // Inputs of sign
    private byte[]? _signCertificate;
    private string? _signCertificateName;
    private string? _signPassword;
    private byte[]? _signFile;
    private string? _signFileName;

    // Inputs of generate 
    private string? _certName;
    private string? _certPass;

    // Inputs of verify
    private byte[]? _verifyFile;
    private byte[]? _verifyFileWithoutSiganture;
    private byte[]? _verifyCertificate;
    private string? _verifyFileName;
    private string? _verifyFileWithoutSigantureName;

    // 0 - encCertError, 1 - encPassError, 2 - encDocError, 3 - certNameError,
    // 4 - certPassError, 5 - exceptionError, 6 - verifyFileWithoutSignatureError,
    // 7 - verifyFileError, 8 - verifyPublicCertificateError, 9 - exceptionErrorOfVerify
    private string[] _errors = { "", "", "", "", "", "", "", "", "", "" };

    // States of success
    private bool _signState;
    private bool _generateState;
    private bool? _verifyState;

    private async Task Sign()
    {
        var userName = await GetUsername();
        var returned = SignFile(userName!, _signCertificate!, _signCertificateName!, _signPassword!, _signFile!, _signFileName!);
        
        _signState = returned.Item1;
        _errors = returned.Item2;
    }
    
    private async Task GenerateCertificate()
    {
        // Check if certificate name doesn't contain spaces
        if (_certName != null && _certName!.Contains(' ')) { var replace = _certName.Replace(' ', '_'); _certName = replace; }
        
        var userName = await GetUsername();
        var returned = GenCertificate(userName!, _certName!, _certPass!);
        
        _generateState = returned.Item1;
        _errors = returned.Item2;
    }
    
    private async Task Verify()
    {
        // Get an username
        var userName = await GetUsername();
        var returned = VerifyFile(userName!, _verifyFile!, _verifyFileName!, _verifyCertificate!, _verifyFileWithoutSiganture!, _verifyFileWithoutSigantureName!);

        if (returned.Item1 != null) _verifyState = (bool)returned.Item1;
        _errors = returned.Item2;
    }

    // Download
    private async Task Download(string fileNameWithExtension, bool file)
    {
        var userName = await GetUsername();
        var fileType = MimeTypes.GetContentType(fileNameWithExtension);
        var path = file ? Path.Combine(Environment.ContentRootPath, "Archive", userName!, "files", fileNameWithExtension) : Path.Combine(Environment.ContentRootPath, "Archive", userName!, "certificates", fileNameWithExtension);
        await JsRuntime.InvokeVoidAsync("Download", fileNameWithExtension, fileType, Convert.ToBase64String(await File.ReadAllBytesAsync(path)));
    }
    
    // Get an username
    private async Task<string?> GetUsername()
    {
        var authState = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        return authState.User.Identity?.Name;
    }
    
    // Methods after input change event
    private async Task OnSignFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _signFile = stream.ToArray();
        _signFileName = obj.File.Name;
    }

    private async Task OnSignCertFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _signCertificate = stream.ToArray();
        _signCertificateName = obj.File.Name;
    }
    
    private async Task OnVerifyDocumentFileWithoutSignature(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyFileWithoutSiganture = stream.ToArray();
        _verifyFileWithoutSigantureName = obj.File.Name;
    }

    private async Task OnVerifyDocumentFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyFile = stream.ToArray();
        _verifyFileName = obj.File.Name;
    }
    
    private async Task OnVerifyCert(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyCertificate = stream.ToArray();
    }
}
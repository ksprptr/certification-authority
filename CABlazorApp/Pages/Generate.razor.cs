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
    private FileData? _signCertificateData;
    private string? _signPassword;
    private FileData? _signFileData;

    // Inputs of generate 
    private string? _certName;
    private string? _certPass;

    // Inputs of verify
    private FileData? _verifyFileData;
    private FileData? _verifyCertificateData;
    private FileData? _verifyFileWithoutSignatureData;

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
        var returned = SignFile(userName!, _signCertificateData!, _signPassword!, _signFileData!);
        
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
        var returned = VerifyFile(userName!, _verifyFileWithoutSignatureData!, _verifyFileData!, _verifyCertificateData!);

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
        _signFileData = new FileData(obj.File.Name, stream.ToArray());
    }

    private async Task OnSignCertFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _signCertificateData = new FileData(obj.File.Name, stream.ToArray());
    }
    
    private async Task OnVerifyDocumentFileWithoutSignature(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyFileWithoutSignatureData = new FileData(obj.File.Name, stream.ToArray());
    }

    private async Task OnVerifyDocumentFile(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyFileData = new FileData(obj.File.Name, stream.ToArray());
    }
    
    private async Task OnVerifyCert(InputFileChangeEventArgs obj)
    {
        await using MemoryStream stream = new();
        await obj.File.OpenReadStream().CopyToAsync(stream);
        _verifyCertificateData = new FileData(obj.File.Name, stream.ToArray());
    }
}
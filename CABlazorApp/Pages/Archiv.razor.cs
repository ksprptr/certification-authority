using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Archiv
{
    // List<string[]>
    public List<string> Files = new();
    public List<string> Certificates = new();
    private string _filesPath;
    private string _certPath;

    private async Task LoadFiles()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity?.Name;
        _filesPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "files");
        if (!Directory.Exists(_filesPath))
        {
            Directory.CreateDirectory(_filesPath);
        }
        List<string> fileArray = Directory.GetFiles(_filesPath).ToList();
        Files.Capacity = 5;
        // Získat velikost a jméno
        foreach (var file in fileArray)
        {
            Files.Add(file);
        }
    }

    private async Task LoadCertificates()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity?.Name;
        _certPath = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates");
        if (!Directory.Exists(_certPath))
        {
            Directory.CreateDirectory(_certPath);
        }
        List<string> certArray = Directory.GetFiles(_certPath).ToList();
        Certificates.Capacity = 5;
        foreach (var certificate in certArray)
        {
            Certificates.Add(Path.GetFileName(certificate));
        }

    }
    
    private async Task DownloadFile(string fileName)
    {
        string path = Path.Combine(_filesPath, fileName);
        string content = Convert.ToBase64String(File.ReadAllBytes(path));
        await JsRuntime.InvokeVoidAsync("DownloadFile", fileName, "text/plain", content);
        await JsRuntime.InvokeAsync<object>("location.reload");
    }

    private void DeleteFile(string fileName)
    {
        Files.Remove(fileName);
        File.Delete(Path.Combine(_filesPath, fileName));
        JsRuntime.InvokeAsync<object>("location.reload");
    }
    
    private void DeleteCert(string certName)
    {
        Certificates.Remove(certName);
        File.Delete(Path.Combine(_certPath, certName));
        JsRuntime.InvokeAsync<object>("location.reload");
    }
}
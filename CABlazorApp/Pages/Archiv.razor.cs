using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Archiv
{
    public List<Tuple<string, int>> Files = new();
    public List<Tuple<string, int>> Certificates = new();
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
        foreach (var file in fileArray)
        {
            FileInfo fi = new FileInfo(file);
            Files.Add(Tuple.Create(fi.Name, (int)fi.Length));
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
            FileInfo fi = new FileInfo(certificate);
            Certificates.Add(Tuple.Create(fi.Name, (int)fi.Length));
        }

    }
    
    private void Download(string fileName)
    {
        string path = Path.Combine(_filesPath, fileName);
        string content = Convert.ToBase64String(File.ReadAllBytes(path));
        JsRuntime.InvokeVoidAsync("DownloadFile", fileName, "text/plain", content);
        JsRuntime.InvokeAsync<object>("location.reload");
    }

    private void DeleteFile(Tuple<string, int> tuple)
    {
        Files.Remove(tuple);
        File.Delete(Path.Combine(_filesPath, tuple.Item1));
        JsRuntime.InvokeAsync<object>("location.reload");
    }
    
    private void DeleteCert(string certName)
    {
        // Certificates.Remove(certName);
        File.Delete(Path.Combine(_certPath, certName));
        JsRuntime.InvokeAsync<object>("location.reload");
    }
}
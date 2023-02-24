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
        string path = Path.Combine(Environment.ContentRootPath, "Archive", name, "files");
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        List<string> fileArray = Directory.GetFiles(path).ToList();
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
        string path = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates");
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        List<string> certArray = Directory.GetFiles(path).ToList();
        foreach (var certificate in certArray)
        {
            FileInfo fi = new FileInfo(certificate);
            Certificates.Add(Tuple.Create(fi.Name, (int)fi.Length));
        }

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
        JsRuntime.InvokeAsync<object>("location.reload");
    }

    private async Task Delete(Tuple<string, int> tuple, bool file)
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var name = user.Identity?.Name;
        string path = String.Empty;
        
        if (file)
        {
            Files.Remove(tuple);
            path = Path.Combine(Environment.ContentRootPath, "Archive", name, "files", tuple.Item1);
        }
        else
        {
            Certificates.Remove(tuple);
            path = Path.Combine(Environment.ContentRootPath, "Archive", name, "certificates", tuple.Item1);
            
        }
        
        File.Delete(path);
        JsRuntime.InvokeAsync<object>("location.reload");
    }
}
using CABlazorApp.Services;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Archiv
{
    // Lists of files and certificates
    private List<Tuple<string, int>> _files = new();
    private List<Tuple<string, int>> _certificates = new();
    
    // Paths
    private string _filesPath;
    private string _certPath;

    // Load files
    private async Task LoadFiles()
    {
        var userName = await GetUsername();
        var path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "files");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var fileArray = Directory.GetFiles(path).ToList();
        foreach (var file in fileArray)
        {
            FileInfo fi = new FileInfo(file);
            _files.Add(Tuple.Create(fi.Name, (int)fi.Length));
        }
    }

    // Load certificates
    private async Task LoadCertificates()
    {
        var userName = await GetUsername();
        var path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var certArray = Directory.GetFiles(path).ToList();
        foreach (var certificate in certArray)
        {
            FileInfo fi = new FileInfo(certificate);
            _certificates.Add(Tuple.Create(fi.Name, (int)fi.Length));
        }
    }
    
    // Download
    private async Task Download(string fileName, bool file)
    {
        var userName = await GetUsername();
        var fileType = MimeTypes.GetContentType(fileName);
        var path = file ? Path.Combine(Environment.ContentRootPath, "Archive", userName, "files", fileName) : Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", fileName);
        var fileData = await File.ReadAllBytesAsync(path);
        await JsRuntime.InvokeVoidAsync("Download", fileName, fileType, Convert.ToBase64String(fileData));
        JsRuntime.InvokeAsync<object>("location.reload");
    }

    // Delete
    private async Task Delete(Tuple<string, int> tuple, bool file)
    {
        var userName = await GetUsername();
        var path = "";
        if (file)
        {
            _files.Remove(tuple);
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "files", tuple.Item1);
        }
        else
        {
            _certificates.Remove(tuple);
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", tuple.Item1);
        }
        File.Delete(path);
        JsRuntime.InvokeAsync<object>("location.reload");
    }
    
    // Get an username
    private async Task<string?> GetUsername()
    {
        var authState = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        return authState.User.Identity?.Name;
    }
}
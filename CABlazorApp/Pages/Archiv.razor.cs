using CABlazorApp.Services;
using Microsoft.JSInterop;

namespace CABlazorApp.Pages;

public partial class Archiv
{
    // List of files
    public List<Tuple<string, int>> Files = new();
    
    // List of certificates
    public List<Tuple<string, int>> Certificates = new();
    
    // Paths
    private string _filesPath;
    private string _certPath;

    // Load files on page load
    private async Task LoadFiles()
    {
        // Get an username
        var userName = await GetUsername();
        
        // Set a path
        var path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "files");
        
        // Check if directory doesn't exist
        if (!Directory.Exists(path))
        {
            // Create a new directory
            Directory.CreateDirectory(path);
        }
        
        // Get files to a list
        List<string> fileArray = Directory.GetFiles(path).ToList();
        
        // Add an each file to Files list
        foreach (var file in fileArray)
        {
            FileInfo fi = new FileInfo(file);
            Files.Add(Tuple.Create(fi.Name, (int)fi.Length));
        }
    }

    // Load certificates on page load
    private async Task LoadCertificates()
    {
        // Get the username
        var userName = await GetUsername();
        
        // Set the path
        var path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates");
        
        // Check if directory doesn't exist
        if (!Directory.Exists(path))
        {
            // Create the new direcotry
            Directory.CreateDirectory(path);
        }
        
        // Get certificates to the list
        List<string> certArray = Directory.GetFiles(path).ToList();
        
        // Add the each certificate to Certificates list
        foreach (var certificate in certArray)
        {
            FileInfo fi = new FileInfo(certificate);
            Certificates.Add(Tuple.Create(fi.Name, (int)fi.Length));
        }
    }
    
    // Download method
    private async Task Download(string fileName, bool file)
    {
        // Get an username
        var userName = await GetUsername();
        
        // Get a file type
        var fileType = MimeTypes.GetContentType(fileName);
        
        // Set a path
        var path = file ? Path.Combine(Environment.ContentRootPath, "Archive", userName, "files", fileName) : Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", fileName);
        
        //  Get file data to array
        byte[] fileData = await File.ReadAllBytesAsync(path);
        
        // Execute JS Download
        await JsRuntime.InvokeVoidAsync("Download", fileName, fileType, Convert.ToBase64String(fileData));
        
        // Invoke async (page content reload)
        JsRuntime.InvokeAsync<object>("location.reload");
    }

    // Delete method
    private async Task Delete(Tuple<string, int> tuple, bool file)
    {
        // Get an username
        var userName = await GetUsername();
        
        // Create a path string
        var path = "";
        
        // Check if it is the file or the certificate
        if (file)
        {
            // Remove the file from the list
            Files.Remove(tuple);
            
            // Set the path to files path
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "files", tuple.Item1);
        }
        else
        {
            // Remove the certificate from the list
            Certificates.Remove(tuple);
            
            // Set the path to certificates path
            path = Path.Combine(Environment.ContentRootPath, "Archive", userName, "certificates", tuple.Item1);
            
        }
        
        // Delete the file
        File.Delete(path);
        
        // Invoke async (page content reload)
        JsRuntime.InvokeAsync<object>("location.reload");
    }
    
    // Get an username method
    private async Task<string?> GetUsername()
    {
        // Get auth state
        var authState = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        
        // Return username as string
        return authState.User.Identity?.Name;
    }
}
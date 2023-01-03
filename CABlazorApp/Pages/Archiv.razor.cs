using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Hosting;

namespace CABlazorApp.Pages;

public class Archiv2 : PageModel
{
    [Inject]
    public IWebHostEnvironment Environment { get; set; }
    
    [Inject]
    public AuthenticationStateProvider GetAuthenticationStateAsync { get; set; }

    public async Task<string> UserLoad()
    {
        var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        var user = authstate.User;
        var username = user.Identity?.Name;
        return username;
    }
    
    public List<string> Test()
    {
        string path = Path.Combine(Environment.WebRootPath, "wwwroot", "uploads", "files");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        List<string> fileArray = Directory.GetFiles(path).ToList();
        List<string> files = new List<string>();
        foreach (var file in fileArray)
        {
            string[] temp = file.Split('\\');
            files.Add(temp.Last());
        }
        return files;
    }
}
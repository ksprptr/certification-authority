using System.ComponentModel.DataAnnotations;
using CABlazorApp.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CABlazorApp.Data;
using Microsoft.AspNetCore.Components.Forms;
using static Microsoft.AspNetCore.Components.NavigationManager;

namespace CABlazorApp.Data;

[ApiController]
[Route("api")]
public class API : Controller
{
    [HttpPost]
    private async Task Method(RequestBody requestBody)
    {
        IFormFile file = Request.Form.Files[0];

        using (Stream stream = file.OpenReadStream())
        using (StreamReader reader = new StreamReader(stream)) {
            string data = await reader.ReadToEndAsync();
        }

        string error = "";

        if ()
        {
            
        }

        var fileToENC = requestBody.fileToEncrypt;
        var certificate = requestBody.fileToEncrypt;

        string filePath = Path.Combine();
        string certPath = Path.Combine();
        await using FileStream fs = new(filePath, FileMode.Create);
        await using FileStream fs2 = new(certPath, FileMode.Create);
        await fileToENC.OpenReadStream(1024 * 15).CopyToAsync(fs);
        await certificate.OpenReadStream(1024 * 15).CopyToAsync(fs2);
        await Enc.Encrypt($"{filePath}", $"{certificate}", $"{requestBody.password}");
        if (error != null)
        {
            Response.Redirect($"/generate?error={error}");
        }
        else
        {
            Response.Redirect($"/generate");
        }
        
    }

    // private async Task Decrypt()
    // {
    //     await Enc.Decrypt("Document.docx", "certifikat.pfx", "1234.");
    // }
    //
}

public class RequestBody
{
    [Required]
    public IBrowserFile fileToEncrypt { get; set; }
    
    [Required]
    public string password { get; set; }
    
    [Required]
    public IBrowserFile certToEncrypt { get; set; }
}
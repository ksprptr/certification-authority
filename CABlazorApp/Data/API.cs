using System.ComponentModel.DataAnnotations;
using CABlazorApp.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace CABlazorApp.Data
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api")]
    public class Api : Controller
    {
        public Api(IWebHostEnvironment environment, AuthenticationStateProvider getAuthenticationStateAsync)
        {
            Environment = environment;
            GetAuthenticationStateAsync = getAuthenticationStateAsync;
        }
        

        [Inject]
        private IWebHostEnvironment Environment { get; set; }
        
        [Inject] 
        private AuthenticationStateProvider GetAuthenticationStateAsync { get; set; }

        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("method")]
        private void Method()
        {

            Console.WriteLine("test");
            // IFormFile file = Request.Form.Files[0];
            //
            // using (Stream stream = file.OpenReadStream())
            // using (StreamReader reader = new StreamReader(stream)) {
            //    await string data = reader.ReadToEndAsync();
            // }

            string error = "";
            
        //
        //     var fileToEnc = null;
        //     var certificate = null;
        //     var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
        //     var user = authstate.User;
        //     var name = user.Identity?.Name;
        //     if (name != null)
        //     {
        //         string filePath = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", name, "files", file.Name);
        //         string fileDir = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", name, "files");
        //         string certPath = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", name, certificate.Name);
        //         string certDir = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", name);
        //
        //         if (!Directory.Exists(fileDir))
        //         {
        //             Directory.CreateDirectory(fileDir);
        //         } else if (Directory.Exists(certDir))
        //         {
        //             Directory.CreateDirectory(certDir);
        //         }
        //         
        //         await using FileStream fs = new(filePath, FileMode.Create);
        //         await using FileStream fs2 = new(certPath, FileMode.Create);
        //         await fileToEnc.OpenReadStream(1024 * 15).CopyToAsync(fs);
        //         await certificate.OpenReadStream(1024 * 15).CopyToAsync(fs2);
        //         await Enc.Encrypt($"{filePath}", $"{certificate}", $"{requestBody.PasswordEncryptInput}");
        //         Response.Redirect(error != "" ? $"/generate?error={error}" : $"/result");
        //     }
        }

        // private async Task Decrypt()
        // {
        //     await Enc.Decrypt("Document.docx", "certifikat.pfx", "1234.");
        // }
        //
    }

    public class RequestBody
    {
        public RequestBody(IBrowserFile fileToEncrypt, string password, IBrowserFile certToEncrypt)
        {
            this.FileEncryptInput = fileToEncrypt;
            this.PasswordEncryptInput = password;
            this.CertEncryptInput = certToEncrypt;
        }

        [Required]
        public IBrowserFile FileEncryptInput { get; set; }
        
        [Required]
        public string PasswordEncryptInput { get; set; }
        
        [Required]
        public IBrowserFile CertEncryptInput { get; set; }
    }   
}
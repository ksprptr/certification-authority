using System.ComponentModel.DataAnnotations;
using CABlazorApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace CABlazorApp.Data
{
    [Microsoft.AspNetCore.Mvc.Route("api")]
    [ApiController]
    public class Api : Controller
    {
        public Api(IWebHostEnvironment environment, AuthenticationStateProvider getAuthenticationStateAsync)
        {
            Environment = environment;
            GetAuthenticationStateAsync = getAuthenticationStateAsync;
        }

        [Inject] private IWebHostEnvironment Environment { get; set; }

        [Inject] public AuthenticationStateProvider GetAuthenticationStateAsync { get; set; }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("encrypt")]
        
        public async Task Encrypt([FromForm] RequestBody requestBody)
        {
            string error = "";
            var file = requestBody.FileEncryptInput;
            var certificate = requestBody.CertEncryptInput;
            string password = requestBody.PasswordEncryptInput;
            // var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();
            // var user = authstate.User;
            // var name = user.Identity?.Name;
            string filePath = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", "files", file.FileName);
            string fileDir = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", "files");
            string certPath = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads", certificate.FileName);
            string certDir = Path.Combine(Environment.ContentRootPath, "wwwroot", "uploads");

            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }
            else if (Directory.Exists(certDir))
            {
                Directory.CreateDirectory(certDir);
            }

            await using FileStream fs = new(filePath, FileMode.Create);
            await using FileStream fs2 = new(certPath, FileMode.Create);
            await file.OpenReadStream().CopyToAsync(fs);
            await certificate.OpenReadStream().CopyToAsync(fs2);
            Response.Redirect(error != "" ? $"/generate?error={error}" : "/result");
        }

        public class RequestBody
        {

            [Required(ErrorMessage = "Toto pole je povinné.")]
            public IFormFile FileEncryptInput { get; set; }

            [Required(ErrorMessage = "Toto pole je povinné")]
            public string PasswordEncryptInput { get; set; }

            [Required(ErrorMessage = "Toto pole je povinné.")]
            public IFormFile CertEncryptInput { get; set; }
        }
    }
}
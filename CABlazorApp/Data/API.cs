using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

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
        
        public string GenerateErrorMessage1 { get; set; }

        [Inject] public AuthenticationStateProvider GetAuthenticationStateAsync { get; set; }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("encrypt")]
        
        public async Task Encrypt([FromForm] RequestBodyEncrypt requestBody)
        {
            var file = requestBody.FileInputEnc;
            var certificate = requestBody.CertInputEnc;
            string password = requestBody.PassInputEnc;
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
            await Enc.Encrypt($"{filePath}", $"{certPath}", $"{password}");
        }
        
        [Microsoft.AspNetCore.Mvc.Route("decrypt")]
        public async Task Decrypt([FromForm] RequestBodyGenerate requestBody)
        {
            string name = requestBody.CertNameInput;
            string password = requestBody.CertPassInput;
        }
        
        [Microsoft.AspNetCore.Mvc.Route("generate")]
        public async Task Generate([FromForm] RequestBodyGenerate requestBody)
        {
            string name = requestBody.CertNameInput;
            string password = requestBody.CertPassInput;
            Console.WriteLine(name);
        }

        public class RequestBodyEncrypt
        {

            [Required(ErrorMessage = "Toto pole je povinné.")]
            public IFormFile CertInputEnc { get; set; }

            [Required(ErrorMessage = "Toto pole je povinné")]
            public string PassInputEnc { get; set; }

            [Required(ErrorMessage = "Toto pole je povinné.")]
            public IFormFile FileInputEnc { get; set; }
        }
        
        public class RequestBodyDecrypt
        {

            [Required(ErrorMessage = "Toto pole je povinné.")]
            public IFormFile CertInputDec { get; set; }

            [Required(ErrorMessage = "Toto pole je povinné")]
            public string PassInputDec { get; set; }

            [Required(ErrorMessage = "Toto pole je povinné.")]
            public IFormFile FileInputDec { get; set; }
        }

        public class RequestBodyGenerate
        {
            [Required(ErrorMessage = "Toto pole je povinné.")]
            public string CertNameInput { get; set; }
            
            [Required(ErrorMessage = "Toto pole je povinné.")]
            public string CertPassInput { get; set; }
        }
    }
}
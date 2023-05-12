// public class DownloadFile : IHttpHandler
// {
//     public void ProcessRequest(HttpContext context)
//     {
//         string filePath = context.Request.QueryString["filePath"];
//         string fileName = Path.GetFileName(filePath);
//
//         byte[] fileBytes = File.ReadAllBytes(filePath);
//
//         // Přidej Alternate Data Stream do dat souboru
//         byte[] signature = GetAlternateDataStream(filePath, "Signature");
//         if (signature != null)
//         {
//             using (MemoryStream ms = new MemoryStream(fileBytes, true))
//             {
//                 ms.Seek(0, SeekOrigin.End);
//                 ms.Write(signature, 0, signature.Length);
//             }
//         }
//
//         // Nastav hlavičky odpovědi
//         context.Response.Clear();
//         context.Response.ContentType = "application/octet-stream";
//         context.Response.AppendHeader("Content-Disposition", $"attachment; filename={fileName}");
//         context.Response.AppendHeader("Content-Length", fileBytes.Length.ToString());
//
//         // Odešli data souboru včetně Alternate Data Streamu
//         context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
//         context.Response.Flush();
//     }
//
//     public bool IsReusable
//     {
//         get { return false; }
//     }
//
//     private byte[] GetAlternateDataStream(string filePath, string streamName) 
//     {
//         try
//         {
//             using (FileStream fs = new FileStream(filePath + ":" + streamName, FileMode.Open, FileAccess.Read))
//             using (MemoryStream ms = new MemoryStream())
//             {
//                 fs.CopyTo(ms);
//                 return ms.ToArray();
//             }
//         }
//         catch (Exception)
//         {
//             // ADS neexistuje
//             return null;
//         }
//     }
// }

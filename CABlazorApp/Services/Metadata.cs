using Aspose.Pdf;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CABlazorApp.Services;

public class Metadata
{
    public static bool SetMetadata(string fileName, string filePath, byte[] signature)
    {
        var fileType = MimeTypes.GetContentType(fileName);

        switch (fileType)
        {
            case "text/plain":
                using (var sw = File.AppendText(filePath))
                {
                    sw.Write($"\n\n------ DON'T DELETE ------\nSignature: {Convert.ToBase64String(signature)}\n------ DON'T DELETE ------\n");
                }
                return true;
            
            case "application/octet-stream":
                return true;
            
            case "application/pdf":
                SetPdfMetadata(filePath, "Signature", signature);
                Console.WriteLine("Metadata set successfully.");
                return true;
            
            case "application/excel":
                return true;

            default:
                return false;
        }
    }
    
    public static byte[]? GetMetadata(string filePath, string propertyName)
    {
        var fileType = MimeTypes.GetContentType(filePath);

        switch (fileType)
        {
            case "text/plain":
                var file = File.ReadAllText(filePath);
                var signature = file.Split("Signature: ")[1];
                return Convert.FromBase64String(signature);
            
            default:
                return null;
        }
    }

    private static void SetPdfMetadata(string filePath, string name, byte[] value)
    {
        Aspose.Pdf.Document pdfDoc = new(filePath);
        var customMetadata = new KeyValuePair<string, XmpValue>("Signature", new XmpValue(Convert.ToBase64String(value)));
        pdfDoc.Metadata.Add("CustomMetaData", customMetadata);
        pdfDoc.Save();
        Console.WriteLine(pdfDoc.Metadata["CustomMetaData"]);
    }
}
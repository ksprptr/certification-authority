using Aspose.Pdf;

namespace CABlazorApp.Services;

public static class Metadata
{
    public static bool SetMetadata(string filePath, byte[] signature)
    {
        var fileType = MimeTypes.GetContentType(Path.GetFileName(filePath));

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
                SetPdfMetadata(filePath, signature);
                return true;
            
            case "application/excel":
                return true;

            default:
                return false;
        }
    }
    
    public static byte[]? GetMetadata(string filePath)
    {
        var fileType = MimeTypes.GetContentType(Path.GetFileName(filePath));

        switch (fileType)
        {
            case "text/plain":
                var file = File.ReadAllText(filePath);
                var signature = file.Split("Signature: ")[1];
                return Convert.FromBase64String(signature);
            
            case "application/octet-stream":
                return null;
            
            case "application/pdf":
               return GetPdfMetadata(filePath);
            
            case "application/excel":
                return null;
            
            default:
                return null;
        }
    }

    private static void SetPdfMetadata(string filePath, byte[] value)
    {
        Document pdfDoc = new(filePath);
        pdfDoc.Metadata["xmp:Signature"] = Convert.ToBase64String(value);
        pdfDoc.Save(filePath);
    }
    
    private static byte[] GetPdfMetadata(string filePath)
    {
        Document pdfDoc = new(filePath);
        return Convert.FromBase64String(pdfDoc.Metadata["xmp:Signature"].ToString());
    }
}
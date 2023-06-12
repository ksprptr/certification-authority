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
                var signature = file.Split("------ DON'T DELETE ------\nSignature: ")[1].Split("\n------ DON'T DELETE ------\n")[0];
                return Convert.FromBase64String(signature);
            
            default:
                return null;
        }
    }
}
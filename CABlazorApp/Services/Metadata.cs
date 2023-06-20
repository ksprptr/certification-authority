using Aspose.Cells;
using Aspose.Pdf;
using Aspose.Slides;
using SaveFormat = Aspose.Slides.Export.SaveFormat;

namespace CABlazorApp.Services;

public static class Metadata
{
    public static bool SetMetadata(string filePath, byte[] signature)
    {
        var fileType = MimeTypes.GetContentType(Path.GetFileName(filePath));
        
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(fileName);

        fileType = fileExtension switch
        {
            ".docx" => "application/msword",
            ".pptx" => "application/mspowerpoint",
            _ => fileType
        };

        switch (fileType)
        {
            // .txt
            case "text/plain":
                using (var sw = File.AppendText(filePath))
                {
                    sw.Write($"\n\n------ DON'T DELETE ------\nSignature: {Convert.ToBase64String(signature)}\n------ DON'T DELETE ------\n");
                }
                return true;
            
            // .csv
            case "application/octet-stream":
                using (var sw = File.AppendText(filePath))
                {
                    sw.WriteLine("\n\n------ DON'T DELETE ------");
                    sw.WriteLine($"Signature: {Convert.ToBase64String(signature)}");
                    sw.WriteLine("------ DON'T DELETE ------");
                }
                return true;
            
            // .pdf
            case "application/pdf":
                SetPdfMetadata(filePath, signature);
                return true;
            
            // .xls, .xlsx
            case "application/excel":
                SetExcelMetadata(filePath, signature);
                return true;
            
            // .doc, .docx
            case "application/msword":
                SetWordMetadata(filePath, signature);
                return true;
            
            // .ppt, .pptx
            case "application/mspowerpoint":
                SetPowerPointMetadata(filePath, signature, fileExtension);
                return true;

            default:
                return false;
        }
    }
    
    public static byte[]? GetMetadata(string filePath)
    {
        var fileType = MimeTypes.GetContentType(Path.GetFileName(filePath));
        
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(fileName);

        fileType = fileExtension switch
        {
            ".docx" => "application/msword",
            ".pptx" => "application/mspowerpoint",
            _ => fileType
        };

        switch (fileType)
        {
            // .txt
            case "text/plain":
                var file = File.ReadAllText(filePath);
                var signature = file.Split("Signature: ")[1].Split('\n')[0];
                return Convert.FromBase64String(signature);
            
            // .csv
            case "application/octet-stream":
                var fileLines = File.ReadAllText(filePath);
                var signatureCsv = fileLines.Split("Signature: ")[1].Split('\n')[0];
                return Convert.FromBase64String(signatureCsv);

            // .pdf
            case "application/pdf":
               return GetPdfMetadata(filePath);
            
            // .xls, .xlsx
            case "application/excel":
                return GetExcelMetadata(filePath);

            // .doc, .docx
            case "application/msword":
                return GetWordMetadata(filePath);
            
            // .ppt, .pptx
            case "application/mspowerpoint":
                return GetPowerPointMetadata(filePath);

            default:
                return null;
        }
    }

    // PDF
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
    
    // Excel
    private static void SetExcelMetadata(string filePath, byte[] value)
    {
        var workbook = new Workbook(filePath);
        Workbook newWorkbook = new();
        newWorkbook.Copy(workbook);
        var properties = newWorkbook.CustomDocumentProperties;
        properties.Add("Signature", Convert.ToBase64String(value));
        newWorkbook.Save(filePath);
    }
    
    private static byte[] GetExcelMetadata(string filePath)
    {
        var workbook = new Workbook(filePath);
        var properties = workbook.CustomDocumentProperties;
        return Convert.FromBase64String(properties["Signature"].Value.ToString());
    }
    
    // Word
    private static void SetWordMetadata(string filePath, byte[] value)
    {
        Aspose.Words.Document doc = new(filePath);
        doc.CustomDocumentProperties.Add("Signature", Convert.ToBase64String(value));
        doc.Save(filePath);
    }
    
    private static byte[] GetWordMetadata(string filePath)
    {
        Aspose.Words.Document doc = new(filePath);
        return Convert.FromBase64String(doc.CustomDocumentProperties["Signature"].Value.ToString());
    }
    
    // PowerPoint
    private static void SetPowerPointMetadata(string filePath, byte[] value, string fileType)
    {
        Presentation pptxDoc = new(filePath);
        var pptxProps = pptxDoc.DocumentProperties;
        pptxProps["Signature"] = Convert.ToBase64String(value);
        pptxDoc.Save(filePath, fileType == ".pptx" ? SaveFormat.Pptx : SaveFormat.Ppt);
    }
    
    private static byte[] GetPowerPointMetadata(string filePath)
    {
        Presentation pptxDoc = new(filePath);
        var pptxProps = pptxDoc.DocumentProperties;
        return Convert.FromBase64String(pptxProps["Signature"].ToString());
    }
}
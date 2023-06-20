using Aspose.Cells;
using Aspose.Pdf;
using Aspose.Slides;
using SaveFormat = Aspose.Slides.Export.SaveFormat;

namespace CABlazorApp.Services;

public static class Metadata
{
    public static void SetMetadata(string filePath, byte[] signature)
    {
        var fileExtension = Path.GetExtension(Path.GetFileName(filePath));

        switch (fileExtension)
        {
            case ".txt":
                SetTxtMetadata(filePath, signature);
                return;

            case ".csv":
                SetCsvMetadata(filePath, signature);
                return;
                
            case ".pdf":
                SetPdfMetadata(filePath, signature);
                return;
            
            case ".xls" or ".xlsx":
                SetExcelMetadata(filePath, signature);
                return;
            
            case ".doc" or ".docx":
                SetWordMetadata(filePath, signature);
                return;
            
            case ".ppt" or ".pptx":
                SetPowerPointMetadata(filePath, signature, fileExtension);
                return;
            
            default:
                return;
        }
    }
    
    public static byte[]? GetMetadata(string filePath)
    {
        var fileExtension = Path.GetExtension(Path.GetFileName(filePath));

        switch (fileExtension)
        {
            case ".txt":
                return GetTxtMetadata(filePath);

            case ".csv":
                return GetCsvMetadata(filePath);
                
            case ".pdf":
                return GetPdfMetadata(filePath);
            
            case ".xls" or ".xlsx":
                return GetExcelMetadata(filePath);
            
            case ".doc" or ".docx":
                return GetWordMetadata(filePath);
            
            case ".ppt" or ".pptx":
                return GetPowerPointMetadata(filePath);
            
            default:
                return null;
        }
    }

    // Txt
    private static void SetTxtMetadata(string filePath, byte[] value)
    {
        using var sw = File.AppendText(filePath);
        sw.Write($"\n\n------ DON'T DELETE ------\nSignature: {Convert.ToBase64String(value)}\n------ DON'T DELETE ------\n");
    }
    
    private static byte[] GetTxtMetadata(string filePath)
    {
        var allText = File.ReadAllText(filePath);
        var signature = allText.Split("Signature: ")[1].Split('\n')[0];
        return Convert.FromBase64String(signature);
    }
    
    // Csv
    private static void SetCsvMetadata(string filePath, byte[] value)
    {
        using var sw = File.AppendText(filePath);
        sw.WriteLine("\n\n------ DON'T DELETE ------");
        sw.WriteLine($"Signature: {Convert.ToBase64String(value)}");
        sw.WriteLine("------ DON'T DELETE ------");
    }
    
    private static byte[] GetCsvMetadata(string filePath)
    {
        var allText = File.ReadAllText(filePath);
        var signature = allText.Split("Signature: ")[1].Split('\n')[0];
        return Convert.FromBase64String(signature);
    }

    // Pdf
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
namespace CABlazorApp.Services;

public class FileData
{
    public FileData(string fileName, byte[] data)
    {
        FileName = fileName;
        Data = data;
    }

    public string FileName { get; set; }
    public byte[] Data { get; set; }
}
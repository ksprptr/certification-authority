namespace CABlazorApp.Services;

public class FileData
{
    public string Name { get; set; }
    public byte[] Data { get; set; }
    
    public FileData(string fileName, byte[] data)
    {
        Name = fileName;
        Data = data;
    }
}
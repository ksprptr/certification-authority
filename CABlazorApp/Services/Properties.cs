namespace CABlazorApp.Services;

public static class Properties
{
    public const string Required = "Toto pole je povinné.";
    public static readonly List<string> AllowedFileExtensions = new() { ".docx", ".docm", ".doc", ".pdf", ".pptx", ".pptm", ".ppt", ".xlsx", ".xlsm", ".xls", ".csv", ".txt" };
    public static readonly List<string> AllowedCertificateExtensions = new() { ".crt", ".cer", ".p7b", ".p7c", ".p7s", ".pem", ".p12", ".pfx" };
}
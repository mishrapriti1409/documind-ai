namespace DocuMind.Application.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string StorageType { get; set; } = "local";
    public string BasePath { get; set; } = "./storage/documents";
    public string? AzureConnectionString { get; set; }
    public string AzureContainerName { get; set; } = "documind-documents";
}


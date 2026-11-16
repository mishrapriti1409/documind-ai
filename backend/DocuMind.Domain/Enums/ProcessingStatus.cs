namespace DocuMind.Domain.Enums;

/// <summary>
/// Status of document ingestion and vector embedding processing.
/// </summary>
public enum ProcessingStatus
{
    Uploaded,
    Pending,
    Processing,
    Completed,
    Failed
}


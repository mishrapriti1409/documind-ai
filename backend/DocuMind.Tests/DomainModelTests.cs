using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;
using Xunit;

namespace DocuMind.Tests;

public class DomainModelTests
{
    [Fact]
    public void Document_Initialization_SetsDefaultsCorrectly()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            UploadedByUserId = userId,
            OriginalFileName = "Contract_Q4.pdf",
            StoragePath = "uploads/org1/Contract_Q4.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024 * 500,
            ProcessingStatus = ProcessingStatus.Pending
        };

        // Assert
        Assert.NotEqual(Guid.Empty, doc.Id);
        Assert.Equal("Contract_Q4.pdf", doc.OriginalFileName);
        Assert.Equal(ProcessingStatus.Pending, doc.ProcessingStatus);
        Assert.Equal(0, doc.TotalPages);
        Assert.Equal(0, doc.TotalChunks);
        Assert.Null(doc.DeletedAt);
    }

    [Fact]
    public void DocumentChunk_AssignsTokenCountAndEmbedding_Successfully()
    {
        // Arrange
        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            ChunkIndex = 0,
            PageNumber = 1,
            Content = "This is a confidential nondisclosure agreement between Acme Corp and Partner.",
            TokenCount = 14,
            Metadata = "{\"section\":\"1. Confidentiality\"}",
            Embedding = new float[] { 0.01f, 0.05f, -0.03f }
        };

        // Assert
        Assert.Equal(0, chunk.ChunkIndex);
        Assert.Equal(1, chunk.PageNumber);
        Assert.Equal(14, chunk.TokenCount);
        Assert.NotNull(chunk.Embedding);
        Assert.Equal(3, chunk.Embedding.Length);
        Assert.Contains("Confidentiality", chunk.Metadata);
    }

    [Fact]
    public void Conversation_TracksMessagesAndScope()
    {
        // Arrange
        var conv = new Conversation
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Contract Analysis Discussion",
            Scope = ConversationScope.SingleDocument
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            Role = MessageRole.User,
            Content = "What is the termination clause in Section 4?",
            TokensUsed = 25
        };

        conv.Messages.Add(message);

        // Assert
        Assert.Single(conv.Messages);
        Assert.Equal(MessageRole.User, conv.Messages.First().Role);
        Assert.Equal(ConversationScope.SingleDocument, conv.Scope);
    }

    [Fact]
    public void AuditLog_StoresActionAndActor()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Action = "DocumentUploaded",
            ResourceType = "Document",
            ResourceId = resourceId,
            IpAddress = "192.168.1.1",
            NewValues = "{\"fileName\":\"policy.pdf\"}"
        };

        // Assert
        Assert.Equal("DocumentUploaded", audit.Action);
        Assert.Equal("Document", audit.ResourceType);
        Assert.Equal(resourceId, audit.ResourceId);
        Assert.Contains("policy.pdf", audit.NewValues);
    }
}

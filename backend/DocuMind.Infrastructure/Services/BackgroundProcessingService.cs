using DocuMind.Application.Interfaces;
using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;
using DocuMind.Infrastructure.Data;
using DocuMind.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DocuMind.Infrastructure.Services;

/// <summary>
/// Background worker that monitors ProcessingJobs queue and dispatches
/// documents to the Python FastAPI microservice for parsing, chunking, and embedding.
/// </summary>
public sealed class BackgroundProcessingService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BackgroundProcessingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public BackgroundProcessingService(IServiceProvider services, ILogger<BackgroundProcessingService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background document processing worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingQueueAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Unhandled error in background processing loop");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Background document processing worker stopping.");
    }

    private async Task ProcessPendingQueueAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var docRepo = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var aiClient = scope.ServiceProvider.GetRequiredService<IAIServiceClient>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingJobs = (await docRepo.GetProcessingQueueAsync(batchSize: 5, ct)).ToList();
        if (!pendingJobs.Any()) return;

        _logger.LogInformation("Processing {Count} pending document jobs", pendingJobs.Count);

        foreach (var job in pendingJobs)
        {
            if (ct.IsCancellationRequested) break;
            await ProcessSingleJobAsync(job, aiClient, db, ct);
        }
    }

    private async Task ProcessSingleJobAsync(
        ProcessingJob job,
        IAIServiceClient aiClient,
        AppDbContext db,
        CancellationToken ct)
    {
        job.Status = "Processing";
        job.StartedAt = DateTimeOffset.UtcNow;
        job.AttemptCount++;
        await db.SaveChangesAsync(ct);

        var doc = job.Document;
        if (doc == null)
        {
            job.Status = "Failed";
            job.ErrorMessage = "Associated document not found";
            job.CompletedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        doc.ProcessingStatus = ProcessingStatus.Processing;
        await db.SaveChangesAsync(ct);

        try
        {
            var result = await aiClient.ProcessDocumentAsync(
                doc.Id,
                doc.OrganizationId,
                doc.StoragePath,
                doc.ContentType,
                ct);

            job.Status = "Completed";
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = null;

            doc.ProcessingStatus = ProcessingStatus.Completed;
            doc.TotalChunks = result.ChunksCreated;
            doc.TotalPages = result.PagesProcessed;
            doc.ProcessedAt = DateTimeOffset.UtcNow;
            doc.ProcessingError = null;

            _logger.LogInformation("Document {DocId} processed successfully: {Pages} pages, {Chunks} chunks",
                doc.Id, result.PagesProcessed, result.ChunksCreated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocId} (Attempt {Attempt})", doc.Id, job.AttemptCount);

            if (job.AttemptCount >= 3)
            {
                job.Status = "Failed";
                doc.ProcessingStatus = ProcessingStatus.Failed;
            }
            else
            {
                job.Status = "Pending"; // will retry
                doc.ProcessingStatus = ProcessingStatus.Pending;
            }

            job.ErrorMessage = ex.Message;
            doc.ProcessingError = ex.Message;
            job.CompletedAt = job.Status == "Failed" ? DateTimeOffset.UtcNow : null;
        }

        await db.SaveChangesAsync(ct);
    }
}


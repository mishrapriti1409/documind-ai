using DocuMind.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Data;

/// <summary>
/// Multi-tenant, Identity-integrated EF Core DbContext supporting pgvector.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentPermission> DocumentPermissions => Set<DocumentPermission>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageSource> MessageSources => Set<MessageSource>();
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserFeedback> UserFeedbacks => Set<UserFeedback>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Enable pgvector extension
        builder.HasPostgresExtension("vector");

        // Organization
        builder.Entity<Organization>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasIndex(o => o.Slug).IsUnique();
            e.Property(o => o.Name).HasMaxLength(256).IsRequired();
            e.Property(o => o.Slug).HasMaxLength(100).IsRequired();
        });

        // ApplicationUser
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.HasOne(u => u.Organization)
             .WithMany(o => o.Users)
             .HasForeignKey(u => u.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Document
        builder.Entity<Document>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasIndex(d => d.OrganizationId);
            e.HasIndex(d => d.ProcessingStatus);
            e.HasIndex(d => d.DeletedAt);
            e.Property(d => d.OriginalFileName).HasMaxLength(512).IsRequired();
            e.Property(d => d.StoragePath).HasMaxLength(1024).IsRequired();
            e.Property(d => d.ContentType).HasMaxLength(128).IsRequired();

            // Global query filter for soft delete
            e.HasQueryFilter(d => d.DeletedAt == null);

            e.HasOne(d => d.Organization)
             .WithMany(o => o.Documents)
             .HasForeignKey(d => d.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(d => d.UploadedBy)
             .WithMany()
             .HasForeignKey(d => d.UploadedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // DocumentChunk with pgvector
        builder.Entity<DocumentChunk>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.DocumentId);
            e.HasIndex(c => c.OrganizationId);
            e.Property(c => c.Content).IsRequired();
            e.Property(c => c.Embedding)
             .HasColumnType("vector(1536)");

            e.HasOne(c => c.Document)
             .WithMany(d => d.Chunks)
             .HasForeignKey(c => c.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // DocumentPermission
        builder.Entity<DocumentPermission>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => new { p.DocumentId, p.UserId }).IsUnique();

            e.HasOne(p => p.Document)
             .WithMany(d => d.Permissions)
             .HasForeignKey(p => p.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Conversation
        builder.Entity<Conversation>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.UserId);
            e.HasIndex(c => c.OrganizationId);

            e.HasOne(c => c.Organization)
             .WithMany(o => o.Conversations)
             .HasForeignKey(c => c.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.User)
             .WithMany()
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Message
        builder.Entity<Message>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.ConversationId);

            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // MessageSource
        builder.Entity<MessageSource>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.MessageId);
            e.HasIndex(s => s.DocumentId);

            e.HasOne(s => s.Message)
             .WithMany(m => m.Sources)
             .HasForeignKey(s => s.MessageId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessingJob
        builder.Entity<ProcessingJob>(e =>
        {
            e.HasKey(j => j.Id);
            e.HasIndex(j => j.Status);
            e.HasIndex(j => j.DocumentId);

            e.HasOne(j => j.Document)
             .WithMany(d => d.ProcessingJobs)
             .HasForeignKey(j => j.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.CreatedAt);
            e.HasIndex(a => a.OrganizationId);
            e.HasIndex(a => a.UserId);
        });

        // UserFeedback
        builder.Entity<UserFeedback>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasOne(f => f.Message)
             .WithOne(m => m.Feedback)
             .HasForeignKey<UserFeedback>(f => f.MessageId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Default Roles
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid> { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Name = "Manager", NormalizedName = "MANAGER" },
            new IdentityRole<Guid> { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), Name = "Employee", NormalizedName = "EMPLOYEE" },
            new IdentityRole<Guid> { Id = Guid.Parse("11111111-0000-0000-0000-000000000004"), Name = "Auditor", NormalizedName = "AUDITOR" }
        );
    }
}


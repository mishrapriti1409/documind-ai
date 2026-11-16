using DocuMind.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Application user associated with a tenant organization.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid OrganizationId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Organization? Organization { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}


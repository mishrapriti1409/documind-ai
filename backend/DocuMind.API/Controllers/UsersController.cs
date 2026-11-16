using DocuMind.Application.DTOs.Auth;
using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;
using DocuMind.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private Guid GetOrganizationId()
    {
        var claim = User.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(claim) ? Guid.Empty : Guid.Parse(claim);
    }

    /// <summary>List all users belonging to the current organization.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var users = await _context.Users
            .Where(u => u.OrganizationId == orgId)
            .OrderBy(u => u.FirstName)
            .Select(u => new UserDto(
                u.Id,
                u.Email!,
                u.FirstName,
                u.LastName,
                u.Role.ToString(),
                u.OrganizationId,
                u.IsActive,
                u.LastLoginAt
            ))
            .ToListAsync(ct);

        return Ok(users);
    }

    /// <summary>Update an organization member's role.</summary>
    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct);
        if (user == null) return NotFound(new { message = "User not found." });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
        {
            return BadRequest(new { message = "Invalid role specified." });
        }

        user.Role = newRole;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Toggle active/inactive status for a user.</summary>
    [HttpPost("{id:guid}/toggle-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId, ct);
        if (user == null) return NotFound(new { message = "User not found." });

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record UpdateRoleRequest(string Role);


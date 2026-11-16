using System.ComponentModel.DataAnnotations;

namespace DocuMind.Application.DTOs.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string OrganizationName
);

public record RegisterResponse(
    Guid UserId,
    string Email,
    string Token,
    string Role,
    Guid OrganizationId,
    string OrganizationName
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid OrganizationId,
    bool IsActive,
    DateTimeOffset? LastLoginAt
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword
);


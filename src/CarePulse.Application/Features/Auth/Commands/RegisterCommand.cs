using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Auth.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CarePulse.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequest Request) : IRequest<ApiResponse<AuthResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return ApiResponse<AuthResponse>.Fail("A user with this email already exists.");
        }

        var roleName = char.ToUpper(request.Role[0]) + request.Role[1..].ToLower(); // Normalize

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return ApiResponse<AuthResponse>.Fail($"Role '{roleName}' does not exist.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true, // For portfolio simplicity; in real systems use email confirmation
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AuthResponse>.Fail("Registration failed.", errors);
        }

        await _userManager.AddToRoleAsync(user, roleName);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15),
            user.RefreshTokenExpiryTime.Value,
            new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName, roles)
        );

        return ApiResponse<AuthResponse>.Ok(response, "User registered successfully.");
    }
}

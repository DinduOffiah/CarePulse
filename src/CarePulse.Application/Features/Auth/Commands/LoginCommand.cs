using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Auth.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CarePulse.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequest Request) : IRequest<ApiResponse<AuthResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.DeletedAt is not null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        // Check lockout
        if (await _userManager.IsLockedOutAsync(user))
        {
            return ApiResponse<AuthResponse>.Fail("Account is temporarily locked due to multiple failed attempts.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        // Reset access failed count on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15),
            user.RefreshTokenExpiryTime.Value,
            new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName, roles)
        );

        return ApiResponse<AuthResponse>.Ok(response, "Login successful.");
    }
}

using CarePulse.Application.Common.Interfaces;
using CarePulse.Application.Features.Auth.DTOs;
using CarePulse.Domain.Entities;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CarePulse.Application.Features.Auth.Commands;

public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<ApiResponse<AuthResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid access token.");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? principal.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return ApiResponse<AuthResponse>.Fail("Invalid token claims.");
        }

        var user = await _userManager.FindByIdAsync(userGuid.ToString());
        if (user is null || user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow ||
            user.DeletedAt is not null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15),
            user.RefreshTokenExpiryTime.Value,
            new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName, roles)
        );

        return ApiResponse<AuthResponse>.Ok(response, "Token refreshed successfully.");
    }
}

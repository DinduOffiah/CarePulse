using CarePulse.Domain.Entities;
using CarePulse.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CarePulse.Application.Features.Auth.Commands;

public record LogoutCommand(Guid UserId) : IRequest<ApiResponse<object>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<object>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public LogoutCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApiResponse<object>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return ApiResponse<object>.Fail("User not found.");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return ApiResponse<object>.Ok(new { }, "Logged out successfully.");
    }
}

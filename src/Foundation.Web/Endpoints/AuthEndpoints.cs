using Foundation.Application.Abstractions;
using Foundation.Application.Features.Profiles;
using Foundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Foundation.Web.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/token", async (
            LoginRequest request,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email)
                       ?? await userManager.FindByNameAsync(request.Email);

            if (user == null)
            {
                return Results.Problem(
                    detail: "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication Failed");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return Results.Problem(
                    detail: "User account is locked out.",
                    statusCode: StatusCodes.Status423Locked,
                    title: "Account Locked");
            }

            if (!result.Succeeded)
            {
                return Results.Problem(
                    detail: "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication Failed");
            }

            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new
            {
                user.Id,
                user.Email,
                user.UserName,
                Roles = roles,
                Message = "Authentication successful"
            });
        })
        .WithName("Authenticate")
        .WithSummary("Authenticates user credentials");

        group.MapGet("/me", async (
            ICurrentUser currentUser,
            IQueryHandler<GetUserProfileQuery, Foundation.Application.Common.Result<UserProfileDto>> profileHandler) =>
        {
            if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var profileResult = await profileHandler.HandleAsync(new GetUserProfileQuery(currentUser.UserId.Value));

            return Results.Ok(new
            {
                UserId = currentUser.UserId.Value,
                currentUser.UserName,
                currentUser.Email,
                Profile = profileResult.IsSuccess ? profileResult.Value : null
            });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Retrieves current authenticated user details");

        return group;
    }

    public record LoginRequest(string Email, string Password);
}

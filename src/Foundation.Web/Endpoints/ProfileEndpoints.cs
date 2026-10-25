using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Profiles;

namespace Foundation.Web.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (
            Guid id,
            IQueryHandler<GetUserProfileQuery, Result<UserProfileDto>> handler) =>
        {
            var result = await handler.HandleAsync(new GetUserProfileQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { Error = result.Error });
        })
        .WithName("GetUserProfileById")
        .WithSummary("Retrieves a user profile by ID");

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateUserProfileCommand command,
            ICommandHandler<UpdateUserProfileCommand, Result<UserProfileDto>> handler) =>
        {
            if (id != command.UserId)
            {
                return Results.BadRequest(new { Error = "Route ID does not match payload ID." });
            }

            var result = await handler.HandleAsync(command);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error, statusCode: StatusCodes.Status403Forbidden);
        })
        .RequireAuthorization()
        .WithName("UpdateUserProfile")
        .WithSummary("Updates user profile");

        return group;
    }
}

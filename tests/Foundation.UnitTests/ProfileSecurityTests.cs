using Foundation.Application.Abstractions;
using Foundation.Application.Features.Profiles;
using Foundation.Domain.Entities;
using Foundation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Foundation.UnitTests;

public class ProfileSecurityTests
{
    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
        public List<string> Roles { get; } = [];
        public bool IsInRole(string role) => Roles.Contains(role);
    }

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => new(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);
    }

    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task UpdateUserProfile_Should_Prevent_IDOR_When_UserA_Tries_To_Update_UserB()
    {
        using var db = CreateInMemoryDb();
        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();

        // Seed User B profile
        db.UserProfiles.Add(new UserProfile
        {
            Id = userBId,
            DisplayName = "User B",
            TimeZone = "UTC"
        });
        await db.SaveChangesAsync();

        // Current user is User A
        var currentUser = new TestCurrentUser { UserId = userAId, UserName = "usera" };
        var dateTime = new TestDateTimeProvider();

        var handler = new UpdateUserProfileCommandHandler(db, currentUser, dateTime);

        // Act: User A tries to modify User B's profile
        var command = new UpdateUserProfileCommand(
            userBId,
            "Malicious Update",
            "Hacked Bio",
            "UTC",
            false,
            null);

        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("Forbidden");

        // Verify database entity unchanged
        var userB = await db.UserProfiles.FindAsync(userBId);
        userB.ShouldNotBeNull();
        userB.DisplayName.ShouldBe("User B");
    }

    [Fact]
    public async Task GetUserProfile_Should_Block_Unauthorized_Access_To_Private_Profile()
    {
        using var db = CreateInMemoryDb();
        var privateUserId = Guid.NewGuid();
        var visitorUserId = Guid.NewGuid();

        // Seed Private User profile
        db.UserProfiles.Add(new UserProfile
        {
            Id = privateUserId,
            DisplayName = "Secret Agent",
            IsPrivate = true,
            TimeZone = "UTC"
        });
        await db.SaveChangesAsync();

        // Current user is visitor
        var currentUser = new TestCurrentUser { UserId = visitorUserId, UserName = "visitor" };

        var handler = new GetUserProfileQueryHandler(db, currentUser);

        // Act
        var result = await handler.HandleAsync(new GetUserProfileQuery(privateUserId));

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.ShouldContain("private");
    }

    [Fact]
    public async Task Admin_Should_Be_Allowed_To_View_Private_Profile()
    {
        using var db = CreateInMemoryDb();
        var privateUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        db.UserProfiles.Add(new UserProfile
        {
            Id = privateUserId,
            DisplayName = "Secret Agent",
            IsPrivate = true,
            TimeZone = "UTC"
        });
        await db.SaveChangesAsync();

        var adminUser = new TestCurrentUser { UserId = adminUserId, UserName = "admin" };
        adminUser.Roles.Add("Admin");

        var handler = new GetUserProfileQueryHandler(db, adminUser);

        // Act
        var result = await handler.HandleAsync(new GetUserProfileQuery(privateUserId));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.DisplayName.ShouldBe("Secret Agent");
    }
}

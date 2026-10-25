using System.Net;
using System.Net.Http.Json;
using Foundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Foundation.IntegrationTests;

public class AuthAndLockoutTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthAndLockoutTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TokenEndpoint_Should_Authenticate_Valid_Credentials()
    {
        var client = _factory.CreateClient();
        var uniqueEmail = $"user_{Guid.NewGuid():N}@example.com";
        var password = "Password123!@#";

        // Seed user
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = uniqueEmail,
                Email = uniqueEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            result.Succeeded.ShouldBeTrue();
        }

        // Authenticate via /api/v1/auth/token
        var response = await client.PostAsJsonAsync("/api/v1/auth/token", new
        {
            Email = uniqueEmail,
            Password = password
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Account_Should_Lock_Out_After_Max_Failed_Attempts()
    {
        var client = _factory.CreateClient();
        var uniqueEmail = $"lockout_{Guid.NewGuid():N}@example.com";
        var correctPassword = "Password123!@#";

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = uniqueEmail,
                Email = uniqueEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, correctPassword);
            result.Succeeded.ShouldBeTrue();
        }

        // 4 consecutive failed attempts (under threshold)
        for (int i = 0; i < 4; i++)
        {
            var failedResponse = await client.PostAsJsonAsync("/api/v1/auth/token", new
            {
                Email = uniqueEmail,
                Password = "WrongPassword!@#"
            });
            failedResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        // 5th failed attempt reaches MaxFailedAccessAttempts and locks the account
        var lockedResponse = await client.PostAsJsonAsync("/api/v1/auth/token", new
        {
            Email = uniqueEmail,
            Password = "WrongPassword!@#"
        });
        lockedResponse.StatusCode.ShouldBe(HttpStatusCode.Locked);

        // Subsequent attempt with correct password is also locked
        var subsequentResponse = await client.PostAsJsonAsync("/api/v1/auth/token", new
        {
            Email = uniqueEmail,
            Password = correctPassword
        });
        subsequentResponse.StatusCode.ShouldBe(HttpStatusCode.Locked);
    }
}

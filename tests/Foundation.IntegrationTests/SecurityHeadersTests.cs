using System.Net;
using Shouldly;
using Xunit;

namespace Foundation.IntegrationTests;

public class SecurityHeadersTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SecurityHeadersTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_Should_Include_Hardened_Security_Headers()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Security headers
        response.Headers.Contains("X-Content-Type-Options").ShouldBeTrue();
        response.Headers.GetValues("X-Content-Type-Options").ShouldContain("nosniff");

        response.Headers.Contains("X-Frame-Options").ShouldBeTrue();
        response.Headers.GetValues("X-Frame-Options").ShouldContain("DENY");

        response.Headers.Contains("Referrer-Policy").ShouldBeTrue();
        response.Headers.GetValues("Referrer-Policy").ShouldContain("strict-origin-when-cross-origin");

        response.Headers.Contains("Content-Security-Policy").ShouldBeTrue();
        var csp = string.Join(" ", response.Headers.GetValues("Content-Security-Policy"));
        csp.ShouldContain("default-src 'self'");
        csp.ShouldContain("nonce-");
    }
}

using System.Net;
using Shouldly;
using Xunit;

namespace Foundation.IntegrationTests;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthLive_Should_Return_Ok()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_Should_Return_Ok_When_Database_Is_Available()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

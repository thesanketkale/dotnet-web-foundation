using System.Net;
using System.Net.Http.Json;
using Foundation.Application.Features.Cms;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Foundation.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Foundation.IntegrationTests;

public class CmsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CmsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPublicPages_Should_Return_Only_Published_Pages()
    {
        var client = _factory.CreateClient();
        var uniqueSlugPublished = $"pub-{Guid.NewGuid():N}";
        var uniqueSlugDraft = $"draft-{Guid.NewGuid():N}";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.CmsPages.Add(new CmsPage
            {
                Title = "Published Article",
                Slug = uniqueSlugPublished,
                ContentMarkdown = "# Published",
                ContentHtml = "<h1>Published</h1>",
                Status = ContentStatus.Published,
                PublishedAtUtc = DateTime.UtcNow
            });

            db.CmsPages.Add(new CmsPage
            {
                Title = "Draft Article",
                Slug = uniqueSlugDraft,
                ContentMarkdown = "# Draft",
                ContentHtml = "<h1>Draft</h1>",
                Status = ContentStatus.Draft
            });

            await db.SaveChangesAsync();
        }

        // Act 1: Get published page by slug
        var publishedResponse = await client.GetAsync($"/api/v1/content/pages/{uniqueSlugPublished}");
        publishedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await publishedResponse.Content.ReadFromJsonAsync<CmsPageDto>();
        page.ShouldNotBeNull();
        page.Slug.ShouldBe(uniqueSlugPublished);

        // Act 2: Visitor should not be able to get draft page
        var draftResponse = await client.GetAsync($"/api/v1/content/pages/{uniqueSlugDraft}");
        draftResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

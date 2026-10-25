using Foundation.Application.Features.Cms;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Foundation.Infrastructure.Services;
using Shouldly;
using Xunit;

namespace Foundation.UnitTests;

public class DomainAndSanitizerTests
{
    [Fact]
    public void CmsPage_Should_Initialize_With_Defaults()
    {
        var page = new CmsPage
        {
            Title = "Test Page",
            Slug = "test-page"
        };

        page.Id.ShouldNotBe(Guid.Empty);
        page.Status.ShouldBe(ContentStatus.Draft);
        page.IsDeleted.ShouldBeFalse();
        page.ConcurrencyToken.ShouldNotBe(Guid.Empty);
        page.CreatedAtUtc.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("  Leading and Trailing   ", "leading-and-trailing")]
    [InlineData("Special!@#$%^&*()Chars", "special-chars")]
    [InlineData("Multiple---Dashes", "multiple-dashes")]
    public void SlugHelper_Should_Normalize_Slugs_Correctly(string input, string expected)
    {
        var actual = SlugHelper.NormalizeSlug(input);
        actual.ShouldBe(expected);
    }

    [Fact]
    public void MarkdigRenderer_Should_Sanitize_Malicious_Xss_Payloads()
    {
        var renderer = new MarkdigRenderer();
        var maliciousMarkdown = @"
# Hello Safe Heading

<script>alert('xss');</script>
<img src=""invalid"" onerror=""alert('hack')"" />
<a href=""javascript:alert('danger')"">Click Me</a>
<p style=""color: red;"">Safe Paragraph</p>
";

        var html = renderer.RenderHtml(maliciousMarkdown);

        html.ShouldNotContain("<script>");
        html.ShouldNotContain("alert('xss')");
        html.ShouldNotContain("onerror");
        html.ShouldNotContain("javascript:");
        html.ShouldContain("Hello Safe Heading");
        html.ShouldContain("Safe Paragraph");
    }

    [Fact]
    public void UserProfile_Should_Store_Display_And_Timezone()
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = "Jane Doe",
            TimeZone = "America/New_York",
            IsPrivate = true
        };

        profile.DisplayName.ShouldBe("Jane Doe");
        profile.TimeZone.ShouldBe("America/New_York");
        profile.IsPrivate.ShouldBeTrue();
    }
}

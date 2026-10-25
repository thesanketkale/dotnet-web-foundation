using Foundation.Application.Abstractions;
using Ganss.Xss;
using Markdig;

namespace Foundation.Infrastructure.Services;

public class MarkdigRenderer : IMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;
    private readonly HtmlSanitizer _sanitizer;

    public MarkdigRenderer()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseAutoLinks()
            .Build();

        _sanitizer = new HtmlSanitizer();
        // Allow safe standard HTML tags for editorial styling
        _sanitizer.AllowedTags.Add("div");
        _sanitizer.AllowedTags.Add("span");
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedTags.Add("blockquote");
        _sanitizer.AllowedAttributes.Add("class");
        _sanitizer.AllowedAttributes.Add("id");
    }

    public string RenderHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
        var rawHtml = Markdown.ToHtml(markdown, _pipeline);
        return SanitizeHtml(rawHtml);
    }

    public string SanitizeHtml(string rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml)) return string.Empty;
        return _sanitizer.Sanitize(rawHtml);
    }
}

namespace Foundation.Application.Abstractions;

public interface IMarkdownRenderer
{
    string RenderHtml(string markdown);
    string SanitizeHtml(string rawHtml);
}

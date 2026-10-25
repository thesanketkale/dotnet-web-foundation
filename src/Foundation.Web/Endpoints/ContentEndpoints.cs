using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;

namespace Foundation.Web.Endpoints;

public static class ContentEndpoints
{
    public static RouteGroupBuilder MapContentEndpoints(this RouteGroupBuilder group)
    {
        // Public Pages
        group.MapGet("/pages", async (
            IQueryHandler<GetPagesQuery, Result<PagedList<CmsPageDto>>> handler,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await handler.HandleAsync(new GetPagesQuery(IncludeUnpublished: false, PageNumber: page, PageSize: pageSize));
            return Results.Ok(result.Value);
        })
        .WithName("GetPublicPages")
        .WithSummary("Lists published CMS pages");

        group.MapGet("/pages/{slug}", async (
            string slug,
            IQueryHandler<GetPageBySlugQuery, Result<CmsPageDto>> handler) =>
        {
            var result = await handler.HandleAsync(new GetPageBySlugQuery(slug, AllowUnpublished: false));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { Error = result.Error });
        })
        .WithName("GetPublicPageBySlug")
        .WithSummary("Retrieves a published CMS page by slug");

        // Public Posts
        group.MapGet("/posts", async (
            IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>> handler,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await handler.HandleAsync(new GetPostsQuery(IncludeUnpublished: false, PageNumber: page, PageSize: pageSize));
            return Results.Ok(result.Value);
        })
        .WithName("GetPublicPosts")
        .WithSummary("Lists published CMS posts");

        group.MapGet("/posts/{slug}", async (
            string slug,
            IQueryHandler<GetPostBySlugQuery, Result<CmsPostDto>> handler) =>
        {
            var result = await handler.HandleAsync(new GetPostBySlugQuery(slug, AllowUnpublished: false));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { Error = result.Error });
        })
        .WithName("GetPublicPostBySlug")
        .WithSummary("Retrieves a published CMS post by slug");

        // Media streaming
        group.MapGet("/media/{fileName}", async (
            string fileName,
            IFileStorageService storage,
            HttpContext context) =>
        {
            var stream = await storage.GetFileAsync(fileName);
            if (stream == null)
            {
                return Results.NotFound();
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "image/jpeg"
            };

            context.Response.Headers.CacheControl = "public, max-age=2592000, immutable";
            return Results.File(stream, contentType);
        })
        .WithName("GetMediaFile")
        .WithSummary("Streams stored media file");

        return group;
    }
}

using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Audit;
using Foundation.Application.Features.Cms;
using Foundation.Application.Features.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Profile Handlers
        services.AddScoped<IQueryHandler<GetUserProfileQuery, Result<UserProfileDto>>, GetUserProfileQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateUserProfileCommand, Result<UserProfileDto>>, UpdateUserProfileCommandHandler>();
        services.AddScoped<IQueryHandler<ExportPersonalDataQuery, Result<PersonalDataExportDto>>, ExportPersonalDataQueryHandler>();

        // CMS Page Handlers
        services.AddScoped<CmsPageHandlers>();
        services.AddScoped<ICommandHandler<CreatePageCommand, Result<CmsPageDto>>>(sp => sp.GetRequiredService<CmsPageHandlers>());
        services.AddScoped<ICommandHandler<UpdatePageCommand, Result<CmsPageDto>>>(sp => sp.GetRequiredService<CmsPageHandlers>());
        services.AddScoped<ICommandHandler<DeletePageCommand, Result>>(sp => sp.GetRequiredService<CmsPageHandlers>());
        services.AddScoped<IQueryHandler<GetPagesQuery, Result<PagedList<CmsPageDto>>>>(sp => sp.GetRequiredService<CmsPageHandlers>());
        services.AddScoped<IQueryHandler<GetPageBySlugQuery, Result<CmsPageDto>>>(sp => sp.GetRequiredService<CmsPageHandlers>());
        services.AddScoped<IQueryHandler<GetPageByIdQuery, Result<CmsPageDto>>>(sp => sp.GetRequiredService<CmsPageHandlers>());

        // CMS Post Handlers
        services.AddScoped<CmsPostHandlers>();
        services.AddScoped<ICommandHandler<CreatePostCommand, Result<CmsPostDto>>>(sp => sp.GetRequiredService<CmsPostHandlers>());
        services.AddScoped<ICommandHandler<UpdatePostCommand, Result<CmsPostDto>>>(sp => sp.GetRequiredService<CmsPostHandlers>());
        services.AddScoped<ICommandHandler<DeletePostCommand, Result>>(sp => sp.GetRequiredService<CmsPostHandlers>());
        services.AddScoped<IQueryHandler<GetPostsQuery, Result<PagedList<CmsPostDto>>>>(sp => sp.GetRequiredService<CmsPostHandlers>());
        services.AddScoped<IQueryHandler<GetPostBySlugQuery, Result<CmsPostDto>>>(sp => sp.GetRequiredService<CmsPostHandlers>());
        services.AddScoped<IQueryHandler<GetPostByIdQuery, Result<CmsPostDto>>>(sp => sp.GetRequiredService<CmsPostHandlers>());

        // Media Handlers
        services.AddScoped<MediaHandlers>();
        services.AddScoped<ICommandHandler<UploadMediaCommand, Result<MediaFileDto>>>(sp => sp.GetRequiredService<MediaHandlers>());
        services.AddScoped<ICommandHandler<DeleteMediaCommand, Result>>(sp => sp.GetRequiredService<MediaHandlers>());
        services.AddScoped<IQueryHandler<GetMediaFilesQuery, Result<PagedList<MediaFileDto>>>>(sp => sp.GetRequiredService<MediaHandlers>());

        // Audit Handlers
        services.AddScoped<IQueryHandler<GetAuditLogsQuery, Result<PagedList<AuditLogDto>>>, GetAuditLogsQueryHandler>();

        return services;
    }
}

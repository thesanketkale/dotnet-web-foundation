using Foundation.Application.Abstractions;
using Foundation.Infrastructure.Persistence;
using Foundation.Infrastructure.Persistence.Interceptors;
using Foundation.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureDbContext = null)
    {
        // 1. Core Service Abstractions
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IMarkdownRenderer, MarkdigRenderer>();

        // 2. Interceptors
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        // 3. Database Context
        if (configureDbContext != null)
        {
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
                var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
                options.AddInterceptors(auditInterceptor, softDeleteInterceptor);

                configureDbContext(sp, options);
            });
        }

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // 4. File Storage Service
        var storagePath = configuration.GetValue<string>("Storage:LocalStoragePath") ?? "App_Data/uploads";
        services.AddSingleton<IFileStorageService>(new LocalFileStorageService(storagePath));

        // 5. Transactional Email Sender
        services.AddTransient<IEmailSender>(sp =>
        {
            var host = configuration.GetValue<string>("Email:Host") ?? "localhost";
            var port = configuration.GetValue<int>("Email:Port", 1025);
            var senderEmail = configuration.GetValue<string>("Email:SenderEmail") ?? "noreply@example.com";
            var senderName = configuration.GetValue<string>("Email:SenderName") ?? "dotnet-web-foundation";
            var username = configuration.GetValue<string>("Email:Username");
            var password = configuration.GetValue<string>("Email:Password");
            var enableSsl = configuration.GetValue<bool>("Email:EnableSsl", false);
            var logger = sp.GetRequiredService<ILogger<SmtpEmailSender>>();

            return new SmtpEmailSender(host, port, senderEmail, senderName, username, password, enableSsl, logger);
        });

        return services;
    }
}

using Foundation.Application.Abstractions;
using Foundation.Domain.Entities;
using Foundation.Infrastructure.Identity;
using Foundation.Infrastructure.Persistence;
using Foundation.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "IntegrationTestDb_" + Guid.NewGuid().ToString("N");

    public TestEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Database:Provider", "InMemory");
        builder.UseSetting("Database:InMemoryDbName", _databaseName);
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace email sender with mock
            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailDescriptor != null)
            {
                services.Remove(emailDescriptor);
            }
            services.AddSingleton<IEmailSender>(EmailSender);

            // Build service provider and seed default roles
            using var scope = services.BuildServiceProvider().CreateScope();
            var scopedServices = scope.ServiceProvider;
            var roleManager = scopedServices.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in new[] { "Admin", "Editor", "User" })
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new ApplicationRole(role)).GetAwaiter().GetResult();
                }
            }
        });
    }
}

public class TestEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = [];

    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((toEmail, subject, htmlMessage));
        return Task.CompletedTask;
    }
}

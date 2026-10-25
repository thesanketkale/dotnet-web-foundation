using Foundation.Application.Abstractions;
using Foundation.Domain.Entities;
using Foundation.Domain.Enums;
using Foundation.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMarkdownRenderer renderer,
        ILogger logger)
    {
        // 1. Seed Roles
        string[] roles = ["Admin", "Editor", "User"];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Seeding role: {Role}", roleName);
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        // 2. Seed Administrator
        const string adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_INITIAL_PASSWORD");
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            adminPassword = "AdminSecurePass123!";
            logger.LogWarning("ADMIN_INITIAL_PASSWORD environment variable not set. Using default development seed password.");
        }

        if (adminUser == null)
        {
            logger.LogInformation("Seeding Administrator account: {AdminEmail}", adminEmail);
            adminUser = new ApplicationUser(adminEmail)
            {
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRolesAsync(adminUser, ["Admin", "Editor"]);

                // Create Admin Profile
                var adminProfile = new UserProfile
                {
                    Id = adminUser.Id,
                    DisplayName = "System Administrator",
                    Bio = "Platform administrator and maintainer.",
                    TimeZone = "UTC",
                    IsPrivate = false,
                    CreatedAtUtc = DateTime.UtcNow
                };
                context.UserProfiles.Add(adminProfile);
                await context.SaveChangesAsync();
            }
            else
            {
                logger.LogError("Failed to create seed admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // 3. Seed Demo Editor
        const string editorEmail = "editor@example.com";
        var editorUser = await userManager.FindByEmailAsync(editorEmail);
        if (editorUser == null)
        {
            logger.LogInformation("Seeding Editor account: {EditorEmail}", editorEmail);
            editorUser = new ApplicationUser(editorEmail)
            {
                Email = editorEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(editorUser, "EditorSecurePass123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(editorUser, "Editor");

                var editorProfile = new UserProfile
                {
                    Id = editorUser.Id,
                    DisplayName = "Lead Editor",
                    Bio = "Content manager and editorial reviewer.",
                    TimeZone = "UTC",
                    IsPrivate = false,
                    CreatedAtUtc = DateTime.UtcNow
                };
                context.UserProfiles.Add(editorProfile);
                await context.SaveChangesAsync();
            }
        }

        // 4. Seed Synthetic CMS Pages
        if (!await context.CmsPages.IgnoreQueryFilters().AnyAsync())
        {
            logger.LogInformation("Seeding initial CMS pages...");
            var aboutMarkdown = @"# About dotnet-web-foundation

**dotnet-web-foundation** is an open-source, modular foundation for ASP.NET Core web applications.

### Key Capabilities
- **Enterprise Authentication:** ASP.NET Core Identity with TOTP 2FA, lockout, and recovery codes.
- **Role-Based Admin:** Protected dashboards for administrators and content editors.
- **Dual Database Support:** Native Entity Framework Core migrations for Microsoft SQL Server and MySQL.
- **Built-in CMS:** Lightweight markdown content engine with strict HTML sanitization.
- **Observability:** OpenTelemetry traces and metrics with structured Serilog logging.
";

            var archMarkdown = @"# Architecture Overview

This application adheres to **Clean Architecture** and vertical-slice design principles:

1. **Foundation.Domain:** Pure domain entities, value objects, domain exceptions.
2. **Foundation.Application:** Use-case commands, queries, validators, DTOs.
3. **Foundation.Infrastructure:** Entity Framework Core DbContext, identity stores, file storage.
4. **Foundation.Web:** ASP.NET Core Razor Pages, Minimal APIs (`/api/v1`), composition root.
";

            var authorId = adminUser?.Id ?? Guid.NewGuid();

            var pages = new List<CmsPage>
            {
                new()
                {
                    Title = "About",
                    Slug = "about",
                    Summary = "Overview and mission of dotnet-web-foundation.",
                    ContentMarkdown = aboutMarkdown,
                    ContentHtml = renderer.RenderHtml(aboutMarkdown),
                    Status = ContentStatus.Published,
                    PublishedAtUtc = DateTime.UtcNow,
                    AuthorId = authorId,
                    MetaTitle = "About dotnet-web-foundation",
                    MetaDescription = "Learn about the architecture and capabilities of dotnet-web-foundation.",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Title = "Architecture",
                    Slug = "architecture",
                    Summary = "In-depth guide to the modular monolith solution structure.",
                    ContentMarkdown = archMarkdown,
                    ContentHtml = renderer.RenderHtml(archMarkdown),
                    Status = ContentStatus.Published,
                    PublishedAtUtc = DateTime.UtcNow,
                    AuthorId = authorId,
                    MetaTitle = "System Architecture",
                    MetaDescription = "Discover the clean architecture boundaries and vertical-slice patterns.",
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            context.CmsPages.AddRange(pages);
            await context.SaveChangesAsync();
        }

        // 5. Seed Synthetic CMS Posts
        if (!await context.CmsPosts.IgnoreQueryFilters().AnyAsync())
        {
            logger.LogInformation("Seeding initial CMS posts...");
            var postMarkdown = @"# Welcome to dotnet-web-foundation

We are pleased to introduce the production-ready .NET 10 modular web application starter.

### Highlights
- Zero commercial third-party dependencies (strictly MIT, Apache-2.0, BSD-2-Clause).
- Playwright end-to-end browser smoke test coverage.
- Fully isolated migrations assemblies for SQL Server and MySQL.
";

            var authorId = adminUser?.Id ?? Guid.NewGuid();

            var post = new CmsPost
            {
                Title = "Welcome to dotnet-web-foundation",
                Slug = "welcome-to-dotnet-web-foundation",
                Summary = "Introducing our production-ready .NET 10 web foundation.",
                ContentMarkdown = postMarkdown,
                ContentHtml = renderer.RenderHtml(postMarkdown),
                Status = ContentStatus.Published,
                PublishedAtUtc = DateTime.UtcNow,
                AuthorId = authorId,
                MetaTitle = "Welcome to dotnet-web-foundation",
                MetaDescription = "Discover the features of our modular ASP.NET Core foundation.",
                CreatedAtUtc = DateTime.UtcNow
            };

            context.CmsPosts.Add(post);
            await context.SaveChangesAsync();
        }
    }
}

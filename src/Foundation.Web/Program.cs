using System.Threading.RateLimiting;
using Foundation.Application;
using Foundation.Application.Abstractions;
using Foundation.Infrastructure;
using Foundation.Infrastructure.Identity;
using Foundation.Infrastructure.Persistence;
using Foundation.Infrastructure.Persistence.Seed;
using Foundation.Web.Endpoints;
using Foundation.Web.Middleware;
using Foundation.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Structured Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
        formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. OpenTelemetry Tracing & Metrics
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "dotnet-web-foundation";
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
var enableConsoleTelemetry = builder.Configuration.GetValue<bool>("OpenTelemetry:EnableConsoleExporter");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(o =>
            {
                o.Filter = httpContext =>
                {
                    var path = httpContext.Request.Path.Value ?? string.Empty;
                    return !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase);
                };
            })
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }

        if (enableConsoleTelemetry)
        {
            tracing.AddConsoleExporter();
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }

        if (enableConsoleTelemetry)
        {
            metrics.AddConsoleExporter();
        }
    });

// 3. Problem Details & Antiforgery
builder.Services.AddProblemDetails();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// 4. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// 5. Database Provider Resolution
var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
Action<IServiceProvider, DbContextOptionsBuilder> configureDbContext = (sp, options) =>
{
    if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase(builder.Configuration["Database:InMemoryDbName"] ?? "FoundationTestDb");
    }
    else if (dbProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
    {
        var mySqlConn = builder.Configuration.GetConnectionString("MySqlConnection")
                        ?? builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");
        options.UseMySQL(mySqlConn, b => b.MigrationsAssembly("Foundation.Migrations.MySql"));
    }
    else
    {
        var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseSqlServer(sqlServerConn, b => b.MigrationsAssembly("Foundation.Migrations.SqlServer"));
    }
};

// 6. Application & Infrastructure Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructureServices(builder.Configuration, configureDbContext);

// 7. ASP.NET Core Identity & Authorization
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;

    // Lockout policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Sign-in policy
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireEditor", policy => policy.RequireRole("Admin", "Editor"));
});

// 8. User Context & Web Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

// 9. Razor Pages & Conventions
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "RequireEditor");
    options.Conventions.AuthorizeFolder("/Admin/Users", "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Admin/Audit", "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Account/Manage");
});

// 10. OpenAPI & Health Checks
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

// 11. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseMiddleware<NonceMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// 12. Health Endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database") || check.Name == "database"
});

// 13. OpenAPI Specification Endpoint
app.MapOpenApi();

// 14. Minimal APIs (/api/v1)
var apiGroup = app.MapGroup("/api/v1");
apiGroup.MapGroup("/auth").MapAuthEndpoints().RequireRateLimiting("AuthPolicy");
apiGroup.MapGroup("/content").MapContentEndpoints();
apiGroup.MapGroup("/profiles").MapProfileEndpoints();

// 15. Razor Pages
app.MapRazorPages();

// 16. Automatic Database Migration & Seeding on Startup
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsAtStartup", false) && !dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var renderer = services.GetRequiredService<IMarkdownRenderer>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying EF Core migrations for provider: {Provider}", dbProvider);
        await db.Database.MigrateAsync();

        logger.LogInformation("Seeding default roles, admin user, and sample CMS data...");
        await DbInitializer.SeedAsync(db, userManager, roleManager, renderer, logger);
        logger.LogInformation("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();

namespace Foundation.Web
{
    public partial class Program { }
}

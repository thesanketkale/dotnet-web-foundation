# Database Provider Configuration & Migrations

`dotnet-web-foundation` provides full dual-provider support for both **Microsoft SQL Server** and **MySQL / MariaDB** using isolated Entity Framework Core migration assemblies.

---

## 1. Provider Selection

The active provider is controlled by the `Database:Provider` configuration setting (or environment variable `Database__Provider`):

```json
{
  "Database": {
    "Provider": "SqlServer", // or "MySql"
    "ApplyMigrationsAtStartup": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=FoundationDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true;",
    "MySqlConnection": "Server=localhost;Port=3306;Database=FoundationDb;User=root;Password=YourStrong@Passw0rd;"
  }
}
```

---

## 2. Dedicated Migration Assemblies

Because SQL Server and MySQL generate different SQL dialect syntax (e.g. data types, index options, value generation strategies), migrations are maintained in separate projects:

- **`Foundation.Migrations.SqlServer`**: Houses migrations generated via `Microsoft.EntityFrameworkCore.SqlServer`.
- **`Foundation.Migrations.MySql`**: Houses migrations generated via `MySql.EntityFrameworkCore`.

### Adding a New Migration

When modifying domain entities or EF configurations, generate migrations for both providers:

#### For SQL Server:
```bash
dotnet ef migrations add AddNewFeature \
  --project src/Foundation.Migrations.SqlServer \
  --startup-project src/Foundation.Web \
  --context AppDbContext
```

#### For MySQL:
```bash
# Temporarily set environment or override provider in appsettings
dotnet ef migrations add AddNewFeature \
  --project src/Foundation.Migrations.MySql \
  --startup-project src/Foundation.Web \
  --context AppDbContext
```

---

## 3. Automatic Startup Migrations & Seeding

When running in local containers or during integration testing:
- Setting `Database:ApplyMigrationsAtStartup: true` automatically invokes `db.Database.MigrateAsync()` during application bootstrap.
- Next, `DbInitializer.SeedAsync(...)` verifies whether standard roles (`Admin`, `Editor`, `User`), default administrative accounts, and synthetic CMS seed pages exist, creating them if missing.
- **Production Guard:** For production deployments, disable `ApplyMigrationsAtStartup` and run migrations as part of a pre-deployment CI/CD pipeline or migration bundle (`dotnet ef migrations bundle`).

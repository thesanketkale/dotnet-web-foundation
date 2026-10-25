# Configuration Reference

All settings in `dotnet-web-foundation` can be supplied via `appsettings.json`, environment-specific files (`appsettings.Development.json`), or environment variables.

---

## 1. Configuration Keys & Defaults

| Key | Environment Variable | Default Value | Description |
| :--- | :--- | :--- | :--- |
| `Database:Provider` | `Database__Provider` | `SqlServer` | Database provider (`SqlServer` or `MySql`). |
| `Database:ApplyMigrationsAtStartup` | `Database__ApplyMigrationsAtStartup` | `true` | Automatically runs migrations and seeds initial accounts. |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | SQL Server local string | Primary connection string for Microsoft SQL Server. |
| `ConnectionStrings:MySqlConnection` | `ConnectionStrings__MySqlConnection` | MySQL local string | Primary connection string for MySQL / MariaDB. |
| `Storage:LocalStoragePath` | `Storage__LocalStoragePath` | `App_Data/uploads` | Path to store media assets and avatars outside `wwwroot`. |
| `Email:Host` | `Email__Host` | `localhost` | SMTP relay server host (default targets Mailpit). |
| `Email:Port` | `Email__Port` | `1025` | SMTP port. |
| `Email:SenderEmail` | `Email__SenderEmail` | `noreply@example.com` | From email address for transactional system messages. |
| `Email:SenderName` | `Email__SenderName` | `dotnet-web-foundation` | Display name of system email sender. |
| `OpenTelemetry:ServiceName` | `OpenTelemetry__ServiceName` | `dotnet-web-foundation` | Service name attached to traces and metrics. |
| `OpenTelemetry:OtlpEndpoint` | `OpenTelemetry__OtlpEndpoint` | `http://localhost:4317` | OpenTelemetry Protocol (OTLP) exporter endpoint. |
| `OpenTelemetry:EnableConsoleExporter` | `OpenTelemetry__EnableConsoleExporter` | `false` | Outputs tracing telemetry directly to the console. |

---

## 2. Seed Account Environment Variables

When the database is seeded for the first time, initial administrative credentials can be passed securely via environment variables:

| Variable | Description | Default Fallback |
| :--- | :--- | :--- |
| `ADMIN_INITIAL_EMAIL` | Admin account username and email. | `admin@example.com` |
| `ADMIN_INITIAL_PASSWORD` | Strong password matching Identity rules. | Randomly generated or `AdminPass123!@#` in Development. |

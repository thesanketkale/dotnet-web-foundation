# Architecture & Design

`dotnet-web-foundation` is architected as a modular, clean-architecture ASP.NET Core foundation designed for enterprise web applications, internal tools, and SaaS starters.

---

## 1. Solution Structure & Layer Boundaries

The repository strictly enforces dependency direction: outer layers depend inward toward domain entities, while the domain layer maintains zero dependencies on external frameworks or databases.

```mermaid
graph TD
    Web["Foundation.Web (Razor Pages, Minimal APIs, Middleware)"]
    App["Foundation.Application (Use Cases, CQRS Handlers, DTOs)"]
    Infra["Foundation.Infrastructure (EF Core DbContext, Identity, Storage)"]
    Domain["Foundation.Domain (Entities, Value Objects, Enums)"]
    MigSql["Foundation.Migrations.SqlServer (SQL Server EF Migrations)"]
    MigMySql["Foundation.Migrations.MySql (MySQL EF Migrations)"]

    Web --> App
    Web --> Infra
    Web --> MigSql
    Web --> MigMySql
    Infra --> App
    App --> Domain
    Infra --> Domain
    MigSql --> Infra
    MigMySql --> Infra
```

### Layer Responsibilities

| Project | Responsibility | Dependencies |
| :--- | :--- | :--- |
| `Foundation.Domain` | Core entities (`UserProfile`, `CmsPage`, `CmsPost`, `MediaFile`, `AuditEntry`), business rules, and enums. | None (pure .NET standard library). |
| `Foundation.Application` | CQRS use cases, validation, DTOs, domain service abstractions (`IAppDbContext`, `ICurrentUser`, `IFileStorageService`, `IMarkdownRenderer`). | `Foundation.Domain`. |
| `Foundation.Infrastructure` | EF Core `AppDbContext`, Identity stores, audit interceptors, local disk storage, SMTP email delivery, Markdig markdown pipeline. | `Foundation.Application`, `Foundation.Domain`, EF Core, ASP.NET Core Identity. |
| `Foundation.Migrations.SqlServer` | Dedicated Entity Framework Core migrations for Microsoft SQL Server. | `Foundation.Infrastructure`. |
| `Foundation.Migrations.MySql` | Dedicated Entity Framework Core migrations for MySQL / MariaDB. | `Foundation.Infrastructure`. |
| `Foundation.Web` | Razor Pages (UI, Admin, Account management), Minimal APIs (`/api/v1`), security middleware, OpenTelemetry, Serilog. | All source projects. |

---

## 2. C4 Container Diagram

```mermaid
C4Container
    title Container Diagram for dotnet-web-foundation

    Person(user, "User / Editor / Admin", "Interacts via browser with Razor Pages or client apps via REST API")
    Container(webApp, "ASP.NET Core Web Application", ".NET 10, C#", "Renders Razor Pages, serves Minimal APIs (/api/v1), enforces security headers and rate limiting")
    ContainerDb(db, "Relational Database", "Microsoft SQL Server 2022 or MySQL 8.4", "Stores Identity accounts, user profiles, CMS markdown content, and audit trail logs")
    Container(storage, "Local Storage / Media Store", "File System (outside wwwroot)", "Stores uploaded media images with MIME whitelist validation")
    Container(smtp, "SMTP Server / Mailpit", "SMTP / Mailpit", "Receives transactional confirmation and password reset emails")

    Rel(user, webApp, "HTTPS requests", "TLS 1.3 / Port 5000 / 8080")
    Rel(webApp, db, "EF Core queries / mutations", "TCP / Port 1433 or 3306")
    Rel(webApp, storage, "Read/write streamed files", "Local filesystem I/O")
    Rel(webApp, smtp, "Send transactional emails", "SMTP / Port 1025")
```

---

## 3. Key Architectural Decisions

1. **Clean-Room & Zero Commercial Dependencies:** Zero commercial packages or restrictive licenses. No MediatR v12+, AutoMapper, or FluentAssertions v8+. CQRS handlers use clean first-party generic interfaces `ICommandHandler<TCommand, TResult>` and `IQueryHandler<TQuery, TResult>`.
2. **Provider-Agnostic Relational Migrations:** Separate migration assemblies allow seamless deployment to Microsoft SQL Server on Azure/AWS or MySQL on RDS/containers without dirty snapshot collisions.
3. **Hardened Defense-in-Depth:**
   - Content Security Policy (CSP) with dynamically generated cryptographic nonces per HTTP request.
   - Soft delete query filters and automatic JSON field-level mutation audit logs via EF Core interceptors.
   - IDOR prevention in every CQRS handler verifying caller authorization against `ICurrentUser`.

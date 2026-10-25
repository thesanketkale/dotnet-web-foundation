# ADR-0004: Dual Database Provider & Dedicated Migration Assemblies

## Status
Accepted

## Date
2026-10-25

## Context
Production web applications require deployment flexibility. Small setups frequently prefer MySQL / MariaDB, while enterprise environments mandate Microsoft SQL Server (MSSQL / Azure SQL). Entity Framework Core supports multiple providers, but sharing a single migrations folder across both SQL Server and MySQL leads to provider-specific annotation conflicts, divergent SQL syntax, and runtime migration failures.

## Decision
1. **Dynamic Provider Switching via Configuration:**
   The active provider is chosen via configuration setting `Database:Provider`:
   - `SqlServer`: Uses `Microsoft.EntityFrameworkCore.SqlServer`.
   - `MySql`: Uses `Pomelo.EntityFrameworkCore.MySql`.
2. **Dedicated Migration Assemblies:**
   Maintain two distinct migration assemblies:
   - `Foundation.Migrations.SqlServer`
   - `Foundation.Migrations.MySql`
   Each assembly contains provider-compiled EF Core migrations (`Up()` and `Down()`). The DbContext (`AppDbContext`) in `Foundation.Infrastructure` contains provider-agnostic Fluent API configurations (e.g. `HasMaxLength`, `IsRequired`, `HasIndex`, `HasQueryFilter`).
3. **Database Seed Data & Automatic Migrations:**
   In development and container environments, automatic migration application is configurable (`Database:ApplyMigrationsAtStartup = true`). A database initializer creates initial roles (`Admin`, `Editor`, `User`), an administrative user seeded with the password from environment variable `ADMIN_INITIAL_PASSWORD`, and demo CMS content.
4. **Integration Testing with Real Containers:**
   Never use EF Core `UseInMemoryDatabase` or SQLite for relational database tests, as they do not support relational constraints, concurrency tokens, or provider-specific SQL functions. Instead, integration tests use `Testcontainers` to spin up real Microsoft SQL Server and MySQL containers.

## Alternatives Considered and Rejected
- **SQLite in Production or Single Migration Directory:**
  - *Rejected:* Causes schema corruption and migration collisions when switching databases.
- **In-Memory Db Provider for Tests:**
  - *Rejected:* Masked foreign key violations, raw SQL syntax bugs, and concurrency token checks.

## Consequences
- **Positive:**
  - Seamless, battle-tested portability between SQL Server and MySQL without touching code.
  - Production-proven migrations with functional rollback (`Down()`) paths.
- **Negative:**
  - Modifying the domain schema requires generating migrations in both provider assemblies.

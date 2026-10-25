# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-10-25

### Added
- Greenfield modular ASP.NET Core foundation on .NET 10 LTS (`net10.0`).
- Solution architecture with clean layer separation: `Foundation.Domain`, `Foundation.Application`, `Foundation.Infrastructure`, `Foundation.Web`, and provider-specific migration assemblies.
- ASP.NET Core Identity with cookie authentication, email confirmation, password reset, account lockout, TOTP two-factor authentication, and single-use recovery codes.
- User profile management with avatar uploads, personal data export (JSON), and account deletion.
- Role-based admin dashboard (`Admin`, `Editor`) with user search, user locking/role assignment, and audit log exploration.
- Lightweight Markdown-driven CMS for Pages and Posts with Markdig rendering and HtmlSanitizer XSS protection.
- Dual database provider support for Microsoft SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`) and MySQL/MariaDB (`Pomelo.EntityFrameworkCore.MySql`) with isolated migration assemblies (`Foundation.Migrations.SqlServer`, `Foundation.Migrations.MySql`).
- EF Core interceptors for automated audit logging (`AuditInterceptor`) and soft deletes (`SoftDeleteInterceptor`).
- Local secure file storage (`LocalFileStorageService`) outside the web root with MIME validation and UUID file isolation.
- REST Minimal APIs under `/api/v1` with OpenAPI 3.1 generation and RFC 9457 Problem Details error handling.
- Security headers middleware (dynamic Content-Security-Policy with cryptographic nonces, HSTS, X-Content-Type-Options, X-Frame-Options).
- Rate limiting middleware on sensitive authentication endpoints.
- Structured logging with Serilog and OpenTelemetry instrumentation for traces, metrics, and OTLP export.
- Health check endpoints at `/health/live` and `/health/ready`.
- Test suites: unit tests, architecture tests (NetArchTest), integration tests (WebApplicationFactory with Testcontainers), and Playwright E2E smoke test.
- Hardened multi-stage non-root chiseled container `Dockerfile` and `docker-compose.yml` with SQL Server, MySQL, and Mailpit profiles.
- Automated verification scripts (`scripts/verify.sh`, `scripts/verify.ps1`, `scripts/check-licenses.ps1`).
- GitHub Actions CI/CD workflows with least-privilege permissions and SHA-pinned actions.

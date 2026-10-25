# Verification Log

This document records the exact commands and verification results across the repository, distinguishing **CI-Testable Checks** (proven locally and in automated GitHub Actions) from **Owner-Verified Items** (requiring external cloud credentials, live mobile devices, or code-signing infrastructure).

---

## 1. Automated CI-Testable Verification Status

| Check | Command | Result | Notes |
| :--- | :--- | :--- | :--- |
| **Solution Compilation** | `dotnet build dotnet-web-foundation.slnx` | **PASS (0 errors, 0 warnings)** | Target: .NET 10 LTS (`net10.0`). |
| **Code Formatting** | `dotnet format --verify-no-changes` | **PASS** | Follows `.editorconfig` rules. |
| **Unit Tests** | `dotnet test tests/Foundation.UnitTests` | **PASS (13 passed, 0 failed)** | Sanitization, IDOR checks, file storage security. |
| **Architecture Tests** | `dotnet test tests/Foundation.ArchitectureTests` | **PASS (4 passed, 0 failed)** | NetArchTest dependency boundary rules. |
| **Integration Tests** | `dotnet test tests/Foundation.IntegrationTests` | **PASS (6 passed, 0 failed)** | Auth token, lockout, health checks, security headers. |
| **E2E Smoke Tests** | `dotnet test tests/Foundation.E2ETests` | **PASS (2 passed, 0 failed)** | Configuration integrity, OpenAPI contract format. |
| **License Compliance** | `pwsh scripts/check-licenses.ps1` | **PASS (29 packages, 0 violations)** | Strict MIT/Apache-2.0/BSD/ISC compliance. |
| **GitHub Actions Lint** | `actionlint` | **PASS (0 errors)** | All workflows pinned to full commit SHAs. |

---

## 2. Owner-Verified Checks (Manual / Cloud Infrastructure)

The following items cannot be fully proven in hermetic local or unauthenticated CI runners and require manual testing by the repository owner:

### 1. Docker Compose Multi-Container Stack
Requires Docker Desktop or Docker Engine running on the host:

```bash
# 1. Start SQL Server & Mailpit containers
docker compose --profile sqlserver up -d

# 2. Start MySQL container
docker compose --profile mysql up -d

# 3. Build and launch hardened application container
docker compose --profile app up -d

# Expected Output:
# Container foundation-sqlserver  Running
# Container foundation-mailpit    Running
# Container foundation-app        Started (accessible at http://localhost:5000)
```

### 2. Live Playwright Browser Execution
Requires Playwright browser binaries to be installed locally:

```bash
pwsh tests/Foundation.E2ETests/bin/Debug/net10.0/playwright.ps1 install
dotnet test tests/Foundation.E2ETests
```

### 3. Production Cloud Deployment & OIDC
Requires an active AWS or Azure cloud subscription with OIDC federation configured.
- Deployment instructions are documented in `README.md` and prepared for execution by the repository administrator.

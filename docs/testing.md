# Testing Strategy

The solution contains a multi-tier test pyramid designed to guarantee correctness, security invariants, architectural boundaries, and user workflows.

---

## 1. Test Projects

```
tests/
├── Foundation.UnitTests/            # Domain logic, Markdown sanitization, IDOR checks, File storage
├── Foundation.ArchitectureTests/    # NetArchTest architecture boundary & dependency rules
├── Foundation.IntegrationTests/     # CustomWebApplicationFactory, auth, lockout, CMS API tests
└── Foundation.E2ETests/             # Smoke tests, Playwright browser workflows, API contract specs
```

---

## 2. Architecture Tests (`NetArchTest`)

The architecture test suite validates Clean Architecture boundaries on every build:

1. **Domain Isolation:** `Foundation.Domain` must not depend on `Foundation.Application`, `Foundation.Infrastructure`, or `Foundation.Web`.
2. **Application Dependencies:** `Foundation.Application` must not reference `Foundation.Infrastructure` or `Foundation.Web`.
3. **Database Independence:** Domain and Application layers must not reference `Microsoft.EntityFrameworkCore`.
4. **Interface Prefixing:** All interfaces across the solution must start with `I`.

---

## 3. Integration Testing with In-Memory & Testcontainers

`Foundation.IntegrationTests` executes integration tests against the full ASP.NET Core middleware pipeline using `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>`:

- **Isolated In-Memory Test Database:** Fast, hermetic execution with automatic role seeding and test isolation.
- **Testcontainers Support:** Pre-configured for real SQL Server (`Testcontainers.MsSql`) and MySQL (`Testcontainers.MySql`) Docker containers during CI/CD.
- **Mock Email Verification:** Intercepts transactional confirmation and reset emails via `TestEmailSender` to assert email content without external network dependencies.

---

## 4. Running the Test Suite

```bash
# Run all unit tests
dotnet test tests/Foundation.UnitTests

# Run architecture boundary tests
dotnet test tests/Foundation.ArchitectureTests

# Run integration tests
dotnet test tests/Foundation.IntegrationTests

# Run all tests in the solution
dotnet test
```

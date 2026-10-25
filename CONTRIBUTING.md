# Contributing to dotnet-web-foundation

Thank you for your interest in contributing to `dotnet-web-foundation`! We welcome bug reports, feature discussions, documentation improvements, and pull requests.

## Code of Conduct

All contributors and participants are expected to adhere to our [Code of Conduct](CODE_OF_CONDUCT.md) (Contributor Covenant 2.1).

## Clean-Room and Intellectual Property Standards

`dotnet-web-foundation` is an open-source clean-room template. By contributing, you agree that:
1. All contributions are your original work or come from permissive open-source codebases (MIT, Apache-2.0, BSD-2/3-Clause, ISC).
2. No proprietary, client-confidential, or internal corporate assets, secrets, or identifiers are included.
3. All dependencies must comply with our strict license policy (see [ADR-0003](docs/adr/ADR-0003-dependency-license-policy.md)). Commercial or dual-licensed packages (e.g., MediatR v12+, FluentAssertions v8+) will not be accepted.

## Development Workflow

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (10.0.100 or later)
- [Docker](https://www.docker.com/) (for running disposable SQL Server and MySQL test containers)
- Git

### Verification Scripts
Before submitting a pull request, ensure all verification checks pass locally:

**Fast Checks (< 30 seconds):**
```bash
# Linux/macOS
bash scripts/verify.sh fast

# Windows PowerShell
./scripts/verify.ps1 fast
```

**Full Checks (build, tests, format, analyzers):**
```bash
# Linux/macOS
bash scripts/verify.sh full

# Windows PowerShell
./scripts/verify.ps1 full
```

### Pull Request Guidelines
1. Branch from `main` using descriptive branch names (`feat/<slug>`, `fix/<slug>`, `chore/<slug>`).
2. Follow Conventional Commits format (e.g., `feat(auth): add passkey support`, `fix(cms): prevent slug duplication`).
3. Add unit and integration tests covering any new behavior or bug fix.
4. Ensure code formatting is clean (`dotnet format --verify-no-changes`).
5. Open a Pull Request referencing any related issues.

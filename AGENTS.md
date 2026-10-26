# AGENTS.md: dotnet-web-foundation

Shared rules for every AI agent on this repo, in Claude Code (through CLAUDE.md) and Google Antigravity (which reads this file directly). Keep it under ~150 lines. It is read at the start of every run. Write `TBD` rather than guessing.

## Project
- What it is: Modular ASP.NET Core web application foundation with Identity, user profiles, an admin area with a small CMS, EF Core on SQL Server or MySQL, observability and a hardened container image.
- Users: Developers building web applications and SaaS starters requiring an extensible, modular foundation.
- Related repos and contracts: `reusable-workflows` (GitHub Actions workflow library)

## Stack packs
<!-- One per line. Agents load {skills dir}/stack-<name>/SKILL.md for each. -->
- dotnet-web
- sql-db
- web-frontend
- github-actions

## Commands
- Install: `dotnet restore`
- Run locally: `docker compose --profile mysql up` or `--profile sqlserver up` → URL: http://localhost:8080 (or `dotnet run --project src/Foundation.Web` → URL: http://localhost:5080 with user-secrets)
- Verify fast: `bash scripts/verify.sh fast` (Windows PowerShell: `./scripts/verify.ps1 fast`)
- Verify full: `bash scripts/verify.sh full`
- Disposable database: Testcontainers databases that tests create and destroy, and the `foundation_dev` database in the compose profiles (agents may only touch this one)

## Git
- Base branch: main
- Branches: `feat/<slug>`, `fix/<slug>`, `chore/<slug>`
- PR: manual   <!-- auto = the agent team pushes the branch and opens a PR at the end -->

## Conventions
- This is a public MIT repository written clean-room: no code, assets, names or data from client or employer projects. Sample data is synthetic.
- The standards and this repository's requirements are in `.pipeline/SPEC.md` (local, never committed). Requirement IDs appear in AC lines as `AC-n [ID]` and in test names.
- Docs describe only what exists and was run. What an agent cannot run (push, live cloud, signing, a Unity Editor, GitHub settings) is a `[USER]` item in `docs/release-checklist.md` and is never described as tested.
- GitHub Actions: top-level `permissions: {}` plus minimal per-job permissions; `timeout-minutes` and `concurrency` on every workflow; actions pinned to a full SHA with the version in a comment; explicit `shell:`; LF line endings; no `${{ github.event.* }}` inside `run:` (use `env:`); OIDC for cloud login. Check with `actionlint`.
- Commits are small and follow `<type>(<scope>): <goal> [step N]`. Dates are real and history is never rewritten.
- Dependencies need MIT-compatible licences. Record stack choices and open-decision outcomes in `docs/adr/`.
- Layers: Web depends on Application, Infrastructure implements Application interfaces, and Domain has no framework references. NetArchTest enforces this, so those tests are never weakened.
- Application uses vertical slices with plain handler interfaces. No MediatR, AutoMapper, MassTransit or FluentAssertions. Every new package's licence is checked against the allow-list in `licences.allow.json`, and packages are added only when the plan names them.
- Nullable reference types and warnings as errors stay on. Package versions live in `Directory.Packages.props`.
- Errors use ProblemDetails. Every page and endpoint has an authorization policy or an explicit `AllowAnonymous` with a test. No admin route without a policy.
- EF Core: one migrations assembly per provider, and a model change needs a migration for both. Concurrency tokens, the soft-delete filter and UTC timestamps stay in place.
- Seed data is synthetic. The seeded admin password comes from `FOUNDATION_ADMIN_PASSWORD`, never from source.
- Markdown is rendered with Markdig and sanitised with HtmlSanitizer. Unsanitised content never reaches `Html.Raw`.

## Design
- Design system / tokens: One accent colour and a documented token set in `wwwroot/css/tokens.css`
- Direction: Clean editorial light for the public site and a restrained, dense admin
- Brand: One accent colour, neutral palette

## Safety: agents never do these. They prepare the command and hand it over.
- Deploy, publish, release, upload, sign or notarize builds
- `terraform` / `terragrunt` apply, destroy, import or any state command
- Any write to a database other than the disposable one above
- Change real secrets, `.env` values, CI/CD or production configuration
- Push to the base branch, force-push, rewrite published history
- Never push, tag, create releases, change GitHub settings or run `git filter-repo`; write the command into `docs/release-checklist.md` instead. Never print secret values: show only file, line, rule and the last four characters.
- `dotnet ef database update` against anything except the local or disposable database, and never `dotnet ef database drop` outside it.
- Publishing or deploying (`dotnet publish` to servers, Azure Web Deploy, IIS), and editing production appsettings or Key Vault.

## Known baseline failures
<!-- Pre-existing failures moved out of `fast`, with a note on why. Remove as they get fixed. -->

## Lessons
<!-- One line each, added only with the user's approval after a review found a repeatable mistake.
     Format: - [area] Do X, not Y, because Z. -->

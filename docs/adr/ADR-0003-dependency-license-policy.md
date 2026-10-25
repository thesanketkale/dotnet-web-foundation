# ADR-0003: Dependency License Policy & Elimination of Commercialized Libraries

## Status
Accepted

## Date
2026-10-25

## Context
Several widely used .NET ecosystem libraries have recently transitioned to commercial, dual-license, or restrictive models:
- MediatR (v12+ has restrictive enterprise terms and commercial licensing).
- AutoMapper (maintenance ceased / relicensed alternatives).
- FluentAssertions (v8+ transitioned to commercial Xceed licensing).
- MassTransit / Quartz (some enterprise extensions require paid licenses).

For an open-source template repository (MIT license), introducing packages that impose commercial fees or legal uncertainty on downstream users is strictly unacceptable.

## Decision
1. **Zero Commercial or Ambiguous Dependencies:** The repository strictly forbids dependencies with commercial, dual, or viral copyleft licenses. All direct and transitive dependencies must carry permissive open-source licenses: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, or ISC.
2. **Elimination of MediatR:** Instead of an external mediator library, we use direct C# vertical-slice handler interfaces:
   - `ICommandHandler<TCommand, TResult>`
   - `IQueryHandler<TQuery, TResult>`
   Registered directly in the DI container. Handlers are injected directly into Razor Pages or Minimal API route handlers.
3. **Elimination of AutoMapper:** Object mapping is implemented using explicit, strongly-typed C# extension methods (`ToDto()`, `ToEntity()`) or records with primary constructors. This eliminates reflection overhead, provides compile-time safety, and enables straightforward refactoring.
4. **Elimination of FluentAssertions:** Test assertions use standard xUnit assertions (`Assert.Equal`, `Assert.NotNull`, etc.) or Shouldly under MIT license.
5. **Automated License Enforcement:** Include a CI script (`scripts/check-licenses.ps1` and CI workflow step) that audits all installed NuGet packages against an allow-list, failing the build if an unapproved license is encountered.

## Alternatives Considered and Rejected
- **Pinning to older MediatR v11 or FluentAssertions v7:**
  - *Rejected:* Pinned legacy versions will accumulate unpatched security vulnerabilities and cannot receive .NET 10 optimizations. Clean native C# abstractions are superior and zero-cost.

## Consequences
- **Positive:**
  - Downstream commercial adopters can freely clone, customize, and deploy the template without license compliance risks.
  - Compile-time type checking and zero reflection overhead in mapping and request dispatching.
- **Negative:**
  - Developers familiar with `IMediator.Send` write direct handler injections, which is slightly more verbose but substantially more transparent.

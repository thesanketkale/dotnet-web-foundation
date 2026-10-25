# ADR-0001: Framework and UI Model Selection (.NET 10 LTS, Razor Pages + Minimal APIs)

## Status
Accepted

## Date
2026-10-25

## Context
The goal of `dotnet-web-foundation` is to provide a clean, modern, robust web application foundation with Identity, user profiles, an admin dashboard, a lightweight markdown CMS, and REST endpoints. The project must serve as an extensible starting point for production web applications without incurring unnecessary frontend build chain complexity or vendor lock-in.

We must decide:
1. Which .NET LTS release to target.
2. Which UI architecture to implement for the public site, user accounts, and administrative dashboard.
3. Which API style to use for machine-to-machine and REST interactions.

## Decision
1. **Target .NET 10 LTS (`net10.0`):** Target the current Long-Term Support release of .NET, leveraging modern C# 13/14 language features, enhanced JSON serialization, built-in OpenTelemetry instrumentation, and optimized runtime performance.
2. **Server-Rendered Razor Pages for UI:** Implement the public website, authentication flows, user profile management, and administrative CMS using ASP.NET Core Razor Pages. Razor Pages provides page-focused encapsulation, native Anti-Forgery token support, seamless integration with ASP.NET Core Identity, and zero client build overhead.
3. **Minimal APIs for `/api/v1`:** Implement REST endpoints using ASP.NET Core Minimal APIs grouped by route handlers. This provides high throughput, lower memory allocations, concise endpoint definitions, and native OpenAPI 3.1 generation.

## Alternatives Considered and Rejected
- **Blazor Server / Blazor WebAssembly:**
  - *Rejected:* Blazor Server requires persistent WebSocket connections (SignalR) that introduce memory overhead on server nodes, complicate load balancing, and degrade under poor network latency. Blazor WebAssembly introduces high initial bundle download sizes and unnecessary complexity for server-driven business applications.
- **Single Page Application (SPA) React / Vue / Angular:**
  - *Rejected:* Coupling ASP.NET Core with an external Node/npm build chain in a core starter template increases cognitive load, complicates authentication cookie handling (CSRF / SameSite), and dilutes focus from backend architectural rigor.
- **Traditional ASP.NET Core MVC (Controllers & Views):**
  - *Rejected:* MVC controllers tend to gather disparate actions into large controller classes, whereas Razor Pages colocate the view (`.cshtml`) and page model (`.cshtml.cs`) in a clean vertical slice per page.

## Consequences
- **Positive:**
  - Single dotnet toolchain with zero npm/webpack dependencies required to build and run the web application.
  - Built-in, battle-tested security controls (anti-forgery tokens, automatic model binding validation).
  - Fast server-side initial page rendering with high Lighthouse accessibility and SEO scores.
- **Negative:**
  - Rich dynamic client-side interactions require purposeful vanilla JavaScript rather than a reactive SPA framework.

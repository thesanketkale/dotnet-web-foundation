# ADR-0002: Authentication Architecture & Passkeys/WebAuthn vs TOTP

## Status
Accepted

## Date
2026-10-25

## Context
A primary requirement for `dotnet-web-foundation` is a resilient, enterprise-grade authentication system supporting cookie sign-in, email confirmation, password reset, account lockout, and two-factor authentication (2FA). The specification asks to decide at build time whether to implement FIDO2/WebAuthn passkeys natively in addition to standard TOTP and recovery codes.

## Decision
1. **Core Identity Model:** Implement standard ASP.NET Core Identity with `ApplicationUser : IdentityUser<Guid>` and `ApplicationRole : IdentityRole<Guid>`.
2. **Two-Factor Authentication:** Implement Time-Based One-Time Password (TOTP) conforming to RFC 6238 via standard authenticator apps (Google Authenticator, Microsoft Authenticator, 1Password) along with 10 single-use cryptographically generated recovery codes.
3. **Passkeys / WebAuthn Posture:** While passkeys are emerging in the web ecosystem, core ASP.NET Core Identity does not yet provide an out-of-the-box, zero-dependency Passkey provider without requiring third-party commercial packages or heavy FIDO2 libraries. We document the architectural extension point for WebAuthn in `docs/authentication.md` while keeping the foundation self-contained, standards-based, and zero-commercial-dependency with TOTP.
4. **Account Defense:** Enforce account lockout after 5 consecutive failed attempts for a 15-minute cooldown period. Enforce rate limiting on `/Account/Login`, `/Account/Register`, and `/Account/ForgotPassword`.

## Alternatives Considered and Rejected
- **Third-Party Commercial Identity (Auth0, Okta, Firebase):**
  - *Rejected:* Breaches the requirement for a self-hosted, independent foundation.
- **Third-Party Passkey NuGet packages (e.g. Fido2-Net-Lib):**
  - *Rejected:* Introduces additional transitive dependencies and license constraints. TOTP + recovery codes provide standard, robust 2FA across all platforms without third-party libraries.

## Consequences
- **Positive:**
  - 100% self-hosted with zero external SaaS or proprietary dependencies.
  - Universal 2FA support across all smartphones and password managers.
- **Negative:**
  - Passwordless passkey login is not enabled by default, but the codebase is structured to allow dropping in WebAuthn credentials when .NET Identity provides native support.

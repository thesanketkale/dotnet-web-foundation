# Authentication & Identity Architecture

`dotnet-web-foundation` provides an enterprise-ready authentication system built on **ASP.NET Core Identity** targeting .NET 10 LTS.

---

## 1. Security Policies & Hardening

| Feature | Configuration | Rationale |
| :--- | :--- | :--- |
| **Password Policy** | Min 12 chars, upper, lower, digit, symbol. | Defends against brute-force and credential stuffing attacks. |
| **Account Lockout** | Max 5 failed attempts; 15-minute lockout window. | Protects user accounts from online dictionary attacks. |
| **Email Confirmation** | Required before login (`RequireConfirmedEmail = true`). | Prevents account squatting and verified user contact. |
| **Rate Limiting** | 10 requests / minute per IP on authentication routes. | Prevents denial of service and automated credential spraying. |
| **Session Cookies** | `HttpOnly`, `SameSite = Lax`, `SecurePolicy = Always`. | Immune to client-side XSS cookie harvesting. |

---

## 2. Multi-Factor Authentication (2FA / TOTP)

- **Standard RFC 6238 TOTP:** Compatible with Google Authenticator, Microsoft Authenticator, and 1Password.
- **Base32 Secret Key & QR Code:** Formatted in 4-character blocks for visual entry or barcode scanner configuration (`otpauth://totp/...`).
- **Recovery Codes:** Generates 10 single-use recovery codes upon activation. Displayed to the user once with instructions to save in a secure vault.
- **Login Flow:** When 2FA is active, initial sign-in redirects to `/Account/LoginWith2fa` with a fallback link to `/Account/LoginWithRecoveryCode`.

---

## 3. Role-Based Access Control (RBAC)

The system seeds three hierarchical roles:

1. **`Admin`**: Full system administrative permissions. Access to User Management (`/Admin/Users`), Security Audit Logs (`/Admin/Audit`), CMS management, and role promotion/demotion.
2. **`Editor`**: Access to CMS Content Management (`/Admin/Pages`, `/Admin/Posts`, `/Admin/Media`). Can author, modify, and publish public articles.
3. **`User`**: Default authenticated user. Can update their own display name, bio, avatar, change password, manage 2FA, and export/delete their personal data.

---

## 4. GDPR Self-Service Compliance

- **Right to Portability (JSON Export):**
  Users can download an export of their personal data from `/Account/Manage/PersonalData`. This packages their profile information, authored pages/posts, and historical audit entries into a formatted `personal-data.json` file.
- **Right to Erasure (Account Deletion):**
  Users can permanently purge their account and associated profile records from `/Account/Manage/DeletePersonalData`, requiring password confirmation prior to execution.

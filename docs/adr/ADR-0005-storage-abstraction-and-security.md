# ADR-0005: File Storage Strategy and Security Boundary

## Status
Accepted

## Date
2026-10-25

## Context
The application supports avatar uploads for user profiles and a media library for CMS content (images, documents). File uploads present high security risks:
- Remote code execution if executable files are uploaded to an executed web root.
- Path traversal attacks allowing files to be written outside the intended directory.
- Stored cross-site scripting (XSS) via maliciously crafted SVG or HTML files.
- Denial of service via oversized uploads.

## Decision
1. **Interface Abstraction (`IFileStorageService`):**
   Define a clean storage interface in `Foundation.Application`:
   ```csharp
   public interface IFileStorageService
   {
       Task<string> SaveFileAsync(Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default);
       Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
       Task<Stream?> GetFileAsync(string relativePath, CancellationToken cancellationToken = default);
   }
   ```
2. **Local Storage Implementation Outside `wwwroot`:**
   Implement `LocalFileStorageService` storing files under an application data directory (`App_Data/uploads`), completely separated from static assets in `wwwroot`. Files are served strictly through an authenticated/authorized endpoint or specialized streaming middleware with `X-Content-Type-Options: nosniff` and proper caching headers.
3. **Hardened Upload Validation:**
   - Enforce maximum file size (5MB for avatars, 10MB for media).
   - Enforce a strict MIME whitelist (`image/png`, `image/jpeg`, `image/webp`).
   - Validate magic bytes (file signature) matching the declared MIME type.
   - Generate unique GUID-based filenames (`Guid.NewGuid().ToString("N") + extension`) to completely neutralize path traversal (`../`) and filename injection.
4. **Cloud Storage Readiness:**
   The `IFileStorageService` contract allows downstream adopters to drop in an AWS S3, Azure Blob Storage, or Cloudflare R2 implementation without touching application or domain code.

## Alternatives Considered and Rejected
- **Saving files directly into `wwwroot/uploads`:**
  - *Rejected:* Direct static file serving of user-uploaded files creates severe risks of arbitrary code execution or cache poisoning.
- **Storing files as byte arrays (BLOB) inside the database:**
  - *Rejected:* Causes database bloat, slows backup/restore cycles, and exhausts memory buffers during high concurrency.

## Consequences
- **Positive:**
  - Complete defense against path traversal, filename collisions, and arbitrary script execution.
  - Zero performance degradation on database backups.
- **Negative:**
  - Serving user-uploaded files requires streaming through a managed controller/endpoint rather than direct web server static file bypass.

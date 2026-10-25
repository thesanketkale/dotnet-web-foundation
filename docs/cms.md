# Content Management System (CMS) & Media Pipeline

`dotnet-web-foundation` includes a lightweight, secure Content Management System for static pages, blog articles, and asset storage.

---

## 1. Markdown Sanitization & XSS Prevention

All user-submitted Markdown content undergoes a two-stage parsing and sanitization pipeline before rendering:

1. **Parsing:** Parsed via `Markdig` using standard GitHub Flavored Markdown (GFM) extensions (tables, task lists, strikethrough).
2. **Sanitization:** Raw HTML output is processed through `HtmlSanitizer` to strip dangerous elements (`<script>`, `<iframe style="...">`, `<object>`, `<embed>`) and malicious attributes (`onerror`, `onload`, `javascript:...`).

```csharp
// Implemented in Foundation.Infrastructure/Services/MarkdigRenderer.cs
public string RenderHtml(string markdown)
{
    var rawHtml = Markdown.ToHtml(markdown, _pipeline);
    return _sanitizer.Sanitize(rawHtml);
}
```

---

## 2. Slug Normalization

URLs for pages (`/p/{slug}`) and blog posts (`/blog/{slug}`) are normalized using `SlugHelper`:
- Converted to lowercase.
- Whitespace and non-alphanumeric characters replaced with single hyphens (`-`).
- Stripped of leading/trailing hyphens.
- Verified for unique index constraints in `AppDbContext`.

---

## 3. Media Storage Architecture

- **Isolation outside wwwroot:** Media files are never saved directly under the publicly executable `wwwroot` directory. Files are written to an isolated directory configured via `Storage:LocalStoragePath` (`App_Data/uploads`).
- **UUID File Renaming:** Uploaded files are assigned random UUID filenames on disk to eliminate path traversal attacks and file overwrite conflicts.
- **MIME & Magic Byte Verification:** Files are validated against an allowlist of image MIME types (`image/jpeg`, `image/png`, `image/webp`, `image/svg+xml`).
- **Streaming Endpoint:** Files are streamed through a controlled endpoint (`/api/v1/content/media/{fileName}`) with caching headers (`Cache-Control: public, max-age=2592000, immutable`).

---

## 4. Automatic Audit Trail Interceptor

Every entity creation, modification, or soft-deletion is recorded by `AuditInterceptor`:
- Captured automatically on `DbContext.SaveChangesAsync()`.
- Records `UserId`, `EntityName`, `EntityId`, `Action` (`Create`, `Update`, `Delete`), and `TimestampUtc`.
- For modifications, JSON field-level diffs (`OldValue` vs `NewValue`) are computed and stored in the `AuditEntries` table, viewable by administrators at `/Admin/Audit`.

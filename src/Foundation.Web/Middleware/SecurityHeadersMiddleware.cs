namespace Foundation.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        var nonce = context.Items[NonceMiddleware.NonceKey]?.ToString() ?? string.Empty;

        // Content Security Policy with dynamic cryptographic nonce
        var csp = $"default-src 'self'; " +
                  $"script-src 'self' 'nonce-{nonce}'; " +
                  $"style-src 'self' 'nonce-{nonce}' https://fonts.googleapis.com; " +
                  $"font-src 'self' https://fonts.gstatic.com data:; " +
                  $"img-src 'self' data:; " +
                  $"connect-src 'self'; " +
                  $"frame-ancestors 'none'; " +
                  $"form-action 'self'; " +
                  $"base-uri 'self'; " +
                  $"object-src 'none';";

        headers["Content-Security-Policy"] = csp;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        if (!_env.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        await _next(context);
    }
}

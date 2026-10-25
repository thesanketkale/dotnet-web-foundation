using System.Security.Cryptography;

namespace Foundation.Web.Middleware;

public class NonceMiddleware
{
    private readonly RequestDelegate _next;
    public const string NonceKey = "CspNonce";

    public NonceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var nonceBytes = new byte[16];
        RandomNumberGenerator.Fill(nonceBytes);
        var nonce = Convert.ToBase64String(nonceBytes);

        context.Items[NonceKey] = nonce;

        await _next(context);
    }
}

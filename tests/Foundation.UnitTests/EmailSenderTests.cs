using System.Reflection;
using Foundation.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Foundation.UnitTests;

public class EmailSenderTests
{
    [Fact(DisplayName = "AC-3: SmtpEmailSender defines source-generated LoggerMessage methods", Skip = "pending AC-3 (step 1)")]
    public void AC_3_SmtpEmailSender_Defines_Source_Generated_LoggerMessage_Methods()
    {
        var methods = typeof(SmtpEmailSender)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        var loggerMessageMethods = methods
            .Where(m => m.GetCustomAttribute<LoggerMessageAttribute>() != null)
            .ToList();

        // AC-3 requires source-generated [LoggerMessage] partial methods for logging calls
        loggerMessageMethods.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "AC-3: SmtpEmailSender falls back to logger on SMTP failure", Skip = "pending AC-3 (step 1)")]
    public async Task AC_3_SmtpEmailSender_Falls_Back_To_Logger_On_Smtp_Failure()
    {
        var testLogger = new TestLogger<SmtpEmailSender>();
        var sender = new SmtpEmailSender(
            host: "127.0.0.1",
            port: 1, // Invalid port guaranteed to fail connection
            senderEmail: "noreply@foundation.local",
            senderName: "Foundation",
            username: null,
            password: null,
            enableSsl: false,
            logger: testLogger);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Must not throw an unhandled exception when SMTP delivery fails
        await Should.NotThrowAsync(async () =>
        {
            await sender.SendEmailAsync(
                "user@example.com",
                "Reset Password",
                "<p>Token: 123456</p>",
                cts.Token);
        });

        // Verify fallback log output contains recipient and content
        testLogger.Logs.ShouldContain(l => l.Level == LogLevel.Warning && l.Message.Contains("Failed to send email via SMTP"));
        testLogger.Logs.ShouldContain(l => l.Level == LogLevel.Information && l.Message.Contains("user@example.com"));
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Logs { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, formatter(state, exception), exception));
        }
    }
}

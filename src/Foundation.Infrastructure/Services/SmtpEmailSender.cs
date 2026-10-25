using System.Net;
using System.Net.Mail;
using Foundation.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Foundation.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string? _username;
    private readonly string? _password;
    private readonly bool _enableSsl;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        string host,
        int port,
        string senderEmail,
        string senderName,
        string? username,
        string? password,
        bool enableSsl,
        ILogger<SmtpEmailSender> logger)
    {
        _host = host;
        _port = port;
        _senderEmail = senderEmail;
        _senderName = senderName;
        _username = username;
        _password = password;
        _enableSsl = enableSsl;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {Recipient} with subject '{Subject}'", toEmail, subject);

        using var client = new SmtpClient(_host, _port)
        {
            EnableSsl = _enableSsl
        };

        if (!string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password))
        {
            client.Credentials = new NetworkCredential(_username, _password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_senderEmail, _senderName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail));

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email successfully sent to {Recipient}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email via SMTP to {Recipient}. Falling back to log output.", toEmail);
            // In dev / testcontainers without SMTP server active, log the message body so tokens can be retrieved
            _logger.LogInformation("--- EMAIL CONTENT ---\nTo: {To}\nSubject: {Subject}\nBody:\n{Body}\n--- END EMAIL ---",
                toEmail, subject, htmlMessage);
        }
    }
}

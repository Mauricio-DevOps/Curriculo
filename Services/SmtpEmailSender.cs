using System.Net;
using System.Net.Mail;
using Curriculo.Models;
using Microsoft.Extensions.Options;

namespace Curriculo.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendContactAsync(ContactFormViewModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new InvalidOperationException("EmailSettings:Host nao foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            throw new InvalidOperationException("EmailSettings:SenderEmail nao foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.RecipientEmail))
        {
            throw new InvalidOperationException("EmailSettings:RecipientEmail nao foi configurado.");
        }

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (string.IsNullOrWhiteSpace(_settings.UserName))
        {
            smtpClient.UseDefaultCredentials = true;
        }
        else
        {
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = FormatSubject(model.Subject),
            Body = BuildBody(model),
            IsBodyHtml = false
        };

        message.To.Add(_settings.RecipientEmail);

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            message.ReplyToList.Add(new MailAddress(model.Email, model.Name));
        }

        await smtpClient.SendMailAsync(message, cancellationToken);

        _logger.LogInformation("Email de contato enviado por {Email}", model.Email);
    }

    private static string FormatSubject(string? subject)
    {
        return string.IsNullOrWhiteSpace(subject)
            ? "Novo contato recebido pelo site"
            : $"Novo contato: {subject.Trim()}";
    }

    private static string BuildBody(ContactFormViewModel model)
    {
        return $"Nome: {model.Name}{Environment.NewLine}"
             + $"Email: {model.Email}{Environment.NewLine}"
             + $"Assunto: {model.Subject}{Environment.NewLine}{Environment.NewLine}"
             + model.Message;
    }
}

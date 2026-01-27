using Curriculo.Models;

namespace Curriculo.Services;

public interface IEmailSender
{
    Task SendContactAsync(ContactFormViewModel model, CancellationToken cancellationToken = default);
}

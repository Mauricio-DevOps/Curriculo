using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Curriculo.Models;
using Curriculo.Services;

namespace Curriculo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailSender _emailSender;

    public HomeController(ILogger<HomeController> logger, IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Sobre()
    {
        return View();
    }

    public IActionResult Habilidades()
    {
        return View();
    }

    public IActionResult ChatBot()
    {
        return View();
    }

    public IActionResult Projetos()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Contato()
    {
        return View(new ContactFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contato(ContactFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _emailSender.SendContactAsync(model, cancellationToken);
            TempData["ContactSuccess"] = "Obrigado! Sua mensagem foi enviada com sucesso.";
            return RedirectToAction(nameof(Contato));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem de contato.");
            ModelState.AddModelError(string.Empty, "Nao foi possivel enviar sua mensagem agora. Tente novamente em instantes.");
            return View(model);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

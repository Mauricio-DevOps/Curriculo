using System.Net.Http;
using System.Text;
using System.Text.Json;
using Curriculo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Curriculo.Controllers;

[ApiController]
[Route("api/assistant-chat")]
public class AssistantChatController : ControllerBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConversationApiSettings _settings;
    private readonly ILogger<AssistantChatController> _logger;

    public AssistantChatController(
        IHttpClientFactory httpClientFactory,
        IOptions<ConversationApiSettings> settings,
        ILogger<AssistantChatController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] AssistantChatRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { message = "O campo prompt e obrigatorio." });
        }



        if (string.IsNullOrWhiteSpace(_settings.Endpoint))
        {
            _logger.LogError("Conversation API endpoint is not configured.");
            return StatusCode(500, new { message = "Endpoint da API de conversa nao esta configurado." });
        }

        var payload = new
        {
            prompt = request.Prompt,
            previousResponseId = string.IsNullOrWhiteSpace(request.PreviousResponseId)
                ? null
                : request.PreviousResponseId,
            useFileSearch = request.UseFileSearch ?? _settings.UseFileSearch
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json")
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Conversation API returned status {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    content);

                return StatusCode((int)response.StatusCode, TryDeserialize(content));
            }

            var assistantResponse = JsonSerializer.Deserialize<AssistantChatResponse>(content, SerializerOptions);
            if (assistantResponse is null || string.IsNullOrWhiteSpace(assistantResponse.OutputText))
            {
                _logger.LogWarning("Conversation API response missing output text. Body: {Body}", content);
                return StatusCode(502, new { message = "Resposta invalida da API de conversa." });
            }

            return Ok(assistantResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar a API de conversa.");
            return StatusCode(500, new
            {
                message = "Erro interno ao consultar a API de conversa.",
                detail = ex.Message
            });
        }
    }

    private static object? TryDeserialize(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(body);
        }
        catch
        {
            return body;
        }
    }
}

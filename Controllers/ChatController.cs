using System.Text;
using System.Text.Json;
using Curriculo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Curriculo.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private const string RemoteEndpoint = "https://rg-apiia-dev-cvckd4dkedfefdey.brazilsouth-01.azurewebsites.net/api/IA/ask";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IHttpClientFactory httpClientFactory, ILogger<ChatController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var client = _httpClientFactory.CreateClient();
        var payload = JsonSerializer.Serialize(new
        {
            prompt = request.Prompt,
            previousResponseId = request.PreviousResponseId,
            useFileSearch = request.UseFileSearch
        }, SerializerOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, RemoteEndpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        try
        {
            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Assistant API returned {Status}. Payload: {Payload}", response.StatusCode, responseContent);
                return StatusCode((int)response.StatusCode, new
                {
                    message = "Falha ao consultar o assistente remoto.",
                    statusCode = (int)response.StatusCode,
                    payload = TryDeserialize(responseContent)
                });
            }

            var assistantResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, SerializerOptions);
            return Ok(assistantResponse ?? new ChatResponse { OutputText = "Sem resposta disponível." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar o assistente remoto.");
            return StatusCode(500, new
            {
                message = "Erro interno ao consultar o assistente.",
                detail = ex.Message
            });
        }
    }

    private static object? TryDeserialize(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch
        {
            return content;
        }
    }
}

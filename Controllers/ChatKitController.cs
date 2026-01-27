using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Curriculo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Curriculo.Controllers;

[ApiController]
[Route("api/chatkit")]
public class ChatKitController : ControllerBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChatKitSettings _settings;
    private readonly ILogger<ChatKitController> _logger;

    public ChatKitController(
        IHttpClientFactory httpClientFactory,
        IOptions<ChatKitSettings> options,
        ILogger<ChatKitController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = options.Value;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateSession(
        [FromBody] ChatKitSessionRequest? request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.WorkflowId))
        {
            return StatusCode(500, new { message = "ChatKit workflow ID is not configured." });
        }

        var apiKey = ResolveApiKey();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("ChatKit API key is not configured.");

            return StatusCode(500, new { message = "OpenAI API key is not configured." });
        }

        var userId = string.IsNullOrWhiteSpace(request?.DeviceId)
            ? Guid.NewGuid().ToString("N")
            : request.DeviceId!;

        var workflowPayload = new Dictionary<string, string>
        {
            ["id"] = _settings.WorkflowId!
        };

        if (!string.IsNullOrWhiteSpace(_settings.WorkflowVersion))
        {
            workflowPayload["version"] = _settings.WorkflowVersion!;
        }

        var postBody = new
        {
            workflow = workflowPayload,
            user = userId
        };

        var client = _httpClientFactory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.ResolvedSessionEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(postBody, SerializerOptions), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.TryAddWithoutValidation("OpenAI-Beta", "chatkit_beta=v1");

        try
        {
            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ChatKit session creation failed with status {StatusCode}. Payload: {Payload}",
                    response.StatusCode,
                    content);

                return StatusCode((int)response.StatusCode, new
                {
                    message = "Falha ao criar sessão do ChatKit.",
                    detail = TryDeserialize(content)
                });
            }

            var session = JsonSerializer.Deserialize<ChatKitSessionResponse>(content, SerializerOptions);
            if (session is null || string.IsNullOrWhiteSpace(session.ClientSecret))
            {
                _logger.LogWarning("ChatKit response did not contain a client secret.");
                return StatusCode(502, new { message = "Resposta inválida do ChatKit." });
            }

            return Ok(new { client_secret = session.ClientSecret });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar a sessão do ChatKit.");
            return StatusCode(500, new
            {
                message = "Erro interno ao criar sessão do ChatKit.",
                detail = ex.Message
            });
        }
    }

    private string? ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return _settings.ApiKey;
        }

        return Environment.GetEnvironmentVariable("OPENAI_API_KEY");
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

namespace Curriculo.Models;

public class ChatKitSettings
{
    public string? ApiKey { get; set; }
    public string? WorkflowId { get; set; }
    public string? WorkflowVersion { get; set; }
    public string? SessionEndpoint { get; set; }
    public string? DomainKey { get; set; }

    public string ResolvedSessionEndpoint =>
        string.IsNullOrWhiteSpace(SessionEndpoint)
            ? "https://api.openai.com/v1/chatkit/sessions"
            : SessionEndpoint;
}

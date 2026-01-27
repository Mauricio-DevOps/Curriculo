using System.Text.Json.Serialization;

namespace Curriculo.Models;

public class ChatKitSessionResponse
{
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
}

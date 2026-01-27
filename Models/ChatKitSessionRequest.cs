using System.Text.Json.Serialization;

namespace Curriculo.Models;

public class ChatKitSessionRequest
{
    [JsonPropertyName("deviceId")]
    public string? DeviceId { get; set; }
}

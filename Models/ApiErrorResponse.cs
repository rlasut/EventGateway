using System.Text.Json.Serialization;

namespace EventGateway.Models;

public sealed class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; set; }
}

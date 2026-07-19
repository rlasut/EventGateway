using System.Text.Json.Serialization;

namespace EventGateway.Models;

public sealed class EventResponse
{
    [JsonPropertyName("eventld")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("accountld")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("eventTimestamp")]
    public DateTimeOffset EventTimestamp { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

public sealed class DuplicateEventResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public EventResponse? Event { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventGateway.Models;

public sealed class CreateEventRequest : IValidatableObject
{
    [JsonPropertyName("eventId")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("accountId")]
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EventId))
        {
            yield return new ValidationResult("eventId is required.", new[] { nameof(EventId) });
        }

        if (string.IsNullOrWhiteSpace(AccountId))
        {
            yield return new ValidationResult("accountId is required.", new[] { nameof(AccountId) });
        }

        if (string.IsNullOrWhiteSpace(Type))
        {
            yield return new ValidationResult("type is required.", new[] { nameof(Type) });
        }
        else if (!string.Equals(Type, "CREDIT", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(Type, "DEBIT", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("type must be either CREDIT or DEBIT.", new[] { nameof(Type) });
        }

        if (Amount <= 0)
        {
            yield return new ValidationResult("amount must be greater than 0.", new[] { nameof(Amount) });
        }

        if (string.IsNullOrWhiteSpace(Currency))
        {
            yield return new ValidationResult("currency is required.", new[] { nameof(Currency) });
        }

        if (EventTimestamp == default)
        {
            yield return new ValidationResult("eventTimestamp is required and must be a valid ISO8601 date.", new[] { nameof(EventTimestamp) });
        }
    }
}

using System.Text.Json.Serialization;

namespace EventGateway.Models;

public sealed class AccountBalanceResponse
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }
}

public sealed class AccountDetailsResponse
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("recentTransactions")]
    public List<EventResponse> RecentTransactions { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace EventGateway.Models;

public sealed class EventEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AccountId { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Type { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset EventTimestamp { get; set; }

    public string? MetadataJson { get; set; }
}

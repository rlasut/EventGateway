using System.ComponentModel.DataAnnotations;

namespace EventGateway.Models;

public sealed class AccountBalanceEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string AccountId { get; set; } = string.Empty;

    [Required]
    public decimal Balance { get; set; }
}

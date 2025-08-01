using System.ComponentModel.DataAnnotations;

namespace Legal_IA.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] [MaxLength(100)] public string FirstName { get; set; } = string.Empty;

    [Required] [MaxLength(100)] public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required] [MaxLength(20)] public string DNI { get; set; } = string.Empty;

    [MaxLength(20)] public string? CIF { get; set; } = null;

    [MaxLength(255)] public string BusinessName { get; set; } = string.Empty;

    [MaxLength(500)] public string Address { get; set; } = string.Empty;

    [MaxLength(10)] public string PostalCode { get; set; } = string.Empty;

    [MaxLength(100)] public string City { get; set; } = string.Empty;

    [MaxLength(100)] public string Province { get; set; } = string.Empty;

    [MaxLength(20)] public string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
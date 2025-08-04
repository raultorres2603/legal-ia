using System;

namespace Legal_IA.DTOs
{
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string Email { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string? CIF { get; set; } = null;
        public string BusinessName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? BusinessName { get; set; }
        public string? Address { get; set; }
        public string? CIF { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string? CIF { get; set; } = null;
        public string BusinessName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateUserOrchestrationInput
    {
        public Guid UserId { get; set; }
        public UpdateUserRequest UpdateRequest { get; set; } = default!;
    }
}

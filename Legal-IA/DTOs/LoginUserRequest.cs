using System.ComponentModel.DataAnnotations;

namespace Legal_IA.DTOs
{
    public class LoginUserRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}


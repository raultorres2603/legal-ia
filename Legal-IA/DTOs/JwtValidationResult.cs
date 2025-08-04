namespace Legal_IA.DTOs
{
    public class JwtValidationResult
    {
        public bool IsValid { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public Dictionary<string, string>? Claims { get; set; }
    }
}


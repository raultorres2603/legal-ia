using Legal_IA.Models;

namespace Legal_IA.DTOs;

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string CIF { get; set; } = string.Empty;
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
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateDocumentRequest
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public int? Quarter { get; set; }
    public int? Year { get; set; }
    public bool IsTemplate { get; set; } = false;
}

public class UpdateDocumentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public DocumentStatus? Status { get; set; }
    public string? Tags { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public int? Quarter { get; set; }
    public int? Year { get; set; }
}

public class DocumentGenerationRequest
{
    public Guid DocumentId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string CIF { get; set; } = string.Empty;
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

public class DocumentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool IsTemplate { get; set; }
    public string Tags { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int? Quarter { get; set; }
    public int? Year { get; set; }
}

/// <summary>
///     Request for AI-generated document creation
/// </summary>
public class GenerateDocumentRequest
{
    public Guid UserId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string> UserPrompts { get; set; } = new();
    public string AdditionalContext { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; } = "EUR";
    public int? Quarter { get; set; }
    public int? Year { get; set; }
}

/// <summary>
///     Response for AI document generation with metadata
/// </summary>
public class GenerateDocumentResponse
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public DocumentStatus Status { get; set; }
    public string BlobPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
///     Request for document regeneration with updated prompts
/// </summary>
public class RegenerateDocumentRequest
{
    public Guid DocumentId { get; set; }
    public List<string> UpdatedPrompts { get; set; } = new();
    public string? UpdatedContext { get; set; }
    public string? Reason { get; set; }
}

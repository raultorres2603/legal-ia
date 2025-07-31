using System.ComponentModel.DataAnnotations;

namespace Legal_IA.Models;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid UserId { get; set; }

    [Required] [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [Required] public DocumentType Type { get; set; }

    [MaxLength(1000)] public string Description { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [MaxLength(500)] public string FilePath { get; set; } = string.Empty;

    [MaxLength(100)] public string FileName { get; set; } = string.Empty;

    [MaxLength(50)] public string FileFormat { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GeneratedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public bool IsTemplate { get; set; } = false;

    [MaxLength(500)] public string Tags { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    [MaxLength(10)] public string Currency { get; set; } = "EUR";

    public int? Quarter { get; set; }
    public int? Year { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}

public enum DocumentType
{
    Invoice = 1,
    Expense = 2,
    VATReturn = 3, // Declaración de IVA
    IRPFReturn = 4, // Declaración de IRPF
    IncomeStatement = 5, // Declaración de ingresos
    SocialSecurityForm = 6, // Formulario Seguridad Social
    ExpenseReport = 7, // Informe de gastos
    Contract = 8, // Contrato
    Receipt = 9, // Recibo
    TaxForm = 10, // Formulario fiscal
    BusinessPlan = 11, // Plan de negocio
    LegalDocument = 12, // Documento legal
    Other = 99
}

public enum DocumentStatus
{
    Draft = 1,
    InProgress = 2,
    Generated = 3,
    Submitted = 4,
    Approved = 5,
    Rejected = 6,
    Archived = 7
}
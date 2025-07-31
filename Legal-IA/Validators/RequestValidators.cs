using FluentValidation;
using Legal_IA.DTOs;

namespace Legal_IA.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.DNI)
            .NotEmpty().WithMessage("DNI is required")
            .Matches(@"^\d{8}[A-Za-z]$").WithMessage("DNI must have 8 digits followed by a letter");

        RuleFor(x => x.CIF)
            .NotEmpty().WithMessage("CIF is required")
            .Matches(@"^[A-Za-z]\d{7}[A-Za-z0-9]$").WithMessage("Invalid CIF format");

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{5}$").WithMessage("Postal code must be 5 digits")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Phone)
            .Matches(@"^(\+34|0034|34)?[6789]\d{8}$").WithMessage("Invalid Spanish phone number")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class CreateDocumentRequestValidator : AbstractValidator<CreateDocumentRequest>
{
    public CreateDocumentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Document title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid document type");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Quarter)
            .InclusiveBetween(1, 4).WithMessage("Quarter must be between 1 and 4")
            .When(x => x.Quarter.HasValue);

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, DateTime.Now.Year + 1).WithMessage("Year must be between 2020 and next year")
            .When(x => x.Year.HasValue);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .When(x => x.Amount.HasValue);
    }
}

public class GenerateDocumentRequestValidator : AbstractValidator<GenerateDocumentRequest>
{
    public GenerateDocumentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Valid document type is required");

        RuleFor(x => x.UserPrompts)
            .NotEmpty().WithMessage("At least one user prompt is required")
            .Must(prompts => prompts.Count <= 10).WithMessage("Maximum 10 prompts allowed")
            .Must(prompts => prompts.All(p => !string.IsNullOrWhiteSpace(p)))
            .WithMessage("All prompts must contain text");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AdditionalContext)
            .MaximumLength(5000).WithMessage("Additional context cannot exceed 5000 characters");

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Tags));

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.Currency)
            .MaximumLength(10).WithMessage("Currency code cannot exceed 10 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Quarter)
            .InclusiveBetween(1, 4).WithMessage("Quarter must be between 1 and 4")
            .When(x => x.Quarter.HasValue);

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2030).WithMessage("Year must be between 2020 and 2030")
            .When(x => x.Year.HasValue);
    }
}

public class RegenerateDocumentRequestValidator : AbstractValidator<RegenerateDocumentRequest>
{
    public RegenerateDocumentRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.UpdatedPrompts)
            .NotEmpty().WithMessage("At least one updated prompt is required")
            .Must(prompts => prompts.Count <= 10).WithMessage("Maximum 10 prompts allowed")
            .Must(prompts => prompts.All(p => !string.IsNullOrWhiteSpace(p)))
            .WithMessage("All prompts must contain text");

        RuleFor(x => x.UpdatedContext)
            .MaximumLength(5000).WithMessage("Updated context cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.UpdatedContext));

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

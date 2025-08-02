using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Models;

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
            .NotEmpty().WithMessage("CIF is required for businesses")
            .Matches(@"^[A-Za-z]\d{7}[A-Za-z0-9]$").WithMessage("Invalid CIF format")
            .When(x => !string.IsNullOrWhiteSpace(x.BusinessName));

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{5}$").WithMessage("Postal code must be 5 digits")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Phone)
            .Matches(@"^(\+34|0034|34)?[6789]\d{8}$").WithMessage("Invalid Spanish phone number")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.CIF)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.BusinessName))
            .WithMessage("CIF is required when BusinessName is provided.");
        // Add other rules as needed
    }
}

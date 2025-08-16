using FluentValidation;
using Legal_IA.DTOs;

namespace Legal_IA.Validators;

public class BatchUpdateInvoiceItemRequestValidator : AbstractValidator<BatchUpdateInvoiceItemRequest>
{
    public BatchUpdateInvoiceItemRequestValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("ItemId must not be empty.");
        RuleFor(x => x.UpdateRequest)
            .NotNull().WithMessage("UpdateRequest must not be null.")
            .SetValidator(new UpdateInvoiceItemRequestValidator());
    }
}
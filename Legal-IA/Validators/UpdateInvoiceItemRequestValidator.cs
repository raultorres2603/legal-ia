using FluentValidation;
using Legal_IA.DTOs;

namespace Legal_IA.Validators;

public class UpdateInvoiceItemRequestValidator : AbstractValidator<UpdateInvoiceItemRequest>
{
    public UpdateInvoiceItemRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Description != null || x.Quantity != null || x.UnitPrice != null || x.VAT != null ||
                       x.IRPF != null || x.Total != null)
            .WithMessage("At least one field must be provided for update.");
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.VAT).GreaterThanOrEqualTo(0).When(x => x.VAT.HasValue);
        RuleFor(x => x.IRPF).GreaterThanOrEqualTo(0).When(x => x.IRPF.HasValue);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0).When(x => x.Total.HasValue);
    }
}
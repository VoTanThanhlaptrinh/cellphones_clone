using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Demand;

public class CreateDemandRequestValidator : AbstractValidator<CreateDemandRequest>
{
    public CreateDemandRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage(localizer["CategoryIdRequired"])
            .GreaterThan(0).WithMessage(localizer["CategoryIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["DemandNameRequired"])
            .MaximumLength(255).WithMessage(localizer["DemandNameMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

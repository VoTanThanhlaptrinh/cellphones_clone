using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.Demand;

public class UpdateDemandRequestValidator : AbstractValidator<UpdateDemandRequest>
{
    public UpdateDemandRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Demand ID must be greater than 0");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue).WithMessage(localizer["CategoryIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Name)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Name)).WithMessage(localizer["DemandNameMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

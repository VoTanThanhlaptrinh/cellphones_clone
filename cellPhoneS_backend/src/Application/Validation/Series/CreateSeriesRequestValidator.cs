using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.Series;

public class CreateSeriesRequestValidator : AbstractValidator<CreateSeriesRequest>
{
    public CreateSeriesRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.BrandId)
            .NotNull().WithMessage(localizer["BrandIdRequired"])
            .GreaterThan(0).WithMessage(localizer["BrandIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["SeriesNameRequired"])
            .MaximumLength(255).WithMessage(localizer["SeriesNameMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

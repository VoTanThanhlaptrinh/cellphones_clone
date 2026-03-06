using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.Series;

public class UpdateSeriesRequestValidator : AbstractValidator<UpdateSeriesRequest>
{
    public UpdateSeriesRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Series ID must be greater than 0");

        RuleFor(x => x.BrandId)
            .GreaterThan(0).When(x => x.BrandId.HasValue).WithMessage(localizer["BrandIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Name)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Name)).WithMessage(localizer["SeriesNameMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

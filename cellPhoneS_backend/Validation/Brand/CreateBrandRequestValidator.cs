using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.Brand;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage(localizer["CategoryIdRequired"])
            .GreaterThan(0).WithMessage(localizer["CategoryIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["BrandNameRequired"])
            .MaximumLength(255).WithMessage(localizer["BrandNameMaxLength"]);

        RuleFor(x => x.ImageId)
            .GreaterThan(0).When(x => x.ImageId.HasValue).WithMessage(localizer["ImageIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

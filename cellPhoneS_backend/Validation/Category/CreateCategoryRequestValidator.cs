using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.Category;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["CategoryNameRequired"])
            .MaximumLength(255).WithMessage(localizer["CategoryNameMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

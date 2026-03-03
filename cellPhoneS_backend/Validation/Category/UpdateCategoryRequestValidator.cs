using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.Category;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");

        RuleFor(x => x.Name)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Name)).WithMessage(localizer["CategoryNameMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class SpecificationDTOValidator : AbstractValidator<SpecificationDTO>
{
    public SpecificationDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["SpecificationNameRequired"])
            .MaximumLength(255).WithMessage(localizer["SpecificationNameMaxLength"]);

        RuleFor(x => x.SpecDetails)
            .NotNull().WithMessage(localizer["SpecificationDetailsRequired"])
            .Must(list => list != null && list.Any())
                .WithMessage(localizer["SpecificationDetailsMustHaveAtLeastOne"]);

        When(x => x.SpecDetails != null, () =>
        {
            RuleForEach(x => x.SpecDetails!)
                .SetValidator(new SpecificationDetailDTOValidator(localizer));
        });
    }
}

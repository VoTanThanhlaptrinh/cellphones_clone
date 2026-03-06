using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class SpecificationDetailDTOValidator : AbstractValidator<SpecificationDetailDTO>
{
        public SpecificationDetailDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["SpecificationDetailNameRequired"])
            .MaximumLength(255).WithMessage(localizer["SpecificationDetailNameMaxLength"]);

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage(localizer["SpecificationDetailValueRequired"])
            .MaximumLength(1000).WithMessage(localizer["SpecificationDetailValueMaxLength"]);
    }
}

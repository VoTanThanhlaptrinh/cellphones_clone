using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class ColorDTOValidator : AbstractValidator<ColorDTO>
{
    public ColorDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["ColorNameRequired"])
            .MaximumLength(100).WithMessage(localizer["ColorNameMaxLength"]);

        When(x => !string.IsNullOrEmpty(x.ColorCode), () =>
        {
            RuleFor(x => x.ColorCode)
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
                .WithMessage(localizer["ColorCodeInvalid"]);
        });
    }
}

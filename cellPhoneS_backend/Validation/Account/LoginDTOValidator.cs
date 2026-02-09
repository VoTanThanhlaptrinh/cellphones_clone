using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellphones_backend.Validation.Account;

public class LoginDTOValidator : AbstractValidator<LoginDTO>
{
    public LoginDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Email)
        .NotEmpty().WithMessage(localizer["EmailRequired"]);

        RuleFor(x => x.Password)
        .NotEmpty().WithMessage(localizer["PasswordRequired"]);

    }
}

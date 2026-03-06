using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class CartValidator : AbstractValidator<CartRequest>
{
   public CartValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.productId)
            .NotEmpty().WithMessage(localizer["ProductIdRequired"]);

        RuleFor(x => x.productId)
            .GreaterThan(0)
            .WithMessage(localizer["ProductIdMustBeGreaterThanZero"]);

        RuleFor(x => x.colorId)
            .NotEmpty().WithMessage(localizer["ColorIdRequired"]);
        RuleFor(x => x.colorId)
            .GreaterThan(0)
            .WithMessage(localizer["ColorIdMustBeGreaterThanZero"]);
    }
}

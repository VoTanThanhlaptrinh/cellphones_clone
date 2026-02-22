using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class StoreDTOValidator : AbstractValidator<StoreDTO>
{
    public StoreDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.StoreHouseId)
            .NotEmpty().WithMessage(localizer["StoreHouseIdRequired"])
            .GreaterThan(0).WithMessage(localizer["StoreHouseIdMustBeGreaterThanZero"]);

        RuleFor(x => x.ColorId)
            .NotEmpty().WithMessage(localizer["ColorIdRequired"])
            .GreaterThan(0).WithMessage(localizer["ColorIdMustBeGreaterThanZero"]);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage(localizer["QuantityMustBeGreaterOrEqualZero"]);
    }
}

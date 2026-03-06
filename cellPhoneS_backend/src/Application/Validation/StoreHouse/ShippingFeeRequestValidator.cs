using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation;

public class ShippingFeeRequestValidator : AbstractValidator<ShippingFeeRequest>
{
    public ShippingFeeRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.CartDetails)
            .NotEmpty().WithMessage(localizer["CartDetailsRequired"]);

        RuleFor(x => x.ProvinceName)
            .NotEmpty().WithMessage(localizer["ProvinceRequired"]);

        RuleFor(x => x.DistrictName)
            .NotEmpty().WithMessage(localizer["DistrictRequired"]);
    }
}

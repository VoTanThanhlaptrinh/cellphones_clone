using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Order;

public class DeliveryOrderRequestValidator : AbstractValidator<DeliveryOrderRequest>
{
    public DeliveryOrderRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.CartDetailIds)
            .NotNull().WithMessage(localizer["CartDetailsRequired"])
            .Must(list => list != null && list.Any())
            .WithMessage(localizer["CartDetailsMustHaveAtLeastOne"]);

        RuleFor(x => x.ProvinceName)
            .NotEmpty().WithMessage(localizer["ProvinceRequired"])
            .MaximumLength(255).WithMessage(localizer["ProvinceMaxLength"]);

        RuleFor(x => x.DistrictName)
            .NotEmpty().WithMessage(localizer["DistrictRequired"])
            .MaximumLength(255).WithMessage(localizer["DistrictMaxLength"]);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage(localizer["StreetRequired"])
            .MaximumLength(500).WithMessage(localizer["StreetMaxLength"]);
    }
}

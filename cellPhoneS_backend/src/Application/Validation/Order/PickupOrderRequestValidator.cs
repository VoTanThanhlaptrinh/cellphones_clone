using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Order;

public class PickupOrderRequestValidator : AbstractValidator<PickupOrderRequest>
{
    public PickupOrderRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        // Kiểm tra danh sách sản phẩm trong giỏ hàng
        RuleFor(x => x.CartDetailIds)
            .NotNull().WithMessage(localizer["CartDetailsRequired"])
            .Must(list => list != null && list.Any())
            .WithMessage(localizer["CartDetailsMustHaveAtLeastOne"]);

        // Kiểm tra ID cửa hàng đến lấy
        RuleFor(x => x.StoreHouseId)
            .NotNull().WithMessage(localizer["StoreHouseIdRequired"])
            .GreaterThan(0).WithMessage(localizer["StoreHouseIdInvalid"]);
    }
}
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation.Admin;

public class SetInventoryRequestValidator : AbstractValidator<SetInventoryRequest>
{
    public SetInventoryRequestValidator()
    {
        RuleFor(x => x.StoreHouseId).GreaterThan(0).WithMessage("Mã kho không hợp lệ.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Mã sản phẩm không hợp lệ.");
        RuleFor(x => x.ColorId).GreaterThan(0).WithMessage("Mã màu không hợp lệ.");
        RuleFor(x => x.Quantity).InclusiveBetween(0, InventoryLimits.MaxQuantity)
            .WithMessage($"Số lượng phải từ 0 đến {InventoryLimits.MaxQuantity}.");
    }
}

public class AdjustInventoryRequestValidator : AbstractValidator<AdjustInventoryRequest>
{
    public AdjustInventoryRequestValidator()
    {
        RuleFor(x => x.StoreHouseId).GreaterThan(0).WithMessage("Mã kho không hợp lệ.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Mã sản phẩm không hợp lệ.");
        RuleFor(x => x.ColorId).GreaterThan(0).WithMessage("Mã màu không hợp lệ.");
        RuleFor(x => x.Delta).NotEqual(0).WithMessage("Delta phải khác 0.")
            .InclusiveBetween(-InventoryLimits.MaxQuantity, InventoryLimits.MaxQuantity)
            .WithMessage($"Delta phải trong khoảng ±{InventoryLimits.MaxQuantity}.");
        RuleFor(x => x.Reason).MaximumLength(500).WithMessage("Lý do tối đa 500 ký tự.");
    }
}

internal static class InventoryLimits
{
    /// <summary>Chặn số quá lớn gây tràn int khi cộng dồn.</summary>
    public const int MaxQuantity = 1_000_000;
}

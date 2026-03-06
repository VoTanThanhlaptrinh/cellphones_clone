using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.StoreHouse;

public class UpdateStoreHouseRequestValidator : AbstractValidator<UpdateStoreHouseRequest>
{
    public UpdateStoreHouseRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("StoreHouse ID must be greater than 0");

        RuleFor(x => x.Street)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Street)).WithMessage(localizer["StreetMaxLength"]);

        RuleFor(x => x.District)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.District)).WithMessage(localizer["DistrictMaxLength"]);

        RuleFor(x => x.City)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.City)).WithMessage(localizer["CityMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

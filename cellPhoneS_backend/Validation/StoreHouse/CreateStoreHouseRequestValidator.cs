using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Validation.StoreHouse;

public class CreateStoreHouseRequestValidator : AbstractValidator<CreateStoreHouseRequest>
{
    public CreateStoreHouseRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage(localizer["StreetRequired"])
            .MaximumLength(255).WithMessage(localizer["StreetMaxLength"]);

        RuleFor(x => x.District)
            .NotEmpty().WithMessage(localizer["DistrictRequired"])
            .MaximumLength(255).WithMessage(localizer["DistrictMaxLength"]);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage(localizer["CityRequired"])
            .MaximumLength(255).WithMessage(localizer["CityMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null).WithMessage(localizer["StatusMaxLength"]);
    }
}

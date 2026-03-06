using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.J2O;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class AddProductRequestValidator : AbstractValidator<AddProductRequest>
{
    public AddProductRequestValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["ProductNameRequired"])
            .MaximumLength(255).WithMessage(localizer["ProductNameMaxLength"]);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage(localizer["BasePriceMustBeGreaterOrEqualZero"]);

        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0).WithMessage(localizer["SalePriceMustBeGreaterOrEqualZero"]);

        RuleFor(x => x.Version)
            .MaximumLength(255).When(x => x.Version != null)
            .WithMessage(localizer["VersionMaxLength"]);

        RuleFor(x => x.Status)
            .MaximumLength(50).When(x => x.Status != null)
            .WithMessage(localizer["StatusMaxLength"]);

        // ThumbnailImage validation
        RuleFor(x => x.ThumbnailImage)
            .NotNull().WithMessage(localizer["ThumbnailImageRequired"])
            .SetValidator(new ImageDTOValidator(localizer)!);

        // ProductImages validation
        When(x => x.ProductImages != null, () =>
        {
            RuleForEach(x => x.ProductImages!)
                .SetValidator(new ImageDTOValidator(localizer));
        });

        // Specifications validation
        When(x => x.Specifications != null, () =>
        {
            RuleForEach(x => x.Specifications!)
                .SetValidator(new SpecificationDTOValidator(localizer));
        });

        // CategoryId validation
        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage(localizer["CategoryIdRequired"])
            .GreaterThan(0).WithMessage(localizer["CategoryIdMustBeGreaterThanZero"]);

        // BrandId validation
        RuleFor(x => x.BrandId)
            .NotNull().WithMessage(localizer["BrandIdRequired"])
            .GreaterThan(0).WithMessage(localizer["BrandIdMustBeGreaterThanZero"]);

        // Commitments validation
        When(x => x.Commitments != null, () =>
        {
            RuleForEach(x => x.Commitments!)
                .NotEmpty().WithMessage(localizer["CommitmentNotEmpty"])
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(localizer["CommitmentNotWhiteSpace"]);
        });

        // Colors validation
        RuleFor(x => x.Colors)
            .NotNull().WithMessage(localizer["ColorsRequired"])
            .Must(list => list != null && list.Any())
            .WithMessage(localizer["ColorsMustHaveAtLeastOne"]);

        When(x => x.Colors != null, () =>
        {
            RuleForEach(x => x.Colors!)
                .SetValidator(new ColorDTOValidator(localizer));
        });

        // InitialInventory validation
        RuleFor(x => x.InitialInventory)
            .NotNull().WithMessage(localizer["InitialInventoryRequired"])
            .Must(list => list != null && list.Any())
            .WithMessage(localizer["InitialInventoryMustHaveAtLeastOne"]);

        When(x => x.InitialInventory != null, () =>
        {
            RuleForEach(x => x.InitialInventory!)
                .SetValidator(new StoreDTOValidator(localizer));
        });
    }
}

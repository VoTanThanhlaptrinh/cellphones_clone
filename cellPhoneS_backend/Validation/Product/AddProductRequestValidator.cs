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

        // ThumbnailImage (nếu gửi thì validate)
        When(x => x.ThumbnailImage != null, () =>
        {
            RuleFor(x => x.ThumbnailImage)
                .SetValidator(new ImageDTOValidator(localizer));
        });

        // ProductImages
        When(x => x.ProductImages != null, () =>
        {
            RuleForEach(x => x.ProductImages!)
                .SetValidator(new ImageDTOValidator(localizer));
        });

        // Specifications
        When(x => x.Specifications != null, () =>
        {
            RuleForEach(x => x.Specifications!)
                .SetValidator(new SpecificationDTOValidator(localizer));
        });

        // CategoryId và BrandId > 0 nếu được gửi
        When(x => x.CategoryId.HasValue, () =>
        {
            RuleFor(x => x.CategoryId!.Value)
                .GreaterThan(0).WithMessage(localizer["CategoryIdMustBeGreaterThanZero"]);
        });

        When(x => x.BrandId.HasValue, () =>
        {
            RuleFor(x => x.BrandId!.Value)
                .GreaterThan(0).WithMessage(localizer["BrandIdMustBeGreaterThanZero"]);
        });

        // Commitments: nếu gửi thì không chứa chuỗi rỗng / toàn khoảng trắng
        When(x => x.Commitments != null, () =>
        {
            RuleForEach(x => x.Commitments!)
                .NotEmpty().WithMessage(localizer["CommitmentNotEmpty"])
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(localizer["CommitmentNotWhiteSpace"]);
        });
    }
}

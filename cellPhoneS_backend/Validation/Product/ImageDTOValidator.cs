using System;
using cellphones_backend.Resources;
using cellPhoneS_backend.DTOs;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellPhoneS_backend.Validation.Product;

public class ImageDTOValidator : AbstractValidator<ImageDTO>
{
    public ImageDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        RuleFor(x => x.OriginUrl)
            .NotEmpty().WithMessage(localizer["ImageOriginUrlRequired"])
            .MaximumLength(1000).WithMessage(localizer["ImageOriginUrlMaxLength"])
            .Must(url =>
            {
                if (string.IsNullOrWhiteSpace(url)) return false;
                return Uri.IsWellFormedUriString(url, UriKind.Absolute);
            })
            .WithMessage(localizer["ImageOriginUrlInvalid"]);

        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage(localizer["ImageMimeTypeRequired"])
            .MaximumLength(100).WithMessage(localizer["ImageMimeTypeMaxLength"]);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["ImageNameRequired"])
            .MaximumLength(255).WithMessage(localizer["ImageNameMaxLength"]);

        RuleFor(x => x.Alt)
            .NotEmpty().WithMessage(localizer["ImageAltRequired"])
            .MaximumLength(255).WithMessage(localizer["ImageAltMaxLength"]);
    }
}

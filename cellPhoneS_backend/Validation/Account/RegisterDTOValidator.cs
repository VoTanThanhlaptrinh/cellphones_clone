using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellphones_backend.Validation.Account;

public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
{
    public RegisterDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        // Validate Email
        // - Không được để trống
        // - Phải đúng định dạng email chuẩn
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["EmailRequired"])
            .EmailAddress().WithMessage(localizer["EmailInvalid"]);

        // Validate ngày sinh (BirthDay)
        // - Không được để trống
        RuleFor(x => x.BirthDay)
            .NotEmpty().WithMessage(localizer["BirthDayRequired"]);

        // Validate họ tên đầy đủ (FullName)
        // - Không được để trống
        // - Tối đa 100 ký tự
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(localizer["FullNameRequired"])
            .MaximumLength(100).WithMessage(localizer["MaxName"]);

        // Validate số điện thoại (Phone)
        // - Không được để trống
        // - Bắt buộc phải có đúng 10 chữ số
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(localizer["PhoneRequired"])
            .Length(10).WithMessage(localizer["Phone10Digit"]);

        // Validate mật khẩu (Password)
        // - Không được để trống
        // - Ít nhất 8 ký tự
        // - Phải trùng với trường xác nhận mật khẩu (ConfPassword)
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer["PasswordRequired"])
            .MinimumLength(8).WithMessage(localizer["MinPassword"])
            .Equal(x => x.ConfPassword).WithMessage(localizer["PasswordNotMatchRequired"]);

        // Validate xác nhận mật khẩu (ConfPassword)
        // - Không được để trống
        RuleFor(x => x.ConfPassword)
            .NotEmpty().WithMessage(localizer["ConfirmPasswordRequired"]);

    }
}

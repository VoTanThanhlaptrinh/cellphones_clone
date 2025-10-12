using System;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace cellphones_backend.Validation.Account;

public class StudentRegisterDTOValidator : AbstractValidator<StudentRegisterDTO>
{
    private readonly long threshHold = 2 * 1024 * 1024;
    public StudentRegisterDTOValidator(IStringLocalizer<ShareResource> localizer)
    {
        // Validate Register object bằng một validator khác (RegisterDTOValidator)
        RuleFor(x => x.Register)
            .SetValidator(new RegisterDTOValidator(localizer));

        // Validate trường TypeUser: không được để trống
        RuleFor(x => x.TypeUser)
            .NotEmpty().WithMessage(localizer["TypeUserRequired"]);

        // Validate trường TypeSchool: không được để trống
        RuleFor(x => x.TypeSchool)
            .NotEmpty().WithMessage(localizer["TypeSchoolRequired"]);

        // Validate trường NameSchool: không được để trống
        RuleFor(x => x.NameSchool)
            .NotEmpty().WithMessage(localizer["NameSchoolRequired"]);

        // Validate trường IdStudent: không được để trống
        RuleFor(x => x.IdStudent)
            .NotEmpty().WithMessage(localizer["IdStudentRequired"]);

        // Validate trường NameInCard: không được để trống
        RuleFor(x => x.NameInCard)
            .NotEmpty().WithMessage(localizer["NameInCardRequired"]);

        // Validate ngày hết hạn thẻ
        // - Không được để trống
        // - Phải lớn hơn hoặc bằng ngày hôm nay (>= Today)
        RuleFor(x => x.ExpiredDateCard)
            .NotEmpty().WithMessage(localizer["ExpiredDateCardRequired"])
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage(localizer["ExpiredDateMustBeTodayOrFuture"]);

        // Validate EmailSchool
        // - Không được để trống
        // - Đúng format email cơ bản
        // - Phải có domain đuôi .edu (có thể kèm theo quốc gia như .edu.vn)
        RuleFor(x => x.EmailSchool)
            .NotEmpty().WithMessage(localizer["EmailRequired"])
            .EmailAddress().WithMessage(localizer["EmailInvalid"])
            .Matches(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.edu(\.[a-z]{2,})?$")
            .WithMessage(localizer["EmailSchoolInvalid"]);

        // Validate mặt trước của thẻ (FrontFaceCard)
        // - Không được để trống
        // - Phải có dữ liệu (LongLength > 0)
        // - Kích thước file phải nhỏ hơn ngưỡng cho phép (threshHold ~ 2MB)
        RuleFor(x => x.FrontFaceCard)
            .NotEmpty().WithMessage(localizer["FrontFaceCardRequired"])
            .Must(x => x != null && x.LongLength > 0).WithMessage(localizer["FrontFaceCardRequired"])
            .Must(x => x != null && x.LongLength < threshHold).WithMessage(localizer["FileExceed2Mb"]);

        // Validate mặt sau của thẻ (BehindFaceCard)
        // - Không được để trống
        // - Phải có dữ liệu (LongLength > 0)
        // - Kích thước file phải nhỏ hơn ngưỡng cho phép (threshHold ~ 2MB)
        RuleFor(x => x.BehindFaceCard)
            .NotEmpty().WithMessage(localizer["BehindFaceCardRequired"])
            .Must(x => x != null && x.LongLength > 0).WithMessage(localizer["BehindFaceCardRequired"])
            .Must(x => x != null && x.LongLength < threshHold).WithMessage(localizer["FileExceed2Mb"]);

    }
}


using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation.Admin;

public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    public UpdateUserStatusRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => s != null && (s.Trim().ToLowerInvariant() is UserStatuses.Active or UserStatuses.Locked))
            .WithMessage("Trạng thái chỉ có thể là active hoặc locked.");
        RuleFor(x => x.Reason).MaximumLength(500).WithMessage("Lý do tối đa 500 ký tự.");
    }
}

public class UpdateUserRolesRequestValidator : AbstractValidator<UpdateUserRolesRequest>
{
    public UpdateUserRolesRequestValidator()
    {
        RuleFor(x => x.Roles).NotNull().WithMessage("Danh sách role không được để trống.")
            .Must(r => r == null || r.All(n => !string.IsNullOrWhiteSpace(n)))
            .WithMessage("Tên role không được để trống.");
    }
}

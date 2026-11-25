using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation.Admin;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên role không được để trống.")
            .Matches(RoleNamePattern.Value).WithMessage(RoleNamePattern.Message);
    }
}

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id role không được để trống.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên role không được để trống.")
            .Matches(RoleNamePattern.Value).WithMessage(RoleNamePattern.Message);
    }
}

public class UpdateRolePermissionsRequestValidator : AbstractValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsRequestValidator()
    {
        RuleFor(x => x.Permissions).NotNull().WithMessage("Danh sách permission không được để trống (gửi [] để bỏ hết).");
    }
}

internal static class RoleNamePattern
{
    // Tên role được chuẩn hoá chữ hoa khi lưu nên cho phép nhập chữ thường.
    public const string Value = @"^\s*[A-Za-z0-9_\-]{2,50}\s*$";
    public const string Message = "Tên role gồm 2-50 ký tự chữ, số, '_' hoặc '-'.";
}

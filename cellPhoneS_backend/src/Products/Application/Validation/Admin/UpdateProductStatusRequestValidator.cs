using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation.Admin;

public class UpdateProductStatusRequestValidator : AbstractValidator<UpdateProductStatusRequest>
{
    public UpdateProductStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s == RecordStatuses.Active || s == RecordStatuses.Inactive)
            .WithMessage("Trạng thái chỉ nhận active hoặc inactive.");
    }
}

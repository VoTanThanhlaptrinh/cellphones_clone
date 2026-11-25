using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation.Admin;

public class UpdateReviewStatusRequestValidator : AbstractValidator<UpdateReviewStatusRequest>
{
    public UpdateReviewStatusRequestValidator()
    {
        RuleFor(x => x.Status).Must(s => ReviewStatuses.IsValid(s?.Trim().ToLowerInvariant()))
            .WithMessage("Trạng thái chỉ có thể là visible hoặc hidden.");
    }
}

public class AdminReviewFilterValidator : AbstractValidator<AdminReviewFilter>
{
    public AdminReviewFilterValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue)
            .WithMessage("rating phải từ 1 đến 5.");
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId.HasValue)
            .WithMessage("productId không hợp lệ.");
        RuleFor(x => x.Status).Must(s => ReviewStatuses.IsValid(s!.Trim().ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("status phải là visible hoặc hidden.");
        RuleFor(x => x.SortBy).Must(s => s!.ToLowerInvariant() is "createddate" or "rating")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("sortBy chỉ nhận createdDate hoặc rating.");
        RuleFor(x => x.SortDir).Must(s => s.ToLowerInvariant() is "asc" or "desc")
            .WithMessage("sortDir chỉ nhận asc hoặc desc.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("search tối đa 100 ký tự.");
        RuleFor(x => x).Must(x => x.From!.Value <= x.To!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("from phải nhỏ hơn hoặc bằng to.");
    }
}

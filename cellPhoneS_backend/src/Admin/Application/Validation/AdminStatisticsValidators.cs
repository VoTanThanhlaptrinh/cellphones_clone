using cellPhoneS_backend.DTOs.Requests;
using FluentValidation;

namespace cellPhoneS_backend.Validation.Admin;

/// <summary>Luật khoảng ngày chung của API thống kê. Mặc định khi bỏ trống from/to do service áp và kiểm tra lại.</summary>
public abstract class StatsRangeValidatorBase<T> : AbstractValidator<T> where T : StatsRangeQuery
{
    protected StatsRangeValidatorBase()
    {
        RuleFor(x => x).Must(x => x.From!.Value <= x.To!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("from phải nhỏ hơn hoặc bằng to.");
        RuleFor(x => x).Must(x => x.To!.Value.DayNumber - x.From!.Value.DayNumber + 1 <= 366)
            .When(x => x.From.HasValue && x.To.HasValue && x.From.Value <= x.To.Value)
            .WithMessage("Khoảng thời gian tối đa 366 ngày.");
    }
}

public class StatsRangeQueryValidator : StatsRangeValidatorBase<StatsRangeQuery> { }

public class StatsPeriodQueryValidator : StatsRangeValidatorBase<StatsPeriodQuery>
{
    public StatsPeriodQueryValidator()
    {
        RuleFor(x => x.GroupBy).Must(g => g?.Trim().ToLowerInvariant() is "day" or "week" or "month")
            .WithMessage("groupBy phải là day, week hoặc month.");
    }
}

public class TopProductsQueryValidator : StatsRangeValidatorBase<TopProductsQuery>
{
    public TopProductsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 50).WithMessage("limit phải từ 1 đến 50.");
        RuleFor(x => x.OrderBy).Must(o => o?.Trim().ToLowerInvariant() is "quantity" or "revenue")
            .WithMessage("orderBy phải là quantity hoặc revenue.");
    }
}

public class LowStockQueryValidator : AbstractValidator<LowStockQuery>
{
    public LowStockQueryValidator()
    {
        RuleFor(x => x.Threshold).GreaterThanOrEqualTo(0).WithMessage("threshold phải lớn hơn hoặc bằng 0.");
        RuleFor(x => x.Limit).InclusiveBetween(1, 100).WithMessage("limit phải từ 1 đến 100.");
    }
}

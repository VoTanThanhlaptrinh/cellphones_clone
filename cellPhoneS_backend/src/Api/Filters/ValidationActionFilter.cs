using cellphones_backend.DTOs.Responses;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace cellPhoneS_backend.Filters
{
    /// <summary>
    /// Chạy các <see cref="IValidator{T}"/> (FluentValidation) đã đăng ký cho từng argument của action.
    /// Có lỗi thì trả 400 <c>ApiResponse&lt;object?&gt;</c>: Message là lỗi đầu tiên, Data là danh sách <c>{ field, message }</c>.
    /// Argument không có validator thì bỏ qua.
    /// </summary>
    public class ValidationActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var failures = new List<ValidationFailure>();
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

                var result = await validator.ValidateAsync(
                    new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
                failures.AddRange(result.Errors);
            }

            if (failures.Count > 0)
            {
                var errors = failures.Select(f => new { field = f.PropertyName, message = f.ErrorMessage }).ToList();
                context.Result = new BadRequestObjectResult(new ApiResponse<object?>(failures[0].ErrorMessage, errors));
                return;
            }

            await next();
        }
    }
}

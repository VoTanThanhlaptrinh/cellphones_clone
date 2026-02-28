using System;

namespace cellPhoneS_backend.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public ServiceErrorType ErrorType { get; private set; } // Thêm thuộc tính này

    public static ServiceResult<T> Success(T data, string? message)
        => new ServiceResult<T> { IsSuccess = true, Data = data, Message = message, ErrorType = ServiceErrorType.General };

    // 2. Hàm Fail bây giờ nhận thêm ErrorType
    public static ServiceResult<T> Fail(string message, ServiceErrorType errorType = ServiceErrorType.General)
        => new ServiceResult<T> { IsSuccess = false, Message = message, ErrorType = errorType };
}
public enum ServiceErrorType
{
    General,
    InternalError,
    BadRequest,
    ValidationFailed,
    NotFound,
    Conflict,
    TooManyRequests,
    Unauthorized,
    Forbidden,
    Timeout,
    NotImplemented
}
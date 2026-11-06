namespace cellPhoneS_backend.Services;

/// <summary>
/// Lỗi nghiệp vụ do service ném ra. Service chỉ mô tả "chuyện gì sai", việc đổi sang
/// status code + ApiResponse do <c>ApiExceptionFilter</c> ở tầng Api đảm nhiệm.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
    protected AppException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Dữ liệu gửi lên không hợp lệ hoặc vi phạm quy tắc nghiệp vụ.</summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
    public BadRequestException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Không tìm thấy đối tượng được yêu cầu.</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Đối tượng đã tồn tại / xung đột với dữ liệu hiện có (VD: trùng tên).</summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Chưa xác thực hoặc thông tin đăng nhập / token không hợp lệ.</summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
    public UnauthorizedException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Đã xác thực nhưng không được phép thực hiện thao tác.</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Lỗi phía server hoặc dịch vụ bên ngoài (HTTP 500).</summary>
public class InternalServerException : AppException
{
    public InternalServerException(string message) : base(message) { }
    public InternalServerException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>Lỗi cổng kết nối hoặc dịch vụ bên ngoài không phản hồi (HTTP 502).</summary>
public class BadGatewayException : AppException
{
    public BadGatewayException(string message) : base(message) { }
    public BadGatewayException(string message, Exception? innerException) : base(message, innerException) { }
}

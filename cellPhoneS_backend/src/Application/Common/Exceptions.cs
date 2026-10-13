namespace cellPhoneS_backend.Services;

/// <summary>
/// Lỗi nghiệp vụ do service ném ra. Service chỉ mô tả "chuyện gì sai", việc đổi sang
/// status code + ApiResponse do <c>ApiExceptionFilter</c> ở tầng Api đảm nhiệm.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

/// <summary>Dữ liệu gửi lên không hợp lệ hoặc vi phạm quy tắc nghiệp vụ.</summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}

/// <summary>Không tìm thấy đối tượng được yêu cầu.</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Đối tượng đã tồn tại / xung đột với dữ liệu hiện có (VD: trùng tên).</summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Chưa xác thực hoặc thông tin đăng nhập / token không hợp lệ.</summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>Đã xác thực nhưng không được phép thực hiện thao tác.</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>Lỗi phía server hoặc dịch vụ bên ngoài (HTTP 500).</summary>
public class InternalServerException : AppException
{
    public InternalServerException(string message) : base(message) { }
}

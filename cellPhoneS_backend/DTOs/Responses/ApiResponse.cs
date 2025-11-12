namespace cellphones_backend.DTOs.Responses;

public record class ApiResponse<T>
(
    string Message,
    T Data,
    int Status
);

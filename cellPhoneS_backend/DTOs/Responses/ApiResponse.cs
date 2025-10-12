namespace cellphones_backend.DTOs.Responses;

public record class ApiResponse
(
    string Message,
    Object Data,
    int Status
);

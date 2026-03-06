namespace cellPhoneS_backend.DTOs.Requests;

public record CreateCategoryRequest(
    string Name,
    long? ParentCategoryId,
    string? Status = "active"
);

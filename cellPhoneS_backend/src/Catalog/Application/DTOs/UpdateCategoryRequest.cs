namespace cellPhoneS_backend.DTOs.Requests;

public record UpdateCategoryRequest(
    long Id,
    string? Name,
    long? ParentCategoryId,
    string? Status
);

namespace cellPhoneS_backend.DTOs.Email;

public record SendEmailRequest(
    string To,
    string Subject,
    string Body
);

public record BulkEmailRecipientRequest(
    string To,
    Dictionary<string, string>? Variables = null
);

public record SendBulkEmailRequest(
    string Subject,
    string Body,
    List<BulkEmailRecipientRequest> Recipients
);

public record MailItemResponse(
    string Id,
    string To,
    string Subject,
    string Body,
    string Status,
    string? Error,
    DateTime? SentAt,
    DateTime CreatedAt
);

public record BulkMailResponse(
    int Total,
    int QueuedCount,
    int SentCount,
    int FailedCount,
    List<MailItemResponse> Items
);

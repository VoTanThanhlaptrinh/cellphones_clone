namespace cellPhoneS_backend.DTOs.Requests;

/// <summary><c>Status</c> chỉ nhận Shipping hoặc Completed; <c>Note</c> được lưu qua audit log.</summary>
public record UpdateOrderStatusRequest(string Status, string? Note);

/// <summary>Lý do huỷ đơn, lưu qua audit log.</summary>
public record CancelOrderRequest(string? Reason);

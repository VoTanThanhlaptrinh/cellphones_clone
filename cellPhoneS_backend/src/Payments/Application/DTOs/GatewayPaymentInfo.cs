namespace cellPhoneS_backend.Services.Interface;

public record GatewayPaymentInfo(string? QrCode, string? BankBin, string? AccountNumber, string? AccountName, string? TransferContent);

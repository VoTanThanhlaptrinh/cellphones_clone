using System.Security.Cryptography;
using System.Text;

namespace cellPhoneS_backend.Services.Implement;

/// <summary>
/// Kiểm tra chữ ký callback của payment gateway:
/// <c>sha256=</c> + hex(HMAC-SHA256(callbackSecret, "&lt;timestamp&gt;.&lt;body thô&gt;")).
/// </summary>
public static class PaymentCallbackSignature
{
    public static readonly TimeSpan Tolerance = TimeSpan.FromMinutes(5);

    private const long MaxEpochSeconds = 253402300799; // 9999-12-31

    public static bool IsValid(string? secret, string? timestamp, string? signature, ReadOnlySpan<byte> body, DateTimeOffset now)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature)) return false;
        if (!long.TryParse(timestamp, out var epoch) || epoch < 0 || epoch > MaxEpochSeconds) return false;
        if ((now - DateTimeOffset.FromUnixTimeSeconds(epoch)).Duration() > Tolerance) return false;

        var prefix = Encoding.UTF8.GetBytes(timestamp + ".");
        var message = new byte[prefix.Length + body.Length];
        prefix.CopyTo(message, 0);
        body.CopyTo(message.AsSpan(prefix.Length));

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), message);
        var expected = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }
}

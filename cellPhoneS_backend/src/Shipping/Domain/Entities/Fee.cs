using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.Models;

public class Fee
{
    [Required]
    public long Id { get; set; }
    public ICollection<FeeDetail> FeeDetails { get; set; } = new List<FeeDetail>();

    /// <summary>
    /// Orders.FeeId là bắt buộc (FK tới Fees) nên đơn nào cũng cần một Fee;
    /// đơn không phí (nhận tại cửa hàng) có Fee không có chi tiết.
    /// </summary>
    public static Fee ForShipping(long? shippingFee)
    {
        var fee = new Fee();
        if (shippingFee is > 0)
        {
            var now = DateTime.UtcNow;
            fee.FeeDetails.Add(new FeeDetail { Value = shippingFee.Value, Name = "Shipping fee calculated by GHTK", Status = "active", CreateDate = now, UpdateDate = now });
        }
        return fee;
    }
}

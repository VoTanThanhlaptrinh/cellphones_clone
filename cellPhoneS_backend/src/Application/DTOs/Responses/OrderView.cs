using System;

namespace cellPhoneS_backend.DTOs.Responses;

public class OrderView
{
    public long Id { get; set; }
    public DateTime CreateDate { get; set; }
    public List<OrderDetailView> OrderDetails { get; set; } = new List<OrderDetailView>();
    public List<FeeView> Fee { get; set; } = new List<FeeView>();
};

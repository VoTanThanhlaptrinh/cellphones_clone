using System;

namespace cellPhoneS_backend.DTOs.Responses;

public record CartDetailView(long CartDetailId,int Quantity, long? ProductId,string? ImgUrl, string? ProductName, double BasePrice, double SalePrice);

    

    


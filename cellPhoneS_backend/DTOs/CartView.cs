using System;

namespace cellPhoneS_backend.DTOs;

public record CartView(long cartDetailId,long ProductId, string ProductName, string ProductImage, double basePrice, double SalePrice, int Quantity);

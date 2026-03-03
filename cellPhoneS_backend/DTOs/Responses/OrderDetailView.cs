using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs.Responses;

public record OrderDetailView(ProductView? Product, long? ColorId, string? ColorName, string? ColorImageUrl, int Quantity);

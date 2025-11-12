using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs;

public record ProductView(long? id,string? ImgUrl, string? ProductName, double BasePrice, double SalePrice);


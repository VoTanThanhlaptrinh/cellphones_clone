using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs.Responses;

public record HomeViewModel(IEnumerable<CategoryView>? categories);

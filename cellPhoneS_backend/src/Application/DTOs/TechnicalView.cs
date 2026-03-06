using System;

namespace cellPhoneS_backend.DTOs;

public record TechnicalView(Dictionary<string, Dictionary<string, string>>? data);

using System;

namespace cellPhoneS_backend.DTOs.Responses;

public record Oauth2GoogleCallBackResponse(string? FullName, string? Email, bool IsLoginSuccessful);

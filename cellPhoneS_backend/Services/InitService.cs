using System;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Services;

public interface InitService
{
    public Task<ApiResponse<HomeViewModel>> InitHomePage();
}

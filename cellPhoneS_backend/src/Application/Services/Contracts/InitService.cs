using System;
using cellphones_backend.DTOs.Responses;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using cellPhoneS_backend.Services;

namespace cellPhoneS_backend.Services;

public interface InitService
{
    Task<ServiceResult<HomeViewModel>> InitHomePage();
}

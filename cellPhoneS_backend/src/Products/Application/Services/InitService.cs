using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services;

public interface InitService
{
    Task<HomeViewModel> InitHomePage();
}

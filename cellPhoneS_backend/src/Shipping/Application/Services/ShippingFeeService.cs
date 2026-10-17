using cellPhoneS_backend.DTOs.Requests;

namespace cellPhoneS_backend.Services.Interface;

public interface ShippingFeeService
{
    public Task<long> CalculateAccurateShippingFee(List<PackageToShip> packages, string destProvince, string destDistrict);
}

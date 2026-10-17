namespace cellPhoneS_backend.DTOs.Responses;

public class VietnamProvinceDto
{
    public string? name { get; set; }
    public int code { get; set; }
    public string? codename { get; set; }
    public string? division_type { get; set; }
    public int phone_code { get; set; }
    public List<VietnamDistrictDto>? districts { get; set; }
}

public class VietnamDistrictDto
{
    public string? name { get; set; }
    public int code { get; set; }
    public string? codename { get; set; }
    public string? division_type { get; set; }
    public string? short_codename { get; set; }
    public List<VietnamWardDto>? wards { get; set; }
}

public class VietnamWardDto
{
    public string? name { get; set; }
    public int code { get; set; }
    public string? codename { get; set; }
    public string? division_type { get; set; }
    public string? short_codename { get; set; }
}

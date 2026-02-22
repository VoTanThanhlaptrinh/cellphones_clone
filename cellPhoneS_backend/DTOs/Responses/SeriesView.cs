namespace cellPhoneS_backend.DTOs.Responses;

public class SeriesView
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public long BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? Status { get; set; }

    public SeriesView(long id, string? name, long brandId, string? brandName, string? status)
    {
        Id = id;
        Name = name;
        BrandId = brandId;
        BrandName = brandName;
        Status = status;
    }
}

using System;

namespace cellPhoneS_backend.DTOs.Responses;

public class BrandView
{
    public long Id { get; set; }
    public string? Name { get; set; }

    public BrandView(long id,  string? name)
    {
        Id = id;
        Name = name;
    }
}

using System;

namespace cellPhoneS_backend.DTOs.Requests;
// request DTO for shipping fee calculation
public class ShippingRequest
{
    public string? pick_province { get; set; }
    public string? pick_district { get; set; }
    public string? pick_ward { get; set; }
    public string? pick_street { get; set; }
    public string? address { get; set; }
    public string? province { get; set; }
    public string? district { get; set; }
    public string? street { get; set; }
    public decimal? weight { get; set; }
    public decimal? value { get; set; }
    public string? transport { get; set; } = "road";
    // funcion check all fields not null
    public bool AllFieldsNotNull()
    {
        return pick_province != null &&
               pick_district != null &&
               pick_ward != null &&
               pick_street != null &&
               address != null &&
               province != null &&
               district != null &&
               street != null &&
               weight != null &&
               value != null &&
               transport != null;
    }
}

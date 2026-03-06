using System;
using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.Models;

public class Fee
{
    [Required]
    [Key]
    public long Id { get; set; }
    public ICollection<FeeDetail> FeeDetails { get; set; } = new List<FeeDetail>();
}

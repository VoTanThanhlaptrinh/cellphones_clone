using System;
using System.ComponentModel.DataAnnotations;

namespace cellPhoneS_backend.DTOs.Requests;

public class CartRequest
{
    public long productId {get;set;}
    public long colorId {get;set;}
}

using System;
using cellPhoneS_backend.Models;

namespace cellPhoneS_backend.Services.Interface;

public interface ProductSearchService
{
    public  Task<List<ProductSearchResult>> SearchAsync(string keyword);
    public  Task<List<ProductSearchResult>> InitializeAsync();
}

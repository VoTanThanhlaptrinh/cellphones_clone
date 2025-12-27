using System.Collections.ObjectModel;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Repositories;

public class StoreHouseRepository : BaseRepository<StoreHouse>, IStoreHouseRepository
{
    public StoreHouseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<StoreView>> GetStoreViews()
    {
        var shc = await _context.StoreHouses.Where(sh => "active".Equals(sh.Status)).Select(sh => new StoreHouseView(sh.Id, sh.City, sh.District, sh.Street)).ToListAsync();
        return shc.GroupBy(sh => sh.City)
            .Select(ctg => new StoreView(ctg.Key
                                            , ctg.GroupBy(d => d.District)
                                            .Select(dtg => new DistrictView(dtg.Key,
                                                                            dtg.Select(st => new StreetView(st.Id, st.Street)).ToList()
                                                                            )).ToList())).ToList();
    }
}

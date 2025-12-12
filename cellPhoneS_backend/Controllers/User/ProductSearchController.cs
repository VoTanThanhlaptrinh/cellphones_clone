using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Mvc;

namespace cellPhoneS_backend.Controllers.User
{
    [Route("api/")]
    [ApiController]
    public class ProductSearchController : ControllerBase
    {
        private readonly ElasticsearchClient _client;
        private readonly IProductRepository _productDBcontext;

        // Inject Client đã cấu hình ở Program.cs vào đây
        public ProductSearchController(ElasticsearchClient client, IProductRepository productDBcontext)
        {
            _client = client;
            _productDBcontext = productDBcontext;
        }

        // 1. API ĐỒNG BỘ DỮ LIỆU (Gọi api này sau khi Insert vào SQL thành công)
        [HttpPost("sync")]
        public async Task<IActionResult> AddProductToIndex([FromBody] Product product)
        {
            // Hàm này sẽ đẩy dữ liệu vào Index tên là "products"
            // Nếu Index chưa có, nó tự tạo.
            var response = await _client.IndexAsync(product, idx => idx.Index("products"));

            if (response.IsValidResponse)
            {
                return Ok("Đã đồng bộ sang Elasticsearch thành công!");
            }
            return BadRequest($"Lỗi: {response.DebugInformation}");
        }

        // 2. API TÌM KIẾM (INSTANT SEARCH)
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var response = await _client.SearchAsync<ProductIndexModel>(s => s
                .Index("products")
                .Size(5)
                .Query(q => q
                    .Bool(b => b // Dùng Bool để kết hợp nhiều chiến thuật
                        .Should(should => should
                            // CHIẾN THUẬT 1: Fuzzy (Vẫn giữ để bắt lỗi gõ sai)
                            .MultiMatch(m => m
                                .Query(keyword)
                                .Fields(new[] { "name^5" })
                                .Type(TextQueryType.BoolPrefix)
                            )
                        )
                        .Should(should => should
                            // CHIẾN THUẬT 2: Exact Match (QUAN TRỌNG NHẤT)
                            // Nếu khớp chính xác, KHÔNG fuzzy -> Cộng điểm gấp 10 lần
                            // Cái này sẽ đá đít mấy thằng "Leitz Phone" xuống dưới vì nó không có chữ "iphone"
                            .MultiMatch(m => m
                                .Query(keyword)
                                .Fields(new[] { "name^1" })
                                .Fuzziness(new Fuzziness(2))
                            // Mặc định boost = 1, ở đây ta set ^10
                            )
                        )
                        .Should(should => should
                            // 3. CHIẾN THUẬT WILDCARD (Vét cạn)
                            // Giúp tìm "iph*"
                            .Wildcard(w => w
                                .Field(p => p.Name)
                                .Value($"*{keyword}*")
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                return BadRequest(response.DebugInformation);
            }

            return Ok(response.Documents);
        }
        [HttpPost("migrate-data")]
        public async Task<IActionResult> MigrateDataFromSQL()
        {
            // BƯỚC 1: Lấy dữ liệu gốc từ SQL Server
            // Mẹo: Chỉ lấy những cột cần thiết để Search (Id, Name, Price, Description, Image)
            // Đừng lấy mấy cột rác như CreatedDate, CreatedBy nếu không cần search.
            var productsFromSql = _productDBcontext.GetAllAsync().Result;

            if (productsFromSql.Count() == 0) return BadRequest("SQL chưa có dữ liệu nào!");

            // BƯỚC 2: Map sang Object của Elasticsearch (Product class bạn đã tạo)
            var esProducts = productsFromSql.Select(p => new ProductIndexModel
            {
                Id = p.Id,
                Name = p.Name!,
                ImageUrl = p.ImageUrl ?? "", // Xử lý null để ES không lỗi
                BasePrice = p.BasePrice,
                SalePrice = p.SalePrice
            });

            // BƯỚC 3: Bulk Insert - Đẩy 1 lần tất cả sang Elasticsearch
            // IndexManyAsync là hàm tối ưu tự động chia lô để gửi đi cực nhanh
            var response = await _client.IndexManyAsync(esProducts, "products");

            if (response.IsValidResponse)
            {
                return Ok(new
                {
                    Message = $"Đã chuyển nhà thành công {esProducts.Count()} sản phẩm!",
                    Time = DateTime.Now
                });
            }
            else
            {
                // Nếu có lỗi, nó sẽ báo chi tiết lỗi gì
                return BadRequest($"Lỗi: {response.DebugInformation}");
            }
        }
    }
}
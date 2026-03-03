using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSearchView_RemoveSpecs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // BƯỚC 2: Tạo lại View MỚI - Siêu nhẹ, chỉ lấy từ bảng Products
            var createViewSql = @"
            CREATE MATERIALIZED VIEW ""mv_ProductSearch"" AS
            SELECT
                ""Id"",
                ""Name"",
                ""ImageUrl"",
                ""SalePrice"",
                ""BasePrice"",
                LOWER(unaccent(""Name"")) AS ""SearchVector""
            FROM ""Products"";
            ";

            migrationBuilder.Sql(createViewSql);

            // BƯỚC 3: Tạo Index GIN cho cột SearchVector 
            // (Phục vụ cho tương lai nếu bạn muốn dùng hàm LIKE hoặc pg_trgm thẳng dưới DB)
            migrationBuilder.Sql(@"
            CREATE INDEX idx_mv_product_search_vector 
            ON ""mv_ProductSearch"" 
            USING GIN (""SearchVector"" gin_trgm_ops);
            ");

            // BƯỚC 4: Tạo Index duy nhất cho ID để hỗ trợ Refresh Concurrently
            migrationBuilder.Sql(@"
            CREATE UNIQUE INDEX idx_mv_product_search_id 
            ON ""mv_ProductSearch"" (""Id"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa View khi rollback
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS ""mv_ProductSearch"";");
        }
    }
}

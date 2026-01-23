using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class product_search_view : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createViewSql = @"
            CREATE MATERIALIZED VIEW ""mv_ProductSearch"" AS
            SELECT
                p.""Id"",
                p.""Name"",
                p.""ImageUrl"",
                p.""SalePrice"",
                p.""BasePrice"",
                (
                    LOWER(unaccent(p.""Name"")) || ' ' || 
                    LOWER(unaccent(COALESCE(STRING_AGG(sd.""Value"", ' '), '')))
                ) AS ""SearchVector""
            FROM ""Products"" p
            -- Join vào các bảng chi tiết để lấy thông số kỹ thuật
            LEFT JOIN ""ProductSpecifications"" ps ON p.""Id"" = ps.""ProductId""
            LEFT JOIN ""SpecificationDetails"" sd ON ps.""SpecificationId"" = sd.""SpecificationId""
            
            -- Group by bắt buộc vì có dùng hàm aggregate STRING_AGG
            GROUP BY p.""Id"", p.""Name"", p.""ImageUrl"", p.""SalePrice"", p.""BasePrice"";
        ";
        
        migrationBuilder.Sql(createViewSql);

        // 3. Tạo Index GIN cho cột SearchVector (BẮT BUỘC để tìm nhanh)
        migrationBuilder.Sql(@"
            CREATE INDEX idx_mv_product_search_vector 
            ON ""mv_ProductSearch"" 
            USING GIN (""SearchVector"" gin_trgm_ops);
        ");
        
        // 4. Tạo Index duy nhất cho ID để hỗ trợ Refresh Concurrent (giúp refresh ko bị lock bảng)
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

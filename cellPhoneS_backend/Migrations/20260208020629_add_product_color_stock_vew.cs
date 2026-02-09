using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class add_product_color_stock_vew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW ""ProductColorStockView"" AS
                SELECT 
                    ""ProductId"", 
                    ""ColorId"", 
                    CAST(SUM(""Quantity"") AS integer) AS ""TotalQuantity""
                FROM ""Stores""
                GROUP BY ""ProductId"", ""ColorId"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS ""ProductColorStockView;""");
        }
    }
}

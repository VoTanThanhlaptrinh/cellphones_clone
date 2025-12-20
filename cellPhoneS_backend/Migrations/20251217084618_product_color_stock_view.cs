using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class product_color_stock_view : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE VIEW ProductColorStockView AS
            SELECT 
                ProductId,
                ColorId,
                SUM(Quantity) as TotalQuantity
            FROM Stores
            WHERE Status = 'active'
            GROUP BY ProductId, ColorId
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW ProductColorStockView");
        }
    }
}

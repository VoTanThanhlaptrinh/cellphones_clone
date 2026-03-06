using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class update_schema_order_detail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StoreHouseId",
                table: "OrderDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_StoreHouseId",
                table: "OrderDetails",
                column: "StoreHouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_StoreHouses_StoreHouseId",
                table: "OrderDetails",
                column: "StoreHouseId",
                principalTable: "StoreHouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_StoreHouses_StoreHouseId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_StoreHouseId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "StoreHouseId",
                table: "OrderDetails");
        }
    }
}

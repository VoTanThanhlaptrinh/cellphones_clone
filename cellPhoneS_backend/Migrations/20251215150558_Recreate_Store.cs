using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class Recreate_Store : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreHouseId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ColorId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValue: "active"),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    // Composite PK: StoreHouseId + ProductId + ColorId
                    table.PrimaryKey("PK_Stores", x => new { x.StoreHouseId, x.ProductId, x.ColorId });

                    table.ForeignKey(
                        name: "FK_Stores_StoreHouses_StoreHouseId",
                        column: x => x.StoreHouseId,
                        principalTable: "StoreHouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_Stores_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_Stores_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Stores_Users_CreateBy",       // đổi thành AspNetUsers nếu User map vào đó
                        column: x => x.CreateBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Stores_Users_UpdateBy",       // đổi thành AspNetUsers nếu cần
                        column: x => x.UpdateBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indexes (FK lookups)
            migrationBuilder.CreateIndex(
                name: "IX_Stores_ProductId",
                table: "Stores",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_ColorId",
                table: "Stores",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CreateBy",
                table: "Stores",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_UpdateBy",
                table: "Stores",
                column: "UpdateBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
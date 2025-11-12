using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class updateV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Stores",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreHouseId",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ColorImages",
                table: "ColorImages");

            migrationBuilder.DropIndex(
                name: "IX_ColorImages_ColorId",
                table: "ColorImages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductSpecifications");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ColorImages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BrandImages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stores",
                table: "Stores",
                columns: new[] { "StoreHouseId", "ImageId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ColorImages",
                table: "ColorImages",
                columns: new[] { "ColorId", "ImageId" });

            migrationBuilder.CreateTable(
                name: "Demand",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Demand_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Demand_AspNetUsers_UpdateBy",
                        column: x => x.UpdateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Demand_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandImage",
                columns: table => new
                {
                    DemandId = table.Column<long>(type: "bigint", nullable: false),
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandImage", x => new { x.DemandId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_DemandImage_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemandImage_AspNetUsers_UpdateBy",
                        column: x => x.UpdateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemandImage_Demand_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandImage_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Demand_CategoryId",
                table: "Demand",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Demand_CreateBy",
                table: "Demand",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_Demand_UpdateBy",
                table: "Demand",
                column: "UpdateBy");

            migrationBuilder.CreateIndex(
                name: "IX_DemandImage_CreateBy",
                table: "DemandImage",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_DemandImage_ImageId",
                table: "DemandImage",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandImage_UpdateBy",
                table: "DemandImage",
                column: "UpdateBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemandImage");

            migrationBuilder.DropTable(
                name: "Demand");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stores",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ColorImages",
                table: "ColorImages");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Stores",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ProductSpecifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "ColorImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "BrandImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stores",
                table: "Stores",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ColorImages",
                table: "ColorImages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreHouseId",
                table: "Stores",
                column: "StoreHouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorImages_ColorId",
                table: "ColorImages",
                column: "ColorId");
        }
    }
}

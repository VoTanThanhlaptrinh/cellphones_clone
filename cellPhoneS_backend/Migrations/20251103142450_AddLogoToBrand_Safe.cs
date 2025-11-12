using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoToBrand_Safe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Images_ImageId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "BrandImages");

            migrationBuilder.DropTable(
                name: "ColorImages");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "Stores",
                newName: "ColorId");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_ImageId",
                table: "Stores",
                newName: "IX_Stores_ColorId");

            migrationBuilder.AddColumn<long>(
                name: "ImageId",
                table: "Colors",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ImageId",
                table: "Brands",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colors_ImageId",
                table: "Colors",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_ImageId",
                table: "Brands",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Brands_Images_ImageId",
                table: "Brands",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Colors_Images_ImageId",
                table: "Colors",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Colors_ColorId",
                table: "Stores",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Brands_Images_ImageId",
                table: "Brands");

            migrationBuilder.DropForeignKey(
                name: "FK_Colors_Images_ImageId",
                table: "Colors");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Colors_ColorId",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Colors_ImageId",
                table: "Colors");

            migrationBuilder.DropIndex(
                name: "IX_Brands_ImageId",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Brands");

            migrationBuilder.RenameColumn(
                name: "ColorId",
                table: "Stores",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_Stores_ColorId",
                table: "Stores",
                newName: "IX_Stores_ImageId");

            migrationBuilder.CreateTable(
                name: "BrandImages",
                columns: table => new
                {
                    BrandId = table.Column<long>(type: "bigint", nullable: false),
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandImages", x => new { x.BrandId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_BrandImages_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandImages_AspNetUsers_UpdateBy",
                        column: x => x.UpdateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandImages_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrandImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColorImages",
                columns: table => new
                {
                    ColorId = table.Column<long>(type: "bigint", nullable: false),
                    ImageId = table.Column<long>(type: "bigint", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorImages", x => new { x.ColorId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_ColorImages_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ColorImages_AspNetUsers_UpdateBy",
                        column: x => x.UpdateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ColorImages_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColorImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandImages_CreateBy",
                table: "BrandImages",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_BrandImages_ImageId",
                table: "BrandImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandImages_UpdateBy",
                table: "BrandImages",
                column: "UpdateBy");

            migrationBuilder.CreateIndex(
                name: "IX_ColorImages_CreateBy",
                table: "ColorImages",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_ColorImages_ImageId",
                table: "ColorImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorImages_UpdateBy",
                table: "ColorImages",
                column: "UpdateBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Images_ImageId",
                table: "Stores",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

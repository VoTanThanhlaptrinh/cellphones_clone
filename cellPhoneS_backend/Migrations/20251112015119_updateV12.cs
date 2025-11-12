using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class updateV12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_AspNetUsers_CreateBy",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_AspNetUsers_UpdateBy",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Images_ImageId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.RenameTable(
                name: "ProductImage",
                newName: "ProductImages");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_UpdateBy",
                table: "ProductImages",
                newName: "IX_ProductImages_UpdateBy");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ImageId",
                table: "ProductImages",
                newName: "IX_ProductImages_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_CreateBy",
                table: "ProductImages",
                newName: "IX_ProductImages_CreateBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages",
                columns: new[] { "ProductId", "ImageId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_AspNetUsers_CreateBy",
                table: "ProductImages",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_AspNetUsers_UpdateBy",
                table: "ProductImages",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Images_ImageId",
                table: "ProductImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_AspNetUsers_CreateBy",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_AspNetUsers_UpdateBy",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Images_ImageId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages");

            migrationBuilder.RenameTable(
                name: "ProductImages",
                newName: "ProductImage");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_UpdateBy",
                table: "ProductImage",
                newName: "IX_ProductImage_UpdateBy");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ImageId",
                table: "ProductImage",
                newName: "IX_ProductImage_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_CreateBy",
                table: "ProductImage",
                newName: "IX_ProductImage_CreateBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                columns: new[] { "ProductId", "ImageId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_AspNetUsers_CreateBy",
                table: "ProductImage",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_AspNetUsers_UpdateBy",
                table: "ProductImage",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Images_ImageId",
                table: "ProductImage",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class updateV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demand_AspNetUsers_CreateBy",
                table: "Demand");

            migrationBuilder.DropForeignKey(
                name: "FK_Demand_AspNetUsers_UpdateBy",
                table: "Demand");

            migrationBuilder.DropForeignKey(
                name: "FK_Demand_Categories_CategoryId",
                table: "Demand");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImage_AspNetUsers_CreateBy",
                table: "DemandImage");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImage_AspNetUsers_UpdateBy",
                table: "DemandImage");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImage_Demand_DemandId",
                table: "DemandImage");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImage_Images_ImageId",
                table: "DemandImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemandImage",
                table: "DemandImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Demand",
                table: "Demand");

            migrationBuilder.RenameTable(
                name: "DemandImage",
                newName: "DemandImages");

            migrationBuilder.RenameTable(
                name: "Demand",
                newName: "Demands");

            migrationBuilder.RenameIndex(
                name: "IX_DemandImage_UpdateBy",
                table: "DemandImages",
                newName: "IX_DemandImages_UpdateBy");

            migrationBuilder.RenameIndex(
                name: "IX_DemandImage_ImageId",
                table: "DemandImages",
                newName: "IX_DemandImages_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandImage_CreateBy",
                table: "DemandImages",
                newName: "IX_DemandImages_CreateBy");

            migrationBuilder.RenameIndex(
                name: "IX_Demand_UpdateBy",
                table: "Demands",
                newName: "IX_Demands_UpdateBy");

            migrationBuilder.RenameIndex(
                name: "IX_Demand_CreateBy",
                table: "Demands",
                newName: "IX_Demands_CreateBy");

            migrationBuilder.RenameIndex(
                name: "IX_Demand_CategoryId",
                table: "Demands",
                newName: "IX_Demands_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemandImages",
                table: "DemandImages",
                columns: new[] { "DemandId", "ImageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Demands",
                table: "Demands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImages_AspNetUsers_CreateBy",
                table: "DemandImages",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImages_AspNetUsers_UpdateBy",
                table: "DemandImages",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImages_Demands_DemandId",
                table: "DemandImages",
                column: "DemandId",
                principalTable: "Demands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImages_Images_ImageId",
                table: "DemandImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_AspNetUsers_CreateBy",
                table: "Demands",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_AspNetUsers_UpdateBy",
                table: "Demands",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Categories_CategoryId",
                table: "Demands",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemandImages_AspNetUsers_CreateBy",
                table: "DemandImages");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImages_AspNetUsers_UpdateBy",
                table: "DemandImages");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImages_Demands_DemandId",
                table: "DemandImages");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandImages_Images_ImageId",
                table: "DemandImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_AspNetUsers_CreateBy",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_AspNetUsers_UpdateBy",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Categories_CategoryId",
                table: "Demands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Demands",
                table: "Demands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemandImages",
                table: "DemandImages");

            migrationBuilder.RenameTable(
                name: "Demands",
                newName: "Demand");

            migrationBuilder.RenameTable(
                name: "DemandImages",
                newName: "DemandImage");

            migrationBuilder.RenameIndex(
                name: "IX_Demands_UpdateBy",
                table: "Demand",
                newName: "IX_Demand_UpdateBy");

            migrationBuilder.RenameIndex(
                name: "IX_Demands_CreateBy",
                table: "Demand",
                newName: "IX_Demand_CreateBy");

            migrationBuilder.RenameIndex(
                name: "IX_Demands_CategoryId",
                table: "Demand",
                newName: "IX_Demand_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandImages_UpdateBy",
                table: "DemandImage",
                newName: "IX_DemandImage_UpdateBy");

            migrationBuilder.RenameIndex(
                name: "IX_DemandImages_ImageId",
                table: "DemandImage",
                newName: "IX_DemandImage_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandImages_CreateBy",
                table: "DemandImage",
                newName: "IX_DemandImage_CreateBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Demand",
                table: "Demand",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemandImage",
                table: "DemandImage",
                columns: new[] { "DemandId", "ImageId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Demand_AspNetUsers_CreateBy",
                table: "Demand",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demand_AspNetUsers_UpdateBy",
                table: "Demand",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demand_Categories_CategoryId",
                table: "Demand",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImage_AspNetUsers_CreateBy",
                table: "DemandImage",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImage_AspNetUsers_UpdateBy",
                table: "DemandImage",
                column: "UpdateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImage_Demand_DemandId",
                table: "DemandImage",
                column: "DemandId",
                principalTable: "Demand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandImage_Images_ImageId",
                table: "DemandImage",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

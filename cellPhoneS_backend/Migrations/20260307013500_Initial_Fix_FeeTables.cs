using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    public partial class Initial_Fix_FeeTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. TẠO BẢNG FEES (Vì chưa có trong DB)
            migrationBuilder.CreateTable(
                name: "Fees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fees", x => x.Id);
                });

            // 2. TẠO BẢNG FEEDETAILS
            migrationBuilder.CreateTable(
                name: "FeeDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    FeeId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeDetails_Fees_FeeId",
                        column: x => x.FeeId,
                        principalTable: "Fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 3. THÊM CỘT FEEID VÀO BẢNG ORDERS (Vì bảng Orders đã tồn tại nhưng chưa có cột này)
            migrationBuilder.AddColumn<long>(
                name: "FeeId",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // 4. TẠO INDEX VÀ FOREIGN KEY CHO BẢNG ORDERS
            migrationBuilder.CreateIndex(
                name: "IX_FeeDetails_FeeId",
                table: "FeeDetails",
                column: "FeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FeeId",
                table: "Orders",
                column: "FeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Fees_FeeId",
                table: "Orders",
                column: "FeeId",
                principalTable: "Fees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa các liên kết và bảng khi Rollback
            migrationBuilder.DropForeignKey(name: "FK_Orders_Fees_FeeId", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_FeeId", table: "Orders");
            migrationBuilder.DropColumn(name: "FeeId", table: "Orders");
            migrationBuilder.DropTable(name: "FeeDetails");
            migrationBuilder.DropTable(name: "Fees");
        }
    }
}
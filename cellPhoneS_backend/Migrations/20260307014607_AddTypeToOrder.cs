using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    public partial class AddTypeToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ thêm cột Type vào bảng Orders
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "delivery"); // Đặt default để tránh lỗi với dữ liệu cũ đang có
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa cột Type nếu Rollback
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Orders");
        }
    }
}
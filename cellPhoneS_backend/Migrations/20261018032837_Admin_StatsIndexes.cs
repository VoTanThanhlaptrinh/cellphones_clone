using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class Admin_StatsIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CreateDate",
                table: "PaymentTransactions",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreateDate",
                table: "Orders",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreateDate",
                table: "Orders",
                columns: new[] { "Status", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreateDate",
                table: "AspNetUsers",
                column: "CreateDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_CreateDate",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreateDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_CreateDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CreateDate",
                table: "AspNetUsers");
        }
    }
}

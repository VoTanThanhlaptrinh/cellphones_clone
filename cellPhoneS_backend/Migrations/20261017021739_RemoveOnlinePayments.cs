using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOnlinePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentAttempt",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Giữ số lần thử đã dùng để Idempotency-Key (cellphones-order-{orderId}-attempt-{n}) không bao giờ bị dùng lại.
            migrationBuilder.Sql(@"
                UPDATE ""Orders"" o
                SET ""PaymentAttempt"" = a.""MaxAttempt""
                FROM (SELECT ""OrderId"", MAX(""Attempt"") AS ""MaxAttempt"" FROM ""OnlinePayments"" GROUP BY ""OrderId"") a
                WHERE o.""Id"" = a.""OrderId"";");

            migrationBuilder.DropTable(
                name: "OnlinePayments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentAttempt",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "OnlinePayments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    CheckoutUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GatewayPaymentId = table.Column<long>(type: "bigint", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Provider = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlinePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlinePayments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePayments_IdempotencyKey",
                table: "OnlinePayments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePayments_OrderId",
                table: "OnlinePayments",
                column: "OrderId");
        }
    }
}

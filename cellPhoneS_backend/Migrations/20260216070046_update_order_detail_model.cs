using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class update_order_detail_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JwtRotations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderDetails");

            migrationBuilder.AddColumn<long>(
                name: "ColorId",
                table: "OrderDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ColorId",
                table: "OrderDetails",
                column: "ColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Colors_ColorId",
                table: "OrderDetails",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Colors_ColorId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ColorId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "OrderDetails");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "JwtRotations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ExprireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplaceByTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RevokeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JwtRotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JwtRotations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JwtRotations_ExprireAt",
                table: "JwtRotations",
                column: "ExprireAt");

            migrationBuilder.CreateIndex(
                name: "IX_JwtRotations_SessionId",
                table: "JwtRotations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_JwtRotations_TokenHash",
                table: "JwtRotations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JwtRotations_UserId",
                table: "JwtRotations",
                column: "UserId");
        }
    }
}

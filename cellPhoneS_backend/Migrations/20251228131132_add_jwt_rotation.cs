using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cellPhoneS_backend.Migrations
{
    /// <inheritdoc />
    public partial class add_jwt_rotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JwtRotations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExprireAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokeAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplaceByTokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
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
                unique: true,
                filter: "[TokenHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JwtRotations_UserId",
                table: "JwtRotations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JwtRotations");
        }
    }
}

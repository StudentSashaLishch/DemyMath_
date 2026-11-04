using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemyMath.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTopicProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProgressKey",
                table: "Topics",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateTable(
                name: "UserTopicProgress",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ProgressKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTopicProgress", x => new { x.UserId, x.ProgressKey });
                    table.ForeignKey(
                        name: "FK_UserTopicProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTopicProgress");

            migrationBuilder.DropColumn(
                name: "ProgressKey",
                table: "Topics");
        }
    }
}

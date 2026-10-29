using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemyMath.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Topics",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Topics");
        }
    }
}

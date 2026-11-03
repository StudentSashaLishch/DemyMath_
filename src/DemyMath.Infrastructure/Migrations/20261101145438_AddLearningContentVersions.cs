
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DemyMath.Infrastructure.Migrations
{
    public partial class AddLearningContentVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_LearningContentVersions",
                        x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "VersionId",
                table: "Topics",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionId",
                table: "LocalizedStrings",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "LearningContentVersions",
                columns: new[]
                {
                    "Status",
                    "CreatedAtUtc",
                    "PublishedAtUtc"
                },
                values: new object[]
                {
                    "Draft",
                    DateTime.UtcNow,
                    null
                });

            migrationBuilder.Sql(
                """
                UPDATE "Topics"
                SET "VersionId" = (
                    SELECT "Id"
                    FROM "LearningContentVersions"
                    WHERE "Status" = 'Draft'
                );

                UPDATE "LocalizedStrings"
                SET "VersionId" = (
                    SELECT "Id"
                    FROM "LearningContentVersions"
                    WHERE "Status" = 'Draft'
                );
                """);

            migrationBuilder.AlterColumn<int>(
                name: "VersionId",
                table: "Topics",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VersionId",
                table: "LocalizedStrings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalizedStrings",
                table: "LocalizedStrings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalizedStrings",
                table: "LocalizedStrings",
                columns: new[]
                {
                    "VersionId",
                    "StringId",
                    "Culture"
                });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_VersionId",
                table: "Topics",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningContentVersions_Status",
                table: "LearningContentVersions",
                column: "Status",
                unique: true,
                filter: "\"Status\" = 'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_LearningContentVersions_Status_Published",
                table: "LearningContentVersions",
                column: "Status",
                unique: true,
                filter: "\"Status\" = 'Published'");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalizedStrings_LearningContentVersions_VersionId",
                table: "LocalizedStrings",
                column: "VersionId",
                principalTable: "LearningContentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_LearningContentVersions_VersionId",
                table: "Topics",
                column: "VersionId",
                principalTable: "LearningContentVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalizedStrings_LearningContentVersions_VersionId",
                table: "LocalizedStrings");

            migrationBuilder.DropForeignKey(
                name: "FK_Topics_LearningContentVersions_VersionId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_VersionId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_LearningContentVersions_Status",
                table: "LearningContentVersions");

            migrationBuilder.DropIndex(
                name: "IX_LearningContentVersions_Status_Published",
                table: "LearningContentVersions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalizedStrings",
                table: "LocalizedStrings");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "LocalizedStrings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalizedStrings",
                table: "LocalizedStrings",
                columns: new[]
                {
                    "StringId",
                    "Culture"
                });

            migrationBuilder.DropTable(
                name: "LearningContentVersions");
        }
    }
}

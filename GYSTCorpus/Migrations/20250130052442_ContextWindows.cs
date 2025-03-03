using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class ContextWindows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TranscriptAnglicism",
                table: "TranscriptAnglicism");

            migrationBuilder.DropIndex(
                name: "IX_TranscriptAnglicism_VideoId_LangCode",
                table: "TranscriptAnglicism");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TranscriptAnglicism");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TranscriptAnglicism",
                table: "TranscriptAnglicism",
                columns: new[] { "VideoId", "LangCode", "Word", "TranscriptIndex" });

            migrationBuilder.CreateTable(
                name: "AnglicismContextWindows",
                columns: table => new
                {
                    Anglicism = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    ContextWord = table.Column<string>(type: "TEXT", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnglicismContextWindows", x => new { x.Anglicism, x.Year, x.Category, x.ContextWord });
                    table.ForeignKey(
                        name: "FK_AnglicismContextWindows_Anglicisms_Anglicism",
                        column: x => x.Anglicism,
                        principalTable: "Anglicisms",
                        principalColumn: "Word",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnglicismContextWindows_Anglicism",
                table: "AnglicismContextWindows",
                column: "Anglicism");

            migrationBuilder.CreateIndex(
                name: "IX_AnglicismContextWindows_Category",
                table: "AnglicismContextWindows",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AnglicismContextWindows_ContextWord",
                table: "AnglicismContextWindows",
                column: "ContextWord");

            migrationBuilder.CreateIndex(
                name: "IX_AnglicismContextWindows_Year",
                table: "AnglicismContextWindows",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnglicismContextWindows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TranscriptAnglicism",
                table: "TranscriptAnglicism");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TranscriptAnglicism",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TranscriptAnglicism",
                table: "TranscriptAnglicism",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAnglicism_VideoId_LangCode",
                table: "TranscriptAnglicism",
                columns: new[] { "VideoId", "LangCode" });
        }
    }
}

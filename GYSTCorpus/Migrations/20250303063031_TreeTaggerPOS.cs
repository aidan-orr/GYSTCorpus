using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class TreeTaggerPOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AnglicismContextWindows",
                table: "AnglicismContextWindows");

            migrationBuilder.AddColumn<int>(
                name: "GermanPos",
                table: "TranscriptAnglicism",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GermanPos",
                table: "AnglicismContextWindows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnglicismContextWindows",
                table: "AnglicismContextWindows",
                columns: new[] { "Anglicism", "Year", "Category", "ContextWord", "GermanPos" });

            migrationBuilder.CreateIndex(
                name: "IX_AnglicismContextWindows_GermanPos",
                table: "AnglicismContextWindows",
                column: "GermanPos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AnglicismContextWindows",
                table: "AnglicismContextWindows");

            migrationBuilder.DropIndex(
                name: "IX_AnglicismContextWindows_GermanPos",
                table: "AnglicismContextWindows");

            migrationBuilder.DropColumn(
                name: "GermanPos",
                table: "TranscriptAnglicism");

            migrationBuilder.DropColumn(
                name: "GermanPos",
                table: "AnglicismContextWindows");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnglicismContextWindows",
                table: "AnglicismContextWindows",
                columns: new[] { "Anglicism", "Year", "Category", "ContextWord" });
        }
    }
}

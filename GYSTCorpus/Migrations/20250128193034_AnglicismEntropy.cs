using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class AnglicismEntropy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entropy",
                table: "TranscriptAnglicism");

            migrationBuilder.AddColumn<float>(
                name: "Entropy",
                table: "Anglicisms",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entropy",
                table: "Anglicisms");

            migrationBuilder.AddColumn<float>(
                name: "Entropy",
                table: "TranscriptAnglicism",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }
    }
}

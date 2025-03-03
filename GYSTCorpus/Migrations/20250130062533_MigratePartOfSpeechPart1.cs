using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class MigratePartOfSpeechPart1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnglishPosTemp",
                table: "Anglicisms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GermanPosTemp",
                table: "Anglicisms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishPosTemp",
                table: "Anglicisms");

            migrationBuilder.DropColumn(
                name: "GermanPosTemp",
                table: "Anglicisms");
        }
    }
}

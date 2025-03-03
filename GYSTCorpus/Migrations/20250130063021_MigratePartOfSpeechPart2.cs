using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class MigratePartOfSpeechPart2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishPos",
                table: "Anglicisms");

            migrationBuilder.DropColumn(
                name: "GermanPos",
                table: "Anglicisms");

			migrationBuilder.RenameColumn(
                name: "EnglishPosTemp",
				table: "Anglicisms",
                newName: "EnglishPos");

			migrationBuilder.RenameColumn(
				name: "GermanPosTemp",
				table: "Anglicisms",
				newName: "GermanPos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GermanPos",
				table: "Anglicisms",
				newName: "GermanPosTemp");

			migrationBuilder.RenameColumn(
				name: "EnglishPos",
				table: "Anglicisms",
				newName: "EnglishPosTemp");
		}
    }
}

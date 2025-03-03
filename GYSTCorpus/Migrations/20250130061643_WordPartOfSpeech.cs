using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class WordPartOfSpeech : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WordPartOfSpeech",
                columns: table => new
                {
                    Word = table.Column<string>(type: "TEXT", nullable: false),
                    GermanPartOfSpeech = table.Column<int>(type: "INTEGER", nullable: false),
                    EnglishPartOfSpeech = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordPartOfSpeech", x => x.Word);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordPartOfSpeech");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class AddAnglicismsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anglicisms",
                columns: table => new
                {
                    Word = table.Column<string>(type: "TEXT", nullable: false),
                    BaseWord = table.Column<string>(type: "TEXT", nullable: false),
                    EnglishPos = table.Column<string>(type: "TEXT", nullable: false),
                    GermanPos = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anglicisms", x => x.Word);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transcripts_VideoId",
                table: "Transcripts",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anglicisms_BaseWord",
                table: "Anglicisms",
                column: "BaseWord");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anglicisms");

            migrationBuilder.DropIndex(
                name: "IX_Transcripts_VideoId",
                table: "Transcripts");
        }
    }
}

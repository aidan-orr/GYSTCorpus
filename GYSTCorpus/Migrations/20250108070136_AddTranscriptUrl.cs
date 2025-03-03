using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Transcripts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Transcripts");
        }
    }
}

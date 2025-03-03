using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYSTCorpus.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptAnglicisms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TranscriptAnglicism",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VideoId = table.Column<string>(type: "TEXT", nullable: false),
                    LangCode = table.Column<string>(type: "TEXT", nullable: false),
                    Word = table.Column<string>(type: "TEXT", nullable: false),
                    TranscriptIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscriptAnglicism", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranscriptAnglicism_Anglicisms_Word",
                        column: x => x.Word,
                        principalTable: "Anglicisms",
                        principalColumn: "Word",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TranscriptAnglicism_Transcripts_VideoId_LangCode",
                        columns: x => new { x.VideoId, x.LangCode },
                        principalTable: "Transcripts",
                        principalColumns: new[] { "VideoId", "LangCode" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAnglicism_VideoId",
                table: "TranscriptAnglicism",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAnglicism_VideoId_LangCode",
                table: "TranscriptAnglicism",
                columns: new[] { "VideoId", "LangCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptAnglicism_Word",
                table: "TranscriptAnglicism",
                column: "Word");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranscriptAnglicism");
        }
    }
}

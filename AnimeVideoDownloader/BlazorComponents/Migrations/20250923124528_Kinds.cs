using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorComponents.Migrations
{
    /// <inheritdoc />
    public partial class Kinds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubtitleLanguage",
                table: "EpisodeSources",
                newName: "SubtitlesLanguage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubtitlesLanguage",
                table: "EpisodeSources",
                newName: "SubtitleLanguage");
        }
    }
}

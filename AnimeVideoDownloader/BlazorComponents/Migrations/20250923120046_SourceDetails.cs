using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorComponents.Migrations
{
    /// <inheritdoc />
    public partial class SourceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PageUri",
                table: "Episodes",
                newName: "SourceUri");

            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "EpisodeSources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubtitleLanguage",
                table: "EpisodeSources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VoiceLanguage",
                table: "EpisodeSources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quality",
                table: "EpisodeSources");

            migrationBuilder.DropColumn(
                name: "SubtitleLanguage",
                table: "EpisodeSources");

            migrationBuilder.DropColumn(
                name: "VoiceLanguage",
                table: "EpisodeSources");

            migrationBuilder.RenameColumn(
                name: "SourceUri",
                table: "Episodes",
                newName: "PageUri");
        }
    }
}

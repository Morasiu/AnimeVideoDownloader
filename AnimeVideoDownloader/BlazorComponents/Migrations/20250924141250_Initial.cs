using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorComponents.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Animes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Directory = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodesUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Animes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceUri = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TotalBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    EpisodeType = table.Column<int>(type: "INTEGER", nullable: false),
                    SourcesUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AnimeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Animes_AnimeId",
                        column: x => x.AnimeId,
                        principalTable: "Animes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                    VoiceLanguage = table.Column<int>(type: "INTEGER", nullable: false),
                    SubtitlesLanguage = table.Column<int>(type: "INTEGER", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EpisodeSources_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EpisodeSourceId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueItems_EpisodeSources_EpisodeSourceId",
                        column: x => x.EpisodeSourceId,
                        principalTable: "EpisodeSources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QueueItems_Episodes_EpisodeId",
                        column: x => x.EpisodeId,
                        principalTable: "Episodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_AnimeId",
                table: "Episodes",
                column: "AnimeId");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeSources_EpisodeId",
                table: "EpisodeSources",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueItems_EpisodeId",
                table: "QueueItems",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueItems_EpisodeSourceId",
                table: "QueueItems",
                column: "EpisodeSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueItems");

            migrationBuilder.DropTable(
                name: "EpisodeSources");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Animes");
        }
    }
}

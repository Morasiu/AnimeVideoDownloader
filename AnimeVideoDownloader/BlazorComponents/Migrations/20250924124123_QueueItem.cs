using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorComponents.Migrations
{
    /// <inheritdoc />
    public partial class QueueItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EpisodeSourceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueItems_EpisodeSources_EpisodeSourceId",
                        column: x => x.EpisodeSourceId,
                        principalTable: "EpisodeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }
    }
}

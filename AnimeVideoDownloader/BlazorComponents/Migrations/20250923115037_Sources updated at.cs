using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorComponents.Migrations
{
    /// <inheritdoc />
    public partial class Sourcesupdatedat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SourcesUpdatedAt",
                table: "Episodes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourcesUpdatedAt",
                table: "Episodes");
        }
    }
}

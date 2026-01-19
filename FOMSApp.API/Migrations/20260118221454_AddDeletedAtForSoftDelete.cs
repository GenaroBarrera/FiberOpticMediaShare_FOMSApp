using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOMSApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Vaults",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Midpoints",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Cables",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Vaults");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Midpoints");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Cables");
        }
    }
}

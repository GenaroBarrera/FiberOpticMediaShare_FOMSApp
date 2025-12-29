using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOMSApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCableDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Cables",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Cables");
        }
    }
}

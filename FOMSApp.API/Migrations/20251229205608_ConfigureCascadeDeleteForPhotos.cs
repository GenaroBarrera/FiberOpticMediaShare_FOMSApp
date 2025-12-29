using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOMSApp.API.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeleteForPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Midpoints_MidpointId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Midpoints_MidpointId",
                table: "Photos",
                column: "MidpointId",
                principalTable: "Midpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos",
                column: "VaultId",
                principalTable: "Vaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Midpoints_MidpointId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Midpoints_MidpointId",
                table: "Photos",
                column: "MidpointId",
                principalTable: "Midpoints",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos",
                column: "VaultId",
                principalTable: "Vaults",
                principalColumn: "Id");
        }
    }
}

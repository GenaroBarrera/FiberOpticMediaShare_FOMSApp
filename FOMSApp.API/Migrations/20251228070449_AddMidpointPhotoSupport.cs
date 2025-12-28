using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FOMSApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMidpointPhotoSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos");

            migrationBuilder.AlterColumn<int>(
                name: "VaultId",
                table: "Photos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "MidpointId",
                table: "Photos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_MidpointId",
                table: "Photos",
                column: "MidpointId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Midpoints_MidpointId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_MidpointId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "MidpointId",
                table: "Photos");

            migrationBuilder.AlterColumn<int>(
                name: "VaultId",
                table: "Photos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Vaults_VaultId",
                table: "Photos",
                column: "VaultId",
                principalTable: "Vaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

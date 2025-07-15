using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AddGuestAndBhandarToEmpTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BHANDAR_ID",
                table: "EMP_TRAN",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GUEST_ID",
                table: "EMP_TRAN",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_BHANDAR_ID",
                table: "EMP_TRAN",
                column: "BHANDAR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_TRAN_GUEST_ID",
                table: "EMP_TRAN",
                column: "GUEST_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_TRAN_BHANDAR_BHANDAR_ID",
                table: "EMP_TRAN",
                column: "BHANDAR_ID",
                principalTable: "BHANDAR",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_TRAN_GUEST_GUEST_ID",
                table: "EMP_TRAN",
                column: "GUEST_ID",
                principalTable: "GUEST",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_TRAN_BHANDAR_BHANDAR_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropForeignKey(
                name: "FK_EMP_TRAN_GUEST_GUEST_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropIndex(
                name: "IX_EMP_TRAN_BHANDAR_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropIndex(
                name: "IX_EMP_TRAN_GUEST_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropColumn(
                name: "BHANDAR_ID",
                table: "EMP_TRAN");

            migrationBuilder.DropColumn(
                name: "GUEST_ID",
                table: "EMP_TRAN");
        }
    }
}

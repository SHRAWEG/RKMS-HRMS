using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class runPayroll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRun",
                table: "PAY_BILLS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RUN_BY_USER_ID",
                table: "PAY_BILLS",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PAY_BILLS_RUN_BY_USER_ID",
                table: "PAY_BILLS",
                column: "RUN_BY_USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PAY_BILLS_AspNetUsers_RUN_BY_USER_ID",
                table: "PAY_BILLS",
                column: "RUN_BY_USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAY_BILLS_AspNetUsers_RUN_BY_USER_ID",
                table: "PAY_BILLS");

            migrationBuilder.DropIndex(
                name: "IX_PAY_BILLS_RUN_BY_USER_ID",
                table: "PAY_BILLS");

            migrationBuilder.DropColumn(
                name: "IsRun",
                table: "PAY_BILLS");

            migrationBuilder.DropColumn(
                name: "RUN_BY_USER_ID",
                table: "PAY_BILLS");
        }
    }
}

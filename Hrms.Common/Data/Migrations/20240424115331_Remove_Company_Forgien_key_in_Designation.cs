using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class Remove_Company_Forgien_key_in_Designation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DESIGNATION_DEPARTMENT_DEPARTMENT_ID",
                table: "DESIGNATION");

            migrationBuilder.DropIndex(
                name: "IX_DESIGNATION_DEPARTMENT_ID",
                table: "DESIGNATION");

            migrationBuilder.DropColumn(
                name: "DEPARTMENT_ID",
                table: "DESIGNATION");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DEPARTMENT_ID",
                table: "DESIGNATION",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DESIGNATION_DEPARTMENT_ID",
                table: "DESIGNATION",
                column: "DEPARTMENT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DESIGNATION_DEPARTMENT_DEPARTMENT_ID",
                table: "DESIGNATION",
                column: "DEPARTMENT_ID",
                principalTable: "DEPARTMENT",
                principalColumn: "DEPT_ID");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class Remove_Company_Forgien_key_in_Company_Department_Designation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DEPARTMENT_COMPANY_CompanyId",
                table: "DEPARTMENT");

            migrationBuilder.DropIndex(
                name: "IX_DEPARTMENT_CompanyId",
                table: "DEPARTMENT");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "DEPARTMENT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "DEPARTMENT",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DEPARTMENT_CompanyId",
                table: "DEPARTMENT",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_DEPARTMENT_COMPANY_CompanyId",
                table: "DEPARTMENT",
                column: "CompanyId",
                principalTable: "COMPANY",
                principalColumn: "Company_Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class SalaryHeadCategory_024_07_221 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FLG_ASSIGN",
                table: "SALARY_HEAD_CATEGORY",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FLG_USE",
                table: "SALARY_HEAD_CATEGORY",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "S_NO",
                table: "SALARY_HEAD_CATEGORY",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FLG_ASSIGN",
                table: "SALARY_HEAD_CATEGORY");

            migrationBuilder.DropColumn(
                name: "FLG_USE",
                table: "SALARY_HEAD_CATEGORY");

            migrationBuilder.DropColumn(
                name: "S_NO",
                table: "SALARY_HEAD_CATEGORY");
        }
    }
}

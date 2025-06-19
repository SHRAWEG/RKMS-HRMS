using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AddedFieldShcTypeSalaryHeadCatAddedFieldDefValueSalaryHead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NAME",
                table: "SALARY_ANNEXURE_DETAIL");

            migrationBuilder.RenameColumn(
                name: "PER_UNIT_RATE",
                table: "SALARY_HEADS",
                newName: "DEF_VALUE");

            migrationBuilder.AddColumn<string>(
                name: "SHC_TYPE",
                table: "SALARY_HEAD_CATEGORY",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SHC_TYPE",
                table: "SALARY_HEAD_CATEGORY");

            migrationBuilder.RenameColumn(
                name: "DEF_VALUE",
                table: "SALARY_HEADS",
                newName: "PER_UNIT_RATE");

            migrationBuilder.AddColumn<string>(
                name: "NAME",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "");
        }
    }
}

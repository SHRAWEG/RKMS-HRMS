using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class hasContribution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HAS_OFFICE_CONTRIBUTION",
                table: "SALARY_ANNEXURE_HEADS",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HAS_OFFICE_CONTRIBUTION",
                table: "SALARY_ANNEXURE_HEADS");
        }
    }
}

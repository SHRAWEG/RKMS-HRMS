using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class SalaryAnnexureDetailUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UNIT_NAME",
                table: "SALARY_ANNEXURE_DETAIL");

            migrationBuilder.AlterColumn<decimal>(
                name: "PER_UNIT_RATE",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "PERCENT",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "AMOUNT",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<decimal>(
                name: "ANNUAL_PERCENT",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OFFICE_CONTRIBUTION",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ANNUAL_SALARY_ESTIMATE",
                table: "SALARY_ANNEXURE",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SALARY_ANNEXURE",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ANNUAL_PERCENT",
                table: "SALARY_ANNEXURE_DETAIL");

            migrationBuilder.DropColumn(
                name: "OFFICE_CONTRIBUTION",
                table: "SALARY_ANNEXURE_DETAIL");

            migrationBuilder.DropColumn(
                name: "ANNUAL_SALARY_ESTIMATE",
                table: "SALARY_ANNEXURE");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SALARY_ANNEXURE");

            migrationBuilder.AlterColumn<double>(
                name: "PER_UNIT_RATE",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PERCENT",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AMOUNT",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "UNIT_NAME",
                table: "SALARY_ANNEXURE_DETAIL",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}

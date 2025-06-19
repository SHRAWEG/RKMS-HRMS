using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class LeaveYearCompanies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_LEDGER_LEAVE_YEAR_LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "LEAVE_YEAR",
                newName: "LEAVE_YEAR_ID");

            migrationBuilder.AlterColumn<int>(
                name: "LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "COMPANY_ID",
                table: "LEAVE_LEDGER",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "COMPANY_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LEAVE_YEAR_COMPANIES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COMPANY_ID = table.Column<int>(type: "integer", nullable: true),
                    LEAVE_YEAR_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEAVE_YEAR_COMPANIES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LEAVE_YEAR_COMPANIES_COMPANY_COMPANY_ID",
                        column: x => x.COMPANY_ID,
                        principalTable: "COMPANY",
                        principalColumn: "Company_Id");
                    table.ForeignKey(
                        name: "FK_LEAVE_YEAR_COMPANIES_LEAVE_YEAR_LEAVE_YEAR_ID",
                        column: x => x.LEAVE_YEAR_ID,
                        principalTable: "LEAVE_YEAR",
                        principalColumn: "LEAVE_YEAR_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_LEDGER_COMPANY_ID",
                table: "LEAVE_LEDGER",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_COMPANY_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_YEAR_COMPANIES_COMPANY_ID",
                table: "LEAVE_YEAR_COMPANIES",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LEAVE_YEAR_COMPANIES_LEAVE_YEAR_ID",
                table: "LEAVE_YEAR_COMPANIES",
                column: "LEAVE_YEAR_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_COMPANY_COMPANY_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "COMPANY_ID",
                principalTable: "COMPANY",
                principalColumn: "Company_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_LEDGER_COMPANY_COMPANY_ID",
                table: "LEAVE_LEDGER",
                column: "COMPANY_ID",
                principalTable: "COMPANY",
                principalColumn: "Company_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_LEDGER_LEAVE_YEAR_LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER",
                column: "LEAVE_YEAR_ID",
                principalTable: "LEAVE_YEAR",
                principalColumn: "LEAVE_YEAR_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_COMPANY_COMPANY_ID",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_LEDGER_COMPANY_COMPANY_ID",
                table: "LEAVE_LEDGER");

            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_LEDGER_LEAVE_YEAR_LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER");

            migrationBuilder.DropTable(
                name: "LEAVE_YEAR_COMPANIES");

            migrationBuilder.DropIndex(
                name: "IX_LEAVE_LEDGER_COMPANY_ID",
                table: "LEAVE_LEDGER");

            migrationBuilder.DropIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_COMPANY_ID",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.DropColumn(
                name: "COMPANY_ID",
                table: "LEAVE_LEDGER");

            migrationBuilder.DropColumn(
                name: "COMPANY_ID",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.RenameColumn(
                name: "LEAVE_YEAR_ID",
                table: "LEAVE_YEAR",
                newName: "ID");

            migrationBuilder.AlterColumn<int>(
                name: "LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_LEDGER_LEAVE_YEAR_LEAVE_YEAR_ID",
                table: "LEAVE_LEDGER",
                column: "LEAVE_YEAR_ID",
                principalTable: "LEAVE_YEAR",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

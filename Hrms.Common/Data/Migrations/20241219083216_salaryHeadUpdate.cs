using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class salaryHeadUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CONTRIBUTION_TYPE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "CREATED_BY",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "DED_TAX_FREE_AMOUNT",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "DED_TAX_FREE_LIMIT_CHECK",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "DEF_VALUE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "DRCR",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "ESTIMATE_POST_MONTHS",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "IS_ACTIVE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "IS_LOCKED",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "IS_TAXABLE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "MAX_NOS",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "MIN_HOURS",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "OFFICE_CONTRIBUTION",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "REF_ID",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "SH_CALC_CATEGORY",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "SH_CALC_DATATYPE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "SH_CALC_MODE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "SH_CALC_TYPE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "TRN_CODE",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "UNIT_NAME",
                table: "SALARY_HEADS");

            migrationBuilder.AddColumn<int>(
                name: "CREATED_BY_USER_ID",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OFFICIAL_EMAIL",
                table: "EMP_TRAN",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SALARY_HEADS_OLD",
                columns: table => new
                {
                    SH_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TRN_CODE = table.Column<string>(type: "varchar(200)", nullable: false),
                    SH_NAME = table.Column<string>(type: "varchar(250)", nullable: false),
                    SHC_ID = table.Column<int>(type: "integer", nullable: false),
                    REF_ID = table.Column<int>(type: "integer", nullable: false),
                    DRCR = table.Column<char>(type: "char(2)", nullable: true),
                    SH_CALC_DATATYPE = table.Column<string>(type: "text", nullable: false),
                    SH_CALC_TYPE = table.Column<string>(type: "text", nullable: false),
                    SH_CALC_MODE = table.Column<string>(type: "text", nullable: false),
                    SH_CALC_CATEGORY = table.Column<int>(type: "integer", nullable: false),
                    IS_TAXABLE = table.Column<bool>(type: "boolean", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    MIN_HOURS = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MAX_NOS = table.Column<int>(type: "integer", nullable: false),
                    PER_UNIT_RATE = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DEF_VALUE = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UNIT_NAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    OFFICE_CONTRIBUTION = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CONTRIBUTION_TYPE = table.Column<int>(type: "integer", nullable: false),
                    DED_TAX_FREE_LIMIT_CHECK = table.Column<bool>(type: "boolean", nullable: false),
                    DED_TAX_FREE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ESTIMATE_POST_MONTHS = table.Column<int>(type: "integer", nullable: false),
                    IS_LOCKED = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_HEADS_OLD", x => x.SH_ID);
                    table.ForeignKey(
                        name: "FK_SALARY_HEADS_OLD_SALARY_HEAD_CATEGORY_SHC_ID",
                        column: x => x.SHC_ID,
                        principalTable: "SALARY_HEAD_CATEGORY",
                        principalColumn: "SHC_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_HEADS_CREATED_BY_USER_ID",
                table: "SALARY_HEADS",
                column: "CREATED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_HEADS_OLD_SHC_ID",
                table: "SALARY_HEADS_OLD",
                column: "SHC_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SALARY_HEADS_AspNetUsers_CREATED_BY_USER_ID",
                table: "SALARY_HEADS",
                column: "CREATED_BY_USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SALARY_HEADS_AspNetUsers_CREATED_BY_USER_ID",
                table: "SALARY_HEADS");

            migrationBuilder.DropTable(
                name: "SALARY_HEADS_OLD");

            migrationBuilder.DropIndex(
                name: "IX_SALARY_HEADS_CREATED_BY_USER_ID",
                table: "SALARY_HEADS");

            migrationBuilder.DropColumn(
                name: "CREATED_BY_USER_ID",
                table: "SALARY_HEADS");

            migrationBuilder.AddColumn<int>(
                name: "CONTRIBUTION_TYPE",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CREATED_BY",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DED_TAX_FREE_AMOUNT",
                table: "SALARY_HEADS",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "DED_TAX_FREE_LIMIT_CHECK",
                table: "SALARY_HEADS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DEF_VALUE",
                table: "SALARY_HEADS",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<char>(
                name: "DRCR",
                table: "SALARY_HEADS",
                type: "char(2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ESTIMATE_POST_MONTHS",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IS_ACTIVE",
                table: "SALARY_HEADS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_LOCKED",
                table: "SALARY_HEADS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_TAXABLE",
                table: "SALARY_HEADS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MAX_NOS",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MIN_HOURS",
                table: "SALARY_HEADS",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OFFICE_CONTRIBUTION",
                table: "SALARY_HEADS",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "REF_ID",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SH_CALC_CATEGORY",
                table: "SALARY_HEADS",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SH_CALC_DATATYPE",
                table: "SALARY_HEADS",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SH_CALC_MODE",
                table: "SALARY_HEADS",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SH_CALC_TYPE",
                table: "SALARY_HEADS",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TRN_CODE",
                table: "SALARY_HEADS",
                type: "varchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UNIT_NAME",
                table: "SALARY_HEADS",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OFFICIAL_EMAIL",
                table: "EMP_TRAN",
                type: "varchar(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);
        }
    }
}

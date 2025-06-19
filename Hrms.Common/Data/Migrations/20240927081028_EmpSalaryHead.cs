using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class EmpSalaryHeads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EMP_SALARY_RECORDS",
                columns: table => new
                {
                    EMP_SALARY_RECORD_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    MONTHLY_SALARY = table.Column<int>(type: "integer", nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_SALARY_RECORDS", x => x.EMP_SALARY_RECORD_ID);
                    table.ForeignKey(
                        name: "FK_EMP_SALARY_RECORDS_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_SALARY_HEADS",
                columns: table => new
                {
                    EMP_SH_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_SALARY_RECORD_ID = table.Column<int>(type: "integer", nullable: false),
                    SH_ID = table.Column<int>(type: "integer", nullable: false),
                    SH_CALC_DATATYPE = table.Column<string>(type: "text", nullable: false),
                    HAS_OFFICE_CONTRIBUTION = table.Column<bool>(type: "boolean", nullable: true),
                    OFFICE_CONTRIBUTION = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CONTRIBUTION_TYPE = table.Column<int>(type: "integer", nullable: true),
                    PER_UNIT_RATE = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_SALARY_HEADS", x => x.EMP_SH_ID);
                    table.ForeignKey(
                        name: "FK_EMP_SALARY_HEADS_EMP_SALARY_RECORDS_EMP_SALARY_RECORD_ID",
                        column: x => x.EMP_SALARY_RECORD_ID,
                        principalTable: "EMP_SALARY_RECORDS",
                        principalColumn: "EMP_SALARY_RECORD_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMP_SALARY_HEADS_SALARY_HEADS_SH_ID",
                        column: x => x.SH_ID,
                        principalTable: "SALARY_HEADS",
                        principalColumn: "SH_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMP_SALARY_HEAD_DETAIL",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_SH_ID = table.Column<int>(type: "integer", nullable: false),
                    REFERENCE_SH_ID = table.Column<int>(type: "integer", nullable: true),
                    IS_PERCENTAGE_OF_MONTHLY_SALARY = table.Column<bool>(type: "boolean", nullable: false),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PERCENT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_SALARY_HEAD_DETAIL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_SALARY_HEAD_DETAIL_EMP_SALARY_HEADS_EMP_SH_ID",
                        column: x => x.EMP_SH_ID,
                        principalTable: "EMP_SALARY_HEADS",
                        principalColumn: "EMP_SH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EMP_SALARY_HEAD_DETAIL_EMP_SALARY_HEADS_REFERENCE_SH_ID",
                        column: x => x.REFERENCE_SH_ID,
                        principalTable: "EMP_SALARY_HEADS",
                        principalColumn: "EMP_SH_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMP_SALARY_HEAD_DETAIL_EMP_SH_ID",
                table: "EMP_SALARY_HEAD_DETAIL",
                column: "EMP_SH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_SALARY_HEAD_DETAIL_REFERENCE_SH_ID",
                table: "EMP_SALARY_HEAD_DETAIL",
                column: "REFERENCE_SH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_SALARY_HEADS_EMP_SALARY_RECORD_ID",
                table: "EMP_SALARY_HEADS",
                column: "EMP_SALARY_RECORD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_SALARY_HEADS_SH_ID",
                table: "EMP_SALARY_HEADS",
                column: "SH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_SALARY_RECORDS_EMP_ID",
                table: "EMP_SALARY_RECORDS",
                column: "EMP_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMP_SALARY_HEAD_DETAIL");

            migrationBuilder.DropTable(
                name: "EMP_SALARY_HEADS");

            migrationBuilder.DropTable(
                name: "EMP_SALARY_RECORDS");
        }
    }
}

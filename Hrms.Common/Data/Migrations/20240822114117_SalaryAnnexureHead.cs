using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class SalaryAnnexureHead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SALARY_ANNEXURE_DETAIL");

            migrationBuilder.CreateTable(
                name: "SALARY_ANNEXURE_HEADS",
                columns: table => new
                {
                    SALARY_ANNEXURE_HEAD_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ANX_ID = table.Column<int>(type: "integer", nullable: false),
                    SH_ID = table.Column<int>(type: "integer", nullable: false),
                    SH_CALC_DATATYPE = table.Column<string>(type: "text", nullable: false),
                    ANNUAL_PERCENT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OFFICE_CONTRIBUTION = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CONTRIBUTION_TYPE = table.Column<int>(type: "integer", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_ANNEXURE_HEADS", x => x.SALARY_ANNEXURE_HEAD_ID);
                    table.ForeignKey(
                        name: "FK_SALARY_ANNEXURE_HEADS_SALARY_ANNEXURE_ANX_ID",
                        column: x => x.ANX_ID,
                        principalTable: "SALARY_ANNEXURE",
                        principalColumn: "ANX_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SALARY_ANNEXURE_HEADS_SALARY_HEADS_SH_ID",
                        column: x => x.SH_ID,
                        principalTable: "SALARY_HEADS",
                        principalColumn: "SH_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SALARY_ANNEXURE_HEAD_DETAILS",
                columns: table => new
                {
                    ANX_DETAIL_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SALARY_ANNEXURE_HEAD_ID = table.Column<int>(type: "integer", nullable: false),
                    REFERENCE_SALARY_ANNEXURE_HEAD_ID = table.Column<int>(type: "integer", nullable: true),
                    IS_PERCENTAGE_OF_MONTHLY_SALARY = table.Column<bool>(type: "boolean", nullable: false),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PERCENT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PER_UNIT_RATE = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_ANNEXURE_HEAD_DETAILS", x => x.ANX_DETAIL_ID);
                    table.ForeignKey(
                        name: "FK_SALARY_ANNEXURE_HEAD_DETAILS_SALARY_ANNEXURE_HEADS_REFERENC~",
                        column: x => x.REFERENCE_SALARY_ANNEXURE_HEAD_ID,
                        principalTable: "SALARY_ANNEXURE_HEADS",
                        principalColumn: "SALARY_ANNEXURE_HEAD_ID");
                    table.ForeignKey(
                        name: "FK_SALARY_ANNEXURE_HEAD_DETAILS_SALARY_ANNEXURE_HEADS_SALARY_A~",
                        column: x => x.SALARY_ANNEXURE_HEAD_ID,
                        principalTable: "SALARY_ANNEXURE_HEADS",
                        principalColumn: "SALARY_ANNEXURE_HEAD_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_ANNEXURE_HEAD_DETAILS_REFERENCE_SALARY_ANNEXURE_HEAD~",
                table: "SALARY_ANNEXURE_HEAD_DETAILS",
                column: "REFERENCE_SALARY_ANNEXURE_HEAD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_ANNEXURE_HEAD_DETAILS_SALARY_ANNEXURE_HEAD_ID",
                table: "SALARY_ANNEXURE_HEAD_DETAILS",
                column: "SALARY_ANNEXURE_HEAD_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_ANNEXURE_HEADS_ANX_ID",
                table: "SALARY_ANNEXURE_HEADS",
                column: "ANX_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_ANNEXURE_HEADS_SH_ID",
                table: "SALARY_ANNEXURE_HEADS",
                column: "SH_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SALARY_ANNEXURE_HEAD_DETAILS");

            migrationBuilder.DropTable(
                name: "SALARY_ANNEXURE_HEADS");

            migrationBuilder.CreateTable(
                name: "SALARY_ANNEXURE_DETAIL",
                columns: table => new
                {
                    ANX_DETAIL_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ANX_ID = table.Column<int>(type: "integer", nullable: false),
                    SH_ID = table.Column<int>(type: "integer", nullable: false),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ANNUAL_PERCENT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OFFICE_CONTRIBUTION = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PER_UNIT_RATE = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PERCENT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_ANNEXURE_DETAIL", x => x.ANX_DETAIL_ID);
                    table.ForeignKey(
                        name: "FK_SALARY_ANNEXURE_DETAIL_SALARY_ANNEXURE_ANX_ID",
                        column: x => x.ANX_ID,
                        principalTable: "SALARY_ANNEXURE",
                        principalColumn: "ANX_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SALARY_ANNEXURE_DETAIL_SALARY_HEADS_SH_ID",
                        column: x => x.SH_ID,
                        principalTable: "SALARY_HEADS",
                        principalColumn: "SH_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_ANNEXURE_DETAIL_ANX_ID",
                table: "SALARY_ANNEXURE_DETAIL",
                column: "ANX_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_ANNEXURE_DETAIL_SH_ID",
                table: "SALARY_ANNEXURE_DETAIL",
                column: "SH_ID");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class payBillGeneration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PAY_BILLS",
                columns: table => new
                {
                    PAY_BILL_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    FROM_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    TO_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_BY_USER_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAY_BILLS", x => x.PAY_BILL_ID);
                    table.ForeignKey(
                        name: "FK_PAY_BILLS_AspNetUsers_CREATED_BY_USER_ID",
                        column: x => x.CREATED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PAY_BILLS_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAY_BILL_SALARY_HEADS",
                columns: table => new
                {
                    PAY_BILL_SH_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PAY_BILL_ID = table.Column<int>(type: "integer", nullable: false),
                    EMP_SH_ID = table.Column<int>(type: "integer", nullable: false),
                    OFFICE_CONTRIBUTION_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TOTAL_UNIT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAY_BILL_SALARY_HEADS", x => x.PAY_BILL_SH_ID);
                    table.ForeignKey(
                        name: "FK_PAY_BILL_SALARY_HEADS_EMP_SALARY_HEADS_EMP_SH_ID",
                        column: x => x.EMP_SH_ID,
                        principalTable: "EMP_SALARY_HEADS",
                        principalColumn: "EMP_SH_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PAY_BILL_SALARY_HEADS_PAY_BILLS_PAY_BILL_ID",
                        column: x => x.PAY_BILL_ID,
                        principalTable: "PAY_BILLS",
                        principalColumn: "PAY_BILL_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAY_BILL_SALARY_HEADS_EMP_SH_ID",
                table: "PAY_BILL_SALARY_HEADS",
                column: "EMP_SH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PAY_BILL_SALARY_HEADS_PAY_BILL_ID",
                table: "PAY_BILL_SALARY_HEADS",
                column: "PAY_BILL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PAY_BILLS_CREATED_BY_USER_ID",
                table: "PAY_BILLS",
                column: "CREATED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PAY_BILLS_EMP_ID",
                table: "PAY_BILLS",
                column: "EMP_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAY_BILL_SALARY_HEADS");

            migrationBuilder.DropTable(
                name: "PAY_BILLS");
        }
    }
}

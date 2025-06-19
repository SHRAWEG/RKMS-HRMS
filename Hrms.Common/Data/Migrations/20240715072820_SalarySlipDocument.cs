using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class SalarySlipDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SALARY_SLIP_DOCUMENTS",
                columns: table => new
                {
                    SALARY_SLIP_DOCUMENT_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FILENAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    USER_ID = table.Column<int>(type: "integer", nullable: false),
                    FILE_PATH = table.Column<string>(type: "varchar(255)", nullable: false),
                    EMP_ID = table.Column<int>(type: "integer", nullable: false),
                    YEAR = table.Column<int>(type: "integer", nullable: false),
                    MONTH = table.Column<int>(type: "integer", nullable: false),
                    UPLOADED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALARY_SLIP_DOCUMENTS", x => x.SALARY_SLIP_DOCUMENT_ID);
                    table.ForeignKey(
                        name: "FK_SALARY_SLIP_DOCUMENTS_AspNetUsers_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SALARY_SLIP_DOCUMENTS_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_SLIP_DOCUMENTS_EMP_ID",
                table: "SALARY_SLIP_DOCUMENTS",
                column: "EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SALARY_SLIP_DOCUMENTS_USER_ID",
                table: "SALARY_SLIP_DOCUMENTS",
                column: "USER_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        { }
    }
}

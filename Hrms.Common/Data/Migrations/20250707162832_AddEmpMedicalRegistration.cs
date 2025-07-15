using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AddEmpMedicalRegistration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EMP_MEDICAL_REGISTRATION",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    REGISTRATION_NUMBER = table.Column<string>(type: "varchar(255)", nullable: false),
                    START_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    END_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    DOCUMENT_ID = table.Column<int>(type: "integer", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMP_MEDICAL_REGISTRATION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EMP_MEDICAL_REGISTRATION_EMP_DETAIL_EMP_ID",
                        column: x => x.EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_EMP_MEDICAL_REGISTRATION_EMP_DOCLIST_DOCUMENT_ID",
                        column: x => x.DOCUMENT_ID,
                        principalTable: "EMP_DOCLIST",
                        principalColumn: "DOC_NO");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMP_MEDICAL_REGISTRATION_DOCUMENT_ID",
                table: "EMP_MEDICAL_REGISTRATION",
                column: "DOCUMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_MEDICAL_REGISTRATION_EMP_ID",
                table: "EMP_MEDICAL_REGISTRATION",
                column: "EMP_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMP_MEDICAL_REGISTRATION");
        }
    }
}

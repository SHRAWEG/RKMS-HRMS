using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class ManpowerRequsition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MANPOWER_REQUISITIONS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JOB_TITLE = table.Column<string>(type: "varchar(255)", nullable: false),
                    STARTING_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    DEPARTMENT_ID = table.Column<int>(type: "integer", nullable: true),
                    DESIGNATION_ID = table.Column<short>(type: "smallint", nullable: true),
                    BRANCH_ID = table.Column<short>(type: "smallint", nullable: true),
                    REPORTING_TO_EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    QUANTITY = table.Column<int>(type: "integer", nullable: false),
                    IS_GENDER_SPECIFIC = table.Column<bool>(type: "boolean", nullable: false),
                    GENDER = table.Column<string>(type: "varchar(10)", nullable: true),
                    EMPLOYMENT_NATURE = table.Column<string>(type: "varchar(255)", nullable: false),
                    REPLACED_EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    EMPLOYMENT_TYPE_ID = table.Column<int>(type: "integer", nullable: true),
                    QUALIFICATIONS = table.Column<string>(type: "varchar(255)", nullable: true),
                    EXPERIENCE = table.Column<string>(type: "varchar(255)", nullable: true),
                    KEY_COMPETENCIES = table.Column<string>(type: "varchar(255)", nullable: true),
                    JOB_DESCRIPTION = table.Column<string>(type: "text", nullable: true),
                    SALARY_RANGE_FROM = table.Column<double>(type: "double precision", nullable: true),
                    SALARY_RANGE_TO = table.Column<double>(type: "double precision", nullable: true),
                    STATUS = table.Column<string>(type: "text", nullable: false),
                    REQUESTED_BY_USER_ID = table.Column<int>(type: "integer", nullable: false),
                    ProcessedById = table.Column<int>(type: "integer", nullable: true),
                    PROCESSED_BY_USER_ID = table.Column<int>(type: "integer", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MANPOWER_REQUISITIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_REQUESTED_BY_USER_ID",
                        column: x => x.REQUESTED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_BRANCH_BRANCH_ID",
                        column: x => x.BRANCH_ID,
                        principalTable: "BRANCH",
                        principalColumn: "BRANCH_ID");
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_DEPARTMENT_DEPARTMENT_ID",
                        column: x => x.DEPARTMENT_ID,
                        principalTable: "DEPARTMENT",
                        principalColumn: "DEPT_ID");
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_DESIGNATION_DESIGNATION_ID",
                        column: x => x.DESIGNATION_ID,
                        principalTable: "DESIGNATION",
                        principalColumn: "DEG_ID");
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REPLACED_EMP_ID",
                        column: x => x.REPLACED_EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REPORTING_TO_EMP_ID",
                        column: x => x.REPORTING_TO_EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_MANPOWER_REQUISITIONS_EMPLOYMENT_TYPES_EMPLOYMENT_TYPE_ID",
                        column: x => x.EMPLOYMENT_TYPE_ID,
                        principalTable: "EMPLOYMENT_TYPES",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_BRANCH_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "BRANCH_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_DEPARTMENT_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "DEPARTMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_DESIGNATION_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "DESIGNATION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_EMPLOYMENT_TYPE_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "EMPLOYMENT_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_ProcessedById",
                table: "MANPOWER_REQUISITIONS",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_REPLACED_EMP_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REPLACED_EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_REPORTING_TO_EMP_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REPORTING_TO_EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_REQUESTED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REQUESTED_BY_USER_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MANPOWER_REQUISITIONS");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class jobAndDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_DOCLIST_EMP_DETAIL_EMP_ID",
                table: "EMP_DOCLIST");

            migrationBuilder.DropIndex(
                name: "IX_EMP_DOCLIST_EMP_ID",
                table: "EMP_DOCLIST");

            migrationBuilder.DropColumn(
                name: "EMP_ID",
                table: "EMP_DOCLIST");

            migrationBuilder.AddColumn<int>(
                name: "AADHAR_DOC_ID",
                table: "EMP_DETAIL",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DRIVING_LICENSE_DOC_ID",
                table: "EMP_DETAIL",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PAN_DOC_ID",
                table: "EMP_DETAIL",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PASSPORT_DOC_ID",
                table: "EMP_DETAIL",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EMPLOYMENT_TYPES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYMENT_TYPES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HIRING_STAGES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    STEP = table.Column<int>(type: "integer", nullable: false),
                    IS_FIXED = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HIRING_STAGES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SKILLS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKILLS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SOURCES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOURCES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "JOBS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TITLE = table.Column<string>(type: "varchar(255)", nullable: false),
                    EMPLOYMENT_TYPE_ID = table.Column<int>(type: "integer", nullable: true),
                    COMPANY_ID = table.Column<int>(type: "integer", nullable: true),
                    DEPARTMENT_ID = table.Column<int>(type: "integer", nullable: true),
                    QUANTITY = table.Column<int>(type: "integer", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: false),
                    STATUS = table.Column<string>(type: "text", nullable: false),
                    ESTIMATED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JOBS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_JOBS_COMPANY_COMPANY_ID",
                        column: x => x.COMPANY_ID,
                        principalTable: "COMPANY",
                        principalColumn: "Company_Id");
                    table.ForeignKey(
                        name: "FK_JOBS_DEPARTMENT_DEPARTMENT_ID",
                        column: x => x.DEPARTMENT_ID,
                        principalTable: "DEPARTMENT",
                        principalColumn: "DEPT_ID");
                    table.ForeignKey(
                        name: "FK_JOBS_EMPLOYMENT_TYPES_EMPLOYMENT_TYPE_ID",
                        column: x => x.EMPLOYMENT_TYPE_ID,
                        principalTable: "EMPLOYMENT_TYPES",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "CANDIDATES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FIRST_NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    MIDDLE_NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    LAST_NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    EMAIL = table.Column<string>(type: "varchar(255)", nullable: false),
                    PHONE = table.Column<string>(type: "varchar(20)", nullable: false),
                    JOB_ID = table.Column<int>(type: "integer", nullable: true),
                    COVER_LETTER = table.Column<string>(type: "text", nullable: false),
                    OVERALL_RATING = table.Column<int>(type: "integer", nullable: true),
                    REMARKS = table.Column<string>(type: "text", nullable: false),
                    HIRING_STAGE_ID = table.Column<int>(type: "integer", nullable: true),
                    CREATED_BY_USER_ID = table.Column<int>(type: "integer", nullable: true),
                    EVALUATED_BY_USER_ID = table.Column<int>(type: "integer", nullable: true),
                    HIRED_BY_USER_ID = table.Column<int>(type: "integer", nullable: true),
                    CV_PATH = table.Column<string>(type: "varchar(255)", nullable: false),
                    IMAGE_PATH = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANDIDATES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CANDIDATES_AspNetUsers_CREATED_BY_USER_ID",
                        column: x => x.CREATED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CANDIDATES_AspNetUsers_EVALUATED_BY_USER_ID",
                        column: x => x.EVALUATED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CANDIDATES_AspNetUsers_HIRED_BY_USER_ID",
                        column: x => x.HIRED_BY_USER_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CANDIDATES_HIRING_STAGES_HIRING_STAGE_ID",
                        column: x => x.HIRING_STAGE_ID,
                        principalTable: "HIRING_STAGES",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_CANDIDATES_JOBS_JOB_ID",
                        column: x => x.JOB_ID,
                        principalTable: "JOBS",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "CANDIDATE_ACTIVITY_LOG",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CANDIDATE_ID = table.Column<int>(type: "integer", nullable: false),
                    Activity = table.Column<string>(type: "varchar(255)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANDIDATE_ACTIVITY_LOG", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_ACTIVITY_LOG_CANDIDATES_CANDIDATE_ID",
                        column: x => x.CANDIDATE_ID,
                        principalTable: "CANDIDATES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CANDIDATE_SKILLS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CANDIDATE_ID = table.Column<int>(type: "integer", nullable: false),
                    SOURCE_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANDIDATE_SKILLS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_SKILLS_CANDIDATES_CANDIDATE_ID",
                        column: x => x.CANDIDATE_ID,
                        principalTable: "CANDIDATES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_SKILLS_SKILLS_SOURCE_ID",
                        column: x => x.SOURCE_ID,
                        principalTable: "SKILLS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CANDIDATE_SOURCES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CANDIDATE_ID = table.Column<int>(type: "integer", nullable: false),
                    SOURCE_ID = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANDIDATE_SOURCES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_SOURCES_CANDIDATES_CANDIDATE_ID",
                        column: x => x.CANDIDATE_ID,
                        principalTable: "CANDIDATES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_SOURCES_SOURCES_SOURCE_ID",
                        column: x => x.SOURCE_ID,
                        principalTable: "SOURCES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_AADHAR_DOC_ID",
                table: "EMP_DETAIL",
                column: "AADHAR_DOC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_DRIVING_LICENSE_DOC_ID",
                table: "EMP_DETAIL",
                column: "DRIVING_LICENSE_DOC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_PAN_DOC_ID",
                table: "EMP_DETAIL",
                column: "PAN_DOC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_PASSPORT_DOC_ID",
                table: "EMP_DETAIL",
                column: "PASSPORT_DOC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_ACTIVITY_LOG_CANDIDATE_ID",
                table: "CANDIDATE_ACTIVITY_LOG",
                column: "CANDIDATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_SKILLS_CANDIDATE_ID",
                table: "CANDIDATE_SKILLS",
                column: "CANDIDATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_SKILLS_SOURCE_ID",
                table: "CANDIDATE_SKILLS",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_SOURCES_CANDIDATE_ID",
                table: "CANDIDATE_SOURCES",
                column: "CANDIDATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_SOURCES_SOURCE_ID",
                table: "CANDIDATE_SOURCES",
                column: "SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATES_CREATED_BY_USER_ID",
                table: "CANDIDATES",
                column: "CREATED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATES_EVALUATED_BY_USER_ID",
                table: "CANDIDATES",
                column: "EVALUATED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATES_HIRED_BY_USER_ID",
                table: "CANDIDATES",
                column: "HIRED_BY_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATES_HIRING_STAGE_ID",
                table: "CANDIDATES",
                column: "HIRING_STAGE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATES_JOB_ID",
                table: "CANDIDATES",
                column: "JOB_ID");

            migrationBuilder.CreateIndex(
                name: "IX_JOBS_COMPANY_ID",
                table: "JOBS",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_JOBS_DEPARTMENT_ID",
                table: "JOBS",
                column: "DEPARTMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_JOBS_EMPLOYMENT_TYPE_ID",
                table: "JOBS",
                column: "EMPLOYMENT_TYPE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_AADHAR_DOC_ID",
                table: "EMP_DETAIL",
                column: "AADHAR_DOC_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_DRIVING_LICENSE_DOC_ID",
                table: "EMP_DETAIL",
                column: "DRIVING_LICENSE_DOC_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_PAN_DOC_ID",
                table: "EMP_DETAIL",
                column: "PAN_DOC_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_PASSPORT_DOC_ID",
                table: "EMP_DETAIL",
                column: "PASSPORT_DOC_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_AADHAR_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_DRIVING_LICENSE_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_PAN_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_PASSPORT_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropTable(
                name: "CANDIDATE_ACTIVITY_LOG");

            migrationBuilder.DropTable(
                name: "CANDIDATE_SKILLS");

            migrationBuilder.DropTable(
                name: "CANDIDATE_SOURCES");

            migrationBuilder.DropTable(
                name: "SKILLS");

            migrationBuilder.DropTable(
                name: "CANDIDATES");

            migrationBuilder.DropTable(
                name: "SOURCES");

            migrationBuilder.DropTable(
                name: "HIRING_STAGES");

            migrationBuilder.DropTable(
                name: "JOBS");

            migrationBuilder.DropTable(
                name: "EMPLOYMENT_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_EMP_DETAIL_AADHAR_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropIndex(
                name: "IX_EMP_DETAIL_DRIVING_LICENSE_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropIndex(
                name: "IX_EMP_DETAIL_PAN_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropIndex(
                name: "IX_EMP_DETAIL_PASSPORT_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropColumn(
                name: "AADHAR_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropColumn(
                name: "DRIVING_LICENSE_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropColumn(
                name: "PAN_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropColumn(
                name: "PASSPORT_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.AddColumn<int>(
                name: "EMP_ID",
                table: "EMP_DOCLIST",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DOCLIST_EMP_ID",
                table: "EMP_DOCLIST",
                column: "EMP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_DOCLIST_EMP_DETAIL_EMP_ID",
                table: "EMP_DOCLIST",
                column: "EMP_ID",
                principalTable: "EMP_DETAIL",
                principalColumn: "EMP_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

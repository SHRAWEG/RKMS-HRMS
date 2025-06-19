using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class Recruitment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JOBS_COMPANY_COMPANY_ID",
                table: "JOBS");

            migrationBuilder.DropTable(
                name: "CANDIDATE_SKILLS");

            migrationBuilder.DropTable(
                name: "SKILLS");

            migrationBuilder.DropIndex(
                name: "IX_JOBS_COMPANY_ID",
                table: "JOBS");

            migrationBuilder.DropColumn(
                name: "COMPANY_ID",
                table: "JOBS");

            migrationBuilder.AddColumn<short>(
                name: "BRANCH_ID",
                table: "JOBS",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "SKILLS",
                table: "CANDIDATES",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_JOBS_BRANCH_ID",
                table: "JOBS",
                column: "BRANCH_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_JOBS_BRANCH_BRANCH_ID",
                table: "JOBS",
                column: "BRANCH_ID",
                principalTable: "BRANCH",
                principalColumn: "BRANCH_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JOBS_BRANCH_BRANCH_ID",
                table: "JOBS");

            migrationBuilder.DropIndex(
                name: "IX_JOBS_BRANCH_ID",
                table: "JOBS");

            migrationBuilder.DropColumn(
                name: "BRANCH_ID",
                table: "JOBS");

            migrationBuilder.DropColumn(
                name: "SKILLS",
                table: "CANDIDATES");

            migrationBuilder.AddColumn<int>(
                name: "COMPANY_ID",
                table: "JOBS",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SKILLS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NAME = table.Column<string>(type: "varchar(255)", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKILLS", x => x.ID);
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

            migrationBuilder.CreateIndex(
                name: "IX_JOBS_COMPANY_ID",
                table: "JOBS",
                column: "COMPANY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_SKILLS_CANDIDATE_ID",
                table: "CANDIDATE_SKILLS",
                column: "CANDIDATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_SKILLS_SOURCE_ID",
                table: "CANDIDATE_SKILLS",
                column: "SOURCE_ID");
        }
    }
}

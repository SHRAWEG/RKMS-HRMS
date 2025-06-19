using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class CandidateStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CANDIDATES_HIRING_STAGES_HIRING_STAGE_ID",
                table: "CANDIDATES");

            migrationBuilder.RenameColumn(
                name: "HIRING_STAGE_ID",
                table: "CANDIDATES",
                newName: "STAGE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_CANDIDATES_HIRING_STAGE_ID",
                table: "CANDIDATES",
                newName: "IX_CANDIDATES_STAGE_ID");

            migrationBuilder.CreateTable(
                name: "CANDIDATE_STAGES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CANDIDATE_ID = table.Column<int>(type: "integer", nullable: false),
                    STAGE_ID = table.Column<int>(type: "integer", nullable: false),
                    CONCERNED_EMP_ID = table.Column<int>(type: "integer", nullable: true),
                    SCHEDULED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANDIDATE_STAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_STAGES_CANDIDATES_CANDIDATE_ID",
                        column: x => x.CANDIDATE_ID,
                        principalTable: "CANDIDATES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CANDIDATE_STAGES_EMP_DETAIL_CONCERNED_EMP_ID",
                        column: x => x.CONCERNED_EMP_ID,
                        principalTable: "EMP_DETAIL",
                        principalColumn: "EMP_ID");
                    table.ForeignKey(
                        name: "FK_CANDIDATE_STAGES_HIRING_STAGES_STAGE_ID",
                        column: x => x.STAGE_ID,
                        principalTable: "HIRING_STAGES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_STAGES_CANDIDATE_ID",
                table: "CANDIDATE_STAGES",
                column: "CANDIDATE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_STAGES_CONCERNED_EMP_ID",
                table: "CANDIDATE_STAGES",
                column: "CONCERNED_EMP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_CANDIDATE_STAGES_STAGE_ID",
                table: "CANDIDATE_STAGES",
                column: "STAGE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_CANDIDATES_HIRING_STAGES_STAGE_ID",
                table: "CANDIDATES",
                column: "STAGE_ID",
                principalTable: "HIRING_STAGES",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CANDIDATES_HIRING_STAGES_STAGE_ID",
                table: "CANDIDATES");

            migrationBuilder.DropTable(
                name: "CANDIDATE_STAGES");

            migrationBuilder.RenameColumn(
                name: "STAGE_ID",
                table: "CANDIDATES",
                newName: "HIRING_STAGE_ID");

            migrationBuilder.RenameIndex(
                name: "IX_CANDIDATES_STAGE_ID",
                table: "CANDIDATES",
                newName: "IX_CANDIDATES_HIRING_STAGE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_CANDIDATES_HIRING_STAGES_HIRING_STAGE_ID",
                table: "CANDIDATES",
                column: "HIRING_STAGE_ID",
                principalTable: "HIRING_STAGES",
                principalColumn: "ID");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class CandidateStageUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_ProcessedById",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.DropIndex(
                name: "IX_MANPOWER_REQUISITIONS_ProcessedById",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.DropColumn(
                name: "ProcessedById",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.AddColumn<int>(
                name: "OVERALL_RATING",
                table: "CANDIDATE_STAGES",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "REMARKS",
                table: "CANDIDATE_STAGES",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_PROCESSED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "PROCESSED_BY_USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_PROCESSED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "PROCESSED_BY_USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_PROCESSED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.DropIndex(
                name: "IX_MANPOWER_REQUISITIONS_PROCESSED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.DropColumn(
                name: "OVERALL_RATING",
                table: "CANDIDATE_STAGES");

            migrationBuilder.DropColumn(
                name: "REMARKS",
                table: "CANDIDATE_STAGES");

            migrationBuilder.AddColumn<int>(
                name: "ProcessedById",
                table: "MANPOWER_REQUISITIONS",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MANPOWER_REQUISITIONS_ProcessedById",
                table: "MANPOWER_REQUISITIONS",
                column: "ProcessedById");

            migrationBuilder.AddForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_ProcessedById",
                table: "MANPOWER_REQUISITIONS",
                column: "ProcessedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

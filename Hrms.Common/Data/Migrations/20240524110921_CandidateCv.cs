using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class CandidateCv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CV_FILE_NAME",
                table: "CANDIDATES",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IMAGE_FILE_NAME",
                table: "CANDIDATES",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CV_FILE_NAME",
                table: "CANDIDATES");

            migrationBuilder.DropColumn(
                name: "IMAGE_FILE_NAME",
                table: "CANDIDATES");
        }
    }
}

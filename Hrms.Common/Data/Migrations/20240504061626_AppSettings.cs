using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class AppSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ATTENDANCE_IN_BS",
                table: "SETTING",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DAILY_ATTENDANCE",
                table: "SETTING",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DAILY_ATTENDNACE_IN_BS",
                table: "SETTING",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ATTENDANCE_IN_BS",
                table: "SETTING");

            migrationBuilder.DropColumn(
                name: "DAILY_ATTENDANCE",
                table: "SETTING");

            migrationBuilder.DropColumn(
                name: "DAILY_ATTENDNACE_IN_BS",
                table: "SETTING");
        }
    }
}

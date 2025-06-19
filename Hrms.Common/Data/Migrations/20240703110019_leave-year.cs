using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class leaveyear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IS_CLOSED",
                table: "LEAVE_LEDGER",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IS_CLOSED",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IS_CLOSED",
                table: "LEAVE_LEDGER");

            migrationBuilder.DropColumn(
                name: "IS_CLOSED",
                table: "LEAVE_APPLICATION_HISTORY");
        }
    }
}

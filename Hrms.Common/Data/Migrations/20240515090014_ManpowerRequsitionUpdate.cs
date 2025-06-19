using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class ManpowerRequsitionUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_REQUESTED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.RenameColumn(
                name: "REQUESTED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS",
                newName: "REQUESTED_BY_Emp_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MANPOWER_REQUISITIONS_REQUESTED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS",
                newName: "IX_MANPOWER_REQUISITIONS_REQUESTED_BY_Emp_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REQUESTED_BY_Emp_ID",
                principalTable: "EMP_DETAIL",
                principalColumn: "EMP_ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.RenameColumn(
                name: "REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                newName: "REQUESTED_BY_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_MANPOWER_REQUISITIONS_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                newName: "IX_MANPOWER_REQUISITIONS_REQUESTED_BY_USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_AspNetUsers_REQUESTED_BY_USER_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REQUESTED_BY_USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

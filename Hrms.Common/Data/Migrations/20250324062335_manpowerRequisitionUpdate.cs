using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class manpowerRequisitionUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.AlterColumn<int>(
                name: "REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REQUESTED_BY_Emp_ID",
                principalTable: "EMP_DETAIL",
                principalColumn: "EMP_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS");

            migrationBuilder.AlterColumn<int>(
                name: "REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MANPOWER_REQUISITIONS_EMP_DETAIL_REQUESTED_BY_Emp_ID",
                table: "MANPOWER_REQUISITIONS",
                column: "REQUESTED_BY_Emp_ID",
                principalTable: "EMP_DETAIL",
                principalColumn: "EMP_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

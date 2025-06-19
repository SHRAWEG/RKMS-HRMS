using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class leaveApplicationHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_ApprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_DisapprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "STATUS");

            migrationBuilder.RenameColumn(
                name: "Cancellation_Remarks",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "CANCELLATION_REMARKS");

            migrationBuilder.RenameColumn(
                name: "DisapprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "DISAPPROVED_BY_USER_ID");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "APPROVED_BY_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_DisapprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "IX_LEAVE_APPLICATION_HISTORY_DISAPPROVED_BY_USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_ApprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "IX_LEAVE_APPLICATION_HISTORY_APPROVED_BY_USER_ID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UPDATED_AT",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CREATED_AT",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "REMARKS",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_APPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "APPROVED_BY_USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_DISAPPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "DISAPPROVED_BY_USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_APPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.DropForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_DISAPPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.DropColumn(
                name: "REMARKS",
                table: "LEAVE_APPLICATION_HISTORY");

            migrationBuilder.RenameColumn(
                name: "STATUS",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CANCELLATION_REMARKS",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "Cancellation_Remarks");

            migrationBuilder.RenameColumn(
                name: "DISAPPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "DisapprovedBy_Id");

            migrationBuilder.RenameColumn(
                name: "APPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "ApprovedBy_Id");

            migrationBuilder.RenameIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_DISAPPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "IX_LEAVE_APPLICATION_HISTORY_DisapprovedBy_Id");

            migrationBuilder.RenameIndex(
                name: "IX_LEAVE_APPLICATION_HISTORY_APPROVED_BY_USER_ID",
                table: "LEAVE_APPLICATION_HISTORY",
                newName: "IX_LEAVE_APPLICATION_HISTORY_ApprovedBy_Id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UPDATED_AT",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CREATED_AT",
                table: "LEAVE_APPLICATION_HISTORY",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_ApprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "ApprovedBy_Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LEAVE_APPLICATION_HISTORY_AspNetUsers_DisapprovedBy_Id",
                table: "LEAVE_APPLICATION_HISTORY",
                column: "DisapprovedBy_Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class addTimeStampInLeaveYear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_PERMISSIONS_AspNetUsers_USER_ID",
                table: "USER_PERMISSIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                table: "USER_PERMISSIONS");

            migrationBuilder.AlterColumn<int>(
                name: "USER_ID",
                table: "USER_PERMISSIONS",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PERMISSION_ID",
                table: "USER_PERMISSIONS",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CREATED_AT",
                table: "LEAVE_YEAR",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UPDATED_AT",
                table: "LEAVE_YEAR",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_USER_PERMISSIONS_AspNetUsers_USER_ID",
                table: "USER_PERMISSIONS",
                column: "USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                table: "USER_PERMISSIONS",
                column: "PERMISSION_ID",
                principalTable: "PERMISSIONS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_PERMISSIONS_AspNetUsers_USER_ID",
                table: "USER_PERMISSIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                table: "USER_PERMISSIONS");

            migrationBuilder.DropColumn(
                name: "CREATED_AT",
                table: "LEAVE_YEAR");

            migrationBuilder.DropColumn(
                name: "UPDATED_AT",
                table: "LEAVE_YEAR");

            migrationBuilder.AlterColumn<int>(
                name: "USER_ID",
                table: "USER_PERMISSIONS",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PERMISSION_ID",
                table: "USER_PERMISSIONS",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_USER_PERMISSIONS_AspNetUsers_USER_ID",
                table: "USER_PERMISSIONS",
                column: "USER_ID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_USER_PERMISSIONS_PERMISSIONS_PERMISSION_ID",
                table: "USER_PERMISSIONS",
                column: "PERMISSION_ID",
                principalTable: "PERMISSIONS",
                principalColumn: "ID");
        }
    }
}

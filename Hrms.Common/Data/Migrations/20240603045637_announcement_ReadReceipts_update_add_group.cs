using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class announcement_ReadReceipts_update_add_group : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_announcementReadReceipts_AspNetUsers_UserId",
                table: "announcementReadReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "announcementReadReceipts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReadOn",
                table: "announcementReadReceipts",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "announcementReadReceipts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UGroupId",
                table: "announcementReadReceipts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_announcementReadReceipts_UGroupId",
                table: "announcementReadReceipts",
                column: "UGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_announcementReadReceipts_AspNetUsers_UserId",
                table: "announcementReadReceipts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_announcementReadReceipts_UGroups_UGroupId",
                table: "announcementReadReceipts",
                column: "UGroupId",
                principalTable: "UGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_announcementReadReceipts_AspNetUsers_UserId",
                table: "announcementReadReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_announcementReadReceipts_UGroups_UGroupId",
                table: "announcementReadReceipts");

            migrationBuilder.DropIndex(
                name: "IX_announcementReadReceipts_UGroupId",
                table: "announcementReadReceipts");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "announcementReadReceipts");

            migrationBuilder.DropColumn(
                name: "UGroupId",
                table: "announcementReadReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "announcementReadReceipts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReadOn",
                table: "announcementReadReceipts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_announcementReadReceipts_AspNetUsers_UserId",
                table: "announcementReadReceipts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

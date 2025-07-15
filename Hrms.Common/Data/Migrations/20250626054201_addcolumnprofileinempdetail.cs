using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class addcolumnprofileinempdetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DATE",
                table: "EARNED_LEAVE",
                newName: "TO_DATE");

            migrationBuilder.AddColumn<int>(
                name: "Profile_DOC_ID",
                table: "EMP_DETAIL",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FROM_DATE",
                table: "EARNED_LEAVE",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "IS_HALF_DAY",
                table: "EARNED_LEAVE",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EMP_DETAIL_Profile_DOC_ID",
                table: "EMP_DETAIL",
                column: "Profile_DOC_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_Profile_DOC_ID",
                table: "EMP_DETAIL",
                column: "Profile_DOC_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_DETAIL_EMP_DOCLIST_Profile_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropIndex(
                name: "IX_EMP_DETAIL_Profile_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropColumn(
                name: "Profile_DOC_ID",
                table: "EMP_DETAIL");

            migrationBuilder.DropColumn(
                name: "FROM_DATE",
                table: "EARNED_LEAVE");

            migrationBuilder.DropColumn(
                name: "IS_HALF_DAY",
                table: "EARNED_LEAVE");

            migrationBuilder.RenameColumn(
                name: "TO_DATE",
                table: "EARNED_LEAVE",
                newName: "DATE");
        }
    }
}

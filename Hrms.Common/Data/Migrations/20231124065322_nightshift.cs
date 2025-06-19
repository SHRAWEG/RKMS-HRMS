using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class nightshift : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IS_NIGHTSHIFT",
                table: "WORK_HOUR",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NIGHT_END_TIME",
                table: "WORK_HOUR",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NIGHT_START_TIME",
                table: "WORK_HOUR",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegularisationId",
                table: "tblFATTLOG",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblFATTLOG_RegularisationId",
                table: "tblFATTLOG",
                column: "RegularisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblFATTLOG_REGULARISATION_RegularisationId",
                table: "tblFATTLOG",
                column: "RegularisationId",
                principalTable: "REGULARISATION",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblFATTLOG_REGULARISATION_RegularisationId",
                table: "tblFATTLOG");

            migrationBuilder.DropIndex(
                name: "IX_tblFATTLOG_RegularisationId",
                table: "tblFATTLOG");

            migrationBuilder.DropColumn(
                name: "NIGHT_END_TIME",
                table: "WORK_HOUR");

            migrationBuilder.DropColumn(
                name: "NIGHT_START_TIME",
                table: "WORK_HOUR");

            migrationBuilder.DropColumn(
                name: "RegularisationId",
                table: "tblFATTLOG");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_NIGHTSHIFT",
                table: "WORK_HOUR",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}

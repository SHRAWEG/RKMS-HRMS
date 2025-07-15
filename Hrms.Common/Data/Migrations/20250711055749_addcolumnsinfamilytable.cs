using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hrms.Common.Data.Migrations
{
    public partial class addcolumnsinfamilytable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CONTACT_NUMBER",
                table: "EMP_FAMILY",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DOCUMENT_ID",
                table: "EMP_FAMILY",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IS_NOMINEE",
                table: "EMP_FAMILY",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PERCENTAGE_OF_SHARE",
                table: "EMP_FAMILY",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMP_FAMILY_DOCUMENT_ID",
                table: "EMP_FAMILY",
                column: "DOCUMENT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMP_FAMILY_EMP_DOCLIST_DOCUMENT_ID",
                table: "EMP_FAMILY",
                column: "DOCUMENT_ID",
                principalTable: "EMP_DOCLIST",
                principalColumn: "DOC_NO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMP_FAMILY_EMP_DOCLIST_DOCUMENT_ID",
                table: "EMP_FAMILY");

            migrationBuilder.DropIndex(
                name: "IX_EMP_FAMILY_DOCUMENT_ID",
                table: "EMP_FAMILY");

            migrationBuilder.DropColumn(
                name: "CONTACT_NUMBER",
                table: "EMP_FAMILY");

            migrationBuilder.DropColumn(
                name: "DOCUMENT_ID",
                table: "EMP_FAMILY");

            migrationBuilder.DropColumn(
                name: "IS_NOMINEE",
                table: "EMP_FAMILY");

            migrationBuilder.DropColumn(
                name: "PERCENTAGE_OF_SHARE",
                table: "EMP_FAMILY");
        }
    }
}
